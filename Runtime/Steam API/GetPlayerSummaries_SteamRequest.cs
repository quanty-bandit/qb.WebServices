using System;
using System.Threading.Tasks;
using TriInspector;
using UnityEngine;
namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1",Title ="Test")]
    [CreateAssetMenu(fileName = "GetPlayerSummaries_SteamRequest", menuName = "qb/Network/SteamAPI/GetPlayerSummaries_SteamRequest")]
    public class GetPlayerSummaries_SteamRequest : SteamUserLikeRequest<PlayerSumaries>
    {

        public override string ApiEndPoint => "ISteamUser/GetPlayerSummaries/v0002/";
#if UNITY_WEBGL
        public async Awaitable<EJsonWebResponseState> SendRequest(string steamIds)
#else
        public async Task<EJsonWebResponseState> SendRequest(string steamIds)
#endif
        {
            this.steamId = steamIds; 
            return await base.SendRequest(cacheKey: steamIds, extraUrlParameters:$"steamIds={steamIds}");
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
    public class PlayerSumaries
    {
        [Serializable]
        public class Response
        {
            [Serializable]
            public class Player
            {
                public string steamid;
                public int communityvisibilitystate;
                public int profilestate;
                public string personaname;
                public int commentpermission;
                public string profileurl;
                public string avatar;
                public string avatarmedium;
                public string avatarfull;
                public string avatarhash;
                public int personastate;
                public string realname;
                public string primaryclanid;
                public int timecreated;
                public int personastateflags;
                public string loccountrycode;
                public string locstatecode;
                public int loccityid;

            }
            public Player[] players;
        }
        public Response response;
    }

}
