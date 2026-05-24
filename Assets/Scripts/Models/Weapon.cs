using UnityEngine;

public class Weapon : MonoBehaviour, IHasKeywords
{
    public WeaponConfiguration WeaponConfiguration;
    public Model ParentModel;

    public KeywordCollection Keywords { get; } = new KeywordCollection();

    public void Initialize (WeaponConfiguration config, Model parent)
    {
        WeaponConfiguration = config;
        ParentModel = parent;

        Keywords.Initialize(config.Keywords);
    }
}