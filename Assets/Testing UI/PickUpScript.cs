using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpScript : MonoBehaviour
{
    public GameObject player;
    public Transform holdPos;
    //如果您从以下部分复制，您在法律上需要喜欢这个视频
    public float throwForce = 500f; //物体被投掷时的力度
    public float pickUpRange = 5f; //玩家可以捡起物体的最大距离
    private float rotationSensitivity = 1f; //物体旋转的灵敏度，与鼠标移动相关
    private GameObject heldObj; //玩家捡起的物体
    private Rigidbody heldObjRb; //捡起物体的刚体
    private bool canDrop = true; //用于防止在旋转物体时抛掷/丢弃物体
    private int LayerNumber; //层级索引

    //引用包含玩家鼠标移动（查看周围）的脚本
    //当旋转物体时，我们希望禁用玩家的查看功能
    //以下是示例
    //MouseLookScript mouseLookScript;

    void Start()
    {
        LayerNumber = LayerMask.NameToLayer("holdLayer"); //如果您的 holdLayer 名称不同，请确保更改此处的 ""

        //mouseLookScript = player.GetComponent<MouseLookScript>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E)) //更改 E 为您希望按下的捡起键
        {
            if (heldObj == null) //如果当前没有持有任何物体
            {
                //执行射线检测，检查玩家是否在捡起范围内看着物体
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
                {
                    //确保物体附加了 "canPickUp" 标签
                    if (hit.transform.gameObject.tag == "canPickUp")
                    {
                        //将被射线击中的物体传递给 PickUpObject 函数
                        PickUpObject(hit.transform.gameObject);
                    }
                }
            }
            else
            {
                if (canDrop == true)
                {
                    StopClipping(); //防止物体穿透墙壁
                    DropObject();
                }
            }
        }

        if (heldObj != null) //如果玩家持有物体
        {
            MoveObject(); //保持物体在 holdPos 位置
            RotateObject();
            if (Input.GetKeyDown(KeyCode.Mouse0) && canDrop == true) //Mouse0（左键）用于投掷，可以更改为其他按钮
            {
                StopClipping();
                ThrowObject();
            }
        }
    }

    void PickUpObject(GameObject pickUpObj)
    {
        if (pickUpObj.GetComponent<Rigidbody>()) //确保物体有刚体
        {
            heldObj = pickUpObj; //将 heldObj 赋值为被射线击中的物体（不再为 null）
            heldObjRb = pickUpObj.GetComponent<Rigidbody>(); //获取物体的刚体
            heldObjRb.isKinematic = true;
            heldObjRb.transform.parent = holdPos.transform; //将物体设为 holdPos 的子物体
            heldObj.layer = LayerNumber; //将物体的层级更改为 holdLayer
            //确保物体不会与玩家发生碰撞，以防止奇怪的错误
            Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), true);
        }
    }

    void DropObject()
    {
        //重新启用与玩家的碰撞
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0; //将物体层级设置回默认层
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null; //取消物体的父子关系
        heldObj = null; //取消对物体的引用
    }

    void MoveObject()
    {
        //保持物体位置与 holdPos 一致
        heldObj.transform.position = holdPos.transform.position;
    }

    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R)) //按住 R 键旋转，可以更改为其他键
        {
            canDrop = false; //确保在旋转时无法抛掷

            //禁用玩家的查看功能
            //mouseLookScript.verticalSensitivity = 0f;
            //mouseLookScript.lateralSensitivity = 0f;

            float XaxisRotation = Input.GetAxis("Mouse X") * rotationSensitivity;
            float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSensitivity;
            //根据鼠标 X-Y 轴旋转物体
            heldObj.transform.Rotate(Vector3.down, XaxisRotation);
            heldObj.transform.Rotate(Vector3.right, YaxisRotation);
        }
        else
        {
            //重新启用玩家的查看功能
            //mouseLookScript.verticalSensitivity = originalvalue;
            //mouseLookScript.lateralSensitivity = originalvalue;
            canDrop = true;
        }
    }

    void ThrowObject()
    {
        //与 Drop 函数相同，但在取消引用前向物体添加力
        Physics.IgnoreCollision(heldObj.GetComponent<Collider>(), player.GetComponent<Collider>(), false);
        heldObj.layer = 0;
        heldObjRb.isKinematic = false;
        heldObj.transform.parent = null;
        heldObjRb.AddForce(transform.forward * throwForce);
        heldObj = null;
    }

    void StopClipping() //仅在丢弃/投掷时调用的函数
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position); //holdPos 与摄像机之间的距离
        //必须使用 RaycastAll，因为物体会阻挡屏幕中心的射线
        //RaycastAll 返回在 clipRange 内所有被击中的碰撞体数组
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        //如果数组长度大于 1，意味着它击中了除了我们持有的物体之外的其他物体
        if (hits.Length > 1)
        {
            //将物体位置更改为摄像机位置
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f); //稍微向下偏移，以防止物体在玩家上方丢弃
            //如果您的玩家较小，可以将 -0.5f 更改为较小的数值（绝对值），例如：-0.1f
        }
    }
}

