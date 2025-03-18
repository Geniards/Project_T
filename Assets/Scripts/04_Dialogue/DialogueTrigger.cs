using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private string dialogueFileName;
    private bool hasTriggered = false;
    private Unit triggeringUnit; // 트리거를 활성화한 유닛 저장

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasTriggered && collision.CompareTag("Player"))
        {
            hasTriggered = true;
            triggeringUnit = collision.GetComponent<Unit>();

            if (triggeringUnit != null)
            {
                // 유닛 이동이 끝나면 `OnUnitMoveComplete()` 호출
                triggeringUnit.OnMoveComplete += OnUnitMoveComplete;
            }
        }
    }

    /// <summary>
    /// 유닛이 이동을 마친 후 대화 시작
    /// </summary>
    private void OnUnitMoveComplete()
    {
        if (triggeringUnit != null)
        {
            // 이벤트 연결 해제 (한 번만 실행되도록)
            triggeringUnit.OnMoveComplete -= OnUnitMoveComplete;

            // 대화 실행
            DialogueManager.Instance.LoadDialogue(dialogueFileName);
            DialogueManager.Instance.StartDialogue();
        }
    }
}
