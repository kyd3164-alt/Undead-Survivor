using System.Collections;
using UnityEngine;

public class Boss2AuraSkill : MonoBehaviour
{
    [Header("범위 설정")]
    [Tooltip("오라가 영향을 주는 범위")]
    public float radius = 3f;

    [Tooltip("오라 지속 시간")]
    public float duration = 5f;

    [Header("피해 설정")]
    [Tooltip("플레이어에게 초당 주는 피해")]
    public float damagePerSecond = 10f;

    [Header("회복 설정")]
    [Tooltip("Boss2가 초당 회복하는 체력")]
    public float healPerSecond = 5f;

    [Header("판정 설정")]
    [Tooltip("피해/회복 판정 간격")]
    public float tickInterval = 1f;

    private Boss boss;

    private void Start()
    {
        boss = GetComponentInParent<Boss>();

        if (boss == null)
        {
            Debug.LogWarning(
                "Boss2AuraSkill: 부모에서 Boss 스크립트를 찾지 못했습니다."
            );
        }

        StartCoroutine(AuraRoutine());
    }

    IEnumerator AuraRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            ApplyAuraEffect();

            yield return new WaitForSeconds(tickInterval);

            elapsed += tickInterval;
        }

        Destroy(gameObject);
    }

    void ApplyAuraEffect()
    {
        // ==========================================
        // 1. Boss2 회복
        // ==========================================

        if (boss != null && healPerSecond > 0f)
        {
            boss.HealFromAura(healPerSecond);
        }

        // ==========================================
        // 2. 주변 플레이어 공격
        // ==========================================

        if (GameManager.instance == null ||
            GameManager.instance.player == null)
        {
            return;
        }

        Transform player =
            GameManager.instance.player.transform;

        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );

        if (distance <= radius)
        {
            PlayerHealth playerHealth =
                player.GetComponent<PlayerHealth>();

            if (playerHealth != null && damagePerSecond > 0f)
            {
                playerHealth.TakeBossBodyDamage(
                    damagePerSecond
                );
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            transform.position,
            radius
        );
    }
}