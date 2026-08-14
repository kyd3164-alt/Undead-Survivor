using System.Collections.Generic;
using UnityEngine;

public class CerberusBodyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("보스 몸체 기본 데미지")]
    public float baseBodyDamage = 10f;

    [Tooltip("보스 몸체 안에서 딜이 들어가는 주기 (초 단위)")]
    public float tickRate = 0.5f;

    // 플레이어가 여러 개의 콜라이더(자식 오브젝트 등)를 가질 때 타이머가 꼬이는 현상을 방지하기 위한 딕셔너리
    private Dictionary<Collider2D, float> activeColliders = new Dictionary<Collider2D, float>();

    // 1. 처음 몸에 닿는 순간 즉시 딜
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeBossBodyDamage(baseBodyDamage);

                // 이 콜라이더에 대한 다음 딜 타이머 설정 (Time.time 기준)
                activeColliders[other] = Time.time + tickRate;
            }
        }
    }

    // 2. 몸 안에서 비비고 있을 때 tickRate 주기마다 딜
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activeColliders.ContainsKey(other))
        {
            // 지정된 주기가 지났는지 확인
            if (Time.time >= activeColliders[other])
            {
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeBossBodyDamage(baseBodyDamage);
                }

                // 다음 딜 타이머 갱신
                activeColliders[other] = Time.time + tickRate;
            }
        }
    }

    // 3. 영역을 나가면 해당 콜라이더 타이머 정리
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (activeColliders.ContainsKey(other))
            {
                activeColliders.Remove(other);
            }
        }
    }
}