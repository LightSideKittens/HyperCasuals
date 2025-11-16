using System;
using System.Collections.Generic;
using System.Text;
using FunGames.Core.Editor.Config;
using FunGames.Tools.Utils;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

namespace FunGames.Core.Editor.IntegrationManager
{
    public class IntegrationManagerWindow : EditorWindow
    {
        private const int IntegrationTabIndex = 0;
        private const int IssuesTabIndex = 1;
        
        public GUISkin MainSkin;
        private Vector2 scrollPos = Vector2.zero;
        private float previousWindowWidth;
        private float w1;
        private float w4;
        private const float SPACING = 10;
        private const float shortHeight = 200;
        private const float tallHeight = 600;

        private static bool isInit = false;

        private static IntegrationManagerController _imc;
        private List<FGModuleInstallDrawer> _moduleInstallDrawers = new List<FGModuleInstallDrawer>();
        private RemoteSettingsDrawer _remoteSettingsDrawer;
        private RemoteSettingsController _rmc;
        private IntegrationIssuesDrawer _integrationIssuesDrawer;
        private PluginsUpdatePromptDrawer _pluginsUpdatePromptDrawer;
        private int _selectedTab;
        private IntegrationTheme _theme = new();
        private IntegrationView _integrationViewToDraw;
        private float windowHeightPrePrompt;
        private float PromptHeight => tallHeight;
        private float MaxHeight => _rmc.IsVirginProject && _integrationViewToDraw == IntegrationView.SettingsAndModules 
            ? shortHeight : tallHeight;

        [MenuItem("FunGames/Integration Manager")]
        public static void Init()
        {
            GetWindow<IntegrationManagerWindow>();
        }

        public static IntegrationManagerWindow OpenWindow()
        {
            var window = GetWindow<IntegrationManagerWindow>();
            window.Focus();
            return window;
        }

        void OnEnable()
        {
            titleContent.text = "FunGames SDK ";
            position = new Rect(200, 200, 540, shortHeight); //w820
            minSize = new Vector2(position.width, shortHeight);
            maxSize = new Vector2(position.width, shortHeight);
            previousWindowWidth = maxSize.x;
            isInit = false;

            w1 = 155; //180; //position.width * 0.2f;
            w4 = 100; //position.width * 0.4f / 3;
            
            _imc = new IntegrationManagerController();
            _imc.OnDataLoaded += () => isInit = true;
            _imc.Initialize();

            _integrationViewToDraw = IntegrationView.SettingsAndModules;
            _rmc = new RemoteSettingsController(_imc);
            _remoteSettingsDrawer = new RemoteSettingsDrawer(_rmc, _theme);
            _rmc.RequestConfirmPluginUpdates += OnRequestPluginUpdatesConfirmation;
            _rmc.UpdateSdkCompleted += OnUpdateSdkCompleted;
            _rmc.Initialize();
            _pluginsUpdatePromptDrawer = new PluginsUpdatePromptDrawer(_rmc, _theme);
            _pluginsUpdatePromptDrawer.PromptCancelled += OnCancelPluginsPrompt;
            _pluginsUpdatePromptDrawer.PluginsConfirmed += OnDecidePluginsToInstall;
            _moduleInstallDrawers.Clear();
            _integrationIssuesDrawer = new IntegrationIssuesDrawer();
        }

        
        void OnGUI()
        {
            _theme.InitializeOnGUI();
            
            if (Math.Abs(previousWindowWidth - position.width) > 1)
            {
                previousWindowWidth = position.width;
            }

            GUI.skin = MainSkin;

            if (_rmc.IsVirginProject)
            {
                DrawIntegrationTab();
                return;
            }
            
            _selectedTab = GUILayout.Toolbar (_selectedTab, new string[] {"Integration", "Issues"});

            switch (_selectedTab)
            {
                case IntegrationTabIndex:
                    DrawIntegrationTab();
                    break;
                case IssuesTabIndex:
                    DrawIssuesTab();
                    break;
            }
        }
        
        private void SetWindowPositionForPrompt()
        {
            windowHeightPrePrompt = position.height;
            position = new Rect(position.x, position.y, position.width, PromptHeight);
            maxSize = new Vector2(position.width, MaxHeight);
        }

        private void RestoreWindowPositionPostPrompt()
        {
            // If the user has adjusted the height during the prompt, leave it as is
            bool userHasAdjustedHeight = !Mathf.Approximately(position.height, PromptHeight);
            
            if(_rmc.IsVirginProject || !userHasAdjustedHeight) 
                position = new Rect(position.x, position.y, position.width, windowHeightPrePrompt);
            
            maxSize = new Vector2(position.width, MaxHeight);
        }

        private void DrawIssuesTab()
        {
            _integrationIssuesDrawer.Draw(position);
        }

        private void DrawIntegrationTab()
        {
            switch (_integrationViewToDraw)
            {
                case IntegrationView.SettingsAndModules:
                    DrawSettingsAndModules();
                    break;
                case IntegrationView.PluginsUpdatePrompt:
                    _pluginsUpdatePromptDrawer.Draw(position);
                    break;
            }
        }

