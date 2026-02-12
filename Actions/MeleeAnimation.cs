using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class MeleeAnimation : BitActionAnimation
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
			_thisMonster.SetForward(v: true);
			Vector3 localPosition = _thisMonster.transform.localPosition;
			if (!(toHit != null))
			{
				return;
			}
			Vector3 vector = toHit.transform.localPosition - localPosition;
			FieldMonsterVisual fmv = _thisMonster as FieldMonsterVisual;
			if (!(fmv != null))
			{
				return;
			}
			int num = (int)Mathf.Abs(toHit.Coords.x - fmv.animateCoords.x);
			fmv.animateCoords = new Vector2(toHit.Coords.x - Mathf.Sign(toHit.Coords.x - fmv.animateCoords.x), toHit.Coords.y);
			float num2 = vector.x / (float)num;
			Vector3 endValue = new Vector3(toHit.transform.localPosition.x, localPosition.y, localPosition.z);
			endValue.x -= num2;
			float num3 = (float)(num - 1) / (TimeDebugController.instance.meleeAnimInSpeed * TimeDebugController.totalTimeMultiplier);
			TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => _thisMonster.transform.localPosition, delegate(Vector3 x)
			{
				_thisMonster.transform.localPosition = x;
			}, endValue, num3);
			if (num3 > 0f)
			{
				int num4 = UnityEngine.Random.Range(1, 4);
				SoundManager.Instance.PlaySound("fight_move_" + num4);
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
				_thisMonster.SetForward(v: false);
				onEnded();
			};
			Common.VoidDelegate onHTweenedIn = delegate
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
				if (fmv.blockAnimation)
				{
					onEnded();
				}
				else
				{
					fmv.Animate(MonsterAnimationType.MeleeHit, eventDelegates);
				}
			};
			t.SetEase(TimeDebugController.instance.meleeInEase).OnComplete(delegate
			{
				onHTweenedIn();
			}).Play();
		}
	}
}
