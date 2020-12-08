using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEditor;

namespace TerrainComposer2.NodePainter
{
	[CustomEditor(typeof(NodePainter))]
	public class NodePainterEditor : Editor
	{
		#region Variables

		public NodePainter nodePainter;
		public PaintingEditor editor;

		// Paint User IDs
		private string SceneViewID { get { return "Scene-" + SceneView.lastActiveSceneView.GetInstanceID(); } }
		private string GUIWindowID { get { return "GUI-" + this.GetInstanceID(); } }

		// GUI options
		public bool vizCanvasOnTerrain = false;
		public bool expandTargets = false;

		// GUI Styles
		private static Texture2D lockIcon;
		private static Texture2D unlockIcon;
		private static GUIStyle lockStyle;

		// DEBUG
		private static bool debugGenerateRect = false;
		private static bool debugNodeRect = false;
		private static bool debugAreaRect = false;

		#endregion


		#region General Methods

		public void OnEnable()
		{
			// Setup editor
			nodePainter = (NodePainter)target;
			nodePainter.CheckChannelTargetUpgrade();
			if (nodePainter.painter == null)
				throw new Exception("Painter not initialized!");
			editor = new PaintingEditor(nodePainter.painter);

			// Init editor
			nodePainter.UpdateTerrains();
			GlobalPainting.UpdateTerrainBrush();
			GlobalPainting.HideCanvasProjection();

			if (!System.IO.Directory.Exists(Settings.paintingResourcesFolder))
			{ // Send out warning incase settings are faulty
				Debug.LogWarning("Resource folder does not exist! Please select a valid path in the settings!");
				return;
			}
		}

		public void OnDisable()
		{ // Close painter when inspector is closed
			editor.Close();
			GlobalPainting.UpdateTerrainBrush();
			GlobalPainting.HideCanvasProjection();
		}

		private void CheckGUIStyles()
		{
			if (lockIcon == null)
				lockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Settings.paintingResourcesFolder + "/GUI/GUI_LockOn.png");
			if (unlockIcon == null)
				unlockIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(Settings.paintingResourcesFolder + "/GUI/GUI_LockOff.png");
			if (lockStyle == null)
			{
				lockStyle = new GUIStyle();
				lockStyle.normal.background = lockIcon;
				lockStyle.onNormal.background = unlockIcon;
				lockStyle.fixedHeight = lockStyle.fixedWidth = 30;
				lockStyle.margin = new RectOffset(5, 5, 5, 5);
			}
		}

		#endregion

		#region Scene View

