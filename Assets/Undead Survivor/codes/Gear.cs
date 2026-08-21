using UnityEngine;

public class Gear : MonoBehaviour
{
    // =========================================================
    // Data
    // =========================================================

    public ItemData.ItemType type;

    public float rate;


    // =========================================================
    // Current Level
    // =========================================================

    int level = 1;


    // =========================================================
    // Initialize
    // =========================================================

    public void Init(
        ItemData data,
        int startLevel = 1)
    {
        if (data == null)
        {
            Debug.LogError(
                "[Gear] ItemData가 null입니다."
            );

            return;
        }


        if (GameManager.instance == null ||
            GameManager.instance.player == null)
        {
            Debug.LogError(
                "[Gear] Player를 찾을 수 없습니다."
            );

            return;
        }


        name =
            "Gear_" +
            data.itemId;


        transform.SetParent(
            GameManager.instance.player.transform
        );


        transform.localPosition =
            Vector3.zero;


        transform.localRotation =
            Quaternion.identity;


        type =
            data.itemType;


        level =
            Mathf.Max(
                1,
                startLevel
            );


        UpdateRate(
            data,
            level
        );


        ApplyGear();
    }


    // =========================================================
    // Level Up
    // =========================================================

    public void LevelUp(
        ItemData data,
        int newLevel)
    {
        if (data == null)
            return;


        level =
            Mathf.Max(
                1,
                newLevel
            );


        UpdateRate(
            data,
            level
        );


        ApplyGear();
    }


    // =========================================================
    // Rate
    // =========================================================

    void UpdateRate(
        ItemData data,
        int currentLevel)
    {
        rate = 0f;


        // -----------------------------------------------------
        // 우선 Effect를 사용
        // -----------------------------------------------------

        float effectRate = 0f;


        switch (data.itemType)
        {
            case ItemData.ItemType.Glove:

                effectRate =
                    data.GetEffectValue(
                        ItemData.EffectType.AttackSpeed,
                        currentLevel
                    );

                break;


            case ItemData.ItemType.Shoe:

                effectRate =
                    data.GetEffectValue(
                        ItemData.EffectType.MoveSpeed,
                        currentLevel
                    );

                break;
        }


        if (effectRate != 0f)
        {
            rate =
                effectRate;

            return;
        }


        // -----------------------------------------------------
        // Effect가 없을 경우 damages 사용
        // -----------------------------------------------------

        rate =
            data.GetDamage(
                currentLevel
            );
    }


    // =========================================================
    // Apply
    // =========================================================

    public void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:

                RateUp();

                break;


            case ItemData.ItemType.Shoe:

                SpeedUp();

                break;
        }
    }


    // =========================================================
    // Glove
    // =========================================================

    void RateUp()
    {
        if (transform.parent == null)
            return;


        Weapon[] weapons =
            transform.parent
                .GetComponentsInChildren<Weapon>(
                    true
                );


        foreach (Weapon weapon in weapons)
        {
            if (weapon == null)
                continue;


            switch (weapon.id)
            {
                case 0:
                case 5:

                    float rotateSpeed =
                        150f *
                        Character.WeaponSpeed;


                    weapon.speed =
                        rotateSpeed +
                        rotateSpeed * rate;

                    break;


                default:

                    float fireRate =
                        0.5f *
                        Character.WeaponRate;


                    float finalRate =
                        1f -
                        rate;


                    if (finalRate < 0.05f)
                        finalRate = 0.05f;


                    weapon.speed =
                        fireRate *
                        finalRate;

                    break;
            }
        }
    }


    // =========================================================
    // Shoe
    // =========================================================

    void SpeedUp()
    {
        if (GameManager.instance == null ||
            GameManager.instance.player == null)
        {
            return;
        }


        float speed =
            3f *
            Character.Speed;


        GameManager.instance.player.speed =
            speed +
            speed * rate;
    }
}