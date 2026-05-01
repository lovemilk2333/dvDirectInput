using System.Linq;
using DV.HUD;
using UnityEngine;
using static DV.HUD.InteriorControlsManager;

namespace dvDirectInput
{
	public static class LocoControl
	{
		public static void ApplyInputs()
		{
			while (Input.inputQueue.Count > 0)
			{
				// Dont bother doing anything if we arent in a loco
				if (!PlayerManager.Car?.IsLoco ?? true)
				{
					// Probbaly not going to get in a loco this game update so just clear the queue
					Input.inputQueue.Clear();
					break;
				}

				// Eat up all the queue items
				var input = Input.inputQueue.Dequeue();

				// Assign Inputs
				foreach (
					var configControl in Main.settings.configControls.Select(
						(val, idx) => new { idx, val }
					)
				)
				{
					// We should probably do a lookup for the inputs against the mappings instead of iterating
					if (
						configControl.val.Enabled
						&& input.JoystickObj.Properties.JoystickId == configControl.val.DeviceId
						&& input.Offset == configControl.val.DeviceOffset
					)
					{
						var control = new ControlReference();
						if (
							!PlayerManager
								.Car?.interior.GetComponentInChildren<InteriorControlsManager>()
								.TryGetControl((ControlType)configControl.idx, out control)
							?? true
						)
							return;
						float value = input.NormalisedValue();

						float min = configControl.val.InputRangeMin;
						float max = configControl.val.InputRangeMax;
						if (value <= min)
							value = 0f;
						else if (value >= max)
							value = 1f;
						else
							value = (value - min) / (max - min);

						value *= configControl.val.Rate;
						Mathf.Clamp(value, 0f, 1f);

						if (configControl.val.InvertControl)
							value = 1.0f - value;
						control.controlImplBase?.SetValue(value);

						// One axis may control two or more functions if `InputRangeMin` and/or `InputRangeMax` set
						// break;
					}
				}
			}
		}
	}
}
