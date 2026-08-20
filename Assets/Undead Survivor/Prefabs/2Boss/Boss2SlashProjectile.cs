using UnityEngine;

public class Boss2SlashProjectile : MonoBehaviour
{
    [Header("데미지 설정")]
    [Tooltip("참격이 플레이어에게 주는 데미지")]
    public int damage = 20;

    [Header("수명 설정")]
    [Tooltip("플레이어에게 닿지 않아도 이 시간이 지나면 자동으로 사라집니다.")]
    public float duration = 5f;

    [Header("사라질 때 생성할 프리팹")]
    [Tooltip("참격이 사라질 때 생성할 이펙트 프리팹")]
    public GameObject disappearPrefab;

    private bool isDestroyed = false;

    void Start()
    {
        // duration초 후 자동으로 사라짐
        Invoke("Disappear", duration);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDestroyed)
            return;

        // 보스와 보스 투사체는 무시
        if (collision.CompareTag("Enemy") ||
            collision.gameObject.layer == LayerMask.NameToLayer("BossProjectile"))
        {
            return;
        }

        // 플레이어와 충돌
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth =
                collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                playerHealth.TakeBossBodyDamage(damage);
            }

            Disappear();
        }
    }

    void Disappear()
    {
        if (isDestroyed)
            return;

        isDestroyed = true;

        // 예약된 Disappear 취소
        CancelInvoke("Disappear");

        // 사라질 때 이펙트 생성
        if (disappearPrefab != null)
        {
            GameObject effect =
                Instantiate(
                    disappearPrefab,
                    transform.position,
                    transform.rotation
                );

            // 이펙트는 0.5초 후 삭제
            Destroy(effect, 0.5f);
        }

        // 참격 삭제
        Destroy(gameObject);
    }
}