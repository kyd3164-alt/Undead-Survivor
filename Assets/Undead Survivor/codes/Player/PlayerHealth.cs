using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [Header("UI 연동")]
    public Slider hpSlider;

    [Header("Fire Debuff Settings")]
    public float fireDamageMultiplier = 2.0f;
    public float burnDuration = 3.0f;
    public float burnTickDamage = 5f;

    [Header("피격 쿨타임 (연속 피격 방지)")]
    public float bossDamageInterval = 0.5f;
    private float lastBossDamageTime = 0f;

    private bool isOnFire = false;
    private Coroutine burnCoroutine;
    private bool isDead = false;
    private float baseMaxHealth = 100f;

    private void Start()
    {
        if (GameManager.instance.maxHealth <= 0)
        {
            GameManager.instance.maxHealth = baseMaxHealth;
            GameManager.instance.health = baseMaxHealth;
        }

        UpdateHpUI();
    }

    private void Update()
    {
        UpdateHpUI();
    }

    // 💥 일반 적(Enemy) 등에게 데미지를 입을 때 호출하는 함수
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        GameManager.instance.health -= damage;
        Debug.Log($"💥 [일반 적 피격] -{damage:F1} 데미지 | 남은 HP: {GameManager.instance.health:F1}");

        CheckIsDead();
    }

    // 보스 몸 안에서 받는 데미지 처리
    public void TakeBossBodyDamage(float baseDamage)
    {
        if (isDead) return;

        if (Time.time - lastBossDamageTime < bossDamageInterval)
            return;

        lastBossDamageTime = Time.time;

        float finalDamage = baseDamage;

        if (isOnFire)
        {
            finalDamage *= fireDamageMultiplier;
            Debug.Log($"🔥 [{fireDamageMultiplier}배 폭딜!] {finalDamage} 데미지 받음");
        }
        else
        {
            Debug.Log($"💥 [보스 몸체 딜] {finalDamage} 데미지 받음");
        }

        GameManager.instance.health -= finalDamage;
        CheckIsDead();

        ApplyBurnDebuff(burnDuration, burnTickDamage);
    }

    public void ApplyBurnDebuff(float duration, float tickDamage)
    {
        isOnFire = true;
        if (burnCoroutine != null) StopCoroutine(burnCoroutine);
        burnCoroutine = StartCoroutine(BurnDebuffRoutine(duration, tickDamage));
    }

    private IEnumerator BurnDebuffRoutine(float duration, float tickDamage)
    {
        float timer = 0f;
        while (timer < duration && !isDead)
        {
            yield return new WaitForSeconds(1.0f);
            GameManager.instance.health -= tickDamage;
            timer += 1.0f;
            Debug.Log($"🔥 [화염 지속 도트딜] -{tickDamage} (남은 HP: {GameManager.instance.health})");

            CheckIsDead();
        }

        isOnFire = false;
        Debug.Log("불길이 꺼졌습니다.");
    }

    void UpdateHpUI()
    {
        if (hpSlider != null && GameManager.instance.maxHealth > 0)
        {
            hpSlider.value = GameManager.instance.health / GameManager.instance.maxHealth;
        }
    }

    private void CheckIsDead()
    {
        if (GameManager.instance.health <= 0 && !isDead)
        {
            GameManager.instance.health = 0;
            isDead = true;
            UpdateHpUI();
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("💀 플레이어 사망!");

        if (GameManager.instance != null)
        {
            Player playerScript = GetComponent<Player>();
            if (playerScript != null)
            {
                for (int index = 2; index < transform.childCount; index++)
                {
                    transform.GetChild(index).gameObject.SetActive(false);
                }

                Animator anim = GetComponent<Animator>();
                if (anim != null) anim.SetTrigger("Dead");
            }

            GameManager.instance.GameOver();
        }
    }

    public void IncreaseMaxHp(float percent)
    {
        // 항상 기본 최대 HP 100을 기준으로 증가량 계산
        float increaseAmount = baseMaxHealth * (percent / 100f);

        GameManager.instance.maxHealth += increaseAmount;
        GameManager.instance.health += increaseAmount;

        Debug.Log($"❤️ HP 증가!");
        Debug.Log($"증가율: {percent}%");
        Debug.Log($"증가량: +{increaseAmount}");
        Debug.Log($"현재 HP: {GameManager.instance.health}");
        Debug.Log($"최대 HP: {GameManager.instance.maxHealth}");

        UpdateHpUI();
    }

    public void Heal(float amount)
    {
        if (isDead) return;

        GameManager.instance.health += amount;
        if (GameManager.instance.health > GameManager.instance.maxHealth)
            GameManager.instance.health = GameManager.instance.maxHealth;
        UpdateHpUI();
    }
}