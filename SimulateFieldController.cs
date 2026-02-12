using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public class SimulateFieldController : FieldControllerCore
	{
		private Common.ResultDelegate _onSimulated;

		private const bool BroadcastDeathOnPlace = true;

		public void Init(Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armies, int width, int height, CopiedSimulateRandom randomCopy, int skillDelay, int skillShift)
		{
			SimulateArmyController simulateArmyController = new SimulateArmyController(ArmySide.Left);
			SimulateArmyController simulateArmyController2 = new SimulateArmyController(ArmySide.Right);
			SimulateEnviroment.SimulateArmy data = armies[ArmySide.Left];
			SimulateEnviroment.SimulateArmy data2 = armies[ArmySide.Right];
			FieldParameters fieldParameters = new FieldParameters();
			fieldParameters.Init(width, height);
			fieldParameters.AttachControllers(simulateArmyController, simulateArmyController2);
			fieldParameters.InitSkillStuff(skillDelay, skillShift);
			SimulateActionPerformer simulateActionPerformer = new SimulateActionPerformer();
			simulateActionPerformer.Init(simulateArmyController.GetHand, simulateArmyController.GetSkills, simulateArmyController2, fieldParameters);
			SimulateActionPerformer simulateActionPerformer2 = new SimulateActionPerformer();
			simulateActionPerformer2.Init(simulateArmyController2.GetHand, simulateArmyController2.GetSkills, simulateArmyController, fieldParameters);
			simulateArmyController.Init(this, simulateActionPerformer2, fieldParameters, randomCopy, new SimulateIterator(), data);
			simulateArmyController2.Init(this, simulateActionPerformer, fieldParameters, randomCopy, new SimulateIterator(), data2);
			simulateArmyController.CreateArmy();
			simulateArmyController2.CreateArmy();
			_armies = new Dictionary<ArmySide, ArmyControllerCore>
			{
				{
					ArmySide.Left,
					simulateArmyController
				},
				{
					ArmySide.Right,
					simulateArmyController2
				}
			};
		}

		protected override void BreakStack(Common.VoidDelegate del)
		{
			del();
		}

		public void SimulateFightStep(ArmySide side, Common.ResultDelegate onFinished)
		{
			_onSimulated = onFinished;
			_armies[side].PerformActionPhase(delegate
			{
				if ((int)_armies[ArmySide.Left].GetWarlord().Health <= 0 || (int)_armies[ArmySide.Right].GetWarlord().Health <= 0)
				{
					OnSimulateCompleted(defeat: true);
				}
				else
				{
					_armies[side].PerformEndingPhase(delegate
					{
						bool defeat = (int)_armies[ArmySide.Left].GetWarlord().Health <= 0 || (int)_armies[ArmySide.Right].GetWarlord().Health <= 0;
						OnSimulateCompleted(defeat);
					});
				}
			});
		}

		public void SimulateUpkeep(ArmySide side, Common.ResultDelegate onFinished)
		{
			_onSimulated = onFinished;
			int num = 4 - _armies[side].GetHand().Count;
			_armies[side].DrawCards(num, delegate
			{
			}, silent: true);
			_armies[side].PerformUpkeepPhase(delegate
			{
				bool defeat = (int)_armies[ArmySide.Left].GetWarlord().Health <= 0 || (int)_armies[ArmySide.Right].GetWarlord().Health <= 0;
				OnSimulateCompleted(defeat);
			});
		}

		public void SimulatePlacement(ArmySide side, Vector2 place, MonsterData monster, Common.ResultDelegate onSimulated)
		{
			_onSimulated = onSimulated;
			_armies[side].PlaceMonster(monster, place, delegate
			{
				bool defeated = (int)_armies[ArmySide.Left].GetWarlord().Health <= 0 || (int)_armies[ArmySide.Right].GetWarlord().Health <= 0;
				BroadcastDeathRecheck(side, delegate
				{
					BroadcastDeathRecheck(_armies[side].EnemySide, delegate
					{
						OnSimulateCompleted(defeated);
					});
				});
			});
		}

		public void SimulateSkill(ArmySide side, TriggerType skill, Common.ResultDelegate onSimulated)
		{
			_onSimulated = onSimulated;
			_armies[side].PerformTrigger(skill, SkillType.NoSkill, Vector2.zero, null, null, delegate
			{
				bool defeat = (int)_armies[ArmySide.Left].GetWarlord().Health <= 0 || (int)_armies[ArmySide.Right].GetWarlord().Health <= 0;
				OnSimulateCompleted(defeat);
			}, checkDeath: true);
		}

		public void SimulateSkillsAndMonsterPlace(ArmySide side, List<TriggerType> skills, Vector2 place, MonsterData monster, Common.ResultDelegate onSimulated, Action onError)
		{
			List<TriggerType> copySkills = skills.ConvertAll((TriggerType s) => s);
			if (copySkills.Count == 0)
			{
				if (!_armies[side].CanPlaceMonster(place))
				{
					onError?.Invoke();
				}
				else
				{
					SimulatePlacement(side, place, monster, onSimulated);
				}
				return;
			}
			Common.ResultDelegate onSimulated2 = delegate
			{
				copySkills.RemoveAt(0);
				SimulateSkillsAndMonsterPlace(side, copySkills, place, monster, onSimulated, onError);
			};
			SimulateSkill(side, copySkills[0], onSimulated2);
		}

		public void SimulateSkillsList(ArmySide side, List<TriggerType> skills, Common.ResultDelegate onSimulated)
		{
			List<TriggerType> copySkills = skills.ConvertAll((TriggerType s) => s);
			TriggerType skill = copySkills[0];
			copySkills.RemoveAt(0);
			Common.ResultDelegate resultDelegate = null;
			SimulateSkill(onSimulated: (copySkills.Count != 0) ? ((Common.ResultDelegate)delegate
			{
				SimulateSkillsList(side, copySkills, onSimulated);
			}) : ((Common.ResultDelegate)delegate(bool result)
			{
				onSimulated(result);
			}), side: side, skill: skill);
		}

		private void OnSimulateCompleted(bool defeat)
		{
			if (_onSimulated != null)
			{
				_onSimulated(defeat);
				_onSimulated = null;
			}
		}

		public override void InformDefeat(ArmySide defeatedSide, bool delay = false, bool isAborted = false)
		{
			base.InformDefeat(defeatedSide, delay, isAborted);
			OnSimulateCompleted(defeat: true);
		}
	}
}
