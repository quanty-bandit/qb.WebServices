using Newtonsoft.Json;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
namespace qb.Network
{
    public static class HttpHelper
    {
        /// <summary>
        /// Generate a web request from the url and token with optionnal data to post 
        /// </summary>
        /// <param paramName="url">The post request url</param>
        /// <param paramName="token">The authentication token</param>
        /// <param paramName="jsonFormattedParameter">Optionnal json string formatted parameter data to post</param>
        /// <returns>The ready to send UnityWebRequest</returns>
        public static UnityWebRequest GetAJsonPostWebRequest(string url, string token, string jsonFormattedParameter=null)
        { 
            var request = UnityWebRequest.PostWwwForm(url, "");
            
            if(!string.IsNullOrEmpty(token))
                request.SetRequestHeader("Authorization", $"Bearer {token}");

            if (!string.IsNullOrEmpty(jsonFormattedParameter))
            {
                byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(jsonFormattedParameter);
                request.uploadHandler = new UploadHandlerRaw(jsonToSend);
            }
            request.downloadHandler = new DownloadHandlerBuffer();

            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept-Encoding", "gzip, identity");

            return request;
        }

        /// <summary>
        /// Generate a web request from the url and token data to post 
        /// </summary>
        /// <typeparam paramName="T">The type of data to post</typeparam>
        /// <param paramName="url">The post url request</param>
        /// <param paramName="token">The authentification token</param>
        /// <param paramName="parameter">
        /// The parameter container object to post.
        /// This object must be an instance from json serializable class 
        /// </param>
        /// <returns>The ready to send UnityWebRequest</returns>
        public static UnityWebRequest GetAJsonPostWebRequest<T>(string url,string token, T parameter)
        {
            return HttpHelper.GetAJsonPostWebRequest(url, token, JsonConvert.SerializeObject(parameter));
        }

        /// <summary>
        /// Test the server url is on line
        /// </summary>
        /// <param paramName="url">The server url end point to test</param>
        public static async Task<bool> IsTheServerUrlOnLine(string url)
        {
            bool result = false; 
            try
            {

                using (var request = UnityWebRequest.Get(url))
                {
                    var operation = request.SendWebRequest();
                    while (!operation.isDone)
                    {
                        await Task.Yield();
                    }
                    result = request.result == UnityWebRequest.Result.Success;
                }
            }
            catch(System.Exception ex)
            {
#if !NO_DEBUG_LOG_EXCEPTION
                Debug.LogException(ex);
#endif
            }
            return result;
        }
    }
}
