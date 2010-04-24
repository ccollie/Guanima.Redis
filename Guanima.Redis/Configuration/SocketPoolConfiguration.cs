using System;

namespace Guanima.Redis.Configuration
{
	public class SocketPoolConfiguration : ISocketPoolConfiguration
	{
		private int _minPoolSize = 10;
		private int _maxPoolSize = 200;
		private TimeSpan _connectionTimeout = new TimeSpan(0, 0, 10);
		private TimeSpan _receiveTimeout = new TimeSpan(0, 0, 10);
		private TimeSpan _deadTimeout = new TimeSpan(0, 2, 0);

	    int ISocketPoolConfiguration.MinPoolSize
		{
			get { return _minPoolSize; }
			set
			{
				if (value > 1000 || value > _maxPoolSize)
					throw new ArgumentOutOfRangeException("value", "MinPoolSize must be <= MaxPoolSize and must be <= 1000");

				_minPoolSize = value;
			}
		}

		int ISocketPoolConfiguration.MaxPoolSize
		{
			get { return _maxPoolSize; }
			set
			{
				if (value > 1000 || value < _minPoolSize)
					throw new ArgumentOutOfRangeException("value", "MaxPoolSize must be >= MinPoolSize and must be <= 1000");

				_maxPoolSize = value;
			}
		}

		TimeSpan ISocketPoolConfiguration.ConnectionTimeout
		{
			get { return _connectionTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("value", "value must be positive");

				_connectionTimeout = value;
			}
		}

		TimeSpan ISocketPoolConfiguration.ReceiveTimeout
		{
			get { return _receiveTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("value", "value must be positive");

				_receiveTimeout = value;
			}
		}

		TimeSpan ISocketPoolConfiguration.DeadTimeout
		{
			get { return _deadTimeout; }
			set
			{
				if (value < TimeSpan.Zero)
					throw new ArgumentOutOfRangeException("value", "value must be positive");

				_deadTimeout = value;
			}
		}
	}
}
