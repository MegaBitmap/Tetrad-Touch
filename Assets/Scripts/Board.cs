using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }
    public Piece nextPiece { get; private set; }
    public Piece savedPiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int holdiPosition = new Vector3Int(-4, 10, 0);
    public Vector3Int previewPosition = new Vector3Int(3, 11, 0);
    public Vector3Int previewiPosition = new Vector3Int(2, 10, 0);
    public Vector3Int holdPosition = new Vector3Int(-4, 11, 0);

    public Vector3Int[] cells { get; private set; }
    public TetrominoData data;
    private int lineClears;
    private bool pieceHeld = false;
    public static bool pieceSwapped;
    Score score;
    Menu menu;
    
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    public RectInt NoTopBounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, new Vector2Int(10, 21));
        }
    }

    private void Awake()
    {
        menu = GameObject.FindAnyObjectByType<Menu>();
        score = GameObject.FindObjectOfType<Score>();
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();
        nextPiece = gameObject.AddComponent<Piece>();
        nextPiece.enabled = false;
        savedPiece = gameObject.AddComponent<Piece>();
        savedPiece.enabled = false;


        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    public void Start()
    {
        Piece.startTouchPosition.Set(0, -10000); //prevent hard drop of first piece after gameover
        pieceSwapped = false;
        pieceHeld = false;
        score.ResetScore();
        score.SetLevel(1);
        tilemap.ClearAllTiles();
        lineClears = 0;
        SetNextPiece();
        SpawnPiece();
    }

    private void SetNextPiece()
    {
        // Clear the existing piece from the board
        if (nextPiece.cells != null)
        {
            Clear(nextPiece);
        }

        // Pick a random tetromino to use
        int random = Random.Range(0, tetrominoes.Length);
        TetrominoData data = tetrominoes[random];

        // Initialize the next piece with the random data
        // Draw it at the "preview" position on the board
        if (random == 0)
        {
            nextPiece.Initialize(this, previewiPosition, data);
        }
        else
        {
            nextPiece.Initialize(this, previewPosition, data);
        }
        
        Set(nextPiece);
    }

    public void SpawnPiece()
    {
        // Initialize the active piece with the next piece data
        activePiece.Initialize(this, spawnPosition, nextPiece.data);

        // Only spawn the piece if valid position otherwise game over
        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }

        // Set the next random piece
        SetNextPiece();
    }

    public void GameOver()
    {
        menu.TransferScore();

        tilemap.ClearAllTiles();
        score.ResetScore();
        score.SetLevel(1);
        lineClears = 0;

        menu.ShowGameOver();
    }

    public void Set(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, piece.data.tile);
        }
    }

    public void Clear(Piece piece)
    {
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + piece.position;
            tilemap.SetTile(tilePosition, null);
        }
    }

    public void SwapPiece()
    {
        // Temporarily store the current saved data so we can swap
        TetrominoData savedData = savedPiece.data;
        TetrominoData currentPiece = activePiece.data;

        if (pieceSwapped || savedPiece.data.tile == activePiece.data.tile)
        {
            return;
        }
        pieceSwapped = true;

        // Clear the existing saved piece from the board
        if (savedData.cells != null)
        {
            Clear(savedPiece);
        }

        // Store the active piece as the new saved piece
        // Draw this piece at the "hold" position on the board
        if (currentPiece.tile == tetrominoes[0].tile)
        {
            savedPiece.Initialize(this, holdiPosition, currentPiece);
        }
        else
        {
            savedPiece.Initialize(this, holdPosition, currentPiece);
        }
        
        Set(savedPiece);

        // Swap the saved piece to be the active piece
        if (pieceHeld)
        {
            // Clear the existing active piece before swapping
            Clear(activePiece);

            // Re-initialize the active piece with the saved data
            activePiece.Initialize(this, spawnPosition, savedData);
            Set(activePiece);
        }
        else
        {
            SpawnPiece();
        }
        pieceHeld = true;
    }

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = NoTopBounds;

        // The position is only valid if every cell is valid
        for (int i = 0; i < piece.cells.Length; i++)
        {
            Vector3Int tilePosition = piece.cells[i] + position;

            // An out of bounds tile is invalid
            if (!bounds.Contains((Vector2Int)tilePosition))
            {
                return false;
            }

            // A tile already occupies the position, thus invalid
            if (tilemap.HasTile(tilePosition))
            {
                return false;
            }
        }

        return true;
    }

    public void ClearLines()
    {
        RectInt bounds = Bounds;
        int row = bounds.yMin;

        // Clear from bottom to top
        while (row < bounds.yMax)
        {
            // Only advance to the next row if the current is not cleared
            // because the tiles above will fall down when a row is cleared
            if (IsLineFull(row))
            {
                LineClear(row);
            }
            else
            {
                row++;
            }
        }
    }

    public bool IsLineFull(int row)
    {
        RectInt bounds = Bounds;

        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);

            // The line is not full if a tile is missing
            if (!tilemap.HasTile(position))
            {
                return false;
            }
        }

        return true;
    }

    public void LineClear(int row)
    {
        score.AddScore(1000);
        score.AddMultiplier(1);
        lineClears++;

        if (lineClears > 10)
        {
            score.AddLevel(1);
            lineClears = 0;
        }

        RectInt bounds = Bounds;

        // Clear all tiles in the row
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            Vector3Int position = new Vector3Int(col, row, 0);
            tilemap.SetTile(position, null);
        }

        // Shift every row above down one
        while (row < bounds.yMax)
        {
            for (int col = bounds.xMin; col < bounds.xMax; col++)
            {
                Vector3Int position = new Vector3Int(col, row + 1, 0);
                TileBase above = tilemap.GetTile(position);

                position = new Vector3Int(col, row, 0);
                tilemap.SetTile(position, above);
            }

            row++;
        }
    }

    public void Save()
    {
        Clear(activePiece);
        score.SaveScore();

        RectInt bounds = Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            for (int row = bounds.yMin; row < bounds.yMax; row++)
            {
                Vector3Int position = new Vector3Int(col, row, 0);
                TileBase temp = tilemap.GetTile(position);
                if (tilemap.HasTile(position))
                {
                    PlayerPrefs.SetInt(col + "_pos_" + row, 1);
                    for (int i = 0; i < 7; i++)
                    {
                        if (temp == tetrominoes[i].tile)
                        {
                            PlayerPrefs.SetInt(col + "_color_" + row, i);
                            break;
                        }
                    }
                }
                else
                {
                    PlayerPrefs.SetInt(col + "_pos_" + row, 0);
                }
            }
        }
    }

    public void Load()
    {
        score.LoadScore();

        RectInt bounds = Bounds;
        for (int col = bounds.xMin; col < bounds.xMax; col++)
        {
            for (int row = bounds.yMin; row < bounds.yMax; row++)
            {
                Vector3Int position = new Vector3Int(col, row, 0);
                int tempTile = PlayerPrefs.GetInt(col + "_color_" + row);
                if (PlayerPrefs.GetInt(col + "_pos_" + row) == 1)
                {
                    tilemap.SetTile(position, tetrominoes[tempTile].tile);
                }
                else
                {
                    tilemap.SetTile(position, null);
                }
            }
        }
    }
}