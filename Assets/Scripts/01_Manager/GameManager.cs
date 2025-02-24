using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

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

            // 선택된 유닛 재선택시 해제
            if (selectedUnit && clickedTile == selectedUnitTile)
            {
                DeselectUnit();
                return;
            }
            else if (selectedUnit && GridManager.Instance.IsWalkableTile(clickedTile))
            {
                MoveUnit(selectedUnit, clickedTile);
            }

            if (clickedUnit && clickedUnit.unitData.unitTeam == E_UnitTeam.Ally && clickedUnit.unitState == E_UnitState.Idle)
            {
                SelectUnit(clickedUnit);
            }
        }
    }

    /// <summary>
    /// 유닛 선택
    /// </summary>
    /// <param name="unit"></param>
    private void SelectUnit(Unit unit)
    {
        if (selectedUnit)
        {
            selectedUnitTile.ClearHighlight();
            GridManager.Instance.ClearWalkableTiles();
        }

        selectedUnit = unit;
        selectedUnitTile = unit.currentTile;
        selectedUnitTile.HighlightTile(new Color(1f, 1f, 0f, 0.3f));
        GridManager.Instance.FindWalkableTiles(unit);
    }

    /// <summary>
    /// 유닛 선택 해제
    /// </summary>
    private void DeselectUnit()
    {
        if (selectedUnit)
        {
            selectedUnitTile.ClearHighlight();
            GridManager.Instance.ClearWalkableTiles();
            selectedUnit = null;
            selectedUnitTile = null;
            Debug.Log("디버그 유닛 선택 해제");
        }
    }

    public void MoveUnit(Unit unit, Tile targetTile)
    {
#if UNITY_EDITOR
       // if(!targetTile.isOccupied)
           // Debug.Log($"유닛 {unit.unitData.unitId} 이동 {targetTile.vec2IntPos}");
       // else
           // Debug.Log($"유닛이 해당 위치 {targetTile.vec2IntPos} 에 존재합니다. ");
#endif

        // 이동 위치가 벗어난 경우 이동가능 범위의 최대 위치로 이동하기
        // A*로 이동경로를 저장 후 이동가능 범위에서의 최대 위치로 이동
        Tile moveTile = GridManager.Instance.FindNearestReachableTile(unit, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(unit.currentTile, moveTile);

#if UNITY_EDITOR
        //foreach (Tile tile in path)
        //{
        //    Debug.Log($"A* 경로 {tile.vec2IntPos}");
        //}
#endif
        if (path.Count > 0)
        {
            StartCoroutine(MoveUnitAlongPath(unit, path));
        }
    }

    /// <summary>
    /// 유닛을 A* 알고리즘으로 찾은 경로를 따라 이동
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveUnitAlongPath(Unit unit, List<Tile> path)
    {
#if UNITY_EDITOR
        //Debug.Log($"A* 이동 경로 {path.Count-1}");
        //Debug.Log($"유닛 이동  {unit.unitData.moveRange}");
#endif

        foreach (Tile tile in path)
        {
            unit.transform.position = tile.transform.position;
            yield return new WaitForSeconds(0.2f);
        }

        unit.currentTile.isOccupied = false;
        unit.SetCurrentTile(path.Last());
        unit.currentTile.isOccupied = true;

        // 이동 경로의 마지막이 유닛의 이동 가능 범위의 최대치일 경우 턴 종료
        if (unit.currentTile == path.Last())
        {
            unit.unitState = E_UnitState.Complete;
        }
        DeselectUnit();
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

    // TODO : 다음 스테이지 로드
}
