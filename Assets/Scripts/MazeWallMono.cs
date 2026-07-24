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
            Mathf.Abs(Vector3.Dot(extremaFromTo, w)) + r // Hack: The r was added because it works. 
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

    public void SetAnimProgress(float animProgress)
    {
        if (!_isLowering)
        {
            return;
        }

        if (animProgress < 1)
        {
            return;
        }

        _renderer.enabled = false;
        _isLowering = false;
    }


    private void OnRaise()
    {
        _isLowering = false;
        SetRaisedness(true);
    }

    private void OnLower()
    {
        _isLowering = true;
        SetRaisedness(false);
    }

    private void SetRaisedness(bool isRaised)
    {
        _collider.enabled = isRaised;
        _renderer.enabled = isRaised || _prevIsRaised;
        _matPropertyBlock.SetFloat(PrevIsRaised, _prevIsRaised ? 1 : 0);
        _matPropertyBlock.SetFloat(CurrIsRaised, isRaised ? 1 : 0);
        _renderer.SetPropertyBlock(_matPropertyBlock);
        _prevIsRaised = isRaised;
    }

    private bool _isLowering;
    private bool _prevIsRaised;
    private Maze.Wall _wall;
    private MaterialPropertyBlock _matPropertyBlock;

    private static readonly int PrevIsRaised = Shader.PropertyToID("_PrevIsRaised");
    private static readonly int CurrIsRaised = Shader.PropertyToID("_CurrIsRaised");
}
