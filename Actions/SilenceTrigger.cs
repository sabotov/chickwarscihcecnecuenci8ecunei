using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SilenceTrigger : BitStaticTrigger
	{
		private string _paramType;

		private SkillType _skill;

		private string _skillId;

		public SilenceTrigger(string paramType, string paramValue, BitStaticTrigger lowerTrigger = null)
			: base(lowerTrigger)
		{
			Init(paramType, paramValue);
		}

		public SilenceTrigger(string paramType, string paramValue, TriggerType trigger)
			: base(trigger)
		{
			Init(paramType, paramValue);
		}

		private void Init(string paramType, string paramValue)
		{
			_paramType = paramType;
			if (!(paramType == "silence_skill_type"))
			{
				if (paramType == "silence_skill_id")
				{
					_skill = SkillType.NoSkill;
					_skillId = paramValue;
				}
			}
			else
			{
				_skill = SkillStaticData.GetSkillByString(paramValue);
				_skillId = "";
			}
		}

		private bool CheckMonster(FieldMonster mosnter, object param)
		{
			if (!(param is SilenceData silenceData))
			{
				return false;
			}
			string paramType = _paramType;
			if (!(paramType == "silence_skill_type"))
			{
				if (paramType == "silence_skill_id")
				{
					return silenceData.silencedSkills.Find((ActionBitSignature x) => x.skillId == _skillId) != default(ActionBitSignature);
				}
				return false;
			}
			return silenceData.silencedSkills.Find((ActionBitSignature x) => x.signature == _skill) != default(ActionBitSignature);
		}

		public override bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (base.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param) && monster is FieldMonster)
			{
				return CheckMonster(monster as FieldMonster, param);
			}
			return false;
		}
	}
}
