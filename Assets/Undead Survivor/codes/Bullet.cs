using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int id;
    public float damage;
    public int per;

    Rigidbody2D rigid;


    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }


    // =========================================================
    // 💥 최종 피해 계산
    // =========================================================

    float CalculateFinalDamage(Boss boss, float baseDamage)
    {
        float finalDamage = baseDamage;


        // =====================================================
        // 🔫 모든 피해 증가
        // =====================================================
        //
        // 예:
        // 모든 피해 +1% → 100 피해가 101 피해
        // 모든 피해 +10% → 100 피해가 110 피해
        //
        // 이 효과는 관통력(per)과 완전히 별개다.
        // =====================================================

        if (Item.AllDamageBonusRate > 0f)
        {
            float bonusDamage =
                finalDamage *
                Item.AllDamageBonusRate;

            finalDamage += bonusDamage;

            Debug.Log(
                $"🔫 [모든 피해 증가] " +
                $"기본 피해: {baseDamage:F1} | " +
                $"추가 피해: {bonusDamage:F1} | " +
                $"증가율: " +
                $"{Item.AllDamageBonusRate * 100f:F1}% | " +
                $"현재 피해: {finalDamage:F1}"
            );
        }


        // =====================================================
        // 🌟 희망의 호프
        // 대상 최대 체력의 일정 % 추가 피해
        // =====================================================

        if (Item.HopeOfHopeRate > 0f &&
            boss != null)
        {
            float maxHpDamage =
                boss.GetMaxHealth() *
                Item.HopeOfHopeRate;

            finalDamage += maxHpDamage;

            Debug.Log(
                $"🌟 [희망의 호프] " +
                $"기본 피해: {baseDamage:F1} | " +
                $"최대 HP 추가 피해: {maxHpDamage:F1} | " +
                $"최종 피해: {finalDamage:F1}"
            );
        }


        return finalDamage;
    }


    // =========================================================
    // 🩸 블러드 히트
    // 모든 피해의 일정 % 흡혈
    // =========================================================

    void ApplyBloodHit(float finalDamage)
    {
        if (Item.BloodHitRate <= 0f)
            return;


        PlayerHealth playerHealth =
            Object.FindFirstObjectByType<PlayerHealth>();

        if (playerHealth == null)
            return;


        float healAmount =
            finalDamage *
            Item.BloodHitRate;


        if (healAmount <= 0f)
            return;


        playerHealth.Heal(healAmount);


        Debug.Log(
            $"🩸 [블러드 히트] " +
            $"피해: {finalDamage:F1} | " +
            $"흡혈률: {Item.BloodHitRate * 100f:F1}% | " +
            $"회복: {healAmount:F1}"
        );
    }


    // =========================================================
    // 🎯 보스에게 최종 피해 적용
    // =========================================================

    void DamageBoss(Boss boss, float baseDamage)
    {
        if (boss == null)
            return;


        float finalDamage =
            CalculateFinalDamage(
                boss,
                baseDamage
            );


        boss.TakeDamage(finalDamage);


        // 블러드 히트
        ApplyBloodHit(finalDamage);
    }


    // =========================================================
    // 🚨 확정 타격 시스템
    // =========================================================

    void Update()
    {
        Boss targetBoss =
            Object.FindFirstObjectByType<Boss>();


        if (targetBoss != null)
        {
            float distance =
                Vector2.Distance(
                    transform.position,
                    targetBoss.transform.position
                );


            if (distance <= 2.5f)
            {
                float finalWeaponDamage =
                    damage <= 0f ?
                    80f :
                    damage;


                DamageBoss(
                    targetBoss,
                    finalWeaponDamage
                );


                Debug.Log(
                    $"<color=yellow>" +
                    $"[최종 치트 타격]" +
                    $"</color> " +
                    $"보스 발견! 거리: " +
                    $"{distance:F2}m | " +
                    $"기본 데미지: " +
                    $"{finalWeaponDamage:F1}"
                );


                // =================================================
                // 관통력은 기존대로 유지
                // =================================================

                if (id != 0 && id != 5)
                {
                    per--;

                    if (per < 0)
                    {
                        if (rigid != null)
                            rigid.linearVelocity =
                                Vector2.zero;

                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }


    // =========================================================
    // 🔫 총알 초기화
    // =========================================================

    public void Init(
        float damage,
        int per,
        Vector3 dir,
        int id = 0)
    {
        this.damage = damage;
        this.per = per;
        this.id = id;


        if (per >= 0)
        {
            rigid.linearVelocity =
                dir * 15f;
        }
    }


    // =========================================================
    // 💥 충돌
    // =========================================================

    void OnTriggerEnter2D(
        Collider2D collision)
    {
        // =====================================================
        // 🚨 부모에서 Boss 컴포넌트 직접 찾기
        // =====================================================

        Boss directBossScript =
            collision.GetComponentInParent<Boss>();


        if (directBossScript != null)
        {
            DamageBoss(
                directBossScript,
                damage
            );


            Debug.Log(
                $"<color=cyan>" +
                $"[스크립트 강제 타격]" +
                $"</color> " +
                $"보스에게 {damage:F1} " +
                $"기본 피해 전달 성공!"
            );


            if (id != 0 && id != 5)
            {
                per--;


                if (per < 0)
                {
                    if (rigid != null)
                        rigid.linearVelocity =
                            Vector2.zero;

                    gameObject.SetActive(false);
                }
            }


            return;
        }


        // =====================================================
        // Enemy / Boss가 아니면 무시
        // =====================================================

        if ((!collision.CompareTag("Enemy") &&
             !collision.CompareTag("Boss")) ||
            per == -100)
        {
            return;
        }


        // =====================================================
        // Boss
        // =====================================================

        if (collision.CompareTag("Boss"))
        {
            Boss bossScript =
                collision.GetComponent<Boss>();


            if (bossScript != null)
            {
                DamageBoss(
                    bossScript,
                    damage
                );
            }
        }


        // =====================================================
        // 무한 관통
        // =====================================================

        if (id == 0 || id == 5)
            return;


        per--;


        if (per < 0)
        {
            if (rigid != null)
                rigid.linearVelocity =
                    Vector2.zero;

            gameObject.SetActive(false);
        }
    }


    // =========================================================
    // Area 밖으로 나가면 삭제
    // =========================================================

    void OnTriggerExit2D(
        Collider2D collision)
    {
        if (!collision.CompareTag("Area") ||
            per == -100)
        {
            return;
        }


        gameObject.SetActive(false);
    }
}