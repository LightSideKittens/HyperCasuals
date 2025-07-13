using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using LSCore.Extensions.Unity;
using UnityEngine;
using Random = UnityEngine.Random;

public class FieldManager : MonoBehaviour
{
    public List<Spawner> spawners;
    private List<Shape> activeShapes = new();
    private Dictionary<Block, Sprite> originalSprites;
    public Vector2Int gridSize;
    
    public ParticleSystem shapeAppearFx;
    
    public float defaultShapeSize = 0.85f;
    public SpriteRenderer back;

    public Dragger dragger;
    
    private Vector3 gridOffset;
    private Block[,] grid;
    public bool debug;
    private int spawnShapeLock = 0;
    private bool allShapesPlaced;
    private Color shapeColor;
    private Shape currentGhostShape;
    private Shape currentShape => dragger.currentShape;
    
    private int? lastUsedSpriteIndex = null;
    
    [NonSerialized] public List<List<(Vector2Int index, Block block)>> suicidesData = new();
    [NonSerialized] public List<List<(Vector2Int index, Block block)>> uniqueSuicidesData = new();
    public List<(Vector2Int, Block)> duplicateIndexes = new();
    public IEnumerable<(Vector2Int index, Block block)> UniqueSuicidesData
    {
        get
        {
            for (int i = 0; i < uniqueSuicidesData.Count; i++)
            {
                var data = uniqueSuicidesData[i];
                for (var j = 0; j < data.Count; j++)
                {
                    var smallData = data[j];
                    yield return smallData;
                }
            }
        }
    }

    private void Awake()
    {
        originalSprites = new Dictionary<Block, Sprite>();
        grid = new Block[gridSize.x, gridSize.y];
        gridOffset = new Vector3(back.size.x / 2, back.size.y / 2) - LSVector3.half;
        CreateAndInitShape();

        dragger.Started += shape =>
        {
            ClearCurrentGhostShape();
            shape.transform.DOScale(Vector3.one, 0.2f);
            CreateGhostShape();
            currentGhostShape.gameObject.SetActive(false);
        };

        dragger.Ended += shape =>
        {
            ClearCurrentGhostShape();

            var gridIndices = new List<Vector2Int>();

            var canPlace = CanPlaceShape(shape, ref gridIndices);

            if (canPlace)
            {
                var sequence = DOTween.Sequence();

                for (int j = 0; j < shape.blocks.Count; j++)
                {
                    var gridIndex = gridIndices[j];
                    Vector2 worldPos = gridIndex - (Vector2)gridOffset;

                    var block = shape.blocks[j];
                    grid[gridIndex.x, gridIndex.y] = block;

                    block.transform.parent = null;
                    var tween = block.transform.DOMove(back.transform.TransformPoint(worldPos), 0.3f);
                    sequence.Insert(j * 0.025f, tween);
                }

                activeShapes.Remove(shape);

                if (ClearFullLines())
                {
                    BlocksDestroying?.Invoke(shape.BlockPrefab);
                }

                CheckLoseCondition();

                spawnShapeLock++;
                if (spawnShapeLock >= spawners.Count)
                {
                    spawnShapeLock = 0;
                    CreateAndInitShape();
                }

                Destroy(shape.gameObject);
            }
            else
            {
                shape.transform.DOMove(dragger.shapeStartPos, 0.6f).SetEase(Ease.InOutExpo);
                shape.transform.DOScale(Vector3.one * defaultShapeSize, 0.2f);
            }
        };
    }

