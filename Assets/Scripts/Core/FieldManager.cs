using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core;
using DG.Tweening;
using LSCore;
using LSCore.Extensions;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using SourceGenerators;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

[InstanceProxy]
public partial class FieldManager : SingleService<FieldManager>
{
    [NonSerialized] public List<Spawner> _spawners = new();
    private List<Shape> activeShapes = new();
    
    [MinValue(4)] [MaxValue("$MaxBlocksInLine")] public int blocksInLine = 8;
    [MinValue(4)] public Vector2Int gridSize;
    
    private int MaxBlocksInLine => Math.Min(gridSize.x, gridSize.y);
    
    
    public Feel.SoundAndHaptic feel;
    public ParticleSystem shapeAppearFx;
    
    public float defaultShapeSize = 1.4f;
    public SpriteRenderer back => FieldAppearance.Back;
    public SpriteRenderer selector => FieldAppearance.Selector;

    public Dragger dragger;
    public Shape initialShape;
    
    private Vector3 gridOffset;
    private float shapeSpawnerScale = 0.7f;
    private Vector3 defaultScale;
    public Block[,] grid;
    public bool debug;
    private int spawnShapeLock = 0;
    private bool allShapesPlaced;
    private Shape currentGhostShape;
    private Vector2Int lastGhostBlockIndex = new(-1, -1);
    private Shape currentShape => dragger.currentShape;
    
    private int? lastUsedSpriteIndex = null;
    [NonSerialized] public List<List<Vector2Int>> _linesIndices = new();
    
    public List<Block> blockPrefabs => FieldAppearance.BlockPrefabs;
    public List<Block> specialBlockPrefabs => FieldAppearance.SpecialBlockPrefabs;
    
    public static Bounds FieldRect => new(Instance.back.transform.position, 8f.ToVector2());
    public static Block[,] Grid => Instance.grid;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(World.IsPlaying) return;
        EditorApplication.update -= EditorUpdate;
        EditorApplication.update += EditorUpdate;
    }

    private void EditorUpdate()
    {
        if (initialShape)
        {
            InitBack();
            initialShape.transform.position = back.transform.position - (Vector3)4f.ToVector2();
            initialShape.transform.SetScale(defaultScale);
        }
    }
