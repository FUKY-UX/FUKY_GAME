using UnityEngine;

public class RelativeLookAt : MonoBehaviour
{
    public Transform Earth;  // 要旋转的物体
    public Transform Finger;  // 目标物体
    public bool inRotationMode = false;  // 是否处于旋转模式
    public bool SelectMode = false;  // 是否处于选择模式
    // 发光连线设置
    public GameObject RingUI;    // 发光颜色
    public Color glowColor = Color.cyan;    // 发光颜色
    public float glowIntensity = 2f;         // 发光强度
    public float glowWidth = 0.1f;           // 发光线宽度
    private LineRenderer glowLine;           // 发光线渲染器
    public Material lineMaterial;

    // 存储第一次按下E时的位置关系
    private Vector3 initialDirection;     // 初始方向向量
    private Vector3 initialPositionA;     // 物体A初始位置
    private Vector3 initialPositionB;     // 物体B初始位置
    private Quaternion initialRotationA;  // 物体A初始旋转

    private void Start()
    {
        // 初始化发光线
        InitializeGlowLine();
    }

    /// <summary>
    /// 初始化发光线
    /// </summary>
    private void InitializeGlowLine()
    {
        // 确保有GameObject存在
        if (gameObject.GetComponent<LineRenderer>() == null)
        {
            glowLine = gameObject.AddComponent<LineRenderer>();
        }
        else
        {
            glowLine = gameObject.GetComponent<LineRenderer>();
        }
        RingUI.SetActive(false);
        // 设置发光线属性
        glowLine.startWidth = glowWidth;
        glowLine.endWidth = glowWidth;
        glowLine.positionCount = 2;

        // 创建发光材质
        Material glowMaterial = lineMaterial;
        glowMaterial.SetColor("_TintColor", new Color(glowColor.r, glowColor.g, glowColor.b, glowIntensity));
        glowLine.material = glowMaterial;

        // 设置发光线颜色
        glowLine.startColor = glowColor;
        glowLine.endColor = glowColor;

        // 初始状态隐藏发光线
        glowLine.enabled = false;
    }
    
    void Update()
    {
        if (SelectMode)
        {
            if (glowLine != null) glowLine.enabled = true;
            UpdateGlowLine(); return; 
        }
        else if(inRotationMode)
        {
            if (glowLine != null) glowLine.enabled = true;
            ApplyRelativeRotation();
            UpdateGlowLine();
        }
        else
        {
            if (glowLine != null) glowLine.enabled = false;
            Finger.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 更新发光线位置
    /// </summary>
    public void UpdateGlowLine()
    {
        if (glowLine != null && Earth != null && Finger != null)
        {
            // 设置发光线的起点和终点
            glowLine.SetPosition(0, Earth.position);
            glowLine.SetPosition(1, Finger.position);
            RingUI.transform.position = Finger.position;
            // 根据距离动态调整发光强度和宽度
            float distance = Vector3.Distance(Earth.position, Finger.position);
            glowLine.startWidth = glowWidth * (1 + distance * 0.1f);
            glowLine.endWidth = glowLine.startWidth;
        }
    }

    /// <summary>
    /// 开始旋转 - 保存初始位置关系
    /// </summary>
    public void StartRotation()
    {
        if (Earth == null || Finger == null)
        {
            Debug.LogWarning("请先设置objectA和objectB!");
            return;
        }

        inRotationMode = true;

        // 保存初始位置关系
        initialPositionA = Earth.position;
        initialPositionB = Finger.position;
        initialRotationA = Earth.rotation;

        // 计算初始方向向量 (A到B的方向)
        initialDirection = (initialPositionB - initialPositionA).normalized;
    }

    /// <summary>
    /// 应用基于初始位置关系的相对旋转
    /// </summary>
    public Quaternion ApplyRelativeRotation()
    {
        // 计算当前位置关系
        Vector3 currentDirection = (Finger.position - Earth.position).normalized;

        // 计算从初始方向到当前方向的旋转
        Quaternion deltaRotation = Quaternion.FromToRotation(initialDirection, currentDirection);

        // 应用旋转 (保持初始旋转关系的基础上增加旋转)
        Earth.rotation = deltaRotation * initialRotationA;

        return deltaRotation;
    }

    // 在场景中绘制可视化参考
    void OnDrawGizmos()
    {
        if (inRotationMode)
        {
            // 绘制初始位置连线
            Gizmos.color = Color.green;
            Gizmos.DrawLine(initialPositionA, initialPositionB);
            Gizmos.DrawSphere(initialPositionA, 0.1f);
            Gizmos.DrawSphere(initialPositionB, 0.1f);

            // 绘制当前位置连线
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(Earth.position, Finger.position);

            // 使用Gizmos替代Handles.Label
            DrawLabel(initialPositionA + Vector3.up * 0.5f, "初始位置A", Color.green);
            DrawLabel(initialPositionB + Vector3.up * 0.5f, "初始位置B", Color.green);
            DrawLabel(Earth.position + Vector3.up * 0.5f, "当前A", Color.blue);
            DrawLabel(Finger.position + Vector3.up * 0.5f, "当前B", Color.blue);
        }
    }
    /// <summary>
    /// 使用Gizmos绘制文本标签
    /// </summary>
        private void DrawLabel(Vector3 position, string text, Color color)
        {
            // 在Unity 2021.2+中可以使用Gizmos.DrawGUITexture
    #if UNITY_EDITOR
            UnityEditor.Handles.BeginGUI();
            Vector3 screenPos = Camera.current.WorldToScreenPoint(position);
            if (screenPos.z > 0) // 确保对象在相机前面
            {
                var style = new GUIStyle();
                style.normal.textColor = color;
                style.fontSize = 12;
                GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 20, 100, 20), text, style);
            }
            UnityEditor.Handles.EndGUI();
    #endif
    }
}