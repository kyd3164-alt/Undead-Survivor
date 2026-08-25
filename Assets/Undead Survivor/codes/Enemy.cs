using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed;
    public float health;
    public float maxHealth;
    public RuntimeAnimatorController[] animCon;
    public Rigidbody2D target;

    public bool isLive;

    public GameObject expOrbPrefab;

    Rigidbody2D rigid;
    Collider2D coll;
    Animator anim;
    SpriteRenderer spriter;
    WaitForFixedUpdate wait;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        wait = new WaitForFixedUpdate();
    }

    void FixedUpdate()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;

        if (!isLive || target == null || anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"))
            return;

        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
        rigid.linearVelocity = Vector2.zero;
    }

    void LateUpdate()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;

        if (!isLive || target == null)
            return;

        spriter.flipX = target.position.x < rigid.position.x;
    }

    void OnEnable()
    {
        // 🌟 플레이어가 생성된 이후에 안전하게 타겟을 잡도록 수정
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            target = GameManager.instance.player.GetComponent<Rigidbody2D>();
        }

        isLive = true;
        coll.enabled = true;
        rigid.simulated = true;
        spriter.sortingOrder = 2;
        anim.SetBool("Dead", false);
        health = maxHealth;
    }

    public void Init(SpawnData data)
    {
        anim.runtimeAnimatorController = animCon[data.spriteType];
        speed = data.speed;
        maxHealth = data.health;
        health = data.health;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Bullet") || !isLive)
            return;

        Bullet bullet = collision.GetComponent<Bullet>();

        if (bullet == null)
        {
            bullet = collision.GetComponentInParent<Bullet>();
        }

        if (bullet != null)
        {
            // 실제 적에게 들어가는 피해
            float finalDamage = bullet.damage;

            health -= bullet.damage;

            // ==========================================
            // 🩸 블러드 히트
            // ==========================================
            if (Item.BloodHitRate > 0f)
            {
                PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();

                if (playerHealth != null)
                {
                    float healAmount = finalDamage * Item.BloodHitRate;

                    playerHealth.Heal(healAmount);

                    Debug.Log(
                        $"<color=green>[블러드 히트]</color> " +
                        $"Enemy 피해: {finalDamage:F1} | " +
                        $"흡혈률: {Item.BloodHitRate * 100f:F1}% | " +
                        $"회복량: {healAmount:F1}"
                    );
                }
            }
        }

        StartCoroutine(KnockBack());

        if (health > 0)
        {
            anim.SetTrigger("Hit");
            AudioManager.instance.PlaySfx(AudioManager.Sfx.Hit);
        }
        else
        {
            isLive = false;
            coll.enabled = false;
            rigid.simulated = false;
            spriter.sortingOrder = 1;

            anim.ResetTrigger("Hit");

            anim.SetBool("Dead", true);
            GameManager.instance.kill++;

            if (expOrbPrefab != null)
            {
                Instantiate(expOrbPrefab, transform.position, Quaternion.identity);
            }

            if (GameManager.instance.isLive)
                AudioManager.instance.PlaySfx(AudioManager.Sfx.Dead);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (GameManager.instance == null || !GameManager.instance.isLive || !isLive)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f * Time.deltaTime);
            }
        }
    }

    void OnTriggerStay2D(Collider2D collision)
    {
        if (GameManager.instance == null || !GameManager.instance.isLive || !isLive)
            return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(10f * Time.deltaTime);
            }
        }
    }

    IEnumerator KnockBack()
    {
        yield return wait;
        if (GameManager.instance != null && GameManager.instance.player != null)
        {
            Vector3 playerPos = GameManager.instance.player.transform.position;
            Vector3 dirVec = transform.position - playerPos;
            rigid.AddForce(dirVec.normalized * 3, ForceMode2D.Impulse);
        }
    }

    public void Dead()
    {
        gameObject.SetActive(false);
    }
}