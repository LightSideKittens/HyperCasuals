using UnityEngine;

namespace Core
{
    public class BlockCountGoals : MonoBehaviour
    {
        public BlockCountGoal[] goals;
        
        private void Awake()
        {
            goals = GetComponentsInChildren<BlockCountGoal>();
            FieldManager.Placed += OnPlaced;
            Booster.Used += OnGridChanged;
        }
        
        private void OnDestroy()
        {
            FieldManager.Placed -= OnPlaced;
            Booster.Used -= OnGridChanged;
        }

        private void OnPlaced(FieldManager.PlaceData data)
        {
            OnGridChanged(data.lastGrid, data.currentGrid);
        }

        private void OnGridChanged(Block[,] lastGrid, Block[,] currentGrid)
        {
            var destroyedBlocksSet = FieldManager.GetDestroyedBlocks(lastGrid, currentGrid);
            foreach (var block in destroyedBlocksSet)
            {
                for (var i = 0; i < goals.Length; i++)
                {
                    goals[i].Check(block);
                }
            }
        }
    }
}