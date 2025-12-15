using qb.Utility;
using System;
using System.Threading.Tasks;
using TriInspector;
using UnityEngine;

namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1", Title = "Test")]
    [CreateAssetMenu(fileName = "GetPlayerAchievemments_SteamRequest", menuName = "qb/Network/SteamAPI/GetPlayerAchievemments_SteamRequest")]
    public class GetPlayerAchievemments_SteamRequest : SteamUserLikeRequest<PlayerAchievements>
    {
        public override string ApiEndPoint => "ISteamUserStats/GetPlayerAchievements/v0001/";

        public async Task<EJsonWebResponseState> SendRequest(string steamId,string appId)
        {
            this.steamId = steamId;
            this.appId = appId;

            (var langCode, var lang) = LocalizationUtility.GetSelectedLangCodeAndEnglishName(true);
            return await base.SendRequest(cacheKey: $"{steamId}-{appId}-{langCode}", extraUrlParameters: $"playerId={steamId}&appid={appId}&l={lang}");
        }

        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(501)]
        public string appId;

#if UNITY_EDITOR
        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(502)]
        [Button(ButtonSizes.Medium)]
        async void SendRequest()
        {
            await SendRequest(steamId,appId);
        }
#endif
    }

    [Serializable]
    public class PlayerAchievements
    {
        [Serializable]
        public class PlayerStats
        {
            public string steamID;
            public string gameName;
            [Serializable]
            public class Achievements
            {
                public string apiname;
                public int achieved;
                public long unlocktime;
            }

            public Achievements[] achievements;

            public bool success;
            public string error;
        }
        public PlayerStats playerstats;
    }

}
