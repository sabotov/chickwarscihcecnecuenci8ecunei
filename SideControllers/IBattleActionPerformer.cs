using System.Collections.Generic;
using ActionBehaviours;
using UnityEngine;

namespace BattlefieldScripts.SideControllers
{
	public interface IBattleActionPerformer
	{
		void Init(List<Tile> tiles, UserBattleBehaviour.TileDelegate onTile, UserBattleBehaviour.SkillCardDragStops stopSkillDrag, UserBattleBehaviour.CardDragStops stopDrag, UserBattleBehaviour.MonstersHighlightDelegate highlightDelegate, UserBattleBehaviour.SkillDelegate onSkill, UserBattleBehaviour.CardDelegate onCard, UserBattleBehaviour.CardDragStarts startDragDelegate, UserBattleBehaviour.SkillCardDragStarts startSkillDragDelegate);

		void AnimateSkill(SkillCard skillCard, Vector3 targetPosition);

		void AnimatePlace(Card card, Tile tile);
	}
}
