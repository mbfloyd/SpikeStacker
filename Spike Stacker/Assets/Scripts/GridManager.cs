using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 10;
    public int height = 20;

    private CellType[,] grid;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        grid = new CellType[width, height];
        ClearGrid();
    }

    public void ClearGrid()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = CellType.Empty;
    }

    public bool IsInside(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public CellType GetCell(Vector2Int pos)
    {
        if (!IsInside(pos)) return CellType.Block; // treat out-of-bounds as solid
        return grid[pos.x, pos.y];
    }

    public void SetCell(Vector2Int pos, CellType type)
    {
        if (IsInside(pos))
            grid[pos.x, pos.y] = type;
    }

    // --- MAIN FUNCTION ---
    public (int points, int rowsCleared) CheckForFullRows(Vector2Int lastPlacedCell)
    {
        int totalPoints = 0;
        int rowsCleared = 0;

        for (int y = 0; y < height; y++)
        {
            bool fullRow = true;
            HashSet<CellType> typesInRow = new HashSet<CellType>();

            for (int x = 0; x < width; x++)
            {
                var cell = grid[x, y];
                typesInRow.Add(cell);
                if (cell == CellType.Empty)
                    fullRow = false;
            }

            if (fullRow)
            {
                rowsCleared++;

                if (typesInRow.Count == 1)
                {
                    var type = grid[lastPlacedCell.x, y]; // pick any cell type
                    switch (type)
                    {
                        case CellType.Block:
                            totalPoints += ClearRow(y);
                            break;

                        case CellType.Balloon:
                            totalPoints += ClearRow(y);
                            totalPoints += ClearRow(y + 1);
                            totalPoints += ClearRow(y - 1);
                            break;

                        case CellType.Spike:
                            totalPoints += ClearRow(y);
                            totalPoints += ClearColumn(lastPlacedCell.x);
                            break;
                    }
                }
            }
        }

        // Shift down remaining rows after clears
        CollapseRows();

        return (totalPoints, rowsCleared);
    }

    // --- CLEAR ROW ---
    public int ClearRow(int y)
    {
        if (y < 0 || y >= height) return 0;
        int points = 0;

        for (int x = 0; x < width; x++)
        {
            var cell = grid[x, y];
            switch (cell)
            {
                case CellType.Block: points += 100; break;
                case CellType.Balloon: points += 150; break;
                case CellType.Spike: points += 120; break;
            }

            grid[x, y] = CellType.Empty;
        }

        return points;
    }

    // --- CLEAR COLUMN (for spikes) ---
    public int ClearColumn(int x)
    {
        if (x < 0 || x >= width) return 0;
        int points = 0;

        for (int y = 0; y < height; y++)
        {
            var cell = grid[x, y];
            switch (cell)
            {
                case CellType.Block: points += 100; break;
                case CellType.Balloon: points += 150; break;
                case CellType.Spike: points += 120; break;
            }

            grid[x, y] = CellType.Empty;
        }

        return points;
    }

    // --- COLLAPSE ROWS ---
    private void CollapseRows()
    {
        for (int y = 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != CellType.Empty && grid[x, y - 1] == CellType.Empty)
                {
                    int row = y;
                    while (row > 0 && grid[x, row - 1] == CellType.Empty)
                    {
                        grid[x, row - 1] = grid[x, row];
                        grid[x, row] = CellType.Empty;
                        row--;
                    }
                }
            }
        }
    }

    public bool IsValidPosition(Piece piece, Vector2Int newPosition)
    {
        foreach (Vector2Int cell in piece.Cells)
        {
            Vector2Int boardPos = cell + newPosition;

            // Check bounds
            if (boardPos.x < 0 || boardPos.x >= width || boardPos.y < 0)
            {
                return false;
            }

            // Check top of grid (ok to be above top)
            if (boardPos.y >= height)
                continue;

            // Check if cell is already occupied
            if (grid[boardPos.x, boardPos.y] != CellType.Empty)
            {
                return false;
            }
        }

        return true;
    }

    public void SetPiece(Piece piece)
    {
        for (int i = 0; i < piece.Cells.Count; i++)
        {
            Vector2Int boardPos = piece.Cells[i] + piece.Position;

            if (boardPos.x >= 0 && boardPos.x < width && boardPos.y >= 0 && boardPos.y < height)
            {
                grid[boardPos.x, boardPos.y] = piece.cellTypes[i];
            }
        }
    }

    public void ClearFullRows()
    {
        for (int y = 0; y < height; y++)
        {
            bool full = true;
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] == CellType.Empty)
                {
                    full = false;
                    break;
                }
            }

            if (full)
            {
                ClearRow(y);
                ShiftRowsDown(y);
                y--; // recheck same row index after shifting
            }
        }
    }

    private void ShiftRowsDown(int clearedRow)
    {
        for (int y = clearedRow + 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                grid[x, y - 1] = grid[x, y];
            }
        }

        // Clear top row after shifting
        for (int x = 0; x < width; x++)
        {
            grid[x, height - 1] = CellType.Empty;
        }
    }


}
