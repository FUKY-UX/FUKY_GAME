using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System.Threading;
using UnityEngine.Rendering;

public class MRotateReciver : MonoBehaviour
{
    //public Transform TestObj;
    //public TextMeshProUGUI DebugThing;
    public const float fetchInterval = 0.02f;
    public string MURL = "http://10.20.13.100/floatsM";// ������������ݵ�URL    
    //ԭ���������������
    public Quaternion RawMouseRotation = Quaternion.identity; //=> Quaternion.Euler(RawMouseEulerRotate);
    public Vector3 RawMouseEulerRotate => RawMouseRotation.eulerAngles;//��ʵʱ�������������ǵ����ݲ�ת����Unity���Խ��͵�ŷ����ת
    public Matrix4x4 RawMouseRotate4M => Matrix4x4.Rotate(RawMouseRotation);

    //����������ƫ�Ƶ��߼�
    private Vector2 MouseMoving;
    private Vector2 LasteMoving;
    private Quaternion LasteRotation;

    void Start()
    {
        StartCoroutine(FetchMouseCoroutine());
    }

    // Э�������첽��ȡ����    
    private IEnumerator FetchMouseCoroutine()
    {
        while (true) // ����ѭ����ֱ��Э�̱��ⲿֹͣ  
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(MURL))
            {
                // �������󲢵ȴ���Ӧ    
                yield return webRequest.SendWebRequest();

                // ���HTTP״̬���Ƿ�Ϊ200���ɹ���    
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // ��ȡ��Ӧ�ı�    
                    string jsonResponse = webRequest.downloadHandler.text;
                    // ����JSON����    
                    FloatsData data = JsonUtility.FromJson<FloatsData>(jsonResponse);
                    //Debug.Log("Received floats: " + string.Join(", ", data.ypr1));// ����
                    if (data.ypr1.Length >= 4)
                    {
                        RawMouseRotation =   new Quaternion(data.ypr1[1], -data.ypr1[2], -data.ypr1[0], data.ypr1[3]);
                        //RawMouseEulerRotate = new Vector3(-data.ypr1[1] * 90, (data.ypr1[0] * 90), -data.ypr1[2] * 90); // ��ñ���
                    }
                }
                else
                {
                    // �������ʧ�ܣ���ӡ������Ϣ    
                    Debug.LogError("Error fetching data: " + webRequest.error);
                }
            }
            // �ȴ�ָ����ʱ������Ȼ���ٴγ��Ի�ȡ����  
            yield return new WaitForSeconds(fetchInterval);
        }
    }
    // ����һ�������JSON��Ӧƥ���C#��    
    [System.Serializable]
    private class FloatsData
    {
        public float[] ypr1;
    }
}