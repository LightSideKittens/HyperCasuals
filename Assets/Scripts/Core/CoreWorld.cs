using System.Collections.Generic;
using System.ComponentModel;
using DG.Tweening;
using LSCore;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using SourceGenerators;
using UnityEngine;
using UnityEngine.Scripting;

[InstanceProxy]
public partial class CoreWorld : ServiceManager<CoreWorld>
{
#if DEBUG
    public class DebugData
    {
        private static DebugData instance = new DebugData();

        public static void Init()
        {
            SRDebug.Instance.AddOptionContainer(instance);
        }

        public static void DeInit()
        {
            SRDebug.Instance?.RemoveOptionContainer(instance);
        }
        
        [Category("Core")]
        [Preserve]
        public void Win()
        {
            WinWindow.Show();
        }

        [Category("Core")]
        [Preserve]
        public void Lose()
        {
            LoseWindow.Show();
        }
    }
#endif

    public static RJObject Config => config ?? JTokenGameConfig.Get("GameCoreData");
    private static RJObject config;
    
    public LaLa.Settings idleMusic;

    public static int Level
    {
        get => Config.As("level", 1);
        set => Config["level"] = value;
    }
    
    public static int Theme
    {
        get => Config.As("theme", 0);
        set => Config["theme"] = value;
    }
    
    private static JHashSet<int> themesSet;
    private static int id;

    private static void InitThemesSet(JArray themes)
    {
        if (World.IsDiff(ref id))
        {
            themesSet = new JHashSet<int>(themes);
        }
    }
    
    public static bool BuyTheme(int theme)
    {
        var themes = Config.AsJ<JArray>("themes");
        InitThemesSet(themes);
        return themesSet.Add(theme);
    }

    public static bool HasTheme(int theme)
    {
        var themes = Config.AsJ<JArray>("themes");
        InitThemesSet(themes);
        return themesSet.Contains(theme) || Theme == theme;
    }

    private void _StopIdleMusic()
    {
        var source = idleMusic.CurrentSource;
        source?.DOFade(0, 0.5f).OnComplete(source.Stop);
    }

    protected override void Awake()
    {
        base.Awake();
        BaseInitializer.Initialize();
#if DEBUG
        DebugData.Init();  
#endif
    }

    private void Start()
    {
        Init();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _StopIdleMusic();
#if DEBUG
        DebugData.DeInit();
#endif
    }

    private void Init()
    {
        CoreWindow.Show();
        idleMusic.Play();
    }
}