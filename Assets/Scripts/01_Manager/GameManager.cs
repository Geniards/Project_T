using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Pool 프리팹")]
    public GameObject tilePrefab;
    public GameObject unitPrefab;

    [Header("초기 풀 크기")]
    [SerializeField] private int InitialTileCount = 1024;
    [SerializeField] private int InitialUnitCount = 30;

    private PoolingManager<Tile> tilePool;
    private PoolingManager<Unit> unitPool;

    [Header("JSON Data")]
    public TextAsset stageJsonFile;
    public StageData currentStageData;

    [Header("선택된 유닛")]
    private Unit selectedUnit;
    private Tile selectedUnitTile;

    public bool isAutoBattle = false; // 아군 자동 전투 여부
    private void Awake()
    {
        if(!Instance)
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
        InitializePooling();
        LoadStage();
        TurnManager.Instance.StartTurn();
    }

    private void Update()
    {
        ResultGame();

        // 자동 전투 토글
        if (Input.GetKeyDown(KeyCode.T))
        {
            isAutoBattle = !isAutoBattle;
            Debug.Log($"자동 전투 모드: {(isAutoBattle ? "ON" : "OFF")}");
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log($"{selectedUnit.unitData.unitName}아군 턴 완료");
            selectedUnit.unitState = E_UnitState.Complete;
            selectedUnit = null;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            GridManager.Instance.FindAttackableTiles(selectedUnit);
            GridManager.Instance.ShowHighLight();
        }
    }

    /// <summary>
    /// 오브젝트 풀링 초기화
    /// </summary>
    private void InitializePooling()
    {
        tilePool = new PoolingManager<Tile>(tilePrefab, InitialTileCount, transform);
        unitPool = new PoolingManager<Unit>(unitPrefab, InitialUnitCount, transform);
    }

    /// <summary>
    /// 타일 오브젝트 풀에서 가져오기
    /// </summary>
    public Tile GetTile()
    {
        return tilePool.GetObject();
    }

    /// <summary>
    /// 사용이 끝난 타일을 반환
    /// </summary>
    /// <param name="tile"></param>
    public void ReturnTile(Tile tile)
    {
        tilePool.ReturnObject(tile);
    }

    /// <summary>
    /// 유닛 오브젝트 풀에서 가져오기
    /// </summary>
    public Unit GetUnit()
    {
        return unitPool.GetObject();
    }

    /// <summary>
    /// 사용이 끝난 유닛을 반환
    /// </summary>
    /// <param name="unit"></param>
    public void ReturnUnit(Unit unit)
    {
        unitPool.ReturnObject(unit);
    }
    public void LoadStage()
    {
        // 기존 데이터 초기화
        ResetStage();

        // 스테이지 데이터 로드
        currentStageData = JsonUtility.FromJson<StageData>(stageJsonFile.text);

        // 타일 정보 Dictionary 변환
        currentStageData.ConvertTileLegend();

        // 그리드 및 유닛 배치
        GridManager.Instance.LoadGrid(currentStageData);
        UnitManager.Instance.LoadUnit(currentStageData);
    }

    /// <summary>
    /// 스테이지 리셋
    /// </summary>
    public void ResetStage()
    {
        GridManager.Instance.ResetGrid();
        UnitManager.Instance.ResetUnits();
    }

    /// <summary>
    /// 마우스 클릭시 처리
    /// </summary>
    /// <param name="worldPos"></param>
    public void HandleClick(Vector2 worldPos)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        Tile clickedTile = GridManager.Instance.GetTile(gridPos);

        if (clickedTile)
        {
            Unit clickedUnit = UnitManager.Instance.GetUnitAtPosition(clickedTile.vec2IntPos);

            // 이미 행동을 완료한 유닛이라면 클릭 무시
            if (selectedUnit && selectedUnit.unitState == E_UnitState.Complete)
            {
                Debug.Log("이미 턴을 종료한 유닛입니다.");
                return;
            }

            // 제자리 이동
            if (selectedUnit && clickedTile == selectedUnitTile)
            {
                selectedUnit.Deselect();
                GridManager.Instance.FindAttackableTiles(selectedUnit);
                selectedUnit.unitState = E_UnitState.Move;


                // 공격범위를 찾기만하고 다음에 버튼의 UI를 눌러서 진행 할수 있도록 한다.
                // 이때 제자리 이동 후 공격을 안하고 턴을 종료할시 
                // selectedUnit = null; 선택된 유닛을 제거해야한다.
                return;
            }
            // 이동 가능한 타일인 경우 이동
            else if (selectedUnit && GridManager.Instance.IsWalkableTile(clickedTile))
            {
                selectedUnit.MoveTo(clickedTile);
                return;
            }
            // 이동 불가능한 타일인 경우 이동 불가 재탐색
            else if (selectedUnit && clickedTile != selectedUnitTile && selectedUnit.unitState != E_UnitState.Move)
            {
                selectedUnit.Deselect();
                selectedUnit = null;
            }

            // 선택된 유닛이 적을 공격 가능한 경우
            if (selectedUnit && clickedUnit && selectedUnit.unitData.unitTeam != clickedUnit.unitData.unitTeam)
            {
                StartCoroutine(selectedUnit.Attack(clickedUnit));
            }

            // 선택된 유닛이 없는 경우(유닛 선택 부분)
            if (clickedUnit && clickedUnit.unitData.unitTeam == E_UnitTeam.Ally && clickedUnit.unitState == E_UnitState.Idle)
            {
                if (selectedUnit) selectedUnit.Deselect();
                selectedUnit = clickedUnit;
                selectedUnitTile = selectedUnit.currentTile;
                selectedUnit.Select();

            }
        }
    }

    /// <summary>
    /// 특정 타일이 선택된 유닛이 점유하고 있는지 확인
    /// </summary>
    public bool IsTileOccupiedBySelectedUnit(Tile tile)
    {
        return selectedUnitTile == tile;
    }

    /// <summary>
    /// 현재 선택된 유닛이 있는 타일 반환
    /// </summary>
    public Tile GetSelectedUnitTile()
    {
        return selectedUnitTile;
    }

    public Unit GetSelectedUnit()
    {
        return selectedUnit;
    }

    private void ResultGame()
    {
        if (UnitManager.Instance.GetEnemyUnits().Count == 0)
            Debug.Log("승리!");
        else if (UnitManager.Instance.GetPlayerUnits().Count == 0)
            Debug.Log("패배!");
    }

    // TODO : 다음 스테이지 로드
}
