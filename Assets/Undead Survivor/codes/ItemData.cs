using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptble Object/ItemDate")]
public class ItemData : ScriptableObject
{
    // =========================================================
    // Item Type
    // =========================================================

    public enum ItemType
    {
        Melee,
        Range,
        GunDamage,
        Glove,
        Shoe,
        Heal,
        Health,
        BloodHit,
        HopeOfHope
    }


    // =========================================================
    // Item Effect
    // =========================================================

    [System.Serializable]
    public class ItemEffect
    {
        [Tooltip("아이템 효과 종류")]
        public ItemType itemType;

        [Tooltip("레벨별 효과 수치")]
        public float[] values;
    }


    // =========================================================
    // Main Info
    // =========================================================

    [Header("# Main Info")]

    // 기존 코드와의 호환성을 위해 유지
    public ItemType itemType;

    public int itemId;

    public string itemName;

    [TextArea]
    public string itemDesc;

    public Sprite itemIcon;


    // =========================================================
    // Item Effects
    // =========================================================

    [Header("# Item Effects")]

    [Tooltip("하나의 아이템이 가질 수 있는 여러 효과")]
    public ItemEffect[] itemEffects;


    // =========================================================
    // Level Data
    // =========================================================

    [Header("# Level Data")]

    [Tooltip("기본 데미지")]
    public float baseDamage;

    [Tooltip("기본 투사체 개수")]
    public int baseCount;

    [Tooltip("레벨별 데미지")]
    public float[] damages;

    [Tooltip("레벨별 투사체 개수")]
    public int[] counts;


    // =========================================================
    // Weapon
    // =========================================================

    [Header("# Weapon")]

    public GameObject projectile;

    public Sprite hand;


    // =========================================================
    // 최대 레벨
    // =========================================================
    //
    // Damages의 Size와
    // Item Effects > Values의 Size 중
    // 가장 큰 값을 최대 레벨로 사용한다.
    //
    // 예:
    //
    // Damages = Size 5
    //
    // Item Effects
    // ├─ Range      Values Size 5
    // └─ GunDamage  Values Size 5
    //
    // → 최대 레벨 = 5
    //
    // =========================================================

    public int GetMaxLevel()
    {
        int maxLevel = 0;


        // -----------------------------------------------------
        // Damages
        // -----------------------------------------------------

        if (damages != null)
        {
            maxLevel =
                Mathf.Max(
                    maxLevel,
                    damages.Length
                );
        }


        // -----------------------------------------------------
        // Item Effects
        // -----------------------------------------------------

        if (itemEffects != null)
        {
            foreach (ItemEffect effect in itemEffects)
            {
                if (effect == null)
                    continue;

                if (effect.values == null)
                    continue;


                maxLevel =
                    Mathf.Max(
                        maxLevel,
                        effect.values.Length
                    );
            }
        }


        return maxLevel;
    }


    // =========================================================
    // 최대 레벨 여부
    // =========================================================

    public bool IsMaxLevel(int currentLevel)
    {
        // Heal은 레벨업 아이템이 아님
        if (itemType == ItemType.Heal)
            return false;


        int maxLevel =
            GetMaxLevel();


        // 최대 레벨이 0이면 선택 불가능한 데이터
        if (maxLevel <= 0)
            return true;


        return currentLevel >= maxLevel;
    }
}