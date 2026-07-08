using UnityEngine;

public class PetFollow : MonoBehaviour
{
    [Header("跟随目标")]
    public Transform followTarget;

    [Header("移动参数")]
    public float moveSpeed = 5f;
    public float rotateSpeed = 540f;

    [Header("距离控制")]
    public float startFollowDistance = 0.8f;   // 超过这个距离才开始追
    public float stopDistance = 0.45f;         // 靠近到这个距离就停

    private bool isFollowing = false;

    void Update()
    {
        if (followTarget == null) return;

        Vector3 targetPos = followTarget.position;
        targetPos.y = transform.position.y;

        Vector3 toTarget = targetPos - transform.position;
        float distance = toTarget.magnitude;

        if (!isFollowing && distance > startFollowDistance)
        {
            isFollowing = true;
        }

        if (isFollowing && distance <= stopDistance)
        {
            isFollowing = false;
            return;
        }

        if (!isFollowing) return;

        Vector3 moveDir = toTarget.normalized;
        moveSpeed = PlayerAgentMove.Instance.currentSpeed;
        float moveStep = Mathf.Min(moveSpeed * Time.deltaTime, distance - stopDistance);

        transform.position += moveDir * moveStep;

        if (moveDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotateSpeed * Time.deltaTime
            );
        }
    }

    private void OnMouseDown()
    {
        TitleEventReporter.ReportPetInteraction();
    }
}
