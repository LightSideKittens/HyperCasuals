using System.Collections.Generic;
using DG.Tweening;
using LSCore.Extensions.Unity;

namespace Core
{
    public class CreoFieldManager : FieldManager
    {
        public List<Spawner> spawners;
        private int spawnedBlockPrefabIndex = 0;
        
        protected override void CreateAndInitShape()
        {
            _spawners = spawners;
            activeShapes.Clear();
            var shapes = allTempShapes[0];
            if(shapes.Count == 0) return;
            var blockPrefabs = FieldAppearance.BlockPrefabs;
            
            for (int i = 0; i < _spawners.Count; i++)
            {
                var tempShape = shapes[0];
                shapes.RemoveAt(0);
                SpawnShape();
                
                void SpawnShape()
                {
                    var shape = Instantiate(tempShape);
                    activeShapes.Add(shape);
                    shape.BlockPrefab = blockPrefabs[spawnedBlockPrefabIndex++ % blockPrefabs.Count];
                    shape.transform.position = _spawners[i].transform.position;
                    shape.transform.SetScale(shapeSpawnerScale);
                    _spawners[i].currentShape = shape;
                }
            }
            
            for (int i = 0; i < activeShapes.Count; i++)
            {
                var shape = activeShapes[i];
                shape.transform.SetScale(0);
                shape.transform.DOScale(shapeSpawnerScale, 0.2f);
            }
        }
    }
}