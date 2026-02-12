using System;
using NewAssets.Scripts.DataClasses;

namespace BattlefieldScripts.Actions
{
	[Serializable]
	public class PerkBit
	{
		public BitActionAnimation onAttachedAnimation;

		public BitActionAnimation onWorkedAnimation;

		private ActionBitSignature _signature;

		protected FieldMonster _curMonster;

		protected PerkStat _stat;

		public PerkBit(ActionBitSignature signature, PerkStat stat)
		{
			_signature = signature;
			_stat = stat;
		}

		public void Init(FieldMonster curMonster)
		{
			_curMonster = curMonster;
			if (onWorkedAnimation != null)
			{
				onWorkedAnimation.Init(curMonster.VisualMonster);
			}
		}

		public ActionBitSignature GetSignature()
		{
			return _signature;
		}

		public int ModulateInt(int val)
		{
			return _stat.ValidateInt(val);
		}

		public bool ModulateBool(bool val)
		{
			return _stat.ValidateBool(val);
		}

		public bool CheckSkill(SkillType skill)
		{
			return _stat.CheckSkill(skill);
		}

		public bool CheckSide(FieldMonster affected, BitFilter requester)
		{
			return _stat.CheckSide(affected, requester);
		}

		public bool CheckBenefit(FieldElement requester, SkillType skill)
		{
			return _stat.CheckBenefit(requester, skill);
		}
	}
}
