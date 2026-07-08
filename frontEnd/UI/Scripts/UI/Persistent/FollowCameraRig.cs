using UnityEngine;

public class FollowCameraRig : MonoBehaviour
{
    public Transform cameraRig;

    // 新增：相机相对 CameraPivot 的偏移
    public Vector3 offset = new Vector3(0f, 0.200003f, 1f);

    // 新增：相机相对 CameraPivot 的旋转偏移（欧拉角）
    public Vector3 eulerOffset = new Vector3(10f, 0f, 0f);

    void Start()
    {
        if (cameraRig == null)
        {
            var rigObj = GameObject.FindWithTag("CameraRig");
            if (rigObj != null)
                cameraRig = rigObj.transform;
        }

        if (cameraRig != null)
        {
            transform.position = cameraRig.TransformPoint(offset);
            transform.rotation = cameraRig.rotation * Quaternion.Euler(eulerOffset);
        }
    }

    void LateUpdate()
    {
        if (cameraRig == null)
        {
            var rigObj = GameObject.FindWithTag("CameraRig");
            if (rigObj != null)
                cameraRig = rigObj.transform;
            else
                return;
        }

        transform.position = cameraRig.TransformPoint(offset);
        transform.rotation = cameraRig.rotation * Quaternion.Euler(eulerOffset);
    }
}
