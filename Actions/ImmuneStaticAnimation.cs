using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class ImmuneStaticAnimation : StaticAnimationBit
	{
		private int _turns;

		private TriggerType _trigger;

		public void Init(FieldMonsterVisual monster, int turns, TriggerType trigger = TriggerType.TurnEnded, bool shouldDeleteAnim = true)
		{
			Init(monster, shouldDeleteAnim);
			_turns = turns;
			_trigger = trigger;
		}

		public override void InformTrigger(TriggerType trigger)
		{
			if (trigger == _trigger || trigger == TriggerType.ClearParams)
			{
				_turns--;
				if (_turns == 0 || trigger == TriggerType.ClearParams)
				{
					InformCleanse();
				}
			}
		}

		public override void InformCleanse()
		{
			bool flag = true;
			foreach (StaticAnimationBit staticAnimation in _curMonster.StaticAnimations)
			{
				if (staticAnimation != this && staticAnimation is ImmuneStaticAnimation)
				{
					flag = false;
				}
			}
			_curMonster.DeattachStaticAnimation(this);
			Object.Destroy(base.gameObject);
		}
	}
}
