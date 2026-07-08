using UnityEngine;

public class CameraOrbitByJoystick : MonoBehaviour
{
    public Transform target;        // 玩家
    public Transform pivot;         // 相机观察点（建议放玩家头顶/胸口）   // 右摇杆

    [Header("Orbit")]
    public float distance = 6f;
    public float height = 2f;

    [Header("Speed")]
    public float yawSpeed = 180f;
    public float pitchSpeed = 120f;

    [Header("Pitch Clamp")]
    public float minPitch = 5f;
    public float maxPitch = 60f;

    [Header("Collision")]
    public LayerMask collisionMask;     // 建议只勾 Ground / Building / Environment
    public float cameraRadius = 0.15f;
    public float collisionPadding = 0.2f;

    [Header("Smoothing")]
    public float posSmooth = 8f;
    public float rotSmooth = 8f;
    private Joystick lookJoystick;

    private float yaw;
    private float pitch;

    void Start()
    {
        lookJoystick = ControlBtnManager.Instance.viewJoystick;
        if (target == null) return;

        Vector3 center = pivot ? pivot.position : (target.position + Vector3.up * height);
        Vector3 offset = transform.position - center;

        if (offset.sqrMagnitude > 0.0001f)
        {
            distance = offset.magnitude;

            Vector3 flat = new Vector3(offset.x, 0f, offset.z);
            yaw = Mathf.Atan2(flat.x, flat.z) * Mathf.Rad2Deg;

            float horizontal = flat.magnitude;
            pitch = Mathf.Atan2(offset.y, horizontal) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        }
    }

    void LateUpdate()
    {
        if (target == null || lookJoystick == null) return;

        // 1. 输入
        Vector2 d = lookJoystick.Direction;
        yaw += d.x * yawSpeed * Time.deltaTime;
        pitch += -d.y * pitchSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // 2. 中心点
        Vector3 center = pivot ? pivot.position : (target.position + Vector3.up * height);

        // 3. 理想相机位置
        Quaternion orbitRot = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 desiredPos = center + orbitRot * new Vector3(0f, 0f, -distance);

        // 4. 碰撞修正
        Vector3 dir = desiredPos - center;
        float dist = dir.magnitude;

        if (dist > 0.0001f)
        {
            dir /= dist;

            if (Physics.SphereCast(center, cameraRadius, dir, out RaycastHit hit, dist, collisionMask, QueryTriggerInteraction.Ignore))
            {
                if (hit.transform != target && !hit.transform.IsChildOf(target))
                {
                    desiredPos = hit.point - dir * collisionPadding;
                }
                else
                {
                    Vector3 newStart = center + dir * 0.2f;
                    float newDist = Mathf.Max(0f, dist - 0.2f);

                    if (Physics.SphereCast(newStart, cameraRadius, dir, out RaycastHit hit2, newDist, collisionMask, QueryTriggerInteraction.Ignore))
                    {
                        if (hit2.transform != target && !hit2.transform.IsChildOf(target))
                        {
                            desiredPos = hit2.point - dir * collisionPadding;
                        }
                    }
                }
            }
        }

        // 5. 平滑移动
        float posT = 1f - Mathf.Exp(-posSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPos, posT);

        // 6. 平滑朝向
        Vector3 lookDir = center - transform.position;
        if (lookDir.sqrMagnitude > 0.0001f)
        {
            Quaternion desiredRot = Quaternion.LookRotation(lookDir.normalized, Vector3.up);
            float rotT = 1f - Mathf.Exp(-rotSmooth * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredRot, rotT);
        }
    }
}