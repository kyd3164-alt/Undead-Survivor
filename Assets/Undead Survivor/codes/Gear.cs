using UnityEngine;

public class Gear : MonoBehaviour
{
    public ItemData.ItemType type;
    public float rate;

    public void Init(ItemData data)
    {
        //Basic Set
        name = "Gear " + data.itemId;
        transform.parent = GameManager.instance.player.transform;
        transform.localPosition = Vector3.zero;

        // Property Set
        type = data.itemType;
        rate = data.damages[0];
        ApplyGear();
    }

    public void LevelUp(float rate)
    {
        this.rate = rate;
        ApplyGear();
    }

    void ApplyGear()
    {
        switch (type)
        {
            case ItemData.ItemType.Glove:
                RateUp();
                break;
            case ItemData.ItemType.Shoe:
                SpeedUp();
                break;
            // ➕ [추가] Health 타입일 때 HpUp 실행!
            case ItemData.ItemType.Health:
                HpUp();
                break;
        }
    }

    void RateUp()
    {
        Weapon[] weapons = transform.parent.GetComponentsInChildren<Weapon>();

        foreach (Weapon weapon in weapons)
        {
            switch (weapon.id)
            {
                case 0:
                case 5:
                    float speed = 150 * Character.WeaponSpeed;
                    weapon.speed = speed + (speed * rate);
                    break;
                default:
                    speed = 0.5f * Character.WeaponRate;
                    weapon.speed = speed * (1f - rate);
                    break;
            }
        }
    }

    void SpeedUp()
    {
        float speed = 3 * Character.Speed;
        GameManager.instance.player.speed = speed + speed * rate;
    }

    // ➕ [추가] 체력을 늘려주는 함수
    void HpUp()
    {
        // Player에 붙어있는 PlayerHealth 참조 가져오기
        PlayerHealth playerHealth = GameManager.instance.player.GetComponent<PlayerHealth>();

        if (playerHealth != null)
        {
            // rate에 적힌 값(예: 20f)만큼 최대 체력 증가!
            // 고정 수치가 아니라 비율(%)로 늘리고 싶다면 rate * 100 등으로 응용 가능합니다.
            playerHealth.IncreaseMaxHp(rate);
        }
    }
}