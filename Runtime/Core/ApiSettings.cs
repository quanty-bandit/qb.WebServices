using System;
using TriInspector;
using UnityEngine;
using qb.EnvironmentBuild;
using qb.Pattern;
using qb.Datas;
namespace qb.Network
{
    [Serializable]
    public abstract class ApiSettings : ScriptableObject
    {
        [SerializeField]
        int apiVersion = 1;
        public int ApiVersion
        {
            get => apiVersion;
            // Uncomment the following code to allow apiVersion to be setted by a prebuild script.
            /* 
#if UNITY_EDITOR
            set
            {
                if(Application.isPlaying) throw new NotSupportedException();
                Reference.apiVersion = value;
            }
#endif
            */
        }
        public virtual bool UseEnvBuildEntryPointUrl => true;
        /*
        [SerializeField, Required, HideIf(nameof(UseEnvBuildEntryPointUrl))]
        BuildInfo buildInfo;
        */
        [SerializeField, Required, InlineEditor, ShowIf(nameof(UseEnvBuildEntryPointUrl))]
        String_EBD envBuildEntryPointUrl;

        [SerializeField, Required, HideIf(nameof(UseEnvBuildEntryPointUrl))]
        string entryPointUrl = "";

        [ShowInInspector, PropertyOrder(4)]
        public virtual bool UseUrlParameters => true;
        [SerializeField, Required, InlineEditor, PropertyOrder(5), ShowIf(nameof(UseUrlParameters))]
        StringArray_SerializedData urlParametters;

        public virtual bool ManageToken => true;

#if UNITY_EDITOR

        [GUIColor(0.6f, 0.83f, 0.93f)]
        [ShowInInspector, ShowIf(nameof(ManageToken))]
        string _Token => _token;
        bool ShowCopyTokenButton => !string.IsNullOrEmpty(_token) && ManageToken;
        [Button, ShowIf(nameof(ShowCopyTokenButton))]
        [GUIColor(0.6f, 0.83f, 0.93f)]
        void CopyTokenToClipboard()
        {
            GUIUtility.systemCopyBuffer = _token;
        }
#endif

        [NonSerialized]
        private string _token;
        /// <summary>
        /// The current playerSprite token
        /// </summary>
        public string Token => _token;
        public bool IsTokenEmpty
        {
            get
            {
                return string.IsNullOrEmpty(_token);
            }
        }

        [NonSerialized]
        private string _refreshToken;
        /// <summary>
        /// The current refresh token used to refresh the playerSprite token
        /// </summary>
        public string RefreshToken => _refreshToken;
        public bool IsRefreshTokenEmpty
        {
            get
            {
                return string.IsNullOrEmpty(_refreshToken);
            }
        }

        [NonSerialized]
        private bool isOnMaintenance;
        /// <summary>
        /// backend maintenance state
        /// </summary>
        public bool IsOnMaintenance => isOnMaintenance;
        /// <summary>
        /// Switch to maintenance mode
        /// </summary>
        public void SwitchToMaintenance() => isOnMaintenance = true;

        /// <summary>
        /// The backend url from envBuildEntryPointUrl and BackendUrlIndex
        /// </summary>
        public string Url => (UseEnvBuildEntryPointUrl) ? envBuildEntryPointUrl.Value : entryPointUrl;

        //public BuildInfo.EnvironmentType Environment => envBuildEntryPointUrl != null ? envBuildEntryPointUrl.Environment : buildInfo.Environment;


        /// <summary>
        /// The friend key to manage friend method restrictions
        /// </summary>
        [NonSerialized]
        private int _friendKey;

        /// <summary>
        /// Initialize the friend key will be necessary 
        /// for friend class to access to friend methods
        /// </summary>
        /// <param paramName="friendKey"></param>
        /// <exception cref="Exception"></exception>
        public virtual void Initialize(int friendKey = 1)
        {
            if (_friendKey <= 0)
                _friendKey = friendKey;
            else
            {
                throw new Exception($"{this.GetType()} is already initialized!");
            }
        }

        public bool IsInitialized => _friendKey != 0;

#if UNITY_EDITOR
        public void ForceInitializationToTrue() => _friendKey = -1;
        public bool NotRealyInitialized => _friendKey == -1;
#endif
        /// <summary>
        /// Set the token value
        /// </summary>
        /// <param paramName="friendKey">
        /// The friend key to allow the operation.
        /// The friend key must be the same as the one used to initialize the object.
        /// In case of object was not previously initialized or of wrong friend key the method throw an exception!
        /// </param>
        /// <param paramName="token">The new token value</param>
        public void SetToken(int friendKey, string token, string refreshToken)
        {
            if (!ManageToken)
                throw new Exception("This api tweenSettings doesn't manage token!");

            CheckFriendKey(friendKey);
            _token = token;
            _refreshToken = refreshToken;
        }

        void CheckFriendKey(int friendKey)
        {
            if (_friendKey == 0)
                throw new Exception($"{this.GetType()} must be initialized first!");

            if (_friendKey != friendKey)
                throw new Exception("Wrong friend key!");
        }

        public string UrlParametterString => (UseUrlParameters) ?  "?"+urlParametters?.ToString("&") : "";
    }
}