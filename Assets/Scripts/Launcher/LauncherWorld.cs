using System;
using LSCore;
using UnityEngine;

namespace Launcher
{
    public class LauncherWorld : ServiceManager<LauncherWorld>
    {
        [SerializeReference] public DoIt[] onInit = Array.Empty<DoIt>();
        private static bool inited;
        
        protected override void Awake()
        {
            base.Awake();
            Init();
            if (!inited)
            {
                OnSupersonicWisdomReady();
            }
            else
            {
                onInit.Do();
            }
        }

        private void OnSupersonicWisdomReady()
        {
            inited = true;
            onInit.Do();
        }

        private void Init()
        {
            MainWindow.AsHome();
            MainWindow.Show();
        }
    } 
}