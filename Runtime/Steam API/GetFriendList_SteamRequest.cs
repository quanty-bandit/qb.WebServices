using System;
using System.Threading.Tasks;
using TriInspector;
using UnityEngine;
namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1", Title = "Test")]
    [CreateAssetMenu(fileName = "GetFriendList_SteamRequest", menuName = "qb/Network/SteamAPI/GetFriendList_SteamRequest")]
    public class GetFriendList_SteamRequest : SteamUserLikeRequest<FriendListContainer>
    {
        public override string ApiEndPoint => "ISteamUser/GetFriendList/v0001/";

        public async Task<EJsonWebResponseState> SendRequest(string steamId)
        {
            this.steamId = steamId;
            return await base.SendRequest(extraUrlParameters: $"playerId={steamId}");
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
    public class FriendListContainer
    {
        [Serializable]
        public class FriendList
        {
            [Serializable]
            public class Friend
            {
                public string steamid;
                public string relationship;
                public long friend_since;
            }
            public Friend[] friends;
        }
        public FriendList friendslist;
    }

}
