using UnityEngine;
using UnityEngine.Tilemaps;

public class Board : MonoBehaviour
{
    public Tilemap tilemap { get; private set; }
    public Piece activePiece { get; private set; }

    public TetrominoData[] tetrominoes;
    public Vector2Int boardSize = new Vector2Int(10, 20);
    public Vector3Int spawnPosition = new Vector3Int(-1, 8, 0);
    public Vector3Int iPosition = new Vector3Int(-4, 10, 0);
    public Vector3Int nextPosition = new Vector3Int(-4, 11, 0);
    public Vector3Int[] cells { get; private set; }
    public TetrominoData data;
    private int lineClears;
    private int random;
    private bool pieceLoaded = false;
    Score score;
    
    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-boardSize.x / 2, -boardSize.y / 2);
            return new RectInt(position, boardSize);
        }
    }

    private void Awake()
    {
        score = GameObject.FindObjectOfType<Score>();
        tilemap = GetComponentInChildren<Tilemap>();
        activePiece = GetComponentInChildren<Piece>();

        for (int i = 0; i < tetrominoes.Length; i++)
        {
            tetrominoes[i].Initialize();
        }
    }

    public void Start()
    {
        random = Random.Range(0, tetrominoes.Length);
        score.ResetScore();
        lineClears = 0;
        SpawnPiece();
    }

    public void SpawnPiece()
    {
        if (pieceLoaded)
        {
            if (random == 0)
            {
                for (int i = 0; i < data.cells.Length; i++)
                {
                    Vector3Int tilePosition = (Vector3Int)data.cells[i] + iPosition;
                    tilemap.SetTile(tilePosition, null);
                }
            }
            else {
                for (int i = 0; i < data.cells.Length; i++)
                {
                    Vector3Int tilePosition = (Vector3Int)data.cells[i] + nextPosition;
                    tilemap.SetTile(tilePosition, null);
                }
            }
        }
        data = tetrominoes[random];

        activePiece.Initialize(this, spawnPosition, data);

        if (IsValidPosition(activePiece, spawnPosition))
        {
            Set(activePiece);
        }
        else
        {
            GameOver();
        }

        random = Random.Range(0, tetrominoes.Length);
        data = tetrominoes[random];
        pieceLoaded = true;

        if (cells == null)
        {
            cells = new Vector3Int[data.cells.Length];
        }
        for (int i = 0; i < data.cells.Length; i++)
        {
            cells[i] = (Vector3Int)data.cells[i];
        }
        if (random == 0)
        {
            for (int i = 0; i < data.cells.Length; i++)
            {
                Vector3Int tilePosition = (Vector3Int)data.cells[i] + iPosition;
                tilemap.SetTile(tilePosition, data.tile);
            }
        }
        else
        {
            for (int i = 0; i < data.cells.Length; i++)
            {
                Vector3Int tilePosition = (Vector3Int)data.cells[i] + nextPosition;
                tilemap.SetTile(tilePosition, data.tile);
            }
        }
    }

    public void GameOver()
    {
        tilemap.ClearAllTiles();
        score.ResetScore();
        score.SetLevel(1);
        lineClears = 0;
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

    public bool IsValidPosition(Piece piece, Vector3Int position)
    {
        RectInt bounds = Bounds;

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