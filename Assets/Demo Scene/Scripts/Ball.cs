using System;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public event Action<Vector3, Quaternion> OnBounce;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Floor")
        {
            OnBounce?.Invoke(transform.position, transform.rotation);
        }
    }

    private void Awake()
    {
        Destroy(gameObject, 10f);
    }
}
