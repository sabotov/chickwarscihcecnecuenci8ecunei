using System;
using System.Collections.Generic;
using ActionBehaviours;
using Assets.Scripts.DataClasses.UserData;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MapData;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using PnkClient;
using PromoteData;
using ServiceLocator;
using Tutorial;
using UI_Scripts.WindowManager;
using UnityEngine;
using UserData;
using UtilScripts;

namespace BattlefieldScripts
{
	public class FieldController : FieldControllerCore
	{
		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		public const int FIELD_WIDTH = 6;

		public const int FIELD_HEIGHT = 4;

		private FieldCreator _creator;

		protected LevelData _curLevel;

		private int _curTurn;

		private readonly Dictionary<ArmySide, BubbleElement> _fastBubble = new Dictionary<ArmySide, BubbleElement>
		{
			{
				ArmySide.Left,
				null
			},
			{
				ArmySide.Right,
				null
			}
		};

		private Action<FieldScriptWrapper.CompleteFightParams> _onVictory;

		private Action<FieldScriptWrapper.CompleteFightParams> _onDefeat;

		private Action<FieldScriptWrapper.CompleteFightParams> _onTie;

		private Action<FieldScriptWrapper.CompleteFightParams> _onCompletedBack;

		private LevelCompletedWindow.HintType _hint;

		private bool _forceRewards;

		private Func<List<LevelReward>> _rewardsDelegate;

		private Func<List<LevelReward>> _defeatRewardDelegate;

		private FieldRandom _random;

		private bool _showDialogs;

		private bool _fastEnding;

		private bool _firstTurnLock;

		private string _curDecor = "";

		private FieldScriptWrapper.StartBattleArmyParams.BattleType _battleType;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public bool IsDialoging { get; private set; }

		private bool CanApplyReward => FieldScriptWrapper.GetBattleType() != FieldScriptWrapper.StartBattleArmyParams.BattleType.Pit;

		public bool CanUseShuffle => _armies[_curActiveArmy].PlayerCanUseShuffle;

		public int UseShuffleCount => _armies[_curActiveArmy].UseShuffleCount;

		public void UpdateBoostData(MonsterData data)
		{
			foreach (FieldMonster value in _armies[ArmySide.Left].GetFieldMonsters().Values)
			{
				if (value.data.monster_id == data.monster_id)
				{
					PromoteUpgradeData currentTotalPromoteUpgrade = data.GetCurrentTotalPromoteUpgrade();
					int num = -1;
					switch (currentTotalPromoteUpgrade.promoteStat.StatType())
					{
					case PromoteStatType.Attack:
						num = currentTotalPromoteUpgrade.promoteStat.MainValue();
						value.HandleParamsChanged(ParamType.Attack, num);
						break;
					case PromoteStatType.HP:
						num = currentTotalPromoteUpgrade.promoteStat.MainValue();
						value.UpdateMaxHealth();
						value.HandleParamsChanged(ParamType.Health, num);
						break;
					case PromoteStatType.Skill:
						num = currentTotalPromoteUpgrade.promoteStat.MainValue();
						value.AddSkill(SkillDataHelper.GetSkillByName(currentTotalPromoteUpgrade.promoteStat.SecondValue()), num.ToString());
						break;
					case PromoteStatType.Evolve:
						value.HandleEvolveBoost(currentTotalPromoteUpgrade.promoteStat.MainValue());
						break;
					}
				}
			}
		}

		public void Init(FieldCreator creator)
		{
			_creator = creator;
			_showDialogs = true;
			MonsterActionListener.RegisterBehaviour(MonsterActionBehaviour<FastDialogBehaviour>.instance);
		}

		public void InitDecor(string decor)
		{
			_curDecor = decor;
			_creator.InitDecor(_curDecor);
			FieldScriptWrapper.instance.CreateEffect(MyDecorDataHelper.GetEffectByName(_curDecor));
		}

