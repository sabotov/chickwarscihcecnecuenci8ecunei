using System.Collections.Generic;
using BattlefieldScripts.Actions;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts
{
	public class SimulateArmyController : ArmyControllerCore
	{
		private SimulateEnviroment.SimulateArmy _startData;

		public void Init(FieldControllerCore controller, ArmyActionPerformer actionPerformer, FieldParameters fieldParameters, CopiedSimulateRandom random, IteratorCore iterator, SimulateEnviroment.SimulateArmy data)
		{
			_startData = data;
			Init(controller, actionPerformer, fieldParameters, random, iterator, data.drawType, data.battleType);
			if (data.drawType == DrawType.NewSurvival)
			{
				AttachRaritySequence(data.raritySequence);
			}
		}

		public override void CreateArmy()
		{
			base.CreateArmy();
			foreach (KeyValuePair<Vector2, FieldMonster> item in _startData.army)
			{
				PlaceCopyMonster(item.Value, item.Key, _parameters);
			}
			foreach (KeyValuePair<Vector2, FieldRune> rune in _startData.runes)
			{
				PlaceCopyRune(rune.Value, rune.Key, _parameters);
			}
			_deck = new List<MonsterData>();
			SimulateFieldMonster simulateFieldMonster = new SimulateFieldMonster();
			simulateFieldMonster.coords = new Vector2((base.Side == ArmySide.Left) ? (-1) : 10, 1f);
			simulateFieldMonster.InitFromOriginalWarlord(this, _startData.warlord, _parameters, _random, _iterator);
			_warlord = simulateFieldMonster;
			_hand = _startData.hand.ConvertAll((MonsterData x) => x);
			_deck = _startData.deck.ConvertAll((MonsterData x) => x);
			_reserveMonsters = _startData.reserveMosnter.ConvertAll((MonsterData x) => x);
			_availableSkills = ((_startData.availableSkills == null) ? new List<TriggerType>() : new List<TriggerType>(_startData.availableSkills));
			_upkeepCount = _startData.upkeepCount;
		}

		private void PlaceCopyMonster(FieldMonster original, Vector2 place, FieldParameters fieldParameters)
		{
			SimulateFieldMonster simulateFieldMonster = new SimulateFieldMonster();
			simulateFieldMonster.InitFromOriginal(this, original, fieldParameters, _random, _iterator);
			simulateFieldMonster.coords = place;
			_fieldMonsters.Add(place, simulateFieldMonster);
		}

		private void PlaceCopyRune(FieldRune original, Vector2 place, FieldParameters fieldParameters)
		{
			SimulateFieldRune simulateFieldRune = new SimulateFieldRune();
			simulateFieldRune.InitFromOriginal(this, original, fieldParameters, _random, _iterator);
			simulateFieldRune.coords = place;
			_runes.Add(place, simulateFieldRune);
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

		public override void PerformMonsterDead(Vector2 coords)
		{
			if (coords != _warlord.coords)
			{
				_fieldMonsters.Remove(coords);
			}
		}

		public SimulateArmyController(ArmySide thisSide)
			: base(thisSide)
		{
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
