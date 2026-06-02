using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    // Player events
    public event Action OnSprintPerformed;
    public event Action OnSprintCanceled;
    public event Action<Vector2> OnMove;
    public event Action OnJumpPerformed;
    public event Action OnDashPerformed;
    public event Action OnAttackPerformed;
    public event Action OnShootPerformed;
    
    // Global events
    public event Action OnPausePerformed;

    // UI events
    public event Action OnCancelPerformed;

    InputSystem_Actions _actions;

    void Awake()
    {
        _actions = new InputSystem_Actions();
        Debug.Log($"Cancel binding: {_actions.UI.Cancel.bindings.Count}");
    }

    void OnEnable()
    {
        // Player
        _actions.Player.Sprint.performed += _ => OnSprintPerformed?.Invoke();
        _actions.Player.Sprint.canceled += _ => OnSprintCanceled?.Invoke();
        _actions.Player.Move.performed += ctx => OnMove?.Invoke(ctx.ReadValue<Vector2>());
        _actions.Player.Move.canceled  += _ => OnMove?.Invoke(Vector2.zero);
        _actions.Player.Jump.performed += _ => OnJumpPerformed?.Invoke();
        _actions.Player.Dash.performed += _ => OnDashPerformed?.Invoke();
        _actions.Player.Attack.performed += _ => OnAttackPerformed?.Invoke();
        _actions.Player.Shoot.performed += _ => OnShootPerformed?.Invoke();

        // Global
        _actions.Global.Pause.performed += _ => OnPausePerformed?.Invoke();

        // UI
        _actions.UI.Cancel.performed += ctx => {
            Debug.Log($"CANCEL FIRED, subscribers: {OnCancelPerformed?.GetInvocationList()?.Length ?? 0}");
            OnCancelPerformed?.Invoke();
        };

        _actions.Global.Enable();
        SetGameplay();
    }

    void OnDisable() => _actions.Disable();

    public void SetGameplay()
    {
        _actions.UI.Disable();
        _actions.Player.Enable();
        // TAMBAHKAN LOG INI:
        Debug.Log("<color=green>[INPUT TRACKER]</color> Fungsi SetGameplay() dipanggil!");
    }

    public void SetUI()
    {
        _actions.Player.Disable();
        _actions.UI.Enable();
        // TAMBAHKAN LOG INI:
        Debug.Log("<color=red>[INPUT TRACKER]</color> Fungsi SetUI() dipanggil! Seseorang telah mengunci pergerakan player!");
    }
    
    // Tambahkan fungsi ini di dalam InputReader.cs
    public void ClearAllSubscribers()
    {
        // Mengosongkan event agar semua script lama yang menggantung terhapus dari memori
        OnMove = null;
        OnJumpPerformed = null;
        OnDashPerformed = null;
        OnAttackPerformed = null;
        OnShootPerformed = null;
    
        // SESUAIKAN: Kosongkan juga event menu/pause kamu di sini
        OnPausePerformed = null; 
        OnCancelPerformed = null;

        Debug.Log("InputReader: Seluruh delegasi event berhasil dibersihkan total!");
    }
}