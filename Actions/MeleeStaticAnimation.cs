using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class MeleeStaticAnimation : BitActionAnimation
	{
		private EffectAnimation _effectAnimation;

		public override void Init(FieldVisual thisMonster)
		{
			if (!(thisMonster is FieldMonsterVisual))
			{
				Debug.LogError("Trying to init MeleeAnimation on something appears not to be FieldMonsterVisual");
			}
			_effectAnimation = new EffectAnimation(((FieldMonsterVisual)thisMonster).Data.staticInnerData.meleeEffect);
			base.Init(thisMonster);
			_effectAnimation.Init(thisMonster);
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			FieldMonsterVisual toHit = null;
			Common.StringDelegate onHitted = () => "";
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				if (item.Key != null && toHit == null)
				{
					toHit = item.Value as FieldMonsterVisual;
					onHitted = item.Key;
				}
				else if (item.Key != null)
				{
					item.Key();
				}
			}
			if (!(toHit != null))
			{
				return;
			}
			FieldMonsterVisual fmv = _thisMonster as FieldMonsterVisual;
			if (!(fmv != null))
			{
				return;
			}
			TweenCallback onTweened = delegate
			{
				string str = onHitted();
				if (!toHit.Data.isImmortal)
				{
					AnimateMonsterLabel(toHit, str, LabelColor.Red);
				}
				_effectAnimation.Animate(new Dictionary<Common.StringDelegate, FieldVisual> { 
				{
					() => "",
					toHit
				} }, delegate
				{
				});
				onEnded();
			};
			((Common.VoidDelegate)delegate
			{
				bool shouldHit = true;
				Dictionary<string, Common.VoidDelegate> eventDelegates = new Dictionary<string, Common.VoidDelegate> { 
				{
					"hit",
					delegate
					{
						if (shouldHit)
						{
							onTweened();
						}
						shouldHit = false;
					}
				} };
				SoundManager.Instance.PlaySound(((FieldMonsterVisual)_thisMonster).Data.staticInnerData.meleeSound);
				if (UnityEngine.Random.value > 0.5f)
				{
					SoundManager.Instance.PlaySound(((FieldMonsterVisual)_thisMonster).Data.staticInnerData.meleeVoice);
				}
				fmv.Animate(MonsterAnimationType.MeleeHit, eventDelegates);
			})();
		}
	}
}
