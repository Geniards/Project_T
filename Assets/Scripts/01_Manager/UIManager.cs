using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // 행동 선택 UI
    public GameObject actionMenu;
    public Button attackButton; // 공격 버튼
    public Button waitButton;   // 대기 버튼
    public Button cancelButton; // 취소 버튼

    // UI 유닛 상태 프리팹
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
    // 마지막으로 상태 UI를 표시한 유닛
    private Unit lastHoveredUnit;

    // 메뉴창이 열려 있는지 확인하는 변수
    private bool isActionMenuVisible = false;
    // 공격 취소 가능상태
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
    /// 행동 선택 메뉴 표시
    /// </summary>
    /// <param name="unit"></param>
    public void ShowActionMenu(Unit unit)
    {
        selectedUnit = unit;
        actionMenu.SetActive(true);
        isActionMenuVisible = true;

        // 메뉴 활성화 시 UI 뒤쪽 터치 방지
        canvasGroup.blocksRaycasts = true;
        canvasGroup.interactable = true;

        actionMenu.transform.position = unit.transform.position + new Vector3(1f, 0, -1f);
        // 선택된 유닛을 마지막에 호버된 유닛으과 같다면 StatusUI를 false로
        if (lastHoveredUnit == selectedUnit)
        {
            unitStatusUI.SetActive(false);
        }
    }

    /// <summary>
    /// 행동 선택 메뉴 숨기기
    /// </summary>
    public void HideActionMenu()
    {
        actionMenu.SetActive(false);
        isActionMenuVisible = false;

        // 메뉴 숨김 시 UI 뒤쪽 터치 허용
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

        // 1. 메뉴 숨김
        HideActionMenu();

        // 2. 공격 가능 범위 표시
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
    /// 메뉴창이 열려 있을 때 타일 클릭 방지
    /// </summary>
    /// <returns></returns>
    public bool IsActionMenuVisible()
    {
        return isActionMenuVisible;
    }

    /// <summary>
    /// 유닛 상태 UI 표시
    /// </summary>
    /// <param name="unit"></param>
    public void UnitStatusUI(Unit unit)
    {
        // 이미 같은 유닛의 상태를 띄우고 있으면 불필요한 갱신 방지
        if (lastHoveredUnit == unit) return;

        lastHoveredUnit = unit;

        // 행동 선택 메뉴가 열린 유닛이라면 상태 UI를 보이지 않음
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

        // UI 위치 업데이트
        unitStatusUI.SetActive(true);
        unitStatusUI.transform.position = unit.transform.position + new Vector3(1.5f,0f,-0.5f);

        // UI 텍스트 갱신
        hpTMP.text = $"{unit.unitData.hP.ToString()} / {unit.unitData.maxHP.ToString()}";
        manaTMP.text = $"{unit.unitData.mana.ToString()} / {unit.unitData.maxMana.ToString()}";
        expTMP.text = $"{unit.unitData.exp.ToString()} / {unit.unitData.maxExp.ToString()}";

        nameTMP.text = $"{unit.unitData.unitName}";
        levelTMP.text = $"LV.{unit.unitData.level}";
        classTMP.text = $"{unit.unitData.unitType}";

        // 슬라이더 값 갱신
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
