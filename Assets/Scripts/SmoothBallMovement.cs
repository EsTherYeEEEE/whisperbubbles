using UnityEngine;

public class SmoothBallMovement : MonoBehaviour
{
    public Transform target; // 目标物体
    public float moveSpeed = 3.0f; // 移动速度
    public float maxFloatDistance = 1.0f; // 最大浮动距离
    public float attractionDistance = 1.0f; // 吸引距离
    public float arriveDistance = 0.1f; // 吸引距离
    public float floatSpeed = 1.0f; // 浮动速度

    void Start()
    {
        //initialPosition = transform.position;
        //randomOffset = Random.insideUnitSphere * maxFloatDistance;
        //randomOffsetMagnitude = randomOffset.magnitude;
    }

    float Remap(float x, float t1, float t2, float s1, float s2)
    {
        var cc = (s2 - s1) / (t2 - t1) * (x - t1) + s1;
        Mathf.Clamp(cc, s1, s2);
        return cc;
        // return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
    }

    void Update()
    {
        if (target != null)
        {
            // 计算物体到目标的距离
            float distanceToTarget = Vector3.Distance(transform.position, target.position);

            Debug.Log($"distance to target: {distanceToTarget}");
            // 如果距离小于吸引距离，停止运动和浮动
            if (distanceToTarget <= arriveDistance)
            {
                return;
            }
            else
            {
                // 计算移动方向
                Vector3 directionToTarget = (target.position - transform.position).normalized;

                // 移动到目标位置
                transform.position += directionToTarget * moveSpeed * Time.deltaTime;
            }


        }
    }
}
