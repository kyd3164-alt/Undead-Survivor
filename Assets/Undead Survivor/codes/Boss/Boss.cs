using System.Collections;
using UnityEngine;

public class Boss : MonoBehaviour
{
    public enum BossType { Boss1, Boss2, Boss3 }
    public enum BossState { Appearance, Move, Attack_Melee, Attack_Ranged, Dead }

    [Header("========================================")]
    [Header("             [보스 공통 설정]            ")]
    [Header("========================================")]
    public BossType bossType;
    public BossState currentState = BossState.Appearance;

    [Header("--- 공통 능력치 ---")]
    public float maxHealth = 1000f;       // 최대 체력
    public float currentHealth;           // 현재 체력
    public float speed = 2f;              // 이동 속도
    public float damage = 10f;            // 기본 데미지
    public float attackSpeed = 1.0f;      // 공격 속도 가속도
    public float defense = 5f;            // 방어력
    public float healthRegenPerSec = 1f;  // 초당 체력 회복량

    [Header("--- 공통 사거리 & 쿨타임 ---")]
    public float meleeAttackRange = 3f;   // 근접 공격 인식 거리
    public float meleeAttackRadius = 2f;  // 근접 공격 타격 범위
    public float rangedAttackCooldown = 5f; // 원거리/스킬 공격 쿨타임
    private float lastRangedAttackTime;


    [Header("========================================")]
    [Header("       [Boss 1 : 케르베로스 전용]        ")]
    [Header("========================================")]
    [Header("--- 케르베로스 머리 위치 (3개) ---")]
    public Transform[] firePoints;

    [Header("--- 케르베로스 원거리 스킬 프리팹 ---")]
    public GameObject fireBallPrefab;     // 화염구 프리팹
    public GameObject lightningPrefab;    // 번개 프리팹

    [Header("--- 케르베로스 점프 & 용암 장판 설정 ---")]
    public float jumpAttackRadius = 2.5f;   // 점프 착지 시 즉발 범위 공격
    public GameObject lavaPrefab;           // 용암 장판 프리팹
    public float lavaDuration = 3f;         // 용암 바닥 유지 시간 (초)
    public float lavaDamage = 5f;           // 용암 바닥 초당 데미지 (DOT)
    public float lavaRadius = 2f;           // 용암 바닥 타격 범위
    public float slowDebuffAmount = 0.5f;   // 디버프: 이동속도 감소율 (0.5 = 50% 감속)
    public float debuffDuration = 2f;       // 디버프 지속 시간 (초)


    [Header("========================================")]
    [Header("       [Boss 2 & Boss 3 탄막 전용]       ")]
    [Header("========================================")]
    public GameObject bulletPrefab;       // 기본 탄막 프리팹


    [Header("========================================")]
    [Header("          [Boss 3 : 드래곤 각성 전용]     ")]
    [Header("========================================")]
    [Range(1f, 3f)] public float awakenStatMultiplier = 1.5f; // 각성 시 능력치 증폭률
    public GameObject minionPrefab;       // 드래곤 소환수 프리팹
    public Transform[] spawnPoints;       // 소환수 생성 위치들
    public GameObject phase2AuraEffect;   // 각성 오라 이펙트 오브젝트
    [SerializeField] private bool isPhase2OvertimeBuffActive = false;


    private bool isAwakened = false;
    private SpriteRenderer spriteRenderer;
    private Transform target;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        target = GameManager.instance.player.transform;
        currentHealth = maxHealth;

