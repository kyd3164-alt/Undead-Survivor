using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("이동 및 회전 속도 (느리게 추천)")]
    public float speed = 5f;
    public float rotateSpeed = 200f; // 숫자가 클수록 플레이어를 잘 꺾어서 쫓아옴

    [Header("데미지 설정")]
    [Tooltip("인스펙터에서 번개/화염 유도탄의 데미지를 조절하세요.")]
    public int damage = 15; // 기본값 15, 인스펙터에서 변경 가능

    [Header("충돌 시 생성할 번개 폭파 프리팹")]
    public GameObject explosionPrefab;

    private Transform target; // 쫓아갈 플레이어의 위치
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // 씬에서 "Player" 태그를 가진 오브젝트를 찾아 타겟으로 설정
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            target = player.transform;

            // [초기 조준] 발사되는 순간 처음부터 플레이어 쪽을 바라보고 날아가기 시작합니다.
            Vector2 initialDirection = (target.position - transform.position).normalized;
            float angle = Mathf.Atan2(initialDirection.y, initialDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            Debug.LogWarning("씬에 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다!");
        }
    }

    void FixedUpdate()
    {
        if (target == null || rb == null) return;

        // 1. 플레이어가 있는 방향 계산
        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        direction.Normalize();

        // 2. 플레이어 방향으로 자연스럽게 회전하기 위한 계산 (Z축 회전)
        float rotateAmount = Vector3.Cross(direction, transform.right).z;

        // 3. Rigidbody2D를 사용해 투사체를 서서히 회전시킴
        rb.angularVelocity = -rotateAmount * rotateSpeed;

        // 4. 자신이 바라보는 앞방향(오른쪽)으로 이동
        // 최신 유니티 버전에 맞춰 linearVelocity 또는 velocity 둘 다 안전하게 지원하도록 처리
        rb.linearVelocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 보스(Enemy)나 보스 투사체 레이어와 스쳐 지나갈 때는 터지지 않게 보호
        if (collision.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("BossProjectile"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            // [데미지 처리] 플레이어 체력 스크립트를 가져와 인스펙터의 데미지를 넘겨줍니다.
            // ※ 프로젝트의 실제 플레이어 체력 스크립트 이름(예: PlayerHealth)으로 변경해야 작동합니다.
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
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}
