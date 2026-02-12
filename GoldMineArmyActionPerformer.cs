using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.DataClasses;
using Assets.Scripts.DataClasses.UserData;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.UserData;
using NewAssets.Scripts.UtilScripts;
using UI_Scripts.WindowManager;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts
{
	internal class GoldMineArmyActionPerformer : ArmyActionPerformer
	{
		private GoldMineData _mineData;

		private BattlefieldWindow _window;

		private int _turnsLeft;

		private int _monsterPlaced;

		private int _totalGoldAmount;

		private Dictionary<ArmySide, SimulateEnviroment.SimulateArmy> curArmies;

		private List<Dictionary<ArmySide, SimulateEnviroment.SimulateArmy>> afterArmies;

		private Action<MonsterData, Vector2> _onChosen;

		public override void Init(Func<List<MonsterData>> enemyHand, Func<List<TriggerType>> enemySkills, ArmyControllerCore thisController, FieldParameters parameters)
		{
			base.Init(enemyHand, enemySkills, thisController, parameters);
			_mineData = GoldMineModule.GetCurrentMineData();
			_turnsLeft = _mineData.TurnCount + 1;
			_totalGoldAmount = 0;
		}

		public override void PerformPlacingChoose(List<MonsterData> hand, List<TriggerType> skills, Action<MonsterData, Vector2> onChosen, Action<TriggerType> onSkill, bool isAfterSkill = false, int currentTurn = 0)
		{
			_monsterPlaced = 0;
			_onChosen = onChosen;
			_turnsLeft--;
			GoldMineModule.SaveEarnedGold(_turnsLeft);
			if (_turnsLeft <= 0)
			{
				FieldScriptWrapper.instance.fieldController.InformDefeat(_side);
			}
			else
			{
				_onChosen(null, Vector2.zero);
			}
		}

		public void AttachBattlefieldWindow(BattlefieldWindow window)
		{
			_window = window;
			_window.SetDark(dark: false);
		}

		public override void InformMonsterHit(FieldMonster data, bool isWarlord = false)
		{
			if (!isWarlord || _monsterPlaced >= Constants.gold_mine_max_monster_placed_by_turn)
			{
				return;
			}
			_thisController.ForceEnablePlacing();
			MonsterData monToPlace = _thisController.GetHand().GetRandom();
			monToPlace.health = Common.GetRandomInt(_mineData.MonsterHp - _mineData.MonsterHpDelta, _mineData.MonsterHp + _mineData.MonsterHpDelta + 1);
			monToPlace.goldCount = Common.GetRandomInt(_mineData.MonsterGold - _mineData.MonsterGoldDelta, _mineData.MonsterGold + _mineData.MonsterGoldDelta + 1);
			Vector2 place = _parameters.GetClassedTiles(monToPlace.monsterClass, _side).ToList().GetRandom();
			if (place != default(Vector2))
			{
				GameObject effect = CreateEffect();
				Tile tile = FieldScriptWrapper.instance.fieldCreator.GetFieldTiles().Find((Tile a) => a.Coords == place);
				Vector3 position = data.VisualMonster.transform.position;
				position.z = -5f;
				Vector3 position2 = tile.transform.position;
				position2.z = -5f;
				float angle = -(float)Math.PI / 8f;
				effect.transform.position = position;
				effect.transform.AnimateParabolic(position2, angle, 0.2f, delegate
				{
					_monsterPlaced++;
					UnityEngine.Object.Destroy(effect);
					_thisController.PlaceMonster(monToPlace, place, delegate
					{
					});
				});
			}
		}

		private GameObject CreateEffect()
		{
			return UnityEngine.Object.Instantiate(WindowLoader.GetPurePref("fx_trail_gold_mine", "Prefabs/BattlePrefabs/BattleEffects/"));
		}

		public override void InformMonsterDead(FieldMonster data, bool isWarlord = false)
		{
			if (!isWarlord)
			{
				int goldCount = data.data.goldCount;
				_window.AddGold(data.coords, goldCount);
				_totalGoldAmount += goldCount;
				GoldMineModule.SetEarnedGold(_totalGoldAmount);
			}
		}

		public override void InformStop()
		{
		}
	}
}
