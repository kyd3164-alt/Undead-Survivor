using UnityEngine;

public class Weapon : MonoBehaviour
{
    // =========================================================
    // Weapon Data
    // =========================================================

    public int id;

    public int prefabId = -1;

    public float damage;

    public int count;

    public float speed;


    // =========================================================
    // Raw Damage
    // =========================================================

    public float rawDamage;


    // =========================================================
    // Internal
    // =========================================================

    float timer;

    Player player;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        if (GameManager.instance != null)
        {
            player =
                GameManager.instance.player;
        }
    }


    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        if (GameManager.instance == null)
            return;


        if (!GameManager.instance.isLive)
            return;


        if (player == null)
        {
            player =
                GameManager.instance.player;


            if (player == null)
                return;
        }


        switch (id)
        {
            case 0:
            case 5:

                transform.Rotate(
                    Vector3.back *
                    speed *
                    Time.deltaTime
                );

                break;


            default:

                timer +=
                    Time.deltaTime;


                if (timer >= speed)
                {
                    timer = 0f;

                    Fire();
                }

                break;
        }
    }


    // =========================================================
    // Initialize
    // =========================================================

    public void Init(ItemData data)
    {
        if (data == null)
        {
            Debug.LogError(
                "[Weapon] ItemData가 null입니다."
            );

            return;
        }


        if (GameManager.instance == null)
        {
            Debug.LogError(
                "[Weapon] GameManager.instance가 없습니다."
            );

            return;
        }


        if (GameManager.instance.pool == null)
        {
            Debug.LogError(
                "[Weapon] Pool이 없습니다."
            );

            return;
        }


        if (GameManager.instance.pool.prefabs == null)
        {
            Debug.LogError(
                "[Weapon] Pool.prefabs가 없습니다."
            );

            return;
        }


        player =
            GameManager.instance.player;


        if (player == null)
        {
            Debug.LogError(
                "[Weapon] Player를 찾을 수 없습니다."
            );

            return;
        }


        // -----------------------------------------------------
        // Basic
        // -----------------------------------------------------

        name =
            "Weapon_" +
            data.itemId;


        transform.SetParent(
            player.transform
        );


        transform.localPosition =
            Vector3.zero;


        transform.localRotation =
            Quaternion.identity;


        // -----------------------------------------------------
        // ID
        // -----------------------------------------------------

        id =
            data.itemId;


        // -----------------------------------------------------
        // Lv1 Data
        // -----------------------------------------------------

        ApplyLevelData(
            data,
            1
        );


        // -----------------------------------------------------
        // Projectile
        // -----------------------------------------------------

        FindPrefabId(
            data
        );


        // -----------------------------------------------------
        // Weapon Speed
        // -----------------------------------------------------

        SetWeaponSpeed();


        // -----------------------------------------------------
        // Hand
        // -----------------------------------------------------

        ApplyHand(
            data
        );


        // -----------------------------------------------------
        // Gear
        // -----------------------------------------------------

        player.BroadcastMessage(
            "ApplyGear",
            SendMessageOptions.DontRequireReceiver
        );
    }


    // =========================================================
    // Level Data
    // =========================================================

    public void ApplyLevelData(
        ItemData data,
        int level)
    {
        if (data == null)
            return;


        int safeLevel =
            Mathf.Max(
                1,
                level
            );


        rawDamage =
            data.GetDamage(
                safeLevel
            );


        count =
            data.GetCount(
                safeLevel
            ) +
            Character.Count;


        if (count < 0)
            count = 0;


        RecalculateDamage();


        SetWeaponSpeed();


        if (id == 0 ||
            id == 5)
        {
            Batch();
        }
    }


    // =========================================================
    // Count 재계산
    // =========================================================

    public void RebuildCount(
        ItemData data,
        int level)
    {
        if (data == null)
            return;


        count =
            data.GetCount(level) +
            Character.Count;


        if (count < 0)
            count = 0;


        if (id == 0 ||
            id == 5)
        {
            Batch();
        }
    }


    // =========================================================
    // Damage 재계산
    // =========================================================

    public void RecalculateDamage()
    {
        float characterDamage =
            Character.Damage;


        if (characterDamage <= 0f)
            characterDamage = 1f;


        damage =
            rawDamage *
            characterDamage *
            (1f +
             Item.AllDamageBonusRate);


        if (damage < 0f)
            damage = 0f;
    }


    // =========================================================
    // Weapon Speed
    // =========================================================

    void SetWeaponSpeed()
    {
        switch (id)
        {
            case 0:
            case 5:

                speed =
                    150f *
                    Character.WeaponSpeed;

                break;


            default:

                speed =
                    0.5f *
                    Character.WeaponRate;

                break;
        }
    }


    // =========================================================
    // Projectile Prefab ID
    // =========================================================

    void FindPrefabId(
        ItemData data)
    {
        prefabId = -1;


        if (data.projectile == null)
        {
            Debug.LogError(
                "[Weapon] Projectile이 없습니다.\n" +
                "Item : " +
                data.itemName
            );

            return;
        }


        for (
            int index = 0;
            index <
            GameManager.instance.pool.prefabs.Length;
            index++)
        {
            GameObject prefab =
                GameManager.instance.pool.prefabs[index];


            if (prefab == null)
                continue;


            if (prefab == data.projectile)
            {
                prefabId =
                    index;

                break;
            }
        }


        if (prefabId < 0)
        {
            Debug.LogError(
                "[Weapon] Projectile을 Pool에서 찾지 못했습니다.\n" +
                "Item : " +
                data.itemName
            );
        }
    }


    // =========================================================
    // Hand
    // =========================================================

    void ApplyHand(
        ItemData data)
    {
        if (data.hand == null)
            return;


        int handIndex =
            (int)data.itemType;


        if (player.hands == null)
            return;


        if (handIndex < 0 ||
            handIndex >= player.hands.Length)
            return;


        Hand hand =
            player.hands[handIndex];


        if (hand == null)
            return;


        if (hand.spriter != null)
        {
            hand.spriter.sprite =
                data.hand;
        }


        hand.gameObject.SetActive(
            true
        );
    }


    // =========================================================
    // prefabId 검사
    // =========================================================

    bool IsValidPrefabId()
    {
        if (GameManager.instance == null)
            return false;


        if (GameManager.instance.pool == null)
            return false;


        if (GameManager.instance.pool.prefabs == null)
            return false;


        if (prefabId < 0)
            return false;


        if (prefabId >=
            GameManager.instance.pool.prefabs.Length)
        {
            return false;
        }


        if (
            GameManager.instance.pool.prefabs[prefabId]
            == null)
        {
            return false;
        }


        return true;
    }


    // =========================================================
    // Batch
    // =========================================================

    void Batch()
    {
        if (!IsValidPrefabId())
        {
            Debug.LogError(
                "[Weapon] Batch 실패 - prefabId 오류"
            );

            return;
        }


        if (count <= 0)
            return;


        // 기존 자식 수보다 줄어든 경우
        // 초과된 탄환을 비활성화
        for (
            int index = count;
            index < transform.childCount;
            index++)
        {
            Transform extraBullet =
                transform.GetChild(index);


            if (extraBullet != null)
            {
                extraBullet.gameObject.SetActive(
                    false
                );
            }
        }


        for (
            int index = 0;
            index < count;
            index++)
        {
            Transform bullet;


            if (index <
                transform.childCount)
            {
                bullet =
                    transform.GetChild(index);


                bullet.gameObject.SetActive(
                    true
                );
            }
            else
            {
                GameObject bulletObject =
                    GameManager.instance.pool.Get(
                        prefabId
                    );


                if (bulletObject == null)
                    return;


                bullet =
                    bulletObject.transform;


                bullet.SetParent(
                    transform
                );
            }


            bullet.localPosition =
                Vector3.zero;


            bullet.localRotation =
                Quaternion.identity;


            Vector3 rotVec =
                Vector3.forward *
                360f *
                index /
                count;


            bullet.Rotate(
                rotVec
            );


            bullet.Translate(
                bullet.up *
                1.5f,
                Space.World
            );


            Bullet bulletComponent =
                bullet.GetComponent<Bullet>();


            if (bulletComponent == null)
                continue;


            bulletComponent.Init(
                damage,
                -100,
                Vector3.zero,
                id
            );
        }
    }


    // =========================================================
    // Fire
    // =========================================================

    void Fire()
    {
        if (player == null)
            return;


        if (GameManager.instance == null)
            return;


        if (GameManager.instance.pool == null)
            return;


        if (player.scanner == null)
            return;


        if (!player.scanner.nearestTarget)
            return;


        if (!IsValidPrefabId())
            return;


        Vector3 targetPos =
            player.scanner.nearestTarget.position;


        Vector3 dir =
            targetPos -
            transform.position;


        if (dir.sqrMagnitude <=
            0.0001f)
        {
            return;
        }


        dir.Normalize();


        GameObject bulletObject =
            GameManager.instance.pool.Get(
                prefabId
            );


        if (bulletObject == null)
            return;


        Transform bullet =
            bulletObject.transform;


        bullet.position =
            transform.position;


        bullet.rotation =
            Quaternion.FromToRotation(
                Vector3.up,
                dir
            );


        Bullet bulletComponent =
            bullet.GetComponent<Bullet>();


        if (bulletComponent == null)
            return;


        bulletComponent.Init(
            damage,
            count,
            dir,
            id
        );


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.Range
            );
        }
    }
}