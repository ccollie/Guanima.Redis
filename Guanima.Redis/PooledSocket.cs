using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace Guanima.Redis
{
	[DebuggerDisplay("[ Address: {_endpoint}, IsAlive = {IsAlive} ]")]
	public class PooledSocket : IDisposable
	{
		private static log4net.ILog log = log4net.LogManager.GetLogger(typeof(PooledSocket));

		private bool _isAlive = true;
		private Socket _socket;
		private Action<PooledSocket> _cleanupCallback;
		private readonly IPEndPoint _endpoint;

		private BufferedStream _inputStream;

	    internal PooledSocket(IPEndPoint endpoint, TimeSpan connectionTimeout, TimeSpan receiveTimeout, Action<PooledSocket> cleanupCallback)
		{
			_endpoint = endpoint;
			_cleanupCallback = cleanupCallback;

			_socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, connectionTimeout == TimeSpan.MaxValue ? Timeout.Infinite : (int)connectionTimeout.TotalMilliseconds);
			_socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, receiveTimeout == TimeSpan.MaxValue ? Timeout.Infinite : (int)receiveTimeout.TotalMilliseconds);

			// all operations are "atomic", we do not send small chunks of data
			_socket.NoDelay = true;

			_socket.Connect(endpoint);
			_inputStream = new BufferedStream(new BasicNetworkStream(_socket));
		}

	    public IRedisNode OwnerNode { get; internal set; }

        public int CurrentDb { get; internal set; }

	    public int Available
		{
			get { return _socket.Available; }
		}

		public void Reset()
		{
			//this.LockToThread();

			// discard any buffered data
			_inputStream.Flush();

			int available = _socket.Available;

			if (available > 0)
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("Socket bound to {0} has {1} unread data! This is probably a bug in the code. InstanceID was {2}.", _socket.RemoteEndPoint, available, InstanceId);

				byte[] data = new byte[available];

				Read(data, 0, available);

				if (log.IsWarnEnabled)
					log.Warn(Encoding.ASCII.GetString(data));
			}

			if (log.IsDebugEnabled)
				log.DebugFormat("Socket {0} was reset", InstanceId);
		}

		/// <summary>
		/// The ID of this instance. Used by the <see cref="T:RedisServer"/> to identify the instance in its inner lists.
		/// </summary>
		public readonly Guid InstanceId = Guid.NewGuid();

		public bool IsAlive
		{
			get { return _isAlive; }
		}

        public bool IsAuthorized
        {
            get; internal set;
        }

		/// <summary>
		/// Releases all resources used by this instance and shuts down the inner <see cref="T:Socket"/>. This instance will not be usable anymore.
		/// </summary>
		/// <remarks>Use the IDisposable.Dispose method if you want to release this instance back into the pool.</remarks>
		public void Destroy() // TODO this should be a Dispose() override
		{
			this.Dispose(true);
		}

		protected void Dispose(bool disposing)
		{
			if (disposing)
			{
				GC.SuppressFinalize(this);

				if (_socket != null)
				{
					using (_socket)
						_socket.Shutdown(SocketShutdown.Both);
				}

				_inputStream.Dispose();

				_inputStream = null;
				_socket = null;
				_cleanupCallback = null;
			}
			else
			{
				Action<PooledSocket> cc = _cleanupCallback;

				if (cc != null)
				{
					cc(this);
				}
			}
		}

		void IDisposable.Dispose()
		{
			Dispose(false);
		}

		private void CheckDisposed()
		{
			if (_socket == null)
				throw new ObjectDisposedException("PooledSocket");
		}

		/// <summary>
		/// Reads the next byte from the server's response.
		/// </summary>
		/// <remarks>This method blocks and will not return until the value is read.</remarks>
		public int ReadByte()
		{
		    CheckDisposed();

			try
			{
				return _inputStream.ReadByte();
			}
			catch (IOException)
			{
			    _isAlive = false;
				throw;
			}
		}

		/// <summary>
		/// Reads data from the server into the specified buffer.
		/// </summary>
		/// <param name="buffer">An array of <see cref="T:System.Byte"/> that is the storage location for the received data.</param>
		/// <param name="offset">The location in buffer to store the received data.</param>
		/// <param name="count">The number of bytes to read.</param>
		/// <remarks>This method blocks and will not return until the specified amount of bytes are read.</remarks>
		public void Read(byte[] buffer, int offset, int count)
		{
			CheckDisposed();

			int read = 0;
			int shouldRead = count;

			while (read < count)
			{
				try
				{
					int currentRead = _inputStream.Read(buffer, offset, shouldRead);
					if (currentRead < 1)
						continue;

					read += currentRead;
					offset += currentRead;
					shouldRead -= currentRead;
				}
				catch (IOException)
				{
					_isAlive = false;
					throw;
				}
			}
		}

        public string ReadLine()
        {
            var sb = new StringBuilder();
            int c;

            while ((c = ReadByte()) != -1)
            {
                if (c == '\r')
                    continue;
                if (c == '\n')
                    break;
                sb.Append((char)c);
            }
            return sb.ToString();
        }

		public void Write(ArraySegment<byte> data)
		{
			Write(data.Array, data.Offset, data.Count);
		}

        public void Write(byte[] data)
        {
            Write(data, 0, data.Length);
        }

        /// <summary>
        /// Sends the new line to the network (\r\n)
        /// </summary>
        public void WriteNewLine()
        {
            Write(new byte[] { 0xd, 0xa });
        }


		public void Write(byte[] data, int offset, int length)
		{
			CheckDisposed();

			SocketError status;

			_socket.Send(data, offset, length, SocketFlags.None, out status);

			if (status != SocketError.Success)
			{
				_isAlive = false;

				ThrowHelper.ThrowSocketWriteError(_endpoint, status);
			}
		}

		public void Write(IList<ArraySegment<byte>> buffers)
		{
			CheckDisposed();

			SocketError status;

			_socket.Send(buffers, SocketFlags.None, out status);

			if (status != SocketError.Success)
			{
				_isAlive = false;

				ThrowHelper.ThrowSocketWriteError(_endpoint, status);
			}
		}

		#region [ BasicNetworkStream           ]
		private class BasicNetworkStream : Stream
		{
			private readonly Socket _socket;

			public BasicNetworkStream(Socket socket)
			{
				_socket = socket;
			}

			public override bool CanRead
			{
				get { return true; }
			}

			public override bool CanSeek
			{
				get { return false; }
			}

			public override bool CanWrite
			{
				get { return false; }
			}

			public override void Flush()
			{
			}

			public override long Length
			{
				get { throw new NotSupportedException(); }
			}

			public override long Position
			{
				get { throw new NotSupportedException(); }
				set { throw new NotSupportedException(); }
			}

			public override int Read(byte[] buffer, int offset, int count)
			{
				SocketError errorCode;

				int retval = _socket.Receive(buffer, offset, count, SocketFlags.None, out errorCode);

				if (errorCode == SocketError.Success)
					return retval;

				throw new IOException(String.Format("Failed to read from the _socket '{0}'. Error: {1}", _socket.RemoteEndPoint, errorCode));
			}

			public override long Seek(long offset, SeekOrigin origin)
			{
				throw new NotSupportedException();
			}

			public override void SetLength(long value)
			{
				throw new NotSupportedException();
			}

			public override void Write(byte[] buffer, int offset, int count)
			{
				throw new NotSupportedException();
			}
		}
		#endregion
	}
}

#region [ License information          ]
/* ************************************************************
 *
 * Copyright (c) Attila Kiskó, enyim.com
 *
 * This source code is subject to terms and conditions of 
 * Microsoft Permissive License (Ms-PL).
 * 
 * A copy of the license can be found in the License.html
 * file at the root of this distribution. If you can not 
 * locate the License, please send an email to a@enyim.com
 * 
 * By using this source code in any fashion, you are 
 * agreeing to be bound by the terms of the Microsoft 
 * Permissive License.
 *
 * You must not remove this notice, or any other, from this
 * software.
 *
 * ************************************************************/
#endregion