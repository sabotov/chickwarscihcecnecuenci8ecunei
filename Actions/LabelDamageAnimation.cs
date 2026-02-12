using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class LabelDamageAnimation : BitActionAnimation
	{
		private readonly BitActionAnimation _innerAnimation;

		public LabelDamageAnimation(BitActionAnimation innerAnimation)
		{
			_innerAnimation = innerAnimation;
		}

		public override void PlayInnerAnimation(int line, ArmySide side, bool isColumn = false)
		{
			_innerAnimation.Animate(line, side, isColumn);
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				string str = item.Key();
				FieldMonsterVisual fieldMonsterVisual = item.Value as FieldMonsterVisual;
				if (fieldMonsterVisual == null || !fieldMonsterVisual.Data.isImmortal)
				{
					FieldVisual value = item.Value;
					object obj = str ?? "";
					if (obj == null)
					{
						obj = "";
					}
					AnimateMonsterLabel(value, (string)obj, LabelColor.Red);
				}
				dictionary.Add(() => str, item.Value);
			}
			if (_innerAnimation != null)
			{
				_innerAnimation.Animate(dictionary, onEnded);
			}
			else
			{
				onEnded();
			}
		}
	}
}
