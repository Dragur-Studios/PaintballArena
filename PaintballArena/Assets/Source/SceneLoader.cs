using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public GameObject loadingScreen;

    public CanvasGroup canvasGroup;

    static SceneLoader instance;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
            return;
        }    
        instance = this;
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
    public static void LoadScene(string levelName)
    {
        instance.Load(levelName);
    }

    void Load(string levelName)
    {
        StartCoroutine(StartLoad(levelName));
    }

    IEnumerator StartLoad(string levelName)
    {
        loadingScreen.SetActive(true);
        yield return StartCoroutine(FadeLoadingScreen(1, 1));
        AsyncOperation operation = SceneManager.LoadSceneAsync(levelName);
        while (!operation.isDone)
        {
            var slider = loadingScreen.GetComponent<Slider>();
            slider.value = operation.progress * 0.9f;

            yield return null;
        }
        yield return StartCoroutine(FadeLoadingScreen(0, 1));
        loadingScreen.SetActive(false);
    }
    IEnumerator FadeLoadingScreen(float targetValue, float duration)
    {
        float startValue = canvasGroup.alpha;
        float time = 0;
        while (time < duration)
        {
            canvasGroup.alpha = Mathf.Lerp(startValue, targetValue, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        canvasGroup.alpha = targetValue;
    }
}
