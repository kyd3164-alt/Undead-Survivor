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
    IEnumerator BossSpawnRoutine()
    {
        isBossSpawning = true;
        bossSpawnedList[level] = true; // 현재 레벨 구간의 보스는 소환했다고 체크

        Debug.Log($"웨이브 {level}구간 중간 지점 도달! 보스 등장 및 시간 일시 정지");

        // 1. GameManager에게 보스 타임(시간 정지) 진입을 알림
        GameManager.instance.isBossTime = true;

        // 2. 현재 웨이브(level)에 딱 맞는 보스를 배열에서 꺼내서 소환!
        int bossIndex = Mathf.Min(level, bossPrefabs.Length - 1);
        GameObject currentBossPrefab = bossPrefabs[bossIndex];

        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : transform.position;
        GameObject boss = Instantiate(currentBossPrefab, spawnPos, Quaternion.identity);

        // 3. 보스가 죽어서 파괴(null)될 때까지 스포너와 웨이브 진행을 대기
        yield return new WaitUntil(() => boss == null);

        Debug.Log("보스 처치 완료! 게임 시간 재개 및 웨이브 복구");

        // 4. 보스가 죽으면 GameManager에게 시간 재개를 알림
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