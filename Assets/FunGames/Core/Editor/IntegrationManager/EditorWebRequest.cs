using System;
using UnityEngine;
using UnityEngine.Networking;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class EditorWebRequest
    {
        
        private static int _requestCounter = 0;
        
        public static void SendRequest(UnityWebRequest[] webRequests, Action<UnityWebRequest> callback)
        {
            // Debug.Log("Send request to url : " + webRequests[_requestCounter].url);
            webRequests[_requestCounter].SendWebRequest().completed +=
                (req) => { RequestCompleted(webRequests, callback); };
        }
        
        public static UnityWebRequest SimpleRequest(string url)
        {
            UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            return webRequest;
        }
        
        public static void DownloadFileRequest(string url, string destinationPath, Action<bool,string> callback)
        {
            UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            webRequest.downloadHandler = new DownloadHandlerFile(destinationPath);
            int time = 0;
            while (!webRequest.isDone)
            {
                time++;
                if (time >= 6E+8) break; //~30secs max by modules
            }

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error downloading package: " + webRequest.error);
                callback?.Invoke(false, destinationPath);
            }
            else
            {
                callback?.Invoke(true, destinationPath);
            }
        }

        private static void RequestCompleted(UnityWebRequest[] webRequests, Action<UnityWebRequest> callback)
        {
            Debug.Log(webRequests[_requestCounter].url);
            if (webRequests[_requestCounter].result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Request " + _requestCounter + " succeeded!");
                callback?.Invoke(webRequests[_requestCounter]);
                _requestCounter = 0;
            }
            else
            {
                if (_requestCounter.Equals(webRequests.Length - 1))
                {
                    Debug.Log("Request " + _requestCounter + " failed! Stop trying.");
                    _requestCounter = 0;
                }
                else
                {
                    Debug.Log("Request " + _requestCounter + " failed! Trying again...");
                    webRequests[_requestCounter].Abort();
                    webRequests[_requestCounter].Dispose();
                    _requestCounter++;
                    SendRequest(webRequests, callback);
                }
            }
        }
    }
}