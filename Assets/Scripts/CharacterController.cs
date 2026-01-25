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
    
    private void Update()
    {
        transform.position = transform.position.normalized * _radius;
     
        _surfaceNormal = -transform.position.normalized;
        var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
        transform.rotation = targetRotation;
        
        // Quaternion.LookRotation
        
        
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        
        var forward = Vector3.ProjectOnPlane(_lookRoot.forward, _surfaceNormal).normalized;
        var right = Vector3.ProjectOnPlane(_lookRoot.right, _surfaceNormal).normalized;
        var moveDirection = forward * v + right * h;
        
        transform.position += moveDirection * (moveSpeed * Time.deltaTime);
        // Ray ray = new Ray(_foot.position + new Vector3(0, 0.5F, 0), -_surfaceNormal);
        // if (Physics.Raycast(ray, out var hit, groundCheckDistance, _groundLayer))
        // {
        //     Debug.Log("Hit something " + hit.collider.gameObject.name + " at " + hit.point);
        //     _surfaceNormal = -hit.point.normalized;  // It's a point on a sphere
        //     
        //     var targetRotation = Quaternion.FromToRotation(transform.up, _surfaceNormal) * transform.rotation;
        //     transform.rotation = targetRotation;
        //
        //     var targetPosition = hit.point + _surfaceNormal * (transform.position.y - _foot.position.y);
        //     transform.position = targetPosition;
        //     
        //     float h = Input.GetAxis("Horizontal");
        //     float v = Input.GetAxis("Vertical");
        //     var moveDirection = transform.forward * v + transform.right * h;
        //     
        //     transform.position += moveDirection * (moveSpeed * Time.deltaTime);
        // }
    }
    
    private Vector3 _surfaceNormal = Vector3.up;
}