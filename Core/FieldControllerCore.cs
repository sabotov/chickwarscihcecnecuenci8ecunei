using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Core
{
	public class FieldControllerCore
	{
		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		protected Dictionary<ArmySide, ArmyControllerCore> _armies;

		protected float waitTime;

		protected bool skipDelay;

		protected ArmySide _curActiveArmy;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public FieldScriptWrapper.CompleteFightParams LastFightResult { get; protected set; }

		public void RequestMonsterSummon(ArmySide requester, MonsterData monster, Vector2 place, Action onPlaced, FieldMonster copyTarget = null, Action<FieldMonster> onPlacedAction = null, bool fromReborn = false)
		{
			if (requester == ArmySide.Left)
			{
				_armies[ArmySide.Right].PlaceMonster(monster, place, onPlaced, copyTarget, onPlacedAction, fromReborn);
			}
			else
			{
				_armies[ArmySide.Left].PlaceMonster(monster, place, onPlaced, copyTarget, onPlacedAction, fromReborn);
			}
		}

		public void RequestRuneAdding(ArmySide requester, RuneData runeData, Vector2 place)
		{
			if (requester == ArmySide.Left)
			{
				_armies[ArmySide.Right].PlaceRune(runeData, place);
			}
			else
			{
				_armies[ArmySide.Left].PlaceRune(runeData, place);
			}
		}

		public void RequestRuneDeleting(ArmySide requester, FieldRune fieldRune)
		{
			if (requester == ArmySide.Left)
			{
				_armies[ArmySide.Right].DestroyRune(fieldRune);
			}
			else
			{
				_armies[ArmySide.Left].DestroyRune(fieldRune);
			}
		}

		public Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> GetArmiesStates()
		{
			Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> dictionary = new Dictionary<ArmySide, SimulateEnviroment.SimulateArmy>();
			foreach (KeyValuePair<ArmySide, ArmyControllerCore> army in _armies)
			{
				ArmySide key = army.Key;
				SimulateEnviroment.SimulateArmy value = new SimulateEnviroment.SimulateArmy
				{
					army = army.Value.GetFieldMonsters(),
					warlord = army.Value.GetWarlord(),
					pet = army.Value.GetPet(),
					runes = army.Value.GetFieldRunes(),
					drawType = army.Value.GetDrawType(),
					battleType = army.Value.GetBattleType(),
					raritySequence = army.Value.GetRaritySequence(),
					upkeepCount = army.Value.GetUpkeepCount(),
					availableSkills = army.Value.GetAvailableSkills(),
					hand = army.Value.GetHand(),
					deck = army.Value.GetDeck(),
					reserveMosnter = army.Value.GetReserveMonster()
				};
				dictionary.Add(key, value);
			}
			return dictionary;
		}

		protected virtual void BreakStack(Common.VoidDelegate del)
		{
			_delayedActionsHandler.WaitForProcedure(0f, del);
		}

		public void BroadcastDeathRecheck(ArmySide side, Action onChecked)
		{
			BreakStack(delegate
			{
				ArmySide otherSide = ((side == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
				Action onCompleted = delegate
				{
					_armies[otherSide].RecheckDeath(onChecked);
				};
				_armies[side].RecheckDeath(onCompleted);
			});
		}

		public void BroadcastAction(ArmySide side, TriggerType trigger, SkillType origin, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, Action onPerformed, object param = null)
		{
			ArmySide otherSide = ((side == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
			Action onBroadcasted = delegate
			{
				_armies[otherSide].PerformTrigger(trigger, origin, actionPlace, actionParameter, affectedParameter, onPerformed, checkDeath: false, param);
			};
			BreakStack(delegate
			{
				_armies[side].PerformTrigger(trigger, origin, actionPlace, actionParameter, affectedParameter, onBroadcasted, checkDeath: false, param);
			});
		}

		public virtual void InformMonsterDead(ArmySide side, Vector2 place, FieldMonster monster)
		{
			_armies[(side == ArmySide.Left) ? ArmySide.Right : ArmySide.Left].InformEnemyMonsterDead(place, monster);
			_armies[side].InformMonsterDead(place, monster);
		}

		public virtual void InformMonsterHit(ArmySide side, Vector2 place, FieldMonster monster)
		{
			_armies[(side == ArmySide.Left) ? ArmySide.Right : ArmySide.Left].InformEnemyMonsterHit(place, monster);
			_armies[side].InformMonsterHit(place, monster);
		}

		public virtual void InformDefeat(ArmySide defeatedSide, bool delay = false, bool isTie = false)
		{
			ArmySide key = ((defeatedSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
			_armies[defeatedSide].InformFightCompleted();
			_armies[key].InformFightCompleted();
			_armies[defeatedSide].InformDefeat();
			_armies[key].InformVictory();
		}

		public virtual void InformError()
		{
			foreach (KeyValuePair<ArmySide, ArmyControllerCore> army in _armies)
			{
				army.Value.InformError();
			}
		}

		public void AnimateDefeat(ArmySide defeatedSide)
		{
			ArmySide key = ((defeatedSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
			_armies[defeatedSide].AnimateDefeat();
			_armies[key].AnimateVictory();
		}

		protected virtual void SwitchActiveArmy(ArmySide prev)
		{
			_armies[prev].InformEnemyTurnStarted();
			_armies[(prev == ArmySide.Left) ? ArmySide.Right : ArmySide.Left].InformEnemyTurnEnded();
			if (prev == ArmySide.Left)
			{
				PerformStep(ArmySide.Right);
			}
			if (prev == ArmySide.Right)
			{
				PerformStep(ArmySide.Left);
			}
		}

		protected virtual void PerformStep(ArmySide controller)
		{
			_curActiveArmy = controller;
			_armies[_curActiveArmy].PerformUpkeepPhase(OnUpkeepPhase);
		}

		private void OnUpkeepPhase()
		{
			if (_curActiveArmy == ArmySide.Left || waitTime == 0f || skipDelay)
			{
				_armies[_curActiveArmy].PerformPlacementPhase(OnPlacementPhase);
				if (skipDelay)
				{
					skipDelay = false;
				}
			}
			else
			{
				_delayedActionsHandler.WaitForProcedure(waitTime, delegate
				{
					_armies[_curActiveArmy].PerformPlacementPhase(OnPlacementPhase);
				});
			}
		}

		private void OnPlacementPhase()
		{
			_armies[_curActiveArmy].PerformActionPhase(OnActionPhase);
		}

		private void OnActionPhase()
		{
			_armies[_curActiveArmy].PerformEndingPhase(OnEndingPhase);
		}

		private void OnEndingPhase()
		{
			SwitchActiveArmy(_curActiveArmy);
		}
	}
}
