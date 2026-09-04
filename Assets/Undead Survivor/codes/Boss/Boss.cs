using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// 🌟 네임스페이스 충돌을 방지하기 위해 유니티에 꼭 필요한 3개만 남겼습니다.
public class Boss : MonoBehaviour
{
    public enum BossState { Appearance, Idle, Move, Attack_Melee, Attack_Ranged, Dead }

    [Header("========================================")]
    [Header("          [케르베로스 기본 설정]          ")]
    [Header("========================================")]
    public BossState currentState = BossState.Appearance;

    [Header("--- 능력치 ---")]
    public float maxHealth = 1000f;             // 최대 체력
    public float currentHealth;               // 현재 체력
    public float speed = 2f;                  // 이동 속도
    public float damage = 10f;                // 기본 데미지
    public float attackSpeed = 1.0f;          // 공격 속도 가속도
    public float defense = 5f;                // 방어력
    public float healthRegenPerSec = 1f;      // 초당 체력 회복량

    [Header("--- 사거리 설정 ---")]
    public float meleeAttackRange = 3f;   // 근접 공격 인식 거리
    public float meleeAttackRadius = 2f;  // 근접 공격 타격 범위

    [Header("========================================")]
    [Header("          [케르베로스 전용 패턴]          ")]
    [Header("========================================")]
    public GameObject fireBallPrefab;
    public GameObject lightningPrefab;
    public GameObject lavaPrefab;
    public float jumpAttackRadius = 2.5f;

    [Header("# 켈베로스 얼굴 스폰 포인트 (3개)")]
    [Tooltip("좌측 얼굴, 중앙 얼굴, 우측 얼굴 오브젝트를 각각 드래그해서 넣는 배열입니다.")]
    public Transform[] cerberusHeadPoints;

    [Header("# 켈베로스 개별 스킬 쿨타임 설정")]
    public float fireBallCooldown = 3.0f;     // 파이어볼 쿨타임 (초)
    public float lightningCooldown = 4.0f;   // 번개볼트 쿨타임 (초)
    public float jumpAttackCooldown = 2.0f;   // 점프 공격 쿨타임 (초)

    private float lastFireBallTime = 0f;
    private float lastLightningTime = 0f;
    private float lastJumpAttackTime = 0f;

