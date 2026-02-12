using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;
using UserData;

namespace BattlefieldScripts
{
	public class TestArmyController : ArmyControllerCore
	{
		private ArmyData _startData;

		public TestArmyController(ArmySide thisSide)
			: base(thisSide)
		{
		}

		public void Init(FieldControllerCore controller, ArmyActionPerformer actionPerformer, FieldParameters fieldParameters, CopiedSimulateRandom random, DrawType drawType, FieldScriptWrapper.StartBattleArmyParams.BattleType battleType, IteratorCore iterator, ArmyData data)
		{
			_startData = data;
			Init(controller, actionPerformer, fieldParameters, random, iterator, drawType, battleType);
		}

		public override void CreateArmy()
		{
			base.CreateArmy();
			foreach (KeyValuePair<Place, MonsterData> fieldMonster in _startData.fieldMonsters)
			{
				SilentlyPlaceMonster(fieldMonster.Value, fieldMonster.Key);
			}
			foreach (KeyValuePair<Place, RuneData> rune in _startData.runes)
			{
				PlaceRune(rune.Value, rune.Key);
			}
			_deck = new List<MonsterData>();
			foreach (KeyValuePair<int, MonsterData> item in _startData.deck)
			{
				_deck.Add(item.Value);
			}
			_warlord = new SimulateFieldMonster
			{
				coords = new Vector2((base.Side == ArmySide.Left) ? (-1) : 10, 1f)
			};
			_warlord.Init(this, _startData.warlord, null, _parameters, _random, _iterator);
			_hand = new List<MonsterData>();
			_handDraw = _startData.handDraw;
			if (_startData.petData != null)
			{
				_pet = new SimulateFieldMonster
				{
					coords = new Vector2((base.Side == ArmySide.Left) ? (-2) : 10, 2f)
				};
				_pet.Init(this, _startData.petData.petMonsterData, null, _parameters, _random, _iterator);
			}
			_availableSkills = new List<TriggerType>();
			_upkeepCount = 0;
			_actionPerformer.InformArmyCreated();
		}

		public override void PlaceRune(RuneData runeData, Vector2 place)
		{
			if (_parameters.GetArmyTiles(base.EnemySide).Contains(place))
			{
				_controller.RequestRuneAdding(base.Side, runeData, place);
				return;
			}
			SimulateFieldRune simulateFieldRune = new SimulateFieldRune
			{
				coords = place
			};
			simulateFieldRune.Init(this, runeData, null, _parameters, _random, _iterator);
			_runes.Add(place, simulateFieldRune);
		}

		protected override void SilentlyPlaceMonster(MonsterData monster, Vector2 place)
		{
			SimulateFieldMonster simulateFieldMonster = new SimulateFieldMonster
			{
				coords = place
			};
			simulateFieldMonster.Init(this, monster, null, _parameters, _random, _iterator);
			if (_fieldMonsters.ContainsKey(place))
			{
				Debug.LogError(string.Concat("SilentlyPlaceMonster ", monster.image, " ", place, " HasKey"));
			}
			_fieldMonsters.Add(place, simulateFieldMonster);
		}
	}
}
