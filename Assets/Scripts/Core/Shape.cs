using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class Shape : MonoBehaviour
{
    public List<Block> blocks = new();
    public Vector2Int ratio;
    private Block blockPrefab;
    private BoxCollider2D boxCollider;

    public int MaxSide
    {
        get
        {
            var x = ratio.x; var y = ratio.y;
            if (x > y)
            {
                return x;
            }
            
            return y;
        }
    }

    public Block BlockPrefab
    {
        get => blockPrefab;
        set
        {
            blockPrefab = value;
            for (int i = 0; i < blocks.Count; i++)
            {
                var currBlock = blocks[i];
                var currBlockTr = currBlock.transform;
                var siblingIndex = currBlockTr.GetSiblingIndex();
                var newBlock = Block.Create(value, currBlockTr.position, Quaternion.identity, transform);
                newBlock.defaultSortingOrder = -10;
                blocks[i] = newBlock;
                newBlock.transform.SetSiblingIndex(siblingIndex);
                Destroy(currBlockTr.gameObject);
            }
        }
    }

    public Block RandomSpawnSpecial(Block specialBlockPrefab)
    {
        return SpawnSpecial(specialBlockPrefab, Random.Range(0, blocks.Count));
    }
    
    public Block SpawnSpecial(Block specialBlockPrefab, int atIndex)
    {
        var currBlock = blocks[atIndex];
        var currBlockTr = currBlock.transform;
        var newBlock = Block.Create(specialBlockPrefab, currBlockTr.position, specialBlockPrefab.transform.rotation, currBlockTr.parent);
        OverlayBlock(currBlock, newBlock);
        blocks[atIndex] = newBlock;
        return newBlock;
    }

    public static void OverlayBlock(Block existingBlock, Block newBlock)
    {
        newBlock.defaultSortingOrder = existingBlock.sortingOrder+1;
        newBlock.sortingOrder = newBlock.defaultSortingOrder;
        existingBlock.transform.SetParent(newBlock.transform, true);
        newBlock.next = existingBlock;
    }

    public Shape CreateGhost(Shape shape)
    {
        var ghost = Instantiate(shape, shape.transform.position, Quaternion.identity, shape.transform.parent);
        foreach (var block in ghost.blocks)
        {
            block.color = new Color(1f, 1f, 1f, 0.4f);
            block.sortingOrder = -1;
        }

        return ghost;
    }
}
