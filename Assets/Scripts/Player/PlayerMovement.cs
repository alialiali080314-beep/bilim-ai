using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 8f;
    [SerializeField] private float crouchSpeed = 2.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -19.62f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Crouch")]
    [SerializeField] private float standHeight = 2f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float crouchTransitionSpeed = 10f;

    [Header("Head Bob")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float bobFrequency = 10f;
    [SerializeField] private float bobAmplitude = 0.05f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching;
    private bool isSprinting;
    private float bobTimer;
    private Vector3 originalCameraPos;

    public bool IsGrounded => isGrounded;
    public bool IsCrouching => isCrouching;
    public bool IsSprinting => isSprinting;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraHolder != null)
            originalCameraPos = cameraHolder.localPosition;
    }

    private void Update()
    {
        CheckGround();
        HandleCrouch();
        Move();
        HeadBob();
    }

    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;
    }

    private void HandleCrouch()
    {
        bool wantsCrouch = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.C);

        if (wantsCrouch && !isCrouching)
        {
            isCrouching = true;
        }
        else if (!wantsCrouch && isCrouching)
        {
            // Only stand up if there's clearance
            if (!Physics.SphereCast(transform.position, controller.radius, Vector3.up, out _, standHeight - crouchHeight + 0.1f))
                isCrouching = false;
        }

        float targetHeight = isCrouching ? crouchHeight : standHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, crouchTransitionSpeed * Time.deltaTime);
        controller.center = new Vector3(0, controller.height / 2f, 0);
    }

    private void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        isSprinting = Input.GetKey(KeyCode.LeftShift) && z > 0 && !isCrouching && isGrounded;

        float speed = isCrouching ? crouchSpeed : (isSprinting ? sprintSpeed : walkSpeed);

        Vector3 move = Vector3.ClampMagnitude(transform.right * x + transform.forward * z, 1f);
        controller.Move(move * speed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HeadBob()
    {
        if (cameraHolder == null) return;

        bool moving = controller.velocity.magnitude > 0.1f && isGrounded;

        if (moving)
        {
            bobTimer += Time.deltaTime * bobFrequency * (isSprinting ? 1.5f : 1f);
            float mul = isCrouching ? 0.5f : (isSprinting ? 1.5f : 1f);
            Vector3 offset = new Vector3(
                Mathf.Cos(bobTimer) * bobAmplitude * mul,
                Mathf.Sin(bobTimer * 2f) * bobAmplitude * mul,
                0);
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, originalCameraPos + offset, Time.deltaTime * 10f);
        }
        else
        {
            bobTimer = 0;
            cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, originalCameraPos, Time.deltaTime * 10f);
        }
    }
}
