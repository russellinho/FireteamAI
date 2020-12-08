using System;

using UnityEngine;
using UnityEditor;

using TerrainComposer2.NodePainter.Utilities;

namespace TerrainComposer2.NodePainter
{
	public class PaintingEditor
	{
		public Painting painter;
		public int controlID;

		public bool isMouseInWindow { get { return new Rect(0, 0, Screen.width, Screen.height).Contains(Event.current.mousePosition); } }
		public string GUIWindowID = "GUI";

		#region GUI

		private static string[] formatOptions = new string[] { "Color", "Value", "Multi" };
		private static string[] channelOptions = new string[] { "RGBA", "RGB", "R", "G", "B", "A", "Max" };
		private static string[] multiDisplayOptions = new string[] { "Current", "Mix" };

		private static Color selectCol = new Color(0.2f, 0.5f, 0.9f, 1f);

		private static Texture2D warningIcon;
		private static Texture2D eyeOpenIcon;
		private static Texture2D eyeClosedIcon;

		private static GUIStyle brushSelectorButton;
		private static GUIStyle channelSelectorButton;
		private static GUIStyle colorButton;
		internal static GUIStyle headerFoldout;

		private static Texture2D imgBGTex;
		private static Texture2D imgBorderTex;

		// Settings
		private const float minBrushIntensity = 0.001f, minBrushSize = 0.0001f;

		#endregion

		#region Temp

		// Expand
		private bool expandIO = true, expandCanvasPreview = true, expandColor = true, expandBrush = true, expandMods = false, expandResize = false, expandDebug = false;

		// Create Canvas
		private string canvasName = "New Canvas";
		private bool unlockYRes = false;
		private int resX = 1024, resY = 1024;
		private Painting.Format format = Painting.Format.Value;
		private int channelCount = 1;

		// Visualization
		private Material brushGUIMaterial;

		// Canvas Preview
		private bool editName;
		private Painting.Channels workChannels = Painting.Channels.RGB; // 0:RGBA 1:RGB 2:R 3:G 4:B 5:A
		private Painting.CanvasVizState visualizationState = Painting.CanvasVizState.All;

		// Fill
		private Color fillColor = new Color(0, 0, 0, 0);
		private float fillValue = 0;

		// Debug
		private int testIterations = 1000;

		// Shortcuts
		private bool pressedSpace = false;

		// Misc
		private int lastBrushPreset = -1;
		private Tool lastTool;

		// Channel Texture Picker
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
		private int pickerControlID;
		private int pickerChannelIndex;
#endif

		#endregion


		#region General Methods

		public PaintingEditor(Painting Painter)
		{
			painter = Painter;

			GlobalPainting.ReloadBrushTextures();
			GlobalPainting.UpdateTerrainBrush();
			painter.UpdateBrushType();

			lastTool = Tools.current;
			Tools.current = Tool.None;
		}

		public void CheckGUIStyles()
		{
			if (imgBGTex == null)
				imgBGTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Settings.paintingResourcesFolder + "/GUI/GUI_SelectBG.png");
			if (imgBorderTex == null)
				imgBorderTex = AssetDatabase.LoadAssetAtPath<Texture2D>(Settings.paintingResourcesFolder + "/GUI/GUI_SelectBorder.png");

			if (brushSelectorButton == null)
				brushSelectorButton = new GUIStyle();
			brushSelectorButton.onNormal.background = imgBGTex;

			if (channelSelectorButton == null)
			{
				channelSelectorButton = new GUIStyle();
				channelSelectorButton.border = new RectOffset(2, 2, 2, 2);
				channelSelectorButton.margin = new RectOffset(4, 4, 4, 4);
			}
			channelSelectorButton.normal.background = imgBorderTex;

			if (warningIcon == null)
				warningIcon = (Texture2D)EditorGUIUtility.Load("icons/console.warnicon.sml.png");
			if (eyeOpenIcon == null)
				eyeOpenIcon = (Texture2D)EditorGUIUtility.Load("icons/animationvisibilitytoggleon.png");
			if (eyeClosedIcon == null)
				eyeClosedIcon = (Texture2D)EditorGUIUtility.Load("icons/animationvisibilitytoggleoff.png");

			if (colorButton == null)
			{
				colorButton = new GUIStyle(GUI.skin.box);
				colorButton.fixedHeight = colorButton.fixedWidth = 15;
				colorButton.margin = new RectOffset(0, 0, 2, 0);
				colorButton.contentOffset = new Vector2(0, -1);
				colorButton.alignment = TextAnchor.MiddleCenter;
			}

			if (headerFoldout == null)
			{
				headerFoldout = new GUIStyle(EditorStyles.foldout);
				headerFoldout.font = EditorStyles.boldFont;
				headerFoldout.margin.top += 2;
			}
		}

		public void Close()
		{
			Tools.current = lastTool == Tool.None ? Tool.Move : lastTool;
			painter.Hide();
		}

		#endregion

		#region GUI

