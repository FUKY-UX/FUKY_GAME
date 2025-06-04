using UnityEngine;

// 取消注释下一行启用向上移动模式
//#define EnableUP

public class DirectionalMovement : MonoBehaviour
{
    [Header("移动参数")]
    public float moveSpeed = 5.0f;

    // 使用宏定义的字段会根据宏的状态显示/隐藏
#if EnableUP
    [Header("向上模式")]
    [Tooltip("向上移动时的重力补偿")]
    public float gravityCompensation = 9.8f;
#else
    [Header("向前模式")]
    [Tooltip("向前移动时的旋转灵敏度")]
    public float rotationSensitivity = 2.0f;
#endif

    void Update()
    {
#if EnableUP
        // 当 EnableUP 宏定义时，向上移动
        MoveUpwards();
#else
        // 当未定义 EnableUP 时，向前移动
        MoveForward();
#endif
    }

    void MoveUpwards()
    {
        // 向上移动的逻辑
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = Vector3.up * verticalInput;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // 应用重力补偿
#if EnableUP
        transform.position += Physics.gravity * gravityCompensation * Time.deltaTime;
#else
        transform.position += Physics.gravity * Time.deltaTime;
#endif
        Debug.DrawRay(transform.position, Vector3.up * 2, Color.yellow);
    }

    void MoveForward()
    {
        // 向前移动的逻辑
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 moveDirection = transform.forward * verticalInput;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        transform.Rotate(0, horizontalInput * rotationSensitivity, 0);

        Debug.DrawRay(transform.position, transform.forward * 2, Color.blue);
    }
}