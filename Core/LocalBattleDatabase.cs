using System;
using System.Collections.Generic;
using UnityEngine;

namespace LocalBattleSystem.Core
{
    public class LocalBattleDatabase : MonoBehaviour
    {
        [SerializeField] private TextAsset battleDataJson;

        private readonly Dictionary<string, MonsterData> _monsters = new Dictionary<string, MonsterData>();
        private readonly Dictionary<string, SkillData> _skills = new Dictionary<string, SkillData>();

        public bool IsLoaded { get; private set; }

        private void Awake()
        {
            Load();
        }

        public void Load()
        {
            _monsters.Clear();
            _skills.Clear();

            if (battleDataJson == null)
            {
                Debug.LogError("LocalBattleDatabase: battleDataJson is not assigned.");
                IsLoaded = false;
                return;
            }

            BattleDataRoot data;
            try
            {
                data = JsonUtility.FromJson<BattleDataRoot>(battleDataJson.text);
            }
            catch (Exception ex)
            {
                Debug.LogError($"LocalBattleDatabase: failed to parse JSON: {ex.Message}");
                IsLoaded = false;
                return;
            }

            if (data == null)
            {
                Debug.LogError("LocalBattleDatabase: JSON parsed as null.");
                IsLoaded = false;
                return;
            }

            foreach (var monster in data.monsters)
            {
                if (!string.IsNullOrWhiteSpace(monster.id))
                {
                    _monsters[monster.id] = monster;
                }
            }

            foreach (var skill in data.skills)
            {
                if (!string.IsNullOrWhiteSpace(skill.id))
                {
                    _skills[skill.id] = skill;
                }
            }

            IsLoaded = true;
            Debug.Log($"LocalBattleDatabase: loaded {_monsters.Count} monsters and {_skills.Count} skills.");
        }

        public RuntimeMonster CreateRuntimeMonster(string monsterId)
        {
            if (!_monsters.TryGetValue(monsterId, out var monster))
            {
                return null;
            }

            return new RuntimeMonster(monster);
        }

        public SkillData GetSkill(string skillId)
        {
            _skills.TryGetValue(skillId, out var skill);
            return skill;
        }
    }
}
