using UnityEngine;
namespace qb.Network.SteamAPI
{
    [CreateAssetMenu(fileName = "SteamStoreApiSettings", menuName = "qb/Network/SteamAPI/SteamStoreApiSettings")]
    public class SteamStoreApiSettings : ApiSettings
    {
        public override bool ManageToken => false;
        public override bool UseEnvBuildEntryPointUrl => false;

        public override bool UseUrlParameters => true;
    }
}
