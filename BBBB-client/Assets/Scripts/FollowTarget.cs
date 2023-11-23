using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    [SerializeField]
    public Transform Target;
    [SerializeField]
    private Vector3 Offset;
    private void Update()
    {
        if (Target == null) return;
        transform.position = Target.position + Offset;
    }
}