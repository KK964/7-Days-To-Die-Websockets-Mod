using System.Collections.Specialized;
using System.Linq;

//added class to use with 'Contains' statement elsewhere by Mustached_Maniac

namespace _7DTDWebsockets.Extensions
{
    public static class NameValueCollectionExtensions
    {
        public static bool ContainsKey(this NameValueCollection collection, string key)
        {
            return collection.AllKeys.Contains(key);
        }
    }
}