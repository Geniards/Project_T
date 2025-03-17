using System;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput playerInput;    // PlayerInput ���
    private PlayerInputActions inputActions;
    private Transform cameraTransform;
    private Camera cam;

    [Header("�׼� �ൿ")]
    private InputAction selectAction;       // ���� �׼�
    private InputAction moveAction;         // ���콺 �̵� �׼�
    private InputAction rightClickAction;   // Undo �׼�

    [Header("���콺 �̵�")]
    private InputAction cameraMoveAction;   // ī�޶� �̵�
    private InputAction zoomAction;         // ī�޶� ��
    private InputAction dragStartAction;    // ���콺 �巡�� ����
    private InputAction dragEndAction;      // ���콺 �巡�� ��

    [Header("���� �̵�")]
    [SerializeField] private float maxSpeed = 5f;
    private float speed;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float damping = 15f;

    [Header("Zooming")]
    [SerializeField] private float stepSize = 2f;
    [SerializeField] private float zoomDamping = 7.5f;
    [SerializeField] private float minHeight = 3f;
    [SerializeField] private float maxHeight = 10f;
    [SerializeField] private float zoomSpeed = 2f;

    [Header("Screen Edge Motion")]
    [Range(0f, 0.1f)]
    [SerializeField] private float edgeTolerance = 0.05f;

    [Header("��ȭ �� �Է� ���� �÷���")]
    [SerializeField] private bool isDialogueActive = false;

    private Vector3 targetPos;
    private float zoomHeight;

    private Vector3 horizontalVelocity;
    private Vector3 lastPos;




    private void Awake()
    {
        if (!Instance)
            Instance = this;
        else
            Destroy(gameObject);


        // ���콺 �̵� �ʱ�ȭ
        inputActions = new PlayerInputActions();
        cameraTransform = Camera.main.transform;
        cam = Camera.main;
    }

    private void OnEnable()
    {
        lastPos = this.transform.position;
        cameraMoveAction = inputActions.Click.CameraMove;
        zoomAction = inputActions.Click.Zoom;

        zoomAction.performed += ZoomCamera;
        inputActions.Click.Enable();

        // ���� ���� �� Undo
        selectAction = inputActions.Click.Select;
        selectAction.performed += OnSelectPerformed;
        selectAction.Enable();
        
        moveAction = inputActions.Click.Move;
        moveAction.performed += OnMouseMove;
        moveAction.Enable();
        
        rightClickAction = inputActions.Click.RightClick;
        rightClickAction.performed += HandleRightClick;
        rightClickAction.Enable();
    }

    private void OnDisable()
    {
        inputActions.Click.Disable();
    }

    private void Update()
    {
        // Ű���� �Է�
        GetKeyboardMovenet();
        //CheckMouseAtScreenEdge();

        // �ӵ� ������Ʈ �� ��ġ ������Ʈ
        UpdateVelocity();
        //UpdateCameraPos();
        UpdateBasePosition();
    }

    private void UpdateVelocity()
    {
        horizontalVelocity = (this.transform.position - lastPos) / Time.deltaTime;
        horizontalVelocity.z = 0;

        lastPos = this.transform.position;
    }

    private void GetKeyboardMovenet()
    {
        Vector3 inputValue = cameraMoveAction.ReadValue<Vector2>().x * GetCameraRight() + cameraMoveAction.ReadValue<Vector2>().y * GetCameraUp();
        inputValue = inputValue.normalized;

        targetPos = inputValue;
    }

    /// <summary>
    /// 2D ī�޶��� Right����(�¿�)
    /// </summary>
    /// <returns></returns>
    private Vector3 GetCameraRight()
    {
        Vector3 right = cameraTransform.right;
        right.z = 0;
        return right;
    }

    /// <summary>
    /// 2D ī�޶��� UP����(����)
    /// </summary>
    /// <returns></returns>
    private Vector3 GetCameraUp()
    {
        Vector3 up = cameraTransform.up;
        up.z = 0;
        return up;
    }

    private void UpdateBasePosition()
    {
        Vector3 newPos;
        if (targetPos.sqrMagnitude > 0.1f)
        {
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            newPos = transform.position + targetPos * speed * Time.deltaTime;
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            newPos = transform.position + horizontalVelocity * Time.deltaTime;
        }

        transform.position = new Vector3(newPos.x, newPos.y, cam.transform.position.z);
        cameraTransform.position = transform.position;
        targetPos = Vector3.zero;
    }

    /// <summary>
    /// ���콺 Ŭ���� ���� ����/����.
    /// </summary>
    /// <param name="context"></param>
    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        // ��ȭâ�� Ȱ��ȭ�� ���� X
        if (isDialogueActive)
        {
            DialogueManager.Instance.ShowNextDialogue();
            return;
        }
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.transform.position.z));

        GameManager.Instance.HandleClick(worldPos);
    }

    /// <summary>
    /// ���콺 �̵� ���� �� GridManager�� ����
    /// </summary>
    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector3 worldPos = cam.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, cam.transform.position.z));

        GridManager.Instance.HandleMouseOver(worldPos);
    }

    private void HandleRightClick(InputAction.CallbackContext context)
    {
        if (isDialogueActive) return;

        if (UIManager.Instance.IsAttackMode())
        {
            UIManager.Instance.CancelAttackMode();
        }
        else
        {
            GameManager.Instance.UndoMove();
        }
    }

    private void ZoomCamera(InputAction.CallbackContext context)
    {
        if (isDialogueActive) return;

        float zoomInput = -context.ReadValue<Vector2>().y / 1000f;

        if(Mathf.Abs(zoomInput) > 0.01f)
        {
            float newSize = cam.orthographicSize + zoomInput * stepSize;
            cam.orthographicSize = Mathf.Clamp(newSize, minHeight, maxHeight);

            Debug.Log($"newSize {newSize}");
            Debug.Log($"cam.orthographicSize {cam.orthographicSize}");
        }
    }

    private void UpdateCameraPos()
    {
        float targetSize = Mathf.Lerp(cam.orthographicSize, zoomHeight, Time.deltaTime * zoomDamping);
        cam.orthographicSize = Mathf.Clamp(targetSize, minHeight, maxHeight);
    }

    private void CheckMouseAtScreenEdge()
    {
        if (isDialogueActive) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Vector3 moveDirect = Vector3.zero;

        if (mousePos.x < edgeTolerance * Screen.width)
            moveDirect += -GetCameraRight();
        else if (mousePos.x > (1f - edgeTolerance) * Screen.width)
            moveDirect += GetCameraRight();

        if (mousePos.y < edgeTolerance * Screen.height)
            moveDirect += -GetCameraUp();
        else if (mousePos.y > (1f - edgeTolerance) * Screen.height)
            moveDirect += GetCameraUp();

        targetPos += moveDirect;
    }

    public void EnableDialogueActive()
    {
        isDialogueActive = true;
    }

    public void DisableDialogueActive()
    {
        isDialogueActive = false;
    }
}
