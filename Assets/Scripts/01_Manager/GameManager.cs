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

    private void Update()
    {
        ResultGame();
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

            // �̹� �ൿ�� �Ϸ��� �����̶�� Ŭ�� ����
            if (selectedUnit && selectedUnit.unitState == E_UnitState.Complete)
            {
                Debug.Log("�̹� ���� ������ �����Դϴ�.");
                return;
            }

            // ���õ� ���� �缱�ý� ����
            if (selectedUnit && clickedTile == selectedUnitTile)
            {
                GridManager.Instance.ClearWalkableTiles();
                GridManager.Instance.FindAttackableTiles(selectedUnit);
                if (selectedUnit.FindAttackableUnit())
                {
                    GridManager.Instance.ShowHiggLight();
                }
                else
                {
                    Debug.Log("���� ������ ������ ���� - ��� ���� ����");
                }
                return;
            }
            else if (selectedUnit && GridManager.Instance.IsWalkableTile(clickedTile))
            {
                selectedUnit.MoveTo(clickedTile);

                return;
            }

            // ���õ� ������ ���� ���� ������ ���
            if (selectedUnit && clickedUnit && selectedUnit.unitData.unitTeam != clickedUnit.unitData.unitTeam)
            {
                selectedUnit.Attack(clickedUnit);
                GridManager.Instance.ClearAttackableTiles();
                return;
            }

            // ���õ� ������ ���� ���
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

    private void ResultGame()
    {
        if (UnitManager.Instance.GetEnemyUnits().Count == 0)
            Debug.Log("�¸�!");
        else if (UnitManager.Instance.GetAllUnits().Count == 0)
            Debug.Log("�й�!");
    }

    // TODO : ���� �������� �ε�
}
