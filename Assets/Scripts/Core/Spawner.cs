using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public List<Shape> shapes;
    public List<SpriteRenderer> blockPrefabs;

    public Shape SpawnRandomShape(ref int? lastUsedIndex)
    {
        if (shapes.Count == 0 || blockPrefabs.Count == 0)
        {
            return null;
        }

        var shapePrefab = shapes[Random.Range(0, shapes.Count)];
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