		public void OnSceneGUI()
		{
			if (!nodePainter.painter.canvas_Exists)
				return;

			if (vizCanvasOnTerrain)
			{ // Show canvas projection
				GlobalPainting.ShowCanvasProjection(nodePainter.transform.position + Vector3.up * nodePainter.canvasTargetSize.y,
													nodePainter.transform.rotation,
													nodePainter.canvasSize2D,
													nodePainter.painter.tex_VizCanvas,
													nodePainter.painter.canvas_Format == Painting.Format.Value);

				// Show border
				Vector2 cMinMin = RotateVector(Vector2.Scale(new Vector2(-1, -1), nodePainter.canvasSize2D / 2), -nodePainter.rotation) + nodePainter.position;
				Vector2 cMinMax = RotateVector(Vector2.Scale(new Vector2(-1, +1), nodePainter.canvasSize2D / 2), -nodePainter.rotation) + nodePainter.position;
				Vector2 cMaxMax = RotateVector(Vector2.Scale(new Vector2(+1, +1), nodePainter.canvasSize2D / 2), -nodePainter.rotation) + nodePainter.position;
				Vector2 cMaxMin = RotateVector(Vector2.Scale(new Vector2(+1, -1), nodePainter.canvasSize2D / 2), -nodePainter.rotation) + nodePainter.position;
				Handles.DrawSolidRectangleWithOutline(
					new Vector3[] {
						new Vector3(cMinMin.x, nodePainter.transform.position.y, cMinMin.y),
						new Vector3(cMinMax.x, nodePainter.transform.position.y, cMinMax.y),
						new Vector3(cMaxMax.x, nodePainter.transform.position.y, cMaxMax.y),
						new Vector3(cMaxMin.x, nodePainter.transform.position.y, cMaxMin.y) },
					new Color(1, 1, 1, 0), GlobalPainting.canvasTerrainVizColor);
			}

			#region Debug
			if (debugGenerateRect)
			{
				Rect rect = new Rect(Vector2.Scale(nodePainter.paintStrokeArea.position - Vector2.one / 2, nodePainter.areaSize2D),
									Vector2.Scale(nodePainter.paintStrokeArea.size, nodePainter.areaSize2D));
				rect.position += new Vector2(nodePainter.areaPos.x, nodePainter.areaPos.z);
				DrawDebugRect(rect, nodePainter.transform.position.y, Color.red);
			}

			if (debugNodeRect)
			{
				DrawDebugRect(nodePainter.rect_World, nodePainter.transform.position.y, Color.red);
			}

			if (debugAreaRect)
			{
				Rect rect = new Rect(0, 0, nodePainter.areaSize.x, nodePainter.areaSize.z);
				rect.center = new Vector2(nodePainter.areaPos.x, nodePainter.areaPos.z);
				DrawDebugRect(rect, nodePainter.areaPos.y, Color.red);
			}
			#endregion

			// Register stop painting
			if ((Event.current.type == EventType.MouseUp && Event.current.button == 0) || Event.current.button != 0)
				nodePainter.painter.StopPainting(SceneViewID);

			if (!editor.isMouseInWindow || Event.current.modifiers == EventModifiers.Alt)
			{ // Allow circling around in scene view
				GlobalPainting.UpdateTerrainBrush();
				return;
			}

			// Block painting while using any other Tool
			nodePainter.painter.state_BlockPainting = Tools.current != Tool.None;
			// Register control
			editor.controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(editor.controlID);
			// Make sure this control receives mouse clicks
			if (Event.current.GetTypeForControl(editor.controlID) == EventType.MouseDown)
			{ // Block left-click in scene view
				GUIUtility.hotControl = editor.controlID;
			}

			Vector2 brushPos;
			Vector3 worldPos;
			if (nodePainter.CalcBrushWorldPos(out brushPos, out worldPos))
			{ // Mouse is over terrain, brush position was calculated

				if (Event.current.type != EventType.Repaint && Event.current.type != EventType.Layout)
				{ // Handle shortcuts with Undo support
					Undo.RecordObject(nodePainter, "Node Painter Shortcut");
					bool blocked = editor.HandleShortcuts();
					// Block painting when shortcuts call for it (Ctrl+Click)
					nodePainter.painter.state_BlockPainting = nodePainter.painter.state_BlockPainting || blocked;
				}

				if (Event.current.type == EventType.Layout)
				{ // Update terrain brush on terrain
					if (nodePainter.painter.state_BlockPainting)
					{ // Hide brush when blocked
						GlobalPainting.UpdateTerrainBrush();
					}
					else
					{ // Show brush in respect to canvas/area/brush sizes
						float brushSize = nodePainter.painter.paint_Brush.size * nodePainter.canvasTargetSize.x * nodePainter.scale.x;
						GlobalPainting.ShowTerrainBrush(nodePainter.painter, worldPos, brushSize, nodePainter.rotation, nodePainter.canvasSize2D.y / nodePainter.canvasSize2D.x);
					}
				}

				// Update paint state and check whether brush intersects with the canvas
				nodePainter.painter.state_BrushPos = brushPos;
				float expand = nodePainter.painter.paint_Brush.size;
				nodePainter.painter.isOnCanvas = (brushPos.x >= -expand && brushPos.x <= 1 + expand) && (brushPos.y >= -expand && brushPos.y <= 1 + expand);

				// Apply paint update for control
				if (nodePainter.painter.PaintUpdate(SceneViewID, editor.controlID))
					Repaint();
			}
			else
			{ // Mouse not over terrain, hide brush and disable control
				if (GUIUtility.hotControl == editor.controlID)
					GUIUtility.hotControl = 0;
				GlobalPainting.UpdateTerrainBrush();
			}

			if (editor.isMouseInWindow && !nodePainter.painter.state_BlockPainting && 
				(Event.current.type == EventType.MouseMove || Event.current.button == 0))
				HandleUtility.Repaint();
		}

