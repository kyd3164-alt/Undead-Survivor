using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;


    // =========================================================
    // Game Control
    // =========================================================

    [Header("# Game Control")]

    public bool isLive;

    public bool isBossTime;

    public float gameTime;

    public float maxGameTime = 2 * 10f;


    // =========================================================
    // Player Info
    // =========================================================

    [Header("# Player Info")]

    public int playerId;

    public float health;

    public float maxHealth = 100f;

    public int level;

    public int kill;

    public int exp;

    public int[] nextExp =
    {
        3,
        5,
        8,
        12,
        17,
        23,
        30,
        38,
        47,
        57
    };


    // =========================================================
    // Game Object
    // =========================================================

    [Header("# Game Object")]

    public PoolManager pool;

    public Player player;

    public LevelUp uiLevelUp;

    public Result uiResult;

    public Transform uiJoy;

    public GameObject enemyCleaner;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        instance =
            this;


        Application.targetFrameRate =
            60;
    }


    // =========================================================
    // Game Start
    // =========================================================

    public void GameStart(
        int id)
    {
        playerId =
            id;


        health =
            maxHealth;


        if (player != null)
        {
            player.gameObject.SetActive(
                true
            );
        }
        else
        {
            Debug.LogError(
                "GameManager: Player가 연결되지 않았습니다!"
            );
        }


        if (uiLevelUp != null)
        {
            uiLevelUp.Select(
                playerId % 2
            );
        }
        else
        {
            Debug.LogError(
                "GameManager: uiLevelUp이 연결되지 않았습니다!"
            );
        }


        Resume();


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(
                true
            );


            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.Select
            );
        }
    }


    // =========================================================
    // Game Over
    // =========================================================

    public void GameOver()
    {
        StartCoroutine(
            GameOverRoutine()
        );
    }


    IEnumerator GameOverRoutine()
    {
        isLive =
            false;


        yield return new WaitForSeconds(
            0.5f
        );


        if (uiResult != null)
        {
            uiResult.gameObject.SetActive(
                true
            );


            uiResult.Lose();
        }


        Stop();


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(
                false
            );


            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.Lose
            );
        }
    }


    // =========================================================
    // Victory
    // =========================================================

    public void GameVictroy()
    {
        StartCoroutine(
            GameVictroyRoutine()
        );
    }


    IEnumerator GameVictroyRoutine()
    {
        isLive =
            false;


        if (enemyCleaner != null)
        {
            enemyCleaner.SetActive(
                true
            );
        }


        yield return new WaitForSeconds(
            0.5f
        );


        if (uiResult != null)
        {
            uiResult.gameObject.SetActive(
                true
            );


            uiResult.Win();
        }


        Stop();


        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayBGM(
                false
            );


            AudioManager.instance.PlaySfx(
                AudioManager.Sfx.Win
            );
        }
    }


    // =========================================================
    // Retry
    // =========================================================

    public void GameRetry()
    {
        Time.timeScale =
            1f;


        SceneManager.LoadScene(
            0
        );
    }


    // =========================================================
    // Quit
    // =========================================================

    public void GameQuit()
    {
        Application.Quit();
    }


    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        if (!isLive ||
            isBossTime)
        {
            return;
        }


        gameTime +=
            Time.deltaTime;


        if (gameTime >
            maxGameTime)
        {
            gameTime =
                maxGameTime;


            GameVictroy();

            return;
        }


        // -----------------------------------------------------
        // Boss 직접 타격 보정
        // -----------------------------------------------------

        Boss liveBoss =
            Object.FindFirstObjectByType<Boss>();


        if (liveBoss == null)
            return;


        Bullet[] currentBullets =
            Object.FindObjectsByType<Bullet>(
                FindObjectsSortMode.None
            );


        foreach (
            Bullet bullet
            in currentBullets)
        {
            if (bullet == null ||
                !bullet.gameObject.activeInHierarchy)
            {
                continue;
            }


            float distance =
                Vector2.Distance(
                    bullet.transform.position,
                    liveBoss.transform.position
                );


            if (distance >
                3f)
            {
                continue;
            }


            float finalDmg =
                bullet.damage <= 0f
                    ? 50f
                    : bullet.damage;


            liveBoss.TakeDamage(
                finalDmg
            );


            if (bullet.id != 0 &&
                bullet.id != 5)
            {
                bullet.per--;


                if (bullet.per < 0)
                {
                    Rigidbody2D bulletRigid =
                        bullet.GetComponent<Rigidbody2D>();


                    if (bulletRigid != null)
                    {
                        bulletRigid.linearVelocity =
                            Vector2.zero;
                    }


                    bullet.gameObject.SetActive(
                        false
                    );
                }
            }
        }
    }


    // =========================================================
    // EXP
    // =========================================================

    public void GetExp()
    {
        if (!isLive)
            return;


        exp++;


        UpdatePlayerExp();


        if (nextExp == null ||
            nextExp.Length == 0)
        {
            Debug.LogError(
                "GameManager: nextExp 배열이 비어 있습니다!"
            );

            return;
        }


        int requiredExp =
            nextExp[
                Mathf.Clamp(
                    level,
                    0,
                    nextExp.Length - 1
                )
            ];


        if (exp >= requiredExp)
        {
            level++;

            exp = 0;


            UpdatePlayerExp();


            if (uiLevelUp != null)
            {
                uiLevelUp.Show();
            }
            else
            {
                Debug.LogError(
                    "GameManager: uiLevelUp이 연결되지 않았습니다!"
                );
            }
        }
    }


    // =========================================================
    // Player EXP
    // =========================================================

    void UpdatePlayerExp()
    {
        if (player == null)
            return;


        PlayerStats playerStats =
            player.GetComponent<PlayerStats>();


        if (playerStats != null)
        {
            playerStats.currentExp =
                exp;
        }
    }


    // =========================================================
    // Stop
    // =========================================================

    public void Stop()
    {
        isLive =
            false;


        Time.timeScale =
            0f;


        if (uiJoy != null)
        {
            uiJoy.localScale =
                Vector3.zero;
        }
    }


    // =========================================================
    // Resume
    // =========================================================

    public void Resume()
    {
        isLive =
            true;


        Time.timeScale =
            1f;


        if (uiJoy != null)
        {
            uiJoy.localScale =
                Vector3.one;
        }
    }
}