using System;
using System.Collections.Generic;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class ValueCheckAnimation : BitActionAnimation
	{
		private BitActionAnimation _lowerAnimation;

		public ValueCheckAnimation(BitActionAnimation lowerAnimation)
		{
			_lowerAnimation = lowerAnimation;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			Dictionary<Common.StringDelegate, FieldVisual> dictionary = new Dictionary<Common.StringDelegate, FieldVisual>();
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				string testStr = item.Key();
				if (!int.TryParse(testStr, out var result) || result != 0)
				{
					dictionary.Add(() => testStr, item.Value);
				}
			}
			_lowerAnimation.Animate(dictionary, onEnded);
		}
	}
}
