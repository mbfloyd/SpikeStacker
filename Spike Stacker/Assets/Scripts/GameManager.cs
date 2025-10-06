using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score & Level")]
    public int Score { get; private set; } = 0;
    public int currentCombo { get; private set; } = 1; // start at 1x multiplier

    public int level = 1;
    public int linesCleared = 0;
    public int linesPerLevel = 10;
    public float gravitySpeed = 1f;

    [Header("UI")]
    public TMP_Text scoreText;
    public TMP_Text levelText;
    public TMP_Text linesText;

    [Header("Cell Prefab for PieceVisual")]
    public GameObject cellVisualPrefab;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {

        // Spawn the first piece
        PieceFactory.Instance.SpawnNextPiece();
        UpdateUI();
    }

    public void AddScore(int points)
    {
        Score += points * currentCombo;

        // Approximate lines cleared from points
        linesCleared += points / 100; // adjust divisor to balance scoring
        if (linesCleared >= level * linesPerLevel)
        {
            level++;
            gravitySpeed *= 0.9f; // speed up falling
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (scoreText != null) scoreText.text = $"Score: {Score}";
        if (levelText != null) levelText.text = $"Level: {level}";
        if (linesText != null) linesText.text = $"Lines: {linesCleared}";
    }

    // Called from Piece.cs after a piece is placed
    public void SpawnNextPiece()
    {
        PieceFactory.Instance.SpawnNextPiece();
    }

    // Optional: Game over check
    public void GameOver()
    {
        Debug.Log("Game Over!");
        Time.timeScale = 0f;
    }

    public void OnPiecePlaced()
    {
        // Reset combo for next piece, or you could implement consecutive combo logic
        currentCombo = 1;

        GridManager.Instance.CheckForFullRows(/* lastPlacedCell */ new Vector2Int(0, 0));
        PieceFactory.Instance.SpawnNextPiece();
    }

    public void IncrementCombo()
    {
        currentCombo++;
    }

}
