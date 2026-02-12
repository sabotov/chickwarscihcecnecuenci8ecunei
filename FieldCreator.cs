using System.Collections.Generic;
using BattlefieldScripts.Core;
using NewAssets.Scripts.DataClasses;
using NewAssets.Scripts.Data_Helpers;
using NewAssets.Scripts.UtilScripts;
using UnityEngine;

namespace BattlefieldScripts
{
	public class FieldCreator
	{
		public static Vector3 LEFT_ANGLE = new Vector3(0f, 0f, 0f);

		public static Vector3 RIGHT_ANGLE = new Vector3(0f, 180f, 0f);

		private const float TILE_WIDTH = 99f;

		private const float TILE_HEIGHT = 73f;

		private const float Z_SHIFT = -5f;

		private const float RUNE_DELTA = 2f;

		private const float WARLORD_X_SHIFT = 38.5f;

		private const float PET_X_SHIFT = -147.5f;

		public static Vector2 TILE_RECT = new Vector2(99f, 73f);

		private static List<Tile> fieldTiles = new List<Tile>();

		private FieldController _field;

		private FieldScriptWrapper _wrapper;

		private static int _fieldWidth;

		private static int _fieldHeight;

		private string _curDecor = "";

		private Transform _tilesContainer;

		private Transform _monstersContainer;

		public static IEnumerable<Tile> GetListFieldTiles => fieldTiles.AsReadOnly();

		public static float GetZShift => -5f;

		public static float FIELD_WIDTH => _fieldWidth;

		public static float FIELD_HEIGHT => _fieldHeight;

		private Transform tilesContainer => _tilesContainer ?? (_tilesContainer = _wrapper.transform.Find("FieldContainer/TilesContainer"));

		private Transform monstersContainer => _monstersContainer ?? (_monstersContainer = _wrapper.transform.Find("FieldContainer/MonstersContainer"));

		public void Init(FieldScriptWrapper wrapper, FieldController field, List<Tile> tiles)
		{
			_field = field;
			_wrapper = wrapper;
			fieldTiles = tiles;
			foreach (Tile fieldTile in fieldTiles)
			{
				fieldTile.SetTileHighlight(Tile.TileHighlightning.NotHighlighted);
			}
		}

		public void UpdateFieldBoosts(MonsterData data)
		{
			_field.UpdateBoostData(data);
		}

		public void InitDecor(string decor)
		{
			_curDecor = decor;
		}

		public string GetDecor()
		{
			return _curDecor;
		}

		public void CreateBattlefield(int width, int height, string decor = "wood")
		{
			_fieldWidth = width;
			_fieldHeight = height;
		}

		public void ClearBattlefield()
		{
			for (int num = monstersContainer.childCount - 1; num > -1; num--)
			{
				Transform child = monstersContainer.GetChild(num);
				if (child.GetComponent<FieldMonsterVisual>() != null)
				{
					child.GetComponent<FieldMonsterVisual>().Destroy();
				}
				else
				{
					Object.Destroy(child.gameObject);
				}
			}
		}

		public BubbleElement CreateBubble()
		{
			BubbleElement bubbleElement = BubbleElement.CreateBubble();
			bubbleElement.transform.parent = monstersContainer;
			bubbleElement.transform.localScale = new Vector3(1f, 1f, 1f);
			return bubbleElement;
		}

		public FieldRuneVisual PlaceRune(ArmySide side, Vector2 place, string prefab, string name)
		{
			FieldRuneVisual fieldRuneVisual = PrefabCreator.CreateRune(monstersContainer, side, prefab, "warlord_" + side);
			Tile tile = fieldTiles.Find((Tile x) => x.Coords == place);
			int num = (int)place.y;
			float num2 = 0.75f + 0.1f * (float)num;
			fieldRuneVisual.SetPosition(tile.framePosition);
			fieldRuneVisual.panel.isChildPanel = true;
			fieldRuneVisual.ConvertZtoDepth(-5f * (place.y + 1f) + 2f);
			fieldRuneVisual.transform.localScale = new Vector3(num2, num2, 0.1f);
			return fieldRuneVisual;
		}

		public FieldMonsterVisual PlaceWarlord(ArmySide side)
		{
			Vector3 angle = ((side == ArmySide.Left) ? LEFT_ANGLE : RIGHT_ANGLE);
			WarlordMonsterVisual warlordMonsterVisual = PrefabCreator.CreateWarlordMonster(monstersContainer, side, "warlord_" + side);
			warlordMonsterVisual.ApplyEulerAngles(angle);
			if (side != ArmySide.Left)
			{
				_ = (float)(_fieldWidth - 1) / 2f;
			}
			else
			{
				_ = (0f - (float)(_fieldWidth - 1)) / 2f;
			}
			warlordMonsterVisual.transform.localPosition = new Vector3((float)((side != ArmySide.Left) ? 1 : (-1)) * ((float)(_fieldWidth + 1) * 99f / 2f + 38.5f), -120f, -5f);
			warlordMonsterVisual.ApplyColorTint(MyDecorDataHelper.GetUnitTint(_curDecor));
			return warlordMonsterVisual;
		}

		public FieldMonsterVisual PlacePet(ArmySide side)
		{
			Vector3 angle = ((side == ArmySide.Left) ? LEFT_ANGLE : RIGHT_ANGLE);
			FieldMonsterVisual fieldMonsterVisual = PrefabCreator.CreatePetMonster(monstersContainer, side, "pet_" + side);
			fieldMonsterVisual.ApplyEulerAngles(angle);
			fieldMonsterVisual.transform.localPosition = new Vector3((float)((side == ArmySide.Left) ? (-2) : 2) * ((float)(_fieldWidth + 1) * 99f / 2f + -147.5f), -178f, -7f);
			fieldMonsterVisual.ApplyColorTint(MyDecorDataHelper.GetUnitTint(_curDecor));
			return fieldMonsterVisual;
		}

		public FieldMonsterVisual PlaceUnit(ArmySide side, Vector2 place, string name)
		{
			Tile tile = fieldTiles.Find((Tile x) => x.Coords == place);
			Vector3 angle = ((side == ArmySide.Left) ? LEFT_ANGLE : RIGHT_ANGLE);
			FieldMonsterVisual fieldMonsterVisual = PrefabCreator.CreateFieldMonster(monstersContainer, side, name);
			int yCoord = (int)place.y;
			float size = GetSize(yCoord);
			fieldMonsterVisual.ApplyEulerAngles(angle);
			Vector3 position = tile.transform.position;
			fieldMonsterVisual.SetPosition(position);
			fieldMonsterVisual.ConvertZtoDepth(-5f * (place.y + 1f));
			fieldMonsterVisual.transform.localScale = new Vector3(size, size, 1f);
			fieldMonsterVisual.ApplyColorTint(GetMonsterTintColor());
			return fieldMonsterVisual;
		}

		private float GetSize(int yCoord)
		{
			switch (yCoord)
			{
			case 0:
				return 0.85f;
			case 1:
				return 0.9f;
			case 2:
				return 0.95f;
			case 3:
				return 1f;
			default:
				return 1f;
			}
		}

		public List<Tile> GetFieldTiles()
		{
			return fieldTiles;
		}

		public int GetFieldWidth()
		{
			return _fieldWidth;
		}

		public int GetFieldHeight()
		{
			return _fieldHeight;
		}

		public Color GetMonsterTintColor()
		{
			return MyDecorDataHelper.GetUnitTint(_curDecor);
		}
	}
}
