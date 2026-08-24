using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI 슬라이더를 사용하기 위해 추가

public class Boss : MonoBehaviour
{
    public enum BossType { Boss1, Boss2, Boss3 }
    public enum BossState { Appearance, Idle, Move, Attack_Melee, Attack_Ranged, Dead }

    [Header("========================================")]
    [Header("            [보스 공통 설정]            ")]
    [Header("========================================")]
    public BossType bossType;
    public BossState currentState = BossState.Appearance;

    [Header("--- 공통 능력치 ---")]
    public float maxHealth = 1000f;             // 최대 체력
    public float currentHealth;               // 현재 체력
    public float speed = 2f;                  // 이동 속도
    public float damage = 10f;                // 기본 데미지
    public float attackSpeed = 1.0f;          // 공격 속도 가속도
    public float defense = 5f;                // 방어력
    public float healthRegenPerSec = 1f;      // 초당 체력 회복량

    [Header("--- 공통 사거리 & 쿨타임 ---")]
    public float meleeAttackRange = 3f;   // 근접 공격 인식 거리
    public float meleeAttackRadius = 2f;  // 근접 공격 타격 범위
    public float rangedAttackCooldown = 5f; // 원거리/스킬 공격 쿨타임
    private float lastRangedAttackTime;

    [Header("========================================")]
    [Header("       [Boss 1 : 케르베로스 전용]        ")]
    [Header("========================================")]
    public Transform[] firePoints;
    public GameObject fireBallPrefab;
    public GameObject lightningPrefab;
    public float jumpAttackRadius = 2.5f;
    public GameObject lavaPrefab;
    public float lavaDuration = 3f;
    public float lavaDamage = 5f;
    public float lavaRadius = 3f; // 💡 장판 크기에 맞춰 타격 범위도 2 -> 3으로 변경
    public float slowDebuffAmount = 0.5f;
    public float debuffDuration = 2f;

    [Header("========================================")]
    [Header("       [Boss 2 : 악천 전용]        ")]
    [Header("========================================")]

    [Tooltip("Boss2의 주변 검 휘두르기 연출 프리팹")]
    public GameObject slashPrefab;

    [Tooltip("Boss2의 범위 공격 + 회복 오라 프리팹")]
    public GameObject skillPrefab;

    [Header("--- Boss2 주변 참격 피해 설정 ---")]

    [Tooltip("주변 참격이 피해를 주는 범위")]
    public float slashRadius = 4f;

    [Tooltip("참격이 처음 발생할 때 들어가는 강한 피해")]
    public float slashInitialDamage = 50f;

    [Tooltip("초기 피해 이후 지속적으로 들어가는 피해")]
    public float slashDamagePerSecond = 10f;

    [Tooltip("지속 피해를 몇 초마다 한 번 적용할지")]
    public float slashTickInterval = 0.5f;

    [Tooltip("지속 피해가 유지되는 시간")]
    public float slashDamageDuration = 3f;

    [Header("========================================")]
    [Header("         [Boss 3 : 드래곤 각성 전용]      ")]
    [Header("========================================")]
    [Range(1f, 3f)] public float awakenStatMultiplier = 1.5f;
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public GameObject phase2AuraEffect;
    [SerializeField] private bool isPhase2OvertimeBuffActive = false;

    private bool isAwakened = false;
    private SpriteRenderer spriteRenderer;
    private Animator animator; // 애니메이터 컴포넌트
    private Transform target;

    // 💡 착지 목표 지점을 기억할 위치 변수
    private Vector3 lastLandingPosition;

    // 꽂아줄 변수와 함수 (여기에 넣어주세요!)
    private Slider hpSlider;

