using System.IO;
using UnityEngine;

namespace FunGames.Tools.Utils
{
    public class CacheFile<T> where T : class
    {
        private string _fileName;
        private T _data;

        public string Path => Application.persistentDataPath + "/" + _fileName + ".json";

        public CacheFile(string fileName)
        {
            _fileName = fileName;
        }

        public T Read()
        {
            if (!File.Exists(Path))
            {
                Debug.LogWarning(Path + " doesn't exist !");
                return null;
            }

            string content = File.ReadAllText(Path);
            return JsonUtility.FromJson<T>(content);
        }

        public void Create()
        {
            if (File.Exists(Path))
            {
                Debug.LogWarning(Path + " already exist !");
                return;
            }

            var json = new JSONObject();
            json.Add("", new JSONObject());
            File.WriteAllText(Path, json.ToString());
            if (_data != null) Update(_data);
        }

        public void Update(T data)
        {
            File.WriteAllText(Path, JsonUtility.ToJson(data, true));
        }

        public void AddNode(JSONNode node, string key = "")
        {
            if (node == null) return;
            var cachedJson = JSON.Parse(File.ReadAllText(Path));
            cachedJson.Add(key, node);
            File.WriteAllText(Path, cachedJson.ToString());
        }

        public void RemoveNode(string key)
        {
            if (key == null) return;
            var cachedJson = JSON.Parse(File.ReadAllText(Path));
            cachedJson.Remove(key);
            File.WriteAllText(Path, cachedJson.ToString());
        }
    }
}