using UnityEngine;

// ȡ��ע����һ�����������ƶ�ģʽ
//#define EnableUP

public class DirectionalMovement : MonoBehaviour
{
    [Header("�ƶ�����")]
    public float moveSpeed = 5.0f;

    // ʹ�ú궨����ֶλ���ݺ��״̬��ʾ/����
#if EnableUP
    [Header("����ģʽ")]
    [Tooltip("�����ƶ�ʱ����������")]
    public float gravityCompensation = 9.8f;
#else
    [Header("��ǰģʽ")]
    [Tooltip("��ǰ�ƶ�ʱ����ת������")]
    public float rotationSensitivity = 2.0f;
#endif

    void Update()
    {
#if EnableUP
        // �� EnableUP �궨��ʱ�������ƶ�
        MoveUpwards();
#else
        // ��δ���� EnableUP ʱ����ǰ�ƶ�
        MoveForward();
#endif
    }

    void MoveUpwards()
    {
        // �����ƶ����߼�
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 moveDirection = Vector3.up * verticalInput;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);

        // Ӧ����������
#if EnableUP
        transform.position += Physics.gravity * gravityCompensation * Time.deltaTime;
#else
        transform.position += Physics.gravity * Time.deltaTime;
#endif
        Debug.DrawRay(transform.position, Vector3.up * 2, Color.yellow);
    }

    void MoveForward()
    {
        // ��ǰ�ƶ����߼�
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        Vector3 moveDirection = transform.forward * verticalInput;
        transform.Translate(moveDirection * moveSpeed * Time.deltaTime);
        transform.Rotate(0, horizontalInput * rotationSensitivity, 0);

        Debug.DrawRay(transform.position, transform.forward * 2, Color.blue);
    }
}