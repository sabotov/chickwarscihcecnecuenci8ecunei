using System;
using UtilScripts;

namespace BattlefieldScripts
{
	public class CopiedSimulateRandom : FieldRandom
	{
		public CopiedSimulateRandom()
		{
		}

		public CopiedSimulateRandom(Random random)
		{
			_random = random.Clone();
		}
	}
}
