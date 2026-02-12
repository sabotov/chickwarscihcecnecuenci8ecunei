using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class TriggerOwnTrigger : BitStaticTrigger
	{
		private TriggerType _checkTrigger;

		public TriggerOwnTrigger(TriggerType checkTrigger, TriggerType trigger)
			: base(trigger)
		{
			_checkTrigger = checkTrigger;
		}

		public TriggerOwnTrigger(TriggerType checkTrigger, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			_checkTrigger = checkTrigger;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return (monster as FieldMonster).CheckHasTrigger(_checkTrigger);
			}
			return false;
		}
	}
}
