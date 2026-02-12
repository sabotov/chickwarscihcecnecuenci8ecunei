using System;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class ChainAttackAnimation : BitActionAnimation
	{
		private readonly string _effname;

		private readonly GameObject _effect;

		public ChainAttackAnimation(string name)
		{
			_effname = name;
			_effect = EffectAnimation.GetEffect(name);
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			List<FieldVisual> list = new List<FieldVisual>(monstersAction.Values);
			GameObject locEffect = null;
			if (_effect != null)
			{
				locEffect = UnityEngine.Object.Instantiate(_effect);
				locEffect.transform.parent = list[0].transform.parent;
				locEffect.transform.localPosition = list[0].transform.localPosition + new Vector3(0f, 0f, -2f);
			}
			else
			{
				Debug.Log(_effname);
			}
			List<Common.VoidDelegate> delegates = new List<Common.VoidDelegate>();
			int num = -1;
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item2 in monstersAction)
			{
				FieldMonsterVisual mon = ((item2.Value is FieldMonsterVisual) ? (item2.Value as FieldMonsterVisual) : null);
				Common.StringDelegate action = item2.Key;
				if (mon == null)
				{
					continue;
				}
				num++;
				if (num == 0)
				{
					string str = action();
					if (!mon.Data.isImmortal)
					{
						AnimateMonsterLabel(mon, str, LabelColor.Red);
					}
					continue;
				}
				int curCount = num;
				Common.VoidDelegate item = delegate
				{
					TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => locEffect.transform.localPosition, delegate(Vector3 x)
					{
						locEffect.transform.localPosition = new Vector3(x.x, x.y, _thisMonster.transform.localPosition.z);
					}, mon.transform.localPosition, 0.3f);
					TweenCallback action2 = delegate
					{
						string str2 = action();
						if (!mon.Data.isImmortal)
						{
							AnimateMonsterLabel(mon, str2, LabelColor.Red);
						}
						if (curCount == delegates.Count)
						{
							onEnded();
						}
						else
						{
							delegates[curCount]();
						}
					};
					t.SetEase(Ease.OutCubic);
					t.OnComplete(action2);
					t.SetDelay(0.2f);
					t.Play();
				};
				delegates.Add(item);
			}
			if (delegates.Count > 0)
			{
				delegates[0]();
			}
			else
			{
				onEnded();
			}
		}
	}
}
