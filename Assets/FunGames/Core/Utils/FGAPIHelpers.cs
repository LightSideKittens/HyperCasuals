using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Proyecto26;
using RSG;
using UnityEngine;

namespace FunGames.Core.Utils
{
    public static class FGAPIHelpers
    {
        private static readonly char[] Array1 =
        {
            '\u0074', '\u0061', '\u0070', '\u006E', '\u0061', '\u0074', '\u0069', '\u006F', '\u006E', '\u002D',
            '\u0073', '\u0065', '\u0063', '\u0072', '\u0065', '\u0074'
        };

        private static string _secret = new string(Array1);

        internal static string CreateUniqueId(params string[] strings)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var str in strings)
            {
                sb.Append(str);
            }

            return CreateUniqueId(sb);
        }

        internal static string CreateUniqueId(StringBuilder stringBuilder)
        {
            string eventUniqueId = CreateToken(stringBuilder.ToString());
            eventUniqueId = eventUniqueId.Replace("+", String.Empty);
            eventUniqueId = eventUniqueId.Replace("/", String.Empty);
            eventUniqueId = eventUniqueId.Replace("=", String.Empty);
            eventUniqueId = eventUniqueId.Substring(0, 10);
            return eventUniqueId;
        }

        internal static string CreateToken(string message)
        {
            _secret = _secret ?? "";

            var encoding = new ASCIIEncoding();
            var keyByte = encoding.GetBytes(_secret);
            var messageBytes = encoding.GetBytes(message);

            string dest;

            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                var hashmessage = hmacsha256.ComputeHash(messageBytes);
                dest = Convert.ToBase64String(hashmessage);
                dest = dest.Remove(dest.Length - 1);
                return dest;
            }
        }

        internal static string GetBitString()
        {
            if (!String.IsNullOrEmpty(FGMainSettings.settings.ApiKey)) return FGMainSettings.settings.ApiKey;

            var md5Hash = MD5.Create();
            var result = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(_secret));
            var bitString = BitConverter.ToString(result).Replace("-", "").ToLower();

            return bitString;
        }
        
        internal static string CreateAuthorizationHeader(string url, string apikey)
        {
            return $"hmac {apikey} {CreateToken(url)}";
        }

        public static void GET(string url, Action<ResponseHelper, Exception> callback = null)
        {
            RequestHelper rh = new RequestHelper()
            {
                Uri = url,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", "HMAC " + GetBitString() + " " + CreateToken(url) }
                }
            };
            PrintGetRequest(rh);
            HandleResponse(RestClient.Get(rh), callback);
        }

        public static void POST(string url, string body, Action<ResponseHelper, Exception> callback = null)
        {
            RequestHelper rh = new RequestHelper()
            {
                Uri = url,
                Headers = new Dictionary<string, string>
                {
                    { "Content-Type", "application/json" },
                    { "Authorization", "HMAC " + GetBitString() + " " + CreateToken(body) }
                },
                BodyString = body
            };
            PrintPostRequest(rh);
            HandleResponse(RestClient.Post(rh), callback);
        }

        private static void HandleResponse(IPromise<ResponseHelper> request, Action<ResponseHelper, Exception> callback)
        {
            request.Then(response =>
                {
                    Debug.Log("[FG REST] Response: " + response.Text);
                    callback?.Invoke(response, null);
                })
                .Catch(err =>
                {
                    Debug.LogError(
                        "[FG REST] Request error: " + err.Source + " " + err.Message + " " + err.StackTrace);
                    callback?.Invoke(null, err);
                });
        }

        private static void PrintGetRequest(RequestHelper rh)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append("curl " + rh.Uri + " ");
                foreach (var requestHeader in rh.Headers)
                {
                    str.Append("-H \"" + requestHeader.Key + ": " + requestHeader.Value + "\" ");
                }

                Debug.Log("[FG REST] GET Request sent : " + str);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }

        private static void PrintPostRequest(RequestHelper rh)
        {
            try
            {
                StringBuilder str = new StringBuilder();
                str.Append("curl -v ");
                str.Append("\"" + rh.Uri + "\" ");
                str.Append("-d '" + rh.BodyString + "' ");
                foreach (var requestHeader in rh.Headers)
                {
                    str.Append("-H \"" + requestHeader.Key + ": " + requestHeader.Value + "\" ");
                }

                Debug.Log("[FG REST] POST Request sent : " + str);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}