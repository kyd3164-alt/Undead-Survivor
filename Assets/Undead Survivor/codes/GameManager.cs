using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive;
    public bool isBossTime; // 🌟 [추가] 보스 등장 중일 때 게임 타이머를 멈추기 위한 플래그
    public float gameTime;
    public float maxGameTime = 2 * 10f;

    [Header("# Player Info")]
    public int playerId;
    public float health;
    public float maxHealth = 100;
    public int level;
    public int kill;
    public int exp;
    public int[] nextExp = { 3, 5, 8, 12, 17, 23, 30, 38, 47, 57 };

    [Header("# Game Object")]
    public PoolManager pool;
    public Player player;
    public LevelUp uiLevelUp;
    public Result uiResult;
    public Transform uiJoy;
    public GameObject enemyCleaner;

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

    public void GameStart(int id)
    {
        playerId = id;
        health = maxHealth;

        // Player 방어
        if (player != null)
        {
            player.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("❌ GameManager: Player가 연결되지 않았습니다!");
        }

        // LevelUp UI 방어
        if (uiLevelUp != null)
        {
            uiLevelUp.Select(playerId % 2);    // 임시 스크립트 (첫번째 캐릭터 선택)
        }
        else
        {
            Debug.LogError("❌ GameManager: uiLevelUp이 연결되지 않았습니다!");
        }

        Resume();

        // AudioManager 방어
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(true);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager: AudioManager.instance가 없습니다.");
        }
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        // Result UI 방어
        if (uiResult != null)
        {
            uiResult.gameObject.SetActive(true);
            uiResult.Lose();
        }
        else
        {
            Debug.LogError("❌ GameManager: uiResult가 연결되지 않았습니다!");
        }

        Stop();

        // AudioManager 방어
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
        }
    }

    public void GameVictroy()
    {
        StartCoroutine(GameVictroyRoutine());
    }

    IEnumerator GameVictroyRoutine()
    {
        isLive = false;

        // Enemy Cleaner 방어
        if (enemyCleaner != null)
        {
            enemyCleaner.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ GameManager: enemyCleaner가 연결되지 않았습니다.");
        }

        yield return new WaitForSeconds(0.5f);

        // Result UI 방어
        if (uiResult != null)
        {
            uiResult.gameObject.SetActive(true);
            uiResult.Win();
        }
        else
        {
            Debug.LogError("❌ GameManager: uiResult가 연결되지 않았습니다!");
        }

        Stop();

        // AudioManager 방어
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(false);
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
        }
    }

    public void GameRetry()
    {
        SceneManager.LoadScene(0);
    }

    public void GameQuit()
    {
        Application.Quit();
    }

    void Update()
    {
        // 1. [기존 기능 유지] 게임이 끝났거나 보스 타임이면 타이머 멈춤
        if (!isLive || isBossTime)
            return;

        gameTime += Time.deltaTime;

        if (gameTime > maxGameTime)
        {
            gameTime = maxGameTime;
            GameVictroy();
        }

        // ────────────────────────────────────────────────────────
        // 🚨 [새로 추가됨: 물리/태그/레이어 100% 무시 마스터 치트 시스템]
        // 씬 안에 복사 생성된 'Boss' 스크립트 본체를 다이렉트로 탐색합니다.
        // ────────────────────────────────────────────────────────
        Boss liveBoss = Object.FindFirstObjectByType<Boss>();

        if (liveBoss != null)
        {
            // 현재 활성화되어 날아다니고 있는 모든 무기/총알들을 싹 긁어모읍니다.
            Bullet[] currentBullets =
                Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);

            foreach (Bullet bullet in currentBullets)
            {
                // 이미 꺼졌거나 메모리에서 날아간 무기는 검사 제외
                if (bullet == null || !bullet.gameObject.activeInHierarchy)
                    continue;

                // 💡 핵심: 무기의 현재 위치와 보스 본체의 현재 위치 사이의 절대적인 물리 거리 계산
                float distance =
                    Vector2.Distance(
                        bullet.transform.position,
                        liveBoss.transform.position
                    );

                // 현재 화면의 대형 케르베로스 덩치를 감안하여 반지름 3.0m 이내로 무기가 스쳐 지나가기만 하면 무조건 적중 판정!
                if (distance <= 3.0f)
                {
                    // 무기 대미지가 0 이하로 파싱 버그가 걸려있다면 안전장치로 50 대미지 고정 적용
                    float finalDmg =
                        bullet.damage <= 0 ? 50f : bullet.damage;

                    // 🚨 물리 충돌(Trigger)을 거치지 않고 GameManager가 직접 보스의 피통을 강제로 깎아버립니다!
                    liveBoss.TakeDamage(finalDmg);

                    Debug.Log(
                        $"<color=#FFFF00>[GameManager 확정 중계 타격]</color> " +
                        $"보스 감지 성공! 거리: {distance:F2}m | " +
                        $"데미지: {finalDmg} 강제 주입"
                    );

                    // 무한 관통 무기(id 0번, 5번)가 아니라면 대미지를 가했으므로 무기 오브젝트 소멸 처리
                    if (bullet.id != 0 && bullet.id != 5)
                    {
                        bullet.per--;

                        if (bullet.per < 0)
                        {
                            Rigidbody2D bulletRigid =
                                bullet.GetComponent<Rigidbody2D>();

                            if (bulletRigid != null)
                            {
                                bulletRigid.linearVelocity = Vector2.zero;
                            }

                            bullet.gameObject.SetActive(false);
                        }
                    }
                }
            }
        }
    }

    public void GetExp()
    {
        if (!isLive)
            return;

        exp++;

        if (player != null)
        {
            PlayerStats playerStats =
                player.GetComponent<PlayerStats>();

            if (playerStats != null)
            {
                playerStats.currentExp = exp;
            }
        }

        // nextExp 배열 방어
        if (nextExp == null || nextExp.Length == 0)
        {
            Debug.LogError("❌ GameManager: nextExp 배열이 비어 있습니다!");
            return;
        }

        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;

            if (player != null)
            {
                PlayerStats playerStats =
                    player.GetComponent<PlayerStats>();

                if (playerStats != null)
                {
                    playerStats.currentExp = 0;
                }
            }

            // LevelUp UI 방어
            if (uiLevelUp != null)
            {
                uiLevelUp.Show();
            }
            else
            {
                Debug.LogError("❌ GameManager: uiLevelUp이 연결되지 않았습니다!");
            }
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;

        if (uiJoy != null)
            uiJoy.localScale = Vector3.zero;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;

        if (uiJoy != null)
            uiJoy.localScale = Vector3.one;
    }
}