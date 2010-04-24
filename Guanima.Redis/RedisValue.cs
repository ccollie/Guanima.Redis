using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Guanima.Redis.Extensions;
using log4net.Util.TypeConverters;

namespace Guanima.Redis
{
    public enum RedisValueType : byte
    {
        //TODO: null/pending
        Error = (byte)'-',
        Success = (byte)'+',
        Bulk = (byte)'$',
        MultiBulk = (byte)'*',
        Integer = (byte)':'
    }

    // redis.net
    // by Ryan Petrich
    // inspired by redis-sharp
    // license: New BSD License
    // (c) 2010 Ryan Petrich

    public struct RedisValue : IEnumerable<RedisValue>
    {
        internal const int OneGb = 1073741824;  // TODO: Move to global class

        public static readonly RedisValue Empty = new RedisValue() {IsEmpty = true};

        #region Stream Helpers
        static readonly Encoding Encoding = Encoding.UTF8;

        #endregion

        #region Buffer Conversions
        static string BufferToString(byte[] buffer)
        {
            return Encoding.GetString(buffer);
        }
        static byte[] StringToBuffer(string str)
        {
            return Encoding.GetBytes(str);
        }

        static long BufferToLong(byte[] buffer)
        {
            return long.Parse(BufferToString(buffer));
        }

        static byte[] LongToBuffer(long integer)
        {
            return StringToBuffer(integer.ToString());
        }

        static double BufferToDouble(byte[] buffer)
        {
            return long.Parse(BufferToString(buffer));
        }

        static byte[] DoubleToBuffer(double dbl)
        {
            return StringToBuffer(dbl.ToString());
        }
        #endregion

        #region Properties

        private RedisValueType _type;
        private RedisValue[] _multiBulkValues;
        private byte[] _data;

        void CheckForError()
        {
            if (_type == RedisValueType.Error)
                throw new RedisException("Redis error: " + BufferToString(_data));
        }

        static long DataLength(ICollection<byte> data)
        {
            return (data == null) ? 0 : data.Count;
        }

        static void ValidateDataLength(ICollection<byte> value)
        {
            if (DataLength(value) > OneGb)
                throw new ArgumentException("value exceeds 1G", "value");
        }

        public RedisValueType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public bool IsEmpty { get; internal set; }

        public byte[] Data
        {
            get
            {
                CheckForError();
                return _data;
            }
            set
            {
                ValidateDataLength(value);
                _multiBulkValues = null;
                _data = value;
            }
        }

        public RedisValue[] MultiBulkValues
        {
            get
            {
                CheckForError();
                return _multiBulkValues;
            }
            set
            {
                _data = null;
                _multiBulkValues = value;
            }
        }

        public string Text
        {
            get { return BufferToString(Data); }
            set { Data = StringToBuffer(value); }
        }

        public string ErrorText
        {
            get { return BufferToString(_data); }
            set { Data = StringToBuffer(value); }
        }

        public long Integer
        {
            get { return BufferToLong(Data); }
            set { Data = LongToBuffer(value); }
        }

        public double Double
        {
            get { return BufferToDouble(Data); }
            set { Data = DoubleToBuffer(value); }
        }

        #endregion

        #region Conversions

        public static implicit operator long(RedisValue value)
        {
            return value.Integer;
        }
        public static implicit operator RedisValue(long value)
        {
            return new RedisValue { Type = RedisValueType.Integer, Integer = value };
        }

        public static implicit operator double(RedisValue value)
        {
            return value.Integer;
        }
        public static implicit operator RedisValue(double value)
        {
            return new RedisValue { Type = RedisValueType.Integer, Double = value };
        }

        public static implicit operator string(RedisValue value)
        {
            if (value.IsEmpty)
                return null;

            return value.Text;
        }

        public static implicit operator RedisValue(string value)
        {
            return new RedisValue { Type = RedisValueType.Bulk, Text = value };
        }

