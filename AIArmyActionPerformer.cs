using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using DG.Tweening;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using ServiceLocator;
using UI_Scripts.WindowManager;
using UnityEngine;
using UserData;
using UtilScripts;

namespace BattlefieldScripts
{
	internal class AIArmyActionPerformer : ArmyActionPerformer
	{
		[Serializable]
		[CompilerGenerated]
		private sealed class _003C_003Ec
		{
			public static readonly _003C_003Ec _003C_003E9 = new _003C_003Ec();

			public static AIDecisionMaker.DecisionListVoidDelegate _003C_003E9__34_0;

			public static Converter<MonsterData, MonsterData> _003C_003E9__34_1;

			public static Converter<TriggerType, TriggerType> _003C_003E9__34_2;

			public static Converter<AIDecision, AIDecision> _003C_003E9__34_3;

			public static Action<MonsterData, Vector2> _003C_003E9__34_5;

			public static Action _003C_003E9__45_0;

			internal void _003CTestCalcDecision_003Eb__34_0(List<AIDecision> afterFightDecisions)
			{
				string text = "";
				foreach (AIDecision afterFightDecision in afterFightDecisions)
				{
					text = string.Concat(text, afterFightDecision, ". ", afterFightDecision.infoString, "\n");
				}
				UnityEngine.Debug.LogError(text);
			}

			internal MonsterData _003CTestCalcDecision_003Eb__34_1(MonsterData card)
			{
				return card;
			}

			internal TriggerType _003CTestCalcDecision_003Eb__34_2(TriggerType skill)
			{
				return skill;
			}

			internal AIDecision _003CTestCalcDecision_003Eb__34_3(AIDecision de)
			{
				return de.Clone();
			}

			internal void _003CTestCalcDecision_003Eb__34_5(MonsterData monster, Vector2 palce)
			{
				UnityEngine.Debug.LogError("onChosen");
			}

			internal void _003CInformHandCreated_003Eb__45_0()
			{
			}
		}

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		private AIDecisionMaker _aiDecisionMaker;

		private AIDecisionsCalculator _aiDecisionsCalculator;

		private AIDialogCharacter _aiDialogCharacter;

		private float myLastProfit;

		private float enemyLastProfit;

		private bool afterEnemyTurnFired;

		private float myAfterEnemyTurnProfit;

		private float enemyAfterEnemyTurnProfit;

		private bool afterMyTurnFired;

		private float myAfterMyTurnProfit;

		private float enemyAfterMyTurnProfit;

		public bool waitTimeBeforePlacement;

		public bool canConcede;

		public bool isAutoPlay;

		private List<AIDialogTrigger.TriggerType> answeredTriggres = new List<AIDialogTrigger.TriggerType>();

		private bool _breakFlag;

		private bool _animationsBlocked;

		private List<AIDecision> preCalculatedDecisions;

		private Stopwatch decisionTimer;

		private IEnumerator _checkDecisionEnum;

		private readonly CachedService<ILeagueInfoModule> __leagueInfoModule = new CachedService<ILeagueInfoModule>();

		private Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> curArmies;

		private List<Dictionary<ArmySide, SimulateEnviroment.SimulateArmy>> afterArmies;

		private Action<TriggerType> _onSkill;

		private Action<MonsterData, Vector2> _onChosen;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		private ILeagueInfoModule _leagueInfoModule => __leagueInfoModule.Value;

		public static event Common.VoidDelegate TestCalcDecisionEvent;

		public static void FireTestCalcDecisionEvent()
		{
			if (AIArmyActionPerformer.TestCalcDecisionEvent != null)
			{
				AIArmyActionPerformer.TestCalcDecisionEvent();
			}
		}

