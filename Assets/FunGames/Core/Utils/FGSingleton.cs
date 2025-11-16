using UnityEngine;

public class FGSingleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    private static object _lock = new object();

    public static T Instance
    {
        get
        {
            lock (_lock)
            {
                if (_instance != null) return _instance;
                
                Object[] instances = FindObjectsOfType(typeof(T));

                if (instances.Length == 0)
                {
                    Debug.Log("[FGSingleton] An instance of " + typeof(T) + " has been created.");
                    GameObject singleton = new GameObject();
                    singleton.name = typeof(T).ToString();
                    _instance = singleton.AddComponent<T>();
                    return _instance;
                }

                for (int i = 0; i < instances.Length; i++)
                {
                    if (i != 0) Destroy(instances[i]);
                }

                _instance = (T)instances[0];
                if (_instance.transform.parent == null)DontDestroyOnLoad(_instance);
                return _instance;
            }
        }
    }
}