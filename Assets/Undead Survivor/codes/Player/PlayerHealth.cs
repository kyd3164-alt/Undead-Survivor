using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Player Health")]
    public float maxHp = 100f;
    public float currentHp = 100f;

    [Header("Fire Debuff Settings")]
    [Tooltip("몸체 안에서 비빌 때 딜 증폭 배율 (2.0 = 2배)")]
    public float fireDamageMultiplier = 2.0f;

    [Tooltip("화염 디버프 지속 시간 (초)")]
    public float burnDuration = 3.0f;

    [Tooltip("화염 지속 딜 (1초마다 들어가는 딜)")]
    public float burnTickDamage = 5f;

    private bool isOnFire = false;
    private Coroutine burnCoroutine;

    private void Start()
    {
        currentHp = maxHp;
    }

    // 보스 몸 안(장판)에서 받는 데미지 처리
    public void TakeBossBodyDamage(float baseDamage)
    {
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

        currentHp -= finalDamage;
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
        while (timer < duration)
        {
            yield return new WaitForSeconds(1.0f);
            currentHp -= tickDamage;
            timer += 1.0f;
            Debug.Log($"🔥 [화염 지속 도트딜] -{tickDamage} (남은 HP: {currentHp})");
        }

        isOnFire = false;
        Debug.Log("불길이 꺼졌습니다.");
    }

    // 💚 외부(MaxHpSkill 스크립트 등)에서 정해준 수치만큼 최대 체력을 늘려주는 함수
    public void IncreaseMaxHp(float amount)
    {
        maxHp += amount;
        currentHp += amount;

        Debug.Log($"💚 [최대 체력 강화!] +{amount} 증가 -> (최대 HP: {maxHp} | 현재 HP: {currentHp})");
    }

    // 🧪 체력 회복 함수
    public void Heal(float amount)
    {
        currentHp += amount;
        if (currentHp > maxHp) currentHp = maxHp;

        Debug.Log($"🧪 [체력 회복] 현재 HP: {currentHp}/{maxHp}");
    }
}