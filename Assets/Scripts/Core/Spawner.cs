using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public List<Shape> shapes;
    public List<Block> blockPrefabs => FieldManager.BlockPrefabs;
    public List<Block> specialBlockPrefabs => FieldManager.SpecialBlockPrefabs;

    private void Awake()
    {
        FieldManager.Spawners.Add(this);
    }
    
    public Shape SpawnRandomShape(ref int? lastUsedIndex)
    {
        if (shapes.Count == 0 || blockPrefabs.Count == 0)
        {
            return null;
        }

        var shapePrefab = shapes.Random();
        var shapeInstance = Instantiate(shapePrefab, transform.position, Quaternion.identity);

        int newIndex;
        do
        {
            newIndex = Random.Range(0, blockPrefabs.Count);
        }
        while (blockPrefabs.Count > 1 && lastUsedIndex.HasValue && newIndex == lastUsedIndex.Value);

        var prefab = blockPrefabs[newIndex];
        shapeInstance.BlockPrefab = prefab;
        shapeInstance.SpawnSpecial(specialBlockPrefabs[0], 0);
        shapeInstance.SpawnSpecial(specialBlockPrefabs[1], 1);
        shapeInstance.SpawnSpecial(specialBlockPrefabs[0], 2);
        shapeInstance.SpawnSpecial(specialBlockPrefabs[1], 3);
        
        lastUsedIndex = newIndex;
        return shapeInstance;
    }
    
    public List<Shape> GetAllShapePrefabs()
    {
        return shapes;
    }

    public Shape SpawnSpecificShape(Shape shapePrefab, ref int? lastUsedIndex)
    {
        var shapeInstance = Instantiate(shapePrefab, transform.position, Quaternion.identity);

        int newIndex;
        do
        {
            newIndex = Random.Range(0, blockPrefabs.Count);
        }
        while (blockPrefabs.Count > 1 && lastUsedIndex.HasValue && newIndex == lastUsedIndex.Value);

        var prefab = blockPrefabs[newIndex];
        shapeInstance.BlockPrefab = prefab;
        
        lastUsedIndex = newIndex;
        return shapeInstance;
    }
}