#endif
    
    private Dictionary<Block, List<(Vector2Int index, Block block)>> _GetSpecialBlocks(
        IEnumerable<Vector2Int> data)
    {
        var specialBlockPrefabs = new Dictionary<Block, List<(Vector2Int index, Block block)>>();
            
        foreach (var index in data)
        {
            var block = grid.Get(index);
            if(!block) continue;
            var prefab = block.prefab;
            if (this.specialBlockPrefabs.Contains(prefab))
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

    private void InitBack()
    {
        back.size = gridSize;
        defaultScale = new Vector3(8f / gridSize.x, 8f / gridSize.y, 1);
        back.transform.localScale = defaultScale;
        gridOffset = new Vector3(back.size.x / 2, back.size.y / 2) - LSVector3.half;
    }

    private void ApplyInitialShape()
    {
        if (initialShape)
        {
            if (initialShape.blocks.Count > 0)
            {
                for (int j = 0; j < initialShape.blocks.Count; j++)
                {
                    var block = initialShape.blocks[j];
                    var gridIndex = _ToIndex(block.transform.position);
                    grid.Set(gridIndex, block);
                    block.transform.SetParent(null, true);
                }
                
                InitialShapePlaced?.Invoke(initialShape);
                initialShape.transform.DetachChildren();
                Destroy(initialShape.gameObject);
            }
        }
    }

    public List<Shape> easyShapes;
    public List<Shape> mediumShapes;
    public List<Shape> hardShapes;
    
    private void Start()
    {
        grid = new Block[gridSize.x, gridSize.y]; 
        InitBack();
        InitTempShapes();
        ApplyInitialShape();
        CreateAndInitShape();

        dragger.Started += shape =>
        {
            DragStarted?.Invoke();
            ClearCurrentGhostShape();
            shape.transform.DOScale(defaultScale, 0.2f);
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
                float delayConst = 0.1f / shape.blocks.Count;
                for (int j = 0; j < shape.blocks.Count; j++)
                {
                    var gridIndex = gridIndices[j];
                    Vector2 worldPos = _ToPos(gridIndex);

                    var block = shape.blocks[j];
                    block.sortingOrder = -10;
                    grid.Set(gridIndex, block);
                    
                    var parent = new GameObject("BlockParent").transform;
                    parent.position = block.transform.position;
                    block.transform.SetParent(parent, true);
                    parent.DOMove(worldPos, 0.2f).SetDelay(j * delayConst).OnComplete(() =>
                    {
                        parent.DetachChildren();
                        Destroy(parent.gameObject);
                    });
                }

                activeShapes.Remove(shape);
                var placeData = new PlaceData();
                placeData.shape = shape;
                placeData.lastGrid = Internal_CopyGrid();
                
                if (GetFullLines())
                {
                    feel.Do();
                    BlocksDestroying?.Invoke(shape.BlockPrefab);
                    placeData.lines = new List<List<Vector2Int>>(_linesIndices);
                    _linesIndices.Clear();
                }

                placeData.currentGrid = grid;
                Placed?.Invoke(placeData);
                
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
                shape.transform.DOMove(dragger.currentSpawner.transform.position, 0.2f).SetEase(Ease.InOutExpo);
                shape.transform.DOScale(shapeSpawnerScale, 0.2f).SetEase(Ease.InOutExpo);
            }
        };
    }
    
    private void Update()
    {
        UpdateGhost();
    }
    
    protected override void DeInit()
    {
        base.DeInit();
        selectionAreas?.Clear();
#if UNITY_EDITOR
        EditorApplication.update -= EditorUpdate;
#endif
    }

    public static Block[,] CopyGrid() => Instance.Internal_CopyGrid();
    private Block[,] Internal_CopyGrid()
    {
        var size = grid.GetSize();
        var copiedGrid = new Block[size.x, size.y];
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                copiedGrid[x, y] = grid[x, y];
            }
        }
        return copiedGrid;
    }
    
    private List<List<Shape>> allTempShapes = new();
    
    private void InitTempShapes()
    {
        var tempShapesParent = new GameObject("TempShapesParent").transform;
        allTempShapes.Clear();
        AddShapes(hardShapes);
        AddShapes(mediumShapes);
        AddShapes(easyShapes);

        void AddShapes(List<Shape> shapes)
        {
            var tempList = new List<Shape>();
            allTempShapes.Add(tempList);
            foreach (var shape in shapes)
            {
                var temp = Instantiate(shape, new Vector3(30, 30, 0), Quaternion.identity, tempShapesParent);
                temp.BlockPrefab = FieldAppearance.BlockPrefabs[0];
                tempList.Add(temp);
            }
        }
    }

    private void _DeleteSpawners()
    {
        for (var i = 0; i < _spawners.Count; i++)
        {
            Destroy(_spawners[i].gameObject);
        }

        _spawners.Clear();
    }
    
    private async void CreateAndInitShape()
    {
        var lastGrid = grid;
        grid = Internal_CopyGrid();
        activeShapes.Clear();
        for (int i = 0; i < _spawners.Count; i++)
        {
            int shapeListIndex = 0;
            if (Random.value > 0.5f)
            {
                (allTempShapes[0], allTempShapes[1]) = (allTempShapes[1], allTempShapes[0]);
            }
            var tempShapes = new List<Shape>(allTempShapes[shapeListIndex]);
            
            Shape tempShape;
            do
            {
                tempShape = tempShapes.Random(out var index);
                if (await HasPlaceForShape(tempShape, true))
                {
                    SpawnShape();
                    if (GetFullLines())
                    {
                        FieldAnimator.Simulate();
                        _linesIndices.Clear();
                    }
                    
                    break;
                }
                tempShapes.RemoveAt(index);
                if (tempShapes.Count == 0)
                {
                    shapeListIndex++;
                    if (shapeListIndex >= allTempShapes.Count)
                    {
                        break;
                    }
                    tempShapes = new List<Shape>(allTempShapes[shapeListIndex]);
                }
            } while (true);
            
            void SpawnShape()
            {
                var shape = Instantiate(tempShape);
                activeShapes.Add(shape);
                shape.BlockPrefab = FieldAppearance.BlockPrefabs.Random();
                shape.transform.position = _spawners[i].transform.position;
                shape.transform.SetScale(shapeSpawnerScale);
                _spawners[i].currentShape = shape;
            }
        }

        for (int i = 0; i < activeShapes.Count; i++)
        {
            var shape = activeShapes[i];
            
            shape.transform.SetScale(0);
            shapeAppearFx.transform.SetScale(0);
            
            shape.transform.DOScale(shapeSpawnerScale, 0.2f).OnComplete(() =>
            {
                var appearFxInstance = Instantiate(shapeAppearFx, shape.transform.position, Quaternion.identity);
                appearFxInstance.transform.SetScale(0);
                appearFxInstance.transform.DOScale(4, 3f).KillOnDestroy(); 
            });
        }
        
        grid = lastGrid;
    }

    public Vector2Int _ToIndex(Vector2 pos)
    {
        var localPos = back.transform.InverseTransformPoint(pos);
        localPos += gridOffset;
        var gridIndex = new Vector2Int(Mathf.RoundToInt(localPos.x), Mathf.RoundToInt(localPos.y));
        return gridIndex;
    }
    
    public Vector2 _ToPos(Vector2Int index)
    {
        var pos = (Vector2)index;
        pos -= (Vector2)gridOffset;
        var localPos = back.transform.TransformPoint(pos);
        return localPos;
    }

    public static void PlaceBlock(Vector2Int index, Block prefab, out Block block) => Instance.Internal_PlaceBlock(index, prefab, out block);

    public void Internal_PlaceBlock(Vector2Int index, Block prefab, out Block block)
    {
        block = null;
        block = Block.Create(prefab);
        _PlaceBlock(index, block);
    }

    public void _PlaceBlock(Vector2Int index, Block block)
    {
        var existingBlock = grid.Get(index);
        block.transform.position = _ToPos(index);
        block.transform.SetScale(defaultScale);
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
            
            if (grid.HasIndex(gridIndex))
            {
                if (grid.Get(gridIndex) == null) continue;
            }

            canPlace = false;
        }

        return canPlace;
    }

    private async Task<bool> HasPlaceForShape(Shape shape, bool placeIfCan = false)
    {
        var prevScale = shape.transform.localScale;
        var prevPos = shape.transform.position;

        var result = await CheckCanPlace();

        if (result && placeIfCan)
        {
            for (int j = 0; j < shape.blocks.Count; j++)
            {
                var block = shape.blocks[j];
                var gridIndex = _ToIndex(block.transform.position);
                grid.Set(gridIndex, block);
            }
        }
        
        shape.transform.localScale = prevScale;
        shape.transform.position = prevPos;
        
        return result;
        
        async Task<bool> CheckCanPlace()
        {
            shape.transform.localScale = defaultScale;

            Vector2 r = shape.ratio;
            var tp = back.transform.position - Vector3.Scale(gridOffset + LSVector3.half, defaultScale) + Vector3.Scale(r, defaultScale) / 2;
            shape.transform.position = tp;

            int xCount = grid.GetLength(0) - (shape.ratio.x - 1);
            int yCount = grid.GetLength(1) - (shape.ratio.y - 1);

            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    shape.transform.position = tp + Vector3.Scale(new Vector3(x, y), defaultScale);
                    if (debug)
                    {
                        await Task.Delay(200);
                    }

                    if (Check())
                    {
                        return true;
                    }
                }
            }
        
            return false;
        }
        
        bool Check()
        {
            for (var i = 0; i < shape.blocks.Count; i++)
            {
                var block = shape.blocks[i];
                var gridIndex = _ToIndex(block.transform.position);

                if (!grid.HasIndex(gridIndex) || grid.Get(gridIndex))
                {
                    return false;
                }
            }

            return true;
        }
    }
    
    private void ClearCurrentGhostShape()
    {
        if (currentGhostShape == null) return;

        Destroy(currentGhostShape.gameObject);
        currentGhostShape = null;
        lastGhostBlockIndex.x = -1;
        SelectionAreas.ReleaseAll();
    }

    private void CreateGhostShape()
    {
        currentGhostShape = currentShape.CreateGhost(currentShape);
        currentGhostShape.transform.SetScale(defaultScale);
    }

    private async void CheckLoseCondition()
    {
        Debug.Log($"[CheckLoseCondition] Checking lose condition: {activeShapes.Count}");
        if (activeShapes.Count == 0) return;

        foreach (var shape in activeShapes)
        {
            if (shape == null) continue;
            if (await HasPlaceForShape(shape))
            {
                Debug.Log("[CheckLoseCondition] At least one shape can be placed");
                return;
            }
            Debug.Log("[CheckLoseCondition] Can't place shape, checking next");
        }

        Debug.Log("[CheckLoseCondition] All shapes blocked -> Game Over");
        LoseWindow.Show(Revive);
    }
    
    private void Revive()
    {
        Debug.Log("[Revive] called!");
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
        if(currentGhostShape == null) return;
        
        var gridIndices = new List<Vector2Int>();
        var canPlace = CanPlaceShape(currentShape, ref gridIndices);

        if (!canPlace)
        {
            SelectionAreas.ReleaseAll();
            lastGhostBlockIndex.x = -1;
            currentGhostShape.gameObject.SetActive(false);
            return;
        }

        if (lastGhostBlockIndex != gridIndices[0])
        {
            SelectionAreas.ReleaseAll();
            currentGhostShape.gameObject.SetActive(true);
            for (int i = 0; i < currentGhostShape.blocks.Count; i++)
            {
                var gridIndex = gridIndices[i];
                Vector2 worldPos = _ToPos(gridIndex);
                currentGhostShape.blocks[i].transform.position = worldPos;
            }
            HighlightDestroyableLines(gridIndices);
        }
        
        lastGhostBlockIndex = gridIndices[0];
    }

    private OnOffPool<SpriteRenderer> selectionAreas;
    private OnOffPool<SpriteRenderer> SelectionAreas => selectionAreas ??= OnOffPool<SpriteRenderer>.GetOrCreatePool(selector, back.transform, shouldStoreActive: true);

    private Vector2 selectionAreaSizeOffset = 0.7f.ToVector2();
    
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
            var area = SelectionAreas.Get();
            var index1 = list[0].index;
            var index2 = list[^1].index;
            var size = index2 - index1;
            if (size.x == 0)
            {
                area.transform.eulerAngles = 0f.ToVector3();
                area.transform.localScale = 1f.ToVector3();
            }
            else
            {
                (size.x, size.y) = (size.y, size.x);
                area.transform.eulerAngles = new Vector3(0, 0, -90);
                area.transform.localScale = new Vector3(-1, 1, 1);
            }
            area.size = size + Vector2Int.one;
            area.transform.localPosition = index1 - (Vector2)gridOffset - LSVector2.half - selectionAreaSizeOffset / 2;
            area.size += selectionAreaSizeOffset;
        }
    }

    private bool GetFullLines()
    {
        _linesIndices.Clear();
        
        var lines = GetBlockLines();
        
        foreach (var data in lines)
        {
            var lineIndices = new List<Vector2Int>(data.Count);
            _linesIndices.Add(lineIndices);
            for (var i = 0; i < data.Count; i++)
            {
                lineIndices.Add(data[i].index);
            }
        }
        return _linesIndices.Count > 0;
    }
    
    public static List<List<(Vector2Int index, Block block)>> GetBlockLines(bool excludeNull, bool excludeSpecial, bool unique = true) 
        => Instance.Internal_GetBlockLines(excludeNull, excludeSpecial, unique);
    
    private List<List<(Vector2Int index, Block block)>> Internal_GetBlockLines(bool excludeNull, bool excludeSpecial, bool unique = true)
    {
        var result = new List<List<(Vector2Int index, Block block)>>();
        var set = new HashSet<Vector2Int>();
        var buffer = new List<(Vector2Int index, Block block)>();
        for (var i = 0; i < _linesIndices.Count; i++)
        {
            var line = _linesIndices[i];
            buffer.Clear();
            for (var j = 0; j < line.Count; j++)
            {
                var index = line[j];
                var block = grid.Get(index);
                if (excludeNull && block == null) continue;
                if (excludeSpecial && specialBlockPrefabs.Contains(block.prefab)) continue;
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

    public static List<(Vector2Int index, Block block)>
        GetBlocks(bool excludeNull, bool excludeSpecial, bool unique = true) => Instance.Internal_GetBlocks(excludeNull, excludeSpecial, unique);
    
    private List<(Vector2Int index, Block block)> Internal_GetBlocks(bool excludeNull, bool excludeSpecial, bool unique = true)
    {
        var result = new List<(Vector2Int index, Block block)>();
        var lines = Internal_GetBlockLines(excludeNull, excludeSpecial, unique);
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

    public static HashSet<Block> GetDestroyedBlocks(Block[,] lastGrid, Block[,] currentGrid)
    {
        var size = lastGrid.GetSize();
        var lastSet = new HashSet<Block>();
        var currentSet = new HashSet<Block>();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                var index = new Vector2Int(x, y);
                var lastBlock = lastGrid.Get(index);
                if (lastBlock is not null)
                { 
                    lastSet.AddRange(lastBlock.AllBlocks);
                }
                var currentBlock = currentGrid.Get(index);
                if (currentBlock is not null)
                {
                    currentSet.AddRange(currentBlock.AllBlocks);
                }
            }
        }
        
        lastSet.ExceptWith(currentSet);
        return lastSet;
    }

    public static event Action<Block> BlocksDestroying;
    public static event Action<PlaceData> Placed;
    public static event Action<Shape> InitialShapePlaced;
    public static event Action DragStarted;
    
    public struct PlaceData
    {
        public Shape shape;
        public Block[,] lastGrid;
        public Block[,] currentGrid;
        public List<List<Vector2Int>> lines;
    }
}