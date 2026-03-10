using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Event")]
    public DialogueManager DialogueEvent;
    [SerializeField] private GameObject _levelToSpawn;

    public bool FakeLevelShift;
    public string SceneToLoad = "SecondLevel";

    private bool hasTriggered = false;

    public void PlayDialogue(DialogueManager dialogueManager)
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("No DialogueManager assigned.");
            return;
        }

        dialogueManager.gameObject.SetActive(true);
        dialogueManager.SetLevelToSpawn(_levelToSpawn);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;

            PlayerController playerController = other.GetComponentInParent<PlayerController>();
            if (playerController != null)
            {
                if(FakeLevelShift)
                {
                    SceneManager.LoadScene(SceneToLoad);
                }
                else 
                {
                    PlayDialogue(DialogueEvent);
                }
            }
            else
            {
                Debug.LogWarning("PlayerController not found on Player object.");
            }

            
        }
    }
}