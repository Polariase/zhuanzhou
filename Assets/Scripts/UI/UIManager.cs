using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public InventoryPanel inventoryPanel;
    public LoadingPanel loadingPanel;
    public PausePanel pausePanel;
    public DeathPanel deathPanel;

    public CrosshairController crosshair;
    public HUDController hud;

    private readonly Stack<BasePanel> _panelStack = new();
    private PlayerStats stats;
    private PlayerInput _input;
    private PlayerController _pc;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUIMode(GameState state)
    {
        switch (state)
        {
            case GameState.Entry:
                hud.gameObject.SetActive(false);
                inventoryPanel.gameObject.SetActive(false);
                ApplyCursorState(true, CursorLockMode.None, false);
                break;
            case GameState.Survival:
                hud.gameObject.SetActive(true);
                inventoryPanel.gameObject.SetActive(true);
                UpdateUIState();
                break;
        }
        Back();
    }

    public void ShowDeathPanel()
    {
        Back();
        if (deathPanel != null)
        {
            OpenPanel(deathPanel);
        }
    }

    public void ShowLoading(bool state)
    {
        if (loadingPanel != null)
        {
            if (state)
            {
                loadingPanel.Open();
            }
            else
            {
                loadingPanel.Close();
            }
        }
    }

    public void OpenPanel(BasePanel panel)
    {
        if (panel == null || _panelStack.Count > 0) return;

        panel.Open();
        _panelStack.Push(panel);
        UpdateUIState();
    }

    public void Back()
    {
        if (_panelStack.Count == 0) return;

        BasePanel top = _panelStack.Pop();
        top.Close();
        UpdateUIState();
    }

    public void UpdateUIState()
    {
        if (_panelStack.Count > 0)
        {
            _input.SwitchCurrentActionMap("UI");
            ApplyCursorState(true, CursorLockMode.None, false);
        }
        else
        {
            _input.SwitchCurrentActionMap("Player");
            ApplyCursorState(false, CursorLockMode.Locked, true);
        }
    }

    private void ApplyCursorState(bool visible, CursorLockMode lockMode, bool showCrosshair)
    {
        Cursor.visible = visible;
        Cursor.lockState = lockMode;
        if (crosshair != null)
        {
            crosshair.gameObject.SetActive(showCrosshair);
        }
    }

    private void BindInputs()
    {
        if (_input == null) return;

        _input.actions["Player/Inventory"].performed += OnInventoryPerformed;
        _input.actions["UI/Inventory"].performed += OnInventoryPerformed;
        _input.actions["Quit"].performed += OnCancelPerformed;
        _input.actions["Pause"].performed += OnPausePerformed;
    }

    private void UnbindInputs()
    {
        if (_input == null) return;

        _input.actions["Player/Inventory"].performed -= OnInventoryPerformed;
        _input.actions["UI/Inventory"].performed -= OnInventoryPerformed;
        _input.actions["Quit"].performed -= OnCancelPerformed;
        _input.actions["Pause"].performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext ctx)
    {
        if (GameManager.Instance.currentGameState != GameState.Entry)
        {
            if (pausePanel != null && _panelStack.Count == 0)
            {
                OpenPanel(pausePanel);
            }
        }
    }

    private void OnInventoryPerformed(InputAction.CallbackContext ctx)
    {
        if (inventoryPanel != null && inventoryPanel.isOpen)
            Back();
        else
            OpenPanel(inventoryPanel);
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (_panelStack.Count > 0)
            Back();
    }

    public void Initialize(PlayerController player)
    {
        Cleanup();
        _pc = player;
        if (_pc == null) return;
        _input = _pc.GetComponent<PlayerInput>();
        stats = _pc.stats;
        BindInputs();

        hud.Initialize(_pc);
        crosshair.Initialize(_pc);
    }

    private void Cleanup()
    {
        UnbindInputs();
    }

    private void OnDestroy()
    {
        Cleanup();
    }
}