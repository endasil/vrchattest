
using Assets.Scripts;

using Cyan.PlayerObjectPool;

using UdonSharp;
using Unity.XR.Oculus;
using UnityEditor;

using UnityEngine;

using VRC.SDKBase;
#pragma warning disable IDE0090

public class PlayerHandler : UdonSharpBehaviour
{
    public int startingHealth = 100;

    public PlayerListenerBase[] PlayerListeners;

    public Cyan.PlayerObjectPool.CyanPlayerObjectAssigner assigner;


    [System.NonSerialized]
    public Player LocalPlayer;

    [HideInInspector]
    public Player[] players;

    // This is editor only code, don't want U# to try to compile it
#if !COMPILER_UDONSHARP && UNITY_EDITOR

    // Automatically configure the players in the editor with default values
    public static bool SetupPlayers()
    {
         
        CyanPlayerObjectAssigner cyanPlayerObjectAssigner = FindObjectOfType<CyanPlayerObjectAssigner>();
        Player[] players = FindObjectsOfType<Player>();
        PlayerListenerBase[] Listeners = FindObjectsOfType<PlayerListenerBase>();
        PlayerHandler[] playerHandlers = FindObjectsOfType<PlayerHandler>();
        if (playerHandlers.Length > 1)
        {
            ErrorLog(playerHandlers[0], "Multiple Player Handlers found in scene. There should only be one.");
            return false;
        }

        if (playerHandlers.Length == 0)
        {
            ErrorLog("No Player Handlers found in scene. One is needed.");
            return false;
        }

        // Create a reference suitable for handling in editor mode. 
        SerializedObject serializedHandler = new SerializedObject(playerHandlers[0]);
        if (!Utilities.IsValid(serializedHandler))
        {
            Debug.LogError("Handler is not valid");
            return false;
        }

        var assigner = serializedHandler.FindProperty("assigner");
        if (!Utilities.IsValid(assigner))
        {
            Debug.LogError("assigner is not valid");
            return false;
        }


        assigner.objectReferenceValue = cyanPlayerObjectAssigner;

        if (playerHandlers[0].players != players)
        {
            serializedHandler.FindProperty("players").ClearArray();
            for (int i = 0; i < players.Length; i++)
            {
                serializedHandler.FindProperty(nameof(players)).InsertArrayElementAtIndex(i);
                serializedHandler.FindProperty(nameof(players)).GetArrayElementAtIndex(i).objectReferenceValue = players[i];
            }
        }
        else
        {
            InfoLog(playerHandlers[0], "Players already configured. Skipping AutoSetup.");
        }
        serializedHandler.ApplyModifiedProperties();
        int playerHandlersSetup = 0;

        foreach (var player in players)
        {

            // Already setup
            if (player.playerHandler == playerHandlers[0] && player.capsuleCollider != null && player.capsuleCollider.gameObject == player.gameObject)
            {
                continue;
            }

            if (!IsEditable(player))
            {
                ErrorLog("Player is not editable");
            }

            SerializedObject serializedPlayer = new SerializedObject(player);
            serializedPlayer.FindProperty(nameof(Player.playerHandler)).objectReferenceValue = playerHandlers[0];
            serializedPlayer.FindProperty(nameof(Player.capsuleCollider)).objectReferenceValue =
                player.GetComponent<CapsuleCollider>();
            serializedPlayer.ApplyModifiedProperties();
            playerHandlersSetup++;
        }

        InfoLog("PlayerHandlers setup: " + playerHandlersSetup);

        foreach (PlayerListenerBase listener in Listeners)
        {
            if (listener.playerHandler == playerHandlers[0])
            {
                continue;
            }
            if (!IsEditable(listener))
            {
                ErrorLog(listener, "Listener is not editable");
                continue;
            }
            SerializedObject serializedListener = new SerializedObject(listener);
            serializedListener.FindProperty("playerHandler").objectReferenceValue = playerHandlers[0];
            serializedListener.ApplyModifiedProperties();
        }

        return true;
    }

    public static void ErrorLog(string message)
    {
        Debug.LogErrorFormat("<color=red>[PlayerHandler AutoSetup]: ERROR</color> {0}", message);
    }

    public static void ErrorLog(Object context, string message)
    {
        Debug.LogErrorFormat(context, "<color=red>[PlayerHandler AutoSetup]: ERROR</color> {0}", message);
    }

    public static void InfoLog(string message)
    {
        Debug.LogFormat("<color=blue>[PlayerHandler AutoSetup]: INFO</color> {0}", message);

    }

    public static void InfoLog(Object context, string message)
    {
        Debug.LogFormat(context, "<color=blue>[PlayerHandler AutoSetup]: INFO</color> {0}", message);
    }

    public static bool IsEditable(Component component)
    {
        return !EditorUtility.IsPersistent(component.transform.root.gameObject) && !(component.gameObject.hideFlags == HideFlags.NotEditable || component.gameObject.hideFlags == HideFlags.HideAndDontSave);
    }


#endif

}

