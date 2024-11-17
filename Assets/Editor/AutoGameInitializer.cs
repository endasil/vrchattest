using UnityEditor;
using UnityEngine;
using VRC.SDKBase.Editor.BuildPipeline;

namespace Editor
{
    [InitializeOnLoad]
    public class AutoGameInitializer : IVRCSDKBuildRequestedCallback
    {
        public int callbackOrder => 0;
        public static bool AutoSetup = true;
        public const string ROOT_MENU = "VRChatGame/";
        public const string AUTO_SETUP_MENU = ROOT_MENU + "Auto Setup";


        [MenuItem(AUTO_SETUP_MENU)]
        private static void ToggleAction()
        {
            AutoSetup = !AutoSetup;
            EditorPrefs.SetBool(AUTO_SETUP_MENU, AutoSetup);
        }

        [MenuItem(AUTO_SETUP_MENU, true)]
        private static bool ToggleActionValidate()
        {
            Menu.SetChecked(AUTO_SETUP_MENU, AutoSetup);
            return true;
        } 

        [MenuItem(ROOT_MENU + "Trigger")]
        static void TriggerAutoSetup()
        {
            Setup();
        }

        // This method is called automatically when unity start, so that OnPlayModeStateChanged is assigned
        // as a callback on playModeStateChanged. playModeStateChanged happens when play or stop is pressed
        // in unity.
        [InitializeOnLoadMethod]
        public static void Initialize()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange obj)
        {
            if (AutoSetup) Setup();
        }

        public bool OnBuildRequested(VRCSDKRequestedBuildType requestedBuildType)
        {
            Debug.Log("OnBuildRequested");
            if (AutoSetup) return Setup();
            Debug.Log("Auto setup disabled, not running Setup()");
            return true;

        }

        private static bool Setup()
        {
            return PlayerHandler.SetupPlayers();
        }
    }
}
