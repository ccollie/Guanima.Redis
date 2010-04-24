namespace Guanima.Redis
{
    /// <summary>
    /// Extracts a Key Tag from a regular key. 
    /// From Redis Wiki:
    /// ================
    /// A key tag is a special pattern inside a key that, if present, is the only 
    /// part of the key hashed in order to select the server for this key. 
    /// For example in order to hash the key "foo" I simply perform the hash of the 
    /// whole string, but if this key has a pattern in the form of the characters {...} I only hash this 
    /// substring. So for example for the key "foo{bared}" the key hashing code will simply perform the hash of "bared". 
    /// This way using key tags you can ensure that related keys will be stored on the same Redis instance just using the same 
    /// key tag for all this keys. 
    /// </summary>
    public interface IKeyTagExtractor
    {
        string GetKeyTag(string key);
    }
}