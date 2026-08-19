using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("이동 및 회전 속도 (느리게 추천)")]
    public float speed = 5f;
    public float rotateSpeed = 200f; // 숫자가 클수록 플레이어를 잘 꺾어서 쫓아옴

    [Header("데미지 설정")]
    [Tooltip("인스펙터에서 번개볼트의 데미지를 조절하세요.")]
    public int damage = 15; // 기본값 15, 인스펙터에서 변경 가능

    [Header("수명 설정 (몇 초 뒤에 자동으로 터질 것인가)")]
    [Tooltip("플레이어에게 닿지 않아도 이 시간이 지나면 자동으로 터집니다.")]
    public float duration = 5f; // 기본값 5초 후 자동 폭발 (인스펙터 조절 가능)

    [Header("충돌 시 생성할 번개 폭파 프리팹")]
    public GameObject explosionPrefab;

    private Transform target; // 쫓아갈 플레이어의 위치
    private Rigidbody2D rb;
    private bool isExploded = false; // 중복 폭발 방지용

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 씬에서 "Player" 태그를 가진 오브젝트를 찾아 타겟으로 설정
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;

            // [초기 조준] 보스 입에서 발사될 때 처음부터 플레이어 쪽을 바라보고 출발
            Vector2 initialDirection = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }

        // [핵심 기능] 지정된 제한 시간(duration)이 지나면 자동으로 Explode 함수를 예약 실행
        Invoke("Explode", duration);
    }

    void FixedUpdate()
    {
        if (target == null || rb == null || isExploded) return;

        // 1. 플레이어가 있는 방향 계산
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        direction.Normalize();

        // 2. 플레이어 방향으로 자연스럽게 회전하기 위한 계산 (Z축 회전)
        float rotateAmount = Vector3.Cross(direction, transform.right).z;

        // 3. Rigidbody2D를 사용해 투사체를 서서히 회전시킴
        rb.angularVelocity = -rotateAmount * rotateSpeed;

        // 4. 자신이 바라보는 앞방향(오른쪽)으로 이동
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploded) return;

        // 보스(Enemy)나 보스 무기(BossProjectile) 레이어와 충돌할 때는 무시
        if (collision.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("BossProjectile"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            // [데미지 처리] 플레이어 체력 스크립트를 가져와 데미지를 입힙니다.
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }

            Explode();
        }
    }

    void Explode()
    {
        if (isExploded) return;
        isExploded = true;

        // 예약되어 있던 자동 폭발 타이머 취소
        CancelInvoke("Explode");

        if (explosionPrefab != null)
        {
            // 이전에 파이어이그니션에서 배운 방법: 폭발 이펙트 연기도 0.5초 뒤 자동 파괴
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(exp, 0.5f);
        }

        Destroy(gameObject);
    }
}
