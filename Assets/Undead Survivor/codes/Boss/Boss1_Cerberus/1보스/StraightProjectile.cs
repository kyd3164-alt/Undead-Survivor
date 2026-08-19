using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("이동 속도 (빠르게 추천)")]
    public float speed = 15f;

    [Header("데미지 설정")]
    [Tooltip("인스펙터에서 보스 투사체의 데미지를 조절하세요.")]
    public int damage = 10; // 기본값 10, 인스펙터에서 변경 가능

    [Header("충돌 시 생성할 파이어 폭파 프리팹")]
    public GameObject explosionPrefab;

    // 날아갈 방향을 기억할 변수
    private Vector2 moveDirection = Vector2.right;

    void Start()
    {
        // 1. "Player" 태그를 가진 오브젝트를 찾습니다.
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            // 2. 플레이어 위치 - 내 위치 = 플레이어를 향하는 방향 벡터 계산
            moveDirection = (player.transform.position - transform.position).normalized;

            // 3. 투사체 이미지가 플레이어 쪽을 바라보도록 회전시킵니다.
            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            Debug.LogWarning("씬에 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다! 기본 오른쪽으로 날아갑니다.");
        }
    }

    void Update()
    {
        // 4. Start에서 계산한 플레이어 방향으로 이동합니다.
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 보스(Enemy)나 보스 무기 레이어 등과는 부딪혀도 터지지 않게 분기 처리
        if (collision.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("BossProjectile"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            // [중요] 플레이어의 데미지 처리 스크립트를 가져와 데미지를 입힙니다.
            var playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage); // 인스펙터에서 설정한 데미지 전달
            }

            Explode();
        }
    }

    void Explode()
    {
        if (explosionPrefab != null)
        {
            // [수정된 핵심 부분] 폭발 이펙트를 생성함과 동시에 변수(exp)에 담습니다.
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);

            // 생성된 폭발 이펙트(검은 연기)를 0.5초 뒤에 자동으로 파괴하도록 예약합니다.
            // 만약 연기가 너무 오래 남는 것 같으면 0.5f를 0.3f 등으로 줄이시면 됩니다!
            Destroy(exp, 0.3f);
        }

        // 투사체 본체는 즉시 삭제
        Destroy(gameObject);
    }
}
