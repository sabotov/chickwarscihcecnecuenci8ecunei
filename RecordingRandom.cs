using System.Collections.Generic;

namespace BattlefieldScripts
{
	public class RecordingRandom : FieldRandom
	{
		private readonly RecordedRandom _record;

		public RecordingRandom()
		{
			_record = new RecordedRandom();
		}

		protected override void RecordInt(int min, int max, int val)
		{
			base.RecordInt(min, max, val);
			if (!_record.intRandoms.ContainsKey(min))
			{
				_record.intRandoms.Add(min, new Dictionary<int, List<int>>());
			}
			if (!_record.intRandoms[min].ContainsKey(max))
			{
				_record.intRandoms[min].Add(max, new List<int>());
			}
			_record.intRandoms[min][max].Add(val);
		}

		public RecordedRandom GetRecord()
		{
			return _record;
		}
	}
}
