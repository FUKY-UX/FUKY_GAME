using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoader : MonoBehaviour
{

    //吐槽句子
    public DialogData[] ShitTalkingInMenue;

    [Header("关卡加载视觉")]
    public Image loadingScreen;
    public Slider progressBar;
    public TextMeshProUGUI progressText;
    public float minLoadTime = 1.5f;
    public float ToBlackSpeed = 1f;

    [Header("Scene Settings")]
    public string mainMenuScene = "MainMenu";
    public string firstLevelScene = "Level1";

    private AsyncOperation loadingOperation;
    private string targetScene;
    private bool InLoading = false;

    [Header("关卡管理-UI")]
    public GameObject[] MenueElement;
    public GameObject[] LevelOne;
    void Awake()
    {
        // 初始设置
        loadingScreen.color *= new Color(1,1,1,0);
    }
    private void Start()
    {
        AudioManager2025.Instance.PlaySound("Auralnauts - Andor (Main Title Theme) (1975 Version)");
    }
    // 加载场景方法
    public void LoadScene(string sceneName)
    {
        if (InLoading)
        {
            return;
        }
        InLoading = true;
        targetScene = sceneName;
        StartCoroutine(LoadSceneCoroutine());
    }

    // 加载下一个场景
    public void LoadNextScene()
    {
        int currentIndex = SceneManager.GetActiveScene().buildIndex;
        int nextIndex = currentIndex + 1;

        if (nextIndex < SceneManager.sceneCountInBuildSettings)
        {
            LoadSceneByIndex(nextIndex);
        }
        else
        {
            Debug.LogWarning("No next scene available. Loading main menu.");
            LoadMainMenu();
        }
    }

    // 按索引加载场景
    public void LoadSceneByIndex(int sceneIndex)
    {
        if (sceneIndex >= 0 && sceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            targetScene = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
            StartCoroutine(LoadSceneCoroutine());
        }
        else
        {
            Debug.LogError("Invalid scene index: " + sceneIndex);
        }
    }

    // 重新加载当前场景
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    // 加载主菜单
    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    // 加载第一个关卡
    public void LoadFirstLevel()
    {
        LoadScene(firstLevelScene);
    }

    // 异步加载协程
    private IEnumerator LoadSceneCoroutine()
    {
        loadingScreen.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(true);
        progressText.gameObject.SetActive(true);
        // 准备加载
        while (loadingScreen.color.a < 1)
        {
            loadingScreen.color += new Color(0, 0, 0, ToBlackSpeed*Time.deltaTime);
            yield return null;
        }
        progressBar.value = 0;
        progressText.text = "0%";


        // 开始异步加载
        loadingOperation = SceneManager.LoadSceneAsync(targetScene);
        loadingOperation.allowSceneActivation = false;

        float timer = 0f;

        // 加载循环
        while (!loadingOperation.isDone)
        {
            timer += Time.deltaTime;

            // 计算加载进度 (Unity加载到90%会暂停，直到allowSceneActivation=true)
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            float visualProgress = Mathf.Clamp01(timer / minLoadTime);
            float displayProgress = Mathf.Max(progress, visualProgress);

            // 更新UI
            progressBar.value = displayProgress;
            progressText.text = Mathf.RoundToInt(displayProgress * 100) + "%";

            // 加载完成
            if (progress >= 0.9f && timer >= minLoadTime)
            {
                progressBar.value = 1;
                progressText.text = "100%";

                // 添加短暂延迟让玩家看到100%
                yield return new WaitForSeconds(0.5f);

                // 激活场景
                loadingOperation.allowSceneActivation = true;
                foreach (var item in MenueElement)
                {
                    item.SetActive(false);
                }
                // 加载完成
                while (loadingScreen.color.a > 0)
                {
                    loadingScreen.color -= new Color(0, 0, 0, ToBlackSpeed * Time.deltaTime);
                    yield return null;
                }
                loadingScreen.gameObject.SetActive(false);
                //依次加载
                foreach (var item in LevelOne)
                {
                    item.SetActive(true);
                    yield return null;
                }
                InLoading = false;
            }

            yield return null;
        }


    }

    public void DontTouchMe()
    {
        DialogController.Instance.StartDialog(ShitTalkingInMenue[Random.Range(0,ShitTalkingInMenue.Length)]);
    }
}