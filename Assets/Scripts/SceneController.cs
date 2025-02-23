using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneController : MonoBehaviour
{
    public static SceneController instance { get; private set; }

    public enum SceneType
    {
        Menu = 0,
        Game = 1
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoadScene(SceneType sceneType)
    {
        StartCoroutine(LoadSceneAsync(sceneType));
    }

    private IEnumerator LoadSceneAsync(SceneType sceneType)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync((int)sceneType);
        
        while (!asyncLoad.isDone)
        {
            // TODO: Update loading progress UI if needed
            yield return null;
        }
    }
}