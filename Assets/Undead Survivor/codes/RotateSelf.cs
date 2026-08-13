using UnityEngine;

public class RotateSelf : MonoBehaviour
{
    public float rotateSpeed = 720f;

    void Update()
    {
        transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
    }
}
