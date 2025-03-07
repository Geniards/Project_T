using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();

    [SerializeField] private DialogueUI dialogueUI;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            dialogueUI.OnNextButtonClick();
        }
    }

    /// <summary>
    /// Json에서 대화 데이터 로드
    /// </summary>
    /// <param name="jsonFileName"></param>
    public void LoadDialogue(string jsonFileName)
    {
        TextAsset  jsonFile = Resources.Load<TextAsset>($"Dialogues/{jsonFileName}");
        if(!jsonFile)
        {
            Debug.LogError($"대화파일을 찾을 수 없습니다.{jsonFileName}");
            return;
        }

        DialogueData dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);
        if(dialogueData != null && dialogueData.dialogues.Count > 0)
        {
            dialogueQueue.Clear();
            foreach(var entry in dialogueData.dialogues)
            {
                dialogueQueue.Enqueue(entry);
            }
        }
    }

    /// <summary>
    /// 대화 시작
    /// </summary>
    public void StartDialogue()
    {
        if (dialogueQueue.Count > 0)
            ShowNextDialogue();
    }

    /// <summary>
    /// 다음 대화 출력
    /// </summary>
    public void ShowNextDialogue()
    {
        // 큐가 비었으면 대화 종료
        if (dialogueQueue.Count == 0)
        {
            dialogueUI.HideDialogueBox();
            return;
        }
        // 다음 대사를 꺼냄
        DialogueEntry entry = dialogueQueue.Dequeue();
        // 1) 대사 표시
        dialogueUI.UpdateDialogue(entry.name, entry.portrait, entry.text);

        // 2) 만약 유닛 애니메이션 정보를 가지고 있으면 실행
        if (entry.unitId != 0)
        {
            // 여러 유닛이 같은 unitId를 가질 수도 있으니,
            // 우선 GetUnitsByType().Find(...) 등을 써서 대상 유닛을 잡는다.
            Unit targetUnit = UnitManager.Instance.GetUnitsByType(entry.unitId).Find(x => true); // 조건 필요시 추가
            if (targetUnit != null)
            {
                // 코루틴으로 애니메이션(또는 연출)을 재생
                StartCoroutine(PlayStoryAnimation(targetUnit, entry.animationType, entry.repeatCount));
            }
        }
    }

    private IEnumerator PlayStoryAnimation(Unit unit, string animType, int repeatCount)
    {
        int count = 0;
        while (repeatCount == 0 || count < repeatCount)
        {
            switch (animType)
            {
                case "Attack":
                    unit.Attack(null);
                    yield return new WaitForSeconds(1f);
                    break;

                case "Move":
                    Tile randomTile = GridManager.Instance.GetTile(unit.currentTile.vec2IntPos + Vector2Int.right);
                    if (randomTile != null)
                    {
                        unit.MoveTo(randomTile);
                        yield return new WaitForSeconds(1.5f);
                    }
                    break;

                case "Hit":
                    unit.TakeDamage(0);
                    yield return new WaitForSeconds(1f);
                    break;
            }
            count++;
        }
    }
}
