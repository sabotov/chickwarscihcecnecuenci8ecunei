using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SkillUsedTrigger : BitStaticTrigger
	{
		private SkillType _skill;

		public SkillUsedTrigger(SkillType skill, TriggerType trigger)
			: base(trigger)
		{
			_skill = skill;
		}

		public SkillUsedTrigger(SkillType skill, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			_skill = skill;
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param))
			{
				return CheckSkillCondition(originSkill, _skill);
			}
			return false;
		}

		private bool CheckSkillCondition(SkillType origin, SkillType skill)
		{
			if (origin == skill)
			{
				return true;
			}
			if (origin == SkillType.Heal || origin == SkillType.HealToMax || origin == SkillType.ClearHealToMax)
			{
				if (skill != SkillType.Heal && skill != SkillType.HealToMax)
				{
					return skill == SkillType.ClearHealToMax;
				}
				return true;
			}
			return false;
		}
	}
}
