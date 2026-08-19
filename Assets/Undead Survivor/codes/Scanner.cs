using UnityEngine;

public class Scanner : MonoBehaviour
{
    public float scanRange;

    // 🚨 [완벽 수정] 인스펙터 창에서 숫자를 쳐서 칸수를 늘릴 수 있고, 
    // 늘어난 각 칸마다 마우스 클릭으로 레이어 이름을 선택할 수 있는 전용 레이어마스크 배열 시스템!
    public LayerMask[] targetLayers;

    public RaycastHit2D[] targets;
    public Transform nearestTarget;

    void FixedUpdate()
    {
        // 인스펙터 배열 칸에 등록한 여러 개의 레이어 이름들을 하나의 물리 필터로 병합합니다.
        int finalLayerMask = 0;
        if (targetLayers != null)
        {
            foreach (LayerMask mask in targetLayers)
            {
                finalLayerMask |= mask.value;
            }
        }

        targets = Physics2D.CircleCastAll(transform.position, scanRange, Vector2.zero, 0, finalLayerMask);
        nearestTarget = GetNearest();
    }

    Transform GetNearest()
    {
        Transform result = null;
        float diff = 100;

        if (targets != null)
        {
            foreach (RaycastHit2D target in targets)
            {
                if (target.transform == null) continue;

                Vector3 myPos = transform.position;
                Vector3 targetPos = target.transform.position;
                float curDiff = Vector3.Distance(myPos, targetPos);

                if (curDiff < diff)
                {
                    diff = curDiff;
                    result = target.transform;
                }
            }
        }

        return result;
    }
}
