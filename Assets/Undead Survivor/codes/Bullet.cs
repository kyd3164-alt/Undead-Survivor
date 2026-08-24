using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int id;
    public float damage;
    public int per;

    Rigidbody2D rigid;

    // 같은 총알이 같은 보스에게 여러 프레임 동안
    // 계속 피해를 주는 것을 방지
    bool hitBoss = false;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    // =========================================================
    // 보스 확정 타격 시스템
    // =========================================================

    void Update()
    {
        // 이미 이 총알이 보스를 한 번 맞췄다면
        // 다시 피해를 주지 않음
        if (hitBoss)
            return;

        // Boss 컴포넌트를 가진 보스를 직접 찾음
        Boss targetBoss = Object.FindFirstObjectByType<Boss>();

        if (targetBoss != null)
        {
            Debug.Log(
                $"[Bullet 테스트] Boss 발견! " +
                $"상태: {targetBoss.currentState}"
            );

            // 총알/무기의 현재 위치와 보스 본체의 현재 위치 사이의 절대적인 수학적 거리 좌표 계산
            float distance = Vector2.Distance(transform.position, targetBoss.transform.position);

            Debug.Log(
                $"[Bullet 테스트] 총알-보스 거리: {distance:F2}"
            );

            // 💡 현재 화면을 보니 보스 덩치가 매우 큽니다. 거리를 2.5m로 넉넉하게 확장하여 스치기만 해도 맞게 세팅합니다!
            if (distance <= 2.5f)
            {
                // ==========================================
                // 희망의 호프 추가 피해
                // ==========================================

                float hopeDamage = targetBoss.GetMaxHealth() * (Item.HopeOfHopeRate / 100f);

                // 최종 피해
                float totalDamage = damage + hopeDamage;

                Debug.Log(
                    $"<color=red>[Bullet → Boss]</color> " +
                    $"TakeDamage 호출! " +
                    $"최종 데미지: {totalDamage:F1}"
                );

                // 보스에게 피해
                targetBoss.TakeDamage(totalDamage);

                // ★ 같은 총알이 같은 보스를 계속 공격하지 못하도록 기록
                hitBoss = true;

                Debug.Log(
                    $"<color=yellow>[최종 타격]</color> " +
                    $"거리: {distance:F2}m | " +
                    $"기본 피해: {damage:F1} | " +
                    $"희망의 호프: {hopeDamage:F1} | " +
                    $"최종 피해: {totalDamage:F1}"
                );

                // 무한 관통 무기(id 0번, 5번)가 아니라면 대미지를 줬으니 총알 소멸 처리
                if (id != 0 && id != 5)
                {
                    per--;
                    if (per < 0)
                    {
                        if (rigid != null) rigid.linearVelocity = Vector2.zero;
                        gameObject.SetActive(false);
                    }
                }
            }
        }
    }


    public void Init(float damage, int per, Vector3 dir, int id = 0)
    {
        this.damage = damage;
        this.per = per;
        this.id = id;

        // 오브젝트 풀에서 재사용될 때
        // 다시 보스를 공격할 수 있도록 초기화
        hitBoss = false;

        if (per >= 0)
        {
            rigid.linearVelocity = dir * 15f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // Boss의 피해 처리는 Update()에서 처리합니다.
        // 따라서 여기서는 Boss에 대한 피해 처리를 하지 않습니다.

        // Enemy도 아니고 Boss도 아니면 무시
        if ((!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) || per == -100)
            return;

        // Boss는 Update()에서 이미 처리하므로 여기서는 종료
        if (collision.CompareTag("Boss"))
            return;

        // 여기부터는 기존 관통 처리
        if (id == 0 || id == 5)
            return;

        per--;

        if (per < 0)
        {
            if (rigid != null)
                rigid.linearVelocity = Vector2.zero;
            
            gameObject.SetActive(false);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag("Area") || per == -100)
            return;

        gameObject.SetActive(false);
    }
}
