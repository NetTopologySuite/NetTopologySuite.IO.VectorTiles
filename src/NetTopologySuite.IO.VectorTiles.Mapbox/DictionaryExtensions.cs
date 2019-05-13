using System.Collections.Generic;

namespace NetTopologySuite.IO.VectorTiles.Mapbox
{
    internal static class DictionaryExtensions
    {
        /// <summary>
        /// Adds a new key with a new id or returns the existing one.
        /// </summary>
        /// <param name="dic">The dictionary.</param>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public static uint AddOrGet(this Dictionary<string, uint> dic, string key)
        {
            if (dic.TryGetValue(key, out var keyId)) return keyId;
            keyId = (uint)dic.Count;
            dic[key] = keyId;
            return keyId;
        }
    }
}