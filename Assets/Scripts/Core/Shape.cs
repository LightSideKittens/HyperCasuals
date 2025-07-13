using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public List<Block> blocks = new();
    public Vector2Int ratio;
    private Block blockPrefab;

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
                var newBlock = Instantiate(value, currBlockTr.position, Quaternion.identity, transform);
                blocks[i] = newBlock;
                newBlock.transform.SetSiblingIndex(siblingIndex);
                Destroy(currBlockTr.gameObject);
            }
        }
    }

    [Button]
    public void AddBlocks()
    {
        blocks = new List<Block>(GetComponentsInChildren<Block>());
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
