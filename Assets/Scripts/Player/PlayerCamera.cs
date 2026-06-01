using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [Header("Mouse")]
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float adsMultiplier = 0.4f;
    [SerializeField] private float maxLookAngle = 80f;

    [Header("Recoil")]
    [SerializeField] private float recoilReturnSpeed = 12f;

    [SerializeField] private Transform playerBody;

    private float xRotation;
    private float recoilX;
    private float recoilY;
    private bool isADS;

    public float Sensitivity
    {
        get => mouseSensitivity;
        set => mouseSensitivity = Mathf.Clamp(value, 0.1f, 10f);
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Sensitivity = PlayerPrefs.GetFloat("Sensitivity", 2f);
    }

    private void Update()
    {
        float mult = isADS ? adsMultiplier : 1f;
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * mult;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * mult;

        recoilX = Mathf.Lerp(recoilX, 0, recoilReturnSpeed * Time.deltaTime);
        recoilY = Mathf.Lerp(recoilY, 0, recoilReturnSpeed * Time.deltaTime);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation + recoilX, -maxLookAngle, maxLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * (mouseX + recoilY));
    }

    public void AddRecoil(float vertical, float horizontal)
    {
        recoilX -= vertical;
        recoilY += Random.Range(-horizontal, horizontal);
    }

    public void SetADS(bool ads) => isADS = ads;
}
