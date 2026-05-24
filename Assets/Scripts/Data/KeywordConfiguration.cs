using System.Collections.Generic;
using System.Text;
using UnityEngine;

[CreateAssetMenu (fileName = "New Keyword", menuName = "Keyword")]
public class KeywordConfiguration : ScriptableObject
{
    public string Id;
    public string DisplayName;

    [TextArea]
    public string RulesText;

    public bool HasValue;
    public string ValueLabel; // "n", "x", "uses", etc.

    public KeywordTiming[] Timings;
 
    public bool IsWeaponKeyword;
    public bool IsModelKeyword;
    public bool IsStatusKeyword;
}

[System.Serializable]
public class KeywordInstance
{
    public KeywordConfiguration Definition;
    public int Value;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(Definition.DisplayName);
        if (Definition.HasValue)
        {
            sb.Replace("(n)", $"({Value})");
        }

        return sb.ToString();
    }
}
public class RuntimeKeyword
{
    public KeywordConfiguration Definition { get; }
    public int Value { get; private set; }  
    public int CurrentUses { get; private set; } // For consumable keywords

    public RuntimeKeyword (KeywordInstance source)
    {
        Definition = source.Definition;
        Value = source.Value;
        CurrentUses = source.Value;
    }

    public void SetValue(int newValue) => Value = newValue;
    public void IncrementValue(int amount = 1) => Value += amount;
    public void DecrementValue(int amount = 1) => Value = Mathf.Max(0, Value - amount);

    public void ResetUses() => CurrentUses = Value;
    public void DecrementCurrentUses(int amount = 1) => CurrentUses = Mathf.Max(0, CurrentUses - amount);

}
public class KeywordCollection
{
    private readonly Dictionary<string, RuntimeKeyword> keywords = new Dictionary<string, RuntimeKeyword>();

    public void Initialize (IEnumerable<KeywordInstance> keywords)
    {
        foreach (var keyword in keywords)
        {
            if (keyword.Definition == null || string.IsNullOrEmpty(keyword.Definition.Id))
            {
                Debug.LogWarning("Invalid keyword instance: missing definition or ID.");
                continue;
            }
            if (this.keywords.ContainsKey(keyword.Definition.Id))
            {
                Debug.LogWarning($"Duplicate keyword ID '{keyword.Definition.Id}' found. Skipping.");
                continue;
            }
            this.keywords.Add(keyword.Definition.Id, new RuntimeKeyword(keyword));
        }
    }

    public void AddKeyword(KeywordInstance instance)
    {
        if (instance.Definition == null || string.IsNullOrEmpty(instance.Definition.Id))
        {
            Debug.LogWarning("Invalid keyword instance: missing definition or ID.");
            return;
        }
        if (keywords.ContainsKey(instance.Definition.Id))
        {
            Debug.LogWarning($"Keyword with ID '{instance.Definition.Id}' already exists. Use UpdateKeywordValue to modify it.");
            return;
        }
        keywords[instance.Definition.Id] = new RuntimeKeyword(instance);
    }
    public bool HasKeyword(string keywordId) => keywords.ContainsKey(keywordId);
    public RuntimeKeyword GetKeyword(string keywordId)
    {
        if (keywords.TryGetValue(keywordId, out var keyword))
        {
            return keyword;
        }
        Debug.LogWarning($"Keyword with ID '{keywordId}' not found.");
        return null;
    }
    public void RemoveKeyword(string keywordId)
    {
        if (!keywords.Remove(keywordId))
        {
            Debug.LogWarning($"Failed to remove keyword with ID '{keywordId}': not found.");
        }
    }

    public IEnumerable<RuntimeKeyword> All => keywords.Values;
}