		/// <summary>
		/// Draw Rectangle in Scene View for debug purposes
		/// </summary>
		private void DrawDebugRect(Rect rect, float height, Color col)
		{
			Handles.DrawSolidRectangleWithOutline(
				new Vector3[] {
					new Vector3(rect.xMin, height, rect.yMin),
					new Vector3(rect.xMax, height, rect.yMin),
					new Vector3(rect.xMax, height, rect.yMax),
					new Vector3(rect.xMin, height, rect.yMax) }, 
				new Color(1, 1, 1, 0), col);
		}

		private Vector2 RotateVector(Vector2 vec, float rotation)
		{
			float sin = Mathf.Sin(rotation * Mathf.Deg2Rad);
			float cos = Mathf.Cos(rotation * Mathf.Deg2Rad);

			float tx = vec.x;
			float ty = vec.y;
			vec.x = (cos * tx) - (sin * ty);
			vec.y = (sin * tx) + (cos * ty);
			return vec;
		}

		#endregion

		#region GUI

		public override void OnInspectorGUI()
		{
			if (editor == null)
				return;

			// Update user ID
			editor.GUIWindowID = GUIWindowID;

			// Register stop painting
			if ((Event.current.type == EventType.MouseUp && Event.current.button == 0) || Event.current.button != 0)
				nodePainter.painter.StopPainting(GUIWindowID);

			// Set up styles and textures
			CheckGUIStyles();
			
			
			// Debug
			/*GUILayout.Label("Area Rect: " + nodePainter.rect_Area.ToString());
			GUILayout.Label("Raw Paint Rect: " + nodePainter.paintArea.ToString());
			GUILayout.Label("Extended Paint Rect: " + nodePainter.paintStrokeArea.ToString());

			PaintingEditor.SubSectionSeperator();

			GUILayout.Label("Canvas Ratio: " + nodePainter.canvasRatio.ToString());
			GUILayout.Label("Rotated Canvas Ratio: " + nodePainter.effectiveCanvasRatio.ToString());

			PaintingEditor.SubSectionSeperator();

			GUILayout.Label("Canvas Res Aspect: " + nodePainter.painter.canvas_Aspect.ToString());
			GUILayout.Label("Canvas Size Aspect: " + nodePainter.canvasAspect.ToString());
			GUILayout.Label("Area Aspect: " + nodePainter.areaAspect.ToString());
			GUILayout.Label("Total Aspect: " + (nodePainter.canvasAspect / nodePainter.painter.canvas_Aspect).ToString());

			PaintingEditor.SectionSeperator();*/


			// Draw painter UI first
			Undo.RecordObject(nodePainter, "Node Painter Settings");
			editor.DoPainterUI();

			PaintingEditor.SectionSeperator();

			if (nodePainter.painter.canvas_Exists)
			{ // Draw NodePainter-specific UI afterwards

				expandTargets = EditorGUILayout.Foldout(expandTargets, "Node Targets", PaintingEditor.headerFoldout);
				if (expandTargets)
				{
					// Allow to edit assigned TCArea
					nodePainter.TCArea = (TC_Area2D)EditorGUILayout.ObjectField("TC2 Area 2D", nodePainter.TCArea, typeof(TC_Area2D), true);

					// Use current node as node target
					if (nodePainter.painter.canvas_Format != Painting.Format.Multi && nodePainter.TCNode != null)
						GUILayout.Label("Current node: " + nodePainter.TCNode.name);

					// Draw node target list
					DrawNodeTargetList(nodePainter.painter.canvasChannels[nodePainter.painter.curChannelIndex].targets);

					// Display message to lock inspector to be able to drag nodes
					GUILayout.BeginHorizontal();
					bool newLock = GUILayout.Toggle(ActiveEditorTracker.sharedTracker.isLocked, GUIContent.none, lockStyle);
					if (newLock != ActiveEditorTracker.sharedTracker.isLocked)
					{ // Switch lock state
						Selection.activeGameObject = nodePainter.gameObject;
						ActiveEditorTracker.sharedTracker.isLocked = newLock;
						ActiveEditorTracker.sharedTracker.RebuildIfNecessary();
					}
					EditorGUILayout.HelpBox("In order to be able to drag and drop nodes into the fields, you need to lock the inspector (click the lock)!", MessageType.Info);
					GUILayout.EndHorizontal();
				}
			}

			PaintingEditor.SectionSeperator();

			// Bottom settings button
			if (GUILayout.Button("Show Settings", EditorStyles.toolbarButton))
				SettingsWindow.Open();

			PaintingEditor.SubSectionSeperator();

			//if (editor.isMouseInWindow)
				Repaint();

			if (vizCanvasOnTerrain != nodePainter.painter.visualizeCanvas)
			{ // Update canvas projection on terrain after triggering in painter UI
				vizCanvasOnTerrain = nodePainter.painter.visualizeCanvas;
				if (vizCanvasOnTerrain)
				{ // Show projection
					GlobalPainting.ShowCanvasProjection(nodePainter.transform.position + Vector3.up * nodePainter.canvasTargetSize.y,
													nodePainter.transform.rotation,
													nodePainter.canvasSize2D,
													nodePainter.painter.tex_VizCanvas,
													nodePainter.painter.canvas_Format == Painting.Format.Value);
				}
				else
				{ // Hide Canvas Projection
					GlobalPainting.HideCanvasProjection();
				}

				HandleUtility.Repaint();
				SceneView.RepaintAll();
			}
		}

