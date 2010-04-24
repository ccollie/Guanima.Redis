using System;
using System.IO;
using System.Text;

namespace Guanima.Redis.Protocol
{
    /// <summary>
    /// Low level redis client implementation
    /// </summary>
    public class RedisProtocol : IRedisProtocol
    {
        #region Constants

        private const string SentinelError = "-";
        private const string SentinelBulkCount = "$";
        private const string SentinelMultiBulkCount = "*";

        #endregion

        #region Fields

        private PooledSocket _socket;

        private static readonly Encoding DataEncoding = Encoding.UTF8;
        private static readonly Encoding CommandEncoding = Encoding.ASCII;


        #endregion

        #region Constructors

        public RedisProtocol()
        {
            
        }

        public RedisProtocol(PooledSocket pooledSocket)
        {
            _socket = pooledSocket;
        }

        
        #endregion

        #region Properties
        
        public PooledSocket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        #endregion

        #region Network send/recieve helpers

        protected PooledSocket GetSocket()
        {
            var socket = Socket;
            if (socket == null)
                throw new RedisException("Socket not set on protocol handler");
            return socket;
        }

        /// <summary>
        /// Receives a set number of bytes from the network
        /// </summary>
        /// <param name="byteCount">The byte count.</param>
        /// <returns>The received bytes</returns>
        /// <remarks>
        /// Adapted from:
        /// http://www.yoda.arachsys.com/csharp/readbinary.html
        /// </remarks>
        private byte[] ReceiveBytes(int byteCount)
        {
            // Check.IsInRange(byteCount, 0, Int32.MaxValue, "byteCount");
            var buf = new byte[byteCount];
            _socket.Read(buf, 0, byteCount);
            return buf;
        }

        /// <summary>
        /// Receives a line from the network
        /// </summary>
        /// <returns>The recieved bytes without termination</returns>
        private byte[] ReceiveLine()
        {
            var offset = 0;
            var buf = new byte[256];
            int read;
            var socket = GetSocket();
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
            var terminator = new byte[1];
            read = _socket.ReadByte();
            if (read <= 0)
                throw new RedisClientException("Protocol Error");
            if (read != 0xa)
                throw new RedisClientException("Protocol Error");

            // Make final copy buffer of right size
            var outBuf = new byte[offset];
            Array.Copy(buf, outBuf, offset);

            return outBuf;
        }

