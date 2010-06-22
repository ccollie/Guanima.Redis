using System;
using System.Collections.Generic;

namespace Guanima.Redis.Utils
{
    /// <summary>
    /// A manager to handle buffers for the socket connections
    /// </summary>
    /// <remarks>
    /// When used in an async call a buffer is pinned. Large numbers of pinned buffers
    /// cause problem with the GC (in particular it causes heap fragmentation).
    ///
    /// This class maintains a set of large segments and gives clients pieces of these
    /// segments that they can use for their buffers. The alternative to this would be to
    /// create many small arrays which it then maintained. This methodology should be slightly
    /// better than the many small array methodology because in creating only a few very
    /// large objects it will force these objects to be placed on the LOH. Since the
    /// objects are on the LOH they are at this time not subject to compacting which would
    /// require an update of all GC roots as would be the case with lots of smaller arrays
    /// that were in the normal heap.
    /// 
    /// http://codebetter.com/blogs/gregyoung/archive/2007/06/18/async-sockets-and-buffer-management.aspx
    /// 
    /// </remarks>
    public class BufferManager
    {
        private readonly int _segmentChunks;
        private readonly int _chunkSize;
        private readonly int _segmentSize;
        private readonly Stack<ArraySegment<byte>> _buffers;
        private readonly object _lockObject = new Object();
        private readonly List<byte[]> _segments;

        /// <summary>
        /// The current number of buffers available
        /// </summary>
        public int AvailableBuffers
        {
            get { return _buffers.Count; } //do we really care about volatility here?
        }

        public int SegmentSize
        {
            get { return _segmentSize; }    
        }

        /// <summary>
        /// The total size of all buffers
        /// </summary>
        public int TotalBufferSize
        {
            get { return _segments.Count * _segmentSize; } //do we really care about volatility here?
        }

        /// <summary>
        /// Creates a new segment, makes buffers available
        /// </summary>
        private void CreateNewSegment()
        {
            var bytes = new byte[_segmentChunks * _chunkSize];
            _segments.Add(bytes);
            for (int i = 0; i < _segmentChunks; i++)
            {
                var chunk = new ArraySegment<byte>(bytes, i * _chunkSize, _chunkSize);
                _buffers.Push(chunk);
            }
        }

        /// <summary>
        /// Checks out a buffer from the manager
        /// </summary>
        /// <remarks>
        /// It is the client's responsibility to return the buffer to the manger by
        /// calling <see cref="Checkin"></see> on the buffer
        /// </remarks>
        /// <returns>A <see cref="ArraySegment"></see> that can be used as a buffer</returns>
        public ArraySegment<byte> CheckOut()
        {
            lock (_lockObject)
            {
                if (_buffers.Count == 0)
                {
                    CreateNewSegment();
                }
                return _buffers.Pop();
            }
        }

        /// <summary>
        /// Returns a buffer to the control of the manager
        /// </summary>
        /// <remarks>
        /// It is the client's responsibility to return the buffer to the manger by
        /// calling <see cref="Checkin"></see> on the buffer
        /// </remarks>
        /// <param name="buffer">The <see cref="ArraySegment"></see> to return to the cache</param>
        public void CheckIn(ArraySegment<byte> buffer)
        {
            lock (_lockObject)
            {
                _buffers.Push(buffer);
            }
        }

        #region constructors

        /// <summary>
        /// Constructs a new <see cref="BufferManager"></see> object
        /// </summary>
        /// <param name="segmentChunks">The number of chunks tocreate per segment</param>
        /// <param name="chunkSize">The size of a chunk in bytes</param>
        public BufferManager(int segmentChunks, int chunkSize) :
            this(segmentChunks, chunkSize, 1) { }

        /// <summary>
        /// Constructs a new <see cref="BufferManager"></see> object
        /// </summary>
        /// <param name="segmentChunks">The number of chunks tocreate per segment</param>
        /// <param name="chunkSize">The size of a chunk in bytes</param>
        /// <param name="initialSegments">The initial number of segments to create</param>
        public BufferManager(int segmentChunks, int chunkSize, int initialSegments)
        {
            _segmentChunks = segmentChunks;
            _chunkSize = chunkSize;
            _segmentSize = _segmentChunks * _chunkSize;
            _buffers = new Stack<ArraySegment<byte>>(segmentChunks * initialSegments);
            _segments = new List<byte[]>();
            for (int i = 0; i < initialSegments; i++)
            {
                CreateNewSegment();
            }
        }

        #endregion
    }

}
