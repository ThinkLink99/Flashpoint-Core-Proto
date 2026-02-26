using UnityEngine;

public abstract class TIG_UIBase : MonoBehaviour
{
    private void Awake()
    {
        Init();
    }
    private void OnValidate()
    {
        Init();
    }
    private void Init()
    {
        Setup();
        Configure();
    }

    public abstract void Setup();
    public abstract void Configure();
}
