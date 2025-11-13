using System.Collections;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Launcher
{
    public class UICreationTest : MonoBehaviour
    {
        public GameObject prefab;
        public int count;
        
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(2);
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < count; i++)
            {
                Instantiate(prefab, transform);
            }
            sw.Stop();
            Debug.Log(sw.ElapsedTicks);
        }
    }
}