
namespace Guanima.Redis.Utils
{
	/// <summary>
	/// Combines multiple hash codes into one.
	/// </summary>
	public class HashCodeCombiner
	{
		private int _currentHash;

		public HashCodeCombiner() : this(0x1505) { }

		public HashCodeCombiner(int initialValue)
		{
			_currentHash = initialValue;
		}

		public static int Combine(int code1, int code2)
		{
			return ((code1 << 5) + code1) ^ code2;
		}

		public void Add(int value)
		{
			_currentHash = Combine(_currentHash, value);
		}

		public int CurrentHash
		{
			get { return _currentHash; }
		}

		public static int Combine(int code1, int code2, int code3)
		{
			return Combine(Combine(code1, code2), code3);
		}

		public static int Combine(int code1, int code2, int code3, int code4)
		{
			return Combine(Combine(code1, code2), Combine(code3, code4));
		}
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