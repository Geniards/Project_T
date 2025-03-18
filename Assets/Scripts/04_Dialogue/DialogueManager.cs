using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    private Queue<DialogueEntry> dialogueQueue = new Queue<DialogueEntry>();

    [SerializeField] private DialogueUI dialogueUI;
    [SerializeField] private float cameraMoveSpeed = 3f;

    private bool isDialogueActive = false; // 대화 활성화 상태

    private string dialogueName;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Json에서 대화 데이터 로드
    /// </summary>
    /// <param name="jsonFileName"></param>
    public void LoadDialogue(string jsonFileName)
    {
        dialogueName = jsonFileName;
        TextAsset  jsonFile = Resources.Load<TextAsset>($"Dialogues/{dialogueName}");
        if(!jsonFile)
        {
            Debug.LogError($"대화파일을 찾을 수 없습니다.{dialogueName}");
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
        {
            // 입력 및 UI 숨김
            InputManager.Instance.EnableDialogueActive();
            UIManager.Instance.HideActionMenu();
            isDialogueActive = true;

            // 첫 번째 대화 유닛 확인 후 카메라 이동
            DialogueEntry entry = dialogueQueue.Peek();
            if (entry.unitId != 0)
            {
                Unit targetUnit = UnitManager.Instance.GetUnitsByType(entry.unitId).Find(x => true);
                if (targetUnit)
                {
                    StartCoroutine(MoveCameraToUnit(targetUnit, () =>
                    {
                        Debug.Log("이벤트 대화 동작!");
                        ShowNextDialogue();
                    }));
                    return; // 카메라 이동 후 대화 진행
                }
            }

            // 다음 대화 시작
            ShowNextDialogue();
        }
    }

    /// <summary>
    /// 카메라를 특정 유닛에게 이동 (연출 후 콜백 실행)
    /// </summary>
    private IEnumerator MoveCameraToUnit(Unit unit, Action onComplete)
    {
        Camera mainCamera = Camera.main;
        Vector3 targetPosition = new Vector3(unit.transform.position.x, unit.transform.position.y, mainCamera.transform.position.z);
        Vector3 startPosition = mainCamera.transform.position;

        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            elapsedTime += Time.deltaTime * cameraMoveSpeed;
            mainCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime);
            yield return null;
        }

        mainCamera.transform.position = targetPosition; // 최종 위치 고정
        onComplete?.Invoke(); // 이동 후 대화 시작
    }

    /// <summary>
    /// 다음 대화 출력
    /// </summary>
    public void ShowNextDialogue()
    {
        // 큐가 비었으면 대화 종료
        if (dialogueQueue.Count == 0)
        {
            isDialogueActive = false;
            dialogueUI.HideDialogueBox();
            InputManager.Instance.DisableDialogueActive();

            if(UIManager.Instance.selectedUnit)
                UIManager.Instance.ShowActionMenu();
            return;
        }
        // 다음 대사를 꺼냄
        DialogueEntry entry = dialogueQueue.Dequeue();

        // 0) 대화 중 BGM 변경 (필드가 있으면 변경)
        if (!string.IsNullOrEmpty(entry.bgmChange))
        {
            AudioClip newBGM = Resources.Load<AudioClip>($"Audio/{entry.bgmChange}");
            if (newBGM != null)
            {
                SoundManager.Instance.PlayBGM(newBGM);
            }
        }

        // 1) 만약 유닛 애니메이션 정보를 가지고 있으면 실행
        if (entry.unitId != 0)
        {
            Debug.Log("대화 중 유닛 이동");
            // 여러 유닛이 같은 unitId를 가질 수도 있으니,
            // 우선 GetUnitsByType().Find(...) 등을 써서 대상 유닛을 잡는다.
            Unit targetUnit = UnitManager.Instance.GetUnitsByType(entry.unitId).Find(x => true); // 조건 필요시 추가
            if (targetUnit)
            {
                Debug.Log("targetUnit not null");
                // 코루틴으로 애니메이션(또는 연출)을 재생
                StartCoroutine(PlayStoryAnimation(targetUnit, entry.animationType, entry.repeatCount, entry.targetX, entry.targetY));
            }
        }

        // 2) 대사 표시
        dialogueUI.UpdateDialogue(entry.name, entry.portrait, entry.BG, entry.text);
    }

    private IEnumerator PlayStoryAnimation(Unit unit, string animType, int repeatCount, int targetX, int targetY)
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
                    Tile targetTile = GridManager.Instance.GetTile(new Vector2Int(targetX, targetY));
                    Tile initTile = unit.currentTile;
                    if (targetTile)
                    {
                        Debug.Log("유닛 조건문 동작");
                        yield return StartCoroutine(unit.MoveToCoroutine(targetTile));
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

    public bool IsDialogueActive()
    {
        return isDialogueActive;
    }

    public void EnableIsDialogueActive()
    {
        isDialogueActive = true;
    }

    public void DisableIsDialogueActive()
    {
        isDialogueActive = false;
    }
}
