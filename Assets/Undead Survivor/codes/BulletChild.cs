using UnityEngine;

public class BulletChild : MonoBehaviour
{
    Bullet parentBullet;

    void Awake()
    {
        parentBullet = GetComponentInParent<Bullet>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (parentBullet != null)
        {
            parentBullet.SendMessage("OnTriggerEnter2D", collision, SendMessageOptions.DontRequireReceiver);
        }
    }
}