		public override void Init(Func<List<MonsterData>> enemyHand, Func<List<TriggerType>> enemySkills, ArmyControllerCore thisController, FieldParameters parameters)
		{
			base.Init(enemyHand, enemySkills, thisController, parameters);
			_aiDecisionsCalculator = new AIDecisionsCalculator(_enemyHand, _enemySkills, _thisController, _parameters);
			if (!isAutoPlay)
			{
				AIArmyActionPerformer.TestCalcDecisionEvent = TestCalcDecision;
			}
		}

		public void BlockAnimation(bool blocked)
		{
			_animationsBlocked = blocked;
		}

		private void TestCalcDecision()
		{
			if (_003C_003Ec._003C_003E9__34_0 == null)
			{
				_003C_003Ec._003C_003E9__34_0 = delegate(List<AIDecision> afterFightDecisions)
				{
					string text = "";
					foreach (AIDecision afterFightDecision in afterFightDecisions)
					{
						text = string.Concat(text, afterFightDecision, ". ", afterFightDecision.infoString, "\n");
					}
					UnityEngine.Debug.LogError(text);
				};
			}
			Func<CopiedSimulateRandom> sRandomCopyGetter = GetSRandomCopyGetter();
			List<MonsterData> mHand = _thisController.GetHand().ConvertAll((MonsterData card) => card);
			List<TriggerType> mySkills = _thisController.GetSkills().ConvertAll((TriggerType skill) => skill);
			_enemyHand();
			List<AIDecision> prevCalc = preCalculatedDecisions.ConvertAll((AIDecision de) => de.Clone());
			SimulateEnviroment.OnStepSimulated calculateDecision = delegate
			{
				UnityEngine.Debug.LogError("OnCalc");
				preCalculatedDecisions = prevCalc;
			};
			Action<MonsterData, Vector2> onChosen = delegate
			{
				UnityEngine.Debug.LogError("onChosen");
			};
			_aiDecisionsCalculator.MakePreDecisions(calculateDecision, onChosen, UseSkill, new AIDecisionsCalculator.DesicionProcessingData(mySkills, mHand, sRandomCopyGetter));
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
			WindowScriptCore<BattlefieldWindow>.instance.SetAiName(aiCharacter.patternName + ", IQ: " + aiCharacter.IQ);
		}

		public void SetAiDecicionMaker(int id)
		{
			AICharacter aICharacter = null;
			if (false)
			{
				aICharacter = AICharacterHelper.GetAICharacterById(TestUtilFunctions.GetAiCharId());
			}
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
			decisionTimer = new Stopwatch();
			decisionTimer.Start();
			if (Constants.stop_ai_decision_enabled)
			{
				if (_checkDecisionEnum != null)
				{
					UnityEngine.Debug.LogError("AI STOPED COROUTINE BEFORE START");
					_delayedActionsHandler.StopCoroutine(_checkDecisionEnum);
				}
				UnityEngine.Debug.LogError("AI START COROUTINE");
				_checkDecisionEnum = CheckDecisionDuration();
				_delayedActionsHandler.StartCoroutine(_checkDecisionEnum);
			}
			Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> simulateArmies = GetSimulateArmies();
			Func<CopiedSimulateRandom> sRandomCopyGetter = GetSRandomCopyGetter();
			TrySpeak(AIDialogTrigger.TriggerType.AfterEnemyTurn, simulateArmies);
			AIDecisionsCalculator.DesicionProcessingData data = new AIDecisionsCalculator.DesicionProcessingData(skills, hand, sRandomCopyGetter);
			bool isFastDecision = FieldScriptWrapper.currentBattleType == FieldScriptWrapper.StartBattleArmyParams.BattleType.GoldMine;
			SimulateEnviroment.OnStepSimulated calculateDecisionDelegate = _aiDecisionsCalculator.GetCalculateDecisionDelegate(OnDecision, data, isAutoPlay, isFastDecision);
			_aiDecisionsCalculator.MakePreDecisions(calculateDecisionDelegate, _onChosen, UseSkill, data);
		}

