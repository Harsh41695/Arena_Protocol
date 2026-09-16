using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : NetworkBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 15f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -20f;

    private CharacterController characterController;
    private Camera mainCamera;

    private float verticalVelocity;
    private bool hasFocus;
    private PlayerHealth playerHealth;
    private void Awake()
    {
        characterController =
            GetComponent<CharacterController>();

        playerHealth =
            GetComponent<PlayerHealth>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
            return;

        mainCamera = Camera.main;

        hasFocus = Application.isFocused;
    }

    private void OnApplicationFocus(bool focus)
    {
        hasFocus = focus;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (playerHealth != null &&
            playerHealth.IsDead.Value)
            return;

        if (!hasFocus)
            return;

        HandleRotation();
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection =
            transform.forward * vertical +
            transform.right * horizontal;

        if (moveDirection.sqrMagnitude > 1f)
            moveDirection.Normalize();

        if (characterController.isGrounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = moveDirection * moveSpeed;
        velocity.y = verticalVelocity;

        characterController.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (mainCamera == null)
            return;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        Plane groundPlane =
            new Plane(Vector3.up, transform.position);

        if (!groundPlane.Raycast(ray, out float distance))
            return;

        Vector3 hitPoint = ray.GetPoint(distance);

        Vector3 lookDirection =
            hitPoint - transform.position;

        lookDirection.y = 0f;

        if (lookDirection.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation =
            Quaternion.LookRotation(lookDirection.normalized);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    [Rpc(SendTo.Owner)]
    public void SetSpawnPositionRpc(Vector3 position)
    {
        if (characterController != null)
            characterController.enabled = false;

        transform.position = position;

        if (characterController != null)
            characterController.enabled = true;
    }
}