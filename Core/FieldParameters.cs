using System.Collections.Generic;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.DataClasses.MonsterParams;
using UnityEngine;

namespace BattlefieldScripts.Core
{
	public class FieldParameters
	{
		private ArmyControllerCore _leftController;

		private ArmyControllerCore _rightController;

		public int skillDrawDelay;

		public int skillDrawShift;

		private static object _lockAllPlObj = new object();

		private static volatile List<Vector2> _allPlaces;

		private static object _lockArmyObj = new object();

		private static volatile Dictionary<ArmySide, List<Vector2>> _armyTiles;

		private static object _lockClassedObj = new object();

		private static volatile Dictionary<ArmySide, Dictionary<Class, List<Vector2>>> ClassedTilesHashe = new Dictionary<ArmySide, Dictionary<Class, List<Vector2>>>
		{
			{
				ArmySide.Left,
				new Dictionary<Class, List<Vector2>>()
			},
			{
				ArmySide.Right,
				new Dictionary<Class, List<Vector2>>()
			}
		};

		public int width { get; private set; }

		public int height { get; private set; }

		public void Init(int nWidth, int nHeight)
		{
			width = nWidth;
			height = nHeight;
		}

		public void InitSkillStuff(int drawDelay, int drawShift)
		{
			skillDrawDelay = drawDelay;
			skillDrawShift = drawShift;
		}

		public void AttachControllers(ArmyControllerCore left, ArmyControllerCore right)
		{
			_leftController = left;
			_rightController = right;
		}

		public List<MonsterData> GetStartDeck(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.GetStartDeck();
			case ArmySide.Right:
				return _rightController.GetStartDeck();
			default:
				return new List<MonsterData>();
			}
		}

		public List<MonsterData> GetDeck(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.GetDeck();
			case ArmySide.Right:
				return _rightController.GetDeck();
			default:
				return new List<MonsterData>();
			}
		}

		public List<MonsterData> GetHand(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.GetHand();
			case ArmySide.Right:
				return _rightController.GetHand();
			default:
				return new List<MonsterData>();
			}
		}

		public FieldMonster GetWarlord(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.GetWarlord();
			case ArmySide.Right:
				return _rightController.GetWarlord();
			default:
				return null;
			}
		}

		public Dictionary<Vector2, FieldMonster> GetMonsters(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.GetFieldMonsters();
			case ArmySide.Right:
				return _rightController.GetFieldMonsters();
			default:
				return new Dictionary<Vector2, FieldMonster>();
			}
		}

		public Dictionary<Vector2, FieldRune> GetRunes(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.GetFieldRunes();
			case ArmySide.Right:
				return _rightController.GetFieldRunes();
			default:
				return new Dictionary<Vector2, FieldRune>();
			}
		}

		public bool CanPlace(ArmySide side)
		{
			switch (side)
			{
			case ArmySide.Left:
				return _leftController.CanPlace();
			case ArmySide.Right:
				return _rightController.CanPlace();
			default:
				return false;
			}
		}

		private void InitAllTiles()
		{
			lock (_lockAllPlObj)
			{
				if (_allPlaces == null)
				{
					_allPlaces = new List<Vector2>(GetArmyTiles(ArmySide.Left));
					_allPlaces.AddRange(GetArmyTiles(ArmySide.Right));
				}
			}
		}

		public List<Vector2> AllPlaces()
		{
			if (_allPlaces == null)
			{
				InitAllTiles();
			}
			return _allPlaces;
		}

		public IEnumerable<Vector2> AllEmptyPlaces()
		{
			foreach (Vector2 item in AllPlaces())
			{
				if (!GetMonsters(ArmySide.Left).ContainsKey(item) && !GetMonsters(ArmySide.Right).ContainsKey(item))
				{
					yield return item;
				}
			}
		}

		public IEnumerable<Vector2> AllRunesAndEmptyPlaces()
		{
			foreach (Vector2 item in AllPlaces())
			{
				if (GetRunes(ArmySide.Left).ContainsKey(item) || GetRunes(ArmySide.Right).ContainsKey(item) || (!GetMonsters(ArmySide.Left).ContainsKey(item) && !GetMonsters(ArmySide.Right).ContainsKey(item)))
				{
					yield return item;
				}
			}
		}

		private void InitArmyTiles()
		{
			lock (_lockArmyObj)
			{
				if (_armyTiles != null)
				{
					return;
				}
				_armyTiles = new Dictionary<ArmySide, List<Vector2>>();
				foreach (ArmySide item in new List<ArmySide>
				{
					ArmySide.Left,
					ArmySide.Right
				})
				{
					List<Vector2> list = new List<Vector2>();
					if (item == ArmySide.Left)
					{
						for (int i = 0; i < width / 2; i++)
						{
							for (int j = 0; j < height; j++)
							{
								list.Add(new Vector2(i, j));
							}
						}
					}
					else
					{
						for (int k = width / 2; k < width; k++)
						{
							for (int l = 0; l < height; l++)
							{
								list.Add(new Vector2(k, l));
							}
						}
					}
					_armyTiles.Add(item, list);
				}
			}
		}

		public List<Vector2> GetArmyTiles(ArmySide side)
		{
			if (_armyTiles == null)
			{
				InitArmyTiles();
			}
			return _armyTiles[side];
		}

		public IEnumerable<Vector2> GetClassedTiles(Class cls, ArmySide side)
		{
			Dictionary<Vector2, FieldMonster> monsters = GetMonsters(side);
			return GetClassedTiles(cls, monsters, side);
		}

		public IEnumerable<Vector2> GetClassedTiles(Class cls, Dictionary<Vector2, FieldMonster> fMonsters, ArmySide side)
		{
			if (!ClassedTilesHashe[side].ContainsKey(cls))
			{
				lock (_lockClassedObj)
				{
					if (!ClassedTilesHashe[side].ContainsKey(cls))
					{
						List<Vector2> list = new List<Vector2>();
						List<Vector2> armyTiles = GetArmyTiles(side);
						int num = -1;
						int num2 = 1000;
						switch (cls)
						{
						case Class.Ranged:
							if (side == ArmySide.Left)
							{
								num = 0;
								num2 = width / 4;
							}
							if (side == ArmySide.Right)
							{
								num = width * 3 / 4;
								num2 = width - 1;
							}
							break;
						case Class.Melee:
							if (side == ArmySide.Left)
							{
								num = width / 4;
								num2 = width / 2 - 1;
							}
							if (side == ArmySide.Right)
							{
								num = width / 2;
								num2 = width * 3 / 4;
							}
							break;
						}
						foreach (Vector2 item in armyTiles)
						{
							if (item.x >= (float)num && item.x <= (float)num2)
							{
								list.Add(item);
							}
						}
						ClassedTilesHashe[side].Add(cls, list);
					}
				}
			}
			foreach (Vector2 item2 in ClassedTilesHashe[side][cls])
			{
				if (!fMonsters.ContainsKey(item2))
				{
					yield return item2;
				}
			}
		}

		public int GetUpkeepCount(ArmySide side, bool visual = false)
		{
			if (side == ArmySide.Left)
			{
				return _leftController.GetUpkeepCount(visual);
			}
			return _rightController.GetUpkeepCount(visual);
		}

		public int GetTurn(bool visual = false)
		{
			if (_leftController.CheckUpkeepEquality())
			{
				return _leftController.GetUpkeepCount(visual);
			}
			return _leftController.GetUpkeepCount(visual) - 1;
		}
	}
}
