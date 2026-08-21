using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Boss : MonoBehaviour
{
    // =========================================================
    // Boss Type
    // =========================================================

    public enum BossType
    {
        Boss1,
        Boss2,
        Boss3
    }

    public enum BossState
    {
        Appearance,
        Idle,
        Move,
        Attack_Melee,
        Attack_Ranged,
        Dead
    }


    // =========================================================
    // 공통 설정
    // =========================================================

    [Header("========================================")]
    [Header("            [보스 공통 설정]            ")]
    [Header("========================================")]

    public BossType bossType;
    public BossState currentState = BossState.Appearance;


    [Header("--- 공통 능력치 ---")]

    public float maxHealth = 1000f;
    public float currentHealth;

    public float speed = 2f;
    public float damage = 10f;
    public float attackSpeed = 1f;
    public float defense = 5f;

    public float healthRegenPerSec = 1f;


    [Header("--- 공통 사거리 & 쿨타임 ---")]

    public float meleeAttackRange = 3f;
    public float meleeAttackRadius = 2f;

    public float rangedAttackCooldown = 5f;

    private float lastRangedAttackTime;


    // =========================================================
    // 원본 능력치
    // =========================================================

    private float baseMaxHealth;
    private float baseSpeed;
    private float baseDamage;
    private float baseAttackSpeed;
    private float baseDefense;
    private float baseHealthRegen;


    // =========================================================
    // Boss1
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss 1 : 케르베로스 전용]       ")]
    [Header("========================================")]

    public Transform[] firePoints;

    public GameObject fireBallPrefab;
    public GameObject lightningPrefab;

    public float jumpAttackRadius = 2.5f;

    public GameObject lavaPrefab;
    public float lavaDuration = 3f;
    public float lavaDamage = 5f;
    public float lavaRadius = 3f;

    public float slowDebuffAmount = 0.5f;
    public float debuffDuration = 2f;


    // =========================================================
    // Boss2
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss 2 : 악천 전용]             ")]
    [Header("========================================")]


    // =========================================================
    // Boss2 버프 스킬
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss2 버프 스킬]                 ")]
    [Header("========================================")]

    [Tooltip("악천 전용 버프 스킬 프리팹")]
    public GameObject buffSkillPrefab;

    [Tooltip("Boss2 버프 스킬 사용 여부")]
    public bool useBuffSkill = true;

    [Tooltip("1페이지 버프 스킬 지속시간")]
    public float buffSkillDuration = 5f;

    [Tooltip("버프 스킬 사정거리")]
    public float buffSkillRange = 4f;

    [Tooltip("버프 스킬이 플레이어에게 주는 초당 데미지")]
    public float buffDamagePerSecond = 10f;

    [Tooltip("버프 스킬이 보스 자신에게 주는 초당 회복량")]
    public float buffHealPerSecond = 5f;

    [Tooltip("버프 피해 및 회복 판정 간격")]
    public float buffTickInterval = 1f;

    [Tooltip("1페이지 버프 종료 후 다시 사용하기까지 대기시간")]
    public float buffSkillCooldown = 3f;

    private GameObject activeBuffSkill;

    private Coroutine buffSkillRoutine;

    private float lastBuffSkillTime = -999f;

    // 2페이지 / 3페이지에서 상시 버프 사용 여부
    private bool boss2PersistentBuff = false;


    // =========================================================
    // Boss2 1페이지
    // =========================================================

    [Header("--- Boss2 1페이지 슬래시 ---")]

    [Tooltip("1페이지에서 생성되는 슬래시 프리팹")]
    public GameObject slashPrefab;

    [Tooltip("1페이지 슬래시 사용 후 참격이 시작되기까지 시간")]
    public float phase1SlashDelay = 1f;

    [Tooltip("1페이지에서 발사할 참격 프리팹")]
    public GameObject phase1ProjectilePrefab;

    [Tooltip("1페이지 참격 발사 간격")]
    public float phase1ShotInterval = 0.25f;

    [Tooltip("1페이지 한 번에 생성할 참격 개수")]
    public int phase1SlashCount = 1;

    [Tooltip("1페이지 참격 이동 속도")]
    public float phase1SlashSpeed = 7f;

    [Tooltip("1페이지 참격 생성 거리")]
    public float phase1SpawnDistance = 0f;

    [Tooltip("1페이지 슬래시가 유지되는 시간")]
    public float phase1SlashDuration = 5f;


    // =========================================================
    // Boss2 1페이지 주변 공격
    // =========================================================

    [Header("--- Boss2 1페이지 주변 공격 ---")]

    public GameObject skillPrefab;

    public float slashRadius = 4f;
    public float slashInitialDamage = 50f;
    public float slashDamagePerSecond = 10f;
    public float slashTickInterval = 0.5f;
    public float slashDamageDuration = 3f;


    // =========================================================
    // Boss2 폼체인지
    // =========================================================

    [Header("--- Boss2 폼체인지 ---")]

    [Tooltip("Boss2 2페이지에서 생성할 폼 프리팹")]
    public GameObject phase2Form;

    [Tooltip("Boss2 3페이지에서 생성할 분노 폼 프리팹")]
    public GameObject phase3Form;

    // 실제 생성된 폼
    private GameObject activePhaseForm;


    // =========================================================
    // Boss2 1페이지 보상
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss2 1페이지 보상]              ")]
    [Header("========================================")]

    [Tooltip("1페이지 사망 시 플레이어 레벨 증가량")]
    public int phase1RewardLevel = 1;

    [Tooltip("1페이지 사망 시 보상 선택 횟수")]
    public int phase1RewardChoiceCount = 1;

    [Tooltip("1페이지 사망 시 BloodHit 잠금해제")]
    public bool unlockBloodHitOnPhase1 = true;

    [Tooltip("BloodHit 잠금해제를 LevelUp UI에서 연결")]
    public UnityEvent onBoss2Phase1UnlockBloodHit;


    // =========================================================
    // Boss2 페이지별 방어력
    // =========================================================

    [Header("--- Boss2 페이지별 방어력 증가 ---")]

    [Tooltip("2페이지 추가 방어력")]
    public float phase2DefenseBonus = 10f;

    [Tooltip("3페이지 추가 방어력")]
    public float phase3DefenseBonus = 20f;


    // =========================================================
    // Boss2 페이지별 능력치 배율
    // =========================================================

    [Header("--- Boss2 페이지별 능력치 배율 ---")]

    [Tooltip("2페이지 능력치 배율")]
    public float phase2BuffMultiplier = 1f;

    [Tooltip("3페이지 능력치 배율")]
    public float phase3BuffMultiplier = 2f;


    // =========================================================
    // Boss2 2페이지 참격
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss2 2페이지 참격]              ")]
    [Header("========================================")]

    [Tooltip("2페이지에서 발사할 참격 프리팹")]
    public GameObject phase2ProjectilePrefab;

    [Tooltip("2페이지 참격이 발동되는 지속 시간")]
    public float phase2ProjectileDuration = 5f;

    [Tooltip("2페이지 참격 발사 간격")]
    public float phase2ShotInterval = 0.25f;

    [Tooltip("2페이지 한 번에 생성할 참격 개수")]
    public int phase2SlashCount = 1;

    [Tooltip("2페이지 참격 이동 속도")]
    public float phase2SlashSpeed = 7f;

    [Tooltip("2페이지 참격 생성 거리")]
    public float phase2SpawnDistance = 0f;

    [Tooltip("2페이지 참격이 끝난 후 다음 참격까지 대기 시간")]
    public float phase2ProjectileCooldown = 2f;


    // =========================================================
    // Boss2 3페이지 기본 참격
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss2 3페이지 기본 참격]          ")]
    [Header("========================================")]

    [Tooltip("3페이지 기본 참격 프리팹")]
    public GameObject phase3ProjectilePrefab;

    [Tooltip("3페이지 기본 참격 발사 간격")]
    public float phase3ShotInterval = 0.25f;

    [Tooltip("3페이지 한 번에 생성할 참격 개수")]
    public int phase3SlashCount = 1;

    [Tooltip("3페이지 참격 이동 속도")]
    public float phase3SlashSpeed = 7f;

    [Tooltip("3페이지 참격 생성 거리")]
    public float phase3SpawnDistance = 0f;


    // =========================================================
    // Boss2 3페이지 추가 슬래시
    // =========================================================

    [Header("========================================")]
    [Header("       [Boss2 3페이지 추가 슬래시]        ")]
    [Header("========================================")]

    [Tooltip("3페이지 추가 슬래시 패턴 사용")]
    public bool usePhase3ExtraSlash = true;

    [Tooltip("3페이지 추가 슬래시 프리팹")]
    public GameObject phase3ExtraSlashPrefab;

    [Tooltip("3페이지 추가 슬래시 후 참격까지 대기")]
    public float phase3ExtraSlashDelay = 1f;

    [Tooltip("3페이지 추가 참격 프리팹")]
    public GameObject phase3ExtraProjectilePrefab;

    [Tooltip("3페이지 추가 참격 지속 시간")]
    public float phase3ExtraProjectileDuration = 5f;

    [Tooltip("3페이지 추가 참격 발사 간격")]
    public float phase3ExtraShotInterval = 0.25f;

    [Tooltip("3페이지 추가 참격 개수")]
    public int phase3ExtraSlashCount = 1;

    [Tooltip("3페이지 추가 참격 속도")]
    public float phase3ExtraSlashSpeed = 7f;

    [Tooltip("3페이지 추가 참격 생성 거리")]
    public float phase3ExtraSpawnDistance = 0f;

    [Tooltip("3페이지 추가 슬래시 패턴 쿨타임")]
    public float phase3ExtraSlashCooldown = 8f;

    private float lastPhase3ExtraSlashTime = -999f;


    // =========================================================
    // Boss2 내부 상태
    // =========================================================

    private int boss2Phase = 1;

    private bool boss2PhaseChanging = false;

    private GameObject activeSlashObject;
    private GameObject activeSkillObject;


    // =========================================================
    // Boss3
    // =========================================================

    [Header("========================================")]
    [Header("         [Boss 3 : 드래곤 각성 전용]     ")]
    [Header("========================================")]

    [Range(1f, 3f)]
    public float awakenStatMultiplier = 1.5f;

    public GameObject minionPrefab;

    public Transform[] spawnPoints;

    public GameObject phase2AuraEffect;

    [SerializeField]
    private bool isPhase2OvertimeBuffActive = false;

    private bool isAwakened = false;


    // =========================================================
    // 내부 변수
    // =========================================================

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform target;

    private Vector3 lastLandingPosition;

    private Slider hpSlider;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        baseMaxHealth = maxHealth;
        baseSpeed = speed;
        baseDamage = damage;
        baseAttackSpeed = attackSpeed;
        baseDefense = defense;
        baseHealthRegen = healthRegenPerSec;
    }


    // =========================================================
    // Start
    // =========================================================

    void Start()
    {
        if (GameManager.instance != null &&
            GameManager.instance.player != null)
        {
            target =
                GameManager.instance.player.transform;
        }

        currentHealth = maxHealth;


        // =====================================================
        // Boss2 폼 초기화
        // 실제 프리팹은 아직 생성하지 않는다.
        // =====================================================

        DestroyActivePhaseForm();


        // =====================================================
        // Boss2 초기 본체 표시
        // =====================================================

        if (bossType == BossType.Boss2)
        {
            SetBoss2BodyVisible(true);
        }


        // =====================================================
        // HP BAR
        // =====================================================

        GameObject sliderObj =
            GameObject.Find("Boss Health Bar");

        if (sliderObj != null)
            sliderObj.SetActive(true);


        // =====================================================
        // 등장
        // =====================================================

        StartCoroutine(AppearanceRoutine());

        StartCoroutine(HealthRegenRoutine());
    }


    // =========================================================
    // HP BAR
    // =========================================================

    public void SetupHPBar(Slider slider)
    {
        hpSlider = slider;

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }


    // =========================================================
    // 최대 체력 반환
    // =========================================================

    public float GetMaxHealth()
    {
        return maxHealth;
    }


    // =========================================================
    // Aura 회복
    // =========================================================

    public void HealFromAura(float amount)
    {
        if (currentState == BossState.Dead)
            return;

        if (amount <= 0f)
            return;

        if (currentHealth >= maxHealth)
            return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
            currentHealth = maxHealth;

        if (hpSlider != null)
            hpSlider.value = currentHealth;
    }


    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        if (hpSlider != null)
            hpSlider.value = currentHealth;

        if (currentState == BossState.Dead)
            return;


        // =====================================================
        // 총알 감지
        // =====================================================

        Bullet[] activeBullets =
            Object.FindObjectsByType<Bullet>(
                FindObjectsSortMode.None
            );

        foreach (Bullet bullet in activeBullets)
        {
            if (bullet == null ||
                !bullet.gameObject.activeInHierarchy)
                continue;

            float distance =
                Vector2.Distance(
                    transform.position,
                    bullet.transform.position
                );

            if (distance <= 2.2f)
            {
                float weaponDamage =
                    bullet.damage <= 0f
                    ? 20f
                    : bullet.damage;

                TakeDamage(weaponDamage);

                Debug.Log(
                    $"<color=#FF00FF>" +
                    $"[보스 자체 확정 타격]" +
                    $"</color> 무기 감지! " +
                    $"거리: {distance:F2}m | " +
                    $"데미지: {weaponDamage}"
                );

                if (bullet.id != 0 &&
                    bullet.id != 5)
                {
                    bullet.per--;

                    if (bullet.per < 0)
                    {
                        Rigidbody2D bulletRigid =
                            bullet.GetComponent<Rigidbody2D>();

                        if (bulletRigid != null)
                            bulletRigid.linearVelocity =
                                Vector2.zero;

                        bullet.gameObject.SetActive(false);
                    }
                }
            }
        }
    }


    // =========================================================
    // OnDestroy
    // =========================================================

    void OnDestroy()
    {
        StopAllBoss2Effects();

        DestroyActivePhaseForm();

        GameObject sliderObj =
            GameObject.Find("Boss Health Bar");

        if (sliderObj != null)
            sliderObj.SetActive(false);
    }


    // =========================================================
    // HP Regen
    // =========================================================

    IEnumerator HealthRegenRoutine()
    {
        while (currentState != BossState.Dead)
        {
            yield return new WaitForSeconds(1f);

            if (currentHealth < maxHealth &&
                currentState != BossState.Appearance)
            {
                currentHealth += healthRegenPerSec;

                if (currentHealth > maxHealth)
                    currentHealth = maxHealth;
            }
        }
    }


    // =========================================================
    // 등장
    // =========================================================

    IEnumerator AppearanceRoutine()
    {
        currentState =
            BossState.Appearance;

        for (int i = 0; i < 3; i++)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color =
                    new Color(
                        1f,
                        0f,
                        0f,
                        0.5f
                    );
            }

            yield return new WaitForSeconds(0.2f);

            if (spriteRenderer != null)
                spriteRenderer.color =
                    Color.white;

            yield return new WaitForSeconds(0.2f);
        }


        if (bossType == BossType.Boss2 &&
            boss2Phase == 1)
        {
            SetBoss2BodyVisible(true);
        }

        currentState =
            BossState.Move;

        StartCoroutine(BossLoop());
    }


    // =========================================================
    // Boss AI
    // =========================================================

    IEnumerator BossLoop()
    {
        while (currentState != BossState.Dead)
        {
            if (boss2PhaseChanging)
                yield break;

            currentState = BossState.Move;

            float checkInterval =
                Random.Range(1.5f, 3f) /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                );

            float timer = 0f;

            while (timer < checkInterval)
            {
                if (currentState == BossState.Dead)
                    yield break;

                MoveTowardsPlayer();

                if (target != null)
                {
                    float distanceToPlayer =
                        Vector2.Distance(
                            transform.position,
                            target.position
                        );

                    if (distanceToPlayer <=
                        meleeAttackRange)
                    {
                        yield return
                            StartCoroutine(
                                Pattern_MeleeSlash()
                            );

                        break;
                    }
                }

                timer += Time.deltaTime;

                yield return null;
            }


            // =================================================
            // 원거리 스킬 쿨타임
            // =================================================

            if (currentState == BossState.Move &&
                Time.time >=
                lastRangedAttackTime +
                rangedAttackCooldown /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                ))
            {
                if (bossType == BossType.Boss1)
                {
                    yield return
                        StartCoroutine(
                            Pattern_CerberusSkills()
                        );
                }
                else
                {
                    if (bossType == BossType.Boss3 &&
                        Random.value < 0.4f &&
                        minionPrefab != null)
                    {
                        SpawnMinions();
                    }

                    yield return
                        StartCoroutine(
                            Pattern_RangedShotgun()
                        );
                }
            }
        }
    }


    // =========================================================
    // 이동
    // =========================================================

    void MoveTowardsPlayer()
    {
        if (target == null ||
            currentState != BossState.Move)
            return;

        Vector2 dir =
            (
                target.position -
                transform.position
            ).normalized;

        transform.position =
            Vector2.MoveTowards(
                transform.position,
                target.position,
                speed * Time.deltaTime
            );

        if (spriteRenderer != null &&
            spriteRenderer.enabled)
        {
            spriteRenderer.flipX =
                dir.x < 0f;
        }

        // 폼도 항상 Boss Transform의 자식이므로
        // Boss와 함께 이동한다.
    }


    // =========================================================
    // 근접 공격
    // =========================================================

    IEnumerator Pattern_MeleeSlash()
    {
        currentState =
            BossState.Attack_Melee;

        if (spriteRenderer != null &&
            !(bossType == BossType.Boss2 &&
              boss2Phase >= 2))
        {
            spriteRenderer.color =
                new Color(
                    1f,
                    0.5f,
                    0f
                );
        }

        yield return
            new WaitForSeconds(
                0.5f /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                )
            );

        if (spriteRenderer != null &&
            !(bossType == BossType.Boss2 &&
              boss2Phase >= 2))
        {
            spriteRenderer.color =
                Color.white;
        }

        if (target != null)
        {
            float distanceToPlayer =
                Vector2.Distance(
                    transform.position,
                    target.position
                );

            if (distanceToPlayer <=
                meleeAttackRadius)
            {
                Debug.Log(
                    "보스 근접 타격 성공!"
                );
            }
        }

        yield return
            new WaitForSeconds(
                0.5f /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                )
            );

        if (currentState != BossState.Dead)
            currentState =
                BossState.Move;
    }


    // =========================================================
    // Boss1 스킬
    // =========================================================

    IEnumerator Pattern_CerberusSkills()
    {
        currentState =
            BossState.Attack_Ranged;

        lastRangedAttackTime =
            Time.time;

        int patternType =
            Random.Range(0, 3);

        if (patternType == 0 &&
            fireBallPrefab != null)
        {
            if (spriteRenderer != null)
                spriteRenderer.color =
                    new Color(
                        1f,
                        0.5f,
                        0f
                    );

            yield return
                new WaitForSeconds(
                    0.5f /
                    Mathf.Max(
                        attackSpeed,
                        0.01f
                    )
                );

            if (spriteRenderer != null)
                spriteRenderer.color =
                    Color.white;

            if (firePoints != null)
            {
                for (int i = 0;
                     i < firePoints.Length;
                     i++)
                {
                    if (firePoints[i] == null)
                        continue;

                    Vector2 spawnPos =
                        firePoints[i].position;

                    Vector2 dirToPlayer =
                        target != null
                        ?
                        (
                            target.position -
                            (Vector3)spawnPos
                        ).normalized
                        :
                        Vector2.down;

                    GameObject fireBall =
                        Instantiate(
                            fireBallPrefab,
                            spawnPos,
                            Quaternion.identity
                        );

                    Rigidbody2D rb =
                        fireBall.GetComponent<Rigidbody2D>();

                    if (rb != null)
                    {
                        rb.linearVelocity =
                            dirToPlayer * 6f;
                    }
                }
            }
        }
        else if (patternType == 1 &&
                 lightningPrefab != null)
        {
            if (spriteRenderer != null)
                spriteRenderer.color =
                    new Color(
                        0.3f,
                        0.5f,
                        1f
                    );

            yield return
                new WaitForSeconds(
                    0.5f /
                    Mathf.Max(
                        attackSpeed,
                        0.01f
                    )
                );

            if (spriteRenderer != null)
                spriteRenderer.color =
                    Color.white;

            if (firePoints != null)
            {
                for (int i = 0;
                     i < firePoints.Length;
                     i++)
                {
                    if (firePoints[i] == null)
                        continue;

                    Instantiate(
                        lightningPrefab,
                        firePoints[i].position,
                        Quaternion.identity
                    );

                    yield return
                        new WaitForSeconds(0.15f);
                }
            }
        }
        else
        {
            yield return
                StartCoroutine(
                    Pattern_CerberusJumpAttack()
                );
        }

        yield return
            new WaitForSeconds(
                0.6f /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                )
            );

        if (currentState != BossState.Dead)
            currentState =
                BossState.Move;
    }


    // =========================================================
    // Boss1 점프
    // =========================================================

    IEnumerator Pattern_CerberusJumpAttack()
    {
        if (target == null)
            yield break;

        if (animator != null)
            animator.SetTrigger("DoJump");

        yield return
            new WaitForSeconds(
                0.4f /
                Mathf.Max(
                    attackSpeed,
                    0.01f
                )
            );

        Vector3 startPos =
            transform.position;

        lastLandingPosition =
            target.position;

        float timer = 0f;

        float jumpDuration =
            0.5f /
            Mathf.Max(
                attackSpeed,
                0.01f
            );

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;

            transform.position =
                Vector3.Lerp(
                    startPos,
                    lastLandingPosition,
                    timer / jumpDuration
                );

            yield return null;
        }

        if (animator != null)
            animator.SetTrigger("Land");

        float distanceToPlayer =
            Vector2.Distance(
                transform.position,
                target.position
            );

        if (distanceToPlayer <=
            jumpAttackRadius)
        {
            ApplyDebuff(
                target.gameObject
            );
        }

        yield return
            new WaitForSeconds(0.4f);
    }


    // =========================================================
    // Boss1 용암
    // =========================================================

    public void SpawnLava()
    {
        if (lavaPrefab == null)
            return;

        GameObject lava =
            Instantiate(
                lavaPrefab,
                lastLandingPosition,
                Quaternion.identity
            );

        lava.transform.localScale =
            new Vector3(
                3f,
                3f,
                1f
            );

        StartCoroutine(
            LavaRoutine(lava)
        );
    }


    IEnumerator LavaRoutine(GameObject lava)
    {
        float elapsed = 0f;

        Animator lavaAnim =
            lava.GetComponent<Animator>();

        while (elapsed < lavaDuration &&
               lava != null)
        {
            if (target != null)
            {
                float distToPlayer =
                    Vector2.Distance(
                        lava.transform.position,
                        target.position
                    );

                if (distToPlayer <= lavaRadius)
                {
                    Debug.Log(
                        $"용암 장판 도트 데미지 적용: {lavaDamage}"
                    );
                }
            }

            elapsed += 1f;

            yield return
                new WaitForSeconds(1f);
        }

        if (lava != null)
        {
            if (lavaAnim != null)
                lavaAnim.SetTrigger("DoEnd");

            yield return
                new WaitForSeconds(0.5f);

            Destroy(lava);
        }
    }


    void ApplyDebuff(GameObject targetObj)
    {
        Debug.Log(
            $"이동속도 {slowDebuffAmount * 100f}% 감소 디버프 적용!"
        );
    }


    // =========================================================
    // 원거리 공격
    // =========================================================

    IEnumerator Pattern_RangedShotgun()
    {
        currentState =
            BossState.Attack_Ranged;

        lastRangedAttackTime =
            Time.time;


        // =====================================================
        // Boss2
        // =====================================================

        if (bossType == BossType.Boss2)
        {
            if (boss2Phase == 1)
                SetBoss2BodyVisible(true);
            else
                SetBoss2BodyVisible(false);


            // ================================================
            // 1페이지
            // ================================================

            if (boss2Phase == 1)
            {
                yield return
                    StartCoroutine(
                        Boss2Phase1Attack()
                    );
            }


            // ================================================
            // 2페이지
            // ================================================

            else if (boss2Phase == 2)
            {
                SetBoss2BodyVisible(false);

                yield return
                    StartCoroutine(
                        Boss2Phase2Attack()
                    );
            }


            // ================================================
            // 3페이지
            // ================================================

            else if (boss2Phase == 3)
            {
                SetBoss2BodyVisible(false);

                yield return
                    StartCoroutine(
                        Boss2Phase3Attack()
                    );
            }

            if (currentState != BossState.Dead)
                currentState =
                    BossState.Move;

            yield break;
        }


        // =====================================================
        // Boss3
        // =====================================================

        if (bossType == BossType.Boss3)
        {
            if (spriteRenderer != null)
                spriteRenderer.color =
                    new Color(
                        0.5f,
                        0f,
                        0.5f
                    );

            yield return
                new WaitForSeconds(
                    0.8f /
                    Mathf.Max(
                        attackSpeed,
                        0.01f
                    )
                );

            if (spriteRenderer != null)
                spriteRenderer.color =
                    Color.white;

            if (currentState != BossState.Dead)
                currentState =
                    BossState.Move;
        }
    }


    // =========================================================
    // Boss2 버프 스킬 시작
    // =========================================================

    void StartBoss2BuffSkill()
    {
        if (bossType != BossType.Boss2)
            return;

        if (!useBuffSkill)
            return;

        if (buffSkillPrefab == null)
        {
            Debug.LogWarning(
                "[Boss2] 버프 스킬 프리팹이 지정되지 않았습니다."
            );

            return;
        }

        if (buffSkillRoutine != null)
            return;

        buffSkillRoutine =
            StartCoroutine(
                Boss2BuffSkillRoutine()
            );
    }


    // =========================================================
    // Boss2 버프 스킬
    // =========================================================

    IEnumerator Boss2BuffSkillRoutine()
    {
        activeBuffSkill =
            Instantiate(
                buffSkillPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

        activeBuffSkill.transform.localPosition =
            Vector3.zero;

        activeBuffSkill.transform.localRotation =
            Quaternion.identity;

        float elapsed = 0f;

        float safeTick =
            Mathf.Max(
                buffTickInterval,
                0.05f
            );


        // =====================================================
        // 버프 작동
        // =====================================================

        while (currentState != BossState.Dead)
        {
            if (boss2PersistentBuff)
            {
                // 2 / 3페이지 무제한
            }
            else
            {
                if (elapsed >= buffSkillDuration)
                    break;
            }


            // -------------------------------------------------
            // 보스 회복
            // -------------------------------------------------

            if (buffHealPerSecond > 0f)
            {
                float healAmount =
                    buffHealPerSecond * safeTick;

                HealFromAura(healAmount);

                Debug.Log(
                    $"<color=lime>" +
                    "[Boss2 버프 스킬]" +
                    "</color> " +
                    $"보스 회복 +{healAmount:F1}"
                );
            }


            // -------------------------------------------------
            // 플레이어 피해
            // -------------------------------------------------

            if (GameManager.instance != null &&
                GameManager.instance.player != null)
            {
                Transform player =
                    GameManager.instance.player.transform;

                float distance =
                    Vector2.Distance(
                        transform.position,
                        player.position
                    );

                if (distance <= buffSkillRange)
                {
                    PlayerHealth playerHealth =
                        player.GetComponent<PlayerHealth>();

                    if (playerHealth != null &&
                        buffDamagePerSecond > 0f)
                    {
                        float damageAmount =
                            buffDamagePerSecond * safeTick;

                        playerHealth.TakeBossBodyDamage(
                            damageAmount
                        );

                        Debug.Log(
                            $"<color=red>" +
                            "[Boss2 버프 스킬]" +
                            "</color> " +
                            $"사거리 {buffSkillRange:F1}m | " +
                            $"플레이어 피해 {damageAmount:F1}"
                        );
                    }
                }
            }


            yield return
                new WaitForSeconds(
                    safeTick
                );

            elapsed += safeTick;
        }


        DestroyActiveBuffSkill();

        buffSkillRoutine = null;

        lastBuffSkillTime =
            Time.time;
    }


    // =========================================================
    // 버프 프리팹 제거
    // =========================================================

    void DestroyActiveBuffSkill()
    {
        if (activeBuffSkill != null)
        {
            Destroy(
                activeBuffSkill
            );

            activeBuffSkill = null;
        }
    }


    // =========================================================
    // Boss2 모든 효과 제거
    // =========================================================

    void StopAllBoss2Effects()
    {
        boss2PersistentBuff = false;

        if (buffSkillRoutine != null)
        {
            StopCoroutine(
                buffSkillRoutine
            );

            buffSkillRoutine = null;
        }

        DestroyActiveBuffSkill();

        if (activeSlashObject != null)
        {
            Destroy(
                activeSlashObject
            );

            activeSlashObject = null;
        }

        if (activeSkillObject != null)
        {
            Destroy(
                activeSkillObject
            );

            activeSkillObject = null;
        }
    }


    // =========================================================
    // Boss2 1페이지
    // 버프 + 슬래시 → 딜레이 → 참격
    // =========================================================

    IEnumerator Boss2Phase1Attack()
    {
        if (spriteRenderer != null)
            spriteRenderer.enabled = true;


        boss2PersistentBuff = false;

        StartBoss2BuffSkill();


        if (animator != null)
            animator.SetTrigger("doSlash");


        if (slashPrefab != null)
        {
            activeSlashObject =
                Instantiate(
                    slashPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform
                );

            activeSlashObject.transform.localPosition =
                Vector3.zero;

            DisableOldSlashSkill(
                activeSlashObject
            );
        }


        if (phase1SlashDelay > 0f)
        {
            yield return
                new WaitForSeconds(
                    phase1SlashDelay
                );
        }


        if (phase1ProjectilePrefab != null)
        {
            StartCoroutine(
                FireProjectilePattern(
                    phase1ProjectilePrefab,
                    phase1SlashDuration,
                    phase1ShotInterval,
                    phase1SlashCount,
                    phase1SlashSpeed,
                    phase1SpawnDistance
                )
            );
        }


        ApplySlashInitialDamage();

        StartCoroutine(
            SlashDamageOverTime()
        );


        if (phase1SlashDuration > 0f)
        {
            yield return
                new WaitForSeconds(
                    phase1SlashDuration
                );
        }


        if (activeSlashObject != null)
        {
            Destroy(
                activeSlashObject
            );

            activeSlashObject = null;
        }


        if (buffSkillRoutine == null &&
            useBuffSkill &&
            buffSkillPrefab != null &&
            buffSkillCooldown > 0f)
        {
            yield return
                new WaitForSeconds(
                    buffSkillCooldown
                );
        }
    }


    // =========================================================
    // Boss2 2페이지
    // 버프 상시 + 참격 지속시간 → 쿨타임
    // =========================================================

    IEnumerator Boss2Phase2Attack()
    {
        SetBoss2BodyVisible(false);

        boss2PersistentBuff = true;

        StartBoss2BuffSkill();


        yield return
            StartCoroutine(
                FireProjectilePattern(
                    phase2ProjectilePrefab,
                    phase2ProjectileDuration,
                    phase2ShotInterval,
                    phase2SlashCount,
                    phase2SlashSpeed,
                    phase2SpawnDistance
                )
            );


        if (phase2ProjectileCooldown > 0f)
        {
            yield return
                new WaitForSeconds(
                    phase2ProjectileCooldown
                );
        }
    }


    // =========================================================
    // Boss2 3페이지
    // 버프 상시 + 기본 참격 + 추가 패턴
    // =========================================================

    IEnumerator Boss2Phase3Attack()
    {
        SetBoss2BodyVisible(false);

        boss2PersistentBuff = true;

        StartBoss2BuffSkill();


        if (usePhase3ExtraSlash &&
            phase3ExtraSlashPrefab != null &&
            Time.time >=
            lastPhase3ExtraSlashTime +
            phase3ExtraSlashCooldown)
        {
            lastPhase3ExtraSlashTime =
                Time.time;

            StartCoroutine(
                Boss2Phase3ExtraSlashRoutine()
            );
        }


        yield return
            StartCoroutine(
                FireProjectilePattern(
                    phase3ProjectilePrefab,
                    phase3ShotInterval,
                    phase3ShotInterval,
                    phase3SlashCount,
                    phase3SlashSpeed,
                    phase3SpawnDistance
                )
            );
    }


    // =========================================================
    // 3페이지 추가 슬래시
    // =========================================================

    IEnumerator Boss2Phase3ExtraSlashRoutine()
    {
        if (phase3ExtraSlashPrefab == null)
            yield break;


        GameObject extraSlash =
            Instantiate(
                phase3ExtraSlashPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );

        extraSlash.transform.localPosition =
            Vector3.zero;

        DisableOldSlashSkill(
            extraSlash
        );


        if (phase3ExtraSlashDelay > 0f)
        {
            yield return
                new WaitForSeconds(
                    phase3ExtraSlashDelay
                );
        }


        yield return
            StartCoroutine(
                FireProjectilePattern(
                    phase3ExtraProjectilePrefab,
                    phase3ExtraProjectileDuration,
                    phase3ExtraShotInterval,
                    phase3ExtraSlashCount,
                    phase3ExtraSlashSpeed,
                    phase3ExtraSpawnDistance
                )
            );


        if (extraSlash != null)
            Destroy(extraSlash);
    }


    // =========================================================
    // 공통 참격 발사
    // =========================================================

    IEnumerator FireProjectilePattern(
        GameObject projectilePrefab,
        float duration,
        float interval,
        int count,
        float projectileSpeed,
        float spawnDistance)
    {
        if (projectilePrefab == null)
            yield break;


        if (duration <= 0f)
        {
            FireProjectile(
                projectilePrefab,
                count,
                projectileSpeed,
                spawnDistance
            );

            yield break;
        }


        float elapsed = 0f;

        float safeInterval =
            Mathf.Max(
                interval,
                0.01f
            );


        while (elapsed < duration)
        {
            if (currentState == BossState.Dead)
                yield break;


            FireProjectile(
                projectilePrefab,
                count,
                projectileSpeed,
                spawnDistance
            );


            yield return
                new WaitForSeconds(
                    safeInterval
                );

            elapsed += safeInterval;
        }
    }


    // =========================================================
    // 참격 실제 생성
    // =========================================================

    void FireProjectile(
        GameObject projectilePrefab,
        int count,
        float projectileSpeed,
        float spawnDistance)
    {
        if (projectilePrefab == null)
            return;


        int safeCount =
            Mathf.Max(
                count,
                1
            );


        for (int i = 0;
             i < safeCount;
             i++)
        {
            float randomAngle =
                Random.Range(
                    0f,
                    360f
                );


            float radians =
                randomAngle *
                Mathf.Deg2Rad;


            Vector2 direction =
                new Vector2(
                    Mathf.Cos(radians),
                    Mathf.Sin(radians)
                ).normalized;


            Vector3 spawnPosition =
                transform.position +
                (Vector3)(
                    direction *
                    spawnDistance
                );


            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    randomAngle
                );


            GameObject projectile =
                Instantiate(
                    projectilePrefab,
                    spawnPosition,
                    rotation
                );


            Rigidbody2D rb =
                projectile.GetComponent<Rigidbody2D>();


            if (rb != null)
            {
                rb.linearVelocity =
                    direction *
                    projectileSpeed;
            }
        }
    }


    // =========================================================
    // 기존 Boss2SlashSkill 자동 실행 방지
    // =========================================================

    void DisableOldSlashSkill(
        GameObject slashObject)
    {
        if (slashObject == null)
            return;


        Boss2SlashSkill oldSkill =
            slashObject.GetComponent<Boss2SlashSkill>();


        if (oldSkill != null)
            oldSkill.enabled = false;
    }


    // =========================================================
    // Boss2 본체 표시
    // =========================================================

    void SetBoss2BodyVisible(
        bool visible)
    {
        if (bossType != BossType.Boss2)
            return;

        if (spriteRenderer != null)
            spriteRenderer.enabled = visible;
    }


    // =========================================================
    // ★ Boss2 폼 생성
    // =========================================================

    void SpawnBoss2Form(GameObject formPrefab)
    {
        if (bossType != BossType.Boss2)
            return;

        if (formPrefab == null)
        {
            Debug.LogWarning(
                "[Boss2 폼] 생성할 폼 프리팹이 지정되지 않았습니다."
            );

            return;
        }


        // 기존 폼 제거
        DestroyActivePhaseForm();


        // =====================================================
        // 실제 프리팹 Instantiate
        // =====================================================

        activePhaseForm =
            Instantiate(
                formPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );


        // =====================================================
        // Boss 몸 중심에 정확하게 부착
        // =====================================================

        activePhaseForm.transform.localPosition =
            Vector3.zero;

        activePhaseForm.transform.localRotation =
            Quaternion.identity;

        activePhaseForm.transform.localScale =
            Vector3.one;


        // =====================================================
        // 폼 내부 SpriteRenderer 전부 활성화
        // =====================================================

        SpriteRenderer[] formRenderers =
            activePhaseForm.GetComponentsInChildren<
                SpriteRenderer>(
                true
            );

        int rendererCount = 0;

        foreach (SpriteRenderer renderer in formRenderers)
        {
            if (renderer == null)
                continue;

            renderer.enabled = true;

            renderer.color = Color.white;

            rendererCount++;
        }


        Debug.Log(
            $"<color=cyan>" +
            $"[Boss2 폼 생성 성공]" +
            $"</color> " +
            $"프리팹: {formPrefab.name} | " +
            $"생성 오브젝트: {activePhaseForm.name} | " +
            $"부모: {activePhaseForm.transform.parent.name} | " +
            $"LocalPosition: {activePhaseForm.transform.localPosition} | " +
            $"SpriteRenderer: {rendererCount}개 활성화"
        );
    }


    // =========================================================
    // ★ 현재 폼 제거
    // =========================================================

    void DestroyActivePhaseForm()
    {
        if (activePhaseForm != null)
        {
            Debug.Log(
                $"[Boss2 폼 제거] {activePhaseForm.name}"
            );

            Destroy(
                activePhaseForm
            );

            activePhaseForm = null;
        }
    }


    // =========================================================
    // Boss2 1페이지 초기 피해
    // =========================================================

    void ApplySlashInitialDamage()
    {
        if (GameManager.instance == null ||
            GameManager.instance.player == null)
            return;


        Transform player =
            GameManager.instance.player.transform;


        float distance =
            Vector2.Distance(
                transform.position,
                player.position
            );


        if (distance <= slashRadius)
        {
            PlayerHealth playerHealth =
                player.GetComponent<PlayerHealth>();


            if (playerHealth != null)
            {
                playerHealth.TakeBossBodyDamage(
                    slashInitialDamage
                );


                Debug.Log(
                    $"<color=red>" +
                    $"[Boss2 주변 참격]" +
                    $"</color> 초기 피해 " +
                    $"{slashInitialDamage} 적용!"
                );
            }
        }
    }


    // =========================================================
    // Boss2 1페이지 지속 피해
    // =========================================================

    IEnumerator SlashDamageOverTime()
    {
        float elapsed = 0f;


        while (elapsed < slashDamageDuration)
        {
            yield return
                new WaitForSeconds(
                    slashTickInterval
                );


            elapsed += slashTickInterval;


            if (GameManager.instance == null ||
                GameManager.instance.player == null)
                continue;


            Transform player =
                GameManager.instance.player.transform;


            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position
                );


            if (distance <= slashRadius)
            {
                PlayerHealth playerHealth =
                    player.GetComponent<PlayerHealth>();


                if (playerHealth != null)
                {
                    playerHealth.TakeBossBodyDamage(
                        slashDamagePerSecond
                    );


                    Debug.Log(
                        $"<color=orange>" +
                        $"[Boss2 주변 참격]" +
                        $"</color> 지속 피해 " +
                        $"{slashDamagePerSecond} 적용!"
                    );
                }
            }
        }
    }


    // =========================================================
    // Boss2 기존 스킬 삭제
    // =========================================================

    void HideBoss2OldVisuals()
    {
        if (activeSlashObject != null)
        {
            Destroy(
                activeSlashObject
            );

            activeSlashObject = null;
        }


        if (activeSkillObject != null)
        {
            Destroy(
                activeSkillObject
            );

            activeSkillObject = null;
        }
    }


    // =========================================================
    // Boss2 폼체인지
    // =========================================================

    void ChangeBoss2Form(
        int phase)
    {
        if (bossType != BossType.Boss2)
            return;


        // =====================================================
        // 기존 폼 제거
        // =====================================================

        DestroyActivePhaseForm();


        // =====================================================
        // 기존 공격 이펙트 제거
        // =====================================================

        HideBoss2OldVisuals();


        // =====================================================
        // 1페이지
        // =====================================================

        if (phase == 1)
        {
            SetBoss2BodyVisible(true);

            boss2PersistentBuff = false;

            return;
        }


        // =====================================================
        // 2페이지
        // =====================================================

        if (phase == 2)
        {
            SetBoss2BodyVisible(false);


            // ★ 실제 프리팹 생성
            SpawnBoss2Form(
                phase2Form
            );


            boss2PersistentBuff = true;

            StartBoss2BuffSkill();

            return;
        }


        // =====================================================
        // 3페이지
        // =====================================================

        if (phase == 3)
        {
            SetBoss2BodyVisible(false);


            // ★ 실제 프리팹 생성
            SpawnBoss2Form(
                phase3Form
            );


            boss2PersistentBuff = true;

            StartBoss2BuffSkill();
        }
    }


    // =========================================================
    // Boss2 능력치 적용
    // =========================================================

    void ApplyBoss2PhaseStats(
        float multiplier,
        float defenseBonus)
    {
        maxHealth =
            baseMaxHealth *
            multiplier;

        speed =
            baseSpeed *
            multiplier;

        damage =
            baseDamage *
            multiplier;

        attackSpeed =
            baseAttackSpeed *
            multiplier;

        healthRegenPerSec =
            baseHealthRegen *
            multiplier;

        defense =
            baseDefense +
            defenseBonus;

        currentHealth =
            maxHealth;


        if (hpSlider != null)
        {
            hpSlider.maxValue =
                maxHealth;

            hpSlider.value =
                currentHealth;
        }


        Debug.Log(
            $"<color=cyan>" +
            $"[Boss2 능력치 적용]" +
            $"</color> " +
            $"배율 x{multiplier} | " +
            $"HP {maxHealth} | " +
            $"방어력 {defense}"
        );
    }


    // =========================================================
    // ★ Boss2 1페이지 보상
    // =========================================================

    void GiveBoss2Phase1Reward()
    {
        Debug.Log(
            "<color=yellow>" +
            "[Boss2 1페이지 보상 시작]" +
            "</color>"
        );


        // =====================================================
        // 플레이어 레벨 +1
        // =====================================================

        if (GameManager.instance != null)
        {
            GameManager.instance.level +=
                phase1RewardLevel;

            Debug.Log(
                $"<color=green>" +
                "[Boss2 1페이지 보상]" +
                "</color> " +
                $"플레이어 레벨 +{phase1RewardLevel}"
            );
        }


        // =====================================================
        // BloodHit 잠금해제
        // =====================================================

        if (unlockBloodHitOnPhase1)
        {
            if (onBoss2Phase1UnlockBloodHit != null)
            {
                onBoss2Phase1UnlockBloodHit.Invoke();

                Debug.Log(
                    "<color=magenta>" +
                    "[Boss2 1페이지 보상]" +
                    "</color> BloodHit 잠금해제 이벤트 실행!"
                );
            }
            else
            {
                Debug.LogWarning(
                    "<color=yellow>" +
                    "[Boss2 1페이지 보상]" +
                    "</color> " +
                    "BloodHit 잠금해제 이벤트가 Inspector에 연결되지 않았습니다."
                );
            }
        }


        // =====================================================
        // 아이템 선택
        // =====================================================

        LevelUp levelUpUI =
            GameObject.FindAnyObjectByType<LevelUp>();


        if (levelUpUI != null)
        {
            // 현재 프로젝트의 기존 보상 UI를 호출한다.
            levelUpUI.ShowBossReward();

            Debug.Log(
                "<color=cyan>" +
                "[Boss2 1페이지 보상]" +
                "</color> 아이템 보상 선택 UI 호출!"
            );
        }
        else
        {
            Debug.LogWarning(
                "[Boss2 1페이지 보상] LevelUp UI를 찾을 수 없습니다."
            );
        }
    }


    // =========================================================
    // Boss 데미지
    // =========================================================

    public void TakeDamage(
        float incomingDamage)
    {
        if (currentState ==
            BossState.Appearance)
            return;


        if (boss2PhaseChanging)
            return;


        float finalDamage =
            incomingDamage -
            defense;


        if (finalDamage < 1f)
            finalDamage = 1f;


        currentHealth -=
            finalDamage;


        if (hpSlider != null)
            hpSlider.value =
                currentHealth;


        Debug.Log(
            $"<color=orange>" +
            $"[보스 피격]" +
            $"</color> 받은 데미지: " +
            $"{finalDamage:F1} | " +
            $"남은 체력: " +
            $"{currentHealth:F1} / " +
            $"{maxHealth}"
        );


        if (currentHealth <= 0f)
        {
            currentHealth = 0f;


            // =================================================
            // Boss3 각성
            // =================================================

            if (bossType == BossType.Boss3 &&
                !isAwakened)
            {
                StartCoroutine(
                    AwakenRoutine()
                );

                return;
            }


            // =================================================
            // Boss2 페이지 전환
            // =================================================

            if (bossType == BossType.Boss2)
            {
                if (!boss2PhaseChanging)
                {
                    StartCoroutine(
                        Boss2PhaseDeathRoutine()
                    );
                }

                return;
            }


            // =================================================
            // 일반 사망
            // =================================================

            Die();
        }
    }


    // =========================================================
    // Boss2 페이지 사망
    // =========================================================

    IEnumerator Boss2PhaseDeathRoutine()
    {
        if (boss2PhaseChanging)
            yield break;


        boss2PhaseChanging = true;


        // =====================================================
        // AI 정지
        // =====================================================

        currentState =
            BossState.Dead;


        // =====================================================
        // 모든 공격 스킬 제거
        // =====================================================

        HideBoss2OldVisuals();


        // =====================================================
        // 1페이지 → 2페이지
        // =====================================================

        if (boss2Phase == 1)
        {
            Debug.Log(
                "<color=yellow>" +
                "[Boss2]</color> " +
                "1페이지 HP 0 → 2페이지 전환!"
            );


            // =================================================
            // 1페이지 버프 종료
            // =================================================

            boss2PersistentBuff = false;

            if (buffSkillRoutine != null)
            {
                StopCoroutine(
                    buffSkillRoutine
                );

                buffSkillRoutine = null;
            }

            DestroyActiveBuffSkill();


            // =================================================
            // ★ 1페이지 보상
            // 레벨 +1
            // BloodHit 해금
            // 아이템 보상
            // =================================================

            GiveBoss2Phase1Reward();


            // =================================================
            // 2페이지
            // =================================================

            boss2Phase = 2;


            ApplyBoss2PhaseStats(
                phase2BuffMultiplier,
                phase2DefenseBonus
            );


            ChangeBoss2Form(2);


            boss2PhaseChanging = false;

            currentState =
                BossState.Move;

            StartCoroutine(
                BossLoop()
            );

            yield break;
        }


        // =====================================================
        // 2페이지 → 3페이지
        // =====================================================

        if (boss2Phase == 2)
        {
            Debug.Log(
                "<color=red>" +
                "[Boss2]</color> " +
                "2페이지 종료 → 3페이지 분노!"
            );


            boss2Phase = 3;


            ApplyBoss2PhaseStats(
                phase3BuffMultiplier,
                phase2DefenseBonus +
                phase3DefenseBonus
            );


            ChangeBoss2Form(3);


            boss2PhaseChanging = false;

            currentState =
                BossState.Move;

            StartCoroutine(
                BossLoop()
            );

            yield break;
        }


        // =====================================================
        // 3페이지 → 완전 사망
        // =====================================================

        Debug.Log(
            "<color=red>" +
            "[Boss2]</color> " +
            "3페이지 종료 → 완전 사망!"
        );


        boss2PhaseChanging = false;


        Die();
    }


    // =========================================================
    // Boss3 각성
    // =========================================================

    IEnumerator AwakenRoutine()
    {
        isAwakened = true;

        currentState =
            BossState.Dead;


        float timer = 0f;

        Vector3 originalPos =
            transform.position;


        while (timer < 2f)
        {
            transform.position =
                originalPos +
                (Vector3)
                (
                    Random.insideUnitCircle *
                    0.1f
                );


            if (spriteRenderer != null)
            {
                spriteRenderer.color =
                    Color.Lerp(
                        Color.white,
                        Color.red,
                        timer / 2f
                    );
            }


            timer += Time.deltaTime;

            yield return null;
        }


        transform.position =
            originalPos;


        maxHealth *=
            awakenStatMultiplier;

        currentHealth =
            maxHealth;

        speed *=
            awakenStatMultiplier;

        damage *=
            awakenStatMultiplier;

        attackSpeed *=
            awakenStatMultiplier;

        defense *=
            awakenStatMultiplier;

        healthRegenPerSec *=
            awakenStatMultiplier;


        if (hpSlider != null)
        {
            hpSlider.maxValue =
                maxHealth;

            hpSlider.value =
                currentHealth;
        }


        meleeAttackRange *= 1.3f;
        meleeAttackRadius *= 1.3f;


        if (bossType == BossType.Boss3 &&
            phase2AuraEffect != null)
        {
            phase2AuraEffect.SetActive(true);
        }


        isPhase2OvertimeBuffActive =
            true;


        StartCoroutine(
            Phase2StatScaling()
        );


        currentState =
            BossState.Move;


        StartCoroutine(
            BossLoop()
        );
    }


    // =========================================================
    // Boss3 지속 성장
    // =========================================================

    IEnumerator Phase2StatScaling()
    {
        while (
            currentState != BossState.Dead &&
            isPhase2OvertimeBuffActive
        )
        {
            yield return
                new WaitForSeconds(5f);

            damage *= 1.05f;

            speed *= 1.02f;
        }
    }


    // =========================================================
    // Boss3 미니언 생성
    // =========================================================

    void SpawnMinions()
    {
        if (minionPrefab == null)
            return;


        if (spawnPoints == null ||
            spawnPoints.Length == 0)
            return;


        int spawnCount =
            Mathf.Min(
                3,
                spawnPoints.Length
            );


        for (int i = 0;
             i < spawnCount;
             i++)
        {
            if (spawnPoints[i] == null)
                continue;


            Instantiate(
                minionPrefab,
                spawnPoints[i].position,
                Quaternion.identity
            );
        }


        Debug.Log(
            "<color=cyan>" +
            "[Boss3]</color> 미니언 생성!"
        );
    }


    // =========================================================
    // 완전 사망
    // =========================================================

    void Die()
    {
        if (currentState == BossState.Dead &&
            bossType != BossType.Boss2)
        {
            return;
        }


        currentState =
            BossState.Dead;


        // =====================================================
        // Boss2 모든 스킬 종료
        // =====================================================

        if (bossType == BossType.Boss2)
        {
            boss2PersistentBuff = false;

            if (buffSkillRoutine != null)
            {
                StopCoroutine(
                    buffSkillRoutine
                );

                buffSkillRoutine = null;
            }

            DestroyActiveBuffSkill();

            HideBoss2OldVisuals();

            // ★ 실제 생성된 폼 제거
            DestroyActivePhaseForm();
        }


        // =====================================================
        // Boss3 버프 종료
        // =====================================================

        if (isPhase2OvertimeBuffActive)
        {
            isPhase2OvertimeBuffActive =
                false;

            if (phase2AuraEffect != null)
                phase2AuraEffect.SetActive(false);
        }


        // =====================================================
        // Boss1 보상
        // =====================================================

        if (bossType == BossType.Boss1)
        {
            if (GameManager.instance != null)
            {
                GameManager.instance.level += 5;

                Debug.Log(
                    "<color=green>" +
                    "[Boss1 보상]" +
                    "</color> 플레이어 레벨 +5"
                );
            }


            LevelUp levelUpUI =
                GameObject.FindAnyObjectByType<LevelUp>();


            if (levelUpUI != null)
            {
                levelUpUI.UnlockCouragePower();

                levelUpUI.ShowBossReward();


                Debug.Log(
                    "<color=cyan>" +
                    "[Boss1 보상]" +
                    "</color> 아이템 5회 선택!"
                );
            }
        }


        // =====================================================
        // Boss2 최종 사망
        // =====================================================

        if (bossType == BossType.Boss2)
        {
            HideBoss2OldVisuals();

            DestroyActivePhaseForm();

            SetBoss2BodyVisible(false);

            GiveBoss2FinalReward();
        }


        // =====================================================
        // 삭제
        // =====================================================

        StartCoroutine(
            SafeDestroyRoutine()
        );
    }


    // =========================================================
    // Boss2 최종 보상
    // =========================================================

    void GiveBoss2FinalReward()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.level += 5;


            Debug.Log(
                "<color=green>" +
                "[Boss2 최종 보상]" +
                "</color> 플레이어 레벨 +5"
            );
        }


        LevelUp levelUpUI =
            GameObject.FindAnyObjectByType<LevelUp>();


        if (levelUpUI != null)
        {
            levelUpUI.ShowBossReward();


            Debug.Log(
                "<color=cyan>" +
                "[Boss2 최종 보상]" +
                "</color> 아이템 5회 선택 시작!"
            );
        }
        else
        {
            Debug.LogWarning(
                "LevelUp UI를 찾을 수 없습니다."
            );
        }
    }


    // =========================================================
    // 안전 삭제
    // =========================================================

    IEnumerator SafeDestroyRoutine()
    {
        SpriteRenderer renderer =
            GetComponent<SpriteRenderer>();


        if (renderer != null)
            renderer.enabled = false;


        Collider2D bCollider =
            GetComponentInChildren<Collider2D>();


        if (bCollider != null)
            bCollider.enabled = false;


        yield return
            new WaitUntil(
                () =>
                    Time.timeScale > 0.1f
            );


        Time.timeScale = 1f;


        Destroy(gameObject);
    }


    // =========================================================
    // 플레이어 충돌
    // =========================================================

    void OnCollisionStay2D(
        Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") &&
            currentState != BossState.Appearance &&
            currentState != BossState.Dead)
        {
            PlayerHealth playerHealth =
                collision.gameObject
                .GetComponent<PlayerHealth>();


            if (playerHealth != null)
            {
                playerHealth.TakeBossBodyDamage(
                    damage
                );


                Debug.Log(
                    $"<color=red>" +
                    $"[성공]</color> " +
                    $"PlayerHealth 발견! " +
                    $"데미지 {damage} 전달 완료"
                );
            }
            else
            {
                Debug.LogWarning(
                    "플레이어 오브젝트에 " +
                    "PlayerHealth 스크립트가 없습니다!"
                );
            }
        }
    }


    // =========================================================
    // Gizmos
    // =========================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;


        Gizmos.DrawWireSphere(
            transform.position,
            meleeAttackRange
        );


        Gizmos.color =
            Color.red;


        Gizmos.DrawWireSphere(
            transform.position,
            meleeAttackRadius
        );


        Gizmos.color =
            Color.cyan;


        Gizmos.DrawWireSphere(
            transform.position,
            jumpAttackRadius
        );


        // =====================================================
        // Boss2 버프 범위
        // =====================================================

        if (bossType == BossType.Boss2)
        {
            Gizmos.color =
                Color.magenta;

            Gizmos.DrawWireSphere(
                transform.position,
                buffSkillRange
            );
        }
    }
}