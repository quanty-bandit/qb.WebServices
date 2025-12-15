using System;
using System.Threading.Tasks;
using qb.Pattern;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TriInspector;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using qb.Datas;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

namespace qb.Network
{
    /// <summary>
    /// Abstract class to manage web request with json as data format
    /// </summary>
    /// </summary>
    /// <typeparam paramName="T"></typeparam>
    public abstract class JsonWebRequest<T> : SOSingleton<JsonWebRequest<T>>
    {
        #region inspector
        [ShowInInspector, GUIColor(0f, 1f, 1f), PropertySpace(SpaceBefore = 10), PropertyOrder(-4)]
        public virtual string ApiEndPoint => "";

        public enum QueryType { Post,Get};
        [ShowInInspector, GUIColor(0f, 1f, 1f), PropertyOrder(-3)]
        protected virtual QueryType RequestType => QueryType.Post;

        [ShowInInspector, GUIColor(0f, 1f, 1f), PropertyOrder(-2), PropertySpace(SpaceAfter = 10)]
        protected virtual string Url => "https://" + (UseApiSettings ? apiSettings?.Url : "") + ApiEndPoint + (UseApiSettings ? apiSettings?.UrlParametterString : "");

        [ShowInInspector, PropertyOrder(0)]
        protected virtual bool UseApiSettings => true;

        [ShowIf(nameof(UseApiSettings))]
        [SerializeField, Required, InlineEditor, PropertyOrder(2)]
        protected ApiSettings apiSettings;

#if UNITY_EDITOR
        bool ShowUseToken => RequestType == QueryType.Post;
        [ShowIf(nameof(ShowUseToken))]
#endif
        [ShowInInspector, PropertyOrder(3)]
        protected virtual bool UseToken => RequestType == QueryType.Post;

        [ShowInInspector, PropertyOrder(4)]
        protected virtual bool UseUrlParameters => true;
        [SerializeField, Required, InlineEditor, PropertyOrder(5), ShowIf(nameof(UseUrlParameters))]
        StringArray_SerializedData urlParametters;

#if UNITY_EDITOR
#pragma warning disable 0414
        void TimeOutUsageChange()
        {
            if (!useTimeout)
                requestTimeoutInSeconds = 0;
        }
        [SerializeField, PropertyOrder(7), OnValueChanged(nameof(TimeOutUsageChange))]
        bool useTimeout = false;
#pragma warning restore 0414
        bool ShowTimeoutTime => useTimeout;
        [ShowIf(nameof(ShowTimeoutTime))]
#endif
        [SerializeField, Min(0), PropertyOrder(8)]
        int requestTimeoutInSeconds = 0;

#if UNITY_EDITOR
#pragma warning disable 0414
        [SerializeField, PropertyOrder(9)]
        bool useLocalResponseInOffLineMode = false;
        protected bool UseLocalResponseInOffLineMode => useLocalResponseInOffLineMode;
        protected bool ShowLocalResponseField => useLocalResponseInOffLineMode;

        [SerializeField, PropertyOrder(10), ShowIf(nameof(ShowLocalResponseField))]
        [GUIColor(0.6f, 0.83f, 0.93f)]
        protected T localResponse = default(T);
        public T LocalResponse => localResponse;

