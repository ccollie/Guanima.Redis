using System;
namespace Guanima.Redis
{
    public static class ServerResponseConstants
    {
        public const string Newline = "\r\n";
        public const string Ok      = "Ok";
        public const string Error   = "ERR";
        public const string Queued  = "QUEUED";
        public const string NULL    = "nil";

        public const char PrefixStatus     = '+';
        public const char PrefixError      = '-';
        public const char PrefixInteger    = ':';
        public const char PrefixBulk       = '$';
        public const char PrefixMultiBulk  = '*';
    }


    public class ServerResponseReader
    {
        public const string Newline = "\r\n";
        public const string Ok = "Ok";
        public const string Error = "ERR";
        public const string Queued = "QUEUED";
        public const string NULL = "nil";

        public const char PrefixStatus = '+';
        public const char PrefixError = '-';
        public const char PrefixInteger = ':';
        public const char PrefixBulk = '$';
        public const char PrefixMultiBulk = '*';
    }


}
