using System;
using System.Text;

namespace Guanima.Redis.Protocol
{
    // A fully interruptable, binary-safe Redis reply parser.
    // 'callback' is called with each reply parsed in 'feed'.
    // 'thisArg' is the "thisArg" for the callback "call".

    public class RedisReplyParser {
	    private const string CRLF = "\r\n";
        private const int CRLF_LEN = 2;

        private const int PLUS      = 0x2B; // +
        private const int MINUS     = 0x2D; // -
        private const int DOLLAR    = 0x24; // $
        private const int STAR      = 0x2A; // *
        private const int COLON     = 0x3A; // :
        private const int CR        = 0x0D; // \r
        private const int LF        = 0x0A; // \n
                                    
        private int _skip;
	    private int _valueBufferLen;
	    private byte[] _valueBuffer;
	    private int? _bulkLengthExpected;
	    private int _multibulkIndex;
	    private byte[][] _multibulkReplies; 
	    private int? _multibulkRepliesExpected;
	    private readonly object _callbackArg;
        private readonly Action<object, RedisValue> _callback;

	    public RedisReplyParser(Action<object, RedisValue> callback, object arg)
	    {
		    _callback = callback;
		    _callbackArg = arg;
		    ClearState();
		    ClearMultiBulkState();
	    }

	    public RedisValueType Type {get; private set;}
    	
	    public void ClearState() 
	    {
		    Type = RedisValueType.None;
		    _bulkLengthExpected = null;
		    _valueBufferLen = 0;
		    _skip = 0;
		    _valueBuffer = new byte[4096];
	    }	

	    public void ClearMultiBulkState() 
	    {
		    _multibulkReplies = null; 
		    _multibulkRepliesExpected = null;
		    _multibulkIndex = 0;
	    }

	    public void Update(byte[] inbound)
	    {
	        Update(inbound, 0, inbound.Length);   
	    }

	    public void Update(byte[] inbound, int start, int length) 
	    {
		    for (int i=start, count = 0; count < length; i++, count++) 
		    {
			    if (_skip > 0) 
			    {
				    _skip--;
				    continue;
			    }

			    var typeBefore = Type;

			    if (Type == RedisValueType.None) 
			    {
				    switch (inbound[i]) {
					    case DOLLAR: Type = RedisValueType.Bulk;      break;
					    case STAR:   Type = RedisValueType.MultiBulk; break;
					    case COLON:  Type = RedisValueType.Integer;   break;
					    case PLUS:   Type = RedisValueType.Inline;    break;
					    case MINUS:  Type = RedisValueType.Error;     break;
				    }
			    }

			    // Just a state transition on '*', '+', etc.?  

			    if (typeBefore != Type)
				    continue;

			    switch (inbound[i]) {
			    case CR:
				    switch (Type) {
					    case RedisValueType.Inline:
					    case RedisValueType.Error:
						    // CR denotes end of the inline/error value.  
						    // +OK\r\n
						    //    ^

						    var inlineBuf = new byte[_valueBufferLen];
						    Array.Copy(_valueBuffer, inlineBuf, _valueBufferLen);
				            var val = new RedisValue
				                          {
                                            Data = inlineBuf,
                                            Type = Type
				                          };
						    MaybeCallbackWithReply(val);
						    break;

					    case RedisValueType.Integer:
						    // CR denotes the end of the integer value.  
						    // :42\r\n
						    //    ^
						    var n = ParseInt(_valueBuffer, _valueBufferLen);
						    MaybeCallbackWithReply( n );
						    break;

					    case RedisValueType.Bulk:
						    if (_bulkLengthExpected == null) {
							    // CR denotes end of first line of a bulk reply,
							    // which is the length of the bulk reply value.
							    // $5\r\nhello\r\n
							    //   ^

                                var lengthExpected = ParseInt(_valueBuffer, _valueBufferLen);

							    if (lengthExpected <= 0) {
								    MaybeCallbackWithReply( RedisValue.Empty );
							    } else {
								    ClearState();

								    _bulkLengthExpected = lengthExpected;
								    Type = RedisValueType.Bulk;
								    _skip = 1;  // _skip LF
							    }
						    } else if (_valueBufferLen == _bulkLengthExpected) {
							    // CR denotes end of the bulk reply value.
							    // $5\r\nhello\r\n
							    //            ^

							    var bulkBuf = new Byte[_valueBufferLen];
                                Array.Copy(_valueBuffer, bulkBuf, _valueBufferLen);
							    MaybeCallbackWithReply(bulkBuf);
						    } else {
							    // CR is just an embedded CR and has nothing to do
							    // with the reply specification.
							    // $11\r\nhello\rworld\r\n
							    //             ^	
							    _valueBuffer[_valueBufferLen++] = inbound[i];
						    }
						    break;

					    case RedisValueType.MultiBulk:
						    // Parse the count which is the number of expected replies
						    // in the multi-bulk reply.
						    // *2\r\n$5\r\nhello\r\n$5\r\nworld\r\n
						    //   ^

						    var repliesExpected = ParseInt(_valueBuffer, _valueBufferLen);

						    if (repliesExpected <= 0) {
							    MaybeCallbackWithReply( RedisValue.Empty );
						    } else {
							    ClearState();
							    _skip = 1;    // _skip LF
							    _multibulkReplies = new byte[repliesExpected][];
							    _multibulkRepliesExpected = repliesExpected;
							    _multibulkIndex = 0;
						    }
						    break;
				    }
				    break;

			    default:
				    _valueBuffer[_valueBufferLen++] = inbound[i];
				    break;
			    }

			    // If the current value buffer is too big, create a new buffer, copy in
			    // the old buffer, and replace the old buffer with the new buffer.
    	 
			    if (_valueBufferLen == _valueBuffer.Length) 
			    {
				    var newBuffer = new byte[_valueBuffer.Length * 2];
				    _valueBuffer.CopyTo(newBuffer, 0);
				    _valueBuffer = newBuffer;
			    }
		    }
	    }

        private static int ParseInt(byte[] buf, int len)
        {
            var temp = Encoding.ASCII.GetString(buf, 0, len);
            return int.Parse(temp);
        }

        private void MaybeCallbackWithReply(RedisValue reply)
        {
            // If the reply is a part of a multi-bulk reply.  Save it.  If we have
            // received all the expected replies of a multi-bulk reply, then
            // _callback.  If the reply is not part of a multi-bulk. Call back
            // immediately.

		    if (_multibulkReplies != null) 
		    {
			    _multibulkReplies[_multibulkIndex++] = reply;
			    if (--_multibulkRepliesExpected == 0) 
			    {
				    _callback(_callbackArg,  _multibulkReplies);
				    ClearMultiBulkState();
			    }
		    } else {
			    _callback(_callbackArg, reply);
		    }
		    ClearState();
		    _skip = 1; // Skip LF
		}


    }
    
}
