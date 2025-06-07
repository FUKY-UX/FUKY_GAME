using UnityEngine;
using System.Collections.Generic;

public class PotColliderSetting : MonoBehaviour
{
    [Tooltip("������Ҫ������ת�Ĵ���ײ�����Ϸ����")]
    public List<GameObject> colliderObjects = new List<GameObject>();
    public float tilt = -45f;

    void Start() 
    {
        foreach (var item in colliderObjects)
        {
            item.transform.localRotation *= Quaternion.Euler(0, -45, 0);
        }
    }
    void Update() { }
}