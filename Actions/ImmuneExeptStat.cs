using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;

namespace BattlefieldScripts.Actions
{
	public class ImmuneExeptStat : PerkStat
	{
		private List<SkillType> _notImmuneTo;

		private SkillType _skill;

		public ImmuneExeptStat(List<SkillType> notImmuneTo, SkillType skill = SkillType.NoSkill)
		{
			_notImmuneTo = notImmuneTo;
			_skill = skill;
		}

		public override bool CheckSkill(SkillType skill)
		{
			return _notImmuneTo.Contains(skill);
		}

		public override bool CheckSide(FieldMonster affected, BitFilter requester)
		{
			if (_skill == SkillType.NoSkill)
			{
				return true;
			}
			if (_skill == SkillType.ImmuneToEnemy)
			{
				return affected.Side == requester.side;
			}
			if (_skill == SkillType.ImmuneToFriend)
			{
				return affected.Side != requester.side;
			}
			return true;
		}

		public override bool CheckBenefit(FieldElement affectedParameter, SkillType skill)
		{
			if (_skill == SkillType.NoSkill)
			{
				return true;
			}
			ActionBit actionBit = affectedParameter.Actions.Find((ActionBit x) => x.GetSignature().signature == skill);
			if (actionBit == null)
			{
				return false;
			}
			if (_skill == SkillType.ImmuneToPositive)
			{
				return !actionBit.GetSignature().isNegative;
			}
			if (_skill == SkillType.ImmuneToNegative)
			{
				return actionBit.GetSignature().isNegative;
			}
			return true;
		}
	}
}
