using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

public class SimpleReorderableList<T> where T : UnityEngine.Object
{
    ReorderableList list;

    Action onDrawElementAction;
    Func<T> onAddAction; // Return the element to add
    Action<T> onRemoveAction;
    Action<T> onSelectAction;

    List<T> listRef;

    string name;

    bool ifDrawUsingLabel;


    public SimpleReorderableList(string name, List<T> listRef, SerializedProperty arrayProp, 
        Action onDrawElementAction = null, 
        Func<T> onAddAction = null, 
        Action<T> onRemoveAction = null, 
        Action<T> onSelectAction = null,
        
        bool ifDrawUsingLabel = true)
    {
        list = new ReorderableList(arrayProp.serializedObject, arrayProp, true, true, true, true);
        list.drawElementCallback = DrawListItems;
        list.drawHeaderCallback = DrawHeader;
        list.onAddCallback = OnAddElement;
        list.onRemoveCallback = OnRemoveElement;
        list.onReorderCallback = OnReorder;

        list.onSelectCallback = OnSelect;

        this.name = name;
        this.listRef = listRef;

        if (onAddAction == null)
            this.onAddAction = DefaultOnAddAction;
        else
            this.onAddAction = onAddAction;

        this.onDrawElementAction = onDrawElementAction;
        this.onRemoveAction = onRemoveAction;
        this.onSelectAction = onSelectAction;

        this.ifDrawUsingLabel = ifDrawUsingLabel;
    }

    public void DrawList()
    {
        list.serializedProperty.serializedObject.Update();
        list.DoLayoutList();
    }

    void DrawListItems(Rect rect, int index, bool isActive, bool isFocused)
    {
        onDrawElementAction?.Invoke();
        SerializedProperty prop = list.serializedProperty.GetArrayElementAtIndex(index);
        rect.size = new Vector2(rect.size.x, EditorGUIUtility.singleLineHeight);

        rect.position = rect.position + new Vector2(0f, 1f);
        if (ifDrawUsingLabel && prop.objectReferenceValue != null)
            EditorGUI.LabelField(rect, new GUIContent(prop.objectReferenceValue.name));
        else
            EditorGUI.PropertyField(rect, prop);

        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    void DrawHeader(Rect rect)
    {
        EditorGUI.LabelField(rect, name);
    }

    T DefaultOnAddAction()
    {
        return null;
    }

    void OnAddElement(ReorderableList list)
    {
        listRef.Add(onAddAction.Invoke());

        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    private void OnRemoveElement(ReorderableList list)
    {
        SerializedProperty prop = list.serializedProperty.GetArrayElementAtIndex(list.index);
        onRemoveAction?.Invoke((T)prop.objectReferenceValue);

        listRef.RemoveAt(list.index);
        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    void OnReorder(ReorderableList list)
    {
        list.serializedProperty.serializedObject.ApplyModifiedProperties();
    }

    void OnSelect(ReorderableList list)
    {
        SerializedProperty prop = list.serializedProperty.GetArrayElementAtIndex(list.index);
        onSelectAction?.Invoke((T)prop.objectReferenceValue);
    }

}
