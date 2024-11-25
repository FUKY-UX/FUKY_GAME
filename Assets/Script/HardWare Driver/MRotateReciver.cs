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
    public string MURL = "http://10.20.13.100/floatsM";// 鼠标陀螺仪数据的URL    
    //原生鼠标陀螺仪输入
    public Quaternion RawMouseRotation = Quaternion.identity; //=> Quaternion.Euler(RawMouseEulerRotate);
    public Vector3 RawMouseEulerRotate => RawMouseRotation.eulerAngles;//会实时接收来自陀螺仪的数据并转换成Unity可以解释的欧拉旋转
    public Matrix4x4 RawMouseRotate4M => Matrix4x4.Rotate(RawMouseRotation);

    //处理陀螺仪偏移的逻辑
    private Vector2 MouseMoving;
    private Vector2 LasteMoving;
    private Quaternion LasteRotation;

    void Start()
    {
        StartCoroutine(FetchMouseCoroutine());
    }

    // 协程用于异步获取数据    
    private IEnumerator FetchMouseCoroutine()
    {
        while (true) // 无限循环，直到协程被外部停止  
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(MURL))
            {
                // 发送请求并等待响应    
                yield return webRequest.SendWebRequest();

                // 检查HTTP状态码是否为200（成功）    
                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    // 获取响应文本    
                    string jsonResponse = webRequest.downloadHandler.text;
                    // 解析JSON数据    
                    FloatsData data = JsonUtility.FromJson<FloatsData>(jsonResponse);
                    //Debug.Log("Received floats: " + string.Join(", ", data.ypr1));// 调试
                    if (data.ypr1.Length >= 4)
                    {
                        RawMouseRotation =   new Quaternion(data.ypr1[1], -data.ypr1[2], -data.ypr1[0], data.ypr1[3]);
                        //RawMouseEulerRotate = new Vector3(-data.ypr1[1] * 90, (data.ypr1[0] * 90), -data.ypr1[2] * 90); // 存好备用
                    }
                }
                else
                {
                    // 如果请求失败，打印错误信息    
                    Debug.LogError("Error fetching data: " + webRequest.error);
                }
            }
            // 等待指定的时间间隔，然后再次尝试获取数据  
            yield return new WaitForSeconds(fetchInterval);
        }
    }
    // 定义一个与你的JSON响应匹配的C#类    
    [System.Serializable]
    private class FloatsData
    {
        public float[] ypr1;
    }
}