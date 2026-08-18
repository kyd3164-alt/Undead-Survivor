using UnityEngine;

public class StraightProjectile : MonoBehaviour
{
    [Header("이동 속도 (빠르게 추천)")]
    public float speed = 15f;

    [Header("충돌 시 생성할 파이어 폭파 프리팹")]
    public GameObject explosionPrefab;

    void Update()
    {
        // 자신이 바라보는 방향(오른쪽)으로 직선 이동
        transform.Translate(Vector2.right * speed * Time.deltaTime);
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
