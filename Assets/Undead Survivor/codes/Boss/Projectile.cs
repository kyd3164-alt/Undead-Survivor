using UnityEngine;

public enum ElementType { Fire, Lightning }

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public ElementType element;
    public float speed = 10f;
    public float damage = 15f;
    public float lifetime = 5f;

    private Vector2 direction;
    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(Vector2 fireDirection)
    {
        direction = fireDirection.normalized;

        // 발사 방향에 맞춰 투사체 회전 (2D Z축 회전)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 일정 시간 후 자동 파괴
        Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            // 플레이어에게 데미지 전달 (PlayerHealth 스크립트가 있다면 주석 해제)
            // collision.GetComponent<PlayerHealth>()?.TakeDamage(damage);

            Debug.Log($"[{element}] 속성 공격 맞춤! 데미지: {damage}");

            // TODO: 충돌 이펙트 생성 위치
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Ground"))
        {
            // 바닥이나 벽에 부딪히면 파괴
            Destroy(gameObject);
        }
    }
}