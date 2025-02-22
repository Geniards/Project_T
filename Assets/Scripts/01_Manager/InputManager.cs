using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput playerInput;    // PlayerInput ���
    private InputAction selectAction;   // ���� �׼�
    private InputAction moveAction;     // ���콺 �̵� �׼�

    private void Awake()
    {
        if (!Instance)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            playerInput = GetComponent<PlayerInput>();      // PlayerInput ������Ʈ ��������
            selectAction = playerInput.actions["Select"];   // Select �׼� ��������
            moveAction = playerInput.actions["Move"];       //Move �׼� ��������

            selectAction.performed += OnSelectPerformed;    // �׼� ����� ȣ���� �Լ� ���
            moveAction.performed += OnMouseMove;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// ���콺 Ŭ���� ���� ����/����.
    /// </summary>
    /// <param name="context"></param>
    private void OnSelectPerformed(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GameManager.Instance.HandleClick(worldPos);
    }

    /// <summary>
    /// ���콺 �̵� ���� �� GridManager�� ����
    /// </summary>
    private void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        GridManager.Instance.HandleMouseOver(worldPos);
    }
}
