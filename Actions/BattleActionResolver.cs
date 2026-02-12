using System.Collections.Generic;
using LocalBattleSystem.AI;
using LocalBattleSystem.Core;

namespace LocalBattleSystem.Actions
{
    public static class BattleActionResolver
    {
        public static BattleResult RunOneVsOne(LocalBattleDatabase database, RuntimeMonster left, RuntimeMonster right, int maxRounds = 50)
        {
            var result = new BattleResult();
            var leftCooldowns = new Dictionary<string, int>();
            var rightCooldowns = new Dictionary<string, int>();

            for (var round = 1; round <= maxRounds; round++)
            {
                result.rounds = round;

                ExecuteAttack(database, left, right, leftCooldowns, result, "P1");
                if (right.IsDead)
                {
                    result.winnerId = left.Source.id;
                    result.log.Add($"Round {round}: {right.Source.name} defeated.");
                    return result;
                }

                ExecuteAttack(database, right, left, rightCooldowns, result, "AI");
                if (left.IsDead)
                {
                    result.winnerId = right.Source.id;
                    result.log.Add($"Round {round}: {left.Source.name} defeated.");
                    return result;
                }

                TickCooldowns(leftCooldowns);
                TickCooldowns(rightCooldowns);
            }

            result.winnerId = left.CurrentHp >= right.CurrentHp ? left.Source.id : right.Source.id;
            result.log.Add("Max rounds reached. Winner selected by remaining HP.");
            return result;
        }

        private static void ExecuteAttack(
            LocalBattleDatabase database,
            RuntimeMonster attacker,
            RuntimeMonster defender,
            Dictionary<string, int> cooldowns,
            BattleResult result,
            string sideLabel)
        {
            var selectedSkill = SimpleAiController.ChooseSkill(database, attacker.Source, cooldowns);
            var skillPower = selectedSkill != null ? selectedSkill.power : 0;

            var rawDamage = attacker.Source.attack + skillPower - defender.Source.defense;
            var finalDamage = rawDamage < 1 ? 1 : rawDamage;

            defender.ReceiveDamage(finalDamage);

            if (selectedSkill != null)
            {
                cooldowns[selectedSkill.id] = selectedSkill.cooldown;
                result.log.Add($"{sideLabel}: {attacker.Source.name} used {selectedSkill.name} for {finalDamage} dmg. {defender.Source.name} HP={defender.CurrentHp}");
            }
            else
            {
                result.log.Add($"{sideLabel}: {attacker.Source.name} basic attack for {finalDamage} dmg. {defender.Source.name} HP={defender.CurrentHp}");
            }
        }

        private static void TickCooldowns(Dictionary<string, int> cooldowns)
        {
            var keys = new List<string>(cooldowns.Keys);
            foreach (var key in keys)
            {
                cooldowns[key]--;
                if (cooldowns[key] <= 0)
                {
                    cooldowns.Remove(key);
                }
            }
        }
    }
}
