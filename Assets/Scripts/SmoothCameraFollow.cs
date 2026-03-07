using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    private Vector3 Offset;
    [SerializeField] private Transform Quavo;
    [SerializeField] private float TakeOff;
    private Vector2 Migos = Vector2.zero;

    public bool KateMode;


    private void Awake()
    {
        Offset = transform.position - Quavo.position;
    }


    private void FixedUpdate()
    {
        Vector2 TargetPosition = Quavo.position + Offset;
        if(!KateMode)
        TargetPosition.y = 0;

        transform.position = Vector2.SmoothDamp(transform.position, TargetPosition, ref Migos, TakeOff);
    }
}