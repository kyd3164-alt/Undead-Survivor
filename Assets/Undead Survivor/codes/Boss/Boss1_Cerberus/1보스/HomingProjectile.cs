using UnityEngine;

public class HomingProjectile : MonoBehaviour
{
    [Header("이동 및 회전 속도 (느리게 추천)")]
    public float speed = 5f;
    public float rotateSpeed = 200f; // 숫자가 클수록 플레이어를 잘 꺾어서 쫓아옴

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
        }
    }

    void FixedUpdate()
    {
        if (target == null) return;

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
        if (collision.CompareTag("Player"))
        {
            Explode();
            // 플레이어 데미지 처리 로직 위치
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
