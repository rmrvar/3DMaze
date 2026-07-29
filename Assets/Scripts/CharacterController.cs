using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [SerializeField] 
    private float moveSpeed = 5f;
    [SerializeField]
    private float groundCheckDistance = 10000;
    [SerializeField]
    private LayerMask _groundLayer;
    [SerializeField]
    private Transform _foot;
    [SerializeField]
    private float _radius;
    [SerializeField]
    private Transform _lookRoot;
    [SerializeField]
    private Rigidbody _rb;
    
    private void FixedUpdate()
    {
        _surfaceNormal = -_rb.position.normalized;

        var up = _rb.rotation * Vector3.up;
        
        var targetRotation = Quaternion.FromToRotation(up, _surfaceNormal) * _rb.rotation;
        _rb.MoveRotation(targetRotation);

        var right = Vector3.ProjectOnPlane(_lookRoot.right, _surfaceNormal).normalized;
        var forward = Vector3.ProjectOnPlane(_lookRoot.forward, _surfaceNormal).normalized;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector2 input = (new Vector2(h, v)).normalized;

        var moveDirection = forward * input.y + right * input.x;
        
        _rb.MovePosition(_rb.position.normalized * (_radius + _foot.transform.localPosition.y) + moveDirection * (moveSpeed * Time.deltaTime));
    }
    
    private Vector3 _surfaceNormal = Vector3.up;
}