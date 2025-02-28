using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // �ൿ ���� UI
    public GameObject actionMenu;
    public Button attackButton; // ���� ��ư
    public Button waitButton;   // ��� ��ư
    public Button cancelButton; // ��� ��ư

    // UI ���� ���� ������
    [SerializeField] private GameObject unitStatusUI;
    [SerializeField] private TMP_Text hpTMP;
    [SerializeField] private TMP_Text manaTMP;
    [SerializeField] private TMP_Text expTMP;
    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text levelTMP;
    [SerializeField] private TMP_Text classTMP;

    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Slider expSlider;

    private Unit selectedUnit;

    private void Awake()
    {
        if (Instance == null)
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
        attackButton.onClick.AddListener(OnAttackButtonClick);
        waitButton.onClick.AddListener(OnWaitButtonClick);
        cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    /// <summary>
    /// �ൿ ���� �޴� ǥ��
    /// </summary>
    /// <param name="unit"></param>
    public void ShowActionMenu(Unit unit)
    {
        selectedUnit = unit;
        actionMenu.SetActive(true);

        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(selectedUnit.currentTile.vec2IntPos.x), Mathf.RoundToInt(selectedUnit.currentTile.vec2IntPos.y));
        Tile selectTile = GridManager.Instance.GetTile(gridPos);

        actionMenu.transform.position = new Vector3(gridPos.x + 1.0f, gridPos.y, -1f);
    }

    /// <summary>
    /// �ൿ ���� �޴� �����
    /// </summary>
    public void HideActionMenu()
    {
        actionMenu.SetActive(false);
    }

    private void OnAttackButtonClick()
    {
        GridManager.Instance.FindAttackableTiles(selectedUnit);
        GridManager.Instance.ShowHighLight();
    }

    private void OnWaitButtonClick()
    {
        selectedUnit.unitState = E_UnitState.Complete;
        HideActionMenu();
        TurnManager.Instance.CheckPlayerTurnEnd();
    }

    private void OnCancelButtonClick()
    {
        selectedUnit.Deselect();
        HideActionMenu();
    }

    public void UnitStatusUI(Unit unit)
    {
        if (!unit)
        {
            unitStatusUI.transform.position = Vector3.zero;

            //Debug.LogWarning("���õ� ������ �����ϴ�."); 
            return;
        }

        unitStatusUI.transform.position = unit.transform.position + new Vector3(1.5f,0f,-0.5f);

        hpTMP.text = $"{unit.unitData.hP.ToString()} / {unit.unitData.maxHP.ToString()}";
        manaTMP.text = $"{unit.unitData.mana.ToString()} / {unit.unitData.maxMana.ToString()}";
        expTMP.text = $"{unit.unitData.exp.ToString()} / {unit.unitData.maxExp.ToString()}";

        nameTMP.text = $"{unit.unitData.unitName}";
        levelTMP.text = $"LV.{unit.unitData.level}";
        classTMP.text = $"{unit.unitData.unitType}";

        hpSlider.value = unit.unitData.hP / unit.unitData.maxHP;
        manaSlider.value = unit.unitData.mana / unit.unitData.maxMana;
        expSlider.value = unit.unitData.exp / unit.unitData.maxExp;
    }

    public void CheckHp(Unit unit)
    {
        if(hpSlider)
            hpSlider.value = unit.unitData.hP / unit.unitData.maxHP;
    }

    public void CheckMp(Unit unit)
    {
        if (manaSlider)
            manaSlider.value = unit.unitData.mana / unit.unitData.maxMana;
    }

    public void CheckExp(Unit unit)
    {
        if (expSlider)
            expSlider.value = unit.unitData.exp / unit.unitData.maxExp;
    }
}
