using System.Collections;
using UnityEngine;

public class Boss2SlashSkill : MonoBehaviour
{
    [Header("참격 발사 설정")]
    [Tooltip("실제로 날아가는 참격 프리팹")]
    public GameObject slashProjectilePrefab;

    [Tooltip("스킬 지속 시간")]
    public float duration = 5f;

    [Tooltip("참격 발사 간격")]
    public float shotInterval = 0.25f;

    [Tooltip("한 번 발사할 때 생성되는 참격 개수")]
    public int slashCount = 1;

    [Tooltip("참격 이동 속도")]
    public float slashSpeed = 7f;

    [Tooltip("참격이 시작되는 거리")]
    public float spawnDistance = 0f;

    private void Start()
    {
        StartCoroutine(SlashRoutine());
    }

    IEnumerator SlashRoutine()
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            FireRandomSlashes();

            yield return new WaitForSeconds(shotInterval);

            elapsed += shotInterval;
        }

        Destroy(gameObject);
    }

    void FireRandomSlashes()
    {
        if (slashProjectilePrefab == null)
            return;

        for (int i = 0; i < slashCount; i++)
        {
            // 0~360도 완전 랜덤
            float randomAngle = Random.Range(0f, 360f);

            float radians = randomAngle * Mathf.Deg2Rad;

            Vector2 direction = new Vector2(
                Mathf.Cos(radians),
                Mathf.Sin(radians)
            ).normalized;

            Vector3 spawnPosition =
                transform.position +
                (Vector3)(direction * spawnDistance);

            Quaternion rotation =
                Quaternion.Euler(0f, 0f, randomAngle);

            GameObject slash = Instantiate(
                slashProjectilePrefab,
                spawnPosition,
                rotation
            );

            // 참격 이동
            Rigidbody2D rb = slash.GetComponent<Rigidbody2D>();

            if (rb != null)
            {
                rb.linearVelocity = direction * slashSpeed;
            }
        }
    }
}