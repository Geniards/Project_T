using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string dialogueFileName;

    [SerializeField] private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Ãæµ¹");
        Debug.Log(collision.tag);
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            DialogueManager.Instance.LoadDialogue(dialogueFileName);
            DialogueManager.Instance.StartDialogue();
        }
    }
}