		protected virtual void CreateArmyControllers(out ArmyControllerCore leftArmy, out ArmyControllerCore rightArmy, ArmyData playerArmy, ArmyData playerArmy2, List<MonsterData> leftOrderedDeck, List<MonsterData> rightOrderedDeck, FieldScriptWrapper.StartBattleArmyParams.ControllerType playerController, FieldScriptWrapper.StartBattleArmyParams.ControllerType enemyController, FieldScriptWrapper.StartBattleParameters parameters)
		{
			GetArmyData(parameters, playerArmy, playerArmy2, out var army, out var army2);
			leftArmy = new ArmyController(ArmySide.Left);
			rightArmy = new ArmyController(ArmySide.Right);
			FieldParameters fieldParameters = new FieldParameters();
			fieldParameters.Init(_creator.GetFieldWidth(), _creator.GetFieldHeight());
			fieldParameters.AttachControllers(leftArmy, rightArmy);
			fieldParameters.InitSkillStuff(parameters.skillDelay, parameters.skillShift);
			ArmyActionPerformer performerRight = GetPerformerRight(enemyController, leftArmy, rightArmy, fieldParameters, parameters);
			ArmyActionPerformer performerLeft = GetPerformerLeft(playerController, leftArmy, rightArmy, fieldParameters, parameters);
			_random = new RecordingRandom();
			(leftArmy as ArmyController).Init(_creator, this, performerLeft, fieldParameters, army, _random, new AnimatedIterator(), parameters.drawType, _battleType);
			(rightArmy as ArmyController).Init(_creator, this, performerRight, fieldParameters, army2, _random, new AnimatedIterator(), parameters.drawType, _battleType);
			if (parameters.drawType == ArmyControllerCore.DrawType.NewSurvival)
			{
				leftArmy.AttachRaritySequence(parameters.raritySequence);
				rightArmy.AttachRaritySequence(parameters.raritySequence);
			}
			if (leftOrderedDeck != null && leftOrderedDeck.Count > 0)
			{
				leftArmy.AttachOrderedDeck(leftOrderedDeck);
			}
			if (rightOrderedDeck != null && rightOrderedDeck.Count > 0)
			{
				rightArmy.AttachOrderedDeck(rightOrderedDeck);
			}
			leftArmy.CreateArmy();
			rightArmy.CreateArmy();
		}

		protected virtual void GetArmyData(FieldScriptWrapper.StartBattleParameters parameters, ArmyData playerArmy, ArmyData playerArmy2, out ArmyData army1, out ArmyData army2)
		{
			army1 = playerArmy;
			army2 = playerArmy2;
			if (parameters.surroundLevel == null || !parameters.applySurroundLevel)
			{
				return;
			}
			TryFlipStartArmyData(parameters, out var leftData, out var rightData);
			if (parameters.battleType == FieldScriptWrapper.StartBattleArmyParams.BattleType.TestAI)
			{
				return;
			}
			Dictionary<int, int> dictionary = new Dictionary<int, int>();
			List<int> list = new List<int> { 0, 1, 2, 3 };
			for (int i = 0; i < 4; i++)
			{
				int num = list[Common.GetRandomInt(0, list.Count)];
				list.Remove(num);
				dictionary.Add(i, num);
			}
			army1 = new ArmyData
			{
				warlord = playerArmy.warlord,
				warlordData = playerArmy.warlordData,
				petData = (Constants.show_pets ? playerArmy.petData : null),
				deck = playerArmy.deck,
				handDraw = playerArmy.handDraw
			};
			if (leftData != null)
			{
				army1.fieldMonsters = new Dictionary<Place, MonsterData>();
				foreach (KeyValuePair<Place, MonsterData> fieldMonster in leftData.fieldMonsters)
				{
					army1.fieldMonsters.Add(new Place
					{
						x = fieldMonster.Key.x,
						y = dictionary[fieldMonster.Key.y]
					}, fieldMonster.Value);
				}
				army1.runes = new Dictionary<Place, RuneData>();
				foreach (KeyValuePair<Place, RuneData> rune in leftData.runes)
				{
					army1.runes.Add(new Place
					{
						x = rune.Key.x,
						y = dictionary[rune.Key.y]
					}, rune.Value);
				}
			}
			else
			{
				army1.fieldMonsters = playerArmy.fieldMonsters;
				army1.runes = playerArmy.runes;
			}
			army2 = new ArmyData
			{
				warlord = playerArmy2.warlord,
				warlordData = playerArmy2.warlordData,
				petData = (Constants.show_pets ? playerArmy2.petData : null),
				deck = playerArmy2.deck,
				handDraw = playerArmy2.handDraw
			};
			if (rightData != null)
			{
				army2.fieldMonsters = new Dictionary<Place, MonsterData>();
				foreach (KeyValuePair<Place, MonsterData> fieldMonster2 in rightData.fieldMonsters)
				{
					army2.fieldMonsters.Add(new Place
					{
						x = fieldMonster2.Key.x,
						y = dictionary[fieldMonster2.Key.y]
					}, fieldMonster2.Value);
				}
				army2.runes = new Dictionary<Place, RuneData>();
				{
					foreach (KeyValuePair<Place, RuneData> rune2 in rightData.runes)
					{
						army2.runes.Add(new Place
						{
							x = rune2.Key.x,
							y = dictionary[rune2.Key.y]
						}, rune2.Value);
					}
					return;
				}
			}
			army2.fieldMonsters = playerArmy2.fieldMonsters;
			army2.runes = playerArmy2.runes;
		}

