using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public SpawnData[] spawnData;
    public float levelTime;

    [Header("Boss Settings")]
    public GameObject[] bossPrefabs;    // 순서대로 보스 1, 2, 3 할당 (Element 0은 비우고 1부터 사용)

    [Header("Boss Spawn Point Settings")]
    [Tooltip("보스가 등장할 위치 (설정 안 하면 스포너 위치에서 소환됩니다)")]
    public Transform bossSpawnPoint;    // 보스가 등장할 전용 위치

    private bool[] bossSpawnedList;     // 각 구간별 보스 소환 여부 체크 배열
    private bool isBossSpawning = false; // 현재 보스 전투/연출 진행 중인지 여부

    int level;
    float timer;

    void Awake()
    {
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;

        // 스폰 데이터(구간) 개수만큼 보스 소환 체크 배열 크기 생성
        bossSpawnedList = new bool[spawnData.Length];
    }

    void Update()
    {
        // 게임이 끝났거나 현재 보스 전투 중이면 일반 스포너 로직을 멈춤
        if (!GameManager.instance.isLive || isBossSpawning)
            return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        // --- 💡 각 구간의 중간 시간 계산 로직 ---
        float currentLevelStartTime = level * levelTime;
        float levelMidTime = currentLevelStartTime + (levelTime * 0.5f); // 해당 구간의 정중앙 시간

        // 현재 시간이 중간 시간을 지났고, 이 구간에서 아직 보스를 소환한 적이 없다면?
        if (level > 0 && !bossSpawnedList[level] && GameManager.instance.gameTime >= levelMidTime)
        {
            StartCoroutine(BossSpawnRoutine());
            return;
        }

        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);

        // 🌟 스포너의 자식 Transform들 중에서 랜덤으로 위치 지정 (자기 자신인 0번 인덱스 제외)
        Transform randomPoint = transform.GetChild(Random.Range(1, transform.childCount));
        enemy.transform.position = randomPoint.position;

        enemy.GetComponent<Enemy>().Init(spawnData[level]);
    }

    // --- 🌟 보스 등장 및 시간 정지/대기 코루틴 ---
    // --- 🌟 보스 등장 및 시간 정지/대기 코루틴 ---
    IEnumerator BossSpawnRoutine()
    {
        isBossSpawning = true;
        bossSpawnedList[level] = true;

        // 1. GameManager에게 보스 타임 진입을 알림
        GameManager.instance.isBossTime = true;

        // 2. 🚨 [안전성 업그레이드] 인스펙터 배열 세팅에 맞추어 현재 level과 인덱스를 매칭하되,
        // 혹시나 인스펙터 배열 크기가 level 수보다 작을 경우 에러가 나지 않도록 최댓값 방어 코드를 적용합니다.
        int bossIndex = Mathf.Min(level, bossPrefabs.Length - 1);

        // 🚨 만약 실수로 해당 칸이 비어있다면(Null) 루틴을 즉시 탈출하여 게임이 굳는 것을 방지합니다.
        if (bossPrefabs[bossIndex] == null)
        {
            Debug.LogError($"🚨 Spawner의 Boss Prefabs 배열 중 [Element {bossIndex}] 칸이 비어있어 보스를 소환할 수 없습니다!");
            GameManager.instance.isBossTime = false;
            isBossSpawning = false;
            yield break;
        }

        GameObject currentBossPrefab = bossPrefabs[bossIndex];
        string currentBossName = currentBossPrefab.name.Replace("(Clone)", "");

        // 3. 보스 소환 위치 계산 및 소환
        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : GameManager.instance.player.transform.position + new Vector3(0, 8f, 0);
        GameObject boss = Instantiate(currentBossPrefab, spawnPos, Quaternion.identity);

        // 4. 일반 몹 제거
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            if (enemy != null)
            {
                Rigidbody2D enemyRigid = enemy.GetComponent<Rigidbody2D>();
                if (enemyRigid != null) enemyRigid.linearVelocity = Vector2.zero;
                enemy.SetActive(false);
            }
        }

        // 5. 연출을 위한 시간 정지
        Time.timeScale = 0f;

        // 🚨 텍스트가 흘러가는 부드러운 연출 코루틴 실행 및 끝날 때까지 대기
        yield return StartCoroutine(BossUIController.instance.PlayBossAppearance(currentBossName));

        // 6. 연출이 끝났으므로 유니티 시간 재개 (이때 경험치 밑의 체력바가 켜짐)
        Time.timeScale = 1.0f;

        // 7. 생성된 보스에게 UI 슬라이더 컴포넌트 넘겨주기
        Boss bScript = boss.GetComponent<Boss>();
        if (bScript != null && BossUIController.instance.bossHPBar != null)
        {
            bScript.SetupHPBar(BossUIController.instance.bossHPBar.GetComponent<UnityEngine.UI.Slider>());
        }

        // 8. 보스가 죽을 때까지 대기
        yield return new WaitUntil(() => boss == null);

        // 9. 보스 처치 완료 후 체력바 끄기
        if (BossUIController.instance.bossHPBar != null) BossUIController.instance.bossHPBar.SetActive(false);

        GameManager.instance.isBossTime = false;
        isBossSpawning = false;
    }

}

[System.Serializable]
public class SpawnData
{
    public float spawnTime;
    public int spriteType;
    public int health;
    public float speed;
}