		private void OnDecision(List<AIDecision> decisions)
		{
			AIDecision decision = _aiDecisionMaker.GetDecisionByIQ(decisions);
			Common.VoidDelegate voidDelegate = delegate
			{
				if (!_aiDecisionMaker.Locked())
				{
					if (decision == null)
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
								FieldScriptWrapper.instance.fieldController.InformDefeat(_side, delay: true);
								FieldScriptWrapper.instance.fieldController.RequestFastDialog(FastDialogData.Event.BattleSurrender, _side);
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
								UseSkill(decision.skill);
								break;
							case AIDecision.Type.SkillAndMonster:
								if (decision.skills.Count > 0)
								{
									_aiDecisionsCalculator.skillsAndMonsterDec = decision;
									TriggerType skill = _aiDecisionsCalculator.skillsAndMonsterDec.skills[0];
									_aiDecisionsCalculator.skillsAndMonsterDec.skills.RemoveAt(0);
									UseSkill(skill);
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
				}
			};
			decisionTimer.Stop();
			float num = (float)decisionTimer.Elapsed.TotalSeconds;
			if (_checkDecisionEnum != null)
			{
				_delayedActionsHandler.StopCoroutine(_checkDecisionEnum);
			}
			float num2 = ((FieldScriptWrapper.currentBattleType != FieldScriptWrapper.StartBattleArmyParams.BattleType.PvP && FieldScriptWrapper.currentBattleType != FieldScriptWrapper.StartBattleArmyParams.BattleType.Survival && FieldScriptWrapper.currentBattleType != FieldScriptWrapper.StartBattleArmyParams.BattleType.PvPBrawl) ? (Constants.enemy_turn_pause + Common.GetRandomFloat(0f, Constants.enemy_turn_pause_delta)) : (Constants.enemy_turn_pause_pvp + Common.GetRandomFloat(0f, Constants.enemy_turn_pause_delta_pvp)));
			if (num >= num2 || !waitTimeBeforePlacement)
			{
				voidDelegate();
				return;
			}
			float time = num2 - num;
			_delayedActionsHandler.WaitForProcedure(time, voidDelegate);
		}

		private void UseSkill(TriggerType skill)
		{
			if (_animationsBlocked)
			{
				_onSkill(skill);
			}
			else
			{
				AnimateSkillSelected(skill);
			}
		}

		private void AnimateSkillSelected(TriggerType trigger)
		{
			if (_onSkill == null)
			{
				return;
			}
			SkillCard skillCard = null;
			try
			{
				skillCard = SkillCard.CreateCard(_parameters.GetWarlord(_side).VisualMonster.transform.parent);
				skillCard.transform.localScale = new Vector3(0f, 0f, 1f);
				skillCard.transform.localPosition = _parameters.GetWarlord(_side).VisualMonster.transform.localPosition;
				if (trigger == TriggerType.WarlordSkillSpecial)
				{
					foreach (ActionBitSignature skill in _parameters.GetWarlord(_side).Skills)
					{
						if (skill.trigger == trigger)
						{
							SkillStaticData skillByName = SkillDataHelper.GetSkillByName(skill.skillId);
							string strValue = skill.strValue;
							skillCard.Init(skillByName, strValue, 0f, 0, withoutAnim: true);
							break;
						}
					}
				}
				else
				{
					for (int i = 0; i < _parameters.GetWarlord(_side).data.skills.Count && i < _parameters.GetWarlord(_side).data.skillValues.Count; i++)
					{
						if (_parameters.GetWarlord(_side).data.skills[i].trigger == trigger)
						{
							string value = GetValue(_parameters.GetWarlord(_side).data.skillValues[i]);
							skillCard.Init(_parameters.GetWarlord(_side).data.skills[i], value, 0f, 0, withoutAnim: true);
							break;
						}
					}
				}
				float step = 0f;
				float scaleMult = 1.7f;
				float xPositionStart = skillCard.transform.position.x;
				float shift = ((_side == ArmySide.Left) ? 1 : (-1)) * 200;
				DOTween.To(() => step, delegate(float x)
				{
					step = x;
					skillCard.transform.localScale = new Vector3(step * scaleMult, step * scaleMult, 1f);
					skillCard.transform.position = new Vector3(xPositionStart + shift * step, skillCard.transform.position.y, skillCard.transform.position.z);
				}, 1f, TimeDebugController.instance.skillCardUsageTweenTime * TimeDebugController.totalTimeMultiplier).OnComplete(delegate
				{
					if (!_breakFlag)
					{
						_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.skillCardUsageTweenDelay / TimeDebugController.totalTimeMultiplier, delegate
						{
							if (_onSkill == null)
							{
								if (skillCard != null)
								{
									skillCard.Destroy();
								}
								UnityEngine.Debug.LogError("AnimateSkillSelected. _onSkill is NULL");
							}
							else if (skillCard == null)
							{
								_onSkill(trigger);
								_onSkill = null;
								UnityEngine.Debug.LogError("AnimateSkillSelected. skillCard is NULL");
							}
							else
							{
								skillCard.TweenAlpha(0f, 0.2f);
								_delayedActionsHandler.WaitForProcedure(0.5f, delegate
								{
									if (skillCard == null)
									{
										UnityEngine.Debug.LogError("AnimateSkillSelected. TryingToDestroy skillCard is NULL");
									}
									else
									{
										skillCard.Destroy();
									}
								});
								_onSkill(trigger);
								_onSkill = null;
							}
						});
					}
					else
					{
						skillCard.Destroy();
					}
				}).SetEase(TimeDebugController.instance.skillCardUsageEase);
			}
			catch (Exception e)
			{
				_onSkill(trigger);
				_onSkill = null;
				if (skillCard != null)
				{
					skillCard.Destroy();
				}
				FirebaseManager.CrashlyticsLogException(e);
			}
		}