        private void DrawSettingsAndModules()
        {
            _remoteSettingsDrawer.Draw();
            if(_rmc.IsVirginProject) return;
            
            DrawModulesHeader();

            if (!isInit)
            {
                GUILayout.Label("Loading...", EditorStyles.boldLabel, GUILayout.Width(w1));
                return;
            }

            scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(this.position.width));
            DrawAllModules();
            GUILayout.EndScrollView();
        }

        private void DrawModulesHeader()
        {
            GUILayout.Space(SPACING);
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Module Name", EditorStyles.boldLabel, GUILayout.Width(w1));
            GUILayout.Space(83);
            GUILayout.Label("Installed Version", EditorStyles.boldLabel, GUILayout.Width(w4));
            // GUILayout.Label("Latest", EditorStyles.boldLabel, GUILayout.Width(w3));
            EditorGUILayout.Space();
            // GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Width(w4));
            // GUILayout.Label("", EditorStyles.boldLabel, GUILayout.Width(w5));
            DrawPrefabButton();
            DrawGenerateConfigFileButton();
            GUILayout.EndHorizontal();
        }

        private void DrawAllModules()
        {
            // foreach (var moduleList in _imc.Data.MainJson.Versions)
            // {
            //     FGModuleInstallDrawer drawer = new FGModuleInstallDrawer(_imc, this);
            //     drawer.Draw(moduleList, 0);
            // }

            for (int i = 0; i < _imc.Data.MainJson.Versions.Count; i++)
            {
                if(_moduleInstallDrawers.Count <= i) _moduleInstallDrawers.Add(new FGModuleInstallDrawer(_imc, this)); 
                FGModuleInstallDrawer drawer = _moduleInstallDrawers[i];
                drawer.Draw(_imc.Data.MainJson.Versions[i], i);
            }
        }

        private void DrawPrefabButton()
        {
            var iconPath = FGModuleInstallDrawer.ICONS_PATH + "Box-Icon.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = "Add All Prefab(s) to Scene and Create Settings assets";
            GUILayoutOption[] options = { GUILayout.Width(32), GUILayout.Height(20) };
            if (GUILayout.Button(new GUIContent(icon, tooltip), options))
            {
                // Check if there is already an EventSystem in the scene
                if (FindObjectOfType<EventSystem>() == null)
                {
                    // Create a new EventSystem
                    GameObject eventSystemObject = new GameObject("EventSystem");
                    eventSystemObject.AddComponent<EventSystem>();
                    eventSystemObject
                        .AddComponent<
                            StandaloneInputModule>(); // Optional: Add an input module (e.g., for mouse/keyboard input)
                }

                FGPackage[] allPackages = ProjectUtils.GetEnumerableOfType<FGPackage>().ToArray();
                foreach (var package in allPackages)
                {
                    package.AddPrefabs();
                    package.CreateSettingsAsset();
                }
            }
        }

        private void DrawGenerateConfigFileButton()
        {
            var iconPath = FGModuleInstallDrawer.ICONS_PATH + "Share-Icon_2.png";
            Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
            string tooltip = "Export Config File";
            GUILayoutOption[] options = { GUILayout.Width(32), GUILayout.Height(20) };
            if (GUILayout.Button(new GUIContent(icon, tooltip), options))
            {
                FGConfigFile.Export();
            }
        }

        public void SelectIssuesTab()
        {
            _selectedTab = IssuesTabIndex;
        }
        
        private void OnRequestPluginUpdatesConfirmation(List<string> pluginsToPrompt)
        {
            _integrationViewToDraw = IntegrationView.PluginsUpdatePrompt;
            _pluginsUpdatePromptDrawer.SetPluginsToPrompt(pluginsToPrompt);
            SetWindowPositionForPrompt();
        }
        
        private void OnCancelPluginsPrompt()
        {
            _integrationViewToDraw = IntegrationView.SettingsAndModules;
            RestoreWindowPositionPostPrompt();
        }

        private void OnDecidePluginsToInstall(List<string> pluginsToInstall)
        {
            _integrationViewToDraw = IntegrationView.SettingsAndModules;
            _remoteSettingsDrawer.OnDecidePluginsToInstall(pluginsToInstall);
            RestoreWindowPositionPostPrompt();
        }

        private void OnUpdateSdkCompleted()
        {
            maxSize = new Vector2(position.width, MaxHeight);
        }
        
        enum IntegrationView
        {
            SettingsAndModules,
            PluginsUpdatePrompt
        }

    }

    public class TooltipBuilder
    {
        List<string> tooltips = new List<string>();

        public void Add(string tooltip)
        {
            tooltips.Add(tooltip);
        }

        public override string ToString()
        {
            StringBuilder tooltipBuilder = new StringBuilder();
            for (int i = 0; i < tooltips.Count; i++)
            {
                if (tooltips.Count > 1) tooltipBuilder.Append(i + ". ");
                tooltipBuilder.Append(tooltips[i]);
                if (tooltips.Count > 1 && i != tooltips.Count - 1) tooltipBuilder.Append("\n");
                if (i != tooltips.Count - 1) tooltipBuilder.Append("\n");
            }

            return tooltipBuilder.ToString();
        }
    }
}