using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Pool ������")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private GameObject gameObjectivesUI; // ���� ��ǥ UI

    [Header("�ʱ� Ǯ ũ��")]
    [SerializeField] private int InitialTileCount = 1024;
    [SerializeField] private int InitialUnitCount = 30;

    private PoolingManager<Tile> tilePool;
    private PoolingManager<Unit> unitPool;

    [Header("JSON Data")]
    [SerializeField] private TextAsset stageJsonFile;
    [SerializeField] private StageData currentStageData;

    [Header("���õ� ����")]
    private Unit selectedUnit;
    private Tile selectedUnitTile;

    public bool isAutoBattle = false; // �Ʊ� �ڵ� ���� ����

    private int currentStageIndex = 0; // ���� �������� ID����
    private int maxTurnCount = 20; // �ִ� �� ��
    private bool isGameOver = false; // ���� ���� ����

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
        StartCoroutine(StartGameSequence());
        InitializePooling();
    }

    /// <summary>
    /// ������ ���۵� �� ù ��° ���丮 �̺�Ʈ ����
    /// </summary>
    private IEnumerator StartGameSequence()
    {
        //yield return new WaitForSeconds(1f); // �� ��ȯ �� �ε� ���

        yield return StartCoroutine(PlayIntroEvent());

        yield return new WaitForSeconds(0.5f);
        // ���� ��ǥ UI ǥ�� (UIManager���� ó��)
        UIManager.Instance.ShowGameObjectives();
    }

    /// <summary>
    /// ���� ���� �� �̺�Ʈ(��ȭ, �ִϸ��̼�) ����
    /// </summary>
    private IEnumerator PlayIntroEvent()
    {
        DialogueManager.Instance.LoadDialogue("IntroStory");
        DialogueManager.Instance.StartDialogue();

        while (DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }

        //// ���� ���� ���� (���������� �߰� ����)
        //Unit playerUnit = UnitManager.Instance.GetUnitsByType(101).Find(x => true);
        //if (playerUnit != null)
        //{
        //    yield return StartCoroutine(playerUnit.MoveTo(GridManager.Instance.GetTile()));
        //}
    }

    private void Update()
    {
        //ResultGame();
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

    /// <summary>
    /// ���� �������� �ε� (ó�� ���� or �ٽ� ����)
    /// </summary>
    /// <param name="stageIndex"></param>
    public void LoadStage(int stageIndex)
    {
        isGameOver = false;
        currentStageIndex = stageIndex;

        // ���� ������ �ʱ�ȭ
        ResetStage();

        // �������� ������ �ε�
        string stageFileName = $"StageData_{stageIndex}";
        Debug.Log(stageFileName);
        TextAsset stageJson = Resources.Load<TextAsset>($"StageData_{stageIndex}");
        currentStageData = JsonUtility.FromJson<StageData>(stageJson.text);


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

            // ���ڸ� �̵�
            if (selectedUnit && clickedTile == selectedUnitTile)
            {
                selectedUnit.Deselect();
                GridManager.Instance.FindAttackableTiles(selectedUnit);
                GridManager.Instance.ShowHighLight();
                selectedUnit.unitState = E_UnitState.Move;
                UIManager.Instance.isAttackMode = true;
                UIManager.Instance.selectedUnit = selectedUnit;

                // ���ݹ����� ã�⸸�ϰ� ������ ��ư�� UI�� ������ ���� �Ҽ� �ֵ��� �Ѵ�.
                // �̶� ���ڸ� �̵� �� ������ ���ϰ� ���� �����ҽ� 
                // selectedUnit = null; ���õ� ������ �����ؾ��Ѵ�.
                return;
            }
            // �̵� ������ Ÿ���� ��� �̵�
            else if (selectedUnit && GridManager.Instance.IsWalkableTile(clickedTile))
            {
                selectedUnit.MoveTo(clickedTile);
                return;
            }
            // �̵� �Ұ����� Ÿ���� ��� �̵� �Ұ� ��Ž��
            else if (selectedUnit && clickedTile != selectedUnitTile && selectedUnit.unitState != E_UnitState.Move)
            {
                selectedUnit.Deselect();
                selectedUnit = null;
            }

            // ���õ� ������ ���� ���� ������ ���
            if (selectedUnit && clickedUnit && selectedUnit.unitData.unitTeam != clickedUnit.unitData.unitTeam)
            {
                StartCoroutine(selectedUnit.Attack(clickedUnit));
            }

            // ���õ� ������ ���� ���(���� ���� �κ�)
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
    /// ���õ� ������ �̵� �ǵ�����
    /// </summary>
    public void UndoMove()
    {
        if (!selectedUnit || selectedUnit.unitState != E_UnitState.Move)
        {
            Debug.Log("���� ��ġ�� �ǵ��� ������ �����ϴ�.");
            return;
        }

        selectedUnit.UndoMove();
        selectedUnitTile = selectedUnit.currentTile;

        // ���� ��� ����
        UIManager.Instance.isAttackMode = false;
        GridManager.Instance.ClearAttackableTiles();

        selectedUnit.Select();
    }

    /// <summary>
    /// ���� ���õ� ������ �ִ� Ÿ�� ��ȯ
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
            Debug.Log("�¸�!");
        else if (UnitManager.Instance.GetPlayerUnits().Count == 0)
            Debug.Log("�й�!");
    }

    public void StartGame()
    {
        isGameOver = false;
        // ���� ���� ���� ���������� ������ ù ��° ������������ ����
        if (currentStageIndex == 0)
        {
            currentStageIndex = 1;
        }

        LoadStage(currentStageIndex);
        Camera.main.transform.position = new Vector3(currentStageData.width / 2, currentStageData.height / 2, Camera.main.transform.position.z);
        TurnManager.Instance.StartTurn();
    }

    public void CheckGameState()
    {
        if (isGameOver) return;

        int playerUnits = UnitManager.Instance.GetPlayerUnits().Count;
        int enemyUnits = UnitManager.Instance.GetEnemyUnits().Count;
        int currentTurn = TurnManager.Instance.GetTurnCount();

        // �¸� ����: ������ ��� ���ŵ�
        if (enemyUnits == 0)
        {
            StartCoroutine(HandleVictory());
            return;
        }

        // �й� ����: �Ʊ��� ��� ���ŵ� �Ǵ� �� �ʰ� ���
        if (playerUnits == 0 || currentTurn > maxTurnCount)
        {
            StartCoroutine(HandleDefeat());
        }
    }

    /// <summary>
    /// �¸� �� ���� (�̺�Ʈ �� ���� ��������)
    /// </summary>
    private IEnumerator HandleVictory()
    {
        isGameOver = true;
        UIManager.Instance.ShowVictoryUI();

        // �̺�Ʈ ���� (��ȭ �� ĳ���� �ִϸ��̼�)
        yield return StartCoroutine(PlayVictoryEvent());
    }

    /// <summary>
    /// �й� �� ���� (UI ǥ�� �� ������ ����)
    /// </summary>
    private IEnumerator HandleDefeat()
    {
        isGameOver = true;
        UIManager.Instance.ShowDefeatUI();

        yield return null; // UI���� �÷��̾� ���� ���
    }

    /// <summary>
    /// �¸� �� ��ȭ �� �ִϸ��̼� ����
    /// </summary>
    private IEnumerator PlayVictoryEvent()
    {
        DialogueManager.Instance.LoadDialogue($"VictoryStage_{currentStageIndex}");
        DialogueManager.Instance.StartDialogue();

        while (DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }

        //// Ư�� ������ �̵��ϴ� ����
        //Unit hero = UnitManager.Instance.GetUnitsByType(101).Find(x => true);
        //if (hero != null)
        //{
        //    yield return StartCoroutine(hero.MoveTo(GridManager.Instance.GetTile(new Vector2Int(5, 5))));
        //}
    }

    /// <summary>
    /// ���� �������� �ε�
    /// </summary>
    public void LoadNextStage()
    {
        int nextStageIndex = currentStageIndex + 1;
        TextAsset nextStageJson = Resources.Load<TextAsset>($"StageData_{nextStageIndex}");
        Debug.Log(nextStageJson);
        if (nextStageJson == null)
        {
            Debug.Log("��� ���������� Ŭ�����߽��ϴ�! ���� ���� �Ǵ� ���� �޴��� �̵�.");
            UIManager.Instance.ShowGameClearUI();
            return;
        }

        // ���� ������ ���� �������� ������ �ε�
        LoadStage(nextStageIndex);
    }

    /// <summary>
    /// ���� �������� �ٽ� ����
    /// </summary>
    public void RestartStage()
    {
        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        LoadStage(currentStageIndex);
    }

    /// <summary>
    /// ���� �޴��� �̵�
    /// </summary>
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("00_Start");
    }
}
