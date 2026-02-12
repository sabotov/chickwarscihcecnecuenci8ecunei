using System.Collections.Generic;
using ActionBehaviours;
using DG.Tweening;
using NewAssets.Scripts.DataClasses;
using UI_Scripts.WindowManager;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.SideControllers
{
	public class AnimatedBattleActionPerformer : IBattleActionPerformer
	{
		private Card _selectedCard;

		private SkillCard _selectedSkillCard;

		private GameObject _dragObj;

		private List<Tile> _tiles;

		private UserBattleBehaviour.TileDelegate tileDelegate;

		private UserBattleBehaviour.CardDelegate _cardDelegate;

		private UserBattleBehaviour.SkillDelegate _skillDelegate;

		private UserBattleBehaviour.MonstersHighlightDelegate _highlightDelegate;

		private UserBattleBehaviour.CardDragStarts _startDragDelegate;

		private UserBattleBehaviour.SkillCardDragStarts _startSkillDragDelegate;

		private Tile _preselectedTile;

		private Tile.TileHighlightning _curBlinkStyle;

		private Color? _curBlinkColor;

		public void Init(List<Tile> tiles, UserBattleBehaviour.TileDelegate onTile, UserBattleBehaviour.SkillCardDragStops stopSkillDrag, UserBattleBehaviour.CardDragStops stopDrag, UserBattleBehaviour.MonstersHighlightDelegate highlightDelegate, UserBattleBehaviour.SkillDelegate onSkill, UserBattleBehaviour.CardDelegate onCard, UserBattleBehaviour.CardDragStarts startDragDelegate, UserBattleBehaviour.SkillCardDragStarts startSkillDragDelegate)
		{
			_startDragDelegate = startDragDelegate;
			_startSkillDragDelegate = startSkillDragDelegate;
			_tiles = tiles;
			_highlightDelegate = highlightDelegate;
			_cardDelegate = onCard;
			tileDelegate = onTile;
			_skillDelegate = onSkill;
		}

		public void AnimateSkill(SkillCard skillCard, Vector3 targetPosition)
		{
			_selectedSkillCard = skillCard;
			_highlightDelegate(highlighted: false, null, Vector2.zero);
			DOTween.To(() => skillCard.transform.position, delegate(Vector3 newPosition)
			{
				OnDrag(newPosition);
			}, targetPosition, 0.5f).OnComplete(delegate
			{
				_highlightDelegate(highlighted: false, null, Vector2.zero);
				if (_dragObj != null)
				{
					if (_selectedSkillCard != null)
					{
						_skillDelegate(_selectedSkillCard);
					}
					Object.Destroy(_dragObj);
					_dragObj = null;
				}
				_selectedSkillCard = null;
			});
		}

		public void AnimatePlace(Card card, Tile tile)
		{
			ButtonListener.SetAllButtonsInteractive(_interactive: false);
			_selectedCard = card;
			if (_selectedCard != null)
			{
				_cardDelegate(_selectedCard);
			}
			WindowScriptCore<BattlefieldWindow>.instance.AnimatePlace(_selectedCard, tile, delegate(float duration)
			{
				DOTween.To(() => _selectedCard.transform.position, OnDrag, tile.transform.position, duration).OnComplete(delegate
				{
					_highlightDelegate(highlighted: false, null, Vector2.zero);
					if (_dragObj != null)
					{
						if (tile != null)
						{
							tileDelegate(tile);
						}
						FieldMonsterVisual componentNoAlloc = _dragObj.GetComponentNoAlloc<FieldMonsterVisual>();
						if (componentNoAlloc != null)
						{
							componentNoAlloc.Destroy();
						}
						_dragObj = null;
					}
					else if (_selectedCard != null)
					{
						_highlightDelegate(highlighted: true, _selectedCard.data, new Vector2(-1f, -1f));
					}
					_selectedCard = null;
					ButtonListener.SetAllButtonsInteractive(_interactive: true);
				});
			});
		}

		private void OnDrag(Vector3 newPosition)
		{
			if (_dragObj == null)
			{
				if (_selectedCard != null)
				{
					_dragObj = _startDragDelegate(_selectedCard);
				}
				else if (_selectedSkillCard != null)
				{
					_dragObj = _startSkillDragDelegate(_selectedSkillCard);
				}
			}
			float num = 0f;
			if (_selectedCard != null)
			{
				num = 40f;
			}
			_dragObj.transform.position = new Vector3(newPosition.x, newPosition.y + num, _dragObj.transform.position.z);
			if (!(_selectedCard != null))
			{
				return;
			}
			Tile preselectedTile = _preselectedTile;
			_preselectedTile = _tiles.Find((Tile x) => x.Collided(_dragObj.transform.Find("SuitAnimation").position));
			bool flag = false;
			bool highlighted = false;
			Vector3 vector = Vector3.zero;
			MonsterData monster = null;
			if (preselectedTile != null && preselectedTile != _preselectedTile && preselectedTile.curHighlightning == Tile.TileHighlightning.YellowBlinking)
			{
				preselectedTile.SetTileHighlight(_curBlinkStyle, _curBlinkColor);
				flag = true;
				vector = Vector2.zero;
			}
			if (_preselectedTile != null && preselectedTile != _preselectedTile)
			{
				if (_preselectedTile.curHighlightning == Tile.TileHighlightning.GreenBlinking || _preselectedTile.curHighlightning == Tile.TileHighlightning.BowBlinking || _preselectedTile.curHighlightning == Tile.TileHighlightning.TowerBlinking || _preselectedTile.curHighlightning == Tile.TileHighlightning.SwordBlinking)
				{
					_curBlinkStyle = _preselectedTile.curHighlightning;
					_curBlinkColor = _preselectedTile.curColor;
					flag = true;
					highlighted = true;
					monster = _selectedCard.data;
					vector = _preselectedTile.Coords;
					_preselectedTile.SetTileHighlight(Tile.TileHighlightning.YellowBlinking);
				}
				else
				{
					_preselectedTile = null;
				}
			}
			if (flag)
			{
				_highlightDelegate(highlighted, monster, vector);
			}
		}
	}
}
