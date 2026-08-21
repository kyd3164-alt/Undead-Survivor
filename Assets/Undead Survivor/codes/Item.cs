using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public ItemData data;
    public int level;

    public Weapon weapon;
    public Gear gear;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;


    // =========================================================
    // 모든 피해 증가
    // =========================================================

    public static float AllDamageBonusRate { get; private set; } = 0f;


    // =========================================================
    // 블러드 히트
    // =========================================================

    public static float BloodHitRate { get; private set; } = 0f;


    // =========================================================
    // 희망의 호프
    // =========================================================

    public static float HopeOfHopeRate { get; private set; } = 0f;


    // =========================================================
    // 초기화
    // =========================================================

    void Awake()
    {
        Image[] images =
            GetComponentsInChildren<Image>();


        if (images.Length > 1)
        {
            icon = images[1];

            if (data != null)
            {
                icon.sprite =
                    data.itemIcon;
            }
        }


        Text[] texts =
            GetComponentsInChildren<Text>();


        if (texts.Length >= 3)
        {
            textLevel = texts[0];
            textName = texts[1];
            textDesc = texts[2];
        }


        if (textName != null && data != null)
        {
            textName.text =
                data.itemName;
        }
    }


    // =========================================================
    // UI 활성화
    // =========================================================

    void OnEnable()
    {
        UpdateUI();
        UpdateButtonState();
    }


    // =========================================================
    // 최대 레벨 여부
    // =========================================================

    public bool IsMaxLevel()
    {
        if (data == null)
            return true;


        return data.IsMaxLevel(level);
    }


    // =========================================================
    // 최대 레벨 반환
    // =========================================================

    public int GetMaxLevel()
    {
        if (data == null)
            return 0;


        return data.GetMaxLevel();
    }


    // =========================================================
    // UI 갱신
    // =========================================================

    public void UpdateUI()
    {
        if (data == null)
            return;


        // -----------------------------------------------------
        // 최대 레벨
        // -----------------------------------------------------

        int maxLevel =
            data.GetMaxLevel();


        // -----------------------------------------------------
        // 레벨
        // -----------------------------------------------------

        if (textLevel != null)
        {
            if (data.itemType ==
                ItemData.ItemType.Heal)
            {
                textLevel.text = "";
            }
            else
            {
                int displayLevel;


                if (maxLevel > 0)
                {
                    displayLevel =
                        Mathf.Clamp(
                            level + 1,
                            1,
                            maxLevel
                        );
                }
                else
                {
                    displayLevel = 1;
                }


                textLevel.text =
                    "Lv." +
                    displayLevel;
            }
        }


        // -----------------------------------------------------
        // 이름
        // -----------------------------------------------------

        if (textName != null)
        {
            textName.text =
                data.itemName;
        }


        // -----------------------------------------------------
        // 설명
        // -----------------------------------------------------

        if (textDesc == null)
            return;


        // -----------------------------------------------------
        // Heal
        // -----------------------------------------------------

        if (data.itemType ==
            ItemData.ItemType.Heal)
        {
            textDesc.text =
                data.itemDesc;

            return;
        }


        // -----------------------------------------------------
        // Damages 검사
        // -----------------------------------------------------

        if (
            data.damages == null ||
            data.damages.Length == 0
        )
        {
            textDesc.text =
                data.itemDesc;

            return;
        }


        // -----------------------------------------------------
        // 안전한 배열 인덱스
        // -----------------------------------------------------

        int displayIndex =
            Mathf.Clamp(
                level,
                0,
                data.damages.Length - 1
            );


        // -----------------------------------------------------
        // Counts
        // -----------------------------------------------------

        int countValue = 0;


        if (
            data.counts != null &&
            data.counts.Length > 0
        )
        {
            int countIndex =
                Mathf.Clamp(
                    displayIndex,
                    0,
                    data.counts.Length - 1
                );


            countValue =
                data.counts[countIndex];
        }


        // -----------------------------------------------------
        // Item Type
        // -----------------------------------------------------

        switch (data.itemType)
        {
            // =================================================
            // 근접 / 원거리
            // =================================================

            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:

                textDesc.text =
                    string.Format(
                        data.itemDesc,
                        data.damages[displayIndex] * 100,
                        countValue
                    );

                break;


            // =================================================
            // 모든 피해 증가
            // =================================================

            case ItemData.ItemType.GunDamage:

                textDesc.text =
                    string.Format(
                        data.itemDesc,
                        data.damages[displayIndex]
                    );

                break;


            // =================================================
            // 장갑 / 신발
            // =================================================

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:

                textDesc.text =
                    string.Format(
                        data.itemDesc,
                        data.damages[displayIndex] * 100
                    );

                break;


            // =================================================
            // 최대 체력
            // =================================================

            case ItemData.ItemType.Health:

                textDesc.text =
                    string.Format(
                        data.itemDesc,
                        data.damages[displayIndex]
                    );

                break;


            // =================================================
            // 블러드 히트
            // =================================================

            case ItemData.ItemType.BloodHit:

                textDesc.text =
                    string.Format(
                        data.itemDesc,
                        data.damages[displayIndex] * 100
                    );

                break;


            // =================================================
            // 희망의 호프
            // =================================================

            case ItemData.ItemType.HopeOfHope:

                textDesc.text =
                    string.Format(
                        data.itemDesc,
                        data.damages[displayIndex] * 100
                    );

                break;


            // =================================================
            // 기타
            // =================================================

            default:

                textDesc.text =
                    data.itemDesc;

                break;
        }
    }


    // =========================================================
    // 버튼 상태
    // =========================================================

    public void UpdateButtonState()
    {
        Button button =
            GetComponent<Button>();


        if (button == null)
            return;


        if (data == null)
        {
            button.interactable = false;
            return;
        }


        // -----------------------------------------------------
        // Heal
        // -----------------------------------------------------

        if (data.itemType ==
            ItemData.ItemType.Heal)
        {
            button.interactable = true;
            return;
        }


        // -----------------------------------------------------
        // 최대 레벨
        // -----------------------------------------------------

        button.interactable =
            !IsMaxLevel();
    }


    // =========================================================
    // 아이템 선택
    // =========================================================

    public void OnClick()
    {
        if (data == null)
            return;


        // -----------------------------------------------------
        // 최대 레벨 검사
        // -----------------------------------------------------

        if (IsMaxLevel())
        {
            Debug.Log(
                "⚠️ " +
                data.itemName +
                "은 최대 레벨입니다. " +
                "현재 Lv." +
                level +
                " / 최대 Lv." +
                data.GetMaxLevel()
            );

            return;
        }


        Debug.Log(
            "🟢 클릭한 아이템: " +
            data.itemName +
            " / 타입: " +
            data.itemType
        );


        // =====================================================
        // Item Type
        // =====================================================

        switch (data.itemType)
        {
            // =================================================
            // 근접 / 원거리
            // =================================================

            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:

                if (level == 0)
                {
                    GameObject newWeapon =
                        new GameObject(
                            data.itemName
                        );


                    weapon =
                        newWeapon.AddComponent<Weapon>();


                    weapon.Init(data);
                }
                else
                {
                    float nextDamage =
                        data.baseDamage;


                    int nextCount = 0;


                    if (
                        data.damages != null &&
                        level < data.damages.Length
                    )
                    {
                        nextDamage +=
                            data.baseDamage *
                            data.damages[level];
                    }


                    if (
                        data.counts != null &&
                        level < data.counts.Length
                    )
                    {
                        nextCount +=
                            data.counts[level];
                    }


                    if (weapon != null)
                    {
                        weapon.LevelUp(
                            nextDamage,
                            nextCount
                        );
                    }
                }


                level++;

                break;


            // =================================================
            // 모든 피해 증가
            // =================================================

            case ItemData.ItemType.GunDamage:

                if (
                    data.damages == null ||
                    level >= data.damages.Length
                )
                {
                    break;
                }


                AllDamageBonusRate =
                    data.damages[level] / 100f;


                Debug.Log(
                    "🔫 [모든 피해 증가] " +
                    "Lv." +
                    (level + 1) +
                    " | 모든 피해 +" +
                    data.damages[level] +
                    "%"
                );


                level++;

                break;


            // =================================================
            // 장갑 / 신발
            // =================================================

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:

                if (level == 0)
                {
                    GameObject newGear =
                        new GameObject(
                            data.itemName
                        );


                    gear =
                        newGear.AddComponent<Gear>();


                    gear.Init(data);
                }
                else
                {
                    if (
                        gear != null &&
                        data.damages != null &&
                        level < data.damages.Length
                    )
                    {
                        float nextRate =
                            data.damages[level];


                        gear.LevelUp(
                            nextRate
                        );
                    }
                }


                level++;

                break;


            // =================================================
            // 즉시 회복
            // =================================================

            case ItemData.ItemType.Heal:

                PlayerHealth playerHealth =
                    FindFirstObjectByType<PlayerHealth>();


                if (playerHealth != null)
                {
                    playerHealth.Heal(
                        GameManager.instance.maxHealth
                    );


                    Debug.Log(
                        "🥤 음료수 사용 → " +
                        "생명력 전체 회복!"
                    );
                }

                break;


            // =================================================
            // 최대 체력
            // =================================================

            case ItemData.ItemType.Health:

                if (
                    data.damages == null ||
                    level >= data.damages.Length
                )
                {
                    break;
                }


                PlayerHealth health =
                    FindFirstObjectByType<PlayerHealth>();


                if (health != null)
                {
                    float healthPercent =
                        data.damages[level];


                    Debug.Log(
                        "❤️ 최대 체력 증가: " +
                        healthPercent +
                        "%"
                    );


                    health.IncreaseMaxHp(
                        healthPercent
                    );
                }
                else
                {
                    Debug.LogError(
                        "❌ PlayerHealth를 찾지 못했습니다!"
                    );
                }


                level++;

                break;


            // =================================================
            // 블러드 히트
            // =================================================

            case ItemData.ItemType.BloodHit:

                if (
                    data.damages == null ||
                    level >= data.damages.Length
                )
                {
                    break;
                }


                BloodHitRate =
                    data.damages[level];


                Debug.Log(
                    "🩸 블러드 히트 Lv." +
                    (level + 1)
                );


                Debug.Log(
                    "🩸 모든 피해 흡혈률: " +
                    (BloodHitRate * 100) +
                    "%"
                );


                level++;

                break;


            // =================================================
            // 희망의 호프
            // =================================================

            case ItemData.ItemType.HopeOfHope:

                if (
                    data.damages == null ||
                    level >= data.damages.Length
                )
                {
                    break;
                }


                HopeOfHopeRate =
                    data.damages[level];


                Debug.Log(
                    "🌟 희망의 호프 Lv." +
                    (level + 1)
                );


                Debug.Log(
                    "🌟 대상 최대 체력 추가 피해: " +
                    (HopeOfHopeRate * 100) +
                    "%"
                );


                level++;

                break;
        }


        // =====================================================
        // 선택 후 UI 갱신
        // =====================================================

        UpdateUI();
        UpdateButtonState();
    }


    // =========================================================
    // 특수 아이템 효과 초기화
    // =========================================================

    public static void ResetSpecialItemEffects()
    {
        AllDamageBonusRate = 0f;
        BloodHitRate = 0f;
        HopeOfHopeRate = 0f;


        Debug.Log(
            "🔄 모든 피해 증가 / " +
            "블러드 히트 / 희망의 호프 효과 초기화"
        );
    }
}