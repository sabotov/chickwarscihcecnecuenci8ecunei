using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NGUI.Scripts.Internal;
using NGUI.Scripts.UI;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public class BubbleSelectElement : MonoBehaviourExt
	{
		public UILabel text;

		public Transform background;

		public bool isLeft;

		public bool isHorisontal;

		private Common.VoidDelegate completeDelegate;

		private UIWidget _collideWidget;

		private UIWidget CollideWidget
		{
			get
			{
				if ((object)_collideWidget == null)
				{
					_collideWidget = background.GetComponent<UIWidget>();
				}
				return _collideWidget;
			}
		}

		public bool isAnimating { get; private set; }

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		public void Init(string dialogText)
		{
			text.text = dialogText;
			background.GetComponent<UIStretchLegacy>().Update();
			UISprite component = background.GetComponent<UISprite>();
			float x = (float)((!isLeft) ? 1 : (-1)) * Mathf.Max(component.size.x - 154f, 0f) / 4f;
			if (isHorisontal)
			{
				x = (float)((!isLeft) ? 1 : (-1)) * Mathf.Max(component.size.y - 135f, 0f) / 4f;
			}
			base.transform.position = base.transform.position + new Vector3(x, 0f, 0f);
			base.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
			TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => base.transform.localScale, delegate(Vector3 localScale)
			{
				base.transform.localScale = localScale;
			}, new Vector3(1f, 1f, 1f), 0.1f);
			t.SetEase(Ease.OutBack);
			t.Play();
		}

		public void CompleteAnimation()
		{
			if (completeDelegate != null)
			{
				completeDelegate();
			}
		}

		public bool Collided(Vector3 coords)
		{
			bool num = background.position.x + (float)(CollideWidget.width / 2) * background.parent.lossyScale.x > coords.x && background.position.x - (float)(CollideWidget.width / 2) * background.parent.lossyScale.x < coords.x;
			bool flag = background.position.y + (float)(CollideWidget.height / 2) * background.parent.lossyScale.y > coords.y && background.position.y - (float)(CollideWidget.height / 2) * background.parent.lossyScale.y < coords.y;
			return num && flag;
		}
	}
}