    public void SetupHPBar(Slider slider)
    {
        hpSlider = slider;
        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }
    }

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>(); // 애니메이터 가져오기
    }

    void Start()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.transform;
        }

        currentHealth = maxHealth;

        // [자동 연동] 메인 캔버스 안의 "Boss Health Bar"를 코드로 자동 탐색 및 활성화
        GameObject sliderObj = GameObject.Find("Boss Health Bar");
        if (sliderObj != null)
        {
            sliderObj.SetActive(true); // 보스가 등장하면 체력바 켜기
        }

        StartCoroutine(AppearanceRoutine());
        StartCoroutine(HealthRegenRoutine());
    }

    void Update()
    {
        // 1. [체력바 실시간 동기화] 스포너가 꽂아준 hpSlider가 존재할 때만 실시간 체력 수치를 동기화합니다.
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }
    }



    void OnDestroy()
    {
        // [자동 정리] 보스가 죽거나 사라질 때 메인 체력바 다시 끄기
        GameObject sliderObj = GameObject.Find("Boss Health Bar");
        if (sliderObj != null)
        {
            sliderObj.SetActive(false);
        }
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

    IEnumerator BossLoop()
    {
        while (currentState != BossState.Dead)
        {
            currentState = BossState.Move;

            float checkInterval = Random.Range(1.5f, 3f) / attackSpeed;
            float timer = 0f;

            while (timer < checkInterval)
            {
                MoveTowardsPlayer();

                if (target != null)
                {
                    float distanceToPlayer = Vector2.Distance(transform.position, target.position);
                    if (distanceToPlayer <= meleeAttackRange)
                    {
                        yield return StartCoroutine(Pattern_MeleeSlash());
                        break;
                    }
                }

                timer += Time.deltaTime;
                yield return null;
            }

            if (currentState == BossState.Move && Time.time >= lastRangedAttackTime + (rangedAttackCooldown / attackSpeed))
            {
                if (bossType == BossType.Boss1)
                {
                    yield return StartCoroutine(Pattern_CerberusSkills());
                }
                else
                {
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

    IEnumerator Pattern_MeleeSlash()
    {
        currentState = BossState.Attack_Melee;

        spriteRenderer.color = new Color(1f, 0.5f, 0f);
        yield return new WaitForSeconds(0.5f / attackSpeed);
        spriteRenderer.color = Color.white;

        if (target != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, target.position);
            if (distanceToPlayer <= meleeAttackRadius)
            {
                Debug.Log("보스 근접 타격 성공!");
            }
        }

        yield return new WaitForSeconds(0.5f / attackSpeed);
        currentState = BossState.Move;
    }

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
                Vector2 dirToPlayer = target != null ? (target.position - (Vector3)spawnPos).normalized : Vector2.down;

                GameObject fireBall = Instantiate(fireBallPrefab, spawnPos, Quaternion.identity);
                Rigidbody2D rb = fireBall.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = dirToPlayer * 6f;
                }
            }
        }
        else if (patternType == 1 && lightningPrefab != null)
        {
            spriteRenderer.color = new Color(0.3f, 0.5f, 1f);
            yield return new WaitForSeconds(0.5f / attackSpeed);
            spriteRenderer.color = Color.white;

            for (int i = 0; i < firePoints.Length; i++)
            {
                if (firePoints[i] == null) continue;

                // [수정 완료] 이제 플레이어 위치가 아닌 보스 입 위치(firePoints[i].position)에서 생성됩니다!
                Vector3 spawnPos = firePoints[i].position;
                Instantiate(lightningPrefab, spawnPos, Quaternion.identity);

                // 동시에 발사되면 어색하므로 0.15초씩 간격을 두고 발사
                yield return new WaitForSeconds(0.15f);
            }
        }
        else
        {
            yield return StartCoroutine(Pattern_CerberusJumpAttack());
        }

        yield return new WaitForSeconds(0.6f / attackSpeed);
        currentState = BossState.Move;
    }

    IEnumerator Pattern_CerberusJumpAttack()
    {
        if (target == null) yield break;

        // 1. 점프 시작 애니메이션 트리거 실행 (DoJump)
        if (animator != null) animator.SetTrigger("DoJump");

        // 점프 준비 동작 시간 (올라가는 모션 감상)
        yield return new WaitForSeconds(0.4f / attackSpeed);

        Vector3 startPos = transform.position;
        // 💡 플레이어의 현재 위치를 최종 착지 목표 위치로 저장
        lastLandingPosition = target.position;

        float timer = 0f;
        float jumpDuration = 0.5f / attackSpeed;

        // 공중으로 이동하는 동안
        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, lastLandingPosition, timer / jumpDuration);
            yield return null;
        }

        // 2. 착지 모션 트리거 실행 (Land)
        if (animator != null) animator.SetTrigger("Land");

        // 착지 시 직격 디버프 판정
        float distanceToPlayer = Vector2.Distance(transform.position, target.position);
        if (distanceToPlayer <= jumpAttackRadius)
        {
            ApplyDebuff(target.gameObject);
        }

        yield return new WaitForSeconds(0.4f / attackSpeed);
    }

    // 💡 착지 애니메이션 이벤트(Animation Event)에서 호출되는 용암 생성 함수
    public void SpawnLava()
    {
        if (lavaPrefab != null)
        {
            // 1. 보스가 착지한 위치(lastLandingPosition)에 용암 생성
            GameObject lava = Instantiate(lavaPrefab, lastLandingPosition, Quaternion.identity);

            // 2. 💡 애니메이션 파일의 Scale을 무시하고 강제로 3배 확대 적용
            lava.transform.localScale = new Vector3(3f, 3f, 1f);

            StartCoroutine(LavaRoutine(lava));
        }
    }

    // 💡 3초 지속 후 DoEnd 트리거를 실행하고 소멸하는 코루틴
    IEnumerator LavaRoutine(GameObject lava)
    {
        float elapsed = 0f;
        Animator lavaAnim = lava.GetComponent<Animator>();

        while (elapsed < lavaDuration && lava != null)
        {
            if (target != null)
            {
                float distToPlayer = Vector2.Distance(lava.transform.position, target.position);
                if (distToPlayer <= lavaRadius)
                {
                    Debug.Log($"용암 장판 도트 데미지 적용: {lavaDamage}");
                }
            }

            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }

        if (lava != null)
        {
            // 1. 사라지는 애니메이션(Lava_End) 트리거 실행
            if (lavaAnim != null)
            {
                lavaAnim.SetTrigger("DoEnd");
            }

            // 2. Lava_End 애니메이션이 끝날 시간(약 0.5초) 대기 후 오브젝트 삭제
            yield return new WaitForSeconds(0.5f);
            Destroy(lava);
        }
    }

    void ApplyDebuff(GameObject targetObj)
    {
        Debug.Log($"이동속도 {slowDebuffAmount * 100}% 감소 디버프 적용!");
    }

    IEnumerator Pattern_RangedShotgun()
    {
        // 1. 공격 상태로 전환 및 시간 기록
        currentState = BossState.Attack_Ranged;
        lastRangedAttackTime = Time.time;

        // 공격 전 전조 증상 딜레이 (0.8초 동안 기 모으기)
        spriteRenderer.color = new Color(0.5f, 0f, 0.5f); // 보라색으로 충전 이펙트 느낌
        yield return new WaitForSeconds(0.8f / attackSpeed);
        spriteRenderer.color = Color.white;

        // 2. 50% 확률로 슬래시 공격을 하거나, 자가 버프 스킬을 시전합니다.
        if (Random.value > 0.5f)
        {
            // ==========================================
            // [패턴 A: 오로라 주변 참격]
            // ==========================================

            if (animator != null)
            {
                animator.SetTrigger("doSlash");
            }

            // 보스 몸 그래픽 숨기기
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Boss2에 붙어서 따라다니는 슬래시 스킬 생성
            if (slashPrefab != null)
            {
                GameObject slash = Instantiate(
                    slashPrefab,
                    transform.position,
                    Quaternion.identity,
                    transform
                );

                // 생성된 SlashPrefab의 몸 그래픽 숨기기
                SpriteRenderer slashRenderer =
                    slash.GetComponent<SpriteRenderer>();

                if (slashRenderer != null)
                {
                    // 처음에는 보이게
                    slashRenderer.enabled = true;

                    // 2초 후 몸 그래픽 숨기기
                    StartCoroutine(HideSlashBodyAfterDelay(slashRenderer, 2f));
                }
            }

            // 주변 순간 피해
            ApplySlashInitialDamage();

            // 주변 지속 피해
            StartCoroutine(SlashDamageOverTime());

            // 2초 후 보스 몸 다시 표시
            StartCoroutine(RestoreBossSpriteAfterSlash());
        }
        else
        {
            // ==========================================
            // [패턴 B: 오로라 버프 스킬 시전]
            // ==========================================
            // 애니메이터에서 세팅한 doSkill 트리거 발동 (보스 버프 모션 취함)
            animator.SetTrigger("doSkill");

            // 보스 몸 그래픽 숨기기
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            if (skillPrefab != null)
            {
                // 보스 몸 위치에 버프 이펙트 프리팹 소환 (보스를 쫓아다니게 하려면 transform을 부모로 설정)
                GameObject skill = Instantiate(skillPrefab, transform.position, Quaternion.identity, transform);

                // 보스 자체 능력치 강화 (예: 공격 속도 일시 증가)
                attackSpeed += 0.2f;

                // 3초 뒤에 능력치 원상복구
                StartCoroutine(ResetBuff(3f));

                // 버프 이펙트의 SpriteRenderer 가져오기
                SpriteRenderer skillRenderer =
                    skill.GetComponent<SpriteRenderer>();

                // 3초 후 이펙트 SpriteRenderer 끄기
                StartCoroutine(EndBuffVisual(
                    skillRenderer,
                    3f
                ));
            }
        }

        // 3. 공격 후딜레이 후 다시 이동 상태로 복귀
        yield return new WaitForSeconds(0.8f / attackSpeed);
        currentState = BossState.Move;
    }

    // ==========================================
    // 버프 스킬 종료 후 버프 그래픽 끄기
    // + 보스 몸 그래픽 다시 표시
    // ==========================================
    IEnumerator EndBuffVisual(
    SpriteRenderer skillRenderer,
    float delay
)
    {
        yield return new WaitForSeconds(delay);

        // 버프 이펙트 그래픽 끄기
        if (skillRenderer != null)
        {
            skillRenderer.enabled = false;
        }

        // 보스 몸 그래픽 다시 켜기
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    // ==========================================
    // 슬래시 종료 후 보스 몸 다시 표시
    // ==========================================
    IEnumerator RestoreBossSpriteAfterSlash()
    {
        yield return new WaitForSeconds(2f);

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    IEnumerator HideSlashBodyAfterDelay(SpriteRenderer slashRenderer, float delay)
    {
        yield return new WaitForSeconds(delay);

        if (slashRenderer != null)
        {
            slashRenderer.enabled = false;
        }
    }

    void ApplySlashInitialDamage()
    {
        if (GameManager.instance == null ||
            GameManager.instance.player == null)
        {
            return;
        }

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
                    $"<color=red>[Boss2 주변 참격]</color> " +
                    $"초기 피해 {slashInitialDamage} 적용!"
                );
            }
        }
    }

    IEnumerator SlashDamageOverTime()
    {
        float elapsed = 0f;

        while (elapsed < slashDamageDuration)
        {
            yield return new WaitForSeconds(
                slashTickInterval
            );

            elapsed += slashTickInterval;

            if (GameManager.instance == null ||
                GameManager.instance.player == null)
            {
                continue;
            }

            Transform player =
                GameManager.instance.player.transform;

            float distance =
                Vector2.Distance(
                    transform.position,
                    player.position
                );

            // 지속 피해 범위 안에 플레이어가 있는지 확인
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
                        $"<color=orange>[Boss2 주변 참격]</color> " +
                        $"지속 피해 {slashDamagePerSecond} 적용!"
                    );
                }
            }
        }
    }

    // 버프 지속시간이 끝나면 보스 능력치를 원래대로 돌려놓는 서브 기능입니다.
    IEnumerator ResetBuff(float delay)
    {
        yield return new WaitForSeconds(delay);
        attackSpeed -= 0.2f; // 증가했던 공격 속도 차감
    }


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
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    public void HealFromAura(float amount)
    {
        if (currentState == BossState.Dead)
            return;

        if (amount <= 0f)
            return;

        currentHealth += amount;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        Debug.Log(
            $"<color=green>[Boss2 회복]</color> " +
            $"회복량: {amount:F1} | " +
            $"현재 체력: {currentHealth:F1} / {maxHealth}"
        );
    }

    // 외부(무기 등)에서 데미지를 줄 때 호출하는 함수
    public void TakeDamage(float incomingDamage)
    {
        if (currentState == BossState.Appearance) return;

        float finalDamage = incomingDamage - defense;
        if (finalDamage < 1f) finalDamage = 1f;

        Debug.Log(
            $"<color=orange>[Boss 피해 계산]</color> " +
            $"들어온 피해: {incomingDamage:F1} | " +
            $"방어력: {defense:F1} | " +
            $"최종 피해: {finalDamage:F1} | " +
            $"현재 HP: {currentHealth:F1} / {maxHealth:F1}"
        );

        currentHealth -= finalDamage;

        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            if (bossType == BossType.Boss3 && !isAwakened) StartCoroutine(AwakenRoutine());
            else Die();
        }
    }

    IEnumerator AwakenRoutine()
    {
        isAwakened = true;
        currentState = BossState.Dead;

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

        if (hpSlider != null)
        {
            hpSlider.maxValue = maxHealth;
            hpSlider.value = currentHealth;
        }

        meleeAttackRange *= 1.3f;
        meleeAttackRadius *= 1.3f;

        if (bossType == BossType.Boss3 && phase2AuraEffect != null)
        {
            phase2AuraEffect.SetActive(true);
        }

        isPhase2OvertimeBuffActive = true;
        StartCoroutine(Phase2StatScaling());

        currentState = BossState.Move;
        StartCoroutine(BossLoop());
    }
    
    IEnumerator Phase2StatScaling()
    {
        while (currentState != BossState.Dead && isPhase2OvertimeBuffActive)
        {
            yield return new WaitForSeconds(5.0f);
            damage *= 1.05f;
            speed *= 1.02f;
        }
    }

    void Die()
    {
        // 🚨 [안전장치] 중복 사망 연출 방지
        if (currentState == BossState.Dead) return;
        currentState = BossState.Dead;

        if (isPhase2OvertimeBuffActive)
        {
            isPhase2OvertimeBuffActive = false;
            if (phase2AuraEffect != null) phase2AuraEffect.SetActive(false);
        }

        // 🎯 1번 보스(케르베로스) 처치 시 연출
        if (bossType == BossType.Boss1)
        {
            // 1. 플레이어 레벨을 강제로 5단계 먼저 올려줍니다.
            GameManager.instance.level += 5;

            LevelUp levelUpUI = GameObject.FindAnyObjectByType<LevelUp>();
            if (levelUpUI != null)
            {
                levelUpUI.UnlockItem(6); // 6번 아이템 슬롯 해금
                levelUpUI.ShowBossReward();        // 5연속 레벨업 선택창 최초 활성화 (여기서 시간 스케일 0 고정됨)
            }
        }

        // =========================================================
        // 🚨 [누락 방지 핵심 치트 코드]
        // Destroy(gameObject)를 바로 해버리면 스포너 대기 락이 풀려버립니다!
        // 보스 오브젝트를 즉시 파괴하지 않고, 렌더러와 충돌체만 투명하게 숨겨둔 뒤
        // 코루틴(대기 루틴)을 실행해 유저가 아이템 5개를 다 골라 시간(TimeScale)이 1로 복구될 때까지 
        // 확실하게 기다렸다가 완전히 파괴 처리합니다.
        // =========================================================
        StartCoroutine(SafeDestroyRoutine());
    }

    // 🚨 보스가 죽은 뒤 레벨업 5번이 다 끝날 때까지 대기해주는 무적의 안전 코루틴
    IEnumerator SafeDestroyRoutine()
    {
        // 1. 보스의 몸통 이미지와 부딪히는 히트박스를 꺼서 유령 상태로 만듭니다.
        GetComponent<SpriteRenderer>().enabled = false;
        Collider2D bCollider = GetComponentInChildren<Collider2D>();
        if (bCollider != null) bCollider.enabled = false;

        // 2. 🚨 유저가 5번 연속 아이템을 다 골라서 유니티 시간이 정상 배속(1f)으로 돌아올 때까지 멈춰서 무조건 대기합니다!
        yield return new WaitUntil(() => Time.timeScale > 0.1f);

        // 3. 아이템 가챠가 완전히 끝났으므로 유니티 전체 배속을 확실히 풀고 보스를 완전히 삭제합니다.
        Time.timeScale = 1.0f;
        Destroy(gameObject); // 이 순간 Spawner 코루틴의 boss == null이 감지되어 다음 웨이브로 넘어갑니다!
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentState != BossState.Appearance)
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeBossBodyDamage(damage);
                Debug.Log($"<color=red>[성공]</color> PlayerHealth 발견! 데미지 {damage} 전달 완료");
            }
            else
            {
                Debug.LogWarning("⚠️ 플레이어 오브젝트에 PlayerHealth 스크립트가 없습니다!");
            }
        }
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