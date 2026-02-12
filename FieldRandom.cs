using System;
using UtilScripts;

namespace BattlefieldScripts
{
	public class FieldRandom
	{
		protected Random _random;

		public FieldRandom()
		{
			_random = Common.CloneGlobalRandom();
		}

		public CopiedSimulateRandom GetSimulateCopy()
		{
			return new CopiedSimulateRandom(_random);
		}

		public virtual int GetRange(int min, int max)
		{
			int num = _random.Next(min, max);
			RecordInt(min, max, num);
			return num;
		}

		protected virtual void RecordInt(int min, int max, int val)
		{
		}
	}
}
