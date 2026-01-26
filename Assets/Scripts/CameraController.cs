using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float _sensitivity = 2.0F;
    [SerializeField]
    private float _minPitch = -60.0F;
    [SerializeField]
    private float _maxPitch = +60.0F;

    [SerializeField]
    private float _initialRotationX = 90;

    private void Awake()
    {
        _rotationX = _initialRotationX;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        _rotationY += Input.GetAxis("Mouse X") * _sensitivity;
        _rotationX -= Input.GetAxis("Mouse Y") * _sensitivity;
        _rotationX = Mathf.Clamp(_rotationX, _minPitch, _maxPitch);

        transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);
    }
    
    private float _rotationX;
    private float _rotationY;
}