using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui Agent/Agent Effect Enabler")]
public class GluiAgent_Effect : GluiAgentBase
{
	private Order order_For_Enable;

	private Order order_For_Disable = Order.Disable;

	private Order order_For_Reverse = Order.Enable_Reverse;

	private Order order_For_Kill = Order.Kill;

	private Vector3? orderPosition;

	public List<EffectContainer> containers;

	public override void OnDestroy()
	{
		base.OnDestroy();
		Activity_Kill();
	}

	public override bool HandleOrder(GluiOrderPacket orderPacket)
	{
		if (orderPacket.order == order_For_Enable)
		{
			GetOrderMetadata(orderPacket.tag);
			Activity_Enable();
			return true;
		}
		if (orderPacket.order == order_For_Disable)
		{
			Activity_Disable();
			return true;
		}
		if (orderPacket.order == order_For_Reverse)
		{
			GetOrderMetadata(orderPacket.tag);
			Activity_Enable_Reverse();
			return true;
		}
		if (orderPacket.order == order_For_Kill)
		{
			Activity_Kill();
			return true;
		}
		return false;
	}

	private void GetOrderMetadata(object tag)
	{
		if (tag is GameObject)
		{
			orderPosition = ObjectUtils.GetObjectScreenPosition((GameObject)tag);
		}
		else
		{
			orderPosition = null;
		}
	}

	private void ApplyOrderMetadata(EffectContainer container)
	{
		if (orderPosition.HasValue)
		{
			container.SetStartPosition(orderPosition.Value);
		}
	}

	public virtual void Activity_Enable()
	{
		containers.ForEach(Activity_Enable);
	}

	protected void Activity_Enable(EffectContainer container)
	{
		if (!(container == null))
		{
			container.EffectEnable();
			container.Conductors.ForEach(delegate(EffectConductor conductor)
			{
				conductor.Activity_Start();
			});
			ApplyOrderMetadata(container);
		}
	}

	public virtual void Activity_Enable_Reverse()
	{
		containers.ForEach(Activity_Enable_Reverse);
	}

	protected void Activity_Enable_Reverse(EffectContainer container)
	{
		if (!(container == null))
		{
			container.EffectEnable();
			container.Conductors.ForEach(delegate(EffectConductor conductor)
			{
				conductor.Activity_Reverse();
			});
			ApplyOrderMetadata(container);
		}
	}

	public virtual void Activity_Disable()
	{
		containers.ForEach(Activity_Disable);
	}

	protected void Activity_Disable(EffectContainer container)
	{
		if (!(container == null))
		{
			container.EffectDisable();
			container.Conductors.ForEach(delegate(EffectConductor conductor)
			{
				conductor.Activity_Stop();
			});
		}
	}

	public virtual void Activity_Kill()
	{
		containers.ForEach(delegate(EffectContainer container)
		{
			if (container != null)
			{
				container.EffectKillNow();
			}
		});
		containers.Clear();
	}

	protected virtual void Remove(EffectContainer container)
	{
		container.EffectKill();
		containers.Remove(container);
	}
}
