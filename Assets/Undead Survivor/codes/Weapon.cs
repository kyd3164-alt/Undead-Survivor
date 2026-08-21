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
            player = GameManager.instance.player;
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
            player = GameManager.instance.player;

            if (player == null)
                return;
        }


        switch (id)
        {
            // =================================================
            // Melee / 회전형 무기
            // =================================================

            case 0:
            case 5:

                transform.Rotate(
                    Vector3.back * speed * Time.deltaTime
                );

                break;


            // =================================================
            // Range / Gun / 발사형 무기
            // =================================================

            default:

                timer += Time.deltaTime;

                if (timer >= speed)
                {
                    timer = 0f;
                    Fire();
                }

                break;
        }


        // =====================================================
        // Test Code
        // =====================================================

        if (Input.GetButtonDown("Jump"))
        {
            LevelUp(10, 1);
        }
    }


    // =========================================================
    // Level Up
    // =========================================================

    public void LevelUp(float damage, int count)
    {
        this.damage = damage * Character.Damage;

        this.count += count;


        // =====================================================
        // 회전형 무기
        // =====================================================

        if (id == 0 || id == 5)
        {
            Batch();
        }


        // =====================================================
        // 장비 효과 적용
        // =====================================================

        if (player != null)
        {
            player.BroadcastMessage(
                "ApplyGear",
                SendMessageOptions.DontRequireReceiver
            );
        }
    }


    // =========================================================
    // Initialize
    // =========================================================

    public void Init(ItemData data)
    {
        // =====================================================
        // 기본 검사
        // =====================================================

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
                "[Weapon] GameManager.instance가 null입니다."
            );

            return;
        }


        if (GameManager.instance.pool == null)
        {
            Debug.LogError(
                "[Weapon] GameManager의 Pool이 null입니다."
            );

            return;
        }


        if (GameManager.instance.pool.prefabs == null)
        {
            Debug.LogError(
                "[Weapon] Pool.prefabs가 null입니다."
            );

            return;
        }


        if (player == null)
        {
            player = GameManager.instance.player;
        }


        if (player == null)
        {
            Debug.LogError(
                "[Weapon] Player를 찾을 수 없습니다."
            );

            return;
        }


        // =====================================================
        // Basic Set
        // =====================================================

        name = "Weapon" + data.itemId;

        transform.parent = player.transform;

        transform.localPosition = Vector3.zero;

        transform.localRotation = Quaternion.identity;


        // =====================================================
        // Property Set
        // =====================================================

        id = data.itemId;

        damage =
            data.baseDamage *
            Character.Damage;

        count =
            data.baseCount +
            Character.Count;


        // =====================================================
        // Projectile ID 초기화
        // =====================================================

        prefabId = -1;


        // =====================================================
        // Projectile 검사
        // =====================================================

        if (data.projectile == null)
        {
            Debug.LogError(
                "[Weapon] " +
                data.itemName +
                "의 Projectile이 지정되지 않았습니다."
            );
        }
        else
        {
            bool found = false;


            // =================================================
            // Pool에서 Projectile 찾기
            // =================================================

            for (
                int index = 0;
                index < GameManager.instance.pool.prefabs.Length;
                index++
            )
            {
                GameObject prefab =
                    GameManager.instance.pool.prefabs[index];


                if (prefab == null)
                    continue;


                if (data.projectile == prefab)
                {
                    prefabId = index;

                    found = true;

                    Debug.Log(
                        "[Weapon] Projectile 연결 성공\n" +
                        "Item : " + data.itemName + "\n" +
                        "Projectile : " + data.projectile.name + "\n" +
                        "Prefab ID : " + prefabId
                    );

                    break;
                }
            }


            // =================================================
            // Projectile을 찾지 못한 경우
            // =================================================

            if (!found)
            {
                Debug.LogError(
                    "[Weapon] Projectile을 Pool에서 찾을 수 없습니다.\n" +
                    "Item : " + data.itemName + "\n" +
                    "Projectile : " + data.projectile.name
                );
            }
        }


        // =====================================================
        // Weapon Type
        // =====================================================

        switch (id)
        {
            // =================================================
            // 회전형 무기
            // =================================================

            case 0:
            case 5:

                speed =
                    150f *
                    Character.WeaponSpeed;

                Batch();

                break;


            // =================================================
            // 발사형 무기
            // =================================================

            default:

                speed =
                    0.5f *
                    Character.WeaponRate;

                break;
        }


        // =====================================================
        // Hand Set
        // =====================================================

        if (data.hand != null)
        {
            int handIndex =
                (int)data.itemType;


            if (
                player.hands != null &&
                handIndex >= 0 &&
                handIndex < player.hands.Length &&
                player.hands[handIndex] != null
            )
            {
                Hand hand =
                    player.hands[handIndex];


                if (hand.spriter != null)
                {
                    hand.spriter.sprite =
                        data.hand;
                }


                hand.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning(
                    "[Weapon] Hand를 찾을 수 없습니다.\n" +
                    "Item : " + data.itemName
                );
            }
        }


        // =====================================================
        // Apply Gear
        // =====================================================

        player.BroadcastMessage(
            "ApplyGear",
            SendMessageOptions.DontRequireReceiver
        );
    }


    // =========================================================
    // Prefab ID 검사
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

        if (
            prefabId >=
            GameManager.instance.pool.prefabs.Length
        )
        {
            return false;
        }

        if (
            GameManager.instance.pool.prefabs[prefabId] ==
            null
        )
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
        if (GameManager.instance == null)
            return;


        if (GameManager.instance.pool == null)
            return;


        // =====================================================
        // Prefab ID 검사
        // =====================================================

        if (!IsValidPrefabId())
        {
            Debug.LogError(
                "[Weapon] Batch 실패 - prefabId가 유효하지 않습니다.\n" +
                "Weapon ID : " + id + "\n" +
                "Prefab ID : " + prefabId
            );

            return;
        }


        // =====================================================
        // Count 검사
        // =====================================================

        if (count <= 0)
        {
            Debug.LogWarning(
                "[Weapon] count가 0 이하입니다.\n" +
                "Weapon ID : " + id
            );

            return;
        }


        // =====================================================
        // Bullet 생성
        // =====================================================

        for (int index = 0; index < count; index++)
        {
            Transform bullet;


            // =================================================
            // 기존 Bullet 재사용
            // =================================================

            if (index < transform.childCount)
            {
                bullet =
                    transform.GetChild(index);
            }


            // =================================================
            // 새로운 Bullet 생성
            // =================================================

            else
            {
                GameObject bulletObject =
                    GameManager.instance.pool.Get(prefabId);


                if (bulletObject == null)
                {
                    Debug.LogError(
                        "[Weapon] Pool에서 Bullet을 가져오지 못했습니다.\n" +
                        "Prefab ID : " + prefabId
                    );

                    return;
                }


                bullet =
                    bulletObject.transform;


                bullet.SetParent(
                    transform
                );
            }


            // =================================================
            // Bullet 위치
            // =================================================

            bullet.localPosition =
                Vector3.zero;

            bullet.localRotation =
                Quaternion.identity;


            // =================================================
            // 회전
            // =================================================

            Vector3 rotVec =
                Vector3.forward *
                360f *
                index /
                count;


            bullet.Rotate(rotVec);


            // =================================================
            // 거리
            // =================================================

            bullet.Translate(
                bullet.up * 1.5f,
                Space.World
            );


            // =================================================
            // Bullet Component
            // =================================================

            Bullet bulletComponent =
                bullet.GetComponent<Bullet>();


            if (bulletComponent == null)
            {
                Debug.LogError(
                    "[Weapon] Projectile에 Bullet.cs가 없습니다.\n" +
                    "Projectile : " + bullet.name
                );

                continue;
            }


            // =================================================
            // Bullet 초기화
            // =================================================

            // -100 = Infinity Per
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


        // =====================================================
        // GameManager 검사
        // =====================================================

        if (GameManager.instance == null)
            return;


        // =====================================================
        // Pool 검사
        // =====================================================

        if (GameManager.instance.pool == null)
        {
            Debug.LogError(
                "[Weapon] Pool이 없습니다."
            );

            return;
        }


        // =====================================================
        // Scanner 검사
        // =====================================================

        if (player.scanner == null)
        {
            Debug.LogWarning(
                "[Weapon] Player Scanner가 없습니다."
            );

            return;
        }


        // =====================================================
        // Target 검사
        // =====================================================

        if (!player.scanner.nearestTarget)
            return;


        // =====================================================
        // Projectile ID 검사
        // =====================================================

        if (!IsValidPrefabId())
        {
            Debug.LogError(
                "[Weapon] prefabId가 유효하지 않아 발사할 수 없습니다.\n" +
                "Weapon ID : " + id + "\n" +
                "Prefab ID : " + prefabId
            );

            return;
        }


        // =====================================================
        // Direction
        // =====================================================

        Vector3 targetPos =
            player.scanner.nearestTarget.position;


        Vector3 dir =
            targetPos -
            transform.position;


        if (dir.sqrMagnitude <= 0.0001f)
            return;


        dir.Normalize();


        // =====================================================
        // Get Bullet
        // =====================================================

        GameObject bulletObject =
            GameManager.instance.pool.Get(prefabId);


        if (bulletObject == null)
        {
            Debug.LogError(
                "[Weapon] Pool에서 Bullet을 가져오지 못했습니다.\n" +
                "Prefab ID : " + prefabId
            );

            return;
        }


        // =====================================================
        // Bullet Transform
        // =====================================================

        Transform bullet =
            bulletObject.transform;


        bullet.position =
            transform.position;


        bullet.rotation =
            Quaternion.FromToRotation(
                Vector3.up,
                dir
            );


        // =====================================================
        // Bullet Component
        // =====================================================

        Bullet bulletComponent =
            bullet.GetComponent<Bullet>();


        if (bulletComponent == null)
        {
            Debug.LogError(
                "[Weapon] 발사체에 Bullet.cs가 없습니다.\n" +
                "Projectile : " + bullet.name
            );

            return;
        }


        // =====================================================
        // Fire
        // =====================================================

        bulletComponent.Init(
            damage,
            count,
            dir,
            id
        );


        // =====================================================
        // Sound
        // =====================================================

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.Range
            );
        }
    }
}