		public override void InformTurnEnded()
		{
			base.InformTurnEnded();
			answeredTriggres = new List<AIDialogTrigger.TriggerType>();
			TrySpeak(AIDialogTrigger.TriggerType.AfterMyTurn);
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
			Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> simulateArmies = GetSimulateArmies();
			ArmySide side = _side.OtherSide();
			myLastProfit = _aiDecisionMaker.SituationAnalyzer.GetProfitForCurrentSituation(simulateArmies, simulateArmies, _side, new CopiedSimulateRandom());
			enemyLastProfit = _aiDecisionMaker.SituationAnalyzer.GetProfitForCurrentSituation(simulateArmies, simulateArmies, side, new CopiedSimulateRandom());
		}

		private Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> GetSimulateArmies()
		{
			return _thisController.GetArmiesStates();
		}

		public override void InformStop()
		{
			_breakFlag = true;
			_aiDecisionMaker.Lock();
		}

		private IEnumerator CheckDecisionDuration()
		{
			float time = 0f;
			while (_thisController.CanPlace())
			{
				if (time >= (float)Constants.max_ai_decision_duration)
				{
					StringBuilder stringBuilder = new StringBuilder();
					stringBuilder.AppendLine("Can place: " + _thisController.CanPlace()).AppendLine("Time: " + time).AppendLine("max_ai_decision_duration: " + Constants.max_ai_decision_duration);
					UnityEngine.Debug.LogError(stringBuilder.ToString());
					FieldScriptWrapper.instance.fieldController.InformDefeat(_side, delay: true, isTie: true);
					FieldScriptWrapper.instance.fieldController.RequestFastDialog(FastDialogData.Event.BattleSurrender, _side);
					break;
				}
				time += Time.deltaTime;
				yield return null;
			}
		}

		public override void InformEnemySpeech(FastDialogData.Event trigger)
		{
			if (AIDialogCharacter.CanAnswer(trigger))
			{
				AIDialogTrigger.TriggerType triggerType = AIDialogCharacter.TriggerTypeByPhraseEvent(trigger);
				if (!answeredTriggres.Contains(triggerType))
				{
					TrySpeak(triggerType);
				}
			}
		}

