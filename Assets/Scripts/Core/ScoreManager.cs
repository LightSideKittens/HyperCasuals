using System;
using System.Collections.Generic;
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
            FieldManager.Placed += OnPlace;
        }

        protected override void DeInit()
        {
            base.DeInit();
            FieldManager.Placed -= OnPlace;
        }

        public void OnPlace(FieldManager.PlaceData data)
        {
            if (bonuses.Count == 0)
            { 
                turnsForBonus++;
            }
            
            DecreaseBonuses();
            if (turnsForBonus >= bonusizeEveryTurns)
            {
                turnsForBonus = 0;
                Bonusize();
            }
            
            var placedScore = 0;
            foreach (var block in data.shape.blocks)
            {
                if(block.ContainsRegular) placedScore += forBlockPlacing;
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
                return;
            }
            
            currentTurn = 0;
            var destroyedBlocks = 0;
            var bonusScore = 0;
            
            var size = data.lastGrid.GetSize();
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    var index = new Vector2Int(x, y);
                    var lastBlock = data.lastGrid.Get(index);
                    if(lastBlock is null) continue;
                    var currentBlock = data.currentGrid.Get(index);
                    if (currentBlock is null)
                    {
                        if (bonuses.Remove(lastBlock, out var bonus))
                        {
                            bonusScore += forBonusBlockDestroying * int.Parse(bonus.text);
                            Destroy(bonus.gameObject);
                        }
                        
                        if (lastBlock.ContainsRegular)
                        {
                            destroyedBlocks++;
                        }
                    }
                }
            }

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
            foreach (var (block, text) in bonuses)
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
                var bonus = Instantiate(bonusPrefab, block.transform.position, Quaternion.identity);
                bonus.text = Random.Range(bonusLevelRange.x, bonusLevelRange.y).ToString();
                bonuses[block] = bonus;
            }
        }
    }
}