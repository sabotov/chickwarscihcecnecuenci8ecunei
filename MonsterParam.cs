using BattlefieldScripts.Actions;
using UnityEngine;

namespace BattlefieldScripts
{
	public class MonsterParam
	{
		private enum Type
		{
			Adding = 0,
			Value = 1
		}

		private Type _type;

		private int _value;

		private TriggerType _counterReduceTrigger;

		private int _counter = -1;

		private MonsterParam _next;

		public MonsterParam(int value, MonsterParam next = null, TriggerType trigger = TriggerType.NoTrigger, int counter = -1)
		{
			_value = value;
			_next = next;
			_counterReduceTrigger = trigger;
			_counter = counter;
			_type = Type.Adding;
		}

		public void MakeForce()
		{
			_type = Type.Value;
		}

		public MonsterParam GetTrigger(TriggerType trigger)
		{
			if (_next != null && _next != this)
			{
				_next = _next.GetTrigger(trigger);
			}
			if (trigger == TriggerType.ClearParams)
			{
				return _next ?? this;
			}
			if (_counterReduceTrigger != trigger)
			{
				return this;
			}
			_counter--;
			if (_counter == 0)
			{
				return _next;
			}
			return this;
		}

		public int GetValue()
		{
			int b = ((_type != Type.Adding) ? _value : ((_next != null) ? (_next.GetValue() + _value) : _value));
			return Mathf.Max(0, b);
		}

		public MonsterParam Cleanse()
		{
			if (_type == Type.Value || _value < 0)
			{
				if (_next != null)
				{
					return _next.Cleanse();
				}
				return null;
			}
			if (_next != null)
			{
				_next = _next.Cleanse();
			}
			return this;
		}

		public MonsterParam Clone()
		{
			return new MonsterParam(_value, (_next != null) ? _next.Clone() : null, _counterReduceTrigger, _counter)
			{
				_type = _type
			};
		}
	}
}