    private async void CreateAndInitShape()
    {
        int totalSpawners = spawners.Count;

        List<Shape> allShapes = new();
        foreach (var spawner in spawners)
        {
            allShapes.AddRange(spawner.GetAllShapePrefabs());
        }

        List<Shape> validShapes = new();
        foreach (var shapePrefab in allShapes)
        {
            Shape temp = Instantiate(shapePrefab, transform.position, Quaternion.identity);
            bool canPlace = await CanPlace(temp);
            Destroy(temp.gameObject);
            if (canPlace)
            {
                validShapes.Add(shapePrefab);
            }
        }

        int guaranteedIndex = validShapes.Count > 0 ? Random.Range(0, totalSpawners) : -1;
        for (int i = 0; i < totalSpawners; i++)
        {
            var spawner = spawners[i];
            Shape shape;

            if (i == guaranteedIndex)
            {
                shape = spawner.SpawnSpecificShape(validShapes[Random.Range(0, validShapes.Count)],
                    ref lastUsedSpriteIndex);
                Debug.Log($"[SPAWN] Спавнер {i}: ГАРАНТИРОВАННАЯ фигура");
            }
            else
            {
                shape = spawner.SpawnRandomShape(ref lastUsedSpriteIndex);
            }

            activeShapes.Add(shape);
            
            shape.transform.localScale = Vector3.one * defaultShapeSize;
        }

        for (int i = 0; i < activeShapes.Count; i++)
        {
            var shape = activeShapes[i];
            
            shape.transform.localScale = Vector3.zero;
            shapeAppearFx.transform.localScale = Vector3.zero;
            
            
            shape.transform.DOScale(Vector3.one * defaultShapeSize, 0.2f).OnComplete(() =>
            {
                var appearFxInstance = Instantiate(shapeAppearFx, shape.transform.position, Quaternion.identity);
                appearFxInstance.transform.localScale = Vector3.zero;
                appearFxInstance.transform.DOScale(4, 3f).KillOnDestroy(); 
            });
        }
    }

    private void Update()
    {
        UpdateGhost();
    }

    private bool CanPlaceShape(Shape shape, ref List<Vector2Int> gridIndices)
    {
        bool canPlace = true;
        for (var j = 0; j < shape.blocks.Count; j++)
        {
            var block = shape.blocks[j];
            var localPos = back.transform.InverseTransformPoint(block.transform.position);
            localPos += gridOffset;
            var gridIndex = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.y));
            gridIndices.Add(gridIndex);

            if (gridIndex.x >= 0 && gridIndex.x < grid.GetLength(0)
                                 && gridIndex.y >= 0 && gridIndex.y < grid.GetLength(1))
            {
                if (grid[gridIndex.x, gridIndex.y] == null)
                    continue;
            }

