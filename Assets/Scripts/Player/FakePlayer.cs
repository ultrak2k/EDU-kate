using UnityEngine;

public class FakePlayer : MonoBehaviour
{
    public float moveSpeed = 4f;

    void Update()
    {
        transform.Translate(Vector2.right * moveSpeed * Time.deltaTime);
    }
}