		protected virtual void TryFlipStartArmyData(FieldScriptWrapper.StartBattleParameters parameters, out ArmyData leftData, out ArmyData rightData)
		{
			leftData = parameters.surroundLevel.userArmy;
			rightData = parameters.surroundLevel.enemyArmy;
			if (!parameters.flipStartData)
			{
				return;
			}
			leftData = new ArmyData();
			rightData = new ArmyData();
			leftData.fieldMonsters = new Dictionary<Place, MonsterData>();
			foreach (KeyValuePair<Place, MonsterData> fieldMonster in parameters.surroundLevel.enemyArmy.fieldMonsters)
			{
				leftData.fieldMonsters.Add(new Place
				{
					x = 5 - fieldMonster.Key.x,
					y = fieldMonster.Key.y
				}, fieldMonster.Value);
			}
			rightData.fieldMonsters = new Dictionary<Place, MonsterData>();
			foreach (KeyValuePair<Place, MonsterData> fieldMonster2 in parameters.surroundLevel.userArmy.fieldMonsters)
			{
				rightData.fieldMonsters.Add(new Place
				{
					x = 5 - fieldMonster2.Key.x,
					y = fieldMonster2.Key.y
				}, fieldMonster2.Value);
			}
			leftData.runes = new Dictionary<Place, RuneData>();
			foreach (KeyValuePair<Place, RuneData> rune in parameters.surroundLevel.enemyArmy.runes)
			{
				leftData.runes.Add(new Place
				{
					x = 5 - rune.Key.x,
					y = rune.Key.y
				}, rune.Value);
			}
			rightData.runes = new Dictionary<Place, RuneData>();
			foreach (KeyValuePair<Place, RuneData> rune2 in parameters.surroundLevel.userArmy.runes)
			{
				rightData.runes.Add(new Place
				{
					x = 5 - rune2.Key.x,
					y = rune2.Key.y
				}, rune2.Value);
			}
		}

		protected virtual ArmyActionPerformer GetPerformerRight(FieldScriptWrapper.StartBattleArmyParams.ControllerType enemyController, ArmyControllerCore leftArmy, ArmyControllerCore rightArmy, FieldParameters thisParameters, FieldScriptWrapper.StartBattleParameters parameters)
		{
			ArmyActionPerformer result = null;
			switch (enemyController)
			{
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.AI:
			{
				AIArmyActionPerformer aIArmyActionPerformer = new AIArmyActionPerformer
				{
					waitTimeBeforePlacement = (parameters.battleType != FieldScriptWrapper.StartBattleArmyParams.BattleType.Dungeon)
				};
				aIArmyActionPerformer.Init(leftArmy.GetHand, leftArmy.GetSkills, rightArmy, thisParameters);
				aIArmyActionPerformer.canConcede = parameters.canAIConcede;
				if (_curLevel != null && _curLevel.HasAiChar)
				{
					aIArmyActionPerformer.SetAiDecicionMaker(_curLevel.aiId);
				}
				else if (parameters.ai != null)
				{
					aIArmyActionPerformer.SetAiDecicionMaker(parameters.ai);
				}
				else
				{
					aIArmyActionPerformer.SetAiDecicionMaker(parameters.battleType);
				}
				result = aIArmyActionPerformer;
				break;
			}
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.Player:
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostart:
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostartFree:
			{
				PlayerArmyActionPerformer playerArmyActionPerformer = new PlayerArmyActionPerformer
				{
					sendPlaceQuest = parameters.sendPlaceQuests,
					levelEffectDescr = parameters.modificationDescription
				};
				playerArmyActionPerformer.Init(leftArmy.GetHand, leftArmy.GetSkills, rightArmy, thisParameters);
				playerArmyActionPerformer.AttachCreator(_creator);
				WindowScriptCore<BattlefieldWindow>.instance.InitActionPerformer(playerArmyActionPerformer);
				if (enemyController == FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostart || enemyController == FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostartFree)
				{
					playerArmyActionPerformer.StartAutoplay();
				}
				result = playerArmyActionPerformer;
				break;
			}
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.GoldMine:
			{
				GoldMineArmyActionPerformer goldMineArmyActionPerformer = new GoldMineArmyActionPerformer();
				goldMineArmyActionPerformer.Init(leftArmy.GetHand, leftArmy.GetSkills, rightArmy, thisParameters);
				goldMineArmyActionPerformer.AttachBattlefieldWindow(WindowScriptCore<BattlefieldWindow>.instance);
				WindowScriptCore<BattlefieldWindow>.instance.InitGoldMineState(GoldMineModule.GetCurrentMineData());
				result = goldMineArmyActionPerformer;
				break;
			}
			}
			if (_curLevel != null && _curLevel.tutorialStepData.Count > 0 && TestUtilFunctions.ShouldShowTutor())
			{
				TutorialArmyActionPerformer tutorialArmyActionPerformer = new TutorialArmyActionPerformer();
				tutorialArmyActionPerformer.AttachStepsInfo(_curLevel.tutorialStepData);
				tutorialArmyActionPerformer.Init(leftArmy.GetHand, leftArmy.GetSkills, rightArmy, thisParameters);
				result = tutorialArmyActionPerformer;
			}
			return result;
		}

