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

    private bool isDialogueActive = false; // ��ȭ Ȱ��ȭ ����

    private string dialogueName;

    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Json���� ��ȭ ������ �ε�
    /// </summary>
    /// <param name="jsonFileName"></param>
    public void LoadDialogue(string jsonFileName)
    {
        dialogueName = jsonFileName;
        TextAsset  jsonFile = Resources.Load<TextAsset>($"Dialogues/{dialogueName}");
        if(!jsonFile)
        {
            Debug.LogError($"��ȭ������ ã�� �� �����ϴ�.{dialogueName}");
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
    /// ��ȭ ����
    /// </summary>
    public void StartDialogue()
    {
        if (dialogueQueue.Count > 0)
        {
            // �Է� �� UI ����
            InputManager.Instance.EnableDialogueActive();
            UIManager.Instance.HideActionMenu();
            isDialogueActive = true;

            // ù ��° ��ȭ ���� Ȯ�� �� ī�޶� �̵�
            DialogueEntry entry = dialogueQueue.Peek();
            if (entry.unitId != 0)
            {
                Unit targetUnit = UnitManager.Instance.GetUnitsByType(entry.unitId).Find(x => true);
                if (targetUnit)
                {
                    StartCoroutine(MoveCameraToUnit(targetUnit, () =>
                    {
                        Debug.Log("�̺�Ʈ ��ȭ ����!");
                        ShowNextDialogue();
                    }));
                    return; // ī�޶� �̵� �� ��ȭ ����
                }
            }

            // ���� ��ȭ ����
            ShowNextDialogue();
        }
    }

    /// <summary>
    /// ī�޶� Ư�� ���ֿ��� �̵� (���� �� �ݹ� ����)
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

        mainCamera.transform.position = targetPosition; // ���� ��ġ ����
        onComplete?.Invoke(); // �̵� �� ��ȭ ����
    }

    /// <summary>
    /// ���� ��ȭ ���
    /// </summary>
    public void ShowNextDialogue()
    {
        // ť�� ������� ��ȭ ����
        if (dialogueQueue.Count == 0)
        {
            isDialogueActive = false;
            dialogueUI.HideDialogueBox();
            InputManager.Instance.DisableDialogueActive();

            if(UIManager.Instance.selectedUnit)
                UIManager.Instance.ShowActionMenu();
            return;
        }
        // ���� ��縦 ����
        DialogueEntry entry = dialogueQueue.Dequeue();

        // 0) ��ȭ �� BGM ���� (�ʵ尡 ������ ����)
        if (!string.IsNullOrEmpty(entry.bgmChange))
        {
            AudioClip newBGM = Resources.Load<AudioClip>($"Audio/{entry.bgmChange}");
            if (newBGM != null)
            {
                SoundManager.Instance.PlayBGM(newBGM);
            }
        }

        // 1) ���� ���� �ִϸ��̼� ������ ������ ������ ����
        if (entry.unitId != 0)
        {
            Debug.Log("��ȭ �� ���� �̵�");
            // ���� ������ ���� unitId�� ���� ���� ������,
            // �켱 GetUnitsByType().Find(...) ���� �Ἥ ��� ������ ��´�.
            Unit targetUnit = UnitManager.Instance.GetUnitsByType(entry.unitId).Find(x => true); // ���� �ʿ�� �߰�
            if (targetUnit)
            {
                Debug.Log("targetUnit not null");
                // �ڷ�ƾ���� �ִϸ��̼�(�Ǵ� ����)�� ���
                StartCoroutine(PlayStoryAnimation(targetUnit, entry.animationType, entry.repeatCount, entry.targetX, entry.targetY));
            }
        }

        // 2) ��� ǥ��
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
                        Debug.Log("���� ���ǹ� ����");
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
