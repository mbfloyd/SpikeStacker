using UnityEngine;

public class PieceVisual : MonoBehaviour
{
    [HideInInspector] public GameObject cellPrefab;

    public void SetCell(GameObject cellGO, CellType type)
    {
        cellGO.transform.SetParent(transform);
        cellGO.transform.localPosition = Vector3.zero;

        var sr = cellGO.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            switch (type)
            {
                case CellType.Block: sr.color = Color.white; break;
                case CellType.Balloon: sr.color = Color.yellow; break;
                case CellType.Spike: sr.color = Color.red; break;
                case CellType.Empty: sr.color = new Color(0, 0, 0, 0); break;
            }
        }
    }

    public void CreateCell(Vector2Int localPos, CellType type)
    {
        GameObject cellGO = Instantiate(cellPrefab, transform);
        cellGO.transform.localPosition = new Vector3(localPos.x, localPos.y, 0);
        SetCell(cellGO, type);
    }
}