		public bool DoPainterUI()
		{
			bool repaint = false;

			CheckGUIStyles();

			#region Canvas IO

			expandIO = EditorGUILayout.Foldout(expandIO, "Canvas IO", headerFoldout);
			if (expandIO)
			{
				EditorGUI.BeginDisabledGroup(!painter.canvas_Exists);
				if (GUILayout.Button("Delete Canvas"))
					painter.DeleteCanvas();
				EditorGUI.EndDisabledGroup();

				// NATIVE COMBINED BYTES

				GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Native", "Native combined byte format that perfectly stores the canvas in one file with extension .bytes"), GUILayout.Width(60));
				if (GUILayout.Button("Load"))
				{
					string path = EditorUtility.OpenFilePanel("Load canvas data", Application.dataPath, "bytes");
					if (!string.IsNullOrEmpty(path))
						painter.ImportCanvas(path);
				}
				EditorGUI.BeginDisabledGroup(!painter.canvas_Exists);
				if (GUILayout.Button("Save"))
				{
					string path = EditorUtility.SaveFilePanel("Save Canvas", Application.dataPath, painter.canvas_Name, "bytes");
					if (!string.IsNullOrEmpty(path))
					{
						painter.ExportCanvas(path);
						AssetDatabase.Refresh();
					}
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();

				// TEXTURE

				GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Texture", "LOSSY 8-Bit PNG Texture format for exporting to all image editors that do not support RAW. " +
					"Stores most canvas meta information except canvas channel count. " +
					"For multi-canvas, save files are split in name(n) files for each 4 channels."), GUILayout.Width(60));
				if (GUILayout.Button("Import"))
				{
					string path = EditorUtility.OpenFilePanel("Import texture", Application.dataPath, "png");
					if (!painter.ImportCanvas(path))
						Debug.LogError("Failed importing texture into canvas!");
				}
				EditorGUI.BeginDisabledGroup(!painter.canvas_Exists);
				if (GUILayout.Button("Export"))
				{
					string path = EditorUtility.SaveFilePanel("Export texture", Application.dataPath, painter.canvas_Name, "png");
					if (!string.IsNullOrEmpty(path))
					{
						painter.ExportCanvas(path);
						AssetDatabase.Refresh();
					}
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();

				// RAW FILE

				GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Raw", "16-Bit RAW File export format for directly exporting to terrains and few image editors. " +
					"Does not store ANY canvas meta information, needs to be re-entered upon import. " +
					"For multi-canvas, save files are split in name(n) files for each 4 channels."), GUILayout.Width(60));
				if (GUILayout.Button("Import"))
				{
					string path = EditorUtility.OpenFilePanel("Import raw file", Application.dataPath, "raw");
					if (!string.IsNullOrEmpty(path))
						ImportExportDialogue.ImportRawDialogue(painter, path);
				}
				EditorGUI.BeginDisabledGroup(!painter.canvas_Exists);
				if (GUILayout.Button("Export"))
				{
					string path = EditorUtility.SaveFilePanel("Export raw file", Application.dataPath, painter.canvas_Name, "raw");
					if (!string.IsNullOrEmpty(path))
					{
						painter.ExportCanvas(path);
						AssetDatabase.Refresh();
					}
				}
				EditorGUI.EndDisabledGroup();
				GUILayout.EndHorizontal();

				// CACHE

				GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Cache", "Cache file found in the cache folder for quickly creating backup-points. " +
					"Cannot restore overridden cache file!"), GUILayout.Width(60));
				if (GUILayout.Button("Load", GUI.skin.button))
					painter.LoadLastSession(true);
				if (GUILayout.Button("Save", GUI.skin.button))
					painter.SaveCurrentSession();
				GUILayout.EndHorizontal();
			}

			#endregion

			if (!painter.canvas_Exists)
			{
				SubSectionSeperator();

				#region Canvas Creation

				GUILayout.BeginVertical(GUI.skin.box);

				GUILayout.BeginHorizontal();
				GUILayout.Label(new GUIContent("Load Cache", "Specify an already existing cache file to sync painters between scenes."));
				painter.cache_Asset = (TextAsset)EditorGUILayout.ObjectField(painter.cache_Asset, typeof(TextAsset), false);
				if (GUILayout.Button("Load", GUI.skin.button))
					painter.LoadLastSession(true);
				GUILayout.EndHorizontal();

				GUILayout.Label("OR create a new canvas:", EditorStyles.boldLabel);

				canvasName = EditorGUILayout.TextField("Name", canvasName);

				GUILayout.BeginHorizontal();
				GUILayout.Label("Resolution");
				GUILayout.FlexibleSpace();
				GUILayout.Label("X");
				resX = AdditionalGUIUtility.MathPowerSlider(2, resX, 4, 12);
				GUILayout.EndHorizontal();

				GUILayout.BeginHorizontal();

				unlockYRes = GUILayout.Toggle(unlockYRes, GUIContent.none);
				GUILayout.Space(45);
				GUILayout.FlexibleSpace();
#if UNITY_EDITOR
				EditorGUI.BeginDisabledGroup(!unlockYRes);
#endif
				GUILayout.Label("Y");
				if (!unlockYRes)
					resY = resX;
				resY = AdditionalGUIUtility.MathPowerSlider(2, resY, 4, 12);
#if UNITY_EDITOR
				EditorGUI.EndDisabledGroup();
#endif
				GUILayout.EndHorizontal();

				format = (Painting.Format)GUILayout.Toolbar((int)format, formatOptions);

				EditorGUI.BeginDisabledGroup(format != Painting.Format.Multi);
				channelCount = Mathf.Clamp(EditorGUILayout.IntField("Channel Count", channelCount), 1, 32);
				EditorGUI.EndDisabledGroup();

				if (GUILayout.Button("Create!"))
					painter.NewCanvas(resX, resY, format, canvasName, format == Painting.Format.Multi ? channelCount : 1);

				GUILayout.EndVertical();

				#endregion

				SubSectionSeperator();

				// Usually called later, but for the lack of a canvas pulled up
				ShowUndoButtons();

				return repaint;
			}

			SectionSeperator();

			#region Canvas Preview

			// HEADER

			GUILayout.BeginHorizontal();
			if (editName)
			{ // Editing display
				painter.canvas_Name = GUILayout.TextField(painter.canvas_Name, GUILayout.ExpandWidth(true));
#if UNITY_EDITOR
				GUILayout.Space(5);
				GUILayout.Label(new GUIContent("Cache", "Cache-File for this painter. Used to sync painters in different scenes by using the same cache file."), GUILayout.ExpandWidth(false));
				TextAsset cacheAsset = (TextAsset)EditorGUILayout.ObjectField(painter.cache_Asset, typeof(TextAsset), false, GUILayout.ExpandWidth(false), GUILayout.MaxWidth(100));
				if (cacheAsset != painter.cache_Asset)
				{ // New cache file to use has been assigned, load it
					painter.cache_Asset = cacheAsset;
					painter.LoadLastSession();
				}
#endif
			}
			else
			{ // Normal display
				expandCanvasPreview = EditorGUILayout.Foldout(expandCanvasPreview, painter.canvas_Name, headerFoldout);
			}
			// Constant Edit and Viz buttons in canvas header
			if (GUILayout.Button("E", GUILayout.Width(20)))
				editName = !editName;
			if (GUILayout.Button(painter.visualizeCanvas ? eyeOpenIcon : eyeClosedIcon, GUILayout.Width(20)))
				painter.visualizeCanvas = !painter.visualizeCanvas;
			GUILayout.EndHorizontal();


			// CANVAS PREVIEW

			if (!expandCanvasPreview)
				painter.canvasVizState = Painting.CanvasVizState.None;
			else
			{ // Draw canvas preview

				// Calculate rect
				float textureSize = Mathf.Max(Mathf.Min(EditorGUIUtility.currentViewWidth, Settings.maxCanvasPreviewSize) - 46, 256);
				float border = Settings.fixedCanvasPaintBorder + textureSize * painter.paint_Brush.size * Settings.relativeCanvasPaintBorder;
				Rect textureRect = GUILayoutUtility.GetRect(textureSize, textureSize * painter.canvas_SizeY / painter.canvas_SizeX, GUILayout.ExpandWidth(false));
				textureRect = new Rect(textureRect.x + border, textureRect.y + border, textureRect.width - 2 * border, textureRect.height - 2 * border);

				// Format-based rendering
				if (painter.canvas_Format == Painting.Format.Value)
				{ // Format: Value
					DrawCanvasPreview(textureRect, painter.tex_VizCanvas, Painting.Channels.RGB);
				}
				else if (painter.canvas_Format == Painting.Format.Multi)
				{ // Format: Multi Channel
					if (painter.canvasVizState == Painting.CanvasVizState.All)
						DrawCanvasPreview(textureRect, painter.tex_VizCanvas, Painting.Channels.RGB);
					else
						DrawCanvasPreview(textureRect, painter.tex_VizCanvas, (Painting.Channels)(painter.curChannelIndex % 4 + 2));
				}
				else
				{ // Format: Color
					DrawCanvasPreview(textureRect, painter.tex_VizCanvas, workChannels);
				}

				// Format-based Viz toolbar
				if (painter.canvas_Format == Painting.Format.Color)
				{ // Color: Channel Toolbar
					GUILayout.BeginHorizontal();
					workChannels = (Painting.Channels)GUILayout.Toolbar((int)workChannels, channelOptions, EditorStyles.toolbarButton, GUILayout.Width(textureSize));
					painter.canvasVizState = Painting.CanvasVizState.All;
					GUILayout.Space(12);
					GUILayout.EndHorizontal();
				}
				else if (painter.canvas_Format == Painting.Format.Multi)
				{ // Multi: Visualization Toolbar
					GUILayout.BeginHorizontal();
					painter.canvasVizState = visualizationState = (Painting.CanvasVizState)(GUILayout.Toolbar((int)visualizationState - 1, multiDisplayOptions, EditorStyles.toolbarButton, GUILayout.Width(textureSize)) + 1);
					GUILayout.Space(12);
					GUILayout.EndHorizontal();
				}
				else
					painter.canvasVizState = Painting.CanvasVizState.All;

				// GUI Painting

				painter.state_BlockPainting = HandleShortcuts();

				if (Event.current.type != EventType.Layout)
				{ // Try GUI Painting
					Vector2 brushPos;
					if (painter.isOnCanvas = GlobalPainting.CalcBrushGUIPos(textureRect, textureSize * painter.paint_Brush.size, out brushPos))
					{ // Mouse is on UI canvas
						painter.state_BrushPos = brushPos;

						// Paint update
						int controlID = GUIUtility.GetControlID(FocusType.Passive);
						if (painter.PaintUpdate(GUIWindowID, controlID))
							repaint = true;

						if (Event.current.type == EventType.Repaint)
						{ // Draw brush in GUI

							GUI.BeginClip(textureRect);
							
							// Calculate brush rect
							Rect brushRect = new Rect(Vector2.zero, textureRect.size * painter.paint_Brush.size);
							brushRect.size = new Vector2 (brushRect.size.x, brushRect.size.y / painter.canvas_Aspect);
							brushRect.center = Event.current.mousePosition;
							
							// Draw brush
							setupBrushVizMat();
							Graphics.DrawTexture(brushRect, Texture2D.whiteTexture, brushGUIMaterial);

							GUI.EndClip();
						}
					}
					else if (!isMouseInWindow)
					{ // Stop painting when exiting inspector window with mouse
						painter.StopPainting(GUIWindowID);
					}
				}
			}

			#endregion

			SubSectionSeperator();

			ShowUndoButtons();

			#region Painting/Tool Button

			painter.state_BlockPainting = Tools.current != Tool.None;
			if (GUILayout.Button(painter.state_BlockPainting ? "Start Painting" : "Stop Painting"))
			{
				if (painter.state_BlockPainting)
				{
					lastTool = Tools.current;
					Tools.current = Tool.None;
					painter.state_BlockPainting = false;
				}
				else
				{
					Tools.current = lastTool == Tool.None ? Tool.Move : lastTool;
					painter.state_BlockPainting = true;
				}
			}

			#endregion

			SectionSeperator();

			if (painter.canvas_Format == Painting.Format.Color)
			{
				#region Colors

				// HEADER

				GUILayout.BeginHorizontal();
				// Foldout
				expandColor = EditorGUILayout.Foldout(expandColor, "Color", headerFoldout);
				GUILayout.Space(5);
				// Preset Selection
				for (int colI = 0; colI < GlobalPainting.colorPresets.Count; colI++)
				{
					if (PresetButton(" ", GlobalPainting.colorPresets[colI], GlobalPainting.DeleteColorPreset, colI))
						painter.paint_Color = GlobalPainting.colorPresets[colI];
				}
				GUILayout.Space(5);
				// Preset Creation
				if (PresetButton("+", painter.paint_Color, null, 0))
				{
					GlobalPainting.colorPresets.Add(painter.paint_Color);
					Settings.SaveColorPresets();
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();


				if (expandColor)
				{ // Modify color parameters
					painter.paint_Color = EditorGUILayout.ColorField("Color", painter.paint_Color);
					painter.paint_Intensity = EditorGUILayout.Slider("Intensity", painter.paint_Intensity, 0f, 10f);
				}

				#endregion

				SectionSeperator();
			}
			else
			{
				painter.paint_Intensity = 1;
				painter.paint_Color = Color.white;
			}

			if (painter.canvas_Format == Painting.Format.Multi)
			{
				#region Multi Channel

				// Canvas channel selection
				painter.curChannelIndex = Mathf.Clamp(painter.curChannelIndex, 0, painter.canvasChannelCount - 1);
				Rect channelsRect = DrawResponsiveGrid((int)EditorGUIUtility.currentViewWidth - 36 - 20, Settings.imgPreviewTexSize, painter.canvasChannelCount, (int chInd, Rect rect) => {

					CanvasChannel channel = painter.canvasChannels[chInd];
					Rect colorRect = rect;

					if (channel.displayTexture != null)
					{ // Display Texture
						RTTextureViz.DrawTexture(channel.displayTexture, rect, 1, 2, 3, 5);
						colorRect.height = Settings.imgPreviewTexSize / 8;
						colorRect.y += rect.height - colorRect.height;
					}

					// Channel Color
					if (painter.channelVizColors.Length > chInd)
						RTTextureViz.DrawTexture(Texture2D.whiteTexture, colorRect, painter.channelVizColors[chInd]);

					// Outline Selection
					if (painter.curChannelIndex == chInd)
					{ // Draw Selected Border
						RectOffset border = channelSelectorButton.border;
						Rect borderRect = Rect.MinMaxRect(rect.xMin - border.left, rect.yMin - border.top, rect.xMax + border.right, rect.yMax + border.bottom);
						RTTextureViz.DrawTexture(imgBorderTex, borderRect, selectCol);
					}
					else if (Event.current.type == EventType.Repaint)
					{ // Draw Default Border
						GUI.color = Color.gray;
						channelSelectorButton.Draw(rect, GUIContent.none, 0);
						GUI.color = Color.white;
					}

					// Context click
					if (Event.current.type == EventType.MouseUp && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
					{
						GenericMenu context = new GenericMenu();

						context.AddItem(new GUIContent("Fill Channel"), false, () => { painter.FillChannel(chInd, false); });
						context.AddItem(new GUIContent("Clear Channel"), false, () => { painter.FillChannel (chInd, true); });
						context.AddItem(new GUIContent("Delete Channel"), false, () => { painter.DeleteCanvasChannel(chInd); });
						context.AddItem(new GUIContent("Insert new Channel"), false, () => { painter.AddNewCanvasChannel(chInd); });
						if (chInd < painter.canvasChannelCount - 1) context.AddItem(new GUIContent("Move Channel Up"), false, () => { painter.MoveChannel(chInd, chInd + 1); });
						else								context.AddDisabledItem(new GUIContent("Move Channel Up"));
						if (chInd > 0)	context.AddItem(new GUIContent("Move Channel Down"), false, () => { painter.MoveChannel(chInd, chInd - 1); });
						else	context.AddDisabledItem(new GUIContent("Move Channel Down"));

#if UNITY_5_2 || UNITY_5_3_OR_NEWER
						context.AddItem (new GUIContent ("Select Display Texture"), false, () => {
							pickerControlID = EditorGUIUtility.GetControlID (FocusType.Passive);
							pickerChannelIndex = chInd;
							EditorGUIUtility.ShowObjectPicker<Texture> (channel.displayTexture, true, null, pickerControlID);
						});
#endif

						context.ShowAsContext();
						Event.current.Use();
					}

					// Toggle behaviour
					if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
						painter.curChannelIndex = chInd;

				}, channelSelectorButton);
#if UNITY_5_2 || UNITY_5_3_OR_NEWER
				if (Event.current.type == EventType.ExecuteCommand && Event.current.commandName == "ObjectSelectorUpdated" && EditorGUIUtility.GetObjectPickerControlID () == pickerControlID)
				{
					painter.canvasChannels[pickerChannelIndex].displayTexture = (Texture)EditorGUIUtility.GetObjectPickerObject ();
				}
#endif

				Rect addButtonRect = new Rect(EditorGUIUtility.currentViewWidth - 20 - 20, channelsRect.y, 20, Settings.imgPreviewTexSize / 2);
				Rect delButtonRect = new Rect(EditorGUIUtility.currentViewWidth - 20 - 20, channelsRect.y + Settings.imgPreviewTexSize / 2, 20, Settings.imgPreviewTexSize / 2);
				if (GUI.Button(addButtonRect, "+"))
				{
					painter.AddNewCanvasChannel(painter.canvasChannelCount);
				}
				if (GUI.Button(delButtonRect, "-"))
				{
					painter.DeleteCanvasChannel(painter.canvasChannelCount - 1);
				}

#if !UNITY_5_2 && !UNITY_5_3_OR_NEWER
				GUILayout.BeginHorizontal();

				GUILayout.Label("Display Texture");
				CanvasChannel curChannel = painter.canvasChannels[painter.curChannelIndex];
				curChannel.displayTexture = EditorGUILayout.ObjectField(curChannel.displayTexture, typeof(Texture), false) as Texture;

				GUILayout.EndHorizontal();
#endif

				#endregion

				SectionSeperator();
			}

			#region Brush

			// HEADER

			GUILayout.BeginHorizontal();
			// Foldout
			expandBrush = EditorGUILayout.Foldout(expandBrush, "Brush", headerFoldout);
			GUILayout.Space(5);
			// Preset Selection
			for (int brushI = 0; brushI < GlobalPainting.brushPresets.Count; brushI++)
			{
				if (PresetButton((brushI + 1).ToString(), lastBrushPreset == brushI ? selectCol : Color.white, GlobalPainting.DeleteBrushPreset, brushI))
				{
					painter.temp_prevPaintMode = painter.paint_Brush.mode;
					painter.paint_Brush = GlobalPainting.brushPresets[brushI];
					painter.UpdateBrushType();
					lastBrushPreset = brushI;
				}
			}
			GUILayout.Space(5);
			// Preset Creation
			if (PresetButton("+", Color.white, null, 0))
			{
				GlobalPainting.brushPresets.Add(painter.paint_Brush);
				Settings.SaveBrushPresets();
			}
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();


			if (expandBrush)
			{
				EditorGUILayout.Space();

				// Brush Type Selection Grid
				DrawResponsiveGrid((int)EditorGUIUtility.currentViewWidth - 36, Settings.imgPreviewTexSize, GlobalPainting.brushTextures.Length, (int texInd, Rect rect) => {
					if (GUI.Toggle(rect, painter.paint_Brush.type == texInd, GUIContent.none, brushSelectorButton))
					{
						if (painter.paint_Brush.type != texInd)
						{
							painter.paint_Brush.type = texInd;
							painter.UpdateBrushType();
							lastBrushPreset = -1;
						}
					}
					RTTextureViz.DrawTexture(GlobalPainting.brushTextures[texInd], rect, 0, 0, 0, 4);
				});

				// Brush rotation
				painter.paint_BrushRotation = EditorGUILayout.Slider(new GUIContent("Rotation", "Shortcut: Space + Scroll"), painter.paint_BrushRotation, -1f, 1f);

				// BRUSH SETTINGS

				EditorGUI.BeginChangeCheck();

				// Mode
				Painting.PaintMode newMode = (Painting.PaintMode)EditorGUILayout.EnumPopup(new GUIContent("Mode", "Switch to last: Tab; Invert: Shift"), painter.paint_Brush.mode);
				if (newMode != painter.paint_Brush.mode)
				{
					painter.temp_prevPaintMode = painter.paint_Brush.mode;
					painter.paint_Brush.mode = newMode;
				}
				// Size
				painter.paint_Brush.size = EditorGUILayout.Slider(new GUIContent("Size", "Shortcut: Control + Scroll"), painter.paint_Brush.size, minBrushSize, 1f);
				// Intensity
				painter.paint_Brush.intensity = EditorGUILayout.Slider(new GUIContent("Intensity", "Shortcut: Shift + Scroll"), painter.paint_Brush.intensity, minBrushIntensity, 1);
				if (painter.state_BrushFunc > 0)
				{ // Function parameters
					painter.paint_Brush.falloff = EditorGUILayout.Slider("Falloff", painter.paint_Brush.falloff, 0f, 1f);
					painter.paint_Brush.hardness = EditorGUILayout.Slider("Hardness", painter.paint_Brush.hardness, 1f, 4f);
				}

				// After changing brush settings, remove highlight on last loaded brush preset to mark change
				if (EditorGUI.EndChangeCheck())
					lastBrushPreset = -1;


				// EXTRA BRUSH VALUES

				// Smoothen Bias
				if (painter.paint_Brush.mode == Painting.PaintMode.Smoothen || painter.paint_Brush.mode == Painting.PaintMode.Contrast)
					painter.paint_SmoothenBias = EditorGUILayout.Slider(new GUIContent("Bias", "Shortcut: Control + Shift + Scroll"), painter.paint_SmoothenBias, 1, 4);
				// Target Value
				if (painter.paint_Brush.mode == Painting.PaintMode.Replace || painter.paint_Brush.mode == Painting.PaintMode.Lerp)
					painter.paint_TargetValue = EditorGUILayout.Slider(new GUIContent("Target", "Shortcut: Control + Shift + Scroll; Pick Value: Control + LeftClick"), painter.paint_TargetValue, 0f, 1f);

				SubSectionSeperator();


				// EXTRA BRUSH SETTINGS

				// Clamp
				//painter.clampResultStroke = EditorGUILayout.Toggle ("Clamp Stroke", painter.clampResultStroke);
				painter.paint_Clamp = EditorGUILayout.Toggle("Clamp 0-1", painter.paint_Clamp);

				if (painter.canvas_Format == Painting.Format.Multi)
				{ // Normalize Channels with help texts
					painter.paint_normChannels = EditorGUILayout.Toggle("Normalize Channels", painter.paint_normChannels);
					if (painter.paint_normChannels && !painter.sepPass_Support)
						EditorGUILayout.HelpBox("Currently selected mode does not work well with channel normalization - please select a different like Lerp or Add/Substract!", MessageType.Warning);
					if (!painter.paint_normChannels)
						EditorGUILayout.HelpBox("Normalization prevents multiple channels to overlap and exceed the value 1. For blending different setups together, this is usually not desired!", MessageType.Warning);
				}
			}

			#endregion

			SectionSeperator();

			if (painter.canvas_Format != Painting.Format.Multi)
			{
				#region Modifications

				// HEADER

				GUILayout.BeginHorizontal();
				// Foldout
				expandMods = EditorGUILayout.Foldout(expandMods, "Modifications", headerFoldout);
				GUILayout.Space(40);
				// Warning when applying unapplied mods, slows painting down
				if (painter.applyingOngoingMods && warningIcon != null)
					GUILayout.Label(new GUIContent(warningIcon, "Modifications are being generated"), GUILayout.ExpandWidth(false));
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();

				if (expandMods)
				{
					EditorGUI.BeginChangeCheck();

					// Basic Modifications
					painter.mods.contrast = EditorGUILayout.Slider("Contrast", painter.mods.contrast, 0, 2);
					painter.mods.brightness = EditorGUILayout.Slider("Brightness", painter.mods.brightness, -1, 1);

					if (painter.canvas_Format == Painting.Format.Color)
					{ // Complex per-channel modifications
						painter.mods.tintColor = EditorGUILayout.ColorField("Tint", painter.mods.tintColor);
						painter.mods.advancedChannelMods = EditorGUILayout.ToggleLeft("Enable Advanced Channel Mods", painter.mods.advancedChannelMods);
						if (painter.mods.advancedChannelMods)
							ShowChannelMods(ref painter.mods.chR, ref painter.mods.chG, ref painter.mods.chB, ref painter.mods.chA);
					}
					else
						painter.mods.advancedChannelMods = false;

					// After changing settings, update post-processing modifications
					if (EditorGUI.EndChangeCheck())
						painter.UpdateModifications();

					GUILayout.BeginHorizontal();
					if (GUILayout.Button("Reset Modifications"))
					{ // Reset post-process modifications
						painter.mods = new Painting.Modifications(true);
						painter.UpdateModifications();
					}
					if (GUILayout.Button("Apply Modifications"))
					{ // Apply modifications, bake them into the canvas
						painter.ApplyModifications();
					}

					GUILayout.EndHorizontal();

					SubSectionSeperator();

					// Fill canvas
					if (painter.canvas_Format == Painting.Format.Color)
					{ // Color Format
						GUILayout.BeginHorizontal();
						fillColor = EditorGUILayout.ColorField("Color", fillColor);
						if (GUILayout.Button("Fill"))
							painter.Fill(fillColor);
						GUILayout.EndHorizontal();
					}
					else if (painter.canvas_Format == Painting.Format.Value)
					{ // Value Format
						GUILayout.BeginHorizontal();
						fillValue = EditorGUILayout.Slider("Height", fillValue, 0f, 1f);
						if (GUILayout.Button("Set"))
							painter.Fill(new Color(fillValue, fillValue, fillValue, fillValue));
						GUILayout.EndHorizontal();
					}
				}

				#endregion

				SectionSeperator();
			}

			#region Resize

			expandResize = EditorGUILayout.Foldout(expandResize, "Resize (" + painter.canvas_SizeX + ", " + painter.canvas_SizeY + ")", headerFoldout);
			if (expandResize)
			{
				// Size field
				GUILayout.BeginHorizontal();
				GUILayout.Label("New X/Y");
				resX = EditorGUILayout.IntField(resX);
				resY = EditorGUILayout.IntField(resY);
				GUILayout.EndHorizontal();

				// Apply Buttons
				GUILayout.BeginHorizontal();
				if (GUILayout.Button("Resize"))
					painter.Resize(resX, resY);
				if (GUILayout.Button("Expand"))
					painter.Expand(resX, resY);
				GUILayout.EndHorizontal();
			}

			#endregion


			if (Settings.enableDebug)
			{
				SectionSeperator();

				#region Debug

				expandDebug = EditorGUILayout.Foldout(expandDebug, "Debug", headerFoldout);
				if (expandDebug)
				{
					// Separate paint pass
					painter.sepPass_Force = GUILayout.Toggle(painter.sepPass_Force, "Force Seperate Paint Pass");
					GUILayout.Label("Seperate paint pass is " + (painter.sepPass_Enable ? "enabled" : "disabled") + "!");
					if (painter.sepPass_Need && !painter.sepPass_Support)
						GUILayout.Label("SPP is not supported by the current mode!");

					SubSectionSeperator();

					// Iterative paint test
					GUILayout.BeginHorizontal();
					testIterations = System.Math.Min(10000, System.Math.Max(1, EditorGUILayout.IntField("Iterations", testIterations)));
					if (GUILayout.Button("Test"))
					{
						System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
						watch.Start();

						for (int i = 0; i < testIterations; i++)
							painter.Paint(new Vector2(0.5f, 0.5f));

						watch.Stop();
						Debug.Log("Test time with " + testIterations + " iterations: " + watch.ElapsedMilliseconds);
					}
					GUILayout.EndHorizontal();

					SubSectionSeperator();

					// Raw paint debug
					GUILayout.Label("Current Raw Paint:", EditorStyles.boldLabel);
					float textureSize = Mathf.Max(Mathf.Min(Mathf.Min(painter.canvas_SizeX, EditorGUIUtility.currentViewWidth), Settings.maxCanvasPreviewSize) - 46, 256);
					Rect textureRect = GUILayoutUtility.GetRect(textureSize, textureSize * painter.canvas_SizeY / painter.canvas_SizeX, GUILayout.ExpandWidth(false));
					if (painter.tex_CurRawPaint != null)
						DrawCanvasPreview(textureRect, painter.tex_CurRawPaint, workChannels);
				}

				#endregion
			}
			else
				painter.sepPass_Force = false;

			return repaint;
		}

		#endregion

		#region Shortcuts

		public bool HandleShortcuts()
		{
			bool blockPainting = false;

			// SHIFT MODIFIER

			// Invert Brush mode while holding shift
			painter.state_InvertBrush = Event.current.modifiers == EventModifiers.Shift;

			// Scale brush intensity by holding shift while scrolling
			if (Event.current.modifiers == EventModifiers.Shift && Event.current.type == EventType.ScrollWheel)
			{
				Event.current.Use();
				painter.paint_Brush.intensity = Mathf.Clamp(painter.paint_Brush.intensity * (1 - (0.05f * Event.current.delta.y)), minBrushIntensity, 1f);
				//undo = "Brush Intensity";
			}


			// CONTROL MODIFIER

			// Resize brush size by holding control while scrolling
			if (Event.current.modifiers == EventModifiers.Control && Event.current.type == EventType.ScrollWheel)
			{
				Event.current.Use();
				painter.paint_Brush.size = Mathf.Clamp(painter.paint_Brush.size * (1 - (0.05f * Event.current.delta.y)), minBrushSize, 1f);
				//undo = "Brush Size";
			}

			// Set current color to white and intensity to the height under the mouse when leftclicking
			if (Event.current.modifiers == EventModifiers.Control && (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseDrag) && Event.current.button == 0)
			{
				blockPainting = true;
				Event.current.Use();
				if (new Rect(0, 0, 1, 1).Contains(painter.state_BrushPos) && (painter.paint_Brush.mode == Painting.PaintMode.Lerp || painter.paint_Brush.mode == Painting.PaintMode.Replace))
				{
					Color sample = painter.Sample(painter.state_BrushPos);
					if (painter.canvas_Format == Painting.Format.Color)
					{
						painter.paint_TargetValue = 1;
						painter.paint_Intensity = 1;
						painter.paint_Color = sample;
					}
					else
					{
						painter.paint_TargetValue = Mathf.Max(sample.r, sample.g, sample.b, sample.a);
						painter.paint_Intensity = 1;
						painter.paint_Color = Color.white;
					}
					//undo = "Target Value";
				}
			}


			// Detect space key press for usage in non-key events (scroll)
			if (Event.current.isKey)
			{
				if (Event.current.type != EventType.KeyUp && Event.current.keyCode == KeyCode.Space)
					pressedSpace = true;
				else if (Event.current.type != EventType.KeyDown)
					pressedSpace = false;
			}

			// Adjust brush rotation while holding space and scrolling
			if (Event.current.type == EventType.ScrollWheel && pressedSpace)
			{
				Event.current.Use();
				painter.paint_BrushRotation = painter.paint_BrushRotation - 0.05f * (int)(Event.current.delta.y / 3);
				if (painter.paint_BrushRotation > 1f || painter.paint_BrushRotation < -1f)
					painter.paint_BrushRotation = Mathf.Clamp(-painter.paint_BrushRotation, -1, 1);
				//undo = "Brush Rotation";
			}

			// Adjust brush target/smoothen bias by holding control+shift while scrolling
			if (Event.current.modifiers == (EventModifiers.Control | EventModifiers.Shift) && Event.current.type == EventType.ScrollWheel)
			{
				Event.current.Use();
				if (painter.paint_Brush.mode == Painting.PaintMode.Lerp || painter.paint_Brush.mode == Painting.PaintMode.Replace)
					painter.paint_TargetValue = Mathf.Clamp(painter.paint_TargetValue - 0.05f * (int)(Event.current.delta.y / 3), 0f, 1f);
				else if (painter.paint_Brush.mode == Painting.PaintMode.Smoothen || painter.paint_Brush.mode == Painting.PaintMode.Contrast)
					painter.paint_SmoothenBias = Mathf.Clamp(painter.paint_SmoothenBias - 0.2f * (int)(Event.current.delta.y / 3), 1f, 4f);
				//undo = "Brush Value";
			}


			// TAB MODIFIER

			// Switch to last brush mode used when hitting tab
			if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Tab)
			{
				Event.current.Use();
				GUIUtility.hotControl = controlID;

				Painting.PaintMode curMode = painter.paint_Brush.mode;
				if ((int)painter.temp_prevPaintMode >= 0)
					painter.paint_Brush.mode = painter.temp_prevPaintMode;
				painter.temp_prevPaintMode = curMode;

				//undo = "Brush Mode";
			}
			else if (GUIUtility.hotControl == controlID)
				GUIUtility.hotControl = 0;



			return blockPainting;
		}

		#endregion

		#region GUI Functions

		private void DrawCanvasPreview(Rect textureRect, Texture texture, Painting.Channels channels)
		{
			switch ((int)channels)
			{
				case 0:
					RTTextureViz.DrawTexture(texture, textureRect, 1, 2, 3, 4);
					break;
				case 1:
					RTTextureViz.DrawTexture(texture, textureRect, 1, 2, 3, 5);
					break;
				case 2:
					RTTextureViz.DrawTexture(texture, textureRect, 1, 1, 1, 5);
					break;
				case 3:
					RTTextureViz.DrawTexture(texture, textureRect, 2, 2, 2, 5);
					break;
				case 4:
					RTTextureViz.DrawTexture(texture, textureRect, 3, 3, 3, 5);
					break;
				case 5:
					RTTextureViz.DrawTexture(texture, textureRect, 4, 4, 4, 5);
					break;
				case 6:
					RTTextureViz.DrawTexture(texture, textureRect, 1);
					break;
				default:
					RTTextureViz.DrawTexture(texture, textureRect);
					break;
			}
		}

		private void setupBrushVizMat()
		{
			if (brushGUIMaterial == null)
				brushGUIMaterial = new Material(Shader.Find("Hidden/BrushGUIViz"));
			if (GlobalPainting.brushTextures == null)
				GlobalPainting.ReloadBrushTextures();
			if (GlobalPainting.brushTextures == null || GlobalPainting.brushTextures.Length <= 0)
				return;
			
			GlobalPainting.MatSetupViz(painter, brushGUIMaterial);
			brushGUIMaterial.SetFloat("_aspect", 1);
		}

		private void ShowUndoButtons()
		{
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("< " + painter.getNextUndoName, GUI.skin.button, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth / 2 - 25)))
			{
				painter.PerformUndo();
				GUI.changed = false;
			}
			if (GUILayout.Button("" + painter.getNextRedoName + " >", GUI.skin.button, GUILayout.MinWidth(EditorGUIUtility.currentViewWidth / 2 - 25)))
			{
				painter.PerformRedo();
				GUI.changed = false;
			}
			GUILayout.EndHorizontal();
		}

		public static void SubSectionSeperator()
		{
			//GUIUtility.Seperator ();
			EditorGUILayout.Space();
		}

		public static void SectionSeperator()
		{
			AdditionalGUIUtility.Seperator();
			//EditorGUILayout.Space ();
		}

		private static void ShowChannelMods(ref Painting.ChannelMod R, ref Painting.ChannelMod G, ref Painting.ChannelMod B, ref Painting.ChannelMod A)
		{

			GUILayout.BeginHorizontal();

			GUILayout.BeginVertical(GUILayout.MaxWidth(80));
			GUILayout.Label("Shuffle");
			R.shuffle = (int)(Painting.ChannelValue)ShortEnumPopup("R ->", (Painting.ChannelValue)R.shuffle);
			G.shuffle = (int)(Painting.ChannelValue)ShortEnumPopup("G ->", (Painting.ChannelValue)G.shuffle);
			B.shuffle = (int)(Painting.ChannelValue)ShortEnumPopup("B ->", (Painting.ChannelValue)B.shuffle);
			A.shuffle = (int)(Painting.ChannelValue)ShortEnumPopup("A ->", (Painting.ChannelValue)A.shuffle);
			GUILayout.EndVertical();
			EditorGUILayout.Space();

			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			GUILayout.Label("Offset");
			R.offset = ShortSlider(R.offset, -1, 1);
			G.offset = ShortSlider(G.offset, -1, 1);
			B.offset = ShortSlider(B.offset, -1, 1);
			A.offset = ShortSlider(A.offset, -1, 1);
			GUILayout.EndVertical();
			EditorGUILayout.Space();

			GUILayout.BeginVertical(GUILayout.ExpandWidth(true));
			GUILayout.Label("Scale");
			R.scale = ShortSlider(R.scale, 0, 2);
			G.scale = ShortSlider(G.scale, 0, 2);
			B.scale = ShortSlider(B.scale, 0, 2);
			A.scale = ShortSlider(A.scale, 0, 2);
			GUILayout.EndVertical();
			EditorGUILayout.Space();

			GUILayout.BeginVertical(GUILayout.Width(40));
			GUILayout.Label("Invert");
			R.invert = EditorGUILayout.Toggle(R.invert);
			G.invert = EditorGUILayout.Toggle(G.invert);
			B.invert = EditorGUILayout.Toggle(B.invert);
			A.invert = EditorGUILayout.Toggle(A.invert);
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();
		}

		private static Enum ShortEnumPopup(string label, Enum selected)
		{
			GUILayout.BeginHorizontal();
			GUILayout.Label(label, GUILayout.ExpandWidth(true));
			selected = EditorGUILayout.EnumPopup(selected, GUILayout.ExpandWidth(false));
			GUILayout.EndHorizontal();
			return selected;
		}

		private static float ShortSlider(float value, float min, float max)
		{
			GUILayout.BeginHorizontal();
			value = GUILayout.HorizontalSlider(value, min, max, new GUILayoutOption[] { GUILayout.MinWidth(40), GUILayout.MaxHeight(16) });
			value = Mathf.Clamp(EditorGUILayout.FloatField(value, GUILayout.MaxWidth(50)), min, max);
			GUILayout.EndHorizontal();
			return value;
		}

		private static bool PresetButton(string label, Color color, GenericMenu.MenuFunction2 deletePreset, int index)
		{
			Rect rect = GUILayoutUtility.GetRect(new GUIContent(label), colorButton);//, new GUILayoutOption[] { GUILayout.Width (colorButton.fixedWidth), GUILayout.Height (colorButton.fixedHeight) });
			if (deletePreset != null && Event.current.type == EventType.MouseDown && Event.current.button == 1 && rect.Contains(Event.current.mousePosition))
			{
				GenericMenu editMenu = new GenericMenu();
				editMenu.AddItem(new GUIContent("Delete"), false, deletePreset, index as object);
				editMenu.DropDown(rect);
				Event.current.Use();
			}
			GUI.color = new Color(color.r, color.g, color.b, 1);
			bool clicked = GUI.Button(rect, label, colorButton);
			GUI.color = Color.white;
			return clicked;
		}

		private static Rect DrawResponsiveGrid(int gridSize, int cellSize, int cellCount, Action<int, Rect> cellFunc, GUIStyle style = null)
		{
			if (cellFunc == null || gridSize <= 0 || cellSize <= 0 || cellCount <= 0)
				return new Rect();
			int padding = style == null ? 0 : (style.margin.left + style.margin.right) / 2;
			int cellsPerRow = Mathf.FloorToInt((gridSize + padding) / (cellSize + padding));
			int rows = Mathf.CeilToInt((float)cellCount / cellsPerRow);
			int cellInd = 0;
			Rect completeRect = new Rect();
			for (int row = 0; row < rows; row++)
			{
				GUILayout.BeginHorizontal();
				for (int col = 0; col < cellsPerRow && cellInd < cellCount; col++)
				{
					Rect cellRect = style == null ? GUILayoutUtility.GetRect(cellSize, cellSize) : GUILayoutUtility.GetRect(cellSize, cellSize, style);
					if (cellInd == 0) completeRect = cellRect;
					cellFunc.Invoke(cellInd, cellRect);
					cellInd++;
				}
				GUILayout.FlexibleSpace();
				GUILayout.EndHorizontal();
			}
			completeRect.size = new Vector2(cellsPerRow * cellSize, rows * cellSize);
			return completeRect;
		}

		#endregion
	}
}