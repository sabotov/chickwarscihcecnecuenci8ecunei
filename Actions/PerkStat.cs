using NewAssets.Scripts.DataClasses;

namespace BattlefieldScripts.Actions
{
	public class PerkStat
	{
		public virtual bool ValidateBool(bool val)
		{
			return val;
		}

		public virtual int ValidateInt(int val)
		{
			return val;
		}

		public virtual bool CheckSkill(SkillType skill)
		{
			return true;
		}

		public virtual bool CheckSide(FieldMonster affected, BitFilter requester)
		{
			return true;
		}

		public virtual bool CheckBenefit(FieldElement affected, SkillType skill)
		{
			return true;
		}
	}
}
