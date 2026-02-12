using NGUI.Scripts.Internal;
using NGUI.Scripts.UI;
using NewAssets.Scripts.DataClasses;
using UnityEngine;

namespace BattlefieldScripts
{
	public class FieldRuneVisual : FieldVisual
	{
		private Transform _collideElement;

		public void Init(RuneData data)
		{
			_collideElement = base.transform.Find("CollideElement");
			if (_collideElement == null)
			{
				GameObject gameObject = new GameObject("CollideElement");
				gameObject.transform.parent = base.transform;
				_collideElement = gameObject.transform;
			}
			Transform transform = base.transform.Find("Value");
			if (transform != null)
			{
				UILabel component = transform.GetComponent<UILabel>();
				if (component != null && data.skillValues.Count > 0)
				{
					component.text = Localization.Localize("#rune_val_label_" + data.name).Replace("%val%", string.Concat(data.skillValues[0]));
				}
			}
		}

		public override bool Collided(Vector3 position)
		{
			if (base.transform == null)
			{
				return false;
			}
			bool num = position.y > _collideElement.position.y - _collideElement.lossyScale.y / 2f && position.y < _collideElement.position.y + _collideElement.lossyScale.y / 2f;
			bool flag = position.x > _collideElement.position.x - _collideElement.lossyScale.x / 2f && position.x < _collideElement.position.x + _collideElement.lossyScale.x / 2f;
			return num && flag;
		}
	}
}
