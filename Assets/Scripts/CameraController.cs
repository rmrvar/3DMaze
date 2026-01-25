using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private float _sensitivity = 2.0f;
    [SerializeField]
    private float _minPitch = -30.0f;
    [SerializeField]
    private float _maxPitch = 60.0f;

    [SerializeField]
    private float _initialRotationY = 90;

    private void Awake()
    {
        _rotationY = _initialRotationY;
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