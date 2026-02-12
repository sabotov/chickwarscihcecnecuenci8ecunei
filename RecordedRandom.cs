using System;
using System.Collections.Generic;
using UnityEngine;

namespace BattlefieldScripts
{
	public class RecordedRandom : FieldRandom
	{
		public Dictionary<int, Dictionary<int, List<int>>> intRandoms;

		private Dictionary<Vector2, int> _countedRandom;

		public void Reset()
		{
			_countedRandom = new Dictionary<Vector2, int>();
		}

		public RecordedRandom()
		{
			intRandoms = new Dictionary<int, Dictionary<int, List<int>>>();
			_countedRandom = new Dictionary<Vector2, int>();
		}

		public override int GetRange(int min, int max)
		{
			Vector2 key = new Vector2(min, max);
			if (!_countedRandom.ContainsKey(key))
			{
				_countedRandom.Add(key, 0);
			}
			if (_countedRandom[key] >= intRandoms[min][max].Count)
			{
				bool flag = true;
				string message = "SOMETHING WRONG IN RECORDINGS!!!! " + intRandoms[min][max].Count + "  " + _countedRandom[key] + " " + min + " " + max + " ";
				if (flag)
				{
					throw new Exception(message);
				}
				Debug.LogError(message);
				return base.GetRange(min, max);
			}
			int result = intRandoms[min][max][_countedRandom[key]];
			_countedRandom[key]++;
			return result;
		}
	}
}
