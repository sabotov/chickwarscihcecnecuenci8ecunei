using System;
using System.Collections.Generic;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class HealAnimation : BitActionAnimation
	{
		private readonly BitActionAnimation _innerAnimation;

		public HealAnimation(BitActionAnimation innerAnimation)
		{
			_innerAnimation = innerAnimation;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				string str = item.Key();
				if (str != "0")
				{
					AnimateMonsterLabel(item.Value, ("+" + str) ?? "", LabelColor.Green);
					dictionary.Add(() => str, item.Value);
				}
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
