using UnityEngine;

public class CerberusBodyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("보스 몸체 기본 데미지")]
    public float baseBodyDamage = 10f;

    [Tooltip("보스 몸체 안에서 딜이 들어가는 주기 (초 단위)")]
    public float tickRate = 0.5f;

    private float damageTimer = 0f;

    // 1. 처음 몸에 닿는 순간 즉시 딜
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null)
            {
                player.TakeBossBodyDamage(baseBodyDamage);
            }
            damageTimer = 0f;
        }
    }

    // 2. 몸 안에서 비비고 있을 때 tickRate 주기마다 딜
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            damageTimer += Time.deltaTime;

            if (damageTimer >= tickRate)
            {
                PlayerHealth player = other.GetComponent<PlayerHealth>();
                if (player != null)
                {
                    player.TakeBossBodyDamage(baseBodyDamage);
                }
                damageTimer = 0f;
            }
        }
    }

    // 3. 나가면 타이머 초기화
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            damageTimer = 0f;
        }
    }
}