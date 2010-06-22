using System;
using System.Net.Sockets;
using System.Text;
using Guanima.Redis.Commands;
using Guanima.Redis.Extensions;

namespace Guanima.Redis.Protocol
{
    /// <summary>
    /// Low level redis client implementation
    /// </summary>
    public static class RedisProtocol
    {
        #region Constants

        private const string SentinelError = "-";
        private const string SentinelBulkCount = "$";
        private const string SentinelMultiBulkCount = "*";

        #endregion

        #region Fields
        private static readonly Encoding DataEncoding = Encoding.UTF8;
        private static readonly Encoding CommandEncoding = Encoding.ASCII;
        #endregion

        #region Network send/receive helpers


        /// <summary>
        /// Receives a line from the network
        /// </summary>
        /// <returns>The recieved bytes without termination</returns>
        private static byte[] ReceiveLine(this PooledSocket socket)
        {
            var offset = 0;
            var buf = new byte[256];
            int read;
            while (true)
            {
                // Read one char from response
                read = socket.ReadByte();
                // Read one char from response
                //read = socket. stream.Read(buf, offset, 1);

                // Check for protocol errors
                if (read <= 0)
                    throw new RedisClientException("Protocol Error.");
                
                buf[offset] = (byte) read;
                // Check for end of line start \r
                if (buf[offset] == 0xd)
                    break;

                // Increment offset counter
                offset++;

                // Carry on reading if buffer length not exceeded
                if (offset <= buf.Length)
                    continue;

                // resize array in a polynomial fashion
                var newBuf = new byte[buf.Length * 2];
                Array.Copy(buf, newBuf, buf.Length);
                buf = newBuf;
            }

            // Read, check and dispose of termination byte \n
            read = socket.ReadByte();
            if (read <= 0)
                throw new RedisClientException("Protocol Error");
            if (read != 0xa)
                throw new RedisClientException("Protocol Error");

            // Make final copy buffer of right size
            var outBuf = new byte[offset];
            Array.Copy(buf, outBuf, offset);

            return outBuf;
        }

        public static byte[] ReadBulkData(this PooledSocket socket)
        {
            string r = socket.ReadLine();

            // Log("R: {0}", r);
            if (r.Length == 0)
                throw new RedisResponseException("Zero length respose");

            char c = r[0];
            if (c == '-')
                throw new RedisResponseException(r.StartsWith("-ERR") ? r.Substring(5) : r.Substring(1));
            if (c == '$')
            {
                if (r == "$-1")
                    return null;
                int n;

                if (Int32.TryParse(r.Substring(1), out n))
                {
                    var retbuf = new byte[n];
                    socket.Read(retbuf, 0, n);
                    if (socket.ReadByte() != '\r' || socket.ReadByte() != '\n')
                        throw new RedisResponseException("Invalid termination");
                    return retbuf;
                }
                throw new RedisResponseException("Invalid length");
            }
            throw new RedisResponseException("Unexpected bulk reply: " + r);
        }	

        /// <summary>
        /// Expect a the bulk count from the redis server
        /// </summary>
        /// <returns>Bulk item count</returns>
        public static int ExpectBulkCount(this PooledSocket socket)
        {
            // Read line
            var reply = CommandEncoding.GetString(socket.ReceiveLine());

            // Check sentinel
            if (!reply.StartsWith(SentinelBulkCount))
                throw new RedisClientException("Expected bulk count");

            // Convert and return remainder of reply
            return Convert.ToInt32(reply.Substring(1));
        }

        /// <summary>
        /// Expect a multi bulk reply from the server.
        /// </summary>
        /// <returns>Multi bulk item count</returns>
        public static int ExpectMultiBulkCount(this PooledSocket socket)
        {
            // Read line
            var reply = CommandEncoding.GetString(ReceiveLine(socket));

            // Check sentinel
            if (!reply.StartsWith(SentinelMultiBulkCount))
                throw new RedisClientException("Multi-bulk count expected");

            // Convert and return remainder of reply
            return Convert.ToInt32(reply.Substring(1));
        }


        #endregion

        #region Error handling

        /// <summary>
        /// Check for server errors in response
        /// </summary>
        /// <param name="reply">Server reply</param>
        private static void CheckServerError(string reply)
        {
            if (reply == null)
                throw new ArgumentNullException("reply");

            // If not an error, return quickly
            if (!reply.StartsWith(SentinelError))
                return;

            // Extract server message
            var message = reply.Substring(1);

            // Raise error exception
            throw new RedisClientException(
                string.Format("Server Error : {0} ", message));
        }

        #endregion

        #region Expect replies

