using System;
using System.Threading.Tasks;
using TriInspector;
using UnityEngine;
namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1", Title = "Test")]
    [CreateAssetMenu(fileName = "GetOwnedGames_SteamRequest", menuName = "qb/Network/SteamAPI/GetOwnedGames_SteamRequest")]
    public class GetOwnedGames_SteamRequest: SteamUserLikeRequest<OwnedGames>
    {
        public override string ApiEndPoint => "IPlayerService/GetOwnedGames/v0001/";
#if UNITY_WEBGL
        public async Awaitable<EJsonWebResponseState> SendRequest(string steamId)
#else
        public async Task<EJsonWebResponseState> SendRequest(string steamId)
#endif
        {
            this.steamId = steamId;
            return await base.SendRequest(cacheKey:steamId, extraUrlParameters: $"steamid={steamId}&include_appinfo=1");
        }

#if UNITY_EDITOR
        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(501)]
        [Button(ButtonSizes.Medium)]
        async void SendRequest()
        {
            await SendRequest(steamId);
        }
#endif

    }
    [Serializable]
    public class OwnedGames
    {
        [Serializable]
        public class GameDatas
        {
            public long appid;
            public long playtime_forever;
            public string name;
            public string img_icon_url;
            public bool has_community_visible_stats;
            public bool has_leaderboards;
            public int[] content_descriptorids;
        }
        [Serializable]
        public class Response
        {
            public int game_count;
            
            public GameDatas[] games;
        }
        public Response response;
    }
}
