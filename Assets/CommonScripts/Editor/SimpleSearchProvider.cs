using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor;

public class SimpleSearchProvider : ScriptableObject, ISearchWindowProvider
{
    private List<System.Tuple<string, object>> items;
    System.Action<string, object> onSelectEntryCallback;

    public void Init(
        List<System.Tuple<string, object>> items, 
        System.Action<string, object> onSelectEntryCallback)
    {
        this.items = items;
        this.onSelectEntryCallback = onSelectEntryCallback;
    }

    public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
    {
        List<SearchTreeEntry> searchEntryList = new List<SearchTreeEntry>();
        searchEntryList.Add(new SearchTreeGroupEntry(new GUIContent("List"), 0));

        List<string> groups = new List<string>();

        foreach (System.Tuple<string, object> item in items)
        {
            string[] splitPath = item.Item1.Split('/');
            string groupName = "";

            for (int i = 0; i < splitPath.Length - 1; i++)
            {
                groupName += splitPath[i];

                if (!groups.Contains(groupName))
                {
                    searchEntryList.Add(new SearchTreeGroupEntry(new GUIContent(splitPath[i]), i + 1));
                    groups.Add(groupName);
                }

                groupName += "/";
            }

            SearchTreeEntry entry = new SearchTreeEntry(new GUIContent(splitPath[splitPath.Length - 1]));
            entry.level = splitPath.Length;
            entry.userData = item.Item2;
            searchEntryList.Add(entry);
        }
        
        return searchEntryList;
    }

    public bool OnSelectEntry(SearchTreeEntry SearchTreeEntry, SearchWindowContext context)
    {
        onSelectEntryCallback?.Invoke(SearchTreeEntry.content.text, SearchTreeEntry.userData);
        return true;
    }
}