    // 내부 컴포넌트 및 참조 변수
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Transform target;
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
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.transform;
        }

        currentHealth = maxHealth;

        GameObject sliderObj = GameObject.Find("Boss Health Bar");
        if (sliderObj != null)
        {
            sliderObj.SetActive(true);
            Slider sliderComponent = sliderObj.GetComponent<Slider>();
            if (sliderComponent != null)
            {
                SetupHPBar(sliderComponent);
            }
        }

        StartCoroutine(AppearanceRoutine());
        StartCoroutine(HealthRegenRoutine());
    }

    void Update()
    {
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth;
        }
    }

    void OnDestroy()
    {
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

        // 등장 연출 (깜빡임 효과 1.2초)
        for (int i = 0; i < 3; i++)
        {
            spriteRenderer.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(0.2f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(0.2f);
        }

        // 🌟 1. 등장 연출이 끝났으므로 상태를 Move로 확실하게 변경!
        currentState = BossState.Move;

        // 🌟 2. 상태 변경 로그 확인
        Debug.Log("🟢 [보스 등장 완료] 상태가 Move로 변경되었습니다. 이제 플레이어가 데미지를 받습니다.");

        // 🌟 3. 혹시 중복 실행되는 것을 방지하기 위해 기존 BossLoop를 멈추고 새롭게 확실히 시작
        StopCoroutine(nameof(BossLoop));
        StartCoroutine(BossLoop());
    }

    IEnumerator BossLoop()
    {
        while (currentState != BossState.Dead)
        {
            if (currentState == BossState.Appearance)
            {
                yield return new WaitForSeconds(0.1f);
                continue;
            }

            if (currentState == BossState.Attack_Melee || currentState == BossState.Attack_Ranged)
            {
                yield return null;
                continue;
            }

            if (Time.time >= lastJumpAttackTime + (jumpAttackCooldown / attackSpeed))
            {
                yield return StartCoroutine(Pattern_CerberusJumpAttack());
                continue;
            }
            else if (Time.time >= lastFireBallTime + (fireBallCooldown / attackSpeed))
            {
                yield return StartCoroutine(Pattern_CerberusFireBall());
                continue;
            }
            else if (Time.time >= lastLightningTime + (lightningCooldown / attackSpeed))
            {
                yield return StartCoroutine(Pattern_CerberusLightning());
                continue;
            }

            currentState = BossState.Move;
            float checkInterval = Random.Range(1.0f, 1.5f) / attackSpeed;
            float timer = 0f;

            while (timer < checkInterval)
            {
                if (currentState == BossState.Dead) break;

                if (Time.time >= lastJumpAttackTime + (jumpAttackCooldown / attackSpeed) ||
                    Time.time >= lastFireBallTime + (fireBallCooldown / attackSpeed) ||
                    Time.time >= lastLightningTime + (lightningCooldown / attackSpeed))
                    break;

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

            yield return null;
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

    IEnumerator Pattern_CerberusFireBall()
    {
        currentState = BossState.Attack_Ranged;
        lastFireBallTime = Time.time;

        if (target == null) { currentState = BossState.Move; yield break; }

        spriteRenderer.color = new Color(1f, 0.4f, 0f);
        yield return new WaitForSeconds(0.4f);
        spriteRenderer.color = Color.white;

        if (cerberusHeadPoints != null && cerberusHeadPoints.Length > 0)
        {
            foreach (Transform headPoint in cerberusHeadPoints)
            {
                if (headPoint == null) continue;
                Vector3 launchDir = (target.position - headPoint.position).normalized;
                float angle = Mathf.Atan2(launchDir.y, launchDir.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                GameObject go = Instantiate(fireBallPrefab, headPoint.position, rotation);
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = new Vector2(launchDir.x, launchDir.y) * 9f;
            }
        }

        yield return new WaitForSeconds(0.4f / attackSpeed);
        lastFireBallTime = Time.time;
        currentState = BossState.Move;
    }

    IEnumerator Pattern_CerberusLightning()
    {
        currentState = BossState.Attack_Ranged;
        lastLightningTime = Time.time;

        if (target == null) { currentState = BossState.Move; yield break; }

        spriteRenderer.color = new Color(0f, 0.8f, 1f);
        yield return new WaitForSeconds(0.4f);
        spriteRenderer.color = Color.white;

        if (cerberusHeadPoints != null && cerberusHeadPoints.Length > 0)
        {
            foreach (Transform headPoint in cerberusHeadPoints)
            {
                if (headPoint == null) continue;
                Vector3 launchDir = (target.position - headPoint.position).normalized;
                float angle = Mathf.Atan2(launchDir.y, launchDir.x) * Mathf.Rad2Deg;
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);

                GameObject go = Instantiate(lightningPrefab, headPoint.position, rotation);
                Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
                if (rb != null) rb.linearVelocity = new Vector2(launchDir.x, launchDir.y) * 6f;
            }
        }

        yield return new WaitForSeconds(0.4f / attackSpeed);
        lastLightningTime = Time.time;
        currentState = BossState.Move;
    }


    IEnumerator Pattern_CerberusJumpAttack()
    {
        currentState = BossState.Attack_Ranged;
        lastJumpAttackTime = Time.time;

        if (target == null) { currentState = BossState.Move; yield break; }

        spriteRenderer.color = new Color(0.4f, 0.4f, 0.4f);
        yield return new WaitForSeconds(0.3f);
        spriteRenderer.color = Color.white;

        if (animator != null)
        {
            animator.SetTrigger("Jump");
        }

        Vector3 jumpTargetPos = target.position;
        float jumpDuration = 0.5f;
        float elapsed = 0f;
        Vector3 startPos = transform.position;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;
            transform.position = Vector3.Lerp(startPos, jumpTargetPos, t);
            yield return null;
        }

        if (animator != null)
        {
            animator.SetTrigger("Land");
        }

        yield return new WaitForSeconds(0.6f / attackSpeed);
        lastJumpAttackTime = Time.time;
        currentState = BossState.Move;
    }

    // ========================================================
    // 🌟 [애니메이션 이벤트 수신기] Boss_Landing 애니메이션에서 호출함
    // ========================================================
    public void SpawnLava()
    {
        if (lavaPrefab != null)
        {
            // 보스의 현재 착지 발밑 위치에 용암 바닥 생성
            GameObject lavaFloor = Instantiate(lavaPrefab, transform.position, Quaternion.identity);

            // 4초 뒤 용암 바닥 삭제
            Destroy(lavaFloor, 4.0f);
        }
    }

    public void TakeDamage(float dmg, float poisonRate = 0f)
    {
        if (currentState == BossState.Appearance || currentState == BossState.Dead)
            return;

        float finalDamage = Mathf.Max(1f, dmg - defense);
        currentHealth -= finalDamage;

        Debug.Log("[보스 피격] 받은 피해: " + finalDamage.ToString("F1") + " | 남은 체력: " + currentHealth.ToString("F1") + "/" + maxHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(DeadRoutine());
        }
    }

    public float GetMaxHealth()
    {
        return maxHealth;
    }

    IEnumerator DeadRoutine()
    {
        currentState = BossState.Dead;
        Debug.Log("[보스 처치] 케르베로스가 처치되었습니다.");

        yield return new WaitForSeconds(1.5f);

        if (GameManager.instance != null)
        {
            GameManager.instance.GameVictroy();
        }

        Destroy(gameObject);
    }
}

