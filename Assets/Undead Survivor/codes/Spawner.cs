using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public SpawnData[] spawnData;
    public float levelTime;

    [Header("Boss Settings")]
    [Tooltip("케르베로스 보스 프리팹 하나만 여기에 직접 등록해 주세요.")]
    public GameObject cerberusPrefab; // 🌟 단일 보스 프리팹 등록 변수로 단순화

    [Header("Boss Spawn Point Settings")]
    [Tooltip("보스가 등장할 위치 (설정 안 하면 플레이어 머리 위에서 소환됩니다)")]
    public Transform bossSpawnPoint;

    private bool isBossSpawning = false;

    int level;
    float timer;

    void Awake()
    {
        levelTime = GameManager.instance.maxGameTime / spawnData.Length;
    }

    void Update()
    {
        // 게임이 종료되었거나 현재 보스 타임이면 일반 몬스터 스폰을 원천 차단
        if (!GameManager.instance.isLive || GameManager.instance.isBossTime)
            return;

        timer += Time.deltaTime;
        level = Mathf.Min(Mathf.FloorToInt(GameManager.instance.gameTime / levelTime), spawnData.Length - 1);

        // 🌟 [최종 반영] 게임 타이머가 maxGameTime(종료 시간)에 도달했을 때 
        // 오직 단 한 번만 최종 보스 소환 루틴을 실행합니다.
        if (GameManager.instance.gameTime >= GameManager.instance.maxGameTime && !isBossSpawning)
        {
            StartCoroutine(BossSpawnRoutine());
            return;
        }

        // 일반 몹 스폰 타이머 작동
        if (timer > spawnData[level].spawnTime)
        {
            timer = 0;
            Spawn();
        }
    }

    void Spawn()
    {
        GameObject enemy = GameManager.instance.pool.Get(0);
        Transform randomPoint = transform.GetChild(Random.Range(1, transform.childCount));
        enemy.transform.position = randomPoint.position;
        enemy.GetComponent<Enemy>().Init(spawnData[level]);
    }
    // --- 🌟 [최종 반영] 게임 전체 시간이 종료되면 실행되는 보스 등장 연출 및 전투 제어 코루틴 ---
    IEnumerator BossSpawnRoutine()
    {
        isBossSpawning = true;

        // 1. GameManager에게 최종 보스 타임 진입을 알림
        GameManager.instance.isBossTime = true;

        // 🚨 프리팹 원본이 비어있다면 루틴을 즉시 탈출하여 게임이 굳는 것을 방지
        if (cerberusPrefab == null)
        {
            Debug.LogError("🚨 Spawner의 Cerberus Prefab 칸이 비어있어 보스를 소환할 수 없습니다!");
            GameManager.instance.isBossTime = false;
            isBossSpawning = false;
            yield break;
        }

        string currentBossName = cerberusPrefab.name.Replace("(Clone)", "");

        // 2. 보스 소환 위치 계산 및 소환 (설정 안 하면 플레이어 머리 위에서 등장)
        Vector3 spawnPos = bossSpawnPoint != null ? bossSpawnPoint.position : GameManager.instance.player.transform.position + new Vector3(0, 8f, 0);
        GameObject boss = Instantiate(cerberusPrefab, spawnPos, Quaternion.identity);

        // 3. 필드의 모든 일반 몹 제거 (보스와의 1대1 전투 환경 조성)
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

        // 4. 연출을 위한 유니티 시간 정지
        Time.timeScale = 0f;

        // 🎬 텍스트가 흘러가는 부드러운 등장 연출 실행 및 끝날 때까지 대기
        yield return StartCoroutine(BossUIController.instance.PlayBossAppearance(currentBossName));

        // 5. 연출이 끝났으므로 유니티 시간 재개
        Time.timeScale = 1.0f;

        // 6. 생성된 케르베로스에게 UI 슬라이더 컴포넌트 넘겨주기
        Boss bScript = boss.GetComponent<Boss>();
        if (bScript != null && BossUIController.instance.bossHPBar != null)
        {
            bScript.SetupHPBar(BossUIController.instance.bossHPBar.GetComponent<UnityEngine.UI.Slider>());
        }

        // 7. 보스가 완전히 처치될 때까지 대기
        yield return new WaitUntil(() => boss == null);

        // 8. 보스 처치 완료 후 체력바 끄기 및 플래그 초기화
        if (BossUIController.instance.bossHPBar != null)
            BossUIController.instance.bossHPBar.SetActive(false);

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
