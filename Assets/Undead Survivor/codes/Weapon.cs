using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int id;
    public int prefabId;
    public float damage;
    public int count;
    public float speed;

    // =========================================================
    // 삽 / 낫 회전 공격 설정
    // =========================================================

    // 한 바퀴(360도) 도는 동안 몇 번 공격할 것인지
    // 숫자가 높을수록 공격 간격이 짧아짐
    [Header("Spin Weapon Attack")]
    [SerializeField] float spinHitCountPerRotation = 4f;

    float timer;
    Player player;

    void Awake()
    {
        player = GameManager.instance.player;
    }

    void Update()
    {
        if (!GameManager.instance.isLive)
            return;

        switch (id)
        {
            // =================================================
            // 삽 / 낫
            // =================================================
            case 0:
            case 5:
                transform.Rotate(Vector3.back * speed * Time.deltaTime);
                break;

            // =================================================
            // 일반 발사 무기
            // =================================================
            default:
                timer += Time.deltaTime;

                if (timer > speed)
                {
                    timer = 0f;
                    Fire();
                }
                break;
        }
    }

    // =========================================================
    // 무기 레벨업
    // =========================================================

    public void LevelUp(float damage, int count)
    {
        Debug.Log(
            $"<color=cyan>[Weapon LevelUp]</color> " +
            $"ID: {id} | " +
            $"기존 Weapon Damage: {this.damage:F1} | " +
            $"입력 Damage: {damage:F1} | " +
            $"Character.Damage: {Character.Damage:F2}"
        );

        this.damage = damage * Character.Damage;
        this.count += count;

        Debug.Log(
            $"<color=magenta>[Weapon LevelUp AFTER]</color> " +
            $"ID: {id} | " +
            $"변경된 damage: {this.damage:F1} | " +
            $"Character.Damage: {Character.Damage:F2}"
        );

        if (id == 0 || id == 5)
            Batch();

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    // =========================================================
    // 무기 초기화
    // =========================================================

    public void Init(ItemData data)
    {
        // =====================================================
        // Basic Set
        // =====================================================

        name = "Weapon" + data.itemId;
        
        transform.parent = player.transform;
        transform.localPosition = Vector3.zero;

        // =====================================================
        // Property Set
        // =====================================================

        id = data.itemId;

        damage = data.baseDamage * Character.Damage;

        count = data.baseCount + Character.Count;

        // =====================================================
        // Projectile 찾기
        // =====================================================

        for (int index = 0; index < GameManager.instance.pool.prefabs.Length; index++)
        {
            if (data.projectile == GameManager.instance.pool.prefabs[index])
            {
                prefabId = index;
                break;
            }
        }

        // =====================================================
        // Speed Set
        // =====================================================

        switch (id)
        {
            // =================================================
            // 삽 / 낫
            // =================================================

            case 0:
            case 5:
                speed = 150 * Character.WeaponSpeed;
                Batch();
                break;

            // =================================================
            // 일반 발사 무기
            // =================================================
            default:
                speed = 0.5f * Character.WeaponRate;
                break;
        }

        // =====================================================
        // Hand Set
        // =====================================================

        Hand hand = player.hands[(int)data.itemType];
        hand.spriter.sprite = data.hand;
        hand.gameObject.SetActive(true);

        player.BroadcastMessage("ApplyGear", SendMessageOptions.DontRequireReceiver);
    }

    // =========================================================
    // 삽 / 낫 Bullet 배치
    // =========================================================
    void Batch()
    {
        // -----------------------------------------------------
        // 회전 속도를 이용해서 공격 간격 계산
        //
        // speed = 초당 회전 각도
        //
        // 360도 / speed
        // = 한 바퀴 도는 데 걸리는 시간
        //
        // spinHitCountPerRotation을 적용하면
        // 한 바퀴당 여러 번 공격 가능
        // -----------------------------------------------------

        float spinHitInterval = 0f;

        if (speed > 0f && spinHitCountPerRotation > 0f)
        {
            spinHitInterval = 360f / (speed * spinHitCountPerRotation);
        }

        Debug.Log(
            $"<color=cyan>[Spin Weapon]</color> " +
            $"ID: {id} | " +
            $"회전속도: {speed:F1}°/s | " +
            $"공격간격: {spinHitInterval:F3}초"
        );

        // -----------------------------------------------------
        // Bullet 생성 / 배치
        // -----------------------------------------------------

        for (int index = 0; index < count; index++)
        {
            Transform bullet;

            if (index < transform.childCount)
            {
                bullet = transform.GetChild(index);
            }
            else
            {
                bullet = GameManager.instance.pool.Get(prefabId).transform;
                bullet.parent = transform;
            }

            bullet.localPosition = Vector3.zero;
            bullet.localRotation = Quaternion.identity;

            Vector3 rotVec = Vector3.forward * 360 * index / count;
            bullet.Rotate(rotVec);
            bullet.Translate(bullet.up * 1.5f, Space.World);

            // -------------------------------------------------
            // 삽 / 낫
            //
            // per = -100
            // → 무한 관통
            //
            // spinHitInterval
            // → 회전 속도에 따른 공격 간격
            // -------------------------------------------------

            bullet.GetComponent<Bullet>().Init(damage, -100, Vector3.zero, id, spinHitInterval); // -100 is Infinity Per.
        }
    }

    // =========================================================
    // 일반 발사
    // =========================================================
    void Fire()
    {
        if (!player.scanner.nearestTarget)
            return;

        Vector3 targetPos = player.scanner.nearestTarget.position;
        Vector3 dir = targetPos - transform.position;
        dir = dir.normalized;

        Debug.Log(
            $"<color=lime>[Weapon Fire]</color> " +
            $"무기 ID: {id} | " +
            $"Weapon.damage: {damage:F1} | " +
            $"count: {count} | " +
            $"prefabId: {prefabId}"
        );

        Transform bullet = GameManager.instance.pool.Get(prefabId).transform;
        bullet.position = transform.position;
        bullet.rotation = Quaternion.FromToRotation(Vector3.up, dir);

        // =====================================================
        // Bullet 초기화
        // =====================================================

        bullet.GetComponent<Bullet>().Init(damage, count, dir, id, 0f);

        // =====================================================
        // Bullet 생성 확인 로그
        // =====================================================

        Debug.Log(
            $"<color=cyan>[Bullet 생성]</color> " +
            $"이름: {bullet.name} | " +
            $"위치: {bullet.position} | " +
            $"방향: {dir} | " +
            $"velocity: {dir * 15f} | " +
            $"damage: {damage:F1}"
        );

        AudioManager.instance.PlaySfx(AudioManager.Sfx.Range);
    }
}
