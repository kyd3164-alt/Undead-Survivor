using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public static float BloodHitRate;
    public static float HopeOfHopeRate;
    public static float PoisonRate;

    public ItemData data;
    public int level;
    public Weapon weapon;
    public Gear gear;

    Image icon;
    Text textLevel;
    Text textName;
    Text textDesc;

    void Awake()
    {
        icon = GetComponentsInChildren<Image>()[1];
        icon.sprite = data.itemIcon;

        Text[] texts = GetComponentsInChildren<Text>();
        textLevel = texts[0];
        textName = texts[1];
        textDesc = texts[2];
        textName.text = data.itemName;
    }

    void OnEnable()
    {
        if (data.itemType == ItemData.ItemType.Heal)
        {
            textLevel.text = "";
        }
        else
        {
            textLevel.text = "Lv." + (level + 1);
        }

        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100, data.counts[level]);
                break;

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
                break;

            case ItemData.ItemType.Health:
                textDesc.text = string.Format(data.itemDesc, data.damages[level]);
                break;

            case ItemData.ItemType.Heal:
                textDesc.text = data.itemDesc;
                break;

            case ItemData.ItemType.BloodHit:
            case ItemData.ItemType.HopeOfHope:
            case ItemData.ItemType.Poison:
                textDesc.text = string.Format(data.itemDesc, data.damages[level] * 100);
                break;
        }
    }

    public void OnClick()
    {
        Debug.Log($"🟢 클릭한 아이템: {data.itemName} / 타입: {data.itemType}");

        switch (data.itemType)
        {
            case ItemData.ItemType.Melee:
            case ItemData.ItemType.Range:
                if (level == 0)
                {
                    GameObject newWeapon = new GameObject();
                    weapon = newWeapon.AddComponent<Weapon>();
                    weapon.Init(data);
                }
                else
                {
                    float nextDamage = data.baseDamage;
                    int nextCount = 0;

                    nextDamage += data.baseDamage * data.damages[level];
                    nextCount += data.counts[level];

                    weapon.LevelUp(nextDamage, nextCount);
                }

                level++;
                break;

            case ItemData.ItemType.BloodHit:
                BloodHitRate = data.damages[level];
                level++;
                break;

            case ItemData.ItemType.HopeOfHope:
                HopeOfHopeRate = data.damages[level];
                level++;
                break;

            case ItemData.ItemType.Poison:
                PoisonRate = data.damages[level];
                level++;
                break;

            case ItemData.ItemType.Glove:
            case ItemData.ItemType.Shoe:
                if (level == 0)
                {
                    GameObject newGear = new GameObject();
                    gear = newGear.AddComponent<Gear>();
                    gear.Init(data);
                }
                else
                {
                    float nextRate = data.damages[level];
                    gear.LevelUp(nextRate);
                }
                level++;
                break;

            case ItemData.ItemType.Heal:
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

                if (playerHealth != null)
                {
                    playerHealth.Heal(GameManager.instance.maxHealth);
                }

                break;

            case ItemData.ItemType.Health:
                Debug.Log($"❤️ 최대 체력 아이템 선택! Lv.{level + 1}");

                PlayerHealth health = FindFirstObjectByType<PlayerHealth>();

                if (health != null)
                {
                    float healthPercent = data.damages[level];

                    Debug.Log($"❤️ 증가율: {healthPercent}%");

                    health.IncreaseMaxHp(healthPercent);
                }
                else
                {
                    Debug.LogError("❌ PlayerHealth를 찾지 못했습니다!");
                }

                level++;
                break;
        }


        if (data.itemType != ItemData.ItemType.Heal && data.itemType != ItemData.ItemType.Health)
        {
            if (level == data.damages.Length)
            {
                GetComponent<Button>().interactable = false;
            }
        }
    }
}
