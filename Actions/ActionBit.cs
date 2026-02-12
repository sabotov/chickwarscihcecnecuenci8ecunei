using System;
using System.Collections.Generic;
using System.Linq;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	[Serializable]
	public class ActionBit
	{
		private delegate bool _ignoreSkill();

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private ActionBitSignature _signature;

		private List<Common.TripleResultDelegate> _onPerformedStack = new List<Common.TripleResultDelegate>();

		public int performCount = 1;

		public int repeatDelay = 1;

		protected FieldElement _curMonster;

		protected BitTrigger _trigger;

		protected BitTrigger _affectedTrigger;

		protected BitFilter _filter;

		protected BitAction _action;

		protected ConditionType _rechargeType = ConditionType.NoCondition;

		protected bool _rechargeValue;

		protected BitFilter _rechargeCondition;

		protected bool _withoutDelay;

		protected Dictionary<Vector2, FieldMonster> _monsterStorage = new Dictionary<Vector2, FieldMonster>();

		protected AnimatedIterator _statChangeIterator;

		protected int _curCounter;

		public int _startCounter = -1;

		public int _counter = -1;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public ActionBit(ActionBitSignature signature, BitTrigger trigger, BitFilter filter, BitAction action, BitTrigger affectedTrigger = null, int counter = -1)
		{
			_signature = signature;
			_trigger = trigger;
			_affectedTrigger = affectedTrigger;
			_filter = filter;
			_action = action;
			_startCounter = counter;
			_counter = counter;
			_statChangeIterator = new AnimatedIterator();
		}

		public void InitRecharge(ConditionType type, bool value, BitFilter condition)
		{
			_rechargeType = type;
			_rechargeValue = value;
			_rechargeCondition = condition;
		}

		public void Init(FieldElement curMonster, ArmyControllerCore controller, FieldParameters curParameters, Func<Vector2> positionDelegate, FieldRandom random, bool withoutDelay = false)
		{
			_withoutDelay = withoutDelay;
			_curMonster = curMonster;
			_action.Init(curMonster, curParameters, controller, random, () => curMonster is FieldMonster fieldMonster && fieldMonster.IsRanged());
			_trigger.Init(curMonster.Side, curParameters, positionDelegate);
			if (_affectedTrigger != null)
			{
				_affectedTrigger.Init(curMonster.Side, curParameters, positionDelegate);
			}
			_filter.Init(curMonster.Side, curParameters, positionDelegate, random, () => curMonster is FieldMonster fieldMonster && fieldMonster.IsRanged());
			_onPerformedStack = new List<Common.TripleResultDelegate>();
		}

		public ActionBitSignature GetSignature()
		{
			return _signature;
		}

		public BitFilter GetFilter()
		{
			return _filter;
		}

		public bool CheckSignature(FieldElement element, SkillType name)
		{
			for (int i = 0; i < element.Actions.Count; i++)
			{
				if (element.Actions[i]._signature.signature == name)
				{
					return true;
				}
			}
			return false;
		}

		public bool ShouldPerform(TriggerType trigger, SkillType originSkill, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, object param = null)
		{
			bool num = _trigger.CheckTrigger(trigger, originSkill, actionPlace, actionParameter, affectedParameter, param);
			bool flag = _affectedTrigger == null || affectedParameter == null || _affectedTrigger.CheckTrigger(trigger, originSkill, affectedParameter.coords, affectedParameter, actionParameter, param);
			return num && flag;
		}

		public IEnumerable<KeyValuePair<Vector2, FieldMonster>> GetRightMonsters()
		{
			return _filter.GetRightMonsters(null, _signature.signature);
		}

		public void CreateEffectWithoutMonsters()
		{
			ArmySide armySide = (BitStaticFilter.IsEnemySideToPlayEffect ? _curMonster.Side.OtherSide() : _curMonster.Side);
			if (_filter.LinesForEffect.Count != 0)
			{
				foreach (int item in _filter.LinesForEffect)
				{
					if (_action.Animation is LabelDamageAnimation)
					{
						_action.Animation.PlayInnerAnimation(item, armySide);
					}
					else
					{
						_action.Animation.Animate(item, armySide);
					}
				}
				_filter.LinesForEffect.Clear();
			}
			if (_filter.ColumnsForEffect.Count == 0)
			{
				return;
			}
			foreach (int item2 in _filter.ColumnsForEffect)
			{
				if (_action.Animation is LabelDamageAnimation)
				{
					_action.Animation.PlayInnerAnimation(item2, armySide, isColumn: true);
				}
				else
				{
					_action.Animation.Animate(item2, armySide, isColumn: true);
				}
			}
			_filter.ColumnsForEffect.Clear();
		}

		public void TryPerform(TriggerType trigger, SkillType originSkill, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, Common.TripleResultDelegate onPerformed, object param = null)
		{
			bool num = ShouldPerform(trigger, originSkill, actionPlace, actionParameter, affectedParameter, param);
			_onPerformedStack.Add(onPerformed);
			if (!num)
			{
				OnPerformed(success: false, null);
				return;
			}
			if (CheckCounterOfExecute())
			{
				OnPerformed(success: false, null);
				return;
			}
			if (actionParameter is FieldMonster fieldMonster && fieldMonster.HaveMiss && (trigger == TriggerType.Attack || trigger == TriggerType.AttackPerformed))
			{
				OnPerformed(success: false, null);
				return;
			}
			_curCounter++;
			if (_curCounter < repeatDelay)
			{
				OnPerformed(success: false, null);
				return;
			}
			_curCounter = 0;
			_monsterStorage.Clear();
			foreach (KeyValuePair<Vector2, FieldMonster> rightMonster in _filter.GetRightMonsters(affectedParameter, _signature.signature))
			{
				if (!_monsterStorage.ContainsKey(rightMonster.Key))
				{
					_monsterStorage.Add(rightMonster.Key, rightMonster.Value);
				}
			}
			CreateEffectWithoutMonsters();
			if (_action.ShouldCheckIfFilterNotEmpty && !_monsterStorage.Any())
			{
				OnPerformed(success: false, null);
				return;
			}
			_action.SetAffected(affectedParameter);
			_action.PerformAction(_monsterStorage, OnPerformed);
		}

		private bool CheckCounterOfExecute()
		{
			if (_startCounter != -1)
			{
				_counter--;
				if (_counter <= 0)
				{
					return true;
				}
			}
			return false;
		}

		protected bool LockedByEvasion(TriggerType trigger, FieldElement target, FieldElement source)
		{
			if (trigger != TriggerType.AttackPerformed && trigger != TriggerType.Attacked)
			{
				return false;
			}
			FieldMonster fieldMonster = source as FieldMonster;
			FieldMonster fieldMonster2 = target as FieldMonster;
			if (fieldMonster != null && fieldMonster2 != null)
			{
				switch (trigger)
				{
				case TriggerType.AttackPerformed:
					return fieldMonster2.ImmuneByEvasion(fieldMonster);
				case TriggerType.Attacked:
					return fieldMonster.ImmuneByEvasion(fieldMonster2);
				default:
					return false;
				}
			}
			return false;
		}

		private void OnPerformed(bool success, FieldElement paramElem)
		{
			Common.TripleResultDelegate onPerformed = _onPerformedStack[_onPerformedStack.Count - 1];
			_onPerformedStack.RemoveAt(_onPerformedStack.Count - 1);
			bool shouldAddCharge = false;
			if (success)
			{
				if (_rechargeType != ConditionType.NoCondition)
				{
					bool rechargeValue = _rechargeValue;
					foreach (KeyValuePair<Vector2, FieldMonster> item in _monsterStorage)
					{
						if (_rechargeCondition.Data.CheckFilter(item.Key, item.Value, null, SkillType.NoSkill, _rechargeCondition) == rechargeValue)
						{
							if (_rechargeType == ConditionType.Any)
							{
								shouldAddCharge = true;
								break;
							}
							if (_rechargeType == ConditionType.All)
							{
								shouldAddCharge = true;
							}
						}
						else if (_rechargeType == ConditionType.All)
						{
							shouldAddCharge = false;
							break;
						}
					}
				}
				Action onFirstPerformed = delegate
				{
					TriggerType broadcastSignal = _signature.GetBroadcastSignal();
					bool shouldRecheckDeath = _signature.ShouldRecheckDeath();
					if (broadcastSignal != TriggerType.NoTrigger && !LockedByEvasion(broadcastSignal, _curMonster, paramElem))
					{
						_curMonster.BroadcastAction(broadcastSignal, _signature.signature, (paramElem == null) ? Vector2.zero : paramElem.coords, paramElem, _curMonster, delegate
						{
							onPerformed(result1: true, shouldRecheckDeath, shouldAddCharge);
						});
					}
					else
					{
						onPerformed(result1: true, shouldRecheckDeath, shouldAddCharge);
					}
				};
				Common.VoidDelegate performFirstTrigger = delegate
				{
					TriggerType onCompletedTrigger = _signature.GetOnCompletedTrigger();
					if (onCompletedTrigger != TriggerType.NoTrigger && !LockedByEvasion(onCompletedTrigger, paramElem, _curMonster))
					{
						_curMonster.BroadcastAction(onCompletedTrigger, _signature.signature, _curMonster.coords, _curMonster, paramElem, onFirstPerformed);
					}
					else
					{
						onFirstPerformed();
					}
				};
				Action onStatChangePerformed = delegate
				{
					if (_withoutDelay)
					{
						performFirstTrigger();
					}
					else
					{
						_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.afterSkillDelay / TimeDebugController.totalTimeMultiplier, performFirstTrigger);
					}
				};
				Action onSelfPerformed = delegate
				{
					if (_signature.ShouldPerformStatChangeAffected)
					{
						_statChangeIterator.IterateOnActions(_monsterStorage, delegate(KeyValuePair<Vector2, FieldMonster> pair, Action onCompl)
						{
							pair.Value.PerformStatDropTrigger(onCompl);
						}, onStatChangePerformed);
					}
					else
					{
						onStatChangePerformed();
					}
				};
				Action action = delegate
				{
					if (_signature.ShouldPerformStatChangeSelf && _curMonster is FieldMonster)
					{
						(_curMonster as FieldMonster).PerformStatDropTrigger(onSelfPerformed);
					}
					else
					{
						onSelfPerformed();
					}
				};
				if (_signature.ShouldPerformSilence)
				{
					_statChangeIterator.IterateOnActions(_monsterStorage, delegate(KeyValuePair<Vector2, FieldMonster> pair, Action onCompl)
					{
						pair.Value.PerformSilenceTrigger(_curMonster, onCompl);
					}, action);
				}
				else
				{
					action();
				}
			}
			else
			{
				onPerformed(result1: false, result2: false, result3: false);
			}
		}
	}
}
