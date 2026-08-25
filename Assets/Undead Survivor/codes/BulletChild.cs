using UnityEngine;

public class BulletChild : MonoBehaviour
{
    Bullet parentBullet;

    void Awake()
    {
        parentBullet = GetComponentInParent<Bullet>();
    }

    // =========================================================
    // 처음 Boss에 들어갈 때
    // =========================================================

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (parentBullet != null)
        {
            parentBullet.SendMessage("OnTriggerEnter2D", collision, SendMessageOptions.DontRequireReceiver);
        }
    }

    // =========================================================
    // Boss 안에 계속 있을 때
    // =========================================================

    void OnTriggerStay2D(Collider2D collision)
    {
        if (parentBullet != null)
        {
            parentBullet.SendMessage(
                "OnTriggerStay2D",
                collision,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    // =========================================================
    // Boss에서 빠져나올 때
    // =========================================================

    void OnTriggerExit2D(Collider2D collision)
    {
        if (parentBullet != null)
        {
            parentBullet.SendMessage(
                "OnTriggerExit2D",
                collision,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }
}