        public static implicit operator byte[](RedisValue value)
        {
            return value.Data;
        }

        public static implicit operator RedisValue(Byte[] value)
        {
            if (value == null)
                return Empty;
            return new RedisValue { Type = RedisValueType.Bulk, Data = value };
        }

        public static implicit operator byte[][](RedisValue value)
        {
            if (value.IsEmpty)
                return null;

            // todo : throw if not a multibulk
            var values = new byte[value.MultiBulkValues.Length][];
            int i = 0;
            foreach (var v in value.MultiBulkValues)
            {
                values[i++] = v;
            }
            return values;
        }

        public static implicit operator RedisValue(Byte[][] value)
        {
            if (value == null)
                return Empty;

            var multiBulkValues = new RedisValue[value.Length];
            int i = 0;
            foreach(var val in value)
                multiBulkValues[i++] = val;
            return new RedisValue { Type = RedisValueType.MultiBulk, MultiBulkValues = multiBulkValues };
        }


        public static implicit operator Dictionary<string,RedisValue> (RedisValue value)
        {
            if (value.IsEmpty)
                return null;

            if (value.Type != RedisValueType.MultiBulk)
            {
                throw new ConversionNotSupportedException();
            }
            if (value.MultiBulkValues.Length % 2 != 0)
            {
                throw new RedisException("Cannot convert from a multi-bulk value with an odd number of elements.");
            }
            var i = 0;
            var dict = new Dictionary<string, RedisValue>();
            while (i < value.MultiBulkValues.Length)
            {
                // format is key1, value1, key2, value2
                var k = value.MultiBulkValues[i++];
                var v = value.MultiBulkValues[i++];
                dict.Add(k, v);
            }
            return dict;
        }

        public static implicit operator RedisValue(Dictionary<string,RedisValue> value)
        {
            var multiBulkValues = new RedisValue[value.Count*2];
            int i = 0;
            foreach (var val in value)
            {
                multiBulkValues[i++] = val.Key;
                multiBulkValues[i++] = val.Value;
            }
            return new RedisValue {Type = RedisValueType.MultiBulk, MultiBulkValues = multiBulkValues};
        }

        #region List<string>()

        public static implicit operator List<string>(RedisValue value)
        {
            if (value.IsEmpty)
                return null;

            if (value.Type != RedisValueType.MultiBulk)
            {
                throw new ConversionNotSupportedException();
            }
            var list = new List<string>();
            foreach (var redisValue in value)
            {
                list.Add(redisValue);
            }
            return list;
        }

        public static implicit operator RedisValue(List<string> value)
        {
            if (value == null)
                return Empty; // do we need null value ?

            var multiBulkValues = new RedisValue[value.Count];
            int i = 0;
            foreach (var val in value)
            {
                multiBulkValues[i++] = val;
            }
            return new RedisValue { Type = RedisValueType.MultiBulk, MultiBulkValues = multiBulkValues };
        }

        #endregion

        public static RedisValue Error(string errorText)
        {
            return new RedisValue { Type = RedisValueType.Error, ErrorText = errorText };
        }

        public static RedisValue Success(string errorText)
        {
            return new RedisValue { Type = RedisValueType.Success, ErrorText = errorText };
        }


        #endregion

        #region IEnumerable<RedisValue>
        public IEnumerator<RedisValue> GetEnumerator()
        {
            if (_multiBulkValues != null)
                for (long i = 0, length = _multiBulkValues.Length; i < length; i++)
                    yield return _multiBulkValues[i];
        }
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion


        #region Equals/GetHashCode()

