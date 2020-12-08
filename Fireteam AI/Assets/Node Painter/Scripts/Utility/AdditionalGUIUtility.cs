using System;
using UnityEngine;

namespace TerrainComposer2.NodePainter.Utilities
{
	public static class AdditionalGUIUtility
	{
		#region Seperator

		/// <summary>
		/// Efficient space like EditorGUILayout.Space
		/// </summary>
		public static void Space ()
		{
			Space (6);
		}
		/// <summary>
		/// Space like GUILayout.Space but more efficient
		/// </summary>
		public static void Space (float pixels)
		{
			GUILayoutUtility.GetRect (pixels, pixels);
		}


		/// <summary>
		/// A GUI Function which simulates the default seperator
		/// </summary>
		public static void Seperator () 
		{
			setupSeperator ();
			GUILayout.Box (GUIContent.none, seperator, new GUILayoutOption[] { GUILayout.Height (1) });
		}

		/// <summary>
		/// A GUI Function which simulates the default seperator
		/// </summary>
		public static void Seperator (Rect rect) 
		{
			setupSeperator ();
			GUI.Box (new Rect (rect.x, rect.y, rect.width, 1), GUIContent.none, seperator);
		}

		private static GUIStyle seperator;
		private static void setupSeperator () 
		{
			if (seperator == null || seperator.normal.background == null) 
			{
				seperator = new GUIStyle();
				seperator.normal.background = ColorToTex (1, new Color (0.6f, 0.6f, 0.6f));
				seperator.stretchWidth = true;
				seperator.margin = new RectOffset(0, 0, 7, 7);
			}
		}

		#endregion

		#region GUI Proportioning Utilities

		public static float labelWidth = 150;
		public static float fieldWidth = 50;
		public static float indent = 0;
		private static float textFieldHeight { get { return GUI.skin.textField.CalcHeight(new GUIContent("i"), 10); } }

		public static Rect PrefixLabel(Rect totalPos, GUIContent label, GUIStyle style)
		{
			if (label == GUIContent.none || label == null)
				return IndentedRect(totalPos);

			Rect labelPos = new Rect(totalPos.x + indent, totalPos.y, getLabelWidth() - indent, totalPos.height);
			GUI.Label(labelPos, label, style);

			return new Rect(totalPos.x + getLabelWidth(), totalPos.y, totalPos.width - getLabelWidth(), totalPos.height);
		}

		private static Rect IndentedRect(Rect source)
		{
			return new Rect(source.x + indent, source.y, source.width - indent, source.height);
		}

		private static float getLabelWidth()
		{
#if UNITY_EDITOR
			return UnityEditor.EditorGUIUtility.labelWidth;
#else
			if (labelWidth == 0)
				return 150;
			return labelWidth;
#endif
		}

		private static float getFieldWidth()
		{
#if UNITY_EDITOR
			return UnityEditor.EditorGUIUtility.fieldWidth;
#else
			if (fieldWidth == 0)
				return 50;
			return fieldWidth;
#endif
		}

		private static Rect GetFieldRect(bool hasLabel, params GUILayoutOption[] options)
		{
			return GUILayoutUtility.GetRect(getFieldWidth() + (hasLabel ? getLabelWidth() + 5 : 0), getFieldWidth() + getLabelWidth() + 5, textFieldHeight, textFieldHeight, options);
		}

		private static Rect GetSliderRect(bool hasLabel, params GUILayoutOption[] options)
		{
			return GUILayoutUtility.GetRect(getFieldWidth() + (hasLabel ? getLabelWidth() + 5 : 0), getFieldWidth() + getLabelWidth() + 5 + 100, textFieldHeight, textFieldHeight, options);
		}

		private static Rect GetSliderRect(Rect sliderRect)
		{
			return new Rect(sliderRect.x, sliderRect.y, sliderRect.width - getFieldWidth() - 5, sliderRect.height);
		}

		private static Rect GetSliderFieldRect(Rect sliderRect)
		{
			return new Rect(sliderRect.x + sliderRect.width - getFieldWidth(), sliderRect.y, getFieldWidth(), sliderRect.height);
		}

#endregion

		#region Math.Power Slider

		/// <summary>
		/// Slider to select from a set range of powers for a given base value. 
		/// Operates on the final value, rounds it to the next power and displays it.
		/// </summary>
		public static int MathPowerSlider(int baseValue, int value, int minPow, int maxPow, params GUILayoutOption[] options)
		{
			return MathPowerSlider(GUIContent.none, baseValue, value, minPow, maxPow, options);
		}
		/// <summary>
		/// Slider to select from a set range of powers for a given base value. 
		/// Operates on the final value, rounds it to the next power and displays it.
		/// </summary>
		public static int MathPowerSlider(GUIContent label, int baseValue, int value, int minPow, int maxPow, params GUILayoutOption[] options)
		{
			int power = (int)Math.Floor(Math.Log(value) / Math.Log(baseValue));
			power = MathPowerSliderRaw(label, baseValue, power, minPow, maxPow, options);
			return (int)Math.Pow(baseValue, power);
		}
		/// <summary>
		/// Slider to select from a set range of powers for a given base value. 
		/// Operates on the raw power but displays the final calculated value.
		/// </summary>
		public static int MathPowerSliderRaw(int baseValue, int power, int minPow, int maxPow, params GUILayoutOption[] options)
		{
			return MathPowerSliderRaw(GUIContent.none, baseValue, power, minPow, maxPow, options);
		}
		/// <summary>
		/// Slider to select from a set range of powers for a given base value. 
		/// Operates on the raw power but displays the final calculated value.
		/// </summary>
		public static int MathPowerSliderRaw(GUIContent label, int baseValue, int power, int minPow, int maxPow, params GUILayoutOption[] options)
		{
			Rect totalPos = GetSliderRect(label != GUIContent.none, options);
			Rect sliderFieldPos = PrefixLabel(totalPos, label, GUI.skin.label);

			power = Mathf.RoundToInt(GUI.HorizontalSlider(GetSliderRect(sliderFieldPos), power, minPow, maxPow));
			GUI.Label(GetSliderFieldRect(sliderFieldPos), Mathf.Pow(baseValue, power).ToString());
			return power;
		}

		#endregion



		/// <summary>
		/// Create a 1x1 tex with color col
		/// </summary>
		public static Texture2D ColorToTex (int pxSize, Color col) 
		{
			Color[] texCol = new Color[pxSize*pxSize];
			for (int c = 0; c < texCol.Length; c++)
				texCol[c] = col;
			Texture2D tex = new Texture2D (pxSize, pxSize);
			tex.SetPixels (texCol);
			tex.Apply ();
			return tex;
		}
	}

}