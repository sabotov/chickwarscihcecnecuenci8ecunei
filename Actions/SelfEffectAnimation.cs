using System;
using System.Collections.Generic;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SelfEffectAnimation : EffectAnimation
	{
		private readonly BitActionAnimation _innerAnimation;

		public SelfEffectAnimation(BitActionAnimation innerAnimation, string effectName)
			: base(effectName)
		{
			_innerAnimation = innerAnimation;
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			if (_effect != null && _thisMonster != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(_effect);
				try
				{
					gameObject.transform.parent = _thisMonster.transform.parent;
				}
				catch
				{
					Debug.LogError(effname);
				}
				gameObject.transform.localPosition = _thisMonster.transform.localPosition + new Vector3(0f, 0f, -2f);
			}
			else
			{
				Debug.Log(effname);
			}
			if (_innerAnimation != null)
			{
				_innerAnimation.Animate(monstersAction, onEnded);
			}
			else
			{
				onEnded();
			}
		}
	}
}
