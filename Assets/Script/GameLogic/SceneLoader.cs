using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class SceneLoader : MonoBehaviour
{

    //�²۾���
    public DialogData[] ShitTalkingInMenue;

    [Header("�ؿ������Ӿ�")]
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

    [Header("�ؿ�����-UI")]
    public GameObject[] MenueElement;
    public GameObject[] LevelOne;
    void Awake()
    {
        // ��ʼ����
        loadingScreen.color *= new Color(1,1,1,0);
    }
    private void Start()
    {
        AudioManager2025.Instance.PlaySound("Auralnauts - Andor (Main Title Theme) (1975 Version)");
    }
    // ���س�������
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

    // ������һ������
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

    // ���������س���
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

    // ���¼��ص�ǰ����
    public void ReloadCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    // �������˵�
    public void LoadMainMenu()
    {
        LoadScene(mainMenuScene);
    }

    // ���ص�һ���ؿ�
    public void LoadFirstLevel()
    {
        LoadScene(firstLevelScene);
    }

    // �첽����Э��
    private IEnumerator LoadSceneCoroutine()
    {
        loadingScreen.gameObject.SetActive(true);
        progressBar.gameObject.SetActive(true);
        progressText.gameObject.SetActive(true);
        // ׼������
        while (loadingScreen.color.a < 1)
        {
            loadingScreen.color += new Color(0, 0, 0, ToBlackSpeed*Time.deltaTime);
            yield return null;
        }
        progressBar.value = 0;
        progressText.text = "0%";


        // ��ʼ�첽����
        loadingOperation = SceneManager.LoadSceneAsync(targetScene);
        loadingOperation.allowSceneActivation = false;

        float timer = 0f;

        // ����ѭ��
        while (!loadingOperation.isDone)
        {
            timer += Time.deltaTime;

            // ������ؽ��� (Unity���ص�90%����ͣ��ֱ��allowSceneActivation=true)
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);
            float visualProgress = Mathf.Clamp01(timer / minLoadTime);
            float displayProgress = Mathf.Max(progress, visualProgress);

            // ����UI
            progressBar.value = displayProgress;
            progressText.text = Mathf.RoundToInt(displayProgress * 100) + "%";

            // �������
            if (progress >= 0.9f && timer >= minLoadTime)
            {
                progressBar.value = 1;
                progressText.text = "100%";

                // ��Ӷ����ӳ�����ҿ���100%
                yield return new WaitForSeconds(0.5f);

                // �����
                loadingOperation.allowSceneActivation = true;
                foreach (var item in MenueElement)
                {
                    item.SetActive(false);
                }
                // �������
                while (loadingScreen.color.a > 0)
                {
                    loadingScreen.color -= new Color(0, 0, 0, ToBlackSpeed * Time.deltaTime);
                    yield return null;
                }
                loadingScreen.gameObject.SetActive(false);
                //���μ���
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