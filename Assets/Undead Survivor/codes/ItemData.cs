using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Object/ItemData")]
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
    // Effect Type
    // =========================================================

    public enum EffectType
    {
        None,

        WeaponDamage,
        ProjectileCount,

        AllDamage,

        AttackSpeed,
        MoveSpeed,

        MaxHealth,

        BloodHit,
        HopeOfHope
    }


    // =========================================================
    // Item Effect
    // =========================================================

    [System.Serializable]
    public class ItemEffect
    {
        [Tooltip("실제로 적용되는 효과 종류")]
        public EffectType effectType = EffectType.None;

        [Tooltip("레벨별 효과 수치\nElement 0 = Lv1\nElement 1 = Lv2\n...\nElement 4 = Lv5")]
        public float[] values = new float[5];
    }


    // =========================================================
    // Main Info
    // =========================================================

    [Header("# Main Info")]

    [Tooltip("아이템 자체의 종류")]
    public ItemType itemType;

    [Tooltip("아이템 ID")]
    public int itemId;

    [Tooltip("아이템 이름")]
    public string itemName;

    [TextArea]
    [Tooltip("아이템 설명")]
    public string itemDesc;

    [Tooltip("아이템 아이콘")]
    public Sprite itemIcon;


    // =========================================================
    // Level Data
    // =========================================================

    [Header("# Level Data")]

    [Tooltip("레벨별 데미지\nElement 0 = Lv1 ~ Element 4 = Lv5")]
    public float[] damages = new float[5];

    [Tooltip("레벨별 투사체 개수\nElement 0 = Lv1 ~ Element 4 = Lv5")]
    public int[] counts = new int[5];


    // =========================================================
    // Item Effects
    // =========================================================

    [Header("# Item Effects")]

    [Tooltip("하나의 아이템에 여러 효과를 넣을 수 있습니다.")]
    public ItemEffect[] itemEffects;


    // =========================================================
    // Weapon
    // =========================================================

    [Header("# Weapon")]

    [Tooltip("Weapon이 사용할 Projectile")]
    public GameObject projectile;

    [Tooltip("플레이어 손에 표시할 Sprite")]
    public Sprite hand;


    // =========================================================
    // 최대 레벨
    // =========================================================

    public int GetMaxLevel()
    {
        int maxLevel = 0;


        // -----------------------------------------------------
        // Damage
        // -----------------------------------------------------

        if (damages != null)
        {
            maxLevel = Mathf.Max(
                maxLevel,
                damages.Length
            );
        }


        // -----------------------------------------------------
        // Count
        // -----------------------------------------------------

        if (counts != null)
        {
            maxLevel = Mathf.Max(
                maxLevel,
                counts.Length
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

                maxLevel = Mathf.Max(
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
        // Heal은 기존 시스템대로 계속 선택 가능
        if (itemType == ItemType.Heal)
            return false;


        int maxLevel = GetMaxLevel();


        if (maxLevel <= 0)
            return true;


        return currentLevel >= maxLevel;
    }


    // =========================================================
    // Damage 가져오기
    // =========================================================

    public float GetDamage(int level)
    {
        if (damages == null ||
            damages.Length == 0)
        {
            return 0f;
        }


        int index = Mathf.Clamp(
            level - 1,
            0,
            damages.Length - 1
        );


        return damages[index];
    }


    // =========================================================
    // Count 가져오기
    // =========================================================

    public int GetCount(int level)
    {
        if (counts == null ||
            counts.Length == 0)
        {
            return 0;
        }


        int index = Mathf.Clamp(
            level - 1,
            0,
            counts.Length - 1
        );


        return counts[index];
    }


    // =========================================================
    // Effect 찾기
    // =========================================================

    public ItemEffect GetEffect(EffectType type)
    {
        if (itemEffects == null)
            return null;


        foreach (ItemEffect effect in itemEffects)
        {
            if (effect == null)
                continue;


            if (effect.effectType == type)
                return effect;
        }


        return null;
    }


    // =========================================================
    // Effect 값 가져오기
    // =========================================================

    public float GetEffectValue(
        EffectType type,
        int level)
    {
        ItemEffect effect =
            GetEffect(type);


        if (effect == null)
            return 0f;


        if (effect.values == null ||
            effect.values.Length == 0)
        {
            return 0f;
        }


        int index = Mathf.Clamp(
            level - 1,
            0,
            effect.values.Length - 1
        );


        return effect.values[index];
    }


    // =========================================================
    // Effect 존재 여부
    // =========================================================

    public bool HasItemEffects()
    {
        return
            itemEffects != null &&
            itemEffects.Length > 0;
    }
}