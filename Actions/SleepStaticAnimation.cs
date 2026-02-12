using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NGUI.Scripts.UI;
using UnityEngine;

namespace BattlefieldScripts.Actions
{
	public class SleepStaticAnimation : StaticAnimationBit
	{
		public UISprite clock;

		public UILabel turns;

		private Vector3 _trurnsSize;

		private int _turns;

		public void Init(FieldMonsterVisual monster, int turns)
		{
			Init(monster);
			_turns = turns;
			this.turns.text = string.Concat(_turns);
			_trurnsSize = this.turns.transform.localScale;
		}

		public override void InformTrigger(TriggerType trigger)
		{
			if (trigger != TriggerType.NewTurn && trigger != TriggerType.ClearParams)
			{
				return;
			}
			_turns--;
			if (_turns == 0 || trigger == TriggerType.ClearParams)
			{
				_curMonster.DeattachStaticAnimation(this);
				Object.Destroy(base.gameObject);
				return;
			}
			turns.text = string.Concat(_turns);
			TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => turns.transform.localScale, delegate(Vector3 x)
			{
				turns.transform.localScale = new Vector3(x.x, x.y, _trurnsSize.z);
			}, _trurnsSize * 2f, 0.1f);
			TweenCallback action = delegate
			{
				DOTween.To(() => turns.transform.localScale, delegate(Vector3 x)
				{
					turns.transform.localScale = new Vector3(x.x, x.y, _trurnsSize.z);
				}, _trurnsSize, 0.1f).Play();
			};
			t.OnComplete(action);
			t.Play();
		}
	}
}
