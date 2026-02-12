using System;
using System.Collections.Generic;
using BattlefieldScripts.Core;
using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class SoundedEffectAnimation : BitActionAnimation
	{
		private readonly GameObject _effect;

		private readonly string _sound;

		public SoundedEffectAnimation(string name, string sound)
		{
			_effect = EffectAnimation.GetEffect(name);
			_sound = sound;
		}

		public override void Animate(int line, ArmySide side = ArmySide.Right, bool isColumn = false)
		{
			SoundManager.Instance.PlaySound(_sound);
			if (isColumn)
			{
				AnimateColumns(line, side);
			}
			else
			{
				AnimateLines(line, side);
			}
		}

		private void AnimateLines(int line, ArmySide side)
		{
			int num = 0;
			int num2 = (int)FieldCreator.FIELD_WIDTH;
			if (side == ArmySide.Right)
			{
				num = num2 / 2;
			}
			else
			{
				num2 /= 2;
			}
			for (int i = num; i < num2; i++)
			{
				foreach (Tile getListFieldTile in FieldCreator.GetListFieldTiles)
				{
					if (getListFieldTile.Coords.y == (float)line && getListFieldTile.Coords.x == (float)i)
					{
						GameObject gameObject = UnityEngine.Object.Instantiate(_effect);
						if (gameObject != null)
						{
							gameObject.transform.position = new Vector3(getListFieldTile.transform.position.x, getListFieldTile.transform.position.y, getListFieldTile.transform.position.z + FieldCreator.GetZShift * (float)line - 10f);
							gameObject.transform.localScale = Vector3.one;
						}
					}
				}
			}
		}

		private void AnimateColumns(int line, ArmySide side)
		{
			line = ((side == ArmySide.Right) ? (line + 3) : line);
			foreach (Tile getListFieldTile in FieldCreator.GetListFieldTiles)
			{
				if (getListFieldTile.Coords.x == (float)line)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(_effect);
					if (gameObject != null)
					{
						gameObject.transform.position = new Vector3(getListFieldTile.transform.position.x, getListFieldTile.transform.position.y, getListFieldTile.transform.position.z + FieldCreator.GetZShift * (float)line - 10f);
						gameObject.transform.localScale = Vector3.one;
					}
				}
			}
		}

		public override void Animate(Dictionary<Common.StringDelegate, FieldVisual> monstersAction, Action onEnded)
		{
			SoundManager.Instance.PlaySound(_sound);
			foreach (KeyValuePair<Common.StringDelegate, FieldVisual> item in monstersAction)
			{
				if (_effect != null && item.Value != null)
				{
					GameObject gameObject = UnityEngine.Object.Instantiate(_effect, item.Value.transform);
					if (gameObject != null)
					{
						gameObject.transform.localPosition = new Vector3(0f, 0f, -2f);
					}
				}
				item.Key();
			}
			onEnded();
		}
	}
}
