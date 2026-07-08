using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMove : MonoBehaviour
{
    public Joystick joystick;      // 左下角摇杆
    public Transform cam;          // 主相机
    public float moveSpeed = 3.5f;
    public float rotateSpeed = 720f;
    public float gravity = -20f;

    private CharacterController cc;
    private Vector3 velocity;

    void Awake()
    {
        cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (joystick == null) return;

        Vector2 input = joystick.Direction;
        Vector3 move = new Vector3(input.x, 0f, input.y);

        if (cam != null)
        {
            Vector3 camForward = cam.forward;
            Vector3 camRight = cam.right;

            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            move = camForward * input.y + camRight * input.x;
        }

        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );

            cc.Move(move.normalized * moveSpeed * Time.deltaTime);
        }

        if (cc.isGrounded && velocity.y < 0)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        cc.Move(velocity * Time.deltaTime);
    }
}