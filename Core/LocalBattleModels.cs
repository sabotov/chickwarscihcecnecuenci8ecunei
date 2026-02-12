using System;
using System.Collections.Generic;

namespace LocalBattleSystem.Core
{
    [Serializable]
    public class BattleDataRoot
    {
        public List<MonsterData> monsters = new List<MonsterData>();
        public List<SkillData> skills = new List<SkillData>();
    }

    [Serializable]
    public class MonsterData
    {
        public string id;
        public string name;
        public int maxHp;
        public int attack;
        public int defense;
        public List<string> skillIds = new List<string>();
    }

    [Serializable]
    public class SkillData
    {
        public string id;
        public string name;
        public int power;
        public int cooldown;
    }

    public class RuntimeMonster
    {
        public RuntimeMonster(MonsterData source)
        {
            Source = source;
            CurrentHp = source.maxHp;
        }

        public MonsterData Source { get; }
        public int CurrentHp { get; private set; }

        public bool IsDead => CurrentHp <= 0;

        public void ReceiveDamage(int damage)
        {
            CurrentHp -= Math.Max(damage, 0);
            if (CurrentHp < 0)
            {
                CurrentHp = 0;
            }
        }
    }

    public class BattleResult
    {
        public string winnerId;
        public int rounds;
        public List<string> log = new List<string>();
    }
}
