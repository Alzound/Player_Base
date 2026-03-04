using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input")] //Implements IPlayerInput
    [SerializeField] private MonoBehaviour inputSource; 
    [Header("Instance")]
    public static PlayerController Instance { get; private set; }

    [Header("References")]
    [SerializeField] private PlayerMotor motor; 
    [SerializeField] private Transform cameraTransform;
    //[SerializeField] private PlayerInteractor interactor;

    [Header("Jump")]
    [SerializeField] private bool _jump = false;
    [SerializeField] private float jumpForce = 10;


    private PlayerInputContracts.IPlayerInput _input;

    public float MoveY { get; private set; }
    public bool IsRunning { get; private set; }

    private void Reset() => motor = GetComponent<PlayerMotor>();

    private void Awake()
    {
        Instance = this;

        _input = inputSource as PlayerInputContracts.IPlayerInput;
        if(_input == null)
        {
            Debug.LogError("Input source must implement IPlayerInput");
            enabled = false;
            return; 
        }


        if (!motor) motor = GetComponent<PlayerMotor>();
        if (!cameraTransform) Debug.LogWarning("PlayerControllerRB: asigna cameraTransform (tu cámara FPS).");

        // Busca el interactor en el player si no está asignado
        //if (!interactor) interactor = GetComponent<PlayerInteractor>();
        //if (!interactor) Debug.LogWarning("PlayerControllerRB: falta PlayerInteractor (router de interacción).");
    }

    private void FixedUpdate()
    {
        if (_jump)
        {
            motor.Jump();
            _jump = false; 
        }
    }

    private void Update()
    {
        var s = _input.Read();
        if (s.Jump) _jump = true; 

        // Para pasar los valores al animator
        MoveY = s.Move.y;
        IsRunning = s.Sprint;

        // --- movimiento relativo a la cámara ---
        Vector3 fwd, right;
        if (cameraTransform)
        {
            fwd = cameraTransform.forward; fwd.y = 0f; fwd.Normalize();
            right = cameraTransform.right; right.y = 0f; right.Normalize();
        }
        else
        {
            fwd = Vector3.forward;
            right = Vector3.right;
        }

        // s.Move.x = strafe, s.Move.z = adelante/atrás
        Vector3 worldPlanar = right * s.Move.x + fwd * s.Move.y;

        motor.SetDesiredPlanarWorld(worldPlanar);
        motor.SetRun(s.Sprint);

        // Debug opcional
        Debug.DrawRay(transform.position + Vector3.up * 0.2f, worldPlanar, Color.cyan, 0f, false);
    }
}
