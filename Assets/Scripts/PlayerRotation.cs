using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [Header("Camera Settings")]
    [SerializeField] private Transform f_orientation;
    [SerializeField] private Transform f_player;
    [SerializeField] private Transform f_playerObject;
    [SerializeField] private Rigidbody f_rigidbody;
    [SerializeField] private float f_rotationSpeed = 5f;

    [SerializeField] private float f_mouseSensitivity = 2f; // 新增：鼠标灵敏度
    [SerializeField] private float f_topClamp = 90f;
    [SerializeField] private float f_bottomClamp = -90f;

    private PlayerInputActions _controls;
    private float _xRotation = 0f;
    private Camera _mainCamera;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _controls = new PlayerInputActions();
        _controls.Enable();

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        HandleCameraRotation();
        HandlePlayerRotation();
    }

    private void HandleCameraRotation()
    {
        Vector2 lookInput = _controls.PlayerInputs.View.ReadValue<Vector2>();

        float mouseX = lookInput.x * f_mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * f_mouseSensitivity * Time.deltaTime;

        // 上下旋转（相机）
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, f_bottomClamp, f_topClamp);

        if (_mainCamera != null)
        {
            _mainCamera.transform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        }

        // 左右旋转（玩家和方向）
        f_player.Rotate(Vector3.up * mouseX);
        f_orientation.Rotate(Vector3.up * mouseX);
    }

    private void HandlePlayerRotation()
    {
        Vector3 viewDir = f_player.position - new Vector3(_mainCamera.transform.position.x, f_player.position.y, _mainCamera.transform.position.z);
        f_orientation.forward = viewDir.normalized;

        Vector2 moveInput = _controls.PlayerInputs.Move.ReadValue<Vector2>();
        Vector3 input = f_orientation.forward * moveInput.y + f_orientation.right * moveInput.x;

        if (input != Vector3.zero)
            f_playerObject.forward = Vector3.Slerp(f_playerObject.forward, input.normalized, Time.deltaTime * f_rotationSpeed);
    }

    private void OnDisable()
    {
        _controls?.Disable();
    }
}