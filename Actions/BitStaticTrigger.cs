using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class BitStaticTrigger
	{
		protected TriggerType _trigger;

		protected BitStaticTrigger _lowerTrigger;

		public BitStaticTrigger(TriggerType trigger)
		{
			_trigger = trigger;
		}

		public BitStaticTrigger(BitStaticTrigger lowerTrigger = null)
		{
			_lowerTrigger = lowerTrigger;
			if (_lowerTrigger != null)
			{
				_trigger = _lowerTrigger.GetTrigger();
			}
		}

		public TriggerType GetTrigger()
		{
			return _trigger;
		}

		public virtual bool CheckTrigger(TriggerType trigger, SkillType originSkill, Vector2 position, FieldElement monster, FieldElement affectedMonster, BitTrigger requester, object param = null)
		{
			if (_trigger != trigger)
			{
				return false;
			}
			if (_lowerTrigger != null)
			{
				return _lowerTrigger.CheckTrigger(trigger, originSkill, position, monster, affectedMonster, requester, param);
			}
			return true;
		}
	}
}
