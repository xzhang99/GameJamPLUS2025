using UnityEngine;

public class PlayerRotation : MonoBehaviour
{
    [SerializeField] private Transform f_orientation;
    [SerializeField] private Transform f_player;
    [SerializeField] private Transform f_playerObject;
    [SerializeField] private Rigidbody f_rigidbody;
    [SerializeField] private float f_rotationSpeed;

    private PlayerInputActions _controls;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _controls = new PlayerInputActions();
        _controls.Enable();
    }

    private void Update()
    {
        Vector3 viewDir = f_player.position - new Vector3(transform.position.x, f_player.position.y, transform.position.z);
        f_orientation.forward = viewDir.normalized;

        Vector3 input = f_orientation.forward * _controls.PlayerInputs.Move.ReadValue<Vector2>().y + f_orientation.right * _controls.PlayerInputs.Move.ReadValue<Vector2>().x;

        if (input != Vector3.zero)
            f_playerObject.forward = Vector3.Slerp(f_playerObject.forward, input.normalized, Time.deltaTime * f_rotationSpeed);
    }
}
