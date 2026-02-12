using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SkillNameTrigger : BitStaticTrigger
	{
		private string _skill;

		private bool _noReverse = true;

		public SkillNameTrigger(string skill, TriggerType trigger)
			: base(trigger)
		{
			_skill = skill;
		}

		public SkillNameTrigger(string skill, BitStaticTrigger lowerTrigger = null, bool noReverse = true)
			: base(lowerTrigger)
		{
			_skill = skill;
			_noReverse = noReverse;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (_noReverse)
			{
				if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
				{
					return (monster as FieldMonster).CheckHasActionName(_skill);
				}
				return false;
			}
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return !(monster as FieldMonster).CheckHasActionName(_skill);
			}
			return false;
		}
	}
}
