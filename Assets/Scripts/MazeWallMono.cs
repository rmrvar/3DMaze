using Unity.VisualScripting;
using UnityEngine;

public class MazeWallMono : MonoBehaviour
{
    [SerializeField] 
    private Transform _scaleRoot;
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private Renderer _renderer;
    [SerializeField]
    private Transform _position1DebugTransform;
    [SerializeField]
    private Transform _position2DebugTransform;

    private void Awake()
    {
        if (_collider == null && _scaleRoot != null)
        {
            _collider = _scaleRoot.GetComponent<Collider>();
        }
        if (_renderer == null && _scaleRoot != null)
        {
            _renderer = _scaleRoot.GetComponent<Renderer>();
        }
    }

    public void Init(Maze.Wall wall)
    {
        _animProgress = 1; // Maze starts with finished animation.

        _wall = wall;
        _wall.OnRaise += OnRaise;
        _wall.OnLower += OnLower;
        _matPropertyBlock = new MaterialPropertyBlock();
        OnLower();

        float h = GameManager.Instance.WallHeight;
        float r = GameManager.Instance.WallRadius;
        float R = GameManager.Instance.MazeRadius;

        var fromTo = wall.Position2 - wall.Position1;

        var position = wall.Position1 + fromTo * 0.5F; // This is purposefully not normalized.

        var v = -position.normalized;
        var w = fromTo.normalized;

        var rotation = Quaternion.LookRotation(w, v);

        // Logic, VW plane through wall. Position2 and maze sphere intersect in a circle w^2 + (v - R)^2 = R^2.
        var a = 1;
        var b = -2 * GameManager.Instance.MazeRadius;
        var c = r * r;
        float temp = Mathf.Sqrt(Mathf.Max(0, b * b - 4 * a * c));
        float root1 = (-b + temp) / (2 * a);
        float root2 = (-b - temp) / (2 * a);
        float root = Mathf.Max(root1, root2);

        float deltaV = root;
        float deltaW = r;

        Vector3 extrema = wall.Position2 + deltaV * v + deltaW * w;
        Vector3 extremaFromTo = extrema - position;
        Vector3 extents = new(
            r,
            (h + 1.0F) * 0.5F, // Doesn't have to be perfect here, just add some buffer.
            // IMPORTANT: Shader shape depends on Z extents.
            Mathf.Abs(Vector3.Dot(extremaFromTo, w))
        );

        // Must match extents.
        var wallCenter = position + v * (extents.y - 0.5F);

        _matPropertyBlock.SetVector("_WallU", Vector3.Cross(v, w));
        _matPropertyBlock.SetVector("_WallV", v);
        _matPropertyBlock.SetVector("_WallW", w);
        _matPropertyBlock.SetVector("_WallCenter", wallCenter);
        _matPropertyBlock.SetVector("_WallExtents", extents);


        transform.SetPositionAndRotation(wallCenter, rotation);
        _scaleRoot.transform.localScale = extents * 2;

        _position1DebugTransform.position = _wall.Position1;
        _position2DebugTransform.position = _wall.Position2;
    }

    public void ResetAnimProgress()
    {
        _animProgress = 0;
        _matPropertyBlock.SetFloat("_AnimProgress", _animProgress);
        _renderer.SetPropertyBlock(_matPropertyBlock);
    }

    public void UpdateAnimProgress(float deltaTime)
    {
        _animProgress += deltaTime * GameManager.Instance.AnimSpeed;
        _matPropertyBlock.SetFloat("_AnimProgress", _animProgress);
        _renderer.SetPropertyBlock(_matPropertyBlock);

        UpdateRendererEnabledness();
    }

    private void OnRaise()
    {
        SetRaisedness(true);
    }

    private void OnLower()
    {
        SetRaisedness(false);
    }

    private void SetRaisedness(bool isRaised)
    {
        _prevIsRaised = _currIsRaised;
        _currIsRaised = isRaised;

        _collider.enabled = _currIsRaised;
        UpdateRendererEnabledness();

        _matPropertyBlock.SetFloat(PrevIsRaised, _prevIsRaised ? 1 : 0);
        _matPropertyBlock.SetFloat(CurrIsRaised, _currIsRaised ? 1 : 0);
        _renderer.SetPropertyBlock(_matPropertyBlock);
    }

    private void UpdateRendererEnabledness()
    {
        if (_currIsRaised && _prevIsRaised && !_renderer.enabled)
        {
            // STILL (RAISED)
            _renderer.enabled = true;
        }  else
        if (!_currIsRaised && !_prevIsRaised && _renderer.enabled)
        {
            // STILL (LOWERED)
            _renderer.enabled = false;
        } else
        if (_currIsRaised && !_prevIsRaised)
        {
            // RAISING
            if (_animProgress > 0 && !_renderer.enabled)
            {
                _renderer.enabled = true;
            } else
            if (_animProgress <= 0 && _renderer.enabled)
            {
                _renderer.enabled = false;
            }
        } else
        if (!_currIsRaised && _prevIsRaised)
        {
            // LOWERING
            if (_animProgress < 1 && !_renderer.enabled)
            {
                _renderer.enabled = true;
            } else
            if (_animProgress >= 1 && _renderer.enabled)
            {
                _renderer.enabled = false;
            }
        }
    }

    private float _animProgress;
    private bool _prevIsRaised;
    private bool _currIsRaised;
    private Maze.Wall _wall;
    private MaterialPropertyBlock _matPropertyBlock;

    private static readonly int PrevIsRaised = Shader.PropertyToID("_PrevIsRaised");
    private static readonly int CurrIsRaised = Shader.PropertyToID("_CurrIsRaised");
}
