using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class FreezeStaticAnimation : StaticAnimationBit
	{
		private int _turns;

		private TriggerType _trigger;

		public void Init(FieldMonsterVisual monster, int turns, TriggerType trigger = TriggerType.TurnEnded, bool isGold = false)
		{
			Init(monster);
			if (!monster.Image.Ready)
			{
				Initializer.WaitForCondition(() => monster.Image.Ready, delegate
				{
					if (isGold)
					{
						monster.Image.AnimateGold();
					}
					else
					{
						monster.Image.AnimateFrozen();
					}
				});
			}
			else if (isGold)
			{
				monster.Image.AnimateGold();
			}
			else
			{
				monster.Image.AnimateFrozen();
			}
			_turns = turns;
			_trigger = trigger;
		}

		public override void InformTrigger(TriggerType trigger)
		{
			if (trigger != _trigger && trigger != TriggerType.ClearParams)
			{
				return;
			}
			_turns--;
			if (_turns != 0 && trigger != TriggerType.ClearParams)
			{
				return;
			}
			bool flag = true;
			foreach (StaticAnimationBit staticAnimation in _curMonster.StaticAnimations)
			{
				if (staticAnimation != this && staticAnimation is FreezeStaticAnimation)
				{
					flag = false;
				}
			}
			if (flag && !_curMonster.Data.isGoldMineMonster)
			{
				_curMonster.Image.AnimateUnfrozen();
			}
			_curMonster.DeattachStaticAnimation(this);
			Object.Destroy(base.gameObject);
		}

		public override void InformCleanse()
		{
			bool flag = true;
			foreach (StaticAnimationBit staticAnimation in _curMonster.StaticAnimations)
			{
				if (staticAnimation != this && staticAnimation is FreezeStaticAnimation)
				{
					flag = false;
				}
			}
			if (flag)
			{
				_curMonster.Image.AnimateUnfrozen();
			}
			_curMonster.DeattachStaticAnimation(this);
			Object.Destroy(base.gameObject);
		}
	}
}
