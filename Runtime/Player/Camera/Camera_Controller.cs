using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private MonoBehaviour inputSource;
    private PlayerInputContracts.IPlayerInput _input;
    private PlayerInputContracts.PlayerInputState _s;

    [Header("References")]
    [SerializeField] private Transform playerBody;   // Raíz visual/física del jugador (el que rota en Y)

    [Header("Settings")]
    [SerializeField] private float lookSensitivity = 200f;
    [SerializeField] private Vector2 pitchLimits = new Vector2(-60f, 75f);

    private float _pitch; // rotación vertical acumulada (cámara)

    private void Awake()
    {
        _input = inputSource as PlayerInputContracts.IPlayerInput;
        if (_input == null)
        {
            Debug.LogError("Input source does not implement IPlayerInput.");
            enabled = false;
        }
    }

    private void LateUpdate()
    {
        _s = _input.Read();
        RotateBodyAndCamera();
    }

    private void RotateBodyAndCamera()
    {
        Vector2 look = _s.Look;

        // 1) Yaw → rota el cuerpo completo en Y (gira al personaje)
        float yawDelta = look.x * lookSensitivity * Time.deltaTime;
        if (playerBody != null && Mathf.Abs(yawDelta) > Mathf.Epsilon)
        {
            playerBody.Rotate(0f, yawDelta, 0f, Space.World);
        }

        // 2) Pitch → solo la cámara (este CameraRoot), clamp entre límites
        float pitchDelta = -look.y * lookSensitivity * Time.deltaTime; // invertido para que mover mouse arriba mire arriba
        _pitch = Mathf.Clamp(_pitch + pitchDelta, pitchLimits.x, pitchLimits.y);

        // Como este script está en CameraRoot, rotamos local solo en X
        transform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
    }
}
