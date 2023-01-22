using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEditor;

public static class DialogueSystemUnityMenus
{
    [MenuItem("GameObject/DialogueSystem/Speaker")]
    static void CreateSpeaker(MenuCommand menuCommand)
    {
        InstantiatePrefabAtPath("Assets/DialogueSystem/Prefabs/DialogueSpeaker.prefab", menuCommand);
    }

    [MenuItem("GameObject/DialogueSystem/EventTrigger/OnTrigger")]
    static void CreateEventTrigger_OnTrigger(MenuCommand menuCommand)
    {
        InstantiatePrefabAtPath("Assets/DialogueSystem/Prefabs/EventTrigger_OnTrigger.prefab", menuCommand);
    }

    [MenuItem("GameObject/DialogueSystem/EventTrigger/OnCollision")]
    static void CreateEventTrigger_OnCollision(MenuCommand menuCommand)
    {
        InstantiatePrefabAtPath("Assets/DialogueSystem/Prefabs/EventTrigger_OnCollision.prefab", menuCommand);
    }

    [MenuItem("GameObject/DialogueSystem/Interaction/Option")]
    static void CreateInteractionOption(MenuCommand menuCommand)
    {
        InstantiatePrefabAtPath("Assets/DialogueSystem/Prefabs/InteractionOption.prefab", menuCommand);
    }

    [MenuItem("GameObject/DialogueSystem/Interaction/Receiver")]
    static void CreateInteractionReceiver(MenuCommand menuCommand)
    {
        InstantiatePrefabAtPath("Assets/DialogueSystem/Prefabs/InteractionReceiver.prefab", menuCommand);
    }

    [MenuItem("GameObject/DialogueSystem/ModifyFact/OnTrigger")]
    static void CreateModifyFact_OnTrigger(MenuCommand menuCommand)
    {
        InstantiatePrefabAtPath("Assets/DialogueSystem/Prefabs/ModifyFact_OnTrigger.prefab", menuCommand);
    }

    static void InstantiatePrefabAtPath(string path, MenuCommand menuCommand)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab == null)
        {
            Debug.LogError("Cannot find Asset at path:\n" + path);
            return;
        }

        GameObject go = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register undo
        Undo.RegisterCreatedObjectUndo(prefab, "Create " + go.name);
        Selection.activeObject = go;
    }
}
