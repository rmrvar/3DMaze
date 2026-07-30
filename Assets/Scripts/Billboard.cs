using UnityEngine;

public class Billboard : MonoBehaviour
{
    [field: SerializeField]
    public Transform LookTarget { get; set; }

    [field: SerializeField]
    public bool IsNegativeZ { get; set; } = true;

    private void LateUpdate()
    {
        if (LookTarget == null)
        {
            return;
        }
        Vector3 forward = (LookTarget.position - transform.position) * (IsNegativeZ ? -1 : +1);
        Vector3 up = transform.parent?.up ?? Vector3.up;
        transform.rotation = Quaternion.LookRotation(forward, up);
    }
}
