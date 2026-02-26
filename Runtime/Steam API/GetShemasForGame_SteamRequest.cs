using qb.Utility;
using System;
using System.Threading.Tasks;
using TriInspector;
using UnityEngine;

namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1", Title = "Test")]
    [CreateAssetMenu(fileName = "GetShemasForGame_SteamRequest", menuName = "qb/Network/SteamAPI/GetShemasForGame_SteamRequest")]
    public class GetShemasForGame_SteamRequest : JsonWebRequest_Cached<ShemasForGame>
    {
       
        protected override bool UseApiSettings => true;
        protected override bool UseToken => false;
        protected override bool UseUrlParameters => false;
        protected override QueryType RequestType => QueryType.Get;

        public override string ApiEndPoint => "ISteamUserStats/GetSchemaForGame/v0002/";
#if UNITY_WEBGL
        public async Awaitable<EJsonWebResponseState> SendRequest(string appId)
#else
        public async Task<EJsonWebResponseState> SendRequest(string appId)
#endif
        {
            this.appId = appId;
            (var langCode, var lang) = LocalizationUtility.GetSelectedLangCodeAndEnglishName(true);
            return await base.SendRequest(cacheKey:$"{appId}-{langCode}", appId, extraUrlParameters: $"appid={appId}&l={lang}");
        }

        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(501)]
        public string appId;



#if UNITY_EDITOR
        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(501)]
        [Button(ButtonSizes.Medium)]
        async void SendRequest()
        {
            await SendRequest(appId);
        }
#endif
    }
    [Serializable]
    public class ShemasForGame
    {
        [Serializable]
        public class Game
        {
            public string gameName;
            public string gameVersion;
            [Serializable]
            public class AvailableGameStats
            {
                [Serializable]
                public class Achievements
                {
                    public string name;
                    public int defaultvalue;
                    public string displayName;
                    public int hidden;
                    public string icon;
                    public string icongray;
                }
                public Achievements[] achievements;
            }
            public AvailableGameStats availableGameStats;
        }

        public Game game;
    }

}
