using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NGUI.Scripts.UI;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SplitRangedAnimation : BitActionAnimation
	{
		private EffectAnimation _effectAnimation;

		private const float TimerDelta = 1f;

		public override void Init(FieldVisual thisMonster)
		{
			_effectAnimation = new EffectAnimation(((FieldMonsterVisual)thisMonster).Data.staticInnerData.meleeEffect);
			base.Init(thisMonster);
			_effectAnimation.Init(thisMonster);
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			float num = 0f;
			FieldMonsterVisual fmv = _thisMonster as FieldMonsterVisual;
			for (int i = 0; i < monstersAction.Count; i++)
			{
				FieldMonsterVisual toHit = monstersAction.ElementAt(i).Value as FieldMonsterVisual;
				Common.StringDelegate onHitted = monstersAction.ElementAt(i).Key;
				bool isLastAction = i == monstersAction.Count - 1;
				Common.VoidDelegate procedure = delegate
				{
					_thisMonster.SetForward(v: true);
					Common.VoidDelegate voidDelegate = delegate
					{
						Vector3 startPos = _thisMonster.transform.localPosition;
						if (fmv != null)
						{
							startPos = fmv.GetShootPosition();
						}
						if (toHit != null)
						{
							Vector3 position = toHit.transform.position;
							GameObject nGo = UnityEngine.Object.Instantiate(PrefabCreator.ArrowPref);
							nGo.transform.parent = _thisMonster.transform.parent;
							nGo.transform.localScale = new Vector3(0.8f, 0.8f, 1f);
							nGo.transform.position = startPos;
							UISprite component = nGo.transform.Find("Sprite").GetComponent<UISprite>();
							if (fmv.arrowAtlas != null && fmv.arrowAtlas.name.Contains(fmv.arrowName))
							{
								component.atlas = fmv.arrowAtlas;
							}
							component.spriteName = fmv.arrowName;
							component.MakePixelPerfect();
							float num2 = Mathf.Atan((position.y - startPos.y) / (startPos.x - position.x));
							Vector3 zero = Vector3.zero;
							zero.z = (0f - num2) * 180f / (float)Math.PI;
							if (startPos.x > position.x)
							{
								nGo.transform.localScale = new Vector3(0f - nGo.transform.localScale.x, nGo.transform.localScale.y, nGo.transform.localScale.z);
							}
							nGo.transform.localEulerAngles = zero;
							TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => nGo.transform.position, delegate(Vector3 x)
							{
								nGo.transform.position = new Vector3(x.x, x.y, startPos.z);
							}, position, 0.15f);
							TweenCallback action = delegate
							{
								SoundManager.Instance.PlaySound(fmv.Data.staticInnerData.rangedHitSound);
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
								UnityEngine.Object.Destroy(nGo);
								_thisMonster.SetForward(v: false);
								if (isLastAction)
								{
									onEnded();
								}
							};
							t.SetEase(Ease.InCubic);
							t.OnComplete(action);
							t.Play();
						}
						SoundManager.Instance.PlaySound(((FieldMonsterVisual)_thisMonster).Data.staticInnerData.rangedSound);
					};
					if (fmv != null)
					{
						if (fmv.blockAnimation)
						{
							if (isLastAction)
							{
								onEnded();
							}
						}
						else
						{
							Dictionary<string, Common.VoidDelegate> eventDelegates = new Dictionary<string, Common.VoidDelegate> { { "fire", voidDelegate } };
							fmv.Animate(MonsterAnimationType.RangedShot, eventDelegates);
						}
					}
					else
					{
						voidDelegate();
					}
				};
				Initializer.WaitForProcedure(0.25f + num, procedure);
				num += 1f;
			}
		}
	}
}
