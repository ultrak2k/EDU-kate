using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void GoToEverythingTest()
    {
        SceneManager.LoadScene("EverythingTest");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR  //this is cool
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}