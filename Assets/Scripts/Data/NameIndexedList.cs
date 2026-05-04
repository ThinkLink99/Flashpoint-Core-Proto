using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringKeyedList<T>
{
    [Serializable]
    public class Entry
    {
        public string Key;
        public T Value;
    }

    [SerializeField]
    private List<Entry> items = new List<Entry>();

    private Dictionary<string, T> lookup;

    // Build dictionary lazily (not serialized)
    private void BuildLookup()
    {
        if (lookup != null) return;

        lookup = new Dictionary<string, T>();
        foreach (var item in items)
        {
            if (!string.IsNullOrEmpty(item.Key))
            {
                lookup[item.Key] = item.Value;
            }
        }
    }

    public T this[string key]
    {
        get
        {
            BuildLookup();
            if (lookup.TryGetValue(key, out var value))
                return value;

            throw new KeyNotFoundException($"Key '{key}' not found.");
        }
        set
        {
            BuildLookup();

            if (lookup.ContainsKey(key))
            {
                lookup[key] = value;

                // Sync back to list
                foreach (var item in items)
                {
                    if (item.Key == key)
                    {
                        item.Value = value;
                        return;
                    }
                }
            }
            else
            {
                lookup[key] = value;
                items.Add(new Entry { Key = key, Value = value });
            }
        }
    }

    public bool TryGetValue(string key, out T value)
    {
        BuildLookup();
        return lookup.TryGetValue(key, out value);
    }

    public List<Entry> Items => items;
    public T[] ToArray ()
    {
        List<T> list = new List<T>();
        foreach (var entry in items)
        {
            list.Add(entry.Value); 
        }

        return list.ToArray();
    }
}