		protected virtual ArmyActionPerformer GetPerformerLeft(FieldScriptWrapper.StartBattleArmyParams.ControllerType playerController, ArmyControllerCore leftArmy, ArmyControllerCore rightArmy, FieldParameters thisParameters, FieldScriptWrapper.StartBattleParameters parameters)
		{
			ArmyActionPerformer armyActionPerformer = null;
			switch (playerController)
			{
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.AI:
			{
				AIArmyActionPerformer aIArmyActionPerformer = new AIArmyActionPerformer();
				aIArmyActionPerformer.waitTimeBeforePlacement = parameters.battleType != FieldScriptWrapper.StartBattleArmyParams.BattleType.Dungeon;
				aIArmyActionPerformer.Init(rightArmy.GetHand, rightArmy.GetSkills, leftArmy, thisParameters);
				aIArmyActionPerformer.SetAiDecicionMaker(parameters.battleType);
				aIArmyActionPerformer.canConcede = parameters.canAIConcede;
				return aIArmyActionPerformer;
			}
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.Player:
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostart:
			case FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostartFree:
			{
				PlayerArmyActionPerformer playerArmyActionPerformer = new PlayerArmyActionPerformer
				{
					sendPlaceQuest = parameters.sendPlaceQuests,
					levelEffectDescr = parameters.modificationDescription
				};
				playerArmyActionPerformer.Init(rightArmy.GetHand, rightArmy.GetSkills, leftArmy, thisParameters);
				playerArmyActionPerformer.AttachCreator(_creator);
				WindowScriptCore<BattlefieldWindow>.instance.InitActionPerformer(playerArmyActionPerformer);
				if (playerController == FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostart || playerController == FieldScriptWrapper.StartBattleArmyParams.ControllerType.PlayerAutostartFree)
				{
					playerArmyActionPerformer.StartAutoplay();
				}
				return playerArmyActionPerformer;
			}
			default:
				throw new NotImplementedException();
			}
		}

		public void LoadBattlefield(ArmyData playerArmy, ArmyData playerArmy2, List<MonsterData> leftOrderedDeck, List<MonsterData> rightOrderedDeck, FieldScriptWrapper.StartBattleArmyParams.ControllerType playerController, FieldScriptWrapper.StartBattleArmyParams.ControllerType enemyController, FieldScriptWrapper.StartBattleParameters parameters)
		{
			_creator.ClearBattlefield();
			_creator.CreateBattlefield(6, 4);
			if (parameters.surroundLevel != null)
			{
				waitTime = parameters.surroundLevel.enemyDelay;
			}
			else
			{
				waitTime = 0f;
			}
			if (UserArmyModule.inited)
			{
				BoostModule.instance.UpdateActiveBoosts();
			}
			_fastEnding = parameters.fastEnding;
			_showDialogs = parameters.showDialogs;
			_curLevel = parameters.surroundLevel;
			_battleType = parameters.battleType;
			_rewardsDelegate = parameters.rewardDelegate;
			_forceRewards = parameters.isForceRewards;
			_defeatRewardDelegate = parameters.defeatRewardDelegate;
			WindowScriptCore<BattlefieldWindow>.instance.InitButtons(parameters.buttonState);
			CreateArmyControllers(out var leftArmy, out var rightArmy, playerArmy, playerArmy2, leftOrderedDeck, rightOrderedDeck, playerController, enemyController, parameters);
			_armies = new Dictionary<ArmySide, ArmyControllerCore>
			{
				{
					ArmySide.Left,
					leftArmy
				},
				{
					ArmySide.Right,
					rightArmy
				}
			};
		}

		public void StartBattle(Action<FieldScriptWrapper.CompleteFightParams> onVictory, Action<FieldScriptWrapper.CompleteFightParams> onDefeat, Action<FieldScriptWrapper.CompleteFightParams> onTie, Action<FieldScriptWrapper.CompleteFightParams> onCompletedBack, LevelCompletedWindow.HintType hint, ArmySide secondSide)
		{
			Common.ResetRandom();
			_hint = hint;
			_onVictory = onVictory;
			_onDefeat = onDefeat;
			_onTie = onTie;
			_onCompletedBack = onCompletedBack;
			_firstTurnLock = true;
			TutorialHandler.Instance.RecieveTrigger("LEVEL_STARTED");
			_armies[secondSide].GenerateHand();
			_armies[secondSide].AddSkillTimerPoint();
			_armies[(secondSide != ArmySide.Right) ? ArmySide.Right : ArmySide.Left].GenerateHand();
			WindowScriptCore<BattlefieldWindow>.instance.SetDark(secondSide == ArmySide.Left);
			if (secondSide == ArmySide.Left)
			{
				skipDelay = true;
				_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.enemyStartDelay / TimeDebugController.totalTimeMultiplier, delegate
				{
					SwitchActiveArmy(secondSide);
				});
			}
			else
			{
				SwitchActiveArmy(secondSide);
			}
		}

