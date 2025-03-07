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
    /// Json���� ��ȭ ������ �ε�
    /// </summary>
    /// <param name="jsonFileName"></param>
    public void LoadDialogue(string jsonFileName)
    {
        TextAsset  jsonFile = Resources.Load<TextAsset>($"Dialogues/{jsonFileName}");
        if(!jsonFile)
        {
            Debug.LogError($"��ȭ������ ã�� �� �����ϴ�.{jsonFileName}");
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
            ShowNextDialogue();
    }

    /// <summary>
    /// ���� ��ȭ ���
    /// </summary>
    public void ShowNextDialogue()
    {
        // ť�� ������� ��ȭ ����
        if (dialogueQueue.Count == 0)
        {
            dialogueUI.HideDialogueBox();
            return;
        }
        // ���� ��縦 ����
        DialogueEntry entry = dialogueQueue.Dequeue();
        // 1) ��� ǥ��
        dialogueUI.UpdateDialogue(entry.name, entry.portrait, entry.text);

        // 2) ���� ���� �ִϸ��̼� ������ ������ ������ ����
        if (entry.unitId != 0)
        {
            // ���� ������ ���� unitId�� ���� ���� ������,
            // �켱 GetUnitsByType().Find(...) ���� �Ἥ ��� ������ ��´�.
            Unit targetUnit = UnitManager.Instance.GetUnitsByType(entry.unitId).Find(x => true); // ���� �ʿ�� �߰�
            if (targetUnit != null)
            {
                // �ڷ�ƾ���� �ִϸ��̼�(�Ǵ� ����)�� ���
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
