using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("# Game Control")]
    public bool isLive;
    public bool isBossTime; // 🌟 시간 종료 후 보스가 등장했음을 알리는 플래그
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

        player.gameObject.SetActive(true);
        uiLevelUp.Select(playerId % 2);
        Resume();

        AudioManager.instance.PlayBGM(true);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Select);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverRoutine());
    }

    IEnumerator GameOverRoutine()
    {
        isLive = false;

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Lose();
        Stop();

        AudioManager.instance.PlayBGM(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Lose);
    }

    public void GameVictroy()
    {
        StartCoroutine(GameVictroyRoutine());
    }

    IEnumerator GameVictroyRoutine()
    {
        isLive = false;
        enemyCleaner.SetActive(true);

        yield return new WaitForSeconds(0.5f);

        uiResult.gameObject.SetActive(true);
        uiResult.Win();
        Stop();

        AudioManager.instance.PlayBGM(false);
        AudioManager.instance.PlaySfx(AudioManager.Sfx.Win);
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
        // 라이브 상태가 아니면 업데이트 정지
        if (!isLive)
            return;

        // 🌟 [교정] 보스 타임이 아닐 때만 타이머가 흐르고, 제한 시간에 도달하면 타이머를 max로 고정합니다.
        // 플래그를 여기서 켜지 않고 스포너가 직접 통제할 수 있도록 양보합니다.
        if (!isBossTime)
        {
            gameTime += Time.deltaTime;

            if (gameTime > maxGameTime)
            {
                gameTime = maxGameTime;
            }
        }

        // ────────────────────────────────────────────────────────
        // 🚨 [물리/태그/레이어 무시 마스터 치트 시스템] - 유지
        // ────────────────────────────────────────────────────────
        Boss liveBoss = Object.FindFirstObjectByType<Boss>();
        if (liveBoss != null)
        {
            Bullet[] currentBullets = Object.FindObjectsByType<Bullet>(FindObjectsSortMode.None);

            foreach (Bullet bullet in currentBullets)
            {
                if (bullet == null || !bullet.gameObject.activeInHierarchy) continue;

                float distance = Vector2.Distance(bullet.transform.position, liveBoss.transform.position);

                if (distance <= 3.0f)
                {
                    float finalDmg = bullet.damage <= 0 ? 50f : bullet.damage;
                    liveBoss.TakeDamage(finalDmg);
                    Debug.Log($"<color=#FFFF00>[GameManager 확정 중계 타격]</color> 보스 감지 성공! 거리: {distance:F2}m | 데미지: {finalDmg} 강제 주입");

                    if (bullet.id != 0 && bullet.id != 5)
                    {
                        bullet.per--;
                        if (bullet.per < 0)
                        {
                            Rigidbody2D bulletRigid = bullet.GetComponent<Rigidbody2D>();
                            if (bulletRigid != null) bulletRigid.linearVelocity = Vector2.zero;
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
            PlayerStats playerStats = player.GetComponent<PlayerStats>();
            if (playerStats != null)
            {
                playerStats.currentExp = exp;
            }
        }

        if (exp == nextExp[Mathf.Min(level, nextExp.Length - 1)])
        {
            level++;
            exp = 0;

            if (player != null)
            {
                PlayerStats playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.currentExp = 0;
                }
            }

            uiLevelUp.Show();
        }
    }

    public void Stop()
    {
        isLive = false;
        Time.timeScale = 0;
        uiJoy.localScale = Vector3.zero;
    }

    public void Resume()
    {
        isLive = true;
        Time.timeScale = 1;
        uiJoy.localScale = Vector3.one;
    }
}
