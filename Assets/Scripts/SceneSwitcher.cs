using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public string SceneToLoad = "EverythingTest";
    public void GoToEverythingTest()
    {
        SceneManager.LoadScene(SceneToLoad);
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("PeePee");//ts dun work even tho its just the dialogue trigger script
        // Check if the object (or its parent) has the PlayerController script
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {

            PlayerController playerController = other.GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                SceneManager.LoadScene(SceneToLoad);
            }
            else
            {
                Debug.LogWarning("PlayerController not found on Player object.");
            }
        }
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