using System.Collections.Generic;
using UnityEngine;

public class CerberusBodyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("체크 해제 시 부모(Boss)의 damage 값을 자동으로 가져옵니다.")]
    public bool useManualDamage = false;

    [Tooltip("수동 설정 데미지")]
    public float baseBodyDamage = 10f;

    [Tooltip("보스 몸체 안에서 딜이 들어가는 주기 (초 단위)")]
    public float tickRate = 0.5f;

    private Boss bossScript;
    // 플레이어가 여러 개의 콜라이더(자식 오브젝트 등)를 가질 때 타이머가 꼬이는 현상을 방지하기 위한 딕셔너리
    private Dictionary<Collider2D, float> activeColliders = new Dictionary<Collider2D, float>();

    void Awake()
    {
        // 부모 오브젝트에서 Boss 스크립트를 찾아옵니다.
        bossScript = GetComponentInParent<Boss>();
    }

    // 부모 Boss 스크립트의 데미지를 쓸지, 수동 데미지를 쓸지 결정하는 함수
    float GetCurrentDamage()
    {
        if (!useManualDamage && bossScript != null)
        {
            return bossScript.damage; // 부모 보스 스크립트의 데미지 연동!
        }
        return baseBodyDamage;
    }

    // 1. 처음 몸에 닿는 순간 즉시 딜
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // CerberusBodyHitbox.cs 의 OnTriggerEnter2D 내부
            if (bossScript != null && bossScript.currentState == Boss.BossState.Appearance) return;

            PlayerHealth player = other.GetComponentInParent<PlayerHealth>();
            if (player != null)
            {
                player.TakeBossBodyDamage(GetCurrentDamage());
                activeColliders[other] = Time.time + tickRate;
            }
        }
    }

    // 2. 몸 안에서 비비고 있을 때 tickRate 주기마다 딜
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && activeColliders.ContainsKey(other))
        {
            // 보스가 등장 중(Appearance)일 때는 데미지를 주지 않음
            if (bossScript != null && bossScript.currentState == Boss.BossState.Appearance) return;

            // 지정된 주기가 지났는지 확인
            if (Time.time >= activeColliders[other])
            {
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeBossBodyDamage(GetCurrentDamage());
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