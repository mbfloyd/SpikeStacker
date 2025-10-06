using UnityEngine;

public class CellVisual : MonoBehaviour
{
    public SpriteRenderer sr;

    public void SetType(CellType type)
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();

        switch (type)
        {
            case CellType.Block:
                sr.color = Color.gray;
                break;
            case CellType.Balloon:
                sr.color = Color.cyan;
                break;
            case CellType.Spike:
                sr.color = Color.red;
                break;
            default:
                sr.color = Color.clear;
                break;
        }
    }
}
