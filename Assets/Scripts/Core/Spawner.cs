using System;
using System.Collections.Generic;
using LSCore.Extensions;
using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    public List<Shape> shapes;
    [NonSerialized] public Shape currentShape;
    public List<Block> blockPrefabs => FieldAppearance.BlockPrefabs;
    public List<Block> specialBlockPrefabs => FieldAppearance.SpecialBlockPrefabs;


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