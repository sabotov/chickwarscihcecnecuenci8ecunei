using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public class SimulateEnviroment
	{
		public delegate void OnStepSimulated(Dictionary<ArmySide, SimulateArmy> armies);

		public class SimulateArmy
		{
			public Dictionary<Vector2, FieldMonster> army;

			public FieldMonster warlord;

			public FieldMonster pet;

			public Dictionary<Vector2, FieldRune> runes;

			public AIDecisionMaker decisionMaker;

			public List<MonsterData> hand;

			public List<MonsterData> deck;

			public List<MonsterData> reserveMosnter;

			public int upkeepCount;

			public List<TriggerType> availableSkills;

			public ArmyControllerCore.DrawType drawType;

			public FieldScriptWrapper.StartBattleArmyParams.BattleType battleType;

			public List<Rarity> raritySequence;

			public bool isDefeated => (int)warlord.Health <= 0;

			public override string ToString()
			{
				string text = "";
				if (warlord != null)
				{
					text = string.Concat(text, "WARLORD HP: ", warlord.Health, ". ");
				}
				foreach (KeyValuePair<Vector2, FieldMonster> item in army)
				{
					string text2 = "(" + (int)item.Key.x + ", " + (int)item.Key.y + ")";
					text = text + item.Value.data.NameLog() + "(a" + item.Value.Attack + " h" + (string)item.Value.Health + "), " + text2 + ".";
				}
				return text;
			}
		}

		private SimulateFieldController _controller;

		private Dictionary<ArmySide, SimulateArmy> _lastArmy;

		private Dictionary<ArmySide, AIDecisionMaker> _desisionMakers;

		private CopiedSimulateRandom _randomCopy;

		private OnStepSimulated _onSimulated;

		private static List<SimulateEnviroment> _staticEnvPool = new List<SimulateEnviroment>();

		private bool isAvailable;

		public static SimulateEnviroment Create()
		{
			TestUtilFunctions.OnlyOneThread();
			return new SimulateEnviroment();
		}

		public void Init(Dictionary<ArmySide, SimulateArmy> armies, int width, int height, CopiedSimulateRandom randomCopy, int skillDelay, int skillShift)
		{
			try
			{
				isAvailable = false;
				_lastArmy = null;
				_onSimulated = delegate
				{
					Debug.LogError("_onSimulated not inited");
				};
				_controller = new SimulateFieldController();
				_desisionMakers = new Dictionary<ArmySide, AIDecisionMaker>();
				foreach (KeyValuePair<ArmySide, SimulateArmy> army in armies)
				{
					if (army.Value.decisionMaker != null)
					{
						_desisionMakers.Add(army.Key, army.Value.decisionMaker);
					}
				}
				_controller.Init(armies, width, height, randomCopy, skillDelay, skillShift);
				_randomCopy = randomCopy;
			}
			catch (Exception ex)
			{
				Debug.LogError("SimulateEnviroment Init. " + ex);
			}
		}

		public void SimulateFight(ArmySide stepSide, OnStepSimulated onSimulated, int depth = 1)
		{
			int stepNum = 0;
			_onSimulated = onSimulated;
			Common.VoidDelegate step = delegate
			{
			};
			Common.ResultDelegate changeStep = delegate(bool breakFlag)
			{
				if (breakFlag)
				{
					stepNum = int.MaxValue;
					step();
				}
				else
				{
					stepSide = ((stepSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
					stepNum++;
					step();
				}
			};
			step = delegate
			{
				if (stepNum >= depth)
				{
					OnSimulated();
				}
				else
				{
					_controller.SimulateFightStep(stepSide, changeStep);
				}
			};
			step();
		}

		public void SimulatePlaceAndFight(ArmySide stepSide, OnStepSimulated onSimulated, int depth = 1)
		{
			int stepNum = 0;
			_onSimulated = onSimulated;
			Common.ResultDelegate step = delegate
			{
			};
			Common.ResultDelegate place = delegate
			{
			};
			Common.ResultDelegate prepareStep = delegate
			{
			};
			Common.ResultDelegate changeStep = delegate(bool breakFlag)
			{
				if (breakFlag)
				{
					OnSimulated();
				}
				else
				{
					stepSide = ((stepSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
					stepNum++;
					if (stepNum >= depth)
					{
						OnSimulated();
					}
					else
					{
						prepareStep(result: false);
					}
				}
			};
			prepareStep = delegate(bool breakFlag)
			{
				if (breakFlag)
				{
					OnSimulated();
				}
				else
				{
					_controller.SimulateUpkeep(stepSide, place);
				}
			};
			place = delegate(bool breakFlag)
			{
				if (breakFlag)
				{
					OnSimulated();
				}
				else if (_desisionMakers.ContainsKey(stepSide))
				{
					Dictionary<ArmySide, SimulateArmy> armiesStates = _controller.GetArmiesStates();
					bool flag = false;
					foreach (KeyValuePair<ArmySide, SimulateArmy> item in armiesStates)
					{
						item.Value.decisionMaker = _desisionMakers[item.Key];
						flag = flag || item.Value.isDefeated;
					}
					if (flag)
					{
						OnSimulated();
					}
					else
					{
						CopiedSimulateRandom randomCopy = _randomCopy;
						AIDecisionMaker.DecisionVoidDelegate onChosen = delegate(AIDecision decision)
						{
							if (decision != null)
							{
								switch (decision.decisionType)
								{
								case AIDecision.Type.Monster:
									_controller.SimulatePlacement(stepSide, decision.place, decision.monster, step);
									break;
								case AIDecision.Type.Skill:
									_controller.SimulateSkill(stepSide, decision.skill, step);
									break;
								case AIDecision.Type.SkillAndMonster:
									_controller.SimulateSkillsAndMonsterPlace(stepSide, decision.skills, decision.place, decision.monster, step, null);
									break;
								default:
									throw new Exception(string.Concat("DecisionMaker.DecisionVoidDelegate. decision ", decision, " has wrong type"));
								}
							}
							else
							{
								step(result: false);
							}
						};
						_desisionMakers[stepSide].MakeDecisionInnerNew(depth - 1, armiesStates, randomCopy, onChosen);
					}
				}
				else
				{
					step(result: false);
				}
			};
			step = delegate(bool breakFlag)
			{
				if (breakFlag)
				{
					OnSimulated();
				}
				else
				{
					_controller.SimulateFightStep(stepSide, changeStep);
				}
			};
			step(result: false);
		}

		public void SimulateDecision(ArmySide side, AIDecision decision, OnStepSimulated onSimulated, Action onError)
		{
			switch (decision.decisionType)
			{
			case AIDecision.Type.Monster:
				SimulatePlace(side, decision.place, decision.monster, onSimulated);
				break;
			case AIDecision.Type.Skill:
				SimulateSkill(side, decision.skill, onSimulated);
				break;
			case AIDecision.Type.SkillAndMonster:
				SimulateSkillsAndMonsterPlace(side, decision.skills, decision.place, decision.monster, onSimulated, onError);
				break;
			}
		}

		private void SimulatePlace(ArmySide side, Vector2 place, MonsterData monster, OnStepSimulated onSimulated)
		{
			_onSimulated = onSimulated;
			_controller.SimulatePlacement(side, place, monster, delegate
			{
				OnSimulated();
			});
		}

		private void SimulateSkill(ArmySide side, TriggerType skill, OnStepSimulated onSimulated)
		{
			_onSimulated = onSimulated;
			_controller.SimulateSkill(side, skill, delegate
			{
				OnSimulated();
			});
		}

		private void SimulateSkillsAndMonsterPlace(ArmySide side, List<TriggerType> skills, Vector2 place, MonsterData monster, OnStepSimulated onSimulated, Action onError)
		{
			_onSimulated = onSimulated;
			_controller.SimulateSkillsAndMonsterPlace(side, skills, place, monster, delegate
			{
				OnSimulated();
			}, onError);
		}

		protected void OnSimulated()
		{
			_lastArmy = _controller.GetArmiesStates();
			if (_desisionMakers != null)
			{
				foreach (KeyValuePair<ArmySide, SimulateArmy> item in _lastArmy)
				{
					if (_desisionMakers.ContainsKey(item.Key))
					{
						item.Value.decisionMaker = _desisionMakers[item.Key];
					}
				}
			}
			if (_onSimulated != null)
			{
				_onSimulated(_lastArmy);
				_onSimulated = null;
			}
		}

		public Dictionary<ArmySide, SimulateArmy> GetLastArmy()
		{
			return _lastArmy;
		}
	}
}
