using System.Collections.Generic;
using System.Linq;
using SharpDX.DirectInput;
using UnityEngine;
using UnityModManagerNet;
using static DV.HUD.InteriorControlsManager;

namespace dvDirectInput
{
	public class Settings : UnityModManager.ModSettings
	{
		public bool configEnableRecentInputGUI = true;

		public List<ConfigControls> configControls = new();

		public class ConfigControls
		{
			public bool Enabled = false;
			public int DeviceId = 0;
			public JoystickOffset DeviceOffset = JoystickOffset.X;
			public bool InvertControl = false;
			public float InputRangeMin = 0f;
			public float InputRangeMax = 1f;
			public float Rate = 1f;
		}

		public override void Save(UnityModManager.ModEntry modEntry)
		{
			Save(this, modEntry);
		}

		public void OnChange() { }

		public void Render()
		{
			GUILayout.Label("Debug");
			GUILayout.BeginVertical();
			this.configEnableRecentInputGUI = GUILayout.Toggle(
				this.configEnableRecentInputGUI,
				"Enable GUI"
			);
			GUILayout.EndVertical();

			GUILayout.Label("Controls");
			GUILayout.BeginVertical();
			foreach (
				var configControl in this.configControls.Select((val, idx) => new { idx, val })
			)
			{
				var style = new GUIStyle
				{
					alignment = TextAnchor.MiddleLeft,
					stretchWidth = false,
				};
				style.normal.textColor = Color.white;
				style.normal.background = Texture2D.grayTexture;

				GUILayout.Label($"\t{(ControlType)configControl.idx}");
				GUILayout.BeginVertical(style);

				configControl.val.Enabled = GUILayout.Toggle(configControl.val.Enabled, "Enabled");

				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Label("Device ID", GUILayout.Width(100));
				string deviceIdText = GUILayout.TextField(configControl.val.DeviceId.ToString());
				if (string.IsNullOrWhiteSpace(deviceIdText))
				{
					configControl.val.DeviceId = 0;
				}
				else if (int.TryParse(deviceIdText, out int deviceId))
				{
					configControl.val.DeviceId = deviceId;
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Label("Device Offset", GUILayout.Width(100));
				string deviceOffsetText = GUILayout.TextField(
					((int)configControl.val.DeviceOffset).ToString()
				);
				if (string.IsNullOrWhiteSpace(deviceOffsetText))
				{
					configControl.val.DeviceOffset = JoystickOffset.X;
				}
				else if (int.TryParse(deviceOffsetText, out int deviceOffset))
				{
					configControl.val.DeviceOffset = (JoystickOffset)deviceOffset;
				}
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Label("Input Range Min", GUILayout.Width(100));
				string rangeMinText = GUILayout.TextField(
					configControl.val.InputRangeMin.ToString(
						"0.0###",
						System.Globalization.CultureInfo.InvariantCulture
					)
				);
				if (
					float.TryParse(
						rangeMinText,
						System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture,
						out float rangeMin
					)
				)
				{
					if (rangeMin < 0f)
						rangeMin = 0f;
					else if (rangeMin > 1f)
						rangeMin = 1f;
					configControl.val.InputRangeMin = rangeMin;
				}
				GUILayout.Space(16);
				GUILayout.Label("Input Range Max", GUILayout.Width(100));
				string rangeMaxText = GUILayout.TextField(
					configControl.val.InputRangeMax.ToString(
						"0.0###",
						System.Globalization.CultureInfo.InvariantCulture
					)
				);
				if (
					float.TryParse(
						rangeMaxText,
						System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture,
						out float rangeMax
					)
				)
				{
					if (rangeMax < 0f)
						rangeMax = 0f;
					else if (rangeMax > 1f)
						rangeMax = 1f;
					configControl.val.InputRangeMax = rangeMax;
				}

				if (configControl.val.InputRangeMin > configControl.val.InputRangeMax)
				{
					configControl.val.InputRangeMin = configControl.val.InputRangeMax;
				}
				GUILayout.EndHorizontal();
				GUILayout.BeginHorizontal(GUILayout.Width(200));
				GUILayout.Label("Rate", GUILayout.Width(100));
				string rateText = GUILayout.TextField(
					configControl.val.Rate.ToString(
						"0.0###",
						System.Globalization.CultureInfo.InvariantCulture
					)
				);
				if (
					float.TryParse(
						rateText,
						System.Globalization.NumberStyles.Float,
						System.Globalization.CultureInfo.InvariantCulture,
						out float rate
					)
				)
					configControl.val.Rate = rate;
				GUILayout.EndHorizontal();
				configControl.val.InvertControl = GUILayout.Toggle(
					configControl.val.InvertControl,
					"Invert"
				);

				GUILayout.EndVertical();
			}
			GUILayout.EndVertical();
		}
	}
}
