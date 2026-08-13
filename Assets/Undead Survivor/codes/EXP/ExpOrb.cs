using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [Header("자석 효과 설정")]
    public float detectRadius = 5f;
    public float moveSpeed = 8f;

    private Transform playerTransform;
    private bool isFlying = false;

    private int myExp = 10;

    public void Init(int expValue)
    {
        myExp = expValue;
    }

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if (distance <= detectRadius || isFlying)
        {
            isFlying = true;

            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, moveSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.instance.GetExp();
            Destroy(gameObject);
        }
    }
}
