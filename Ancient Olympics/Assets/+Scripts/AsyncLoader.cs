using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AsyncLoader : MonoBehaviour
{
    [SerializeField] Slider slider;

    void Start()
    {
        StartCoroutine(LoadSceneAsync("Scene_MainMenu"));
    }

    IEnumerator LoadSceneAsync(string scene)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(scene);

        while (!loadOperation.isDone) 
        {
            float progress = Mathf.Clamp01(loadOperation.progress / 0.9f);
            slider.value = progress;

            yield return null;
        }
    }
}