        public byte[] ReadBulkData()
        {
            var socket = GetSocket();

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
        public int ExpectBulkCount()
        {
            // Read line
            var reply = CommandEncoding.GetString(ReceiveLine());

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
        public int ExpectMultiBulkCount()
        {
            // Read line
            var reply = CommandEncoding.GetString(ReceiveLine());

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

        #region IRedisNetworkClient Members

        #region Issue commands

        public void IssueCommand(string command, params RedisValue[] parameters)
        {
            var vals = new RedisValue[1 + parameters.Length];
            vals[0] = command;
            if (parameters.Length > 0)
                parameters.CopyTo(vals, 1);
            var toSend = new RedisValue() {Type = RedisValueType.MultiBulk, MultiBulkValues = vals};
            WriteValue(toSend);
        }


        private void WriteBulk(byte[] buffer)
        {
            WriteByte((byte)RedisValueType.Bulk);
            WriteFollowedByNewline(LongToBuffer(buffer.Length));
            WriteFollowedByNewline(buffer);                
        }

        private void WriteFollowedByNewline(byte[] buffer)
        {
            _socket.Write(buffer);
            _socket.WriteNewLine();
        }

        private void WriteByte(byte b)
        {
            var buf = new byte[]{b};
            _socket.Write(buf);
        }

        public void WriteValue(RedisValue value)
        {
            switch (value.Type)
            {
                case RedisValueType.Error:
                case RedisValueType.Success:
                case RedisValueType.Integer:
                case RedisValueType.Bulk:
                    WriteBulk(value.Data);
                    break;
                case RedisValueType.MultiBulk:
                    WriteByte((byte)value.Type); 
                    WriteFollowedByNewline(LongToBuffer(value.MultiBulkValues.Length));
                    foreach (RedisValue child in value.MultiBulkValues)
                    {
                        WriteValue(child);
                    }
                    break;
                default:
                    throw new InvalidDataException("Unknown value type!");
            }
        }

   
        static byte[] StringToBuffer(string str)
        {
            return DataEncoding.GetBytes(str);
        }

        static byte[] LongToBuffer(long value)
        {
            return StringToBuffer(value.ToString());
        }

        #endregion

        #region Expect replies

        /// <summary>
        /// Waits for an integer reply from the server
        /// </summary>
        /// <returns>Integer reply value</returns>
        public int ExpectIntegerReply()
        {
            var reply = CommandEncoding.GetString(ReceiveLine());

            CheckServerError(reply);

            if (!reply.StartsWith(":"))
                throw new RedisException("Expected integer reply");
            return Convert.ToInt32(reply.Substring(1));
        }

        /// <summary>
        /// Waits for a single line reply from the server
        /// </summary>
        /// <returns>Single line reply value</returns>
        public string ExpectSingleLineReply()
        {
            var reply = CommandEncoding.GetString(ReceiveLine());

            CheckServerError(reply);

            if (!reply.StartsWith("+"))
                throw new RedisException("Single line reply expected");

            return reply.Substring(1);
        }

        /// <summary>
        /// Waits for a bulk reply from the server as binary.
        /// </summary>
        /// <returns></returns>
        public byte[] ExpectBulkReply()
        {
            // Get bulk count
            var byteCount = ExpectBulkCount();

            // Check element found
            if (byteCount < 0)
                return null;

            // Read bytes
            var bytes = ReceiveBytes(byteCount);

            // Read trailing line break \r\n
            ReceiveBytes(2);

            // Convert and return
            return bytes;
        }

 
        /// <summary>
        /// Waits for a multi bulk reply from the server in binary.
        /// </summary>
        /// <returns>Multi bulk byte/string array.</returns>
        public byte[][] ExpectMultiBulkReply()
        {
            // Get total result count
            var resultCount = ExpectMultiBulkCount();

            // Create byte array of arrays big enough
            var retVal = new byte[resultCount][];

            // Loop and load each bulk reply into the array
            for (int i = 0; i < resultCount; i++)
                retVal[i] = ExpectBulkReply();

            // Done
            return retVal;
        }

        public RedisValue ReadReply()
        {
            var socket = GetSocket();
            int c = socket.ReadByte();
            if (c == -1)
                throw new RedisResponseException("No more data");

            var s = socket.ReadLine();

            //Log("R: " + s);
            //CheckStatus(c, s);
            switch (c)
            {
                case (int)RedisValueType.MultiBulk:
                    return ParseMultiBulk(s);

                case (int)RedisValueType.Bulk:
                    return ParseBulk(s);

                case (int)RedisValueType.Success:
                    return ParseStatus(s);

                case (int)RedisValueType.Error:
                    return ParseError(s);

                case (int)RedisValueType.Integer:
                    return ParseInteger(s);
            }
            throw new RedisResponseException("Unrecognized response type prefix : " + (char)c);
        }

        #region Parser Helpers

        private static RedisValue ParseStatus(string s)
        {
            if (!String.IsNullOrEmpty(s))
                return RedisValue.Success(s);

            throw new RedisResponseException("Null data reply on status request");
        }

        private static RedisValue ParseError(string s)
        {
            if (!String.IsNullOrEmpty(s))
            {
                s = s.StartsWith("ERR") ? s.Substring(4) : s;
                return RedisValue.Error(s);
            }

            throw new RedisResponseException("No message on error response");
        }

        private static RedisValue ParseInteger(string s)
        {
            long i;
            if (long.TryParse(s, out i))
            {
                return (RedisValue) i;
            }
            throw new RedisResponseException("Malformed reply on integer response: " + s);
        }

        private RedisValue ParseBulk(string line)
        {
            if (line == "-1")
                return RedisValue.Empty;

            int n;

            if (Int32.TryParse(line, out n))
            {
                var retbuf = new byte[n];
                var socket = GetSocket();
                socket.Read(retbuf, 0, n);
                if (socket.ReadByte() != '\r' || socket.ReadByte() != '\n')
                    throw new RedisResponseException("Invalid termination");
                return retbuf;
            }
            throw new RedisResponseException("Invalid length : " + line);
        }

        private RedisValue ParseMultiBulk(string line)
        {
            int count;
            if (int.TryParse(line, out count))
            {
                byte[][] result;
                if (count == -1)
                    result = RedisValue.Empty;
                else if (count == 0)
                    result = new byte[0][] { };
                else
                {
                    result = new byte[count][];

                    for (var i = 0; i < count; i++)
                    {
                        result[i] = ReadBulkData();
                    }
                }
                return result;
            }
            throw new RedisClientException("Expected item count in Multibulk response");
        }

        #endregion

        #endregion

        #endregion

    }
}