        /// <summary>
        /// Waits for an integer reply from the server
        /// </summary>
        /// <returns>Integer reply value</returns>
        public static int ExpectIntegerReply(this PooledSocket socket)
        {
            var reply = CommandEncoding.GetString(socket.ReceiveLine());

            CheckServerError(reply);

            if (!reply.StartsWith(":"))
                throw new RedisException("Expected integer reply");
            return Convert.ToInt32(reply.Substring(1));
        }

        /// <summary>
        /// Waits for a single line reply from the server
        /// </summary>
        /// <returns>Single line reply value</returns>
        public static string ExpectSingleLineReply(this PooledSocket socket)
        {
            var reply = CommandEncoding.GetString(socket.ReceiveLine());

            CheckServerError(reply);

            if (!reply.StartsWith("+"))
                throw new RedisException("Single line reply expected");

            return reply.Substring(1);
        }

        /// <summary>
        /// Waits for a bulk reply from the server as binary.
        /// </summary>
        /// <returns></returns>
        public static byte[] ExpectBulkReply(this PooledSocket socket)
        {
            // Get bulk count
            var byteCount = ExpectBulkCount(socket);

            // Check element found
            if (byteCount < 0)
                return null;

            // Read bytes
            var bytes = socket.ReceiveBytes(byteCount);

            // Read trailing line break \r\n
            socket.ReceiveBytes(2);

            // Convert and return
            return bytes;
        }

 
        /// <summary>
        /// Waits for a multi bulk reply from the server in binary.
        /// </summary>
        /// <returns>Multi bulk byte/string array.</returns>
        public static byte[][] ExpectMultiBulkReply(this PooledSocket socket)
        {
            // Get total result count
            var resultCount = socket.ExpectMultiBulkCount();

            // Create byte array of arrays big enough
            var retVal = new byte[resultCount][];

            // Loop and load each bulk reply into the array
            for (int i = 0; i < resultCount; i++)
                retVal[i] = socket.ExpectBulkReply();

            // Done
            return retVal;
        }


        public static void ParseSubscriptionResponse(this PooledSocket socket, 
                ref string action, ref string channel, ref int subscriptionCount)
        {
            // Get total result count
            var resultCount = socket.ExpectMultiBulkCount();
            if (resultCount != 3)
                throw new RedisResponseException("Protocol Error. 3 tokens expected. Got " + resultCount);

            action = socket.ExpectBulkReply().FromUtf8();
            channel = socket.ExpectBulkReply().FromUtf8();

            subscriptionCount = socket.ExpectIntegerReply();            
        }

        static RedisValue ReceivePublishedMessage(this PooledSocket socket, ref string channel)
        {
            byte[][] result = socket.ExpectMultiBulkReply();
            channel = result[1].FromUtf8();
            return result[2];
        }

        #endregion

        #region Asynch

        public static void AsyncRead(this PooledSocket client, 
                                        ClientAsyncReadState.ValueReceivedHandler hander, object arg)
        {
            try
            {
                // Create the state object.
                var state = new ClientAsyncReadState
                                {
                                    CallbackArg = arg,
                                    WorkSocket = client.Socket
                                };

                if (hander != null)
                    state.ValueReceived += hander;

                // Begin receiving the data from the remote device.
                client.Socket.BeginReceive(state.Buffer, 0, ClientAsyncReadState.BufferSize, 0,
                    new AsyncCallback(AsyncReadCallback), state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void AsyncReadCallback(IAsyncResult ar)
        {
            try
            {
                var state = (ClientAsyncReadState)ar.AsyncState;
                Socket client = state.WorkSocket;
   
                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    // There might be more data, so store the data received so far
                    state.ReplyParser.Update(state.Buffer, 0, bytesRead);
                    if (!state.IsComplete)
                    {
                        //  Get the rest of the data.
                        client.BeginReceive(state.Buffer, 0, ClientAsyncReadState.BufferSize, 0,
                                            new AsyncCallback(AsyncReadCallback), state);
                    }
                }
                else
                {
                    // Signal that all bytes have been received.
                    // receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        public static void WriteAsync(this PooledSocket socket, RedisCommand command, Action<RedisCommand> callback)
        {
            var buffer = new CommandBuffer();
            buffer.Append(command);

            var state = new ClientAsyncWriteState
            {
                Buffer = buffer.Data,
                CallbackArg = command,
                WorkSocket = socket.Socket
            };

            if (callback != null)
            {
                state.CommandSent += (args, cmd) => callback(cmd);
            }
            socket.Socket.BeginSend(buffer.Data, 0, buffer.Size, SocketFlags.None,
                new AsyncCallback(EndWriteCmdAsynch), state);
        }

 
        static void EndWriteCmdAsynch(IAsyncResult ar)
        {
            var state = (ClientAsyncWriteState)ar;
            var client = state.WorkSocket;

            client.EndSend(ar);
            //state.OnCommandWritten(state.Command); // ??????    
        }

        #endregion
    }
}