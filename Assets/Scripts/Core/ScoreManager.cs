using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
using LSCore.Extensions;
using Sirenix.OdinInspector;
using SourceGenerators;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Core
{
    [InstanceProxy]
    public partial class ScoreManager : SingleService<ScoreManager>
    {
        public int forLineDestroying = 20;
        public int forCombo = 10;
        public int forBlockPlacing = 5;
        public int forBonusBlockDestroying = 20;
        public int forBlockDestroying = 5;
        public int turnsForComboReset = 3;
        public int bonusizeEveryTurns = 9;
        public int maxBonusBlock = 3;
        
        [MinMaxSlider(0, 12)] public Vector2Int bonusLevelRange;
        public TextMeshPro bonusPrefab;
        private Dictionary<Block, TextMeshPro> bonuses = new();
        
        private int _lastScore;
        private int _currentScore;
        private int _currentCombo;
        private int currentTurn;
        private int turnsForBonus;

        public static event Action ScoreChanged;
        public static event Action ComboChanged;
        
        protected override void Init()
        {
            base.Init();
            FieldManager.Placed += OnPlaced;
            FieldManager.InitialShapePlaced += OnInitialShapePlaced;
            Booster.Used += OnBoosterUsed;
        }

        protected override void DeInit()
        {
            base.DeInit();
            FieldManager.Placed -= OnPlaced;
            FieldManager.InitialShapePlaced -= OnInitialShapePlaced;
            Booster.Used -= OnBoosterUsed;
        }

        private void OnInitialShapePlaced(Shape shape)
        {
            for (var i = 0; i < shape.blocks.Count; i++)
            {
                var block = shape.blocks[i];
                var bonus = block.GetComponentInChildren<TextMeshPro>();
                if (bonus)
                {
                    var regular = block.GetRegular();
                    if (regular)
                    {
                        bonuses[regular] = bonus;
                        bonus.transform.SetParent(null, true);
                    }
                }
            }
        }

        private void OnPlaced(FieldManager.PlaceData data)
        {
            if (bonuses.Count == 0)
            { 
                turnsForBonus++;
            }
            
            if (turnsForBonus >= bonusizeEveryTurns)
            {
                turnsForBonus = 0;
                Bonusize();
            }
            
            var placedScore = 0;
            foreach (var block in data.shape.blocks)
            {
                if(block.GetRegular()) placedScore += forBlockPlacing;
            }
            
            int linesCount = data.lines?.Count ?? 0;
            if (linesCount == 0)
            {
                currentTurn++;
                if (currentTurn >= turnsForComboReset)
                {
                    _currentCombo = 0;
                    currentTurn = 0;
                }
                _lastScore = _currentScore;
                _currentScore += placedScore;
                ScoreChanged?.Invoke();
                DecreaseBonuses();
                return;
            }
            
            currentTurn = 0;
            var (destroyedBlocks, bonusScore) = CountDestroyedBlocksScore(data.lastGrid, data.currentGrid);
            _lastScore = _currentScore;
            _currentScore += linesCount * (forLineDestroying + _currentCombo * forCombo) 
                             + (_currentCombo + 1) * forBlockDestroying * destroyedBlocks
                             + bonusScore
                             + placedScore;
            
            _currentCombo++;
            ScoreChanged?.Invoke();
            if (_currentCombo > 1)
            { 
                ComboChanged?.Invoke();
            }
            DecreaseBonuses();
        }

        private (int destroyedBlocks, int bonusScore) CountDestroyedBlocksScore(Block[,] lastGrid, Block[,] currentGrid)
        {
            var destroyedBlocks = 0;
            var bonusScore = 0;
            
            var destroyedBlocksSet = FieldManager.GetDestroyedBlocks(lastGrid, currentGrid);
            foreach (var block in destroyedBlocksSet)
            {
                if (!block.IsSpecial)
                {
                    if (bonuses.Remove(block, out var bonus))
                    {
                        bonusScore += forBonusBlockDestroying * int.Parse(bonus.text);
                        Destroy(bonus.gameObject);
                    }
                    
                    destroyedBlocks++;
                }
            }
            
            return (destroyedBlocks, bonusScore);
        }
        
        private void OnBoosterUsed(Block[,] lastGrid, Block[,] currentGrid)
        {
            var (destroyedBlocks, bonusScore) = CountDestroyedBlocksScore(lastGrid, currentGrid);
            _lastScore = _currentScore;
            _currentScore += forBlockDestroying * destroyedBlocks
                             + bonusScore;
            if (_lastScore != _currentScore)
            { 
                ScoreChanged?.Invoke();
            }
        }
        
        private void Update()
        {
            foreach (var (block, text) in bonuses)
            {
                text.transform.position = block.transform.position;
            }
        }

        private void DecreaseBonuses()
        {
            var list = bonuses.ToList();
            foreach (var (block, text) in list)
            {
                var level = int.Parse(text.text);
                text.text = (level - 1).ToString();
                if (level == 1)
                {
                    Destroy(text.gameObject);
                    bonuses.Remove(block);
                }
            }
        }
        
        private void Bonusize()
        {
            if(bonuses.Count > 0) return;
            
            var set = new HashSet<Vector2Int>();
            var size = FieldManager.Grid.GetSize();
            for (int i = 0; i < maxBonusBlock; i++)
            {
                var index = FieldManager.Grid.RandomIndex();
                while (!set.Add(index))
                {
                    if (index.x >= size.x)
                    {
                        index.x = 0;
                        index.y++;
                        index.y %= size.y;
                    }
                    index.x++;
                }
                var block = FieldManager.Grid.Get(index);
                if(block == null) continue;
                block = block.GetRegular();
                var bonus = Instantiate(bonusPrefab, block.transform.position, Quaternion.identity);
                bonus.text = Random.Range(bonusLevelRange.x, bonusLevelRange.y).ToString();
                bonuses[block] = bonus;
            }
        }
    }
}