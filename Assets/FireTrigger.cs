using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FireTrigger : MonoBehaviour
{
    [Header("��Դ����")]
    [Tooltip("����Դ�Ĳ㼶")]
    public LayerMask fireLayer;          // ��Inspector��ѡ���Դ���ڵĲ㼶
    [Tooltip("��ⷶΧ�뾶")]
    public float detectionRadius = 2f;  // �ɵ����ļ��뾶

    [Header("Ŀ������")]
    [Tooltip("��Ҫ���Ƶ�GameObject")]
    public GameObject targetObject;     // ��Ҫ����/���õ�����

    [Header("�ӳ�����")]
    [Tooltip("�뿪��Դ���ӳٹرյ�ʱ�䣨�룩")]
    public float delayOffTime = 1f;     // �ɵ������ӳٹر�ʱ��

    [Header("����ѡ��")]
    public bool showGizmos = true;      // �Ƿ���ʾ��ⷶΧ
    public Color gizmoColor = Color.red; // ��ⷶΧ��ʾ��ɫ

    private bool isNearFire = false;    // �Ƿ��ڻ�Դ����
    private float delayTimer = 0f;      // �ӳټ�ʱ��

    private void Update()
    {
        // �����Χ�Ƿ��л�Դ
        bool fireNearby = Physics.CheckSphere(transform.position, detectionRadius, fireLayer);

        // �������Դ��Χ
        if (fireNearby && !isNearFire)
        {
            isNearFire = true;
            delayTimer = 0f; // ���ü�ʱ��
            SetTargetActive(true);
        }
        // ���뿪��Դ��Χ
        else if (!fireNearby && isNearFire)
        {
            delayTimer += Time.deltaTime;

            // �����ӳ�ʱ���ر�
            if (delayTimer >= delayOffTime)
            {
                isNearFire = false;
                SetTargetActive(false);
            }
        }
        // �����ڻ�Դ��ʱ���ü�ʱ��
        else if (fireNearby)
        {
            delayTimer = 0f;
        }
    }

    // ����Ŀ������״̬
    private void SetTargetActive(bool active)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(active);
        }
    }

    // ��Scene��ͼ����ʾ��ⷶΧ
    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        Gizmos.color = gizmoColor;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
