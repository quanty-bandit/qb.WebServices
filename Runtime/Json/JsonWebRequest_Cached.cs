using System;
using TriInspector;
using UnityEngine;
using System.Threading.Tasks;
using UnityEngine.Networking;
using System.Collections.Concurrent;
using Newtonsoft.Json;
using System.Collections.Generic;
using qb.Cache;
#if !UNITY_WEBGL && !UNITY_WEBGL_API
using System.IO;
using qb.Utility;

#endif
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace qb.Network
{
    /// <summary>
    /// Extended Json request class with tmp management 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DeclareBoxGroup("#S",Title ="Cache tweenSettings")]
    public abstract class JsonWebRequest_Cached<T> : JsonWebRequest<T>, IFileBaseCache
    {
        [SerializeField,ReadOnly,Group("#S"),PropertyOrder(-1), GUIColor(0f, 1f, 1f)]
        string guid;
        [SerializeField,Min(0), Group("#S")/*, GUIColor(0.87f, 0.713f, 0.8431f)*/]
        float validCacheDurationInMinutes=2;
        [SerializeField, Min(0), Group("#S")]
        bool compressCacheFile = false;
        [Serializable]
        public class CacheEntry
        {
            public long time;
            public T data;
            public CacheEntry(T data)
            {
                time = DateTime.UtcNow.ToBinary();
                this.data = data;
            }
            public CacheEntry() { }
            [JsonIgnore]
            public TimeSpan timeSpan => DateTime.UtcNow - DateTime.FromBinary(time);
            [JsonIgnore]
            public double minutesLeft => timeSpan.TotalMinutes;
        }
        /// <summary>
        /// Key => build with significative query parameters 
        /// </summary>
        [NonSerialized]
        protected ConcurrentDictionary<string, CacheEntry> cache = new ConcurrentDictionary<string, CacheEntry>();

        [NonSerialized]
        protected bool isDirty;
        public virtual async Task<EJsonWebResponseState> SendRequest(string cacheKey,object parameter = null, string extraUrlParameters = null)
        {
            if (cache.TryGetValue(cacheKey, out CacheEntry entry))
            {
                if (entry.minutesLeft <= validCacheDurationInMinutes)
                {
                    error = "";
                    response = entry.data;
                    OnDataUpdated();
                    requestResult = UnityWebRequest.Result.Success;
                    isDirty = true;
                    return EJsonWebResponseState.Updated;
                }
                else 
                {
                    cache.TryRemove(cacheKey,out CacheEntry removedEntry); 
                }
            }
            await SendRequest(parameter, extraUrlParameters);
            if(requestResult == UnityWebRequest.Result.Success)
            {
                cache.TryAdd(cacheKey, new CacheEntry(response));
            }
            isDirty = true;
            return responseState;
        }


        public void ClearCache()
        {
            cache.Clear();
#if !UNITY_WEBGL && !UNITY_WEBGL_API
            DeleteCache();
#endif
        }

#if !UNITY_WEBGL && !UNITY_WEBGL_API
        
        protected override void OnEnable()
        {
            base.OnEnable();
            JsonWebRequest_Cache.Register(this);
            LoadCache();
        }
        private void OnDestroy()
        {
            SaveCache();
            JsonWebRequest_Cache.Unregister(this);
        }
        protected string CacheRootFolder => $"{Application.persistentDataPath}/Requests";
        protected string CacheFilePath => $"{CacheRootFolder}/{guid}.json";
        protected string CompressedCacheFilePath => $"{CacheRootFolder}/{guid}.zip";
        public void LoadCache()
        {
            var filePath = compressCacheFile ? CompressedCacheFilePath : CacheFilePath;
            if (!Directory.Exists(CacheRootFolder) || !File.Exists(filePath)) return;
            try
            {
                string jsonString;
                if (compressCacheFile)
                {
                    var bytes = File.ReadAllBytes(filePath);
                    jsonString = ZipUtility.Unzip(bytes);
                }
                else
                    jsonString = File.ReadAllText(CacheFilePath);
                
                if (StaticJsonSerializerSettings == null)
                    InitStaticJSonSerializerSetting();
                var tmp = JsonConvert.DeserializeObject<Dictionary<string, CacheEntry>>(jsonString, StaticJsonSerializerSettings);
                this.cache.Clear();
                foreach (var entry in tmp)
                {
                    this.cache.TryAdd(entry.Key, entry.Value);
                }
                tmp.Clear();
                jsonString = "";
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }
        public void SaveCache()
        {
            try
            {
                if (!isDirty) return;
                if (cache.Count > 0)
                {
                    if (!Directory.Exists(CacheRootFolder))
                    {
                        Directory.CreateDirectory(CacheRootFolder);
                    }
                    Dictionary<string, CacheEntry> tmp = new Dictionary<string, CacheEntry>();
                    foreach (var entry in cache)
                    {
                        tmp.Add(entry.Key, entry.Value);
                    }
                    if (StaticJsonSerializerSettings == null)
                        InitStaticJSonSerializerSetting();
                    var jsonString = JsonConvert.SerializeObject(tmp);
                    if (compressCacheFile)
                    {
                        var bytes = ZipUtility.Zip(jsonString);
                        File.WriteAllBytes(CompressedCacheFilePath, bytes); 
                    }
                    else
                        File.WriteAllText(CacheFilePath, jsonString);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
            finally
            {
                isDirty = false;
            }
        }
        public void DeleteCache()
        {
            if (!Directory.Exists(CacheRootFolder) || !File.Exists(CacheFilePath)) return;
            File.Delete(CacheFilePath);
            isDirty = false;
        }
#endif

        protected override void Awake()
        {
            base.Awake();
#if UNITY_EDITOR
            InitializeGuid();
#endif
        }
#if UNITY_EDITOR
        protected void InitializeGuid()
        {
            
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out guid, out var localid))
            {
                Debug.LogError($"SOSIngleton for object [{name}] guid getter failure!");
            }
        }
#endif
    }

#if !UNITY_WEBGL && !UNITY_WEBGL_API
    public static class JsonWebRequest_Cache
    {
        private static List<IFileBaseCache> registeredRequest = new List<IFileBaseCache>();
        static bool saveInProgress;
        public static void Register(IFileBaseCache entry)
        {
            if (registeredRequest.Contains(entry)) return;
            registeredRequest.Add(entry);
        }
        public static void Unregister(IFileBaseCache entry)
        {
            if (registeredRequest.Contains(entry))
                registeredRequest.Remove(entry);
        }
        public static void SaveAllCache()
        {
            if (saveInProgress) return;
            saveInProgress = true;
            List<IFileBaseCache> validEntries = new List<IFileBaseCache>();
            try
            {
                foreach (var entry in registeredRequest)
                {
                    if (entry != null)
                    {
                        validEntries.Add(entry);
                        entry.SaveCache();
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                saveInProgress = false;
            }
            if (validEntries.Count > 0)
                registeredRequest = validEntries;
        }

    }
#endif

}
