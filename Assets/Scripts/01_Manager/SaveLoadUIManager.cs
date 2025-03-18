using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUIManager : MonoBehaviour
{
    public static SaveLoadUIManager Instance { get; private set; }

    [SerializeField] private Button[] saveSlotButtons; // ���� ��ư �迭
    [SerializeField] private TMP_Text[] saveSlotTexts; // ���� �ؽ�Ʈ �迭

    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;

    [SerializeField] private GameObject panel;

    private int selectedSlotIndex = -1;

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        LoadSaveSlotInfo();
        AssignSlotButtons();

        panel.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���� ��ư�� Ŭ�� �̺�Ʈ �Ҵ�
    /// </summary>
    private void AssignSlotButtons()
    {
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i;
            saveSlotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));
        }

        // ���� �� �ҷ����� ��ư �̺�Ʈ ���
        saveButton.onClick.AddListener(SaveSelectedSlot);
        loadButton.onClick.AddListener(LoadSelectedSlot);
    }

    /// <summary>
    /// Ư�� ������ �������� �� ����
    /// </summary>
    /// <param name="slotIndex"></param>
    private void SelectSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        Debug.Log($"���õ� ����: {selectedSlotIndex}");

        // UI ������Ʈ (���õ� ���� ���� ����)
        UpdateSlotUI();
    }

    /// <summary>
    /// ���õ� ���Կ� ���� ����
    /// </summary>
    private void SaveSelectedSlot()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.LogWarning("������ ������ ���õ��� �ʾҽ��ϴ�.");
            return;
        }

        SaveLoadManager.Instance.SaveGame(selectedSlotIndex);
        LoadSaveSlotInfo();
        OffPanel();
    }

    /// <summary>
    /// ���õ� ���Կ��� ���� �ҷ�����
    /// </summary>
    private void LoadSelectedSlot()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.LogWarning("�ҷ��� ������ ���õ��� �ʾҽ��ϴ�.");
            return;
        }

        SaveLoadManager.Instance.LoadGame(selectedSlotIndex);
        OffPanel();
    }

    /// <summary>
    /// ���� UI ���� ����
    /// </summary>
    private void LoadSaveSlotInfo()
    {
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            string saveFilePath = Path.Combine(Application.persistentDataPath, $"Saves/save_slot_{i}.json");

            if (File.Exists(saveFilePath))
            {
                string json = File.ReadAllText(saveFilePath);
                SaveData saveData = JsonUtility.FromJson<SaveData>(json);

                saveSlotTexts[i].text = $"�������� {saveData.currentStageIndex} | �� {saveData.turnCount} | ����: {saveData.saveTime}";
            }
            else
            {
                saveSlotTexts[i].text = "�� ����";
            }
        }
    }

    /// <summary>
    /// ���� ���õ� ������ UI���� ���� ǥ��
    /// </summary>
    private void UpdateSlotUI()
    {
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            ColorBlock colors = saveSlotButtons[i].colors;
            if (i == selectedSlotIndex)
            {
                // ���õ� ���� ����
                colors.normalColor = Color.yellow;
            }
            else
            {
                colors.normalColor = Color.white;
            }
            saveSlotButtons[i].colors = colors;
        }
    }

    public void OnPanel()
    {
        panel.gameObject.SetActive(true);
    }

    public void OffPanel()
    {
        panel.gameObject.SetActive(false);
    }
}
