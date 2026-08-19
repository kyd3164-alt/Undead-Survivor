using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("이동 속도 (빠르게 추천)")]
    public float speed = 15f;

    [Header("데미지 설정")]
    [Tooltip("인스펙터에서 보스 투사체의 데미지를 조절하세요.")]
    public int damage = 10;

    [Header("충돌 시 생성할 파이어 폭파 프리팹")]
    public GameObject explosionPrefab;

    [Header("수명 설정 (몇 초 뒤에 자동으로 터질 것인가)")]
    [Tooltip("플레이어에게 닿지 않아도 이 시간이 지나면 자동으로 터집니다.")]
    public float duration = 2.0f; // 💡 번개볼트(duration) 변수명과 기능을 완벽 일치!

    private Vector2 moveDirection = Vector2.right;
    private bool isExploded = false; // 💡 번개볼트 스타일의 중복 폭발 에러 방지 플래그

    void Start()
    {
        GameObject player = GameObject.FindWithTag("Player");

        if (player != null)
        {
            moveDirection = (player.transform.position - transform.position).normalized;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        }
        else
        {
            Debug.LogWarning("씬에 'Player' 태그를 가진 오브젝트를 찾을 수 없습니다! 기본 오른쪽으로 날아갑니다.");
        }

        // 🚨 번개볼트와 완벽히 동일한 메커니즘으로 예약 폭발 타이머 가동
        Invoke("Explode", duration);
    }

    void Update()
    {
        if (isExploded) return;

        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isExploded) return;

        if (collision.CompareTag("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("BossProjectile"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
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
        // 🚨 중복 폭발로 인한 메모리 튀는 현상 방지
        if (isExploded) return;
        isExploded = true;

        CancelInvoke("Explode");

        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(exp, 0.3f);
        }

        Destroy(gameObject);
    }
}
