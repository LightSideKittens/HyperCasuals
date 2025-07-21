using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using LSCore;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using SourceGenerators;
using UnityEngine;
using Random = UnityEngine.Random;

[InstanceProxy]
public partial class FieldManager : SingleService<FieldManager>
{
    [NonSerialized] public List<Spawner> _spawners = new();
    private List<Shape> activeShapes = new();
    
    public int blocksInLine = 8;
    public Vector2Int gridSize;
    
    public ParticleSystem shapeAppearFx;
    
    public float defaultShapeSize = 0.85f;
    public SpriteRenderer back;
    public SpriteRenderer selector;

    public Dragger dragger;
    
    private Vector3 gridOffset;
    public Block[,] grid;
    public bool debug;
    private int spawnShapeLock = 0;
    private bool allShapesPlaced;
    private Shape currentGhostShape;
    private Shape currentShape => dragger.currentShape;
    
    private int? lastUsedSpriteIndex = null;
    [NonSerialized] public List<List<Vector2Int>> linesIndices = new();
    
    public List<Block> _blockPrefabs;
    public List<Block> _specialBlockPrefabs;
    
    public static Bounds FieldRect => new(Instance.back.transform.position, (Vector2)Instance.gridSize);

    public Dictionary<Block, List<(Vector2Int index, Block block)>> GetSpecialBlocks(
        IEnumerable<Vector2Int> data)
    {
        var specialBlockPrefabs = new Dictionary<Block, List<(Vector2Int index, Block block)>>();
            
        foreach (var index in data)
        {
            var block = grid.Get(index);
            if(!block) continue;
            var prefab = block.prefab;
            if (_specialBlockPrefabs.Contains(prefab))
            {
                if (!specialBlockPrefabs.TryGetValue(prefab, out var list))
                {
                    list = new();
                    specialBlockPrefabs.Add(prefab, list);
                }
                list.Add((index, block));
            }
        }
        
        return specialBlockPrefabs;
    }

    private void Start()
    {
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
                var tween = DOTween.TweensByTarget(shape.transform, true);
                if (tween is { Count: > 0 })
                {
                    tween[0].Complete();
                }
                for (int j = 0; j < shape.blocks.Count; j++)
                {
                    var gridIndex = gridIndices[j];
                    Vector2 worldPos = ToPos(gridIndex);

                    var block = shape.blocks[j];
                    grid.Set(gridIndex, block);
                    
                    var parent = new GameObject("BlockParent").transform;
                    parent.position = block.transform.position;
                    block.transform.SetParent(parent, true);
                    parent.DOMove(worldPos, 0.3f).SetDelay(j * 0.025f).OnComplete(() =>
                    {
                        parent.DetachChildren();
                        Destroy(parent.gameObject);
                    });
                }

                activeShapes.Remove(shape);

                if (ClearFullLines())
                {
                    BlocksDestroying?.Invoke(shape.BlockPrefab);
                    linesIndices.Clear();
                }

                CheckLoseCondition();

                spawnShapeLock++;
                if (spawnShapeLock >= _spawners.Count)
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
        int totalSpawners = _spawners.Count;

        List<Shape> allShapes = new();
        foreach (var spawner in _spawners)
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
            var spawner = _spawners[i];
            Shape shape;

            if (i == guaranteedIndex)
            {
                shape = spawner.SpawnSpecificShape(validShapes.Random(), ref lastUsedSpriteIndex);
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

    public Vector2Int _ToIndex(Vector2 pos)
    {
        var localPos = back.transform.InverseTransformPoint(pos);
        localPos += gridOffset;
        var gridIndex = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.y));
        return gridIndex;
    }
    
    public Vector2 ToPos(Vector2Int index)
    {
        var localPos = back.transform.TransformPoint((Vector2)index);
        localPos -= gridOffset;
        return localPos;
    }

    public static bool TryPlaceBlock(Vector2Int index, Block prefab, out Block block) => Instance.Internal_TryPlaceBlock(index, prefab, out block);

    public bool Internal_TryPlaceBlock(Vector2Int index, Block prefab, out Block block)
    {
        block = null;
        if(!grid.HasIndex(index)) return false;
        block = Instantiate(prefab);
        _PlaceBlock(index, block);
        return true;
    }

    public void _PlaceBlock(Vector2Int index, Block block)
    {
        var existingBlock = grid.Get(index);
        block.transform.position = ToPos(index);
        if (existingBlock != null)
        {
            Shape.OverlayBlock(existingBlock, block);
        }
        grid.Set(index, block);
    }
    
