using UnityEngine;

public class PersistentSingleton : MonoBehaviour
{
    public static PersistentSingleton Instance { get; private set; }

    private void Awake()
    {
        // ���ʵ���Ѵ����Ҳ��ǵ�ǰ������������ʵ��
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}