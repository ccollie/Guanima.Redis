using System;
using System.Security.Cryptography;
using System.Text;

namespace Guanima.Redis.Tests
{
    public class TestHelpers
    {
        // We dont need to be cryptographically secure, so the following
        // suffices
        public static Random Prng = new Random();

        public static double NextDouble(Random rng, double min, double max)
        {
            return min + (rng.NextDouble() * (max - min));
        }

        public static double NextDouble(double min, double max)
        {
            return NextDouble(Prng, min, max);    
        }

        public static double NextDouble()
        {
            return NextDouble(double.MinValue, double.MaxValue);
        }

        public static int RandomInt(int min, int max)
        {
            return (int) NextDouble(min, max);    
        }


        public string GenerateRandomString(int length)
        {
            var crypto = new RNGCryptoServiceProvider();
            byte[] saltInBytes = new byte[length];
            crypto.GetBytes(saltInBytes);
            return Convert.ToBase64String(saltInBytes).Substring(0, length-1);
        }

        public enum RandStringType
        {
            Binary,
            Alpha,
            Compr
        }

        public static string RandString(int min, int max, RandStringType type) 
        {
            var len = RandomInt(min, max);
	        var sb = new StringBuilder();
	        int minval, maxval;
        	
            if (type == RandStringType.Binary) 
            {
                minval = 0;
                maxval = 255;
            }
            else if (type == RandStringType.Alpha)
            {
		        minval = 48;
                maxval = 122;
            } 
            else  
            {
                minval = 48;
                maxval = 52;
            }
            while (len-- > 0)
	        {
		        sb.Append( (char)RandomInt(minval, maxval) );
            }
            return sb.ToString();
        }


   
    }
}