using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class BitActionAnimation
	{
		protected FieldVisual _thisMonster;

		public virtual void Init(FieldVisual thisMonster)
		{
			_thisMonster = thisMonster;
		}

		public virtual void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				item.Key();
			}
			onEnded();
		}

		public virtual void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded, ArmySide side)
		{
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				item.Key();
			}
			onEnded();
		}

		public virtual void Animate(int line, ArmySide side = ArmySide.Right, bool isColumn = false)
		{
		}

		public virtual void PlayInnerAnimation(int line, ArmySide isRightSide, bool isColumn = false)
		{
		}

		protected void AnimateMonsterLabel(FieldVisual mon, string str, LabelColor color)
		{
			LabelElement.AnimateLabel(mon.transform, str, color);
		}
	}
}
