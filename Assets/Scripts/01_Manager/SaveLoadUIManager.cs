using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveLoadUIManager : MonoBehaviour
{
    public static SaveLoadUIManager Instance { get; private set; }

    [SerializeField] private Button[] saveSlotButtons; // 슬롯 버튼 배열
    [SerializeField] private TMP_Text[] saveSlotTexts; // 슬롯 텍스트 배열

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
    /// 슬롯 버튼에 클릭 이벤트 할당
    /// </summary>
    private void AssignSlotButtons()
    {
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            int slotIndex = i;
            saveSlotButtons[i].onClick.AddListener(() => SelectSlot(slotIndex));
        }

        // 저장 및 불러오기 버튼 이벤트 등록
        saveButton.onClick.AddListener(SaveSelectedSlot);
        loadButton.onClick.AddListener(LoadSelectedSlot);
    }

    /// <summary>
    /// 특정 슬롯을 선택했을 때 실행
    /// </summary>
    /// <param name="slotIndex"></param>
    private void SelectSlot(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        Debug.Log($"선택된 슬롯: {selectedSlotIndex}");

        // UI 업데이트 (선택된 슬롯 강조 가능)
        UpdateSlotUI();
    }

    /// <summary>
    /// 선택된 슬롯에 게임 저장
    /// </summary>
    private void SaveSelectedSlot()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.LogWarning("저장할 슬롯이 선택되지 않았습니다.");
            return;
        }

        SaveLoadManager.Instance.SaveGame(selectedSlotIndex);
        LoadSaveSlotInfo();
        OffPanel();
    }

    /// <summary>
    /// 선택된 슬롯에서 게임 불러오기
    /// </summary>
    private void LoadSelectedSlot()
    {
        if (selectedSlotIndex == -1)
        {
            Debug.LogWarning("불러올 슬롯이 선택되지 않았습니다.");
            return;
        }

        SaveLoadManager.Instance.LoadGame(selectedSlotIndex);
        OffPanel();
    }

    /// <summary>
    /// 슬롯 UI 정보 갱신
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

                saveSlotTexts[i].text = $"스테이지 {saveData.currentStageIndex} | 턴 {saveData.turnCount} | 저장: {saveData.saveTime}";
            }
            else
            {
                saveSlotTexts[i].text = "빈 슬롯";
            }
        }
    }

    /// <summary>
    /// 현재 선택된 슬롯을 UI에서 강조 표시
    /// </summary>
    private void UpdateSlotUI()
    {
        for (int i = 0; i < saveSlotButtons.Length; i++)
        {
            ColorBlock colors = saveSlotButtons[i].colors;
            if (i == selectedSlotIndex)
            {
                // 선택된 슬롯 강조
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
