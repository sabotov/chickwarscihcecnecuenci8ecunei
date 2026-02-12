using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MapData;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using PnkClient;
using ServiceLocator;
using Tutorial;
using UI_Scripts.WindowManager;
using UnityEngine;
using UserData;
using UtilScripts;

namespace BattlefieldScripts
{
	public class TestingFieldController : FieldControllerCore
	{
		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		public const int FIELD_WIDTH = 6;

		public const int FIELD_HEIGHT = 4;

		private LevelData _curLevel;

		private Action<FieldScriptWrapper.CompleteFightParams> _onVictory;

		private Action<FieldScriptWrapper.CompleteFightParams> _onDefeat;

		private Action<FieldScriptWrapper.CompleteFightParams> _onTie;

		private Action<FieldScriptWrapper.CompleteFightParams> _onCompletedBack;

		private bool _forceRewards;

		private Func<List<LevelReward>> _rewardsDelegate;

		private Func<List<LevelReward>> _defeatRewardDelegate;

		private bool _fastEnding;

		private bool _firstTurnLock;

		private string _curDecor = "";

		private FieldScriptWrapper.StartBattleArmyParams.BattleType _battleType;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public void LoadBattlefield(ArmyData playerArmy, ArmyData playerArmy2, List<MonsterData> leftOrderedDeck, List<MonsterData> rightOrderedDeck, FieldScriptWrapper.StartBattleArmyParams.ControllerType playerController, FieldScriptWrapper.StartBattleArmyParams.ControllerType enemyController, FieldScriptWrapper.StartBattleParameters parameters)
		{
			GetArmyData(parameters, playerArmy, playerArmy2, out var army, out var army2);
			_fastEnding = parameters.fastEnding;
			_curLevel = parameters.surroundLevel;
			_rewardsDelegate = parameters.rewardDelegate;
			_forceRewards = parameters.isForceRewards;
			_defeatRewardDelegate = parameters.defeatRewardDelegate;
			_armies = new Dictionary<ArmySide, ArmyControllerCore>();
			FieldParameters fieldParameters = new FieldParameters();
			fieldParameters.Init(6, 4);
			fieldParameters.InitSkillStuff(parameters.skillDelay, parameters.skillShift);
			TestArmyController testArmyController = new TestArmyController(ArmySide.Left);
			TestArmyController testArmyController2 = new TestArmyController(ArmySide.Right);
			_battleType = parameters.battleType;
			TestArmyActionPerformer testArmyActionPerformer = new TestArmyActionPerformer();
			testArmyActionPerformer.Init(testArmyController2.GetHand, testArmyController2.GetSkills, testArmyController, fieldParameters);
			testArmyActionPerformer.SetAiDecicionMaker(AICharacterHelper.AutoPlayCharacter());
			testArmyActionPerformer.canConcede = parameters.canAIConcede;
			TestArmyActionPerformer testArmyActionPerformer2 = new TestArmyActionPerformer();
			testArmyActionPerformer2.Init(testArmyController.GetHand, testArmyController.GetSkills, testArmyController2, fieldParameters);
			testArmyActionPerformer2.canConcede = parameters.canAIConcede;
			if (_curLevel != null && _curLevel.HasAiChar)
			{
				testArmyActionPerformer2.SetAiDecicionMaker(_curLevel.aiId);
			}
			else if (parameters.ai != null)
			{
				testArmyActionPerformer2.SetAiDecicionMaker(parameters.ai);
			}
			else
			{
				testArmyActionPerformer2.SetAiDecicionMaker(parameters.battleType);
			}
			FieldRandom fieldRandom = new RecordingRandom();
			testArmyController.Init(this, testArmyActionPerformer, fieldParameters, fieldRandom.GetSimulateCopy(), parameters.drawType, parameters.battleType, new SimulateIterator(), army);
			testArmyController2.Init(this, testArmyActionPerformer2, fieldParameters, fieldRandom.GetSimulateCopy(), parameters.drawType, parameters.battleType, new SimulateIterator(), army2);
			fieldParameters.AttachControllers(testArmyController, testArmyController2);
			if (parameters.drawType == ArmyControllerCore.DrawType.NewSurvival)
			{
				testArmyController.AttachRaritySequence(parameters.raritySequence);
				testArmyController2.AttachRaritySequence(parameters.raritySequence);
			}
			if (leftOrderedDeck != null && leftOrderedDeck.Count > 0)
			{
				testArmyController.AttachOrderedDeck(leftOrderedDeck);
			}
			if (rightOrderedDeck != null && rightOrderedDeck.Count > 0)
			{
				testArmyController2.AttachOrderedDeck(rightOrderedDeck);
			}
			testArmyController.CreateArmy();
			testArmyController2.CreateArmy();
			_armies.Add(ArmySide.Left, testArmyController);
			_armies.Add(ArmySide.Right, testArmyController2);
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

		public void StartBattle(Action<FieldScriptWrapper.CompleteFightParams> onVictory, Action<FieldScriptWrapper.CompleteFightParams> onDefeat, Action<FieldScriptWrapper.CompleteFightParams> onTie, Action<FieldScriptWrapper.CompleteFightParams> onCompletedBack, LevelCompletedWindow.HintType hint, ArmySide secondSide)
		{
			Common.ResetRandom();
			_onVictory = onVictory;
			_onDefeat = onDefeat;
			_onTie = onTie;
			_onCompletedBack = onCompletedBack;
			_firstTurnLock = true;
			TutorialHandler.Instance.RecieveTrigger("LEVEL_STARTED");
			_armies[secondSide].GenerateHand();
			_armies[secondSide].AddSkillTimerPoint();
			_armies[(secondSide != ArmySide.Right) ? ArmySide.Right : ArmySide.Left].GenerateHand();
			if (secondSide == ArmySide.Left)
			{
				skipDelay = true;
				SwitchActiveArmy(secondSide);
			}
			else
			{
				SwitchActiveArmy(secondSide);
			}
		}

		public override void InformDefeat(ArmySide defeatedSide, bool delay = false, bool isTie = false)
		{
			Time.timeScale = 1f;
			base.InformDefeat(defeatedSide, delay: false, isTie);
			string curMatchId = PvpHandler.instance.CurMatchId;
			List<LevelReward> list = new List<LevelReward>();
			if (defeatedSide == ArmySide.Right)
			{
				if (_curLevel != null && !_forceRewards)
				{
					list.AddRange(_curLevel.GetRewards().FindAll((LevelReward x) => x != null));
				}
				else if (_rewardsDelegate != null)
				{
					list = _rewardsDelegate().FindAll((LevelReward x) => x != null);
				}
			}
			else if (_defeatRewardDelegate != null)
			{
				list = _defeatRewardDelegate().FindAll((LevelReward x) => x != null);
			}
			for (int num = 0; num < list.Count; num++)
			{
				list[num].match_id = curMatchId;
			}
			if (defeatedSide == ArmySide.Left)
			{
				foreach (LevelReward item in list)
				{
					if (item.type != LevelReward.RewardType.Chest)
					{
						item.RandomizeReward();
						UserRewardModule.instance.ApplyReward(item);
					}
				}
				if (!_fastEnding)
				{
					AppsFlyerAdapter.SendEndBattle(_battleType.ToString(), PvpHandler.instance.CurMatchId, victory: false, list);
					foreach (LevelReward item2 in list)
					{
						if (item2.type == LevelReward.RewardType.Chest && ChestModule.Instance.canAddChest)
						{
							item2.RandomizeReward();
							UserRewardModule.instance.ApplyReward(item2);
						}
					}
					FieldMonster warlord = _armies[ArmySide.Right].GetWarlord();
					_onDefeat(new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord.Health, warlord.MaxHealth));
					TutorialHandler.Instance.RecieveTrigger("LEVEL_DEFEAT");
					_onCompletedBack(new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord.Health, warlord.MaxHealth));
					return;
				}
				foreach (LevelReward item3 in list)
				{
					if (item3.type == LevelReward.RewardType.Chest && ChestModule.Instance.canAddChest)
					{
						item3.RandomizeReward();
						UserRewardModule.instance.ApplyReward(item3);
					}
				}
				FieldMonster warlord2 = _armies[ArmySide.Right].GetWarlord();
				_onDefeat(new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord2.Health, warlord2.MaxHealth));
				TutorialHandler.Instance.RecieveTrigger("LEVEL_DEFEAT");
				_onCompletedBack(new FieldScriptWrapper.CompleteFightParams(EFightResult.Lose, warlord2.Health, warlord2.MaxHealth));
				return;
			}
			EFightResult eFightResult = ((!isTie) ? EFightResult.Win : EFightResult.Tie);
			if (eFightResult == EFightResult.Tie)
			{
				list.Clear();
			}
			foreach (LevelReward item4 in list)
			{
				if (item4.type != LevelReward.RewardType.Chest)
				{
					item4.RandomizeReward();
					UserRewardModule.instance.ApplyReward(item4);
				}
			}
			if (list.Count > 0)
			{
				ProfileHandler.Instance.RegisterAction(ProfileActionType.AfterBattleRewards, "");
			}
			if (eFightResult == EFightResult.Win)
			{
				AppsFlyerAdapter.SendEndBattle(_battleType.ToString(), PvpHandler.instance.CurMatchId, victory: true, list);
			}
			FieldMonster warlord3 = _armies[ArmySide.Right].GetWarlord();
			if (eFightResult == EFightResult.Tie)
			{
				_onTie(new FieldScriptWrapper.CompleteFightParams(eFightResult, warlord3.Health, warlord3.MaxHealth));
			}
			else
			{
				_onVictory(new FieldScriptWrapper.CompleteFightParams(eFightResult, warlord3.Health, warlord3.MaxHealth));
			}
			TutorialHandler.Instance.RecieveTrigger("LEVEL_VICTORY");
			foreach (LevelReward item5 in list)
			{
				if (item5.type == LevelReward.RewardType.Chest && ChestModule.Instance.canAddChest)
				{
					item5.RandomizeReward();
					UserRewardModule.instance.ApplyReward(item5);
				}
			}
			if (list.Count > 0)
			{
				ProfileHandler.Instance.RegisterAction(ProfileActionType.AfterBattleRewards, "");
			}
			_onCompletedBack(new FieldScriptWrapper.CompleteFightParams(eFightResult, warlord3.Health, warlord3.MaxHealth));
		}

		protected override void BreakStack(Common.VoidDelegate del)
		{
			_delayedActionsHandler.WaitForProcedure(0f, del);
		}
	}
}