        public static bool operator ==(RedisValue a, RedisValue b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(RedisValue a, RedisValue b)
        {
            return !(a == b);
        }


        public bool Equals(RedisValue other)
        {
            if (other.IsEmpty && IsEmpty)
                return true;

            if (IsEmpty || other.IsEmpty)
                return false;
                
            if (other.Type == Type)
            {
                // ReSharper disable RedundantThisQualifier
                switch (this.Type)
                // ReSharper restore RedundantThisQualifier
                {
                    case RedisValueType.Error:
                    case RedisValueType.Success:
                    case RedisValueType.Integer:
                    case RedisValueType.Bulk:
                        var res = _data.IsEqualTo(other._data);
                        return res;
                    case RedisValueType.MultiBulk:
                        if (_multiBulkValues == null && other._multiBulkValues == null)
                            return true;
                        if (_multiBulkValues == null || other._multiBulkValues == null)
                            return false;
                        if (_multiBulkValues.Length != other._multiBulkValues.Length)
                            return false;

                        for (int i = 0; i < _multiBulkValues.Length; i++)
                        {
                            var first = _multiBulkValues[i];
                            var second = other._multiBulkValues[i];
                            if (!first.Equals(second))
                                return false;
                        }
                        return true;
                }
            }
            return false;
        }

        public override bool Equals(object obj)
        {
            if  (!(obj is RedisValue)) return false;
            return this.Equals((RedisValue) obj);
        }

        public override int GetHashCode()
        {
            int hashCode = 47;
            const int prime = 97;
            
            if (IsEmpty)
                return hashCode;

            hashCode ^= (int)Type;
            switch (Type)
            {
                case RedisValueType.Error:
                case RedisValueType.Success:
                case RedisValueType.Integer:
                case RedisValueType.Bulk:
                    hashCode ^= ((_data == null) ? prime : _data.GetHashCode());
                    break;
                case RedisValueType.MultiBulk:
                    hashCode ^= ((_multiBulkValues == null) ? prime : _multiBulkValues.GetHashCode());
                    break;
            }

            return hashCode;
        }

        #endregion

        #region ToString()

        private string GetAsciiStringOrDefault(string defaultString)
        {
            try
            {
                return Encoding.ASCII.GetString(_data, 0, _data.Length);
            }
            catch
            {
                return defaultString;
            }
        }

        public override string ToString()
        {
            if (IsEmpty)
                return null;

            //sb.Append((char)_type);
            switch (_type)
            {
                case RedisValueType.Error:
                case RedisValueType.Success:
                case RedisValueType.Integer:
                case RedisValueType.Bulk:
                    if (_data == null)
                        return null;
                    return (_data.Length == 0) ? String.Empty : Text;
                case RedisValueType.MultiBulk:
                    var sb = new StringBuilder();
                    int i = 0;
                    //sb.AppendLine(_data.Length.ToString());
                    foreach (var child in _multiBulkValues)
                    {
                        if (i++ > 0)
                            sb.Append(", ");
                        var str = child.ToString();
                        sb.Append(str ?? "null");
                    }
                    return sb.ToString();
                default:
                    throw new InvalidDataException("Unknown value type!");
            }
            //sb.Remove(sb.Length - 2, 2);
        }

        public string ToPrintableString()
        {
            if (IsEmpty)
                return "(null)";

            //sb.Append((char)_type);
            switch (_type)
            {
                case RedisValueType.Error:
                case RedisValueType.Success:
                case RedisValueType.Integer:
                    return (GetAsciiStringOrDefault("(BINARY/UNICODE DATA)"));
                case RedisValueType.Bulk:
                    return (GetAsciiStringOrDefault("(BINARY/UNICODE DATA)"));
                case RedisValueType.MultiBulk:
                    var sb = new StringBuilder();
                    int i = 0;
                    //sb.AppendLine(_data.Length.ToString());
                    foreach (var child in _multiBulkValues)
                    {
                        if (i++ > 0)
                            sb.Append(", ");
                        sb.Append(child.ToString());
                    }
                    return sb.ToString();
                default:
                    throw new InvalidDataException("Unknown value type!");
            }
            //sb.Remove(sb.Length - 2, 2);
        }

        #endregion
    }
}
