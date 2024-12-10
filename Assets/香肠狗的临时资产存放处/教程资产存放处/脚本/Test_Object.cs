using UnityEngine;

// ���� InteractedItemOrigin ������Ŀ���Ѵ����Ҳ���Ҫ override �����Ļ���
// ������Ʒ�࣬��������ץȡ���ͷ��߼�
public class ������Ʒ : InteractedItemOrigin
{
    [SerializeField]
    private Collider _myCollider; // ���ڿ���ץȡ���ͷ�ʱ������

    /// <summary>
    /// ����Ʒ��ץȡʱ����ΪTriggerʹ�䲻��������ײӰ�죬��������ƶ���Ʒ��
    /// </summary>
    public void OnGrab()
    {
        if (_myCollider != null)
        {
            _myCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// ����Ʒ���ͷ�ʱ���ָ�������ײ��
    /// </summary>
    public void OnRelease()
    {
        if (_myCollider != null)
        {
            _myCollider.isTrigger = false;
        }
    }
}

