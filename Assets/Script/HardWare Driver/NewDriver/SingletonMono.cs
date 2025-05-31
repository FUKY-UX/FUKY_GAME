using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingletonMono<T> : MonoBehaviour where T : SingletonMono<T>
{
    public static T Instance { get; private set; }

    protected virtual void OnEnable()
    {
        if (Instance != null)
        {
            Destroy(Instance);
        }
        Instance = this as T;
        DontDestroyOnLoad(gameObject);
    }

    protected virtual void OnDisable()
    {
        Instance = null;
    }
}
