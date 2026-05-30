using UnityEngine;

public class PlayerView : MonoBehaviour
{
    public float _speed = 3.0f;

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        Vector3 dir = new Vector3(horizontal, vertical, 0).normalized;
        transform.Translate(dir * _speed * Time.deltaTime);
    }
}
