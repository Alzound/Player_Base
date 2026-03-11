using UnityEngine;

namespace LvlPlayer
{
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Speeds")]
        [SerializeField] private float walkSpeed = 4.5f;
        [SerializeField] private float runMultiplier = 1.8f;
        [SerializeField] private float acceleration = 30f;
        [SerializeField] private float deceleration = 35f;

        [Header("Grounding")]
        [SerializeField] private float gravity = -20f;
        [SerializeField] private float groundCheckRadius = 0.2f;
        [SerializeField] private float groundCheckOffset = 0.05f;
        [SerializeField] private LayerMask groundMask = ~0;
        [SerializeField] private float maxSlopeAngle = 50f;

        [Header("Jump")]
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private bool useAdvancedJump = false;
        [SerializeField] private float fallMultiplier = 2.2f;
        [SerializeField] private float lowJumpMultiplier = 2f;

        // Estado deseado
        private Vector3 _desiredPlanarWorld = Vector3.zero;
        private bool _wantsRun;
        private bool _jumpHeld;

        // Interno
        private Rigidbody rb;
        private CapsuleCollider capsule;
        [SerializeField] private bool _isGrounded;
        private float _verticalVel;

        // Impulsos externos
        private Vector3 _externalPlanarImpulse = Vector3.zero;

        // --- Métodos públicos ---
        public void SetDesiredPlanarWorld(Vector3 worldPlanar)
        {
            _desiredPlanarWorld = new Vector3(worldPlanar.x, 0f, worldPlanar.z);
            if (_desiredPlanarWorld.sqrMagnitude > 1e-6f)
                _desiredPlanarWorld.Normalize();
        }

        public void SetRun(bool run) => _wantsRun = run;

        public void SetJumpHeld(bool held) => _jumpHeld = held;

        public void AddExternalImpulse(Vector3 worldPlanarImpulse)
        {
            worldPlanarImpulse.y = 0f;
            _externalPlanarImpulse += worldPlanarImpulse;
        }

        public void ClearExternalImpulse() => _externalPlanarImpulse = Vector3.zero;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            capsule = GetComponent<CapsuleCollider>();

            rb.useGravity = false; // gravedad manual
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        private void FixedUpdate()
        {
            GroundCheck();

            // Velocidad objetivo (planar)
            float targetSpeed = walkSpeed * (_wantsRun ? runMultiplier : 1f);
            Vector3 inputPlanar = _desiredPlanarWorld;

            // Añadir impulsos externos (se desvanecen suavemente)
            if (_externalPlanarImpulse.sqrMagnitude > 1e-6f)
            {
                inputPlanar += _externalPlanarImpulse;
                _externalPlanarImpulse = Vector3.MoveTowards(_externalPlanarImpulse, Vector3.zero, 10f * Time.fixedDeltaTime);
            }

            if (inputPlanar.sqrMagnitude > 1e-6f)
                inputPlanar.Normalize();

            Vector3 targetPlanarVel = inputPlanar * targetSpeed;

            // Velocidad actual
            Vector3 v = rb.linearVelocity;
            Vector3 planarVel = new Vector3(v.x, 0f, v.z);

            float accel = (inputPlanar.sqrMagnitude > 1e-6f) ? acceleration : deceleration;
            planarVel = Vector3.MoveTowards(planarVel, targetPlanarVel, accel * Time.fixedDeltaTime);

            ApplyGravity();

            rb.linearVelocity = new Vector3(planarVel.x, _verticalVel, planarVel.z);
        }

        private void ApplyGravity()
        {
            if (_isGrounded && _verticalVel < 0f)
            {
                _verticalVel = -2f; // “pegado” al suelo
                return;
            }

            float gravityMultiplier = 1f;

            if (useAdvancedJump)
            {
                if (_verticalVel < 0f)
                {
                    gravityMultiplier = fallMultiplier;
                }
                else if (_verticalVel > 0f && !_jumpHeld)
                {
                    gravityMultiplier = lowJumpMultiplier;
                }
            }

            _verticalVel += gravity * gravityMultiplier * Time.fixedDeltaTime;
        }

        private void GroundCheck()
        {
            Vector3 origin = transform.position + Vector3.up * capsule.center.y;
            float castDistance = (capsule.height * 0.5f) - capsule.radius + groundCheckOffset;

            _isGrounded = Physics.SphereCast(
                origin,
                groundCheckRadius,
                Vector3.down,
                out RaycastHit hit,
                castDistance,
                groundMask,
                QueryTriggerInteraction.Ignore
            );

            if (_isGrounded && Vector3.Angle(hit.normal, Vector3.up) > maxSlopeAngle)
                _isGrounded = false;
        }

        public void Jump()
        {
            if (!_isGrounded) return;

            _verticalVel = jumpForce;
            _isGrounded = false;
        }

        public Vector3 CurrentVelocity => rb != null ? rb.linearVelocity : Vector3.zero;
        public bool IsGrounded => _isGrounded;

        // --- GIZMOS DE DEPURACIÓN ---
        private void OnDrawGizmosSelected()
        {
            if (capsule == null)
                capsule = GetComponent<CapsuleCollider>();

            Vector3 origin = transform.position + Vector3.up * capsule.center.y;
            float castDistance = (capsule.height * 0.5f) - capsule.radius + groundCheckOffset;

            Gizmos.color = _isGrounded ? Color.green : Color.red;
            Gizmos.DrawLine(origin, origin + Vector3.down * castDistance);
            Gizmos.DrawWireSphere(origin, groundCheckRadius);
            Gizmos.DrawWireSphere(origin + Vector3.down * castDistance, groundCheckRadius);

#if UNITY_EDITOR
            UnityEditor.Handles.color = Color.yellow;
            UnityEditor.Handles.Label(
                origin + Vector3.down * (castDistance * 0.5f),
                $"GroundOffset: {groundCheckOffset:F2}\nGrounded: {_isGrounded}\nAdvancedJump: {useAdvancedJump}"
            );
#endif
        }
    }
}