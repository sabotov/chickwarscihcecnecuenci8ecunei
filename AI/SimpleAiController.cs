using System.Collections.Generic;
using LocalBattleSystem.Core;

namespace LocalBattleSystem.AI
{
    public static class SimpleAiController
    {
        public static SkillData ChooseSkill(LocalBattleDatabase database, MonsterData monster, Dictionary<string, int> cooldowns)
        {
            SkillData best = null;

            foreach (var skillId in monster.skillIds)
            {
                if (cooldowns.ContainsKey(skillId))
                {
                    continue;
                }

                var skill = database.GetSkill(skillId);
                if (skill == null)
                {
                    continue;
                }

                if (best == null || skill.power > best.power)
                {
                    best = skill;
                }
            }

            return best;
        }
    }
}
