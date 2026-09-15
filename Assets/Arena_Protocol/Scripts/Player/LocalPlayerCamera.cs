using Unity.Netcode;
using UnityEngine;

public class LocalPlayerCamera : NetworkBehaviour
{
    [Header("Position")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed = 10f;

    [Header("Rotation")]
    [Range(20f, 80f)]
    [SerializeField] private float xRotation = 55f;

    [SerializeField] private float yRotation = 0f;

    private Camera mainCamera;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            enabled = false;
            return;
        }

        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null)
            return;

        // Follow player
        Vector3 targetPosition = transform.position + offset;

        mainCamera.transform.position = Vector3.Lerp(
            mainCamera.transform.position,
            targetPosition,
            followSpeed * Time.deltaTime
        );

        // Manual camera rotation
        mainCamera.transform.rotation =
            Quaternion.Euler(xRotation, yRotation, 0f);
    }
}