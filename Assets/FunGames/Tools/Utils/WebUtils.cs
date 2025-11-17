using System;
using System.IO;
using FunGames.Core.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace FunGames.Tools.Utils
{
    public class WebUtils
    {
        public static void SendRequest(UnityWebRequest webRequest, Action<UnityWebRequest> callback)
        {
            webRequest.SendWebRequest().completed += (req) => { callback?.Invoke(webRequest); };
        }

        public static UnityWebRequest DownloadRequest(string url)
        {
            UnityWebRequest webRequest = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            return webRequest;
        }

        public static void DownloadFile(string url, string path, Action action = null)
        {
            UnityWebRequest downloadRequest = DownloadRequest(url);
            SendRequest(downloadRequest, (s) => FileDownloaded(s, path, action));
        }

        public static void DownloadFileWAuthorization(string url, string path, Action<bool, long> action = null)
        {
            UnityWebRequest downloadRequest = DownloadRequest(url);
            downloadRequest.SetRequestHeader(
                "authorization", FGAPIHelpers.CreateAuthorizationHeader(url));
            SendRequest(downloadRequest, (request) =>
            {
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"File download error: {request.error}");
                    action?.Invoke(false, request.responseCode);
                    return;
                }
                
                if (request.downloadHandler.data == null)
                {
                    Debug.LogError($"Response code {request.responseCode}. The response data is empty.");
                    action?.Invoke(false, request.responseCode);
                    return;
                }
                
                FileDownloaded(request, path, null);
                action?.Invoke(true, request.responseCode);
            });
        }

        private static void FileDownloaded(UnityWebRequest webRequest, string path, Action action = null)
        {
            Debug.Log("File downloaded: " + path);
            if (File.Exists(path)) File.Delete(path);
            File.WriteAllBytes(path, webRequest.downloadHandler.data);
            action?.Invoke();
        }
    }
}