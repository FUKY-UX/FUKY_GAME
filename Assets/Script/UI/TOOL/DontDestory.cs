using UnityEngine;

public class PersistentSingleton : MonoBehaviour
{
    public static PersistentSingleton Instance { get; private set; }

    private void Awake()
    {
        // 如果实例已存在且不是当前对象，则销毁新实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}