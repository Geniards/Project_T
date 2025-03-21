using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Pool 프리팹")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject unitPrefab;

    [Header("초기 풀 크기")]
    [SerializeField] private int InitialTileCount = 1024;
    [SerializeField] private int InitialUnitCount = 30;

    private PoolingManager<Tile> tilePool;
    private PoolingManager<Unit> unitPool;

    [Header("JSON Data")]
    [SerializeField] private TextAsset stageJsonFile;
    [SerializeField] private StageData currentStageData;

    [Header("선택된 유닛")]
    private Tile selectedUnitTile;
    public Unit selectedUnit { get; set; }

    public bool isAutoBattle = false; // 아군 자동 전투 여부

    private int currentStageIndex = 0; // 현재 스테이지 ID저장
    private int maxTurnCount = 20; // 최대 턴 수
    private bool isGameOver = false; // 게임 종료 여부

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
        // 씬이 전환된 후 LoadStage가 완료되었을 때 저장된 데이터 적용
        if (SaveLoadManager.Instance.HasPendingLoadData())
        {
            InitializePooling();
            Debug.Log("저장된 데이터 적용 시작...");
            SaveLoadManager.Instance.ApplyLoadedData();
            return;
        }

        StartCoroutine(StartGameSequence());
        InitializePooling();
    }

    /// <summary>
    /// 게임이 시작될 때 첫 번째 스토리 이벤트 실행
    /// </summary>
    private IEnumerator StartGameSequence()
    {
        yield return StartCoroutine(PlayIntroEvent());

        // 게임 목표 UI 표시 (UIManager에서 처리)
        UIManager.Instance.ShowGameObjectives();
    }

    /// <summary>
    /// 게임 시작 전 이벤트(대화, 애니메이션) 실행
    /// </summary>
    private IEnumerator PlayIntroEvent()
    {
        DialogueManager.Instance.LoadDialogue("StageDialogue_1");
        DialogueManager.Instance.StartDialogue();

        while (DialogueManager.Instance.IsDialogueActive())
        {
            yield return null;
        }
    }

    private void Update()
    {
        //ResultGame();
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

    /// <summary>
    /// 현재 스테이지 로드 (처음 시작 or 다시 시작)
    /// </summary>
    /// <param name="stageIndex"></param>
    public void LoadStage(int stageIndex)
    {
        isGameOver = false;
        currentStageIndex = stageIndex;

        // 기존 데이터 초기화
        ResetStage();

        // 스테이지 데이터 로드
        string stageFileName = $"StageData_{currentStageIndex}";
        Debug.Log(stageFileName);
        TextAsset stageJson = Resources.Load<TextAsset>($"StageData_{currentStageIndex}");
        currentStageData = JsonUtility.FromJson<StageData>(stageJson.text);

        // 타일 정보 Dictionary 변환
        currentStageData.ConvertTileLegend();

        // 스테이지별 BGM 변경
        AudioClip stageBGM = Resources.Load<AudioClip>($"Audio/{currentStageData.bgmFileName}");
        if (stageBGM != null)
        {
            SoundManager.Instance.PlayBGM(stageBGM);
        }

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
                GridManager.Instance.ShowHighLight();
                selectedUnit.unitState = E_UnitState.Move;
                UIManager.Instance.isAttackMode = true;
                UIManager.Instance.selectedUnit = selectedUnit;

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
    /// 선택된 유닛의 이동 되돌리기
    /// </summary>
    public void UndoMove()
    {
        if (!selectedUnit || selectedUnit.unitState != E_UnitState.Move)
        {
            Debug.Log("이전 위치로 되돌릴 유닛이 없습니다.");
            return;
        }

        selectedUnit.UndoMove();
        selectedUnitTile = selectedUnit.currentTile;

        // 공격 모드 해제
        UIManager.Instance.isAttackMode = false;
        GridManager.Instance.ClearAttackableTiles();

        selectedUnit.Select();
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

    public void StartGame()
    {
        isGameOver = false;
        // 현재 진행 중인 스테이지가 없으면 첫 번째 스테이지부터 시작
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

        // 승리 조건: 적군이 모두 제거됨
        if (enemyUnits == 0)
        {
            StartCoroutine(HandleVictory());
            return;
        }

        // 패배 조건: 아군이 모두 제거됨 또는 턴 초과 경과
        if (playerUnits == 0 || currentTurn > maxTurnCount)
        {
            StartCoroutine(HandleDefeat());
        }
    }

    /// <summary>
    /// 승리 시 실행 (이벤트 후 다음 스테이지)
    /// </summary>
    private IEnumerator HandleVictory()
    {
        isGameOver = true;

        string victoryDialogue = $"VictoryStage_{currentStageIndex}";
        TextAsset dialogueFile = Resources.Load<TextAsset>($"Dialogues/{victoryDialogue}");

        if (dialogueFile != null)
        {
            //대화 이벤트 실행
            DialogueManager.Instance.LoadDialogue(victoryDialogue);
            DialogueManager.Instance.StartDialogue();

            while (DialogueManager.Instance.IsDialogueActive())
            {
                yield return null;
            }
        }
        SoundManager.Instance.PlaySFX(SoundManager.Instance.victorySFX);
        UIManager.Instance.HideActionMenu();
        UIManager.Instance.ShowVictoryUI();
    }

    /// <summary>
    /// 패배 시 실행 (UI 표시 및 선택지 제공)
    /// </summary>
    private IEnumerator HandleDefeat()
    {
        isGameOver = true;

        // 대화 이벤트가 존재하는지 확인
        string defeatDialogue = $"DefeatStage_{currentStageIndex}";
        TextAsset dialogueFile = Resources.Load<TextAsset>($"Dialogues/{defeatDialogue}");

        if (dialogueFile != null)
        {
            // 대화 이벤트 실행
            DialogueManager.Instance.LoadDialogue(defeatDialogue);
            DialogueManager.Instance.StartDialogue();

            while (DialogueManager.Instance.IsDialogueActive())
            {
                yield return null;
            }
        }

        // 대화 이벤트가 끝나면 결과창 표시
        SoundManager.Instance.PlaySFX(SoundManager.Instance.defeatSFX);
        UIManager.Instance.HideActionMenu();
        UIManager.Instance.ShowDefeatUI();
    }

    /// <summary>
    /// 다음 스테이지 로드
    /// </summary>
    public void LoadNextStage()
    {
        int nextStageIndex = currentStageIndex + 1;
        TextAsset nextStageJson = Resources.Load<TextAsset>($"StageData_{nextStageIndex}");
        Debug.Log(nextStageJson);
        if (nextStageJson == null)
        {
            Debug.Log("모든 스테이지를 클리어했습니다! 게임 종료 또는 메인 메뉴로 이동.");
            UIManager.Instance.ShowGameClearUI();
            return;
        }

        ResetGameState();
        // 같은 씬에서 다음 스테이지 데이터 로드

        LoadStage(nextStageIndex);
        StartCoroutine(StartStageDialogue(nextStageIndex));
    }

    /// <summary>
    /// 현재 스테이지 다시 시작
    /// </summary>
    public void RestartStage()
    {
        UIManager.Instance.HideActionMenu();

        //SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        ResetGameState(); 
        LoadStage(currentStageIndex);
    }

    /// <summary>
    /// 메인 메뉴로 이동
    /// </summary>
    public void GoToMainMenu()
    {
        currentStageIndex = 0;
        SceneManager.LoadScene("00_Start");
    }

    /// <summary>
    /// 게임 상태 완전 초기화
    /// </summary>
    private void ResetGameState()
    {
        // UI 초기화
        UIManager.Instance.ClearAllUI();
        UIManager.Instance.CancelAttackMode();
        UIManager.Instance.HideActionMenu();

        // 유닛 데이터 초기화
        UnitManager.Instance.ResetUnits();

        // 타일 데이터 초기화
        GridManager.Instance.ResetGrid();

        // 선택된 유닛 초기화
        selectedUnit = null;
        selectedUnitTile = null;
        UIManager.Instance.selectedUnit = null;

        // 게임 변수 초기화
        isGameOver = false;

        // 모든 UI 요소 초기화
        TurnManager.Instance.ResetTurn();
    }

    /// <summary>
    /// 스테이지 시작 시 대화 이벤트 실행
    /// </summary>
    private IEnumerator StartStageDialogue(int stageIndex)
    {
        string dialogueFileName = $"StageDialogue_{stageIndex}";
        TextAsset dialogueFile = Resources.Load<TextAsset>($"Dialogues/{dialogueFileName}");

        if (dialogueFile != null)
        {
            DialogueManager.Instance.LoadDialogue(dialogueFileName);
            DialogueManager.Instance.StartDialogue();

            while (DialogueManager.Instance.IsDialogueActive())
            {
                yield return null; // 대화가 끝날 때까지 대기
            }
        }

        // 대화 종료 후 게임 목표 UI 표시
        UIManager.Instance.ShowGameObjectives();
    }

    public int GetCurrentStage()
    {
        return currentStageIndex;
    }
}
