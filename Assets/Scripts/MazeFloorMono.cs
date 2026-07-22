using UnityEngine;

public class MazeFloorMono : MonoBehaviour
{
    [SerializeField]
    private Renderer _mazeFloorRenderer;

    private void Start()
    {
        float R = GameManager.Instance.MazeRadius - 0.1F;

        Material material = _mazeFloorRenderer.sharedMaterial;
        material.SetVector("_MazeCenter", Vector3.zero);
        material.SetFloat("_MazeRadius", R);
        material.SetVector("_FloorColor", GameManager.Instance.FloorColor);
        transform.localScale = new Vector3(2 * R, 2 * R, 2 * R);
    }
}
