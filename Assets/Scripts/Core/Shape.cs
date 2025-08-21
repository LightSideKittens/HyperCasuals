using System;
using System.Collections.Generic;
using LSCore.Extensions.Unity;
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
                blocks[i] = newBlock;
                newBlock.transform.SetSiblingIndex(siblingIndex);
                currBlockTr.SetParent(null, true);
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
        existingBlock.transform.SetParent(newBlock.transform, true);
        newBlock.next = existingBlock;
        newBlock.sortingOrder = Block.DefaultSortingOrder;
    }
    
    public static void AddNext(Block existingBlock, Block nextBlock)
    {
        existingBlock.next = nextBlock;
        nextBlock.transform.SetParent(existingBlock.transform, true);
        existingBlock.sortingOrder = Block.DefaultSortingOrder;
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

    private Shape shadowShape;
    private List<Block> shadowBlocks;

    private void OnDestroy()
    {
        if (shadowShape)
        { 
            Destroy(shadowShape.gameObject);
        }
    }

    public bool ShadowEnabled
    {
        set 
        {
            if (shadowShape == null)
            {
                shadowBlocks = new List<Block>();
                shadowShape = Instantiate(this, transform.position - new Vector3(0, 0.025f), Quaternion.identity);
                for (int i = 0; i < shadowShape.blocks.Count; i++)
                {
                    var block = shadowShape.blocks[i].GetRegular();
                    if (block)
                    {
                        block.render.sortingOrder = -30;
                        block.color = Color.black;
                        block.transform.localScale = new Vector3(1.15f, 1.25f, 1);
                        shadowBlocks.Add(block);
                    }
                }
            }
            
            shadowShape.gameObject.SetActive(value);
        }
    }
}
