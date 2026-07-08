using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerAgentMove : MonoBehaviour
{
    public static PlayerAgentMove Instance;

    [Header("动画")]
    public Animator animator;

    [Header("移动")]
    public float moveSpeed = 3.5f;
    public float accelerateMultiplier = 2f;
    public float rotateSpeed = 720f;

    [Header("重力")]
    public float gravity = -9.81f;
    public float groundedStick = -1f;

    private CharacterController cc;
    private Joystick joystick;

    [HideInInspector] public bool isAccelerating = false;
    public bool isBikeMode = false;
    public float currentSpeed;

    private float verticalVelocity;

    void Awake()
    {
        Instance = this;
        cc = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();
    }

    void Start()
    {
        joystick = ControlBtnManager.Instance.moveJoystick;

        if (animator != null)
        {
            animator.applyRootMotion = false;
            animator.enabled = false;   // 开局先不播放动画
        }
    }

    void Update()
    {
#if UNITY_EDITOR
        if (!isBikeMode)   // ★ 自行车模式禁止加速
        {
            if (Input.GetKey(KeyCode.Space))
                isAccelerating = true;
            else if (Input.GetKeyUp(KeyCode.Space))
                isAccelerating = false;
        }
#endif

        if (isBikeMode)
            isAccelerating = false;

        if (joystick == null) return;

        Vector2 input = joystick.Direction;
        bool isMoving = input.sqrMagnitude > 0.01f;

        if (animator != null)
            animator.enabled = isMoving;

        // 速度
        float finalSpeed = isAccelerating ? moveSpeed * accelerateMultiplier : moveSpeed;
        currentSpeed = isMoving ? finalSpeed : 0f;

        // 水平移动向量（世界坐标）
        Vector3 moveDir = Vector3.zero;
        if (isMoving)
            moveDir = new Vector3(input.x, 0f, input.y).normalized;

        // 转向
        if (isMoving)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }

        // 重力
        if (cc.isGrounded && verticalVelocity < 0f)
            verticalVelocity = groundedStick;

        verticalVelocity += gravity * Time.deltaTime;

        // 合成位移
        Vector3 velocity = moveDir * currentSpeed;
        velocity.y = verticalVelocity;

        cc.Move(velocity * Time.deltaTime);
    }
}