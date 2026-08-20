using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("경험치")]
    public int currentExp = 0;

    [Header("체력 확인")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float maxHealth;

    private void Update()
    {
        if (GameManager.instance != null)
        {
            currentHealth = GameManager.instance.health;
            maxHealth = GameManager.instance.maxHealth;
        }
    }

    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"경험치 획득: {amount} (현재 경험치: {currentExp})");
    }
}