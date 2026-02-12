using System;
using System.Collections.Generic;
using Assets.Scripts.DataClasses.UserData;
using Assets.Scripts.UtilScripts.Loaders;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NGUI.Scripts.UI;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MapData;
using NewAssets.Scripts.DataClasses.MonsterParams;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UI_Scripts.UIElements;
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
	public class FieldScriptWrapper : MonoBehaviourExt
	{
		public class StartBattleArmyParams
		{
			public enum ControllerType
			{
				Player = 0,
				PlayerAutostart = 1,
				PlayerAutostartFree = 2,
				AI = 3,
				GoldMine = 4
			}

			public enum BattleType
			{
				PvP = 1,
				PvPBrawl = 2,
				Tournament = 3,
				Survival = 4,
				Training = 5,
				Dungeon = 6,
				TavernBrawl = 7,
				TestAI = 9,
				Pit = 10,
				GoldMine = 11
			}

			public string bg;

			public string name;

			public string country;

			public string subName = "";

			public string subNamelabel = "";

			public ArmyData army;

			public ControllerType controllerType;

			public List<MonsterData> orderedHand;

			public int medalId = 1;

			public GuildData guild;

			public StartBattleArmyParams()
			{
			}

			public StartBattleArmyParams(PvpHandler.OpponentType opponentType)
			{
				name = PvpHandler.instance.GetOpponentName(opponentType);
				army = PvpHandler.instance.GetOpponentArmy(opponentType);
				bg = army.warlordData.warlordBg;
				controllerType = ControllerType.AI;
				guild = PvpHandler.instance.GetOpponentGuild(opponentType);
				country = PvpHandler.instance.GetOpponentCountry(opponentType);
				medalId = PvpHandler.instance.GetOpponentMedalId(opponentType);
			}
		}

		public class CompleteFightParams
		{
			public EFightResult fightResult;

			public int enemyWarlordHp;

			public int maxEnemyWarlordHp;

			public CompleteFightParams(EFightResult fightResult = EFightResult.Lose, int enemyWarlordHp = 0, int maxEnemyWarlordHp = 0)
			{
				this.fightResult = fightResult;
				this.enemyWarlordHp = enemyWarlordHp;
				this.maxEnemyWarlordHp = maxEnemyWarlordHp;
			}
		}

		public class StartBattleParameters
		{
			public string settingName = "hell";

			public ArmySide firstSide;

			public Func<List<LevelReward>> rewardDelegate;

			public Func<List<LevelReward>> defeatRewardDelegate;

			public bool isForceRewards;

			public LevelData surroundLevel;

			public bool applySurroundLevel = true;

			public bool flipStartData;

			public LevelCompletedWindow.HintType hint;

			public Action<CompleteFightParams> victoryDelegate;

			public Action<CompleteFightParams> defeatDelegate;

			public Action<CompleteFightParams> tieDelegate;

			public Action<CompleteFightParams> completeBackDelegate;

			public bool unlockQuestsOnExit = true;

			public bool showDialogs;

			public List<string> tutorStartTriggers;

			public bool fastEnding;

			public AICharacter ai;

			public bool sendPlaceQuests = true;

			public bool canAIConcede = true;

			public BattlefieldWindow.ButtonStates buttonState = BattlefieldWindow.ButtonStates.Concede.Add(BattlefieldWindow.ButtonStates.Speed).Add(BattlefieldWindow.ButtonStates.Auto);

			public string modificationDescription = "";

			public int skillDelay = Constants.SkillsDeliveryCooldown;

			public int skillShift;

			public ArmyControllerCore.DrawType drawType;

			public List<Rarity> raritySequence;

			public StartBattleArmyParams.BattleType battleType;

			public StartBattleParameters(StartBattleArmyParams.BattleType sBattleType)
			{
				battleType = sBattleType;
			}
		}

		private readonly CachedService<IDelayedActionsHandler> __delayedActionsHandler = new CachedService<IDelayedActionsHandler>();

		public static StartBattleArmyParams.BattleType currentBattleType;

		public static bool isBrawl = false;

		public static bool isPvp = false;

		private static Dictionary<string, GameObject> _effectPrefs = new Dictionary<string, GameObject>();

		private static FieldScriptWrapper _instance;

		private GameObject _fieldEffect;

		public UISprite bg;

		public List<UISprite> grid;

		public List<Tile> tiles;

		public FieldController fieldController;

		public TestingFieldController testingFieldController;

		public FieldCreator fieldCreator;

		private StartBattleArmyParams _leftArmy;

		private StartBattleArmyParams _rightArmy;

		private StartBattleParameters _params;

		private bool _battlePreparing;

		private bool _battleStarting;

		private bool _animationCompleted;

		private bool _inBattle;

		public bool simulateStep;

		private IDelayedActionsHandler _delayedActionsHandler => __delayedActionsHandler.Value;

		public static FieldScriptWrapper instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Initializer.UIRootContainer.GetUIRoot(LayerName.Battle_Layer).transform.Find("BattlefieldContainer").GetComponent<FieldScriptWrapper>();
				}
				return _instance;
			}
		}

		public FieldControllerCore CurrentFieldController { get; private set; }

		public static CompleteFightParams LastFightResult => instance.CurrentFieldController?.LastFightResult;

		public bool inBattle
		{
			get
			{
				return _inBattle;
			}
			private set
			{
				if (value)
				{
					Screen.sleepTimeout = -1;
				}
				else
				{
					Screen.sleepTimeout = -2;
				}
				BoostModule.CanUpdate = !value;
				_inBattle = value;
			}
		}

		private static void SetBattleTypeInfo(StartBattleParameters _params)
		{
			currentBattleType = _params.battleType;
			isBrawl = _params.hint == LevelCompletedWindow.HintType.Brawl;
		}

		public ArmyData GetOpponentArmy()
		{
			return _rightArmy.army;
		}

		public static void Reset()
		{
			instance.inBattle = false;
			instance._battleStarting = false;
			instance._battlePreparing = false;
		}

		public static StartBattleArmyParams.BattleType GetBattleType()
		{
			return instance._params.battleType;
		}

		public static LevelCompletedWindow.HintType GetBattleHint()
		{
			return instance._params.hint;
		}

		public static void InformError()
		{
			instance.CurrentFieldController.InformError();
		}

		public void PrepareAndStartFight(StartBattleArmyParams leftArmyParams, StartBattleArmyParams rightArmyParams, StartBattleParameters parameters, string levelText = "", bool withScreen = true, bool fromPvpWindow = false)
		{
			if (_battlePreparing)
			{
				Debug.LogError("PrepareAndStartFight. _battlePreparing == true");
				return;
			}
			isPvp = fromPvpWindow;
			_battleStarting = false;
			_animationCompleted = false;
			_leftArmy = leftArmyParams;
			_rightArmy = rightArmyParams;
			_params = parameters;
			bool animationComplete = false;
			if (withScreen)
			{
				bool startPvpWindowReady = false;
				WindowScriptCore<StartPvPWindow>.instance.PrepareAnimation(delegate
				{
					startPvpWindowReady = true;
				}, levelText);
				_delayedActionsHandler.WaitForCondition(() => startPvpWindowReady && animationComplete, StartBattle);
			}
			else
			{
				_delayedActionsHandler.WaitForCondition(() => animationComplete, StartBattle);
			}
			LoadRequiredAnimationsForFight(leftArmyParams, rightArmyParams, delegate
			{
				animationComplete = true;
				if (withScreen)
				{
					WindowManager.ShowWindow(WindowScriptCore<StartPvPWindow>.NAME);
					WindowScriptCore<StartPvPWindow>.instance.InitLeftWarlord(leftArmyParams);
					WindowScriptCore<StartPvPWindow>.instance.InitRightWarlord(rightArmyParams);
				}
			});
			QuestProgressHandler.instance.SetGoToBlocked("battle", blocked: true);
			_battlePreparing = true;
		}

		private void LoadRequiredAnimationsForFight(StartBattleArmyParams leftArmyParams, StartBattleArmyParams rightArmyParams, Action onComplete)
		{
			LoadRequiredAnimationsForWarlord(leftArmyParams);
			LoadRequiredAnimationsForWarlord(rightArmyParams);
			onComplete.SafeInvoke();
		}

		private void LoadRequiredAnimationsForWarlord(StartBattleArmyParams armyParams, Action onComplete = null)
		{
			List<string> list = new List<string> { armyParams.army.warlord.animationName };
			string text = ((armyParams.army.petData != null) ? armyParams.army.petData.petMonsterData.animationName : "");
			if (!text.IsNullOrEmpty())
			{
				list.Add(text);
			}
			if (onComplete != null)
			{
				MonsterAnimationAssetManager.Instance.LoadElements(list, delegate
				{
					onComplete();
				});
			}
			else
			{
				MonsterAnimationAssetManager.Instance.LoadElements(list, delegate
				{
				});
			}
		}

		private void LoadAnimationsForFight(StartBattleArmyParams leftArmyParams, StartBattleArmyParams rightArmyParams, StartBattleParameters battleParams)
		{
			List<string> monstersToPreload = GetMonstersToPreload(rightArmyParams.army, (MonsterData x) => x.animationName);
			if (rightArmyParams.orderedHand != null)
			{
				monstersToPreload.AddRange(rightArmyParams.orderedHand.ConvertAll((MonsterData x) => x.animationName));
			}
			if (battleParams.surroundLevel != null && battleParams.applySurroundLevel)
			{
				monstersToPreload.AddRange(GetMonstersToPreload(battleParams.surroundLevel.userArmy, (MonsterData x) => x.animationName));
				monstersToPreload.AddRange(GetMonstersToPreload(battleParams.surroundLevel.enemyArmy, (MonsterData x) => x.animationName));
			}
			MonsterAnimationAssetManager.Instance.LoadElements(monstersToPreload, delegate
			{
			});
			List<string> monstersToPreload2 = GetMonstersToPreload(leftArmyParams.army, (MonsterData x) => x.image);
			monstersToPreload2.AddRange(GetMonstersToPreload(rightArmyParams.army, (MonsterData x) => x.image));
			if (leftArmyParams.orderedHand != null)
			{
				monstersToPreload2.AddRange(leftArmyParams.orderedHand.ConvertAll((MonsterData x) => x.image));
			}
			if (rightArmyParams.orderedHand != null)
			{
				monstersToPreload2.AddRange(rightArmyParams.orderedHand.ConvertAll((MonsterData x) => x.image));
			}
			MonsterImageAssetManager.Instance.LoadElements(monstersToPreload2, delegate
			{
			});
		}

		private void StartBattle()
		{
			if (_battleStarting)
			{
				Debug.LogWarning($"StartBattle. _animationCompleted = {_animationCompleted}, _battleStarting = {_battleStarting}");
				return;
			}
			AppsFlyerAdapter.SendStartBattle(_params.battleType.ToString(), PvpHandler.instance.CurMatchId);
			QuestProgressHandler.instance.SetBlocked(blocked: true, QuestProgressHandler.LockSituation.AnotherProcess);
			_battleStarting = true;
			_battlePreparing = false;
			WindowManager.ShowWindow(WindowScriptCore<BattlefieldWindow>.NAME);
			ResetField();
			AutofightUtils.StartBattle();
			SetBattleTypeInfo(_params);
			SoundManager.Instance.PlayBattleMusic(currentBattleType);
			bool flag = _params.battleType == StartBattleArmyParams.BattleType.PvP || _params.battleType == StartBattleArmyParams.BattleType.PvPBrawl || _params.battleType == StartBattleArmyParams.BattleType.Tournament || _params.battleType == StartBattleArmyParams.BattleType.Survival;
			bool showFlags = flag;
			WindowScriptCore<BattlefieldWindow>.instance.SetPlayersInfo(_leftArmy, _rightArmy, flag, showFlags);
			WindowScriptCore<BattlefieldWindow>.instance.SetLevelEffects(_params.modificationDescription);
			fieldCreator.InitDecor(_params.settingName);
			fieldController.InitDecor(_params.settingName);
			LoadSetting(_params.settingName);
			fieldController.LoadBattlefield(_leftArmy.army, _rightArmy.army, _leftArmy.orderedHand, _rightArmy.orderedHand, _leftArmy.controllerType, _rightArmy.controllerType, _params);
			if (_params.tutorStartTriggers != null)
			{
				foreach (string tutorStartTrigger in _params.tutorStartTriggers)
				{
					TutorialHandler.Instance.RecieveTrigger(tutorStartTrigger);
				}
			}
			fieldController.StartBattle(_params.victoryDelegate, _params.defeatDelegate, _params.tieDelegate, OnFight_CompleteBack, _params.hint, (_params.firstSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
			inBattle = true;
			TutorialHandler.Instance.RecieveTrigger("BATTLE_LOADED");
			TutorialHandler.Instance.RecieveTrigger("BATTLE_LOADED_" + _params.battleType.ToString().ToUpper());
			CurrentFieldController = fieldController;
		}

		public void StartBattleSilent(StartBattleArmyParams leftArmyParams, StartBattleArmyParams rightArmyParams, StartBattleParameters parameters, bool fromPvpWindow = false)
		{
			if (inBattle)
			{
				Debug.LogError("Trying to start pvp when it had already been started!");
				return;
			}
			_leftArmy = leftArmyParams;
			_rightArmy = rightArmyParams;
			_params = parameters;
			isPvp = fromPvpWindow;
			QuestProgressHandler.instance.SetGoToBlocked("battle", blocked: true);
			QuestProgressHandler.instance.SetBlocked(blocked: true, QuestProgressHandler.LockSituation.AnotherProcess);
			ResetField();
			SetBattleTypeInfo(_params);
			testingFieldController.LoadBattlefield(_leftArmy.army, _rightArmy.army, _leftArmy.orderedHand, _rightArmy.orderedHand, _leftArmy.controllerType, _rightArmy.controllerType, _params);
			testingFieldController.StartBattle(_params.victoryDelegate, _params.defeatDelegate, _params.tieDelegate, OnFight_CompleteBack, _params.hint, (_params.firstSide == ArmySide.Left) ? ArmySide.Right : ArmySide.Left);
			inBattle = true;
			CurrentFieldController = testingFieldController;
		}

		private void OnFight_CompleteBack(CompleteFightParams fightParams)
		{
			inBattle = false;
			_params.completeBackDelegate(fightParams);
			QuestProgressHandler.instance.SetGoToBlocked("battle", blocked: false);
			if (_params.unlockQuestsOnExit)
			{
				QuestProgressHandler.instance.SetBlocked(blocked: false, QuestProgressHandler.LockSituation.AnotherProcess);
			}
		}

		private List<MonsterData> GetMonstersFromSkill(MonsterData monster)
		{
			List<MonsterData> list = new List<MonsterData>();
			for (int i = 0; i < monster.skills.Count && i < monster.skillValues.Count; i++)
			{
				SkillStaticData skillStaticData = null;
				SkillType skillType = SkillType.NoSkill;
				try
				{
					skillStaticData = monster.skills[i];
					skillType = skillStaticData.skill;
				}
				catch (Exception ex)
				{
					Debug.LogError(monster.ToString() + ". skill error " + ex);
				}
				if ((skillType == SkillType.Summon || skillType == SkillType.Transform) && skillStaticData.filterMode != ValueFilterMode.Monster)
				{
					MonsterData item = MonsterDataUtils.CreateMonster(int.Parse(skillStaticData.value), int.Parse(monster.skillValues[i]));
					list.Add(item);
				}
			}
			return list;
		}

		private List<MonsterData> PreloadMonster(MonsterData monster)
		{
			return PreloadMonsters(new List<MonsterData> { monster });
		}

		private List<MonsterData> PreloadMonsters(List<MonsterData> monsters)
		{
			List<MonsterData> list = new List<MonsterData>();
			foreach (MonsterData monster in monsters)
			{
				list.Add(monster);
				list.AddRange(GetMonstersFromSkill(monster));
			}
			return list;
		}

		private List<string> ConvertPreloadedMonster(List<MonsterData> monsters, Func<MonsterData, string> convertion)
		{
			List<string> list = new List<string>();
			foreach (MonsterData monster in monsters)
			{
				list.Add(convertion(monster));
			}
			return list;
		}

		public List<string> GetMonstersToPreload(ArmyData army, Func<MonsterData, string> convertion)
		{
			List<MonsterData> list = new List<MonsterData>();
			if (army.warlord != null)
			{
				list.AddRange(PreloadMonster(army.warlord));
			}
			if (army.petData != null)
			{
				list.AddRange(PreloadMonster(army.petData.petMonsterData));
			}
			if (army.deck != null)
			{
				list.AddRange(PreloadMonsters(new List<MonsterData>(army.deck.Values)));
			}
			if (army.fieldMonsters != null)
			{
				list.AddRange(PreloadMonsters(new List<MonsterData>(army.fieldMonsters.Values)));
			}
			return ConvertPreloadedMonster(list, convertion);
		}

		public void ResetCompletely()
		{
			Reset();
			instance.fieldCreator?.ClearBattlefield();
			instance.ResetField();
		}

		public void ResetField()
		{
			fieldController = new FieldController();
			testingFieldController = new TestingFieldController();
			fieldCreator = new FieldCreator();
			fieldController.Init(fieldCreator);
			fieldCreator.Init(this, fieldController, tiles);
			if (_fieldEffect != null)
			{
				UnityEngine.Object.Destroy(_fieldEffect);
				_fieldEffect = null;
			}
		}

		private void LoadSetting(string setting)
		{
			bg.spriteName = MyDecorDataHelper.GetBackgroundByName(setting);
			Color gridTint = MyDecorDataHelper.GetGridTint(setting);
			foreach (UISprite item in grid)
			{
				item.color = new Color(gridTint.r, gridTint.g, gridTint.b, item.alpha);
			}
		}

		public void CreateEffect(List<string> effects)
		{
			if (effects.Count == 0)
			{
				return;
			}
			string text = effects[UnityEngine.Random.Range(0, effects.Count)];
			if (!(text == "none"))
			{
				if (!_effectPrefs.ContainsKey(text))
				{
					_effectPrefs.Add(text, Resources.Load<GameObject>("Prefabs/BattlePrefabs/BattleFieldEffects/" + text));
				}
				_fieldEffect = UnityEngine.Object.Instantiate(_effectPrefs[text]);
				_fieldEffect.transform.parent = base.transform;
				_fieldEffect.transform.localScale = new Vector3(1f, 1f, 1f);
				_fieldEffect.transform.localPosition = new Vector3(0f, 0f, 0f);
			}
		}

		private void Update()
		{
			if (simulateStep)
			{
				simulateStep = false;
				SimulateEnviroment simulateEnviroment = new SimulateEnviroment();
				ArmySide stepSide = ArmySide.Left;
				Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armiesStates = CurrentFieldController.GetArmiesStates();
				int depth = 1;
				DebugArmies(armiesStates);
				simulateEnviroment.Init(randomCopy: ((Func<CopiedSimulateRandom>)(() => new CopiedSimulateRandom()))(), armies: armiesStates, width: fieldCreator.GetFieldWidth(), height: fieldCreator.GetFieldHeight(), skillDelay: Constants.SkillsDeliveryCooldown, skillShift: 0);
				simulateEnviroment.SimulateFight(stepSide, DebugArmies, depth);
			}
		}

		public static void DebugArmies(Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> armies)
		{
			foreach (KeyValuePair<ArmySide, SimulateEnviroment.SimulateArmy> army in armies)
			{
				string text = string.Concat("Army side: ", army.Key, "\n");
				foreach (KeyValuePair<Vector2, FieldMonster> item in army.Value.army)
				{
					text = string.Concat(text, item.Key, " ", item.Value.data.image, " ", (string)item.Value.Health, "/", (string)item.Value.MaxHealth, ", ", item.Value.Attack, "\n");
				}
				Debug.Log(text);
			}
		}

		public static StartBattleArmyParams GetPlayerArmy(LevelData levelData, string backgroundName)
		{
			ArmyData playerArmyData = GetPlayerArmyData(levelData);
			backgroundName = ((!string.IsNullOrEmpty(playerArmyData.warlordData.warlordBg)) ? playerArmyData.warlordData.warlordBg : backgroundName);
			StartBattleArmyParams startBattleArmyParams = new StartBattleArmyParams
			{
				name = NameModule.instance.PlayerName,
				army = playerArmyData,
				bg = backgroundName,
				controllerType = StartBattleArmyParams.ControllerType.Player
			};
			if (levelData != null)
			{
				startBattleArmyParams.orderedHand = levelData.PlayerOrderedDeck;
			}
			return startBattleArmyParams;
		}

		public static StartBattleArmyParams GetEnemyArmy(LevelData levelData, ArmyData army, string backgroundName)
		{
			if (levelData != null)
			{
				backgroundName = MyDecorDataHelper.GetBackgroundByName(levelData.settings[UnityEngine.Random.Range(0, levelData.settings.Count)]);
			}
			string text = ((levelData != null) ? levelData.enemyArmy.warlord.GetName() : army.warlord.GetName());
			return new StartBattleArmyParams
			{
				name = text,
				bg = backgroundName,
				controllerType = StartBattleArmyParams.ControllerType.AI,
				army = army
			};
		}

		public static ArmySide GetArmySide(LevelData levelData = null)
		{
			if (levelData == null || levelData.randomFirst)
			{
				if (Common.GetRandomInt(0, 2) != 1)
				{
					goto IL_0026;
				}
			}
			else if (levelData.secondSide != ArmySide.Left)
			{
				goto IL_0026;
			}
			return ArmySide.Right;
			IL_0026:
			return ArmySide.Left;
		}

		public static ArmyData GetPlayerArmyData(LevelData levelData = null)
		{
			if (levelData != null && levelData.userArmy.deck != null && levelData.userArmy.warlord != null)
			{
				return levelData.userArmy;
			}
			ArmyData armyData = UserArmyModule.instance.GetArmyData();
			bool flag = levelData != null && levelData.userArmy.runes != null && levelData.userArmy.runes.Count != 0;
			bool flag2 = levelData != null && levelData.userArmy.fieldMonsters != null && levelData.userArmy.fieldMonsters.Count != 0;
			return new ArmyData
			{
				deck = armyData.deck,
				warlord = armyData.warlord,
				warlordData = armyData.warlordData,
				petData = (Constants.show_pets ? UserArmyModule.instance.GetPetData() : null),
				runes = (flag ? levelData.userArmy.runes : armyData.runes),
				fieldMonsters = (flag2 ? ConvertFieldMonsters(levelData.userArmy.fieldMonsters) : armyData.fieldMonsters)
			};
		}

		private static Dictionary<Place, MonsterData> ConvertFieldMonsters(Dictionary<Place, MonsterData> fields)
		{
			Dictionary<Place, MonsterData> dictionary = new Dictionary<Place, MonsterData>();
			foreach (KeyValuePair<Place, MonsterData> field in fields)
			{
				MonsterData monsterData = MonsterDataUtils.CreateMonster(field.Value.monster_id, field.Value.promote_num);
				if (field.Value.skills != null && monsterData.skills != null)
				{
					foreach (SkillStaticData skill in field.Value.skills)
					{
						if (!monsterData.skills.Contains(skill))
						{
							int num = field.Value.skills.IndexOf(skill);
							if (num > 0 && num < field.Value.skillValues.Count)
							{
								monsterData.skills.Add(skill);
								monsterData.skillValues.Add(field.Value.skillValues[num]);
							}
						}
					}
				}
				monsterData.isLevelMonster = true;
				dictionary.Add(field.Key, monsterData);
			}
			return dictionary;
		}

		public static LevelCompletedWindow.HintType GetLevelComplitedHint(bool isRandom = true)
		{
			if (!isRandom)
			{
				return LevelCompletedWindow.HintType.Brawl;
			}
			if (!(UnityEngine.Random.value < 0.34f))
			{
				if (!(UnityEngine.Random.value < 0.5f))
				{
					return LevelCompletedWindow.HintType.Retry;
				}
				return LevelCompletedWindow.HintType.Promote;
			}
			return LevelCompletedWindow.HintType.Summon;
		}
	}
}
