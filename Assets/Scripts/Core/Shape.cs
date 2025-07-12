using System.Collections.Generic;
using UnityEngine;

public class Shape : MonoBehaviour
{
    public List<SpriteRenderer> blocks = new();
    public Vector2Int ratio;
    public Vector2 offset;
    private SpriteRenderer blockPrefab;

    public SpriteRenderer BlockPrefab
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

    public Shape CreateGhost(Sprite sprite)
    {
        var ghostObj = new GameObject("GhostShape");
        var ghost = ghostObj.AddComponent<Shape>();
        ghost.ratio = ratio;

        ghost.blocks = new List<SpriteRenderer>();

        foreach (var original in blocks)
        {
            var newBlockObj = new GameObject("GhostBlock");
            newBlockObj.transform.position = original.transform.position;
            newBlockObj.transform.SetParent(ghostObj.transform);

            var sr = newBlockObj.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = new Color(1f, 1f, 1f, 0.4f);
            sr.sortingOrder = -1;

            ghost.blocks.Add(sr);
        }

        return ghost;
    }
}
