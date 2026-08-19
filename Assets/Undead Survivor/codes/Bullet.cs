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

    // 🚨 [새로 추가된 확정 타격 시스템]
    // 물리 엔진(OnTriggerEnter2D)이 충돌을 차단하더라도, 무기가 보스 몸뚱이(중심점) 근처로 지나가면 강제로 피를 깎아버립니다.
    void Update()
    {
        // 🚨 [태그 버그 완전 차단] 이름 뒤에 (Clone)이 붙든 태그가 날아갔든 상관없이, 
        // 현재 게임 화면에 존재하는 진짜 'Boss' 컴포넌트를 가진 스크립트 본체를 직접 찾아냅니다.
        Boss targetBoss = Object.FindFirstObjectByType<Boss>();

        if (targetBoss != null)
        {
            // 총알/무기의 현재 위치와 보스 본체의 현재 위치 사이의 절대적인 수학적 거리 좌표 계산
            float distance = Vector2.Distance(transform.position, targetBoss.transform.position);

            // 💡 현재 화면을 보니 보스 덩치가 매우 큽니다. 거리를 2.5m로 넉넉하게 확장하여 스치기만 해도 맞게 세팅합니다!
            if (distance <= 2.5f)
            {
                // [대미지 버그 방지] 혹시 무기 대미지가 0 이하로 버그가 걸려있다면, 강제로 80 대미지를 세팅합니다.
                float finalWeaponDamage = damage <= 0 ? 80f : damage;

                targetBoss.TakeDamage(finalWeaponDamage); // 보스에게 다이렉트로 대미지 꽂기
                Debug.Log($"<color=yellow>[최종 치트 타격]</color> 보스 발견! 거리: {distance:F2}m | 데미지: {finalWeaponDamage} 강제 주입 완료!");

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

        if (per >= 0)
        {
            rigid.linearVelocity = dir * 15f;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 🚨 유니티 물리 엔진(레이어/태그) 버그 우회용 강제 타격 코드
        Boss directBossScript = collision.GetComponentInParent<Boss>();

        if (directBossScript != null)
        {
            directBossScript.TakeDamage(damage); // 보스에게 확정 대미지 전달
            Debug.Log($"<color=cyan>[스크립트 강제 타격]</color> 보스에게 {damage} 대미지 전달 성공!");

            if (id != 0 && id != 5)
            {
                per--;
                if (per < 0)
                {
                    rigid.linearVelocity = Vector2.zero;
                    gameObject.SetActive(false);
                }
            }
            return;
        }

        // [수정] 충돌한 오브젝트가 Enemy도 아니고 Boss도 아니라면 그냥 통과(return)합니다.
        if ((!collision.CompareTag("Enemy") && !collision.CompareTag("Boss")) || per == -100)
            return;

        if (collision.CompareTag("Boss"))
        {
            Boss bossScript = collision.GetComponent<Boss>();
            if (bossScript != null)
            {
                bossScript.TakeDamage(damage);
            }
        }

        if (id == 0 || id == 5)
            return;

        per--;

        if (per < 0)
        {
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
