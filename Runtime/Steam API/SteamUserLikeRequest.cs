using TriInspector;
using UnityEngine;
namespace qb.Network.SteamAPI
{
    [DeclareFoldoutGroup("#1", Title = "Test")]
    public abstract class SteamUserLikeRequest<T>:JsonWebRequest_Cached<T>
    {
        protected override bool UseApiSettings => true;
        protected override bool UseToken => false;
        protected override bool UseUrlParameters => false;
        protected override QueryType RequestType => QueryType.Get;

        [Group("#1"), GUIColor(0f, 1f, 0.5f), PropertyOrder(500)]
        public string steamId;
    }
}
