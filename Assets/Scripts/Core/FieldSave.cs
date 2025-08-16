using System.Collections.Generic;
using System.IO;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Core
{
    public class FieldConfigManager : JTokenGameConfigManager
    {
#if UNITY_EDITOR
        public override void Load()
        {
            base.Load();
            if (!FieldSave.IsEnabled)
            { 
                cached.data = new JObject();
            }
        }

        public override void Save()
        {
            if (FieldSave.IsEnabled)
            { 
                base.Save();
            }
        }
#endif
    }
    
    public static partial class FieldSave
    {
        public static FieldConfigManager Manager => ConfigMaster<FieldConfigManager>.Get(Path.Combine("FieldSaves", GameSave.currentLevel ?? "unknown"));
        
        public static JToken Config => Manager.Config.data;
        
        public static bool Exists
        {
            get
            {
#if UNITY_EDITOR
                if (!IsEnabled) return false;
#endif
                return Manager.Exists;
            }
        }

        public static void Save() => Manager.Save();
        public static void Delete() => Manager.Delete();
        
        public static JArray GradeBlocks => Config.AsJ<JArray>("gradeBlocks");
        public static void SaveGradeBlocks(Dictionary<Vector2Int, int> stages)
        {
            var jGradeBlocks = GradeBlocks;
            jGradeBlocks.Clear();
            foreach (var (index, stage) in stages)
            {
                jGradeBlocks.Add(new JObject
                {
                    {"index", index.ToJObject()},
                    {"stage", stage}
                });
            }
        }
        
        public static JArray Shapes => Config.AsJ<JArray>("shapes");

        public static void SaveShapes(List<(int listIndex, int shapeIndex, string blockPrefab)> shapes)
        {
            var jShapes = Shapes;
            jShapes.Clear();
            foreach (var (listIndex, shapeIndex, blockPrefab) in shapes)
            {
                jShapes.Add(new JObject
                {
                    {"listIndex", listIndex},
                    {"shapeIndex", shapeIndex},
                    {"blockPrefab", blockPrefab}
                });
            }
        }

        public static JArray Bonuses => Config.AsJ<JArray>("bonuses");

        public static void SaveBonusBlocks(Dictionary<Block, BonusBlock> bonuses)
        {
            var jBonuses = Bonuses;
            jBonuses.Clear();
            foreach (var (block, bonus) in bonuses)
            {
                var index = FieldManager.ToIndex(block.transform.position);
                jBonuses.Add(new JObject
                {
                    {"index", index.ToJObject()},
                    {"bonus", bonus.Value}
                });
            }
        }
        
        public static JArray Blocks => Config.AsJ<JArray>("blocks");
        
        public static void SaveField(Block[,] blocks)
        {
            var jBlocks = Blocks;
            jBlocks.Clear();
            var size = blocks.GetSize();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var index = new Vector2Int(x, y);
                    var block = blocks.Get(index);
                    if(!block) continue;
                    var jObj = new JObject
                    {
                        { "index", index.ToJObject() },
                        { "block", block.id.ToString() }
                    };
                    
                    jBlocks.Add(jObj);
                    
                    while (block.next)
                    {
                        block = block.next;
                        var nextJObj = new JObject
                        {
                            {"block", block.id.ToString()}
                        };
                        jObj["next"] = nextJObj;
                        jObj = nextJObj;
                    }
                }
            }
        }
        
        public static bool gridDirtied;
        public static void Set(this Block[,] array, Vector2Int index, Block value)
        {
            array[index.x, index.y] = value;
            gridDirtied = true;
        }
    }
}