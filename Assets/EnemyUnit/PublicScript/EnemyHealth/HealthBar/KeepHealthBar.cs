using UnityEngine;

//工具脚本：防止血条跟着建模一起转的
public class KeepHealthBar : MonoBehaviour
{
    private Quaternion initialRotation;

    private void Start()
    {
        // 记录初始旋转（一般是0,0,0）
        initialRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        // 每帧把旋转重置回初始值，父对象转它也不转
        transform.rotation = initialRotation;
    }
}
