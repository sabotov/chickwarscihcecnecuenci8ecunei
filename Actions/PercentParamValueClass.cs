using System;

namespace BattlefieldScripts.Actions
{
	public class PercentParamValueClass
	{
		private ParamType type;

		private int percentValue;

		public PercentParamValueClass(ParamType type, int value)
		{
			this.type = type;
			percentValue = value;
		}

		public int GetCalculatedValue(FieldMonster monster)
		{
			if (monster == null)
			{
				return 1;
			}
			return Convert.ToInt16(Math.Ceiling((double)(((type == ParamType.Attack) ? monster.Attack : ((int)monster.Health)) * percentValue) / 100.0));
		}
	}
}