		/// <summary>
		/// Draw the specified node list for editing
		/// </summary>
		private void DrawNodeTargetList(List<TC_Node> nodeList)
		{
			for (int i = 0; i < nodeList.Count; i++)
			{
				GUILayout.BeginHorizontal();
				nodeList[i] = EditorGUILayout.ObjectField(nodeList[i], typeof(TC_Node), true) as TC_Node;
				if (GUILayout.Button("X", GUILayout.Width(20)))
				{
					nodeList.RemoveAt(i);
					i--;
				}
				GUILayout.EndHorizontal();
			}

			if (nodeList.Count == 0)
				GUILayout.Label("No node targets specified!");
			if (GUILayout.Button("Add Additional Node Target"))
				nodeList.Add(null);
		}

		/// <summary>
		/// Scans the group the node targets are in and fetches their preview 
		/// (currently unused due to undesired result)
		/// </summary>
		private void GetPreviewTextures(bool overwriteExisting = false)
		{
			for (int ch = 0; ch < nodePainter.painter.canvasChannelCount; ch++)
			{
				if (nodePainter.painter.canvasChannels[ch].displayTexture != null && !overwriteExisting)
					continue;
				List<TC_Node> nodes = nodePainter.painter.canvasChannels[ch].targets;
				TC_Node node = nodes == null ? null : nodes.FirstOrDefault(n => n != null); // TODO: Filter by outputID - make sure each preview is of the same output if possible
				if (node != null)
				{
					// Search layer the node is in
					TC_ItemBehaviour parent = node;
					while (parent.parentItem != null)
					{
						parent = parent.parentItem;
						if (parent.GetType() == typeof(TC_Layer) || parent.GetType() == typeof(TC_LayerGroup))
							break;
					}

					TC_Layer layer = parent as TC_Layer;
					TC_LayerGroup layerGroup = parent as TC_LayerGroup;
					TC_ItemBehaviour previewItem = null;

					if (layer != null)
						previewItem = (TC_ItemBehaviour)layer.selectItemGroup ?? (TC_ItemBehaviour)layer.selectNodeGroup;
					else if (layerGroup != null)
						previewItem = layerGroup.groupResult;

					if (previewItem != null)
					{ // Get preview texture of the given item
						Texture texPreview = null;
						if (previewItem.outputId == TC.colorOutput && previewItem.preview.tex != null)
							texPreview = previewItem.preview.tex;
						else
						{
							if (previewItem.rtDisplay != null) texPreview = previewItem.rtDisplay;
							else if (previewItem.rtPreview != null) texPreview = previewItem.rtPreview;
							else if (previewItem.preview.tex != null) texPreview = previewItem.preview.tex;
						}

						if (texPreview != null)
						{ // Assign preview texture
							nodePainter.painter.canvasChannels[ch].displayTexture = texPreview;
						}
					}
				}
			}
		}
		#endregion
	}
}