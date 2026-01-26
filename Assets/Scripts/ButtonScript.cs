using System.Collections;
using UnityEngine;

public class ButtonScript : MonoBehaviour
{
    [SerializeField]
    private Transform _uiRoot;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private ButtonScript _otherButton;
    
    [SerializeField]
    private MazeGenerator _mazeGenerator;

    [SerializeField]
    private float _sinkSpeed;
    [SerializeField]
    private bool _isHidden;
    
    private void Awake()
    {
        _originalPosition = transform.position;
        if (_isHidden)
        {
            transform.position = _originalPosition + -transform.up * 5;
        }
    }

    private void Update()
    {
        if (_isInside && _canPress && Input.GetKeyDown(KeyCode.F))
        {
            _canPress = false;
            StartCoroutine(IE_PlayAnimation());
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isInside = true;
            _uiRoot.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _isInside = false;
            _uiRoot.gameObject.SetActive(false);
        }
    }

    private IEnumerator IE_PlayAnimation()
    {
        Debug.Log("Press press!");
        _animator.SetTrigger("PressButton");
        while (!_animator.GetCurrentAnimatorStateInfo(0).IsName("PressButton"))
        {
            yield return null;
        }
        AnimatorStateInfo state;
        do
        {
            state = _animator.GetCurrentAnimatorStateInfo(0);
            yield return null;
        } while (state.normalizedTime < 1.0f);

        StartCoroutine(IE_LowerThis());
        yield return StartCoroutine(IE_RaiseThat());
        StartCoroutine(_mazeGenerator.IE_FlushKruskal());
    }

    private IEnumerator IE_LowerThis()
    {
        float t = 0;
        while (true)
        {
            t += Time.deltaTime * _sinkSpeed;
            t = Mathf.Clamp(t, 0, 1);
            transform.position = Vector3.Lerp(_originalPosition, _originalPosition + -transform.up * 5, t);
            if (t >= 1)
            {
                break;
            }
            yield return null;
        }
    }
    
    private IEnumerator IE_RaiseThat()
    {
        if (_otherButton._animator.GetCurrentAnimatorStateInfo(0).IsName("PressButton"))
        {
            _otherButton._animator.SetTrigger("Reset");   
        }
        float t = 0;
        while (true)
        {
            t += Time.deltaTime * _otherButton._sinkSpeed;
            t = Mathf.Clamp(t, 0, 1);
            _otherButton.transform.position = Vector3.Lerp(_otherButton._originalPosition + -_otherButton.transform.up * 5, _otherButton._originalPosition, t);
            if (t >= 1)
            {
                break;
            }
            yield return null;
        }
        _otherButton._canPress = true;
    }

    private Vector3 _originalPosition;
    private bool _isInside;
    private bool _canPress = true;
}
