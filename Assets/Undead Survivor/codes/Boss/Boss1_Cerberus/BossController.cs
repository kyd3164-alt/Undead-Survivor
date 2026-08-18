using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private GameObject lavaFloorPrefab; // 용암 바닥 프리팹
    [SerializeField] private Transform groundCheckPoint; // 착지 지점 (보스 발밑 위치)

    // 애니메이션 이벤트에서 호출할 함수
    public void OnLandAndSpawnLava()
    {
        if (lavaFloorPrefab != null && groundCheckPoint != null)
        {
            // 보스의 발밑 위치에 용암 바닥 생성
            Instantiate(lavaFloorPrefab, groundCheckPoint.position, Quaternion.identity);
        }
    }
}