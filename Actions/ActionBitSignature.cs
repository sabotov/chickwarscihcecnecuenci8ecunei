using System;
using NGUI.Scripts.Internal;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.Data_Helpers;

namespace BattlefieldScripts.Actions
{
	[Serializable]
	public struct ActionBitSignature
	{
		public SkillType signature;

		public TriggerType trigger;

		public string name;

		public string skillId;

		public int value;

		public string strValue;

		public bool levelSkill;

		public string addingSkillId;

		public bool isNegative;

		public bool ShouldPerformSilence
		{
			get
			{
				if (signature != SkillType.Silence)
				{
					return signature == SkillType.Cleanse;
				}
				return true;
			}
		}

		public bool ShouldPerformStatChangeAffected
		{
			get
			{
				if (signature != SkillType.Attack && signature != SkillType.Bleeding && signature != SkillType.Damage && signature != SkillType.Return && signature != SkillType.SetAttack && signature != SkillType.SetAttackTemp && signature != SkillType.SetHealth && signature != SkillType.SetHealthTemp && signature != SkillType.BuffAttack && signature != SkillType.BuffHealth && signature != SkillType.Heal && signature != SkillType.Pierce && signature != SkillType.SplashAttack && signature != SkillType.HealToMax && signature != SkillType.ClearHealToMax && signature != SkillType.ExtraAttack && signature != SkillType.SwapStats && signature != SkillType.Regen && signature != SkillType.Weakness && signature != SkillType.Silence && signature != SkillType.InstantKill && signature != SkillType.Vampiric)
				{
					if (signature == SkillType.AddSkill)
					{
						return addingSkillId == "appdivshield";
					}
					return false;
				}
				return true;
			}
		}

		public bool ShouldPerformStatChangeSelf
		{
			get
			{
				if (signature != SkillType.KillStealAttack && signature != SkillType.KillStealHealth && signature != SkillType.KillStealStats && signature != SkillType.KillStealAll && signature != SkillType.KillStealHpFromAttack && signature != SkillType.KillStealAttackFromHp && signature != SkillType.Vampiric)
				{
					return signature == SkillType.StealAttack;
				}
				return true;
			}
		}

		public ActionBitSignature Clone()
		{
			return new ActionBitSignature
			{
				signature = signature,
				trigger = trigger,
				name = name,
				skillId = skillId,
				value = value,
				strValue = strValue,
				levelSkill = levelSkill,
				isNegative = isNegative
			};
		}

		public ActionBitSignature(SkillType signature, TriggerType trigger, string name, string skillId, string addingSkillId = "", bool isNegative = false)
		{
			this.signature = signature;
			this.trigger = trigger;
			this.name = name;
			this.skillId = skillId;
			value = 0;
			strValue = "";
			levelSkill = false;
			this.addingSkillId = addingSkillId;
			this.isNegative = isNegative;
		}

		public static bool operator ==(ActionBitSignature s1, ActionBitSignature s2)
		{
			return s1.signature == s2.signature;
		}

		public static bool operator !=(ActionBitSignature s1, ActionBitSignature s2)
		{
			return s1.signature != s2.signature;
		}

		public bool ShouldStack(SkillStaticData skill, string sillVal)
		{
			bool flag = false;
			int num;
			if (signature == skill.skill)
			{
				num = ((trigger == skill.trigger) ? 1 : 0);
				if (num != 0)
				{
					SkillStaticData skillByName = SkillDataHelper.GetSkillByName(skillId);
					bool flag2 = skill.strId == skillByName.strId;
					int result = 0;
					int result2 = 0;
					flag = int.TryParse(strValue, out result) && int.TryParse(sillVal, out result2) && flag2;
				}
			}
			else
			{
				num = 0;
			}
			return (byte)((uint)num & (flag ? 1u : 0u)) != 0;
		}

		public string GetStackValue(string val)
		{
			int result = 0;
			int result2 = 0;
			int.TryParse(strValue, out result);
			int.TryParse(val, out result2);
			return (result + result2).ToString();
		}

		public TriggerType GetOnCompletedTrigger()
		{
			if (signature == SkillType.Attack || signature == SkillType.ExtraAttack)
			{
				return TriggerType.AttackPerformed;
			}
			return TriggerType.SkillUsed;
		}

		public TriggerType GetBroadcastSignal()
		{
			if (signature == SkillType.Attack || signature == SkillType.ExtraAttack)
			{
				return TriggerType.Attacked;
			}
			return TriggerType.NoTrigger;
		}

		public bool ShouldRecheckDeath()
		{
			if (signature == SkillType.Attack || signature == SkillType.ExtraAttack || signature == SkillType.Damage || signature == SkillType.Return || signature == SkillType.InstantKill || signature == SkillType.SwapStats || signature == SkillType.Vampiric || signature == SkillType.SummonRune)
			{
				return true;
			}
			return false;
		}

		public bool ShouldMirrorOnBattleField()
		{
			if (signature == SkillType.Immune || signature == SkillType.ClearImmune || signature == SkillType.ImmuneToEnemy || signature == SkillType.ImmuneToFriend || signature == SkillType.ImmuneToPositive || signature == SkillType.ImmuneToNegative)
			{
				return true;
			}
			return false;
		}

		public string GetName()
		{
			return Localization.Localize("#skill_" + name).Replace("%val%", string.Concat(value));
		}

		public string GetDescription()
		{
			return Localization.Localize("#skill_" + name + "_descr").Replace("%val%", string.Concat(value));
		}

		public int GetValueForAi(FieldMonster mon)
		{
			switch (signature)
			{
			case SkillType.Silence:
				return 1;
			case SkillType.AddSkill:
				return 1;
			case SkillType.AddSkillTemporary:
				return 1;
			case SkillType.SwapAttack:
				return 1;
			case SkillType.SwapHealth:
				return 1;
			case SkillType.Immune:
			case SkillType.ImmuneToFriend:
			case SkillType.ImmuneToEnemy:
			case SkillType.ImmuneToPositive:
			case SkillType.ImmuneToNegative:
				return 1;
			case SkillType.ClearImmune:
				return 1;
			case SkillType.Summon:
				return 1;
			case SkillType.Transform:
				return 1;
			case SkillType.InstantKill:
				return 1;
			case SkillType.ExtraAttack:
				return value * mon.Attack;
			case SkillType.ChangeWarlordSkill:
				return 1;
			case SkillType.DivineShield:
				return 1;
			case SkillType.RemoveSkill:
				return 1;
			case SkillType.RemoveSkillTemporary:
				return 1;
			default:
				return value;
			}
		}

		public int GetValueForAiRune()
		{
			switch (signature)
			{
			case SkillType.Silence:
				return 1;
			case SkillType.AddSkill:
				return 1;
			case SkillType.SwapAttack:
				return 1;
			case SkillType.SwapHealth:
				return 1;
			case SkillType.Immune:
				return 1;
			case SkillType.ClearImmune:
				return 1;
			case SkillType.Summon:
				return 1;
			case SkillType.Transform:
				return 1;
			case SkillType.InstantKill:
				return 1;
			case SkillType.ExtraAttack:
				return 1;
			case SkillType.ChangeWarlordSkill:
				return 1;
			case SkillType.DivineShield:
				return 1;
			default:
				return value;
			}
		}
	}
}
