
namespace qb.Cache
{
    public interface IFileBaseCache
    {
        //public void ClearCache();
#if !UNITY_WEBGL && !UNITY_WEBGL_API
        public void LoadCache();
        public void SaveCache();
        public void DeleteCache();
#endif
    }
}
