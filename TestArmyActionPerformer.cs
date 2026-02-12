using System;
using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using ServiceLocator;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	public class TestArmyActionPerformer : ArmyActionPerformer
	{
		private AIDecisionMaker _aiDecisionMaker;

		private AIDecisionsCalculator _aiDecisionsCalculator;

		private AIDialogCharacter _aiDialogCharacter;

		private bool afterEnemyTurnFired;

		private float myAfterEnemyTurnProfit;

		private float enemyAfterEnemyTurnProfit;

		private bool afterMyTurnFired;

		private float myAfterMyTurnProfit;

		private float enemyAfterMyTurnProfit;

		public bool isAutoPlay;

		public bool canConcede;

		private List<AIDecision> preCalculatedDecisions;

		private readonly CachedService<ILeagueInfoModule> __leagueInfoModule = new CachedService<ILeagueInfoModule>();

		private Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> curArmies;

		private List<Dictionary<ArmySide, SimulateEnviroment.SimulateArmy>> afterArmies;

		private Action<TriggerType> _onSkill;

		private Action<MonsterData, Vector2> _onChosen;

		private ILeagueInfoModule _leagueInfoModule => __leagueInfoModule.Value;

		public override void Init(Func<List<MonsterData>> enemyHand, Func<List<TriggerType>> enemySkills, ArmyControllerCore thisController, FieldParameters parameters)
		{
			base.Init(enemyHand, enemySkills, thisController, parameters);
			_aiDecisionsCalculator = new AIDecisionsCalculator(_enemyHand, _enemySkills, _thisController, _parameters);
		}

		public void SetAiDecicionMaker(AICharacter aiCharacter)
		{
			if (false)
			{
				aiCharacter.SetIQ(TestUtilFunctions.GetAiIQ());
			}
			int width = _parameters.width;
			int height = _parameters.height;
			_aiDecisionMaker = new AIDecisionMaker(aiCharacter, _side, width, height, (ArmySide x, Dictionary<Vector2, FieldMonster> y, MonsterData z) => _parameters.GetClassedTiles(z.monsterClass, y, x), _parameters.skillDrawDelay, _parameters.skillDrawShift);
			_aiDecisionsCalculator.SetDecisionMaker(_aiDecisionMaker);
			_aiDialogCharacter = aiCharacter.GetRandomDialogCharacter();
		}

		public void SetAiDecicionMaker(int id)
		{
			AICharacter aICharacter = null;
			aICharacter = AICharacterHelper.GetAICharacterById(id);
			SetAiDecicionMaker(aICharacter);
		}

		public void SetAiDecicionMaker(FieldScriptWrapper.StartBattleArmyParams.BattleType battleType)
		{
			AICharacter aICharacter = null;
			aICharacter = ((0 == 0) ? AICharacterHelper.GetAICharacterByBattleType(battleType) : AICharacterHelper.GetAICharacterById(TestUtilFunctions.GetAiCharId()));
			SetAiDecicionMaker(aICharacter);
		}

		public override void PerformPlacingChoose(List<MonsterData> hand, List<TriggerType> skills, Action<MonsterData, Vector2> onChosen, Action<TriggerType> onSkill, bool isAfterSkill = false, int currentTurn = 0)
		{
			_onSkill = onSkill;
			_onChosen = onChosen;
			if (hand.Count == 0)
			{
				_onChosen(null, Vector2.zero);
				return;
			}
			Func<CopiedSimulateRandom> sRandomCopyGetter = GetSRandomCopyGetter();
			AIDecisionsCalculator.DesicionProcessingData data = new AIDecisionsCalculator.DesicionProcessingData(skills, hand, sRandomCopyGetter);
			SimulateEnviroment.OnStepSimulated calculateDecisionDelegate = _aiDecisionsCalculator.GetCalculateDecisionDelegate(OnDecision, data, isAutoPlay);
			_aiDecisionsCalculator.MakePreDecisions(calculateDecisionDelegate, _onChosen, _onSkill, data);
		}

		private void OnDecision(List<AIDecision> decisions)
		{
			AIDecision decision = _aiDecisionMaker.GetDecisionByIQ(decisions);
			((Common.VoidDelegate)delegate
			{
				if (_aiDecisionMaker.Locked())
				{
					Debug.Log("_aiDecisionMaker.Locked");
				}
				else if (decision == null)
				{
					_onChosen(null, Vector2.zero);
				}
				else
				{
					bool flag = false;
					if (decision.isDefeat)
					{
						bool flag2 = Common.IsChance(_aiDialogCharacter.concedeChance);
						bool flag3 = Constants.turns_ai_cant_concede < _thisController.GetUpkeepCount();
						flag = canConcede && flag2 && flag3;
						if (flag)
						{
							FieldScriptWrapper.instance.testingFieldController.InformDefeat(_side, delay: true);
						}
					}
					if (!flag)
					{
						switch (decision.decisionType)
						{
						case AIDecision.Type.Monster:
							_onChosen(decision.monster, decision.place);
							break;
						case AIDecision.Type.Skill:
							_onSkill(decision.skill);
							break;
						case AIDecision.Type.SkillAndMonster:
							if (decision.skills.Count > 0)
							{
								_aiDecisionsCalculator.skillsAndMonsterDec = decision;
								TriggerType obj = _aiDecisionsCalculator.skillsAndMonsterDec.skills[0];
								_aiDecisionsCalculator.skillsAndMonsterDec.skills.RemoveAt(0);
								_onSkill(obj);
							}
							else
							{
								_aiDecisionsCalculator.skillsAndMonsterDec = null;
								_onChosen(decision.monster, decision.place);
							}
							break;
						default:
							throw new Exception(string.Concat("PerformPlacingChoose. ", decision, ". wrong type"));
						}
					}
				}
			})();
		}

		public override void InformHandCreated(List<MonsterData> cards, List<TriggerType> skills)
		{
			base.InformHandCreated(cards, skills);
			if (_thisController.GetDrawType() != ArmyControllerCore.DrawType.NewSurvival)
			{
				_thisController.DrawCards(_leagueInfoModule.CurLeagueAIExtraCards, delegate
				{
				}, silent: true);
			}
		}

		public override void InformStop()
		{
			_aiDecisionMaker.Lock();
		}
	}
}
