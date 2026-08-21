using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    // =========================================================
    // Static Item Effects
    // =========================================================

    public static float AllDamageBonusRate { get; private set; }

    public static float BloodHitRate { get; private set; }

    public static float HopeOfHopeRate { get; private set; }


    // =========================================================
    // Item Data
    // =========================================================

    [Header("# Item Data")]

    public ItemData data;


    [Header("# Current Level")]

    [Range(1, 5)]
    public int level = 1;


    // =========================================================
    // Runtime
    // =========================================================

    Weapon weapon;

    Gear gear;

    Button button;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        button =
            GetComponent<Button>();
    }


    // =========================================================
    // OnEnable
    // =========================================================

    void OnEnable()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }
    }


    // =========================================================
    // Click
    // =========================================================

    public void OnClick()
    {
        if (data == null)
        {
            Debug.LogError(
                "[Item] ItemData가 연결되지 않았습니다."
            );

            return;
        }


        // -----------------------------------------------------
        // Heal
        // -----------------------------------------------------

        if (data.itemType == ItemData.ItemType.Heal)
        {
            HealPlayer();

            return;
        }


        // -----------------------------------------------------
        // 기존 아이템인지 확인
        // -----------------------------------------------------

        bool isExistingItem =
            FindExistingItem();


        // -----------------------------------------------------
        // 신규 아이템
        // -----------------------------------------------------

        if (!isExistingItem)
        {
            level = 1;

            CreateItem();

            ApplyCurrentEffects();

            UpdateUI();

            UpdateButtonState();

            return;
        }


        // -----------------------------------------------------
        // 이미 존재하는 아이템 → Level Up
        // -----------------------------------------------------

        if (data.IsMaxLevel(level))
        {
            Debug.Log(
                "[Item] 이미 최대 레벨입니다: " +
                data.itemName
            );

            return;
        }


        level++;

        ApplyLevelUp();

        ApplyCurrentEffects();

        UpdateUI();

        UpdateButtonState();
    }


    // =========================================================
    // 기존 Item 찾기
    // =========================================================

    bool FindExistingItem()
    {
        if (GameManager.instance == null)
            return false;


        if (GameManager.instance.player == null)
            return false;


        Weapon[] weapons =
            GameManager.instance.player
                .GetComponentsInChildren<Weapon>(true);


        foreach (Weapon currentWeapon in weapons)
        {
            if (currentWeapon == null)
                continue;


            if (currentWeapon.id == data.itemId)
            {
                weapon =
                    currentWeapon;

                return true;
            }
        }


        Gear[] gears =
            GameManager.instance.player
                .GetComponentsInChildren<Gear>(true);


        foreach (Gear currentGear in gears)
        {
            if (currentGear == null)
                continue;


            GearDataMatch(
                currentGear
            );


            if (currentGear.type == data.itemType)
            {
                gear =
                    currentGear;

                return true;
            }
        }


        return false;
    }


    // =========================================================
    // Gear 비교
    // =========================================================

    void GearDataMatch(Gear currentGear)
    {
        if (currentGear == null)
            return;
    }


    // =========================================================
    // 아이템 생성
    // =========================================================

    void CreateItem()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError(
                "[Item] GameManager.instance가 없습니다."
            );

            return;
        }


        if (GameManager.instance.pool == null)
        {
            Debug.LogError(
                "[Item] PoolManager가 없습니다."
            );

            return;
        }


        if (data.itemType == ItemData.ItemType.Glove ||
            data.itemType == ItemData.ItemType.Shoe ||
            data.itemType == ItemData.ItemType.Health ||
            data.itemType == ItemData.ItemType.BloodHit ||
            data.itemType == ItemData.ItemType.HopeOfHope)
        {
            CreateGear();

            return;
        }


        CreateWeapon();
    }


    // =========================================================
    // Weapon 생성
    // =========================================================

    void CreateWeapon()
    {
        if (data.projectile == null)
        {
            Debug.LogError(
                "[Item] Projectile이 없습니다: " +
                data.itemName
            );

            return;
        }


        GameObject weaponObject =
            new GameObject(
                "Weapon_" + data.itemId
            );


        weapon =
            weaponObject.AddComponent<Weapon>();


        weapon.Init(data);
    }


    // =========================================================
    // Gear 생성
    // =========================================================

    void CreateGear()
    {
        GameObject gearObject =
            new GameObject(
                "Gear_" + data.itemId
            );


        gear =
            gearObject.AddComponent<Gear>();


        gear.Init(
            data,
            level
        );
    }


    // =========================================================
    // Level Up
    // =========================================================

    void ApplyLevelUp()
    {
        if (weapon != null)
        {
            weapon.ApplyLevelData(
                data,
                level
            );

            return;
        }


        if (gear != null)
        {
            gear.LevelUp(
                data,
                level
            );
        }
    }


    // =========================================================
    // 현재 효과 적용
    // =========================================================

    void ApplyCurrentEffects()
    {
        if (data == null)
            return;


        ApplyAllDamageEffect();

        ApplyBloodHitEffect();

        ApplyHopeOfHopeEffect();

        ApplyMaxHealthEffect();

        ApplyWeaponDamageEffect();

        ApplyProjectileCountEffect();

        ApplyAttackSpeedEffect();

        ApplyMoveSpeedEffect();
    }


    // =========================================================
    // All Damage
    // =========================================================

    void ApplyAllDamageEffect()
    {
        AllDamageBonusRate =
            GetEffectValue(
                ItemData.EffectType.AllDamage
            );
    }


    // =========================================================
    // Blood Hit
    // =========================================================

    void ApplyBloodHitEffect()
    {
        BloodHitRate =
            GetEffectValue(
                ItemData.EffectType.BloodHit
            );
    }


    // =========================================================
    // Hope Of Hope
    // =========================================================

    void ApplyHopeOfHopeEffect()
    {
        HopeOfHopeRate =
            GetEffectValue(
                ItemData.EffectType.HopeOfHope
            );
    }


    // =========================================================
    // Max Health
    // =========================================================

    void ApplyMaxHealthEffect()
    {
        float value =
            GetEffectValue(
                ItemData.EffectType.MaxHealth
            );


        if (value == 0f)
            return;


        if (GameManager.instance == null)
            return;


        float baseHealth =
            GameManager.instance.maxHealth;


        GameManager.instance.maxHealth =
            baseHealth *
            (1f + value);


        if (GameManager.instance.health >
            GameManager.instance.maxHealth)
        {
            GameManager.instance.health =
                GameManager.instance.maxHealth;
        }
    }


    // =========================================================
    // Weapon Damage
    // =========================================================

    void ApplyWeaponDamageEffect()
    {
        if (GameManager.instance == null ||
            GameManager.instance.player == null)
            return;


        float value =
            GetEffectValue(
                ItemData.EffectType.WeaponDamage
            );


        if (value == 0f)
            return;


        Weapon[] weapons =
            GameManager.instance.player
                .GetComponentsInChildren<Weapon>(true);


        foreach (Weapon currentWeapon in weapons)
        {
            if (currentWeapon == null)
                continue;


            currentWeapon.RecalculateDamage();
        }
    }


    // =========================================================
    // Projectile Count
    // =========================================================

    void ApplyProjectileCountEffect()
    {
        if (weapon == null)
            return;


        weapon.RebuildCount(
            data,
            level
        );
    }


    // =========================================================
    // Attack Speed
    // =========================================================

    void ApplyAttackSpeedEffect()
    {
        if (GameManager.instance == null ||
            GameManager.instance.player == null)
            return;


        float value =
            GetEffectValue(
                ItemData.EffectType.AttackSpeed
            );


        if (value == 0f)
            return;


        GameManager.instance.player
            .BroadcastMessage(
                "ApplyGear",
                SendMessageOptions.DontRequireReceiver
            );
    }


    // =========================================================
    // Move Speed
    // =========================================================

    void ApplyMoveSpeedEffect()
    {
        if (GameManager.instance == null ||
            GameManager.instance.player == null)
            return;


        float value =
            GetEffectValue(
                ItemData.EffectType.MoveSpeed
            );


        if (value == 0f)
            return;


        GameManager.instance.player
            .BroadcastMessage(
                "ApplyGear",
                SendMessageOptions.DontRequireReceiver
            );
    }


    // =========================================================
    // Effect 값
    // =========================================================

    float GetEffectValue(
        ItemData.EffectType effectType)
    {
        if (data == null)
            return 0f;


        return data.GetEffectValue(
            effectType,
            level
        );
    }


    // =========================================================
    // Heal
    // =========================================================

    void HealPlayer()
    {
        if (GameManager.instance == null)
            return;


        GameManager.instance.health =
            GameManager.instance.maxHealth;


        Debug.Log(
            "[Item] 체력을 모두 회복했습니다."
        );
    }


    // =========================================================
    // UI
    // =========================================================

    public void UpdateUI()
    {
        if (data == null)
            return;


        // UI Text를 사용하는 기존 프리팹과 충돌하지 않도록
        // 여기서는 필수 데이터 갱신만 수행합니다.
    }


    // =========================================================
    // Button State
    // =========================================================

    public void UpdateButtonState()
    {
        if (button == null)
        {
            button =
                GetComponent<Button>();
        }


        if (button == null)
            return;


        if (data == null)
        {
            button.interactable = false;
            return;
        }


        // Heal은 항상 선택 가능
        if (data.itemType == ItemData.ItemType.Heal)
        {
            button.interactable = true;
            return;
        }


        button.interactable =
            !data.IsMaxLevel(level);
    }


    // =========================================================
    // Static Effect Reset
    // =========================================================

    public static void ResetSpecialItemEffects()
    {
        AllDamageBonusRate = 0f;
        BloodHitRate = 0f;
        HopeOfHopeRate = 0f;
    }
}