using UnityEngine;
namespace qb.Network
{
    [CreateAssetMenu(fileName = "SteamApiSettings", menuName = "qb/Network/SteamAPI/SteamApiSettings")]
    public class SteamApiSettings : ApiSettings
    {
        public override bool ManageToken => false;
        public override bool UseEnvBuildEntryPointUrl => false;

        public override bool UseUrlParameters => true;
    }
}
