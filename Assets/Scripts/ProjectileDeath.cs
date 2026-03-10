using UnityEngine;

public class ProjectileDeath : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Destroy this GameObject 3 seconds after it is initialized
        Destroy(gameObject, 3f);
    }

    // Update is called once per frame
    void Update()
    {

    }
}