        StartCoroutine(AppearanceRoutine());
        StartCoroutine(HealthRegenRoutine());
    }

    IEnumerator HealthRegenRoutine()
    {
        while (currentState != BossState.Dead)
        {
            yield return new WaitForSeconds(1f);
            if (currentHealth < maxHealth && currentState != BossState.Appearance)
            {
                currentHealth += healthRegenPerSec;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
            }
        }
    }

    IEnumerator AppearanceRoutine()
    {
        currentState = BossState.Appearance;

        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        currentState = BossState.Move;
        StartCoroutine(BossLoop());
    }

    // 거리 기반 자동 AI 루프
    IEnumerator BossLoop()
    {
        while (currentState != BossState.Dead)
        {
            currentState = BossState.Move;

            // 1. 공격 검사 주기
            float checkInterval = Random.Range(1.5f, 3f) / attackSpeed;
            float timer = 0f;

            while (timer < checkInterval)
            {
                MoveTowardsPlayer();

                // 이동 도중 플레이어가 근접 사거리에 들어오면 바로 근접 공격!
                float distanceToPlayer = Vector2.Distance(transform.position, target.position);
                if (distanceToPlayer <= meleeAttackRange)
                {
                    yield return StartCoroutine(Pattern_MeleeSlash());
                    break;
                }

                timer += Time.deltaTime;
                yield return null;
            }

            // 2. 근접 사거리가 아니고 원거리 쿨타임이 차면 원거리/스킬 공격 수행
            if (currentState == BossState.Move && Time.time >= lastRangedAttackTime + (rangedAttackCooldown / attackSpeed))
            {
                if (bossType == BossType.Boss1)
                {
                    // 케르베로스(Boss1) 스킬 패턴 (화염구/번개 or 점프 공격 무작위 선택)
                    yield return StartCoroutine(Pattern_CerberusSkills());
                }
                else
                {
                    // Boss2, Boss3는 기존 탄막 패턴 발동 (+ Boss3는 확률적으로 미니언 소환 추가)
                    if (bossType == BossType.Boss3 && Random.value < 0.4f && minionPrefab != null)
                    {
                        SpawnMinions();
                    }

                    yield return StartCoroutine(Pattern_RangedShotgun());
                }
            }
        }
    }

    void MoveTowardsPlayer()
    {
        if (target == null || currentState != BossState.Move) return;

        Vector2 dir = (target.position - transform.position).normalized;
        transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        spriteRenderer.flipX = dir.x < 0;
    }

    // [패턴 1] 근접 공격
    IEnumerator Pattern_MeleeSlash()
    {
        currentState = BossState.Attack_Melee;

        // 경고 연출 (주황색)
        spriteRenderer.color = new Color(1f, 0.5f, 0f);
        yield return new WaitForSeconds(0.5f / attackSpeed);
        spriteRenderer.color = Color.white;

        // 실제 타격 판정
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (distanceToPlayer <= meleeAttackRadius)
        {
            Debug.Log("플레이어 근접 타격 성공!");
        }

        yield return new WaitForSeconds(0.5f / attackSpeed);
        currentState = BossState.Move;
    }

    // [패턴 2] 케르베로스 전용 스킬 선택 (삼두 원거리 혹은 점프 찍기)
    IEnumerator Pattern_CerberusSkills()
    {
        currentState = BossState.Attack_Ranged;
        lastRangedAttackTime = Time.time;

        int patternType = Random.Range(0, 3);

        if (patternType == 0 && fireBallPrefab != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0f);
            yield return new WaitForSeconds(0.5f / attackSpeed);
            spriteRenderer.color = Color.white;

            for (int i = 0; i < firePoints.Length; i++)
            {
                if (firePoints[i] == null) continue;

                Vector2 spawnPos = firePoints[i].position;
                Vector2 dirToPlayer = (target.position - (Vector3)spawnPos).normalized;

                GameObject fireBall = Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);
                Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = dirToPlayer * 6f;
                }
            }
            Debug.Log("삼두 화염구 동시 발사!");
        }
        else if (patternType == 1 && lightningPrefab != null)
        {
            spriteRenderer.color = new Color(0.3f, 0.5f, 1f);
            yield return new WaitForSeconds(0.5f / attackSpeed);
            spriteRenderer.color = Color.white;

            for (int i = 0; i < firePoints.Length; i++)
            {
                if (firePoints[i] == null) continue;

                Vector3 strikePos = target.position + (Vector3)Random.insideUnitCircle * 1.5f;
                Instantiate(lightningPrefab, strikePos, Quaternion.identity);

                yield return new WaitForSeconds(0.15f);
            }
            Debug.Log("삼두 연쇄 번개 강하!");
        }
        else
        {
            yield return StartCoroutine(Pattern_CerberusJumpAttack());
        }

        yield return new WaitForSeconds(0.6f / attackSpeed);
        currentState = BossState.Move;
    }

    // [패턴 2-sub] 케르베로스 점프 내려찍기 및 용암 생성 구현
    IEnumerator Pattern_CerberusJumpAttack()
    {
        spriteRenderer.color = Color.red;
        yield return new WaitForSeconds(0.4f / attackSpeed);
        spriteRenderer.color = Color.white;

        Vector3 startPos = transform.position;
        Vector3 landingPos = target.position;

        float timer = 0f;
        float jumpDuration = 0.5f / attackSpeed;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, landingPos, timer / jumpDuration);
            yield return null;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (distanceToPlayer <= jumpAttackRadius)
        {
            Debug.Log("케르베로스 점프 찍기 타격 성공!");
            ApplyDebuff(target.gameObject);
        }

        if (lavaPrefab != null)
        {
            GameObject lava = Instantiate(lavaPrefab, landingPos, Quaternion.identity);
            StartCoroutine(LavaRoutine(lava));
        }

        yield return new WaitForSeconds(0.4f / attackSpeed);
    }

    // 용암 바닥 도트딜 및 유지 시간 루틴
    IEnumerator LavaRoutine(GameObject lava)
    {
        float elapsed = 0f;
        while (elapsed < lavaDuration)
        {
            float distToPlayer = Vector2.Distance(lava.transform.position, target.position);
            if (distToPlayer <= lavaRadius)
            {
                Debug.Log($"용암 장판 도트 데미지 적용: {lavaDamage}");
            }

            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }

        Destroy(lava);
    }

    void ApplyDebuff(GameObject targetObj)
    {
        Debug.Log($"이동속도 {slowDebuffAmount * 100}% 감소 디버프 {debuffDuration}초 동안 적용!");
    }

    // [패턴 3] 일반 보스 원거리 탄막 공격 (Boss2, Boss3 전용)
    IEnumerator Pattern_RangedShotgun()
    {
        currentState = BossState.Attack_Ranged;
        lastRangedAttackTime = Time.time;

        spriteRenderer.color = new Color(0.5f, 0f, 0.5f);
        yield return new WaitForSeconds(0.8f / attackSpeed);
        spriteRenderer.color = Color.white;

        if (bulletPrefab != null)
        {
            int count = (bossType == BossType.Boss3 && isAwakened) ? 16 : 8;
            float angleStep = 360f / count;

            for (int i = 0; i < count; i++)
            {
                float angle = i * angleStep;
                Quaternion rotation = Quaternion.Euler(0, 0, angle);
                GameObject bullet = Instantiate(bulletPrefab, transform.position, rotation);

                Rigidbody2D bulletRb = bullet.GetComponent<Rigidbody2D>();
                if (bulletRb != null)
                {
                    Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                    bulletRb.linearVelocity = dir * 5f;
                }
            }
        }

        yield return new WaitForSeconds(0.8f / attackSpeed);
        currentState = BossState.Move;
    }

    // [드래곤 전용] 미니언 소환 함수
    void SpawnMinions()
    {
        if (minionPrefab == null || spawnPoints == null) return;

        foreach (Transform point in spawnPoints)
        {
            if (point != null)
            {
                Instantiate(minionPrefab, point.position, Quaternion.identity);
            }
        }
        Debug.Log("드래곤 부하 소환!");
    }

    public void TakeDamage(float incomingDamage)
    {
        if (currentState == BossState.Appearance) return;

        float finalDamage = incomingDamage - defense;
        if (finalDamage < 1f) finalDamage = 1f;

        currentHealth -= finalDamage;

        if (currentHealth <= 0)
        {
            if (bossType == BossType.Boss3 && !isAwakened) StartCoroutine(AwakenRoutine());
            else Die();
        }
    }

    // [Boss 3] 드래곤 각성 패턴
    IEnumerator AwakenRoutine()
    {
        isAwakened = true;
        currentState = BossState.Dead;

        Debug.Log("3형 보스(드래곤) 각성!");

        float timer = 0f;
        Vector3 originalPos = transform.position;
        while (timer < 2f)
        {
            transform.position = originalPos + (Vector3)Random.insideUnitCircle * 0.1f;
            spriteRenderer.color = Color.Lerp(Color.white, Color.red, timer / 2f);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPos;

        maxHealth *= awakenStatMultiplier;
        currentHealth = maxHealth;
        speed *= awakenStatMultiplier;
        damage *= awakenStatMultiplier;
        attackSpeed *= awakenStatMultiplier;
        defense *= awakenStatMultiplier;
        healthRegenPerSec *= awakenStatMultiplier;

        meleeAttackRange *= 1.3f;
        meleeAttackRadius *= 1.3f;

        // 드래곤 오라 이펙트 활성화
        if (bossType == BossType.Boss3 && phase2AuraEffect != null)
        {
            phase2AuraEffect.SetActive(true);
        }

        isPhase2OvertimeBuffActive = true;
        StartCoroutine(Phase2StatScaling());

        currentState = BossState.Move;
        StartCoroutine(BossLoop());
    }

    // [Boss 3] 각성 후 시간이 지날수록 강해지는 버프 루틴
    IEnumerator Phase2StatScaling()
    {
        while (currentState != BossState.Dead && isPhase2OvertimeBuffActive)
        {
            yield return new WaitForSeconds(5.0f);

            damage *= 1.05f; // 5초마다 공격력 5% 추가 증가
            speed *= 1.02f;  // 이동속도 2% 추가 증가
            Debug.Log("[각성 오버타임 버프] 보스가 점점 더 강해집니다!");
        }
    }

    void Die()
    {
        currentState = BossState.Dead;

        // 사망 시 오라 이펙트 종료
        if (isPhase2OvertimeBuffActive)
        {
            isPhase2OvertimeBuffActive = false;
            if (phase2AuraEffect != null) phase2AuraEffect.SetActive(false);
        }

        // 🔓 Boss1 처치 시 Item 6 (용기의 본능) 해금 및 선택창 출력
        if (bossType == BossType.Boss1)
        {
            LevelUp levelUpUI = FindFirstObjectByType<LevelUp>();
            if (levelUpUI != null)
            {
                levelUpUI.UnlockItem(6);
                levelUpUI.Show();
            }
        }
        else if (bossType == BossType.Boss3)
        {
            Debug.Log("용왕(드래곤) 완전 처치!");
        }

        Destroy(gameObject, 2.0f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, jumpAttackRadius);
    }
}