using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class CustomUnityEvent : UnityEvent<Component, object> { }

public class GameEventListener : MonoBehaviour
{
    public GameEvent gameEvent;
    public CustomUnityEvent response;

    private void OnEnable()
    {
        if (gameEvent != null)
        {
            gameEvent.RegisterListener(this);
        }
    }
    private void OnDisable()
    {
        if (gameEvent != null)
        {
            // Unregister listener logic here
            gameEvent.UnregisterListener(this);
        }
    }

    public void OnEventRaised (Component sender, object data)
    {
        response.Invoke(sender, data);
    }
}
