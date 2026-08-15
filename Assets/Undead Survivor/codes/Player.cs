using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    public float speed;
    public Scanner scanner;
    public Hand[] hands;
    public RuntimeAnimatorController[] animCon;

    Rigidbody2D rigid;
    SpriteRenderer spriter;
    Animator anim;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        scanner = GetComponent<Scanner>();
        hands = GetComponentsInChildren<Hand>(true);
    }

    void OnEnable()
    {
        // 🌟 GameManager가 안전하게 존재할 때만 실행되도록 방어 코드 추가
        if (GameManager.instance != null)
        {
            speed *= Character.Speed;

            if (animCon != null && animCon.Length > GameManager.instance.playerId)
            {
                anim.runtimeAnimatorController = animCon[GameManager.instance.playerId];
            }
        }
    }

    void Update()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;
    }

    void FixedUpdate()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;

        Vector2 nextVec = inputVec * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }

    void LateUpdate()
    {
        if (GameManager.instance == null || !GameManager.instance.isLive)
            return;

        anim.SetFloat("Speed", inputVec.magnitude);

        if (inputVec.x != 0)
        {
            spriter.flipX = inputVec.x < 0;
        }
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
    }
}