using UnityEngine;
using UtilScripts;

namespace BattlefieldScripts.Actions
{
	public class IntModulateStat : PerkStat
	{
		private Common.IntDelegate _val;

		public IntModulateStat(Common.IntDelegate val)
		{
			_val = val;
		}

		public override int ValidateInt(int val)
		{
			if (val == 0)
			{
				return 0;
			}
			int num = (int)Mathf.Sign(val);
			int num2 = Mathf.Abs(val);
			int num3 = Mathf.Abs(_val());
			return Mathf.Max(0, num2 - num3) * num;
		}
	}
}