    private bool CanPlaceShape(Shape shape, ref List<Vector2Int> gridIndices)
    {
        bool canPlace = true;
        for (var j = 0; j < shape.blocks.Count; j++)
        {
            var block = shape.blocks[j];
            var gridIndex = _ToIndex(block.transform.position);
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
    
    private async Task<bool> CanPlace(Shape shape)
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
            var gridIndex = _ToIndex(block.transform.position);

            if (gridIndex.x < 0 || gridIndex.x >= grid.GetLength(0)
                                || gridIndex.y < 0 || gridIndex.y >= grid.GetLength(1)
                                || grid[gridIndex.x, gridIndex.y] != null)
            {
                return false;
            }
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
    
    private void UpdateGhost()
    {
        selectionAreas.ReleaseAll();
        if(currentGhostShape == null) return;
        currentGhostShape.gameObject.SetActive(true);
        var gridIndices = new List<Vector2Int>();
        var canPlace = CanPlaceShape(currentShape, ref gridIndices);

        if (!canPlace)
        {
            currentGhostShape.gameObject.SetActive(false);
            return;
        }

        for (int i = 0; i < currentGhostShape.blocks.Count; i++)
        {
            var gridIndex = gridIndices[i];
            Vector2 worldPos = ToPos(gridIndex);
            currentGhostShape.blocks[i].transform.position = worldPos;
        }
        
        HighlightDestroyableLines(gridIndices);
    }

    private OnOffPool<SpriteRenderer> selectionAreas => OnOffPool<SpriteRenderer>.GetOrCreatePool(selector, back.transform, shouldStoreActive: true);

    private void HighlightDestroyableLines(List<Vector2Int> gridIndices)
    {
        for (int i = 0; i < gridIndices.Count; i++)
        {
            var index = gridIndices[i];
            grid.Set(index, currentGhostShape.blocks[i]);
        }
        var lines = GetBlockLines();
        for (int i = 0; i < gridIndices.Count; i++)
        {
            var index = gridIndices[i];
            grid.Set(index, null);
        }
        
        if (lines.Count == 0)
        {
            return;
        }

        for (int i = 0; i < lines.Count; i++)
        {
            var list = lines[i];
            var area = selectionAreas.Get();
            area.size = Vector2.one;
            var corner = list[0].index;
            area.transform.localPosition = corner - (Vector2)gridOffset - LSVector2.half;

            for (int j = 1; j < list.Count; j++)
            {
                var diff = list[j].index - corner;
                area.size += diff;
                corner = list[j].index;
            }
        }
    }

    private bool ClearFullLines()
    {
        linesIndices.Clear();
        
        var lines = GetBlockLines();
        
        foreach (var data in lines)
        {
            var lineIndices = new List<Vector2Int>(data.Count);
            linesIndices.Add(lineIndices);
            for (var i = 0; i < data.Count; i++)
            {
                lineIndices.Add(data[i].index);
            }
        }
        return linesIndices.Count > 0;
    }

    public List<List<(Vector2Int index, Block block)>> GetBlockLines(bool excludeNull, bool excludeSpecial, bool unique = true)
    {
        var result = new List<List<(Vector2Int index, Block block)>>();
        var set = new HashSet<Vector2Int>();
        var buffer = new List<(Vector2Int index, Block block)>();
        for (var i = 0; i < linesIndices.Count; i++)
        {
            var line = linesIndices[i];
            buffer.Clear();
            for (var j = 0; j < line.Count; j++)
            {
                var index = line[j];
                var block = grid.Get(index);
                if (excludeNull && block == null) continue;
                if (excludeSpecial && _specialBlockPrefabs.Contains(block.prefab)) continue;
                if(unique && !set.Add(index)) continue;
                buffer.Add((index, block));
            }

            if (buffer.Count > 0)
            {
                result.Add(new(buffer));
            }
        }

        return result;
    }

    public List<(Vector2Int index, Block block)> GetBlocks(bool excludeNull, bool excludeSpecial, bool unique = true)
    {
        var result = new List<(Vector2Int index, Block block)>();
        var lines = GetBlockLines(excludeNull, excludeSpecial, unique);
        for (var i = 0; i < lines.Count; i++)
        {
            var line = lines[i];
            result.AddRange(line);
        }
        return result;
    }

    private List<List<(Vector2Int index, Block block)>> GetBlockLines()
    {
        var size = grid.GetSize();
        var lines = new List<List<(Vector2Int index, Block block)>>();
        var buffer = new List<(Vector2Int index, Block block)>();

        for (int y = 0; y < size.y; y++)
        {
            buffer.Clear();
            for (int x = 0; x < size.x; x++)
            {
                AddLine(x, y);
            }
        }
        
        for (int x = 0; x < size.x; x++)
        {
            buffer.Clear();
            for (int y = 0; y < size.y; y++)
            {
                AddLine(x, y);
            }
        }
        
        return lines;

        void AddLine(int x, int y)
        {
            if (grid[x, y])
            {
                buffer.Add((new(x, y), grid[x, y]));
                if (buffer.Count == blocksInLine)
                {
                    lines.Add(new(buffer));
                    buffer.Clear();
                }
            }
            else
            {
                buffer.Clear();
            }
        }
    }

    public event Action<Block> BlocksDestroying;
}