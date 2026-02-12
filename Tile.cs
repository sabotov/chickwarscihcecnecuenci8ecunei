using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using NGUI.Scripts.UI;
using NGUIPackageUtil;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UnityEngine;

namespace BattlefieldScripts
{
	public class Tile : MonoBehaviourExt
	{
		public enum TileHighlightning
		{
			NotHighlighted = 0,
			GreenBlinking = 1,
			YellowBlinking = 2,
			AttackLine = 3,
			SwordBlinking = 4,
			BowBlinking = 5,
			TowerBlinking = 6,
			AssistBlinking = 7
		}

		private static readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private const bool SHOW_FRAME = true;

		private static Color DARK_COLOR = new Color(0.5f, 0.5f, 0.5f, 1f);

		public static Color GREEN_COLOR = NewAssets.Scripts.UtilScripts.MyUtil.GetColor(157f, 253f, 73f, 1f);

		public static Color RED_COLOR = new Color(1f, 0f, 0f, 1f);

		private static float NORMAL_ALPHA = 1f;

		protected Vector3 FrameNormalSize;

		private UISprite _Frame;

		public UISprite swordSprite;

		public UISprite bowSprite;

		public UISprite towerSprite;

		public Transform collideElement;

		public int row;

		public int column;

		public bool trace;

		public TileHighlightning curHighlightning;

		public Color? curColor;

		private TileHighlightning? _prevHighlightning;

		private bool isAssist;

		private TweenerCore<float, float, FloatOptions> curTween;

		private bool _tweenAlpha;

		private float _alpha;

		private float _alphaMult = 1f;

		private static IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		private UISprite frame
		{
			get
			{
				if (_Frame == null)
				{
					_Frame = base.transform.Find("TileFrame").GetComponent<UISprite>();
					FrameNormalSize = _Frame.size;
				}
				return _Frame;
			}
		}

		public Vector3 framePosition => frame.transform.position;

		public Vector2 Coords => new Vector2(row, column);

		public bool hasPrevHighlightning => _prevHighlightning.HasValue;

		private void Start()
		{
			frame.gameObject.SetActive(value: true);
		}

		public bool Collided(Vector3 globalPos)
		{
			Vector3 position = frame.transform.position;
			Vector3 vector = frame.size;
			if (collideElement != null)
			{
				position = collideElement.transform.position;
				vector = collideElement.transform.lossyScale;
			}
			bool num = globalPos.x < position.x + vector.x / 2f && globalPos.x > position.x - vector.x / 2f;
			bool flag = globalPos.y < position.y + vector.y / 2f && globalPos.y > position.y - vector.y / 2f;
			return num && flag;
		}

		public void SetTileHighlight(TileHighlightning highlightning, Color? color = null)
		{
			if (trace)
			{
				Debug.Log(string.Concat("SetTileHighlight ", highlightning, color.HasValue ? (", " + color.Value.ToString()) : ""));
			}
			_prevHighlightning = curHighlightning;
			curColor = color;
			curHighlightning = highlightning;
			switch (highlightning)
			{
			case TileHighlightning.NotHighlighted:
				SetNotHighlighted();
				isAssist = false;
				break;
			case TileHighlightning.YellowBlinking:
				if (!isAssist)
				{
					SeYellowHighlighted();
				}
				break;
			case TileHighlightning.GreenBlinking:
				if (!isAssist)
				{
					SetHighlighted(showShord: false, showBow: false, showTower: false, color);
				}
				break;
			case TileHighlightning.AssistBlinking:
				SetAssistHighlighted(color);
				isAssist = true;
				break;
			case TileHighlightning.AttackLine:
				if (!isAssist)
				{
					SetLineHighlighted();
				}
				break;
			case TileHighlightning.BowBlinking:
				if (!isAssist)
				{
					SetHighlighted(showShord: false, showBow: true, showTower: false, color);
				}
				break;
			case TileHighlightning.TowerBlinking:
				if (!isAssist)
				{
					SetHighlighted(showShord: false, showBow: false, showTower: true, color);
				}
				break;
			case TileHighlightning.SwordBlinking:
				if (!isAssist)
				{
					SetHighlighted(showShord: true, showBow: false, showTower: false, color);
				}
				break;
			}
		}

		protected virtual void SetNotHighlighted()
		{
			if (swordSprite != null)
			{
				swordSprite.enabled = false;
			}
			if (bowSprite != null)
			{
				bowSprite.enabled = false;
			}
			if (towerSprite != null)
			{
				towerSprite.enabled = false;
			}
			InitAlphaTween();
			_alphaMult = 1f;
			_tweenAlpha = false;
			frame.alpha = 0f;
		}

