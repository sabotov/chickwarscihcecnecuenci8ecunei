using BattlefieldScripts.Core;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NGUI.Scripts.Internal;
using NGUI.Scripts.UI;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;
using UtilScripts;
using UtilScripts.ControlScripts;

namespace BattlefieldScripts
{
	public class BubbleElement : MonoBehaviourExt
	{
		private static GameObject _pref;

		[Header("Primary")]
		public UILabel text;

		public Transform background;

		public Vector2 shift;

		public ArmySide thisSide;

		[Header("Optional")]
		public UIAnchor tail;

		public GameObject Button;

		public BoxCollider ButtonCollider;

		private Common.VoidDelegate completeDelegate;

		private string _dialogText;

		private static GameObject pref
		{
			get
			{
				if (_pref == null)
				{
					_pref = Resources.Load<GameObject>("Prefabs/BattlePrefabs/WarlordSpeechBubble");
				}
				return _pref;
			}
		}

		public bool isAnimating { get; private set; }

		public void Destroy()
		{
			Object.Destroy(base.gameObject);
		}

		public void Init(FieldMonster monster, ArmySide side, string dialogText, Common.VoidDelegate OnClick = null)
		{
			if (OnClick != null)
			{
				ButtonEventListener.AddFunctionToButton(Button, OnClick);
				ButtonCollider.enabled = true;
			}
			else
			{
				ButtonCollider.enabled = false;
			}
			_dialogText = dialogText;
			thisSide = side;
			text.text = Localization.Localize(_dialogText);
			background.GetComponent<UIStretchLegacy>().Update();
			float num = background.GetComponent<UISprite>().size.x / 2f + shift.x;
			if (side == ArmySide.Right)
			{
				num *= -1f;
				background.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
			}
			if ((bool)tail)
			{
				Vector3 localScale = tail.transform.localScale;
				switch (side)
				{
				case ArmySide.Left:
					tail.side = UIAnchor.Side.BottomLeft;
					tail.pixelOffset = new Vector2(Mathf.Abs(tail.pixelOffset.x), tail.pixelOffset.y);
					tail.transform.localScale = new Vector3(Mathf.Abs(localScale.x), localScale.y, localScale.z);
					break;
				case ArmySide.Right:
					tail.side = UIAnchor.Side.BottomRight;
					tail.pixelOffset = new Vector2(0f - Mathf.Abs(tail.pixelOffset.x), tail.pixelOffset.y);
					tail.transform.localScale = new Vector3(0f - Mathf.Abs(localScale.x), localScale.y, localScale.z);
					break;
				}
			}
			base.transform.position = monster.visualElement.transform.position + new Vector3(num, shift.y, 0f);
			base.transform.localPosition = new Vector3(base.transform.localPosition.x, base.transform.localPosition.y, -3f);
			base.transform.localScale = new Vector3(0.9f, 0.9f, 1f);
			TweenerCore<Vector3, Vector3, VectorOptions> t = DOTween.To(() => base.transform.localScale, delegate(Vector3 x)
			{
				base.transform.localScale = x;
			}, new Vector3(1f, 1f, 1f), 0.1f);
			t.SetEase(Ease.OutBack);
			t.Play();
			SoundManager.Instance.PlaySound("int_bubble_appear");
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
			if (base.transform == null)
			{
				return false;
			}
			bool num = background.position.x + background.localScale.x * base.transform.lossyScale.x / 2f > coords.x && background.position.x - background.localScale.x * base.transform.lossyScale.x / 2f < coords.x;
			bool flag = background.position.y + background.localScale.y * base.transform.lossyScale.y / 2f > coords.y && background.position.y - background.localScale.y * base.transform.lossyScale.y / 2f < coords.y;
			return num && flag;
		}

		public void OnLocalize()
		{
			text.text = Localization.Localize(_dialogText);
		}

		public static BubbleElement CreateBubble()
		{
			return Object.Instantiate(pref).GetComponent<BubbleElement>();
		}
	}
}
