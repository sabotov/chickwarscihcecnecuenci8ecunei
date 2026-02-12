using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts
{
	public abstract class FieldElement
	{
		public FieldParameters parameters;

		public FieldVisual visualElement;

		public Vector2 coords;

		protected ArmyControllerCore _curController;

		protected IteratorCore _iterator;

		protected bool _isStillExist = true;

		protected List<ActionBit> _actions;

		protected bool _breakFlag;

		public List<ActionBit> Actions => _actions;

		private bool CanPerformAttackStart
		{
			get
			{
				if (!(this is FieldMonster fieldMonster) || !fieldMonster.canAttack || fieldMonster.data.monsterClass == Class.Building)
				{
					return false;
				}
				if (fieldMonster.data.monsterClass == Class.Ranged)
				{
					return true;
				}
				int num = ((fieldMonster.Side == ArmySide.Left) ? 1 : (-1));
				return !fieldMonster.parameters.GetMonsters(fieldMonster.Side).ContainsKey(new Vector2(coords.x + (float)num, coords.y));
			}
		}

		public ArmySide Side => _curController.Side;

		public ArmySide EnemySide => _curController.EnemySide;

		public void StopAllActions()
		{
			_breakFlag = true;
		}

		public void BroadcastAction(TriggerType trigger, SkillType origin, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, Action onPerformed)
		{
			_curController.BroadcastAction(trigger, origin, actionPlace, actionParameter, affectedParameter, onPerformed);
		}

		public bool ShouldPerformAction(TriggerType trigger, SkillType originSkill, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, object param = null)
		{
			if (_actions != null)
			{
				List<ActionBit>.Enumerator enumerator = _actions.GetEnumerator();
				try
				{
					while (enumerator.MoveNext())
					{
						if (enumerator.Current != null && enumerator.Current.ShouldPerform(trigger, originSkill, actionPlace, actionParameter, affectedParameter, param))
						{
							enumerator.Dispose();
							return true;
						}
					}
				}
				catch (Exception ex)
				{
					throw new Exception("ShouldPerformAction " + ex);
				}
				finally
				{
					enumerator.Dispose();
				}
			}
			return false;
		}

		private IEnumerable<int> ActionsIterator()
		{
			for (int i = 0; _actions.Count > i; i++)
			{
				yield return i;
			}
		}

		public void PerformAction(TriggerType trigger, SkillType originSkill, Vector2 actionPlace, FieldElement actionParameter, FieldElement affectedParameter, Action onPerformed, Action<Action> deathRecheckDelegate, object param = null)
		{
			bool somethingPerformed = false;
			ActionBit bit;
			int curActionCount;
			Action performAction = delegate
			{
				PreActionAnimation(trigger, delegate
				{
					_iterator.IterateOnActions(ActionsIterator(), delegate(int i, Action action)
					{
						if (i < 0 || i >= _actions.Count)
						{
							action();
						}
						else
						{
							bit = _actions[i];
							if (bit == null)
							{
								action();
							}
							else
							{
								curActionCount = bit.performCount;
								Action onStepPerformed = null;
								Action performStep = delegate
								{
									int num = curActionCount;
									curActionCount = num - 1;
									if (bit.ShouldPerform(trigger, originSkill, actionPlace, actionParameter, affectedParameter, param) && _isStillExist)
									{
										bit.TryPerform(trigger, originSkill, actionPlace, actionParameter, affectedParameter, delegate(bool result, bool shouldRecheckDeath, bool shouldAddCharge)
										{
											if (shouldAddCharge)
											{
												int num2 = curActionCount;
												curActionCount = num2 + 1;
											}
											somethingPerformed = somethingPerformed || result;
											if (shouldRecheckDeath)
											{
												deathRecheckDelegate(onStepPerformed);
											}
											else
											{
												onStepPerformed();
											}
										}, param);
									}
									else
									{
										onStepPerformed();
									}
								};
								onStepPerformed = delegate
								{
									if (curActionCount > 0)
									{
										performStep();
									}
									else
									{
										action();
									}
								};
								performStep();
							}
						}
					}, delegate
					{
						PostActionAnimation(trigger, delegate
						{
							if (!_breakFlag)
							{
								OnTriggerCompleted(onPerformed, trigger, somethingPerformed);
							}
						});
					});
				});
			};
			if (trigger == TriggerType.Attack && CanPerformAttackStart)
			{
				BroadcastAction(TriggerType.AttackStarted, SkillType.NoSkill, coords, this, this, delegate
				{
					BroadcastDeathRecheck(delegate
					{
						performAction();
					});
				});
			}
			else
			{
				performAction();
			}
		}

		protected virtual void PreActionAnimation(TriggerType trigger, Action onAnimation)
		{
			onAnimation();
		}

		protected virtual void PostActionAnimation(TriggerType trigger, Action onAnimation)
		{
			onAnimation();
		}

		public virtual void BroadcastDeathRecheck(Action onCompl)
		{
			_curController.BroadcastDeathRecheck(Side, onCompl);
		}

		public virtual void OnTriggerCompleted(Action onPerformed, TriggerType trigger, bool somethingPerformed)
		{
			onPerformed();
		}
	}
}
