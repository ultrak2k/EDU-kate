using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [Header("Dialogue Event")]
    public DialogueManager DialogueEvent;

    private bool hasTriggered = false;

    public void PlayDialogue(DialogueManager dialogueManager)
    {
        if (dialogueManager == null)
        {
            Debug.LogWarning("No DialogueManager assigned.");
            return;
        }

        dialogueManager.gameObject.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasTriggered) return;

        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            hasTriggered = true;

            PlayerController playerController = other.GetComponentInParent<PlayerController>();
            if (playerController != null)
            {

                PlayDialogue(DialogueEvent);
            }
            else
            {
                Debug.LogWarning("PlayerController not found on Player object.");
            }

            
        }
    }
}