            canPlace = false;
        }

        return canPlace;
    }


    public async Task<bool> CanPlace(Shape shape)
    {
        var prevScale = shape.transform.localScale;
        var prevPos = shape.transform.position;

        var result = await CheckCanPlace();
        
        shape.transform.localScale = prevScale;
        shape.transform.position = prevPos;
        
        return result;
        
        async Task<bool> CheckCanPlace()
        {
            shape.transform.localScale = Vector3.one;

            Vector2 r = shape.ratio;
            var tp = back.transform.position - (gridOffset + LSVector3.half) + ((Vector3)r / 2);
            shape.transform.position = tp;

            int xCount = grid.GetLength(0) - (shape.ratio.x - 1);
            int yCount = grid.GetLength(1) - (shape.ratio.y - 1);

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    shape.transform.position = tp + new Vector3(x, y);
                    if (debug)
                    {
                        await Task.Delay(200);
                    }

                    if (Check(shape))
                    {
                        return true;
                    }
                }
            }
        
            return false;
        }
    }
    
    private bool Check(Shape shape)
    {
        for (var i = 0; i < shape.blocks.Count; i++)
        {
            var block = shape.blocks[i];
            var localPos = back.transform.InverseTransformPoint(block.transform.position);
            localPos += gridOffset;
            var gridIndex = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.y));

            if (gridIndex.x < 0 || gridIndex.x >= grid.GetLength(0)
                                || gridIndex.y < 0 || gridIndex.y >= grid.GetLength(1)
                                || grid[gridIndex.x, gridIndex.y] != null)
            {
                return false;
            }
        }

        return true;
    }
    
    private bool TryGetGridIndices(Shape shape, out List<Vector2Int> gridIndices)
    {
        int w = grid.GetLength(0), h = grid.GetLength(1);
        gridIndices = new List<Vector2Int>(shape.blocks.Count);

        float cellWidth  = back.size.x / w;
        float cellHeight = back.size.y / h;

        foreach (var block in shape.blocks)
        {
            Vector3 loc = back.transform.InverseTransformPoint(block.transform.position);
            loc.x += back.size.x * 0.5f;
            loc.y += back.size.y * 0.5f;
            int ix = Mathf.FloorToInt(loc.x / cellWidth);
            int iy = Mathf.FloorToInt(loc.y / cellHeight);
            var idx = new Vector2Int(ix, iy);
            gridIndices.Add(idx);

            if (ix < 0 || ix >= w || iy < 0 || iy >= h || grid[ix, iy] != null)
                return false;
        }

        return true;
    }
    
    
    private void ClearCurrentGhostShape()
    {
        if (currentGhostShape == null) return;
        
        Destroy(currentGhostShape.gameObject);
        currentGhostShape = null;
    }

    private void CreateGhostShape()
    {
        currentGhostShape = currentShape.CreateGhost(currentShape);
    }

    private async void CheckLoseCondition()
    {
        Debug.Log($"[CheckLoseCondition] Checking lose condition: {activeShapes.Count}");
        if (activeShapes.Count == 0) return;

        foreach (var shape in activeShapes)
        {
            if (shape == null) continue;
            if (await CanPlace(shape))
            {
                Debug.Log("[CheckLoseCondition] At least one shape can be placed");
                return;
            }
            Debug.Log("[CheckLoseCondition] Can't place shape, checking next");
        }

        Debug.Log("[CheckLoseCondition] All shapes blocked -> Game Over");
        LoseWindow.Hiding -= ClearLoseWindow;
        LoseWindow.onReviveClicked -= Revive;
        LoseWindow.Hiding += ClearLoseWindow;
        LoseWindow.onReviveClicked += Revive;
        LoseWindow.Show();
    }

    private void ClearLoseWindow()
    {
        LoseWindow.onReviveClicked -= Revive;
        LoseWindow.Hiding -= ClearLoseWindow;
    }
    private void Revive()
    {
        Debug.Log("[Revive] called!");
        LoseWindow.onReviveClicked -= Revive;
        for (int i = activeShapes.Count - 1; i >= 0; i--)
        {
            var shape = activeShapes[i];
            if (shape != null) Destroy(shape.gameObject);
        }
        activeShapes.Clear();
        spawnShapeLock = 0;
        CreateAndInitShape();
    }
    
    public void UpdateGhost()
    {
        if(currentGhostShape == null) return;
        currentGhostShape.gameObject.SetActive(true);
        var gridIndices = new List<Vector2Int>();
        var canPlace = CanPlaceShape(currentShape, ref gridIndices);

        if (!canPlace)
        {
            if (originalSprites != null)
            {
                foreach (var kvp in originalSprites)
                {
                    if (kvp.Key != null)
                    {
                        kvp.Key.sprite = kvp.Value;
                    }
                }

                originalSprites.Clear();
            }
            
            currentGhostShape.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < currentGhostShape.blocks.Count; i++)
        {
            var gridIndex = gridIndices[i];
            Vector2 worldPos = gridIndex;
            worldPos -= (Vector2)gridOffset;
            worldPos = back.transform.TransformPoint(worldPos);

            currentGhostShape.blocks[i].transform.position = worldPos;
        }

        if (currentShape.blocks.Count > 0)
        {
            shapeColor = currentShape.blocks[0].color;
        }

        HighlightDestroyableLines(gridIndices, shapeColor);
    }

    private void HighlightDestroyableLines(List<Vector2Int> futureIndices, Color highlightColor) {
        foreach (var kvp in originalSprites)
        {
            if (kvp.Key != null)
            {
                kvp.Key.sprite = kvp.Value;
            }
        }

        originalSprites.Clear();

        int width = grid.GetLength(0);
        int height = grid.GetLength(1);

        bool[,] simulatedOccupied = new bool[width, height];

        foreach (var index in futureIndices)
        {
            if (index.x >= 0 && index.x < width && index.y >= 0 && index.y < height)
            {
                simulatedOccupied[index.x, index.y] = true;
            }
        }

        void HighlightCell(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height) return;

            var sprite = grid[x, y];
            if (sprite != null)
            {
                if (!originalSprites.ContainsKey(sprite))
                {
                    originalSprites[sprite] = sprite.sprite;
                }

                sprite.sprite = currentGhostShape.blocks[0].sprite;
            }
            else if (currentGhostShape != null && currentGhostShape.blocks != null)
            {
                foreach (var block in currentGhostShape.blocks)
                {
                    if (block == null) continue;

                    var localPos = back.transform.InverseTransformPoint(block.transform.position) + gridOffset;
                    var ghostIndex = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.y));
                    if (ghostIndex == new Vector2Int(x, y))
                    {
                        block.color = highlightColor;
                    }
                }
            }
        }

        for (int y = 0; y < height; y++)
        {
            bool fullRow = true;
            for (int x = 0; x < width; x++)
            {
                if (!simulatedOccupied[x, y] && grid[x, y] == null)
                {
                    fullRow = false;
                    break;
                }
            }

            if (fullRow)
            {
                for (int x = 0; x < width; x++)
                {
                    HighlightCell(x, y);
                }
            }
        }

        for (int x = 0; x < width; x++)
        {
            bool fullColumn = true;
            for (int y = 0; y < height; y++)
            {
                if (!simulatedOccupied[x, y] && grid[x, y] == null)
                {
                    fullColumn = false;
                    break;
                }
            }

            if (fullColumn)
            {
                for (int y = 0; y < height; y++)
                {
                    HighlightCell(x, y);
                }
            }
        }
    }

    private bool ClearFullLines()
    {
        Debug.Log("[ClearFullLines] Invoke");
        suicidesData.Clear();
        duplicateIndexes.Clear();
        uniqueSuicidesData.Clear();
        int w = grid.GetLength(0), h = grid.GetLength(1);
        var rows = new List<int>();
        var cols = new List<int>();

        for (int y = 0; y < h; y++)
        {
            bool full = true;
            for (int x = 0; x < w; x++)
                if (grid[x, y] == null) { full = false; break; }
            if (full) rows.Add(y);
        }
        for (int x = 0; x < w; x++)
        {
            bool full = true;
            for (int y = 0; y < h; y++)
                if (grid[x, y] == null) { full = false; break; }
            if (full) cols.Add(x);
        }

        int destroyed = 0;
        
        Debug.Log("[ClearFullLines] before foreach");
        
        foreach (int y in rows)
        {
            var rowsData = new List<(Vector2Int index, Block block)>();
            var rowsUniqueData = new List<(Vector2Int index, Block block)>();
            for (int x = 0; x < w; x++)
            {
                TryAddSuicide(rowsData, rowsUniqueData, new Vector2Int(x, y), grid[x, y]);
            }
            suicidesData.Add(rowsData);
            uniqueSuicidesData.Add(rowsUniqueData);
            destroyed++;
        }
        foreach (int x in cols)
        {
            var colsData = new List<(Vector2Int index, Block block)>();
            var colsUniqueData = new List<(Vector2Int index, Block block)>();
            for (int y = 0; y < h; y++)
            {
                TryAddSuicide(colsData, colsUniqueData, new Vector2Int(x, y), grid[x, y]);
            }
            suicidesData.Add(colsData);
            uniqueSuicidesData.Add(colsUniqueData);
            destroyed++;
        }
        
        for (var i = 0; i < suicidesData.Count; i++)
        {
            var data = suicidesData[i];
            for (var j = 0; j < data.Count; j++)
            {
                var smallData = data[j].index;
                grid[smallData.x, smallData.y] = data[j].block.next;
            }
        }

        if (destroyed > 0)
        {
        }
        
        Debug.Log("[ClearFullLines] Clear full lines");
        return destroyed > 0;
    }

    private void TryAddSuicide(List<(Vector2Int index, Block block)> data, List<(Vector2Int index, Block block)> uniqueData,  Vector2Int index , Block block)
    {
        if (suicidesData.Any(x => x.Any(y => y.index == index)))
        {
            duplicateIndexes.Add((index, block));
        }
        else
        {
            uniqueData.Add((index, block));
        }
        data.Add((index, block));
    }

    public event Action<Block> BlocksDestroying;
}