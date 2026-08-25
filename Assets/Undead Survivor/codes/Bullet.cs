using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int id;
    public float damage;
    public int per;

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
            rigid.linearVelocity = dir * 15f;
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
                DisableBullet();
            }

            return;
        }

        // =========================================================
        // Area
        // =========================================================

        if (collision.CompareTag("Area") && per != -100)
        {
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
            (Item.HopeOfHopeRate / 100f);

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

        // =====================================================
        // 삽 / 낫
        // =====================================================

        else
        {
            // 삽 / 낫은 계속 회전하므로
            // Bullet을 비활성화하지 않음
        }
    }

    // =========================================================
    // Bullet 비활성화
    // =========================================================

    void DisableBullet()
    {
        if (rigid != null)
        {
            rigid.linearVelocity =
                Vector2.zero;
        }

        gameObject.SetActive(false);
    }

    // =========================================================
    // Area 밖으로 나감
    // =========================================================

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") ||
            per == -100)
            return;

        DisableBullet();
    }
}