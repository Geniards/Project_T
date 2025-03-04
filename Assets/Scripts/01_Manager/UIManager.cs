using System.Collections.Generic;
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

    [SerializeField] private CanvasGroup canvasGroup;

    private Unit selectedUnit;
    // ���������� ���� UI�� ǥ���� ����
    private Unit lastHoveredUnit;

    // �޴�â�� ���� �ִ��� Ȯ���ϴ� ����
    private bool isActionMenuVisible = false;
    // ���� ��� ���ɻ���
    private bool isAttackMode = false;


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
        isActionMenuVisible = true;

        // �޴� Ȱ��ȭ �� UI ���� ��ġ ����
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        actionMenu.transform.position = unit.transform.position + new Vector3(1f, 0, -1f);
        // ���õ� ������ �������� ȣ���� �������� ���ٸ� StatusUI�� false��
        if (lastHoveredUnit == selectedUnit)
        {
            unitStatusUI.SetActive(false);
        }
    }

    /// <summary>
    /// �ൿ ���� �޴� �����
    /// </summary>
    public void HideActionMenu()
    {
        actionMenu.SetActive(false);
        isActionMenuVisible = false;

        // �޴� ���� �� UI ���� ��ġ ���
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public void AfterShowActionMenu()
    {
        actionMenu.SetActive(true);
        isActionMenuVisible = true;
    }

    private void OnAttackButtonClick()
    {
        if (selectedUnit == null) return;

        // 1. �޴� ����
        HideActionMenu();

        // 2. ���� ���� ���� ǥ��
        GridManager.Instance.FindAttackableTiles(selectedUnit);
        GridManager.Instance.ShowHighLight();

        isAttackMode = true;
    }

    private void OnWaitButtonClick()
    {
        selectedUnit.unitState = E_UnitState.Complete;
        HideActionMenu();
        selectedUnit = null;
        TurnManager.Instance.CheckPlayerTurnEnd();

    }

    private void OnCancelButtonClick()
    {
        selectedUnit.Deselect();
        HideActionMenu();
    }

    /// <summary>
    /// �޴�â�� ���� ���� �� Ÿ�� Ŭ�� ����
    /// </summary>
    /// <returns></returns>
    public bool IsActionMenuVisible()
    {
        return isActionMenuVisible;
    }

    /// <summary>
    /// ���� ���� UI ǥ��
    /// </summary>
    /// <param name="unit"></param>
    public void UnitStatusUI(Unit unit)
    {
        // �̹� ���� ������ ���¸� ���� ������ ���ʿ��� ���� ����
        if (lastHoveredUnit == unit) return;

        lastHoveredUnit = unit;

        // �ൿ ���� �޴��� ���� �����̶�� ���� UI�� ������ ����
        if (selectedUnit == unit)
        {
            unitStatusUI.SetActive(false);
            return;
        }

        if (!unit)
        {
            unitStatusUI.SetActive(false);
            return;
        }

        // UI ��ġ ������Ʈ
        unitStatusUI.SetActive(true);
        unitStatusUI.transform.position = unit.transform.position + new Vector3(1.5f,0f,-0.5f);

        // UI �ؽ�Ʈ ����
        hpTMP.text = $"{unit.unitData.hP.ToString()} / {unit.unitData.maxHP.ToString()}";
        manaTMP.text = $"{unit.unitData.mana.ToString()} / {unit.unitData.maxMana.ToString()}";
        expTMP.text = $"{unit.unitData.exp.ToString()} / {unit.unitData.maxExp.ToString()}";

        nameTMP.text = $"{unit.unitData.unitName}";
        levelTMP.text = $"LV.{unit.unitData.level}";
        classTMP.text = $"{unit.unitData.unitType}";

        // �����̴� �� ����
        if (hpSlider.value != (float)unit.unitData.hP / unit.unitData.maxHP)
            hpSlider.value = Mathf.Clamp((float)unit.unitData.hP / unit.unitData.maxHP, 0f, 1f);

        if (manaSlider.value != (float)unit.unitData.mana / unit.unitData.maxMana)
            manaSlider.value = Mathf.Clamp((float)unit.unitData.mana / unit.unitData.maxMana, 0f, 1f);

        if (expSlider.value != (float)unit.unitData.exp / unit.unitData.maxExp)
            expSlider.value = Mathf.Clamp((float)unit.unitData.exp / unit.unitData.maxExp, 0f, 1f);

    }

    public void CheckHp(Unit unit)
    {
        if (hpSlider)
        {
            hpSlider.value = Mathf.Clamp(unit.unitData.hP / unit.unitData.maxHP, 0f, 1f);
            hpTMP.text = $"{unit.unitData.hP.ToString()} / {unit.unitData.maxHP.ToString()}";
        }
    }

    public void CheckMp(Unit unit)
    {
        if (manaSlider)
            manaSlider.value = Mathf.Clamp(unit.unitData.mana / unit.unitData.maxMana, 0f, 1f);
    }

    public void CheckExp(Unit unit)
    {
        if (expSlider)
            expSlider.value = Mathf.Clamp(unit.unitData.exp / unit.unitData.maxExp, 0f, 1f);
    }
}
