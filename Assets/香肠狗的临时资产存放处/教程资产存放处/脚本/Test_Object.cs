using UnityEngine;

// 假设 InteractedItemOrigin 是你项目中已存在且不需要 override 方法的基类
// 测试物品类，仅保留简单抓取与释放逻辑
public class 测试物品 : InteractedItemOrigin
{
    [SerializeField]
    private Collider _myCollider; // 用于控制抓取和释放时物理交互

    /// <summary>
    /// 当物品被抓取时，设为Trigger使其不受物理碰撞影响，方便玩家移动物品。
    /// </summary>
    public void OnGrab()
    {
        if (_myCollider != null)
        {
            _myCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// 当物品被释放时，恢复物理碰撞。
    /// </summary>
    public void OnRelease()
    {
        if (_myCollider != null)
        {
            _myCollider.isTrigger = false;
        }
    }
}

