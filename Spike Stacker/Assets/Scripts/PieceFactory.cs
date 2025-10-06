using UnityEngine;

public class PieceFactory : MonoBehaviour
{
    public static PieceFactory Instance;
    public GameObject pieceContainerPrefab;
    public Transform spawnPoint; // optional, only for reference

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void SpawnNextPiece()
    {
        Vector2Int spawnGridPos = new Vector2Int(GridManager.Instance.width / 2 - 1,
                                                 GridManager.Instance.height - 1);

        GameObject pieceGO = Instantiate(pieceContainerPrefab, Vector3.zero, Quaternion.identity);
        Piece piece = pieceGO.GetComponent<Piece>();

        // --- Dynamic piece shape (Tetromino-like) ---
        Vector2Int[][] shapes = new Vector2Int[][]
        {
        new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // square
        new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(3,0) }, // line
        new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) }, // L
        new Vector2Int[] { new Vector2Int(2,0), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) }, // J
        new Vector2Int[] { new Vector2Int(1,0), new Vector2Int(2,0), new Vector2Int(0,1), new Vector2Int(1,1) }, // S
        new Vector2Int[] { new Vector2Int(0,0), new Vector2Int(1,0), new Vector2Int(1,1), new Vector2Int(2,1) }, // Z
        new Vector2Int[] { new Vector2Int(1,0), new Vector2Int(0,1), new Vector2Int(1,1), new Vector2Int(2,1) }, // T
        };

        Vector2Int[] shape = shapes[Random.Range(0, shapes.Length)];

        // --- Random cell types for each cell ---
        CellType[] types = new CellType[shape.Length];
        for (int i = 0; i < types.Length; i++)
        {
            CellType[] possible = { CellType.Block, CellType.Balloon, CellType.Spike };
            types[i] = possible[Random.Range(0, possible.Length)];
        }

        piece.Initialize(shape, types, spawnGridPos);

        // Check spawn collision
        if (!GridManager.Instance.IsValidPosition(piece, spawnGridPos))
        {
            GameManager.Instance.GameOver();
            Destroy(pieceGO);
            return;
        }
    }


}