		protected virtual void SetHighlighted(bool showShord = false, bool showBow = false, bool showTower = false, Color? color = null)
		{
			InitAlphaTween();
			_alphaMult = 0.99f;
			frame.enabled = false;
			float a = frame.color.a;
			Color color2 = (color.HasValue ? color.Value : Color.white);
			color2.a = a;
			frame.color = color2;
			if (swordSprite != null)
			{
				swordSprite.enabled = showShord;
				swordSprite.color = color2;
			}
			if (bowSprite != null)
			{
				bowSprite.enabled = showBow;
				bowSprite.color = color2;
			}
			if (towerSprite != null)
			{
				towerSprite.enabled = showTower;
				towerSprite.color = color2;
			}
			frame.size = FrameNormalSize;
			_tweenAlpha = true;
		}

		private void SetAssistHighlighted(Color? color = null)
		{
			if (swordSprite != null)
			{
				swordSprite.enabled = false;
			}
			if (bowSprite != null)
			{
				bowSprite.enabled = false;
			}
			if (towerSprite != null)
			{
				towerSprite.enabled = false;
			}
			InitAlphaTween();
			_alphaMult = 0.7f;
			frame.enabled = true;
			Color color2 = (color.HasValue ? color.Value : Color.white);
			float a = frame.color.a;
			color2.a = a;
			frame.color = color2;
			frame.size = FrameNormalSize;
			_tweenAlpha = true;
		}

		protected virtual void SetLineHighlighted()
		{
			if (swordSprite != null)
			{
				swordSprite.enabled = false;
			}
			if (bowSprite != null)
			{
				bowSprite.enabled = false;
			}
			if (towerSprite != null)
			{
				towerSprite.enabled = false;
			}
			InitAlphaTween();
			_alphaMult = 0.7f;
			frame.enabled = true;
			float a = frame.color.a;
			frame.color = new Color(0.7f, 0.01f, 0f, a);
			frame.size = FrameNormalSize;
			_tweenAlpha = true;
		}

		protected virtual void SeYellowHighlighted()
		{
			if (swordSprite != null)
			{
				swordSprite.enabled = false;
			}
			if (bowSprite != null)
			{
				bowSprite.enabled = false;
			}
			if (towerSprite != null)
			{
				towerSprite.enabled = false;
			}
			InitAlphaTween();
			_alphaMult = 1f;
			frame.enabled = true;
			_ = frame.color;
			frame.color = new Color(1f, 1f, 0f);
			frame.size = FrameNormalSize;
			_tweenAlpha = true;
		}

		private void InitAlphaTween()
		{
			if (curTween != null)
			{
				return;
			}
			_alpha = NORMAL_ALPHA;
			TweenCallback outDel = delegate
			{
			};
			TweenCallback inDel = delegate
			{
				TweenerCore<float, float, FloatOptions> t = DOTween.To(() => _alpha, delegate(float x)
				{
					_alpha = x;
					if (_tweenAlpha)
					{
						frame.alpha = _alpha * _alphaMult;
						if (swordSprite != null)
						{
							swordSprite.alpha = _alpha * _alphaMult;
						}
						if (bowSprite != null)
						{
							bowSprite.alpha = _alpha * _alphaMult;
						}
						if (towerSprite != null)
						{
							towerSprite.alpha = _alpha * _alphaMult;
						}
					}
				}, 97f / 170f, 0.8f);
				curTween = t;
				t.OnComplete(outDel);
				t.Play();
			};
			outDel = delegate
			{
				TweenerCore<float, float, FloatOptions> t = DOTween.To(() => _alpha, delegate(float x)
				{
					_alpha = x;
					if (_tweenAlpha)
					{
						frame.alpha = _alpha * _alphaMult;
						if (swordSprite != null)
						{
							swordSprite.alpha = _alpha * _alphaMult;
						}
						if (bowSprite != null)
						{
							bowSprite.alpha = _alpha * _alphaMult;
						}
						if (towerSprite != null)
						{
							towerSprite.alpha = _alpha * _alphaMult;
						}
					}
				}, NORMAL_ALPHA, 0.8f);
				curTween = t;
				t.OnComplete(inDel);
				t.Play();
			};
			if (Locator.HasService<IDelayedActionsHandler>())
			{
				_delayedActionsHandler.WaitForProcedure(0.1f, delegate
				{
					inDel();
				});
			}
			else
			{
				MonoBehaviourExtensions.WaitForProcedure(Initializer.instance, 0.1f, delegate
				{
					inDel();
				});
			}
		}
	}
}
