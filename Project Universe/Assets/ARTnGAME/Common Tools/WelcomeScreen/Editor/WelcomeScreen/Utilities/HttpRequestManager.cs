using System.Collections;
using UnityEngine.Networking;

namespace Artngame.CommonTools.WelcomeScreen.Utilities
{
    public delegate void HandleHttpRequestResultFn(UnityWebRequest originalRequest, string responseText);

    public class HttpRequestManager
    {
        public static IEnumerator SendRequest(string url, HandleHttpRequestResultFn onResult = null)
        {
            using (var www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                while (www.isDone == false)
                    yield return null;

                onResult?.Invoke(www, www.downloadHandler.text);
            }
        }
    }
}
