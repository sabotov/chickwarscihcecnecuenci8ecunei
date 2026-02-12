using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class EnemyTrigger : BitStaticTrigger
	{
		public EnemyTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public EnemyTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				if (requester.parameters.GetWarlord(requester.side).Side == monster.Side)
				{
					return requester.parameters.GetMonsters(requester.enemySide).ContainsKey(position);
				}
				return true;
			}
			return false;
		}
	}
}
