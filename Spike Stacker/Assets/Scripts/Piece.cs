using UnityEngine;
using System.Collections.Generic;

public class Piece : MonoBehaviour
{
    [HideInInspector] public List<Vector2Int> Cells = new List<Vector2Int>();
    [HideInInspector] public List<CellType> cellTypes = new List<CellType>();
    [HideInInspector] public Vector2Int Position;

    private float fallTimer = 0f;
    public float fallInterval = 1f;
    public float softDropMultiplier = 0.1f;

    public enum PieceType { Block, Balloon, Spike, Mixed }
    public PieceType pieceType { get; private set; }

    // Reference to PieceVisual
    private PieceVisual visualComp;

    public void Initialize(Vector2Int[] cellPositions, CellType[] types, Vector2Int startPos)
    {
        Cells.Clear();
        cellTypes.Clear();
        Position = startPos;

        visualComp = GetComponent<PieceVisual>();
        visualComp.cellPrefab = GameManager.Instance.cellVisualPrefab;

        // Destroy any previous visuals
        foreach (Transform t in visualComp.transform)
            Destroy(t.gameObject);

        bool hasBlock = false, hasBalloon = false, hasSpike = false;

        for (int i = 0; i < cellPositions.Length; i++)
        {
            Cells.Add(cellPositions[i]);
            cellTypes.Add(types[i]);

            switch (types[i])
            {
                case CellType.Block: hasBlock = true; break;
                case CellType.Balloon: hasBalloon = true; break;
                case CellType.Spike: hasSpike = true; break;
            }

            visualComp.CreateCell(cellPositions[i], types[i]);
        }

        int typeCount = 0;
        if (hasBlock) typeCount++;
        if (hasBalloon) typeCount++;
        if (hasSpike) typeCount++;

        if (typeCount > 1) pieceType = PieceType.Mixed;
        else if (hasBlock) pieceType = PieceType.Block;
        else if (hasBalloon) pieceType = PieceType.Balloon;
        else pieceType = PieceType.Spike;

        UpdateCellPositions(); // position visuals at start
    }

    private void Update()
    {
        HandleInput();

        fallTimer += Time.deltaTime;
        if (fallTimer >= fallInterval)
        {
            fallTimer = 0f;
            Move(Vector2Int.down);
        }
    }

    private void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(Vector2Int.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2Int.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Rotate(1);
        if (Input.GetKeyDown(KeyCode.Z)) Rotate(-1);

        if (Input.GetKey(KeyCode.DownArrow))
        {
            fallTimer += Time.deltaTime / softDropMultiplier;
            if (fallTimer >= fallInterval)
            {
                fallTimer = 0f;
                Move(Vector2Int.down);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space)) HardDrop();
    }

    private void Move(Vector2Int dir)
    {
        Vector2Int newPos = Position + dir;
        if (GridManager.Instance.IsValidPosition(this, newPos))
        {
            Position = newPos;
            UpdateCellPositions();
        }
        else if (dir == Vector2Int.down)
        {
            LockPiece();
        }
    }

    private void HardDrop()
    {
        while (GridManager.Instance.IsValidPosition(this, Position + Vector2Int.down))
            Position += Vector2Int.down;

        UpdateCellPositions();
        LockPiece();
    }

    private void Rotate(int dir)
    {
        Vector2Int[] oldCells = Cells.ToArray();

        for (int i = 0; i < Cells.Count; i++)
        {
            int x = Cells[i].x;
            int y = Cells[i].y;
            Cells[i] = dir > 0 ? new Vector2Int(-y, x) : new Vector2Int(y, -x);
        }

        if (!GridManager.Instance.IsValidPosition(this, Position))
        {
            Vector2Int[] kicks = new Vector2Int[] { Vector2Int.right, Vector2Int.left, Vector2Int.up };
            bool kicked = false;
            foreach (var kick in kicks)
            {
                if (GridManager.Instance.IsValidPosition(this, Position + kick))
                {
                    Position += kick;
                    kicked = true;
                    break;
                }
            }

            if (!kicked)
                Cells = new List<Vector2Int>(oldCells);
        }

        UpdateCellPositions();
    }

    private void LockPiece()
    {
        int points = GridManager.Instance.PlacePieceAndResolve(this, GameManager.Instance.currentCombo);
        GameManager.Instance.AddScore(points);
        GameManager.Instance.OnPiecePlaced();
        enabled = false;
    }

    public void UpdateCellPositions()
    {
        if (visualComp == null) return;

        Vector3 origin = Vector3.zero; // bottom-left of your board in world space
        for (int i = 0; i < Cells.Count; i++)
        {
            Transform cell = visualComp.transform.GetChild(i);
            Vector2Int boardPos = Cells[i] + Position;
            cell.localPosition = new Vector3(boardPos.x, boardPos.y, 0) + origin;
        }
    }
}
