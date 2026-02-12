using System;
using System.Collections.Generic;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SoundAnimation : BitActionAnimation
	{
		private readonly string _sound;

		public SoundAnimation(string name)
		{
			_sound = name;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			SoundManager.Instance.PlaySound(_sound);
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				item.Key();
			}
			onEnded();
		}
	}
}
