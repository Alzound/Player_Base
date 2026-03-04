using UnityEngine;

public class PlayerInputsystemAdapter : MonoBehaviour, PlayerInputContracts.IPlayerInput
{
    private Controller _inputActions;

    private void Awake()
    {
        _inputActions = new Controller(); 
        if(_inputActions == null)
        {
            enabled = false;
            return; 
        }
    }

    private void OnEnable()
    {
        _inputActions.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Disable();   
    }

    public PlayerInputContracts.PlayerInputState Read()
    {
        Vector2 move = _inputActions.Player.Move.ReadValue<Vector2>(); 
        bool interact = _inputActions.Player.Interact.triggered;
        bool sprint =  _inputActions.Player.Sprint.IsPressed();
        bool jump = _inputActions.Player.Jump.WasPressedThisFrame(); 
        Vector2 look = _inputActions.Player.Look.ReadValue<Vector2>();

        return new PlayerInputContracts.PlayerInputState
        {
            Move = move,
            Interact = interact,
            Look = look,
            Jump = jump,
            Sprint = sprint,
        }; 
    }
}
