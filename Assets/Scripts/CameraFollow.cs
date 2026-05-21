using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -6f);

    [Header("Follow Settings")]
    [SerializeField] private float smoothTime = 0.05f;
    [SerializeField] private float maxSpeed = 100f;

    [Header("Snap Distance")]
    [SerializeField] private float snapDistance = 2f;

    [Header("Shake")]
    [SerializeField] private float maxShake = 1.5f;

    [Header("Landing Shake (DEBUG)")]
    [SerializeField] private bool enableLandingShake = true;
    [SerializeField] private float landingShakeThreshold = -8f; // velocidad mínima para considerar caída
    [SerializeField] private float landingShakeMagnitude = 0.8f;
    [SerializeField] private float landingShakeDuration = 0.15f;

    private Vector3 velocity;

    // SHAKE
    private float shakeTimer;
    private float shakeDuration;
    private float shakeMagnitude;
    private Vector3 shakeOffset;

    // LANDING DETECTION
    private Rigidbody targetRb;
    private bool wasFalling;

    void Start()
    {
        if (target != null)
            targetRb = target.GetComponent<Rigidbody>();
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPos = target.position;
        Vector3 desiredPosition = targetPos + offset;

        float distance = Vector3.Distance(transform.position, desiredPosition);

        if (distance > snapDistance)
        {
            transform.position = desiredPosition;
            velocity = Vector3.zero;
        }
        else
        {
            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref velocity,
                smoothTime,
                maxSpeed
            );
        }

        // ===== LANDING SHAKE =====
        DetectLanding();

        // ===== SHAKE =====
        UpdateShake(Time.unscaledDeltaTime);

        transform.position += shakeOffset;
    }

    // ===== DETECTAR ATERRIZAJE =====
    void DetectLanding()
    {
        if (!enableLandingShake || targetRb == null) return;

        float verticalVel = targetRb.linearVelocity.y;

        // estaba cayendo
        if (verticalVel < landingShakeThreshold)
        {
            wasFalling = true;
        }

        // aterrizó (velocidad cercana a 0 después de caer)
        if (wasFalling && Mathf.Abs(verticalVel) < 0.1f)
        {
            Shake(landingShakeMagnitude, landingShakeDuration);
            wasFalling = false;
        }
    }

    // ===== SHAKE =====
    public void Shake(float magnitude, float duration)
    {
        shakeMagnitude = Mathf.Min(magnitude, maxShake);
        shakeDuration = duration;
        shakeTimer = duration;
    }

    private void UpdateShake(float dt)
    {
        if (shakeTimer <= 0f)
        {
            shakeOffset = Vector3.zero;
            return;
        }

        shakeTimer -= dt;

        float t = 1f - (shakeTimer / shakeDuration);
        float damping = Mathf.Lerp(1f, 0f, t);

        float currentMag = shakeMagnitude * damping;

        shakeOffset = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * currentMag;
    }
}