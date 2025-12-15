using qb.Utility;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TriInspector;
using UnityEngine;

namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1", Title = "Test")]
    [CreateAssetMenu(fileName = "GetAppsDetails_SteamRequest", menuName = "qb/Network/SteamAPI/GetAppsDetails_SteamRequest")]
    public class GetAppsDetails_SteamRequest : JsonWebRequest_Cached<Dictionary<string, AppDetails>>
    {
        protected override bool UseApiSettings => true;
        protected override bool UseToken => false;
        protected override bool UseUrlParameters => false;
        protected override QueryType RequestType => QueryType.Get;

        public override string ApiEndPoint => "appdetails/";

        public async Task<EJsonWebResponseState> SendRequest(string appIds)
        {
            this.appIds = appIds;
            (var langCode, var lang) = LocalizationUtility.GetSelectedLangCodeAndEnglishName(true);
            return await base.SendRequest(cacheKey: $"{appIds}-{langCode}", extraUrlParameters: $"appids={appIds}&l={lang}");
        }

        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(501)]
        public string appIds;



#if UNITY_EDITOR
        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(501)]
        [Button(ButtonSizes.Medium)]
        async void SendRequest()
        {
            await SendRequest(appIds);
        }
#endif
    }
    [Serializable]
    public class AppDetails
    {
        public bool success;
        [Serializable]
        public class Data
        {
            public string type;
            public string name;
            public int steam_appid;
            public int required_age;
            public bool is_free;
            public string controller_support;
            public int[] dlc;
            public string short_description;
            public string header_image;
            public string capsule_image;
            public string capsule_imagev5;
            public string website;
        }
        public Data data;
    }

}