		public int GetDefeatPoints()
		{
			if (_defeatRewardDelegate != null)
			{
				List<LevelReward> list = _defeatRewardDelegate().FindAll((LevelReward x) => x.type == LevelReward.RewardType.PvpPoints);
				int num = 0;
				foreach (LevelReward item in list)
				{
					num += item.quantity;
				}
				return -num;
			}
			return 0;
		}

		public override void InformDefeat(ArmySide defeatedSide, bool delay = false, bool isTie = false)
		{
			Time.timeScale = 1f;
			ArmySide victorySide = ((defeatedSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
			if (victorySide == ArmySide.Left)
			{
				WindowScriptCore<LevelCompletedWindow>.instance.PrepareMainPicture();
			}
			base.InformDefeat(defeatedSide, delay: false, isTie);
			string curMatchId = PvpHandler.instance.CurMatchId;
			Common.VoidDelegate onDialog = delegate
			{
				Common.VoidDelegate onVaited = delegate
				{
					if (WindowScriptCore<BattlefieldWindow>.IsInstantiatedAndShown())
					{
						List<LevelReward> allRewards = new List<LevelReward>();
						if (UserArmyModule.inited)
						{
							if (defeatedSide == ArmySide.Right)
							{
								if (_curLevel != null && !_forceRewards)
								{
									allRewards.AddRange(_curLevel.GetRewards().FindAll((LevelReward x) => x != null));
								}
								else if (_rewardsDelegate != null)
								{
									allRewards = _rewardsDelegate().FindAll((LevelReward x) => x != null);
								}
							}
							else if (_defeatRewardDelegate != null)
							{
								allRewards = _defeatRewardDelegate().FindAll((LevelReward x) => x != null);
							}
						}
						for (int num = 0; num < allRewards.Count; num++)
						{
							allRewards[num].match_id = curMatchId;
						}
						if (defeatedSide == ArmySide.Left)
						{
							Action onDefeated = delegate
							{
								if (CanApplyReward)
								{
									foreach (LevelReward item in allRewards)
									{
										if (item.type == LevelReward.RewardType.Chest && ChestModule.Instance.canAddChest)
										{
											item.RandomizeReward();
											UserRewardModule.instance.ApplyReward(item);
										}
									}
								}
								FieldMonster warlord2 = _armies[ArmySide.Right].GetWarlord();
								FieldScriptWrapper.CompleteFightParams completeFightParams = new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord2.Health, warlord2.MaxHealth);
								base.LastFightResult = completeFightParams;
								_onDefeat(completeFightParams);
								TutorialHandler.Instance.RecieveTrigger("LEVEL_DEFEAT");
							};
							if (CanApplyReward)
							{
								foreach (LevelReward item2 in allRewards)
								{
									if (item2.type != LevelReward.RewardType.Chest)
									{
										item2.RandomizeReward();
										UserRewardModule.instance.ApplyReward(item2);
									}
								}
							}
							if (!_fastEnding)
							{
								AppsFlyerAdapter.SendEndBattle(_battleType.ToString(), PvpHandler.instance.CurMatchId, victory: false, allRewards);
								MapLevelDataObscured mapLevel = ((_curLevel != null) ? _curLevel.mapLevelData : MapLevelDataObscured.DefaultLevel);
								List<LevelReward> rewards = allRewards.FindAll((LevelReward x) => x.type != LevelReward.RewardType.Medal && x.quantity != 0);
								Common.VoidDelegate onLeave = delegate
								{
									onDefeated();
								};
								Action backDelegate = delegate
								{
									ClearBattlefield();
									FieldMonster warlord2 = _armies[ArmySide.Right].GetWarlord();
									_onCompletedBack(new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord2.Health, warlord2.MaxHealth));
								};
								LevelCompletedWindow.ShowDefeat(rewards, onLeave, backDelegate, mapLevel, _hint);
							}
							else
							{
								onDefeated();
								ClearBattlefield();
								FieldMonster warlord = _armies[ArmySide.Right].GetWarlord();
								_onCompletedBack(new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord.Health, warlord.MaxHealth));
							}
						}
						else
						{
							EFightResult fightResult = ((!isTie) ? EFightResult.Win : EFightResult.Tie);
							if (fightResult == EFightResult.Tie)
							{
								allRewards.Clear();
							}
							if (CanApplyReward)
							{
								foreach (LevelReward item3 in allRewards)
								{
									if (item3.type != LevelReward.RewardType.Chest)
									{
										item3.RandomizeReward();
										UserRewardModule.instance.ApplyReward(item3);
									}
								}
								if (allRewards.Count > 0)
								{
									string info = MyUtil.StringListToString(allRewards.ConvertAll((LevelReward r) => string.Concat(r.type, "(q:", r.quantity, ")")));
									ProfileHandler.Instance.RegisterAction(ProfileActionType.AfterBattleRewards, info);
								}
							}
							if (fightResult == EFightResult.Win)
							{
								AppsFlyerAdapter.SendEndBattle(_battleType.ToString(), PvpHandler.instance.CurMatchId, victory: true, allRewards);
							}
							WindowScriptCore<LevelCompletedWindow>.instance.SetTie(isTie);
							List<LevelReward> rewards2 = allRewards.FindAll((LevelReward x) => x.type != LevelReward.RewardType.Medal);
							Common.VoidDelegate onLeave2 = delegate
							{
								FieldMonster warlord2 = _armies[ArmySide.Right].GetWarlord();
								FieldScriptWrapper.CompleteFightParams completeFightParams = new FieldScriptWrapper.CompleteFightParams(fightResult, warlord2.Health, warlord2.MaxHealth);
								base.LastFightResult = completeFightParams;
								if (fightResult == EFightResult.Tie)
								{
									_onTie(completeFightParams);
								}
								else
								{
									_onVictory(completeFightParams);
								}
								TutorialHandler.Instance.RecieveTrigger("LEVEL_VICTORY");
							};
							Action backDelegate2 = delegate
							{
								if (CanApplyReward)
								{
									foreach (LevelReward item4 in allRewards)
									{
										if (item4.type == LevelReward.RewardType.Chest && ChestModule.Instance.canAddChest)
										{
											item4.RandomizeReward();
											UserRewardModule.instance.ApplyReward(item4);
										}
									}
									if (allRewards.Count > 0)
									{
										List<string> list = allRewards.ConvertAll((LevelReward r) => string.Concat(r.type, "(q:", r.quantity, ")"));
										MyUtil.StringListToString(list);
										ProfileHandler.Instance.RegisterAction(ProfileActionType.AfterBattleRewards, "ChestRewards. " + list);
									}
								}
								ClearBattlefield();
								FieldMonster warlord2 = _armies[ArmySide.Right].GetWarlord();
								_onCompletedBack(new FieldScriptWrapper.CompleteFightParams(fightResult, warlord2.Health, warlord2.MaxHealth));
							};
							LevelData curLevel = _curLevel;
							if (curLevel == null)
							{
								_ = MapLevelDataObscured.DefaultLevel;
							}
							else
							{
								_ = curLevel.mapLevelData;
							}
							LevelCompletedWindow.ShowVictory(rewards2, _curLevel, onLeave2, backDelegate2);
						}
					}
				};
				if (delay)
				{
					_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.concedeTimeDelay, delegate
					{
						_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.levelCompletedDelay, onVaited);
						AnimateDefeat(defeatedSide);
					});
				}
				else
				{
					_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.levelCompletedDelay, onVaited);
					AnimateDefeat(defeatedSide);
				}
			};
			Common.VoidDelegate onDialogs = delegate
			{
				CheckDialogs(onDialog, (victorySide == ArmySide.Left) ? (-2) : (-3));
			};
			CheckDialogs(onDialogs, -1);
		}

		public void KillAllFieldUnits(ArmySide side)
		{
			foreach (KeyValuePair<Vector2, FieldMonster> fieldMonster in _armies[side].GetFieldMonsters())
			{
				fieldMonster.Value.Kill();
			}
		}

		public ArmyControllerCore GetArmy(ArmySide side)
		{
			return _armies[side];
		}

		private void ClearBattlefield()
		{
			_creator.ClearBattlefield();
			WindowManager.HideWindow(WindowScriptCore<BattlefieldWindow>.NAME);
		}

		protected override void SwitchActiveArmy(ArmySide prev)
		{
			Common.VoidDelegate procedure = delegate
			{
				if (_curTurn > 0)
				{
					TutorialHandler.Instance.RecieveTrigger("TURN_ENDED");
					TutorialHandler.Instance.RecieveTrigger(string.Concat("TURN_", _curTurn, "_", prev, "_ENDED"));
					TutorialHandler.Instance.RecieveTrigger("TURN_" + _curTurn + "_ENDED");
				}
				if (_curTurn < 1)
				{
					int category = RequestFastDialog(FastDialogData.Event.BattleStarted, (prev == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
					_delayedActionsHandler.WaitForProcedure(TimeDebugController.instance.secondDialogDelay, delegate
					{
						RequestFastDialog(FastDialogData.Event.BattleStartedSecondTurn, prev, category);
					});
				}
				base.SwitchActiveArmy(prev);
			};
			if (!_firstTurnLock)
			{
				float num = Constants.turn_interval;
				if (num <= 0f)
				{
					num = 0.1f;
				}
				_delayedActionsHandler.WaitForProcedure(num, procedure);
			}
			else
			{
				_firstTurnLock = false;
				_delayedActionsHandler.WaitForProcedure(0.1f, procedure);
			}
		}

		protected override void PerformStep(ArmySide controller)
		{
			_curTurn++;
			Common.VoidDelegate onDialogs = delegate
			{
				TutorialHandler.Instance.RecieveTrigger("TURN_STARTED");
				TutorialHandler.Instance.RecieveTrigger(string.Concat("TURN_", _curTurn, "_", (_curActiveArmy == ArmySide.Left) ? ArmySide.Right : ArmySide.Left, "_STARTED"));
				TutorialHandler.Instance.RecieveTrigger("TURN_" + _curTurn + "_STARTED");
				base.PerformStep(controller);
			};
			CheckDialogs(onDialogs, _curTurn);
		}

		public int RequestFastDialog(FastDialogData.Event trigger, ArmySide site, int category = 0)
		{
			_armies[(site == ArmySide.Left) ? ArmySide.Right : ArmySide.Left].InformEnemySpeech(trigger);
			HideFastDialog(site);
			return ShowFastDialog(trigger, _armies[site].GetWarlord(), _armies[(site == ArmySide.Left) ? ArmySide.Right : ArmySide.Left].GetWarlord(), category);
		}

		private int ShowFastDialog(FastDialogData.Event trigger, FieldMonster performer, FieldMonster subject, int category = 0)
		{
			List<FastDialogData> list = FastDialogsDataHelper.GetDialog(trigger, performer.data.monster_id, subject.data.monster_id);
			if (category != 0)
			{
				list = list.FindAll((FastDialogData x) => x.category == category);
			}
			if (list.Count != 0)
			{
				FastDialogData random = list.GetRandom();
				string text = "#dialog_fast_phrase_" + random.id;
				ShowNotDelayedBubble(performer, text);
				SoundManager.Instance.PlaySound(random.sound);
				return random.category;
			}
			return 0;
		}

		private void ShowNotDelayedBubble(FieldMonster fastCurMonster, string text)
		{
			BubbleElement bubble = _creator.CreateBubble();
			ArmySide side = fastCurMonster.Side;
			bubble.Init(fastCurMonster, fastCurMonster.Side, text);
			_fastBubble[side] = bubble;
			Action hideThisDialog = delegate
			{
				if (_fastBubble[side] == bubble)
				{
					HideFastDialog(side);
				}
			};
			MonsterActionBehaviour<FastDialogBehaviour>.instance.AddBubble(bubble, hideThisDialog);
			_delayedActionsHandler.WaitForProcedure(Constants.bubble_delay, delegate
			{
				hideThisDialog();
			});
		}

		public void HideFastDialog(ArmySide side)
		{
			if (_fastBubble[side] != null)
			{
				MonsterActionBehaviour<FastDialogBehaviour>.instance.RemoveBubble(_fastBubble[side]);
				_fastBubble[side].Destroy();
				_fastBubble[side] = null;
			}
		}

		private void HideFastDialog()
		{
			HideFastDialog(ArmySide.Left);
			HideFastDialog(ArmySide.Right);
		}

		private void CheckDialogs(Common.VoidDelegate onDialogs, int turn)
		{
			if (_curLevel != null && _showDialogs)
			{
				List<DialogData> list = _curLevel.dialogs.FindAll((DialogData x) => x.turn == turn);
				Common.VoidDelegate onDialogs2 = onDialogs;
				if (list.Count > 0)
				{
					bool isDarker = WindowScriptCore<BattlefieldWindow>.instance.IsDark;
					WindowScriptCore<BattlefieldWindow>.instance.SetDark(dark: true);
					onDialogs = delegate
					{
						WindowScriptCore<BattlefieldWindow>.instance.SetDark(isDarker);
						onDialogs();
					};
				}
				PerformDialogs(0, list, onDialogs2);
			}
			else
			{
				onDialogs();
			}
		}

		private void PerformDialogs(int num, List<DialogData> dialogs, Common.VoidDelegate onDialogs)
		{
			if (num == dialogs.FindAll((DialogData x) => !x.isFast).Count)
			{
				WindowScriptCore<BattlefieldWindow>.instance.SetDialogState(inDialog: false);
				MonsterActionBehaviour<BubbleSpeechBehaviour>.instance.block = false;
				DialogData dialogData = dialogs.Find((DialogData x) => x.isFast);
				if (dialogData != null)
				{
					FieldMonster dialogMonster = GetDialogMonster(dialogData);
					string text = "#dialog_" + (string)_curLevel.mapLevelData.sectorType + "_" + (string)_curLevel.mapLevelData.sector + "_" + (string)_curLevel.mapLevelData.level + "_phrase_" + dialogData.turnStr + "_" + dialogData.step;
					HideFastDialog();
					ShowNotDelayedBubble(dialogMonster, text);
				}
				onDialogs();
				return;
			}
			HideFastDialog();
			WindowScriptCore<BattlefieldWindow>.instance.SetDialogState(inDialog: true);
			MonsterActionBehaviour<BubbleSpeechBehaviour>.instance.block = true;
			DialogData curDialog = dialogs.Find((DialogData x) => x.step == num + 1 && !x.isFast);
			FieldMonster curMonster = GetDialogMonster(curDialog);
			ArmySide armySide = curMonster.Side;
			ArmySide side = armySide;
			BubbleElement bubble;
			Common.VoidDelegate onPlacementNSelected = delegate
			{
				if (curMonster != null)
				{
					bubble = _creator.CreateBubble();
					string dialogText = "#dialog_" + (string)_curLevel.mapLevelData.sectorType + "_" + (string)_curLevel.mapLevelData.sector + "_" + (string)_curLevel.mapLevelData.level + "_phrase_" + curDialog.turnStr + "_" + curDialog.step;
					bubble.Init(curMonster, side, dialogText);
					if (curDialog.actions.Contains(DialogData.Action.Rotate))
					{
						curMonster.VisualMonster.DialogRotate();
					}
					IsDialoging = true;
					Common.VoidDelegate onClick = delegate
					{
						if (bubble.isAnimating)
						{
							bubble.CompleteAnimation();
						}
						else
						{
							IsDialoging = false;
							MonsterActionListener.UnregisterBehaviour(MonsterActionBehaviour<DialogBehaviour>.instance);
							if (bubble != null)
							{
								bubble.Destroy();
							}
							if (curDialog.actions.Contains(DialogData.Action.Rotate))
							{
								curMonster.VisualMonster.DialogRotate();
							}
							if (curDialog.actions.Contains(DialogData.Action.Destroy))
							{
								curMonster.Kill();
								curMonster.CheckDeath(delegate
								{
									PerformDialogs(num + 1, dialogs, onDialogs);
								});
							}
							else
							{
								PerformDialogs(num + 1, dialogs, onDialogs);
							}
						}
					};
					MonsterActionBehaviour<DialogBehaviour>.instance.Init(onClick);
					MonsterActionListener.RegisterBehaviour(MonsterActionBehaviour<DialogBehaviour>.instance);
				}
				else
				{
					PerformDialogs(num + 1, dialogs, onDialogs);
				}
			};
			if (curMonster == null)
			{
				if (curDialog.actions.Contains(DialogData.Action.CreateFriendly))
				{
					armySide = ArmySide.Left;
				}
				else
				{
					if (!curDialog.actions.Contains(DialogData.Action.CreateEnemy))
					{
						onPlacementNSelected();
						return;
					}
					armySide = ArmySide.Right;
				}
				MonsterData monster = MonsterDataUtils.CreateMonster(curDialog.monsterId, curDialog.monsterPromote);
				_armies[armySide].PlaceMonster(monster, curDialog.performer, delegate
				{
					curMonster = _armies[armySide].GetFieldMonsters()[curDialog.performer];
					onPlacementNSelected();
				});
			}
			else
			{
				onPlacementNSelected();
			}
		}

		private FieldMonster GetDialogMonster(DialogData dData)
		{
			if ((dData.isFriendWarlord || _armies[ArmySide.Left].GetFieldMonsters().ContainsKey(dData.performer)) && !dData.isEnemyWarlord)
			{
				if (!dData.isFriendWarlord)
				{
					return _armies[ArmySide.Left].GetFieldMonsters()[dData.performer];
				}
				return _armies[ArmySide.Left].GetWarlord();
			}
			if (dData.isEnemyWarlord || _armies[ArmySide.Right].GetFieldMonsters().ContainsKey(dData.performer))
			{
				if (!dData.isEnemyWarlord)
				{
					return _armies[ArmySide.Right].GetFieldMonsters()[dData.performer];
				}
				return _armies[ArmySide.Right].GetWarlord();
			}
			return null;
		}

		public void ShuffleActiveHand()
		{
			_armies[_curActiveArmy].ShuffleHand(playerUseShuffle: true);
		}
	}
}
