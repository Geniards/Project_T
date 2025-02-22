using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput playerInput;    // PlayerInput 사용
    private InputAction selectAction;   // 선택 액션
    private InputAction moveAction;     // 마우스 이동 액션

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerInput = GetComponent<PlayerInput>();      // PlayerInput 컴포넌트 가져오기
            selectAction = playerInput.actions["Select"];   // Select 액션 가져오기
            moveAction = playerInput.actions["Move"];       //Move 액션 가져오기

            selectAction.performed += OnSelectPerformed;    // 액션 수행시 호출할 함수 등록
            moveAction.performed += OnMouseMove;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 마우스 클릭시 유닛 선택/해제.
    /// </summary>
    /// <param name="context"></param>
    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GameManager.Instance.HandleClick(worldPos);
    }

    /// <summary>
    /// 마우스 이동 감지 후 GridManager에 전달
    /// </summary>
    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GridManager.Instance.HandleMouseOver(worldPos);
    }
}
