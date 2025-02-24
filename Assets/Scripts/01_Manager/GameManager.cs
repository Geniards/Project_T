using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Pool ������")]
    public GameObject tilePrefab;
    public GameObject unitPrefab;

    [Header("�ʱ� Ǯ ũ��")]
    [SerializeField] private int InitialTileCount = 1024;
    [SerializeField] private int InitialUnitCount = 30;

    private PoolingManager<Tile> tilePool;
    private PoolingManager<Unit> unitPool;

    [Header("JSON Data")]
    public TextAsset stageJsonFile;
    public StageData currentStageData;

    [Header("���õ� ����")]
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
    /// ������Ʈ Ǯ�� �ʱ�ȭ
    /// </summary>
    private void InitializePooling()
    {
        tilePool = new PoolingManager<Tile>(tilePrefab, InitialTileCount, transform);
        unitPool = new PoolingManager<Unit>(unitPrefab, InitialUnitCount, transform);
    }

    /// <summary>
    /// Ÿ�� ������Ʈ Ǯ���� ��������
    /// </summary>
    public Tile GetTile()
    {
        return tilePool.GetObject();
    }

    /// <summary>
    /// ����� ���� Ÿ���� ��ȯ
    /// </summary>
    /// <param name="tile"></param>
    public void ReturnTile(Tile tile)
    {
        tilePool.ReturnObject(tile);
    }

    /// <summary>
    /// ���� ������Ʈ Ǯ���� ��������
    /// </summary>
    public Unit GetUnit()
    {
        return unitPool.GetObject();
    }

    /// <summary>
    /// ����� ���� ������ ��ȯ
    /// </summary>
    /// <param name="unit"></param>
    public void ReturnUnit(Unit unit)
    {
        unitPool.ReturnObject(unit);
    }
    public void LoadStage()
    {
        // ���� ������ �ʱ�ȭ
        ResetStage();

        // �������� ������ �ε�
        currentStageData = JsonUtility.FromJson<StageData>(stageJsonFile.text);

        // Ÿ�� ���� Dictionary ��ȯ
        currentStageData.ConvertTileLegend();

        // �׸��� �� ���� ��ġ
        GridManager.Instance.LoadGrid(currentStageData);
        UnitManager.Instance.LoadUnit(currentStageData);
    }

    /// <summary>
    /// �������� ����
    /// </summary>
    public void ResetStage()
    {
        GridManager.Instance.ResetGrid();
        UnitManager.Instance.ResetUnits();
    }

    /// <summary>
    /// ���콺 Ŭ���� ó��
    /// </summary>
    /// <param name="worldPos"></param>
    public void HandleClick(Vector2 worldPos)
    {
        Vector2Int gridPos = new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
        Tile clickedTile = GridManager.Instance.GetTile(gridPos);

        if (clickedTile)
        {
            Unit clickedUnit = UnitManager.Instance.GetUnitAtPosition(clickedTile.vec2IntPos);

            // ���õ� ���� �缱�ý� ����
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
    /// ���� ����
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
    /// ���� ���� ����
    /// </summary>
    private void DeselectUnit()
    {
        if (selectedUnit)
        {
            selectedUnitTile.ClearHighlight();
            GridManager.Instance.ClearWalkableTiles();
            selectedUnit = null;
            selectedUnitTile = null;
            Debug.Log("����� ���� ���� ����");
        }
    }

    public void MoveUnit(Unit unit, Tile targetTile)
    {
#if UNITY_EDITOR
       // if(!targetTile.isOccupied)
           // Debug.Log($"���� {unit.unitData.unitId} �̵� {targetTile.vec2IntPos}");
       // else
           // Debug.Log($"������ �ش� ��ġ {targetTile.vec2IntPos} �� �����մϴ�. ");
#endif

        // �̵� ��ġ�� ��� ��� �̵����� ������ �ִ� ��ġ�� �̵��ϱ�
        // A*�� �̵���θ� ���� �� �̵����� ���������� �ִ� ��ġ�� �̵�
        Tile moveTile = GridManager.Instance.FindNearestReachableTile(unit, targetTile);
        List<Tile> path = GridManager.Instance.FindPathAStar(unit.currentTile, moveTile);

#if UNITY_EDITOR
        //foreach (Tile tile in path)
        //{
        //    Debug.Log($"A* ��� {tile.vec2IntPos}");
        //}
#endif
        if (path.Count > 0)
        {
            StartCoroutine(MoveUnitAlongPath(unit, path));
        }
    }

    /// <summary>
    /// ������ A* �˰������� ã�� ��θ� ���� �̵�
    /// </summary>
    /// <param name="unit"></param>
    /// <param name="path"></param>
    /// <returns></returns>
    private IEnumerator MoveUnitAlongPath(Unit unit, List<Tile> path)
    {
#if UNITY_EDITOR
        //Debug.Log($"A* �̵� ��� {path.Count-1}");
        //Debug.Log($"���� �̵�  {unit.unitData.moveRange}");
#endif

        foreach (Tile tile in path)
        {
            unit.transform.position = tile.transform.position;
            yield return new WaitForSeconds(0.2f);
        }

        unit.currentTile.isOccupied = false;
        unit.SetCurrentTile(path.Last());
        unit.currentTile.isOccupied = true;

        // �̵� ����� �������� ������ �̵� ���� ������ �ִ�ġ�� ��� �� ����
        if (unit.currentTile == path.Last())
        {
            unit.unitState = E_UnitState.Complete;
        }
        DeselectUnit();
    }

    /// <summary>
    /// Ư�� Ÿ���� ���õ� ������ �����ϰ� �ִ��� Ȯ��
    /// </summary>
    public bool IsTileOccupiedBySelectedUnit(Tile tile)
    {
        return selectedUnitTile == tile;
    }

    /// <summary>
    /// ���� ���õ� ������ �ִ� Ÿ�� ��ȯ
    /// </summary>
    public Tile GetSelectedUnitTile()
    {
        return selectedUnitTile;
    }

    // TODO : ���� �������� �ε�
}
