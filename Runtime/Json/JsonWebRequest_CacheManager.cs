using qb.Pattern;
using UnityEngine;
namespace qb.Network
{
    /// <summary>
    /// Manages caching for JSON web requests and ensures cache persistence across the application's lifetime.
    /// </summary>
    [AddComponentMenu("qb/Cache/JsonWebRequest_CacheManager")]
    public class JsonWebRequest_CacheManager:MBSingleton<JsonWebRequest_CacheManager>
    {
        public override bool IsPersistent => true;
        protected override void OnDestroy()
        {
#if !UNITY_WEBGL && !UNITY_WEBGL_API
            JsonWebRequest_Cache.SaveAllCache();
#endif

            base.OnDestroy();
        }
    }
}
