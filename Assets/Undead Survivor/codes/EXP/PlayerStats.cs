using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public int currentExp = 0;

    public void AddExp(int amount)
    {
        currentExp += amount;
        Debug.Log($"경험치 획득: {amount} (현재 경험치: {currentExp})");
    }
}
