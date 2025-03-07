using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string dialogueFileName;
    private bool hasTriggered = false;
    private Unit triggeringUnit; // Ʈ���Ÿ� Ȱ��ȭ�� ���� ����

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            triggeringUnit = collision.GetComponent<Unit>();

            if (triggeringUnit != null)
            {
                // ���� �̵��� ������ `OnUnitMoveComplete()` ȣ��
                triggeringUnit.OnMoveComplete += OnUnitMoveComplete;
            }
        }
    }

    /// <summary>
    /// ������ �̵��� ��ģ �� ��ȭ ����
    /// </summary>
    private void OnUnitMoveComplete()
    {
        if (triggeringUnit != null)
        {
            // �̺�Ʈ ���� ���� (�� ���� ����ǵ���)
            triggeringUnit.OnMoveComplete -= OnUnitMoveComplete;

            // ��ȭ ����
            DialogueManager.Instance.LoadDialogue(dialogueFileName);
            DialogueManager.Instance.StartDialogue();
        }
    }
}
