using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Offset")]
    [SerializeField] private float distance = 6f;
    [SerializeField] private float height = 2f;

    [Header("Smooth")]
    [SerializeField] private float positionSmoothTime = 0.15f;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    private Vector3 currentVelocity;

    void LateUpdate()
    {
        if (target == null) return;

        // 🎯 Solo usamos la rotación Y del player
        float targetYaw = target.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0f, targetYaw, 0f);

        // 📍 Offset detrás del jugador
        Vector3 offset = targetRotation * new Vector3(0f, 0f, -distance);
        offset += Vector3.up * height;

        Vector3 desiredPosition = target.position + offset;

        // 🎥 Movimiento suave
        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref currentVelocity,
            positionSmoothTime
        );

        // 🔄 Rotación suave mirando al jugador
        Vector3 lookTarget = target.position + Vector3.up * 1.2f;

        Quaternion lookRotation = Quaternion.LookRotation(
            lookTarget - transform.position
        );

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            lookRotation,
            rotationSmoothSpeed * Time.deltaTime
        );
    }
}