        [GUIColor(0.6f, 0.83f, 0.93f)]
        [Button(ButtonSizes.Medium), PropertyOrder(11), ShowIf(nameof(ShowLocalResponseField))]
        private void LoadLocalResponseFromFile()
        {
            jsonString = LoadJsonFileFromPanel();
            if (string.IsNullOrEmpty(jsonString)) return;

            try
            {
                if (StaticJsonSerializerSettings == null)
                    InitStaticJSonSerializerSetting();
                localResponse = JsonConvert.DeserializeObject<T>(jsonString, StaticJsonSerializerSettings);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        protected static string LoadJsonFileFromPanel()
        {
            string filePath = EditorUtility.OpenFilePanel("Load json file", Application.dataPath, "json,txt");
            if (string.IsNullOrEmpty(filePath)) return null;

            return File.ReadAllText(filePath);
        }

#pragma warning restore 0414

        bool ResonseUpdated => responseState == EJsonWebResponseState.Updated;

        [ShowIf(nameof(ResonseUpdated))]
        [GUIColor(0, 1f, 0.5f), Button(ButtonSizes.Medium)]
        void CopyResponseToClipboard()
        {
            try
            {
                string jsonText = JsonConvert.SerializeObject(response);
                GUIUtility.systemCopyBuffer = jsonText;
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
            }
        }

        [ShowIf(nameof(ResonseUpdated))]
#endif

        [GUIColor(0, 1f, 0.5f), Title("Received data")]
        [NonSerialized, ShowInInspector]
        [PropertyOrder(900)]
        protected T response;
        public T Response => response;


        [SerializeField, Title("Debug log error management"), PropertyOrder(200)]
        protected bool verbose = true;

        #endregion

        [NonSerialized]
        private string jsonString;
        public string JsonString => jsonString;

        protected static JsonSerializerSettings StaticJsonSerializerSettings;

        [System.NonSerialized]
        protected EJsonWebResponseState responseState = EJsonWebResponseState.Uninitialized;
        /// <summary>
        /// The current response state
        /// </summary>
        public EJsonWebResponseState ResponseState
        {
            get
            {
#if UNITY_EDITOR
                if (UseApiSettings && !apiSettings.IsInitialized && UseLocalResponseInOffLineMode)
                {
                    responseState = EJsonWebResponseState.Updated;
                }
#endif
                return responseState;
            }
        }

        protected UnityWebRequest.Result requestResult;
        /// <summary>
        /// The current or last request result, usefull to get error type in case of failure 
        /// </summary>
        public UnityWebRequest.Result RequestResult => requestResult;

        protected string error;
        public string Error => error;

#pragma warning disable 67
        protected event UnityAction<T> onUpdate;
#pragma warning restore 67

        /// <summary>
        /// Callback invoked in case of request success.
        /// This action can be used for extra data initialization process
        /// </summary>
        /// <remarks>
        /// Add process detect dupplicate and avoid same callback subscription.
        /// 
        /// Remove process detect null object method and null method unsubscrition
        /// and also check and clear the all invalid subscriptions from deleted objects
        /// </remarks>
        public event UnityAction<T> OnUpdate
        {
            add
            {
                if (onUpdate != null)
                {
                    var invocationList = onUpdate.GetInvocationList();
                    foreach (var invocation in invocationList)
                    {
                        if (invocation == value as Delegate)
                        {
#if !NO_DEBUG_LOG_WARNING
                            Debug.LogWarning($"Duplicate subscription to OnUpdate of {this.name}");
#endif
                            return;
                        }
                    }
                }
                onUpdate += value;
            }
            remove
            {
                if (value != null && !(value.Target.Equals(null)))
                {
                    if (onUpdate != null)
                        onUpdate -= value;
                    else
                    {
#if !NO_DEBUG_LOG_WARNING
                        Debug.LogWarning($"Try to unsubcribe from null OnUpdate of {this.name}");
#endif
                    }
                }
                else
                {
#if !NO_DEBUG_LOG_WARNING
                    Debug.LogWarning($"Try to unsubcribe null delegate action to OnUpdate of {this.name}");
#endif
                    ClearInvalidSubscriptions();
                }
            }
        }

        [System.NonSerialized]
        object cleanUpLocker = new object();

        /// <summary>
        /// Remove all invalid subscriptions from onUpdate Action in case of behaviours deletion
        /// </summary>
        void ClearInvalidSubscriptions()
        {
            if (onUpdate != null)
            {
                lock (cleanUpLocker)
                {
                    var invocationList = onUpdate.GetInvocationList();
                    int validInvocationCount = 0;
                    foreach (var invocation in invocationList)
                    {
                        if (invocation != null && !(invocation.Target.Equals(null)))
                        {
                            validInvocationCount++;
                        }
                    }
                    if (validInvocationCount != invocationList.Length)
                    {
                        onUpdate = null;
                        foreach (var invocation in invocationList)
                        {
                            if (invocation != null && !(invocation.Target.Equals(null)))
                            {
                                onUpdate += invocation as UnityAction<T>;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Invoke safely onUpdate by checking invalid subscriptor resulting of object destruction 
        /// </summary>
        protected void InvokeOnUptade()
        {

            if (onUpdate != null)
            {
                //reference._event.Invoke();
                var invocationList = onUpdate.GetInvocationList();
                int validInvocationCount = 0;
                foreach (var invocation in invocationList)
                {
                    if (invocation != null && !(invocation.Target.Equals(null)))
                    {
                        validInvocationCount++;
                        (invocation as UnityAction<T>).Invoke(response);
                    }
                }
                if (validInvocationCount != invocationList.Length)
                {
                    onUpdate = null;
                    foreach (var invocation in invocationList)
                    {
                        if (invocation != null && !(invocation.Target.Equals(null)))
                            onUpdate += invocation as UnityAction<T>;
#if !NO_DEBUG_LOG_WARNING
                        else
                        {
                            Debug.LogWarning($"The Event channel [{this.name}] try to invoke an action of a null object!");
                        }
#endif
                    }
                }
            }
        }

        /// <summary>
        /// Send the request to the defined end point
        /// </summary>
        /// <param paramName="parameter">optional json serializable object</param>
        /// <returns>
        /// The final state of the response.
        /// </returns>
        public virtual async Task<EJsonWebResponseState> SendRequest(object parameter = null, string extraUrlParameters = null)
        {

            if (StaticJsonSerializerSettings == null)
                InitStaticJSonSerializerSetting();

            if (responseState != EJsonWebResponseState.Pending)
            {
                try
                {
                    error = null;
                    requestResult = UnityWebRequest.Result.InProgress;
                    response = default(T);
#if UNITY_EDITOR
                    if (UseApiSettings && !apiSettings.IsInitialized && useLocalResponseInOffLineMode)
                    {
                        response = localResponse;
                        Debug.Log($"<color=#FFFF00>LOCAL EDITOR VALUE IS USED AS RESPONSE FOR WEB REQUEST: {name}</color>");
                        OnDataUpdated();
                        responseState = EJsonWebResponseState.Updated;
                        requestResult = UnityWebRequest.Result.Success;
                        return responseState;
                    }
#endif
                    responseState = EJsonWebResponseState.Pending;
                    string token = UseToken && apiSettings.ManageToken ? apiSettings.Token : "";
                    using (
                        var request = 
                        (RequestType== QueryType.Post)
                        ?
                            (parameter == null)
                            ? HttpHelper.GetAJsonPostWebRequest(Url, token)
                            : HttpHelper.GetAJsonPostWebRequest(Url, token, parameter)
                        :
                            (parameter == null)
                            ? UnityWebRequest.Get(Url)
                            : UnityWebRequest.Get(Url)
                        )
                    {
                        //if (UseApiSettings && apiSettings != null && apiSettings.UseUrlParameters)
                        //    request.url = apiSettings.AddUrlParameters(request.url);

#if PLATFORM_WEBGL || UNITY_WEBGL
                        request.SetRequestHeader("Access-Control-Allow-Credentials", "true");
                        request.SetRequestHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
                        request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                        request.SetRequestHeader("Access-Control-Allow-Methods", "*");
                        request.SetRequestHeader("Access-Control-Allow-Headers", "*");
#endif

                        if (UseUrlParameters)
                            request.url = AddUrlParameters(request.url);

                        if (!string.IsNullOrEmpty(extraUrlParameters))
                        {
                            if (!request.url.Contains("?"))
                            {
                                request.url = $"{request.url}?{extraUrlParameters}";
                            }
                            else
                            {
                                request.url = $"{request.url}&{extraUrlParameters}";
                            }
                        }

                        var startTime = Time.time;
                        if (requestTimeoutInSeconds > 0)
                        {
                            int intTimeOut = Mathf.RoundToInt(requestTimeoutInSeconds);
                            request.timeout = intTimeOut;
                        }

                        var operation = request.SendWebRequest();
                        while (!operation.isDone)
                        {
                            await Task.Yield();
                        }
                        requestResult = request.result;
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            jsonString = request.downloadHandler.text;
                            response = JsonConvert.DeserializeObject<T>(jsonString, StaticJsonSerializerSettings);
                            OnDataUpdated();
                            responseState = EJsonWebResponseState.Updated;
                            InvokeOnUptade();
                        }
                        else
                        {
                            if (requestTimeoutInSeconds > 0 && Time.time - startTime > requestTimeoutInSeconds)
                            {
                                responseState = EJsonWebResponseState.TimeOut;
                            }
                            else
                            {
                                responseState = EJsonWebResponseState.Error;
                            }
                            if (request.error.Contains("401"))
                            {
                                responseState = EJsonWebResponseState.Unauthorized;
                            }
                            error = request.error;
                            //if (verbose)
                            {
                                Debug.LogError($"{name}[{request.url}]: {request.result}=>{error}");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    responseState = EJsonWebResponseState.Exception;
                    error = ex.Message;
                    //if (verbose)
                    {
                        Debug.LogError($"{name}[{ApiEndPoint}]: {ex.Message}");
                    }
                }
            }
            else
            {
                while (responseState == EJsonWebResponseState.Pending)
                    await Task.Yield();
            }
            return responseState;

        }

        protected void InitStaticJSonSerializerSetting()
        {
            var defaultJsonContratResolver = new DefaultContractResolver();
            StaticJsonSerializerSettings = new JsonSerializerSettings
            {
                ContractResolver = defaultJsonContratResolver,
                Converters = { new IgnoreUnexpectedArraysConverter(defaultJsonContratResolver) },
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,

            };

            StaticJsonSerializerSettings.Error = (sender, args) =>
            {
                // #if UNITY_EDITOR
                //                 if (System.Diagnostics.Debugger.IsAttached)
                //                 {
                //                     System.Diagnostics.Debugger.Break();
                //                 }
                // #endif
                var error = $"JSON Error: {args?.ErrorContext?.Error?.Message} | Path: {args?.ErrorContext?.Path} | Field: {args?.ErrorContext?.Member} | Class: {args?.CurrentObject?.GetType()?.Name}";
                Debug.LogError(error);
                //Debug.LogError("JSON Error : "+args?.ErrorContext?.Error?.Message + "Field : "+args?.ErrorContext?.Member+ " On class "+args?.CurrentObject?.GetType()?.Name);

            };

        }

        /// <summary>
        /// Virtual method to be overriden to do some extra initialization on 
        /// data updated after a succesfull server request
        /// </summary>
        protected virtual void OnDataUpdated()
        {

        }

        /// <summary>
        /// Waiting for data updated or error during a timeout _duration
        /// </summary>
        /// <param paramName="timeOutDuration">Time out _duration of waiting</param>
        public async Task WaitingForUpdate(float timeOutDuration = 60)
        {
            float startTime = Time.time;
            while (responseState == EJsonWebResponseState.Uninitialized
                || responseState == EJsonWebResponseState.Pending
                || Time.time - startTime >= timeOutDuration)
            {
                await Task.Yield();
            }
            if (Time.time - startTime >= timeOutDuration)
                Debug.LogError($"{name} waiting for update timeout (>= {timeOutDuration})!");
        }

        /// <summary>
        /// Update response from a json string
        /// </summary>
        /// <param paramName="jsonString">The formatted json string</param>
        /// <returns>The response state</returns>
        public EJsonWebResponseState UpdateFromJsonString(string jsonString)
        {
            if (string.IsNullOrEmpty(jsonString))
            {
                return EJsonWebResponseState.Error;
            }
            try
            {
                if (StaticJsonSerializerSettings == null)
                    InitStaticJSonSerializerSetting();
                response = JsonConvert.DeserializeObject<T>(jsonString, StaticJsonSerializerSettings);
                OnDataUpdated();
                responseState = EJsonWebResponseState.Updated;
                InvokeOnUptade();
                this.jsonString = jsonString;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
                return EJsonWebResponseState.Error;
            }
            return EJsonWebResponseState.Updated;
        }

        /// <summary>
        /// Try to deserialize a json string
        /// </summary>
        /// <typeparam paramName="T"></typeparam>
        /// <param paramName="jsonString">The formatted json string</param>
        /// <returns>The unnserialized ofject or the defaul(T) if failed</returns>
        public T TryToUnserializeJsonEntry(string jsonString)
        {
            T result = default(T);
            if (!string.IsNullOrEmpty(jsonString))
            {
                try
                {
                    if (StaticJsonSerializerSettings == null)
                        InitStaticJSonSerializerSetting();
                    result = JsonConvert.DeserializeObject<T>(jsonString, StaticJsonSerializerSettings);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
            }
            return result;
        }
        string AddUrlParameters(string url)
        {
            return (UseUrlParameters) ?URLParameterUtility.AddParameters(url, urlParametters.ToString("&")) : url;
        }

        protected override void Awake()
        {
#if UNITY_EDITOR
            SetEditorIcon();
#endif
            base.Awake();
        }
#if UNITY_EDITOR
        void SetEditorIcon()
        {
            var currentPath = AssetDatabase.GetAssetPath(this);
            if (currentPath != null)
            {
                var obj = AssetDatabase.LoadAssetAtPath<JsonWebRequest<T>>(currentPath);
                if (obj == null)
                    return;
                var iconGuids = AssetDatabase.FindAssets($"{GetType().Name} t:texture2D", null);

                if (iconGuids != null && iconGuids.Length > 0)
                {
                    var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuids[0]));
                    EditorGUIUtility.SetIconForObject(obj, icon);
                }
                else
                {
                    iconGuids = AssetDatabase.FindAssets($"JsonWebRequest t:texture2D", null);
                    if (iconGuids != null && iconGuids.Length > 0)
                    {
                        var icon = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(iconGuids[0]));
                        EditorGUIUtility.SetIconForObject(obj, icon);
                    }
                }
            }
        }
#endif

    }
}
