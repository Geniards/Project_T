using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // �ൿ ���� UI
    [Header("Action �޴� UI")]
    [SerializeField] private GameObject actionMenu;
    [SerializeField] private Button attackButton; // ���� ��ư
    [SerializeField] private Button waitButton;   // ��� ��ư
    [SerializeField] private Button cancelButton; // ��� ��ư

    [Header("���� ���� UI")]
    [SerializeField] private GameObject unitStatusUI;
    [SerializeField] private TMP_Text hpTMP;
    [SerializeField] private TMP_Text manaTMP;
    [SerializeField] private TMP_Text expTMP;
    [SerializeField] private TMP_Text nameTMP;
    [SerializeField] private TMP_Text levelTMP;
    [SerializeField] private TMP_Text classTMP;

    [Header("���� ü�¹� UI")]
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Slider manaSlider;
    [SerializeField] private Slider expSlider;

    [SerializeField] private GameObject turnUI;  // �� UI
    [SerializeField] private TMP_Text turnText;  // �� �ؽ�Ʈ

    [Header("UI Canvas")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("UI ��ġ")]
    [SerializeField] private Vector3 uiPos;

    [Header("���� ��ǥ UI")]
    [SerializeField] private GameObject gameObjectivesUI;
    [SerializeField] private TMP_Text gameObjectText;

    // ���������� ���� UI�� ǥ���� ����
    private Unit lastHoveredUnit;

    // �޴�â�� ���� �ִ��� Ȯ���ϴ� ����
    private bool isActionMenuVisible = false;
    public Unit selectedUnit { get; set; }
    // ���� ��� ���ɻ���
    public bool isAttackMode { get; set; }

    private bool isTurnUIActive = false;

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

        // UI OFF
        actionMenu.SetActive(false);
        unitStatusUI.SetActive(false);
        gameObjectivesUI.SetActive(false);
        turnUI.SetActive(false);
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
    public void ShowActionMenu()
    {
        if (!selectedUnit) return;

        actionMenu.SetActive(true);
        isActionMenuVisible = true;

        // �޴� Ȱ��ȭ �� UI ���� ��ġ ����
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;
        
        uiPos = selectedUnit.transform.position + new Vector3(1f, 0, -1f);
        actionMenu.transform.position = uiPos;
    
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

        if (selectedUnit.unitState == E_UnitState.Move)
        {
            HideActionMenu();
            // 2. ���� ���� ���� ǥ��
            GridManager.Instance.FindAttackableTiles(selectedUnit);
            GridManager.Instance.ShowHighLight();
            isAttackMode = true;
        }
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

    public void CancelAttackMode()
    {
        if (!isAttackMode) return;

        isAttackMode = false;
        GridManager.Instance.ClearAttackableTiles();
        ShowActionMenu();
    }

    public bool IsAttackMode()
    {
        return isAttackMode;
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

    /// <summary>
    /// �� ���� UIǥ��
    /// </summary>
    /// <param name="isPlayerTurn"></param>
    /// <param name="turnNumber"></param>
    /// <returns></returns>
    public IEnumerator ShowTurnUI(bool isPlayerTurn, int turnNumber)
    {
        isTurnUIActive = true;
        turnUI.SetActive(true);

        if (isPlayerTurn)
        {
            turnText.text = $"�Ʊ� {turnNumber}��";
        }
        else
        {
            turnText.text = $"���� {turnNumber}��";
        }

        yield return new WaitForSeconds(2f);

        turnUI.SetActive(false);
        isTurnUIActive = false;
    }

    public bool IsTurnUIActive()
    {
        return isTurnUIActive;
    }

    /// <summary>
    /// ���� ��ǥ UI ǥ��
    /// </summary>
    public void ShowGameObjectives()
    {
        gameObjectivesUI.SetActive(true);
    }

    /// <summary>
    /// ���� ��ǥ UI �����
    /// </summary>
    public void HideGameObjectives()
    {
        gameObjectivesUI.SetActive(false);
    }

    public void OnGameStartButtonClick()
    {
        HideGameObjectives(); // ���� ��ǥ UI �ݱ�
        GameManager.Instance.StartGame(); // ���� ����
    }
}
