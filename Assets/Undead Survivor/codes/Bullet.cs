using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int id;
    public float damage;
    public int per;
    public float lifeTime = 3f;

    Rigidbody2D rigid;

    // =========================================================
    // 일반 발사체용
    // =========================================================

    // 일반 발사체는 한 Bullet이 같은 Boss를
    // 여러 번 공격하지 못하도록 사용
    bool hitBoss = false;

    // =========================================================
    // 삽 / 낫용
    // =========================================================

    // 회전 속도에 따라 계산된 공격 간격
    float spinHitInterval = 0f;

    // 다음 공격이 가능한 시간
    float nextSpinHitTime = 0f;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // =========================================================
    // Bullet 초기화
    // =========================================================

    public void Init(float damage, int per, Vector3 dir, int id = 0, float spinHitInterval = 0f)
    {
        this.damage = damage;
        this.per = per;
        this.id = id;

        // 오브젝트 풀 재사용 시 반드시 초기화
        hitBoss = false;

        // 삽 / 낫 공격 간격
        this.spinHitInterval = spinHitInterval;

        // 즉시 공격 가능
        nextSpinHitTime = 0f;

        Debug.Log(
            $"<color=cyan>[Bullet Init]</color> " +
            $"ID: {id} | " +
            $"damage: {damage:F1} | " +
            $"per: {per} | " +
            $"SpinInterval: {spinHitInterval:F3}"
        );

        // 일반 발사체 이동
        if (per >= 0)
        {
            // 일반 원거리 발사체는 빠르게 움직이므로
            // 충돌을 놓치지 않도록 Continuous 사용
            rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            rigid.linearVelocity = dir * 15f;

            // ★ 추가: 3초(lifeTime) 후에 DisableBullet 함수를 강제로 실행해라!
            Invoke("DisableBullet", lifeTime);
        }
        else
        {
            // 삽 / 낫은 회전 위치에 있으므로
            // 이동하지 않음
            rigid.linearVelocity =
                Vector2.zero;
        }
    }

    // =========================================================
    // Trigger Enter
    // =========================================================

    void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(
            $"<color=white>[Bullet 충돌 확인]</color> " +
            $"Bullet: {gameObject.name} | " +
            $"대상: {collision.name} | " +
            $"Tag: {collision.tag}"
        );

        // =========================================================
        // Boss
        // =========================================================

        if (IsBoss(collision))
        {
            HitBoss(collision);
            return;
        }

        // =========================================================
        // Enemy
        // =========================================================

        if (collision.CompareTag("Enemy"))
        {
            Debug.Log(
                $"<color=red>[Bullet → Enemy]</color> " +
                $"Enemy: {collision.name} | " +
                $"damage: {damage:F1}"
            );

            // 삽 / 낫
            // 무한 관통이므로 Enemy 충돌로 사라지지 않음
            if (per == -100)
                return;

            // 기존 무한 관통 무기
            if (id == 0 || id == 5)
                return;

            per--;

            if (per < 0)
            {
                Debug.Log(
                    $"<color=orange>[Bullet 제거]</color> " +
                    $"Enemy 피격 후 Bullet 비활성화"
                );

                DisableBullet();
            }

            return;
        }

        // =========================================================
        // Area
        // =========================================================

        if (collision.CompareTag("Area") && per != -100)
        {
            // 엽총 같은 일반 원거리 무기(id가 0, 5가 아닌 것)는 무시하고 통과
            if (id != 0 && id != 5)
                return;

            Debug.Log(
                $"<color=magenta>[⚠ Bullet → Area]</color> " +
                $"Bullet이 Area와 충돌해서 제거됨"
            );

            DisableBullet();
        }
    }

    // =========================================================
    // Trigger Stay
    //
    // 삽 / 낫이 Boss 안에 계속 들어가 있을 때
    // 회전 속도에 따라 다시 공격하기 위해 필요
    // =========================================================

    void OnTriggerStay2D(Collider2D collision)
    {
        if (!IsBoss(collision))
            return;

        // 삽 / 낫만 사용
        if (id != 0 && id != 5)
            return;

        HitBoss(collision);
    }

    // =========================================================
    // Boss Tag 확인
    // =========================================================

    bool IsBoss(Collider2D collision)
    {
        return
            collision.CompareTag("1_Boss") ||
            collision.CompareTag("2_Boss") ||
            collision.CompareTag("3_Boss");
    }

    // =========================================================
    // Boss 공격
    // =========================================================

    void HitBoss(Collider2D collision)
    {
        // -----------------------------------------------------
        // 삽 / 낫이 아닌 일반 발사체
        // -----------------------------------------------------

        if (id != 0 && id != 5)
        {
            // 이미 같은 Bullet이 Boss를 때렸다면
            // 다시 공격하지 않음
            if (hitBoss)
                return;
        }

        // -----------------------------------------------------
        // 삽 / 낫
        // -----------------------------------------------------

        else
        {
            // 회전 속도에 따른 공격 간격
            if (Time.time < nextSpinHitTime)
                return;
        }

        // -----------------------------------------------------
        // Boss 컴포넌트 찾기
        // -----------------------------------------------------

        Boss boss =
            collision.GetComponent<Boss>();

        // Boss Collider가 자식에 있는 경우
        if (boss == null)
        {
            boss =
                collision.GetComponentInParent<Boss>();
        }

        if (boss == null)
        {
            Debug.LogWarning(
                $"[Bullet] Boss 태그는 찾았지만 " +
                $"Boss 컴포넌트를 찾지 못함: " +
                $"{collision.name}"
            );

            return;
        }

        // =====================================================
        // 삽 / 낫 공격 타이밍 설정
        // =====================================================

        if (id == 0 || id == 5)
        {
            // 다음 공격 가능 시간 설정
            nextSpinHitTime =
                Time.time +
                spinHitInterval;
        }

        // =====================================================
        // 희망의 호프
        // =====================================================

        float hopeDamage =
            boss.GetMaxHealth() *
            Item.HopeOfHopeRate;

        float totalDamage =
            damage +
            hopeDamage;

        Debug.Log(
            $"<color=yellow>[Bullet → Boss 충돌]</color> " +
            $"ID: {id} | " +
            $"기본 피해: {damage:F1} | " +
            $"희망의 호프: {hopeDamage:F1} | " +
            $"총 피해: {totalDamage:F1}"
        );

        // =====================================================
        // Boss 실제 피해
        // =====================================================

        boss.TakeDamage(
            totalDamage,
            Item.PoisonRate
        );

        // =====================================================
        // [추가된 보스 피흡 로직]
        // =====================================================
        if (Item.BloodHitRate > 0f)
        {
            // 총 피해량에 피흡 비율을 곱해 회복량을 정합니다.
            float healAmount = totalDamage * Item.BloodHitRate;

            // 플레이어 체력 컴포넌트를 찾아서 치료합니다.
            PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount);
                Debug.Log($"<color=lime>[보스 피흡 성공]</color> 회복량: {healAmount:F1}");
            }
        }

        // =====================================================
        // 일반 발사체
        // =====================================================

        if (id != 0 && id != 5)
        {
            // 같은 Bullet은 다시 Boss 공격 불가
            hitBoss = true;

            // 무한 관통 무기가 아니라면
            // 기존 방식대로 관통 횟수 감소
            per--;

            if (per < 0)
            {
                DisableBullet();
            }
        }
    }

    // =========================================================
    // Bullet 비활성화
    // =========================================================

    void DisableBullet()
    {
        // ---------------------------------------------------------
        // 3. 비활성화될 때 예약되어 있던 Invoke 취소하기 (오브젝트 풀 꼬임 방지)
        // ---------------------------------------------------------
        CancelInvoke("DisableBullet"); // ★ 추가: 이미 몬스터를 맞춰서 사라질 때, 예약된 Invoke를 취소함

        if (rigid != null)
        {
            rigid.linearVelocity = Vector2.zero;
        }

        gameObject.SetActive(false);
    }

    // =========================================================
    // Area 밖으로 나감
    // =========================================================

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || per == -100)
            return;

        // 엽총 같은 일반 원거리 무기(id가 0, 5가 아닌 것)는 범위를 벗어나도 계속 날아가야 하므로 무시
        if (id != 0 && id != 5)
            return;

        DisableBullet();
    }
}