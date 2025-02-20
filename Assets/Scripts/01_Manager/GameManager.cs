using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // TODO : ���� �������� �ε�
}
