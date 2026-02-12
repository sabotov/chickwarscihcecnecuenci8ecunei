using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SelfWoundedTrigger : BitStaticTrigger
	{
		public SelfWoundedTrigger(TriggerType trigger)
			: base(trigger)
		{
		}

		public SelfWoundedTrigger(BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			bool flag = requester.parameters.GetWarlord(requester.side).coords == requester.placeDelegate() && (int)requester.parameters.GetWarlord(requester.side).Health < (int)requester.parameters.GetWarlord(requester.side).MaxHealth;
			bool flag2 = requester.parameters.GetMonsters(requester.side).ContainsKey(requester.placeDelegate()) && (int)requester.parameters.GetMonsters(requester.side)[requester.placeDelegate()].Health < (int)requester.parameters.GetMonsters(requester.side)[requester.placeDelegate()].MaxHealth;
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return flag || flag2;
			}
			return false;
		}
	}
}
