using System;
using System.Collections.Generic;
using System.Text;
using Guanima.Redis.Extensions;

namespace Guanima.Redis.Commands.Generic
{

    public class SortBuilder 
    {
	    private String _key;
        private readonly IList<String> _getPatterns = new List<String>();
        private bool _sortAscending = true;
        private long? _start;
        private long? _count;
        private string _by;
        private bool? _alpha;
        private String _storeKey;

        public SortBuilder(String key)
        {
            if (key == null)
                throw new ArgumentNullException("key", "Null key for sort");
		    _key = key;
	    }
	    
	    public SortBuilder Alpha()
	    {
	        _alpha = true;
            return this;
	    }

        public SortBuilder Alpha(bool isAlpha)
        {
            _alpha = isAlpha;
            return this;
        }

        public SortBuilder Asc()
        {
            _sortAscending = true;
            return this;
        }

        public SortBuilder Desc(bool isDescending)
        {
            _sortAscending = !isDescending;
            return this;
        }

        public SortBuilder Desc()
	    {
            _sortAscending = false;
            return this;
	    }

	    public SortBuilder By(String pattern)
	    {
            if (String.IsNullOrEmpty(pattern))
                throw new ArgumentException("pattern");

	        _by = pattern;
            return this;
	    }

	    public SortBuilder Get(String pattern)
	    {
            if (String.IsNullOrEmpty(pattern))
                throw new ArgumentException("pattern");

	        if (_getPatterns.IndexOf(pattern) == -1)
                _getPatterns.Add(pattern);

            return this;
	    }

        public SortBuilder StoreTo(string resultKey)
        {
            if (String.IsNullOrEmpty(resultKey))
                throw new ArgumentException("resultKey");
            resultKey = resultKey.Trim();
            if (resultKey.Length == 0)
                throw new ArgumentException("resultKey");

            _storeKey = resultKey;
            return this;
        }

	    public SortBuilder Limit(long start, long count) 
        {
            if (!start.IsInRange(0, long.MaxValue))
                throw new ArgumentOutOfRangeException("start");
            if (!count.IsInRange(1, long.MaxValue))
                throw new ArgumentOutOfRangeException("count", count,
                        "at least 1 element must be selected (from=" + start + ")");

 	        _start = start;
	        _count = count;
            return this;
	    }

        public string Key
        {
            get { return _key;}
            internal set { _key = value; }
        }

        public string ResultKey
        {
            get { return _storeKey; }    
        }

        internal RedisValue[] GetSortParameters()
        {
            var values = new List<RedisValue> {"SORT", _key};

            if (!String.IsNullOrEmpty(_by))
            {
                values.Add("BY");
                values.Add(_by);
            }

            if (_start.HasValue && _count.HasValue)
            {
                values.Add("LIMIT");
                values.Add(_start.Value);
                values.Add(_count.Value);
            }

            foreach (var pattern in _getPatterns)
            {
                values.Add("GET");
                values.Add(pattern);
            }

            if (_alpha.HasValue && _alpha.Value)
            {
                values.Add("ALPHA");
            }

             values.Add(((_sortAscending) ? "ASC " : "DESC "));


            if (!String.IsNullOrEmpty(_storeKey))
            {
                values.Add("STORE");
                values.Add(_storeKey); // todo : needs to be hashed
            }

            return values.ToArray();
        }

        private string GetSortSpec() 
        {
            const String Pad = " ";

            var sb = new StringBuilder("SORT " + _key + Pad);
            if (!String.IsNullOrEmpty(_by)) 
                sb.Append("BY " + Pad + _by + Pad);

            if (_start.HasValue && _count.HasValue)
                sb.Append("LIMIT" + Pad + _start.Value + " " + _count.Value + Pad);

            foreach (var pattern in _getPatterns)
            {
                sb.Append("GET " + pattern + Pad);
            }

            if (_alpha.HasValue && _alpha.Value)
                sb.Append("ALPHA" + Pad);

            sb.Append(((_sortAscending) ? "ASC " : "DESC ") + Pad);


            if (!String.IsNullOrEmpty(_storeKey))
                sb.Append("STORE " + _storeKey); // todo : needs to be hashed
 
		    return sb.ToString().TrimEnd();
	    }


        public override string ToString()
        {
            return GetSortSpec();
        }

    }
}
