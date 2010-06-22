using System;

namespace Guanima.Redis.Utils
{
    public class ResizableBuffer
    {
        private byte[] _buffer;
        private int _index = 0;
        private int _sizeIncrement = 0;
        public const int DefaultBufferSize = 4 * 1024;

        public ResizableBuffer()
            :this(DefaultBufferSize)
        {
            
        }
        public ResizableBuffer(int initialSize)
            :this(initialSize, 0)
        {
            
        }

        public ResizableBuffer(int initialSize, int sizeIncrement)
        {
            _buffer = new byte[initialSize];
            _sizeIncrement = sizeIncrement;
        }

        public byte[] Data { get { return _buffer; } }

        public int Size
        {
            get { return _index; } 
            protected set { _index = value; }
        }

        public int Capacity
        {
            get { return (_buffer == null) ? 0 : _buffer.Length; }
        }

        public int SizeIncrement
        {
            get { return _sizeIncrement; }
            set { _sizeIncrement = value; }
        }


        public void EnsureCapacity(int delta)
        {
            if ((Size + delta) > Capacity)
            {
                int breathingSpaceToReduceReallocations = (_sizeIncrement == 0) ? (32 * 1024) : _sizeIncrement;
                var newLargerBuffer = new byte[_index + delta + breathingSpaceToReduceReallocations];
                Buffer.BlockCopy(_buffer, 0, newLargerBuffer, 0, _buffer.Length);
                _buffer = newLargerBuffer;
            }
        }

       
        public void Append(byte[] cmdBytes)
        {
            EnsureCapacity(cmdBytes.Length);
            Buffer.BlockCopy(cmdBytes, 0, _buffer, _index, cmdBytes.Length);
            _index += cmdBytes.Length;
        }

        public void AppendCrLf()
        {
            EnsureCapacity(2);
            Data[_index++] = 0x0D; // \r
            Data[_index++] = 0x0A; // \n
        }

        public void Reset()
        {
            _index = 0;
        }
    }
}
