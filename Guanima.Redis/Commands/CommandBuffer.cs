using System.IO;
using Guanima.Redis.Utils;

namespace Guanima.Redis.Commands
{
    public class CommandBuffer : ResizableBuffer
    {

        private void AppendLengthMeta(byte cmdPrefix, int noOfLines)
        {
            var strLines = noOfLines.ToString();
            var strLinesLength = strLines.Length;
            var count = 1 + strLinesLength + 2; // 1 for prefix, 2 for CRLF
            EnsureCapacity(count);
            
            Data[Size++] = cmdPrefix;

            for (var i = 0; i < strLinesLength; i++)
                Data[Size++] = (byte)strLines[i];

            AppendCrLf();
        }

        /// <summary>
        /// Command to set multiple binary safe arguments
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public void Append(RedisCommand command)
        {
            var elems = command.Arguments;

            AppendLengthMeta((byte)'*', elems.Length);
            
            foreach (var arg in elems)
            {
                WriteBulk(arg.Data);
            }
        }

        public void Append(RedisValue[] commandInfo)
        {
            // assumes all are bulk values
            AppendLengthMeta((byte)'*', commandInfo.Length);
            foreach (var arg in commandInfo)
            {
                WriteBulk(arg);
            }            
        }

        public void Append(byte[][] commandInfo)
        {
            AppendLengthMeta((byte)'*', commandInfo.Length);
            foreach (var arg in commandInfo)
            {
                WriteBulk(arg);
            }
        }


        void WriteBulk(byte[] buffer)
        {
            AppendLengthMeta((byte)RedisValueType.Bulk, buffer.Length);
            base.Append(buffer);
            AppendCrLf();

        }

        public void Append(RedisValue value)
        {
            switch (value.Type)
            {
                case RedisValueType.Error:
                case RedisValueType.Inline:
                case RedisValueType.Success:
                case RedisValueType.Integer:
                case RedisValueType.Bulk:
                    WriteBulk(value.Data);
                    break;
                case RedisValueType.MultiBulk:
                    AppendLengthMeta((byte)'*', value.MultiBulkValues.Length);
                    foreach (RedisValue child in value.MultiBulkValues)
                    {
                        Append(child);
                    }
                    break;
                default:
                    throw new InvalidDataException("Unknown value type!");
            }
        }

    }
}