		private void TrySpeak(AIDialogTrigger.TriggerType trigger)
		{
			if (!_aiDecisionMaker.Locked())
			{
				Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> simulateArmies = GetSimulateArmies();
				TrySpeak(trigger, simulateArmies);
			}
		}

		private void TrySpeak(AIDialogTrigger.TriggerType trigger, Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armies)
		{
			bool flag = ParamsModule.Inited && ParamsModule.instance.CommentsEnabled;
			if (_aiDecisionMaker.Locked())
			{
				return;
			}
			ArmySide side = _side.OtherSide();
			float profitForCurrentSituation = _aiDecisionMaker.SituationAnalyzer.GetProfitForCurrentSituation(armies, armies, _side, new CopiedSimulateRandom());
			float profitForCurrentSituation2 = _aiDecisionMaker.SituationAnalyzer.GetProfitForCurrentSituation(armies, armies, side, new CopiedSimulateRandom());
			FastDialogData.Event phraseEvent = FastDialogData.Event.Oops;
			float num = 0f;
			float num2 = 0f;
			bool flag2 = true;
			switch (trigger)
			{
			case AIDialogTrigger.TriggerType.AfterEnemyTurn:
				num = myAfterMyTurnProfit;
				num2 = enemyAfterMyTurnProfit;
				myLastProfit = myAfterMyTurnProfit;
				enemyLastProfit = enemyAfterMyTurnProfit;
				myAfterEnemyTurnProfit = profitForCurrentSituation;
				enemyAfterEnemyTurnProfit = profitForCurrentSituation2;
				flag2 = afterMyTurnFired;
				afterEnemyTurnFired = true;
				break;
			case AIDialogTrigger.TriggerType.AfterMyTurn:
				num = myAfterEnemyTurnProfit;
				num2 = enemyAfterEnemyTurnProfit;
				myLastProfit = myAfterEnemyTurnProfit;
				enemyLastProfit = enemyAfterEnemyTurnProfit;
				myAfterMyTurnProfit = profitForCurrentSituation;
				enemyAfterMyTurnProfit = profitForCurrentSituation2;
				flag2 = afterEnemyTurnFired;
				afterMyTurnFired = true;
				break;
			default:
				num = myLastProfit;
				num2 = enemyLastProfit;
				break;
			}
			if (flag2)
			{
				List<MonsterData> startDeck = GetStartDeck();
				float num3 = (UserArmyModule.inited ? UserArmyModule.MaxCardsInDeckAvailable : 8);
				float num4 = num3 / (float)startDeck.Count;
				float myPower = MonsterDataUtils.GetPowerForAI(startDeck) * num4;
				List<MonsterData> enemyGetStartDeck = GetEnemyGetStartDeck();
				float num5 = num3 / (float)enemyGetStartDeck.Count;
				float enemyPower = MonsterDataUtils.GetPowerForAI(enemyGetStartDeck) * num5;
				AIDialogModifier.ModifierType modifierByProfits = _aiDialogCharacter.GetModifierByProfits(trigger, profitForCurrentSituation, num, profitForCurrentSituation2, num2, myPower, enemyPower);
				if (_aiDialogCharacter.GetPhraseBySituation(trigger, modifierByProfits, out phraseEvent) && flag)
				{
					float randomFloat = Common.GetRandomFloat(Constants.enemy_response_pause, Constants.enemy_response_pause + Constants.enemy_response_delta);
					answeredTriggres.Add(trigger);
					Speak(phraseEvent, randomFloat);
				}
			}
		}

		private void Speak(FastDialogData.Event phraseEvent, float delay)
		{
			_delayedActionsHandler.WaitForProcedure(delay, delegate
			{
				FieldScriptWrapper.instance.fieldController.RequestFastDialog(phraseEvent, _side);
			});
		}
	}
}
