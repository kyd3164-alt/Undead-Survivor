using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;

    public float speed;

    public Scanner scanner;

    public Hand[] hands;

    public RuntimeAnimatorController[] animCon;


    float baseSpeed;

    Rigidbody2D rigid;

    SpriteRenderer spriter;

    Animator anim;


    // =========================================================
    // Awake
    // =========================================================

    void Awake()
    {
        rigid =
            GetComponent<Rigidbody2D>();


        spriter =
            GetComponent<SpriteRenderer>();


        anim =
            GetComponent<Animator>();


        scanner =
            GetComponent<Scanner>();


        hands =
            GetComponentsInChildren<Hand>(
                true
            );


        baseSpeed =
            speed;
    }


    // =========================================================
    // OnEnable
    // =========================================================

    void OnEnable()
    {
        if (GameManager.instance == null)
            return;


        ApplyBaseSpeed();


        if (animCon != null &&
            GameManager.instance.playerId >= 0 &&
            GameManager.instance.playerId <
            animCon.Length)
        {
            if (anim != null)
            {
                anim.runtimeAnimatorController =
                    animCon[
                        GameManager.instance.playerId
                    ];
            }
        }
    }


    // =========================================================
    // 기본 이동속도
    // =========================================================

    void ApplyBaseSpeed()
    {
        speed =
            baseSpeed *
            Character.Speed;
    }


    // =========================================================
    // Update
    // =========================================================

    void Update()
    {
        if (GameManager.instance == null ||
            !GameManager.instance.isLive)
        {
            return;
        }
    }


    // =========================================================
    // FixedUpdate
    // =========================================================

    void FixedUpdate()
    {
        if (GameManager.instance == null ||
            !GameManager.instance.isLive)
        {
            return;
        }


        if (rigid == null)
            return;


        Vector2 nextVec =
            inputVec *
            speed *
            Time.fixedDeltaTime;


        rigid.MovePosition(
            rigid.position +
            nextVec
        );
    }


    // =========================================================
    // LateUpdate
    // =========================================================

    void LateUpdate()
    {
        if (GameManager.instance == null ||
            !GameManager.instance.isLive)
        {
            return;
        }


        if (anim != null)
        {
            anim.SetFloat(
                "Speed",
                inputVec.magnitude
            );
        }


        if (spriter != null &&
            inputVec.x != 0f)
        {
            spriter.flipX =
                inputVec.x < 0f;
        }
    }


    // =========================================================
    // Move Input
    // =========================================================

    void OnMove(
        InputValue value)
    {
        inputVec =
            value.Get<Vector2>();
    }
}