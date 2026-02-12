using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;

namespace BattlefieldScripts.Actions
{
	public class ImmuneStat : PerkStat
	{
		private List<SkillType> _immuneToSkill;

		private SkillType _skill;

		public ImmuneStat(List<SkillType> immueToSkill, SkillType skill = SkillType.NoSkill)
		{
			_immuneToSkill = immueToSkill;
			_skill = skill;
		}

		public override bool CheckSkill(SkillType skill)
		{
			bool flag = false;
			for (int i = 0; i < _immuneToSkill.Count; i++)
			{
				if (_immuneToSkill[i] == skill)
				{
					flag = true;
					break;
				}
			}
			return !flag;
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
