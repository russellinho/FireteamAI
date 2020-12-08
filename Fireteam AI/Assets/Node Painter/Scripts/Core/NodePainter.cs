using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace TerrainComposer2.NodePainter
{
	public partial class CanvasChannel
	{
		[UnityEngine.Serialization.FormerlySerializedAs("NodePainter.ChannelNodeTargets.targets")]
		public List<TC_Node> targets = new List<TC_Node>();
	}

	[ExecuteInEditMode]
	public class NodePainter : MonoBehaviour
	{
		// Painter
		public Painting painter;

		#region Terrain Composer 2

		// TC2 Areas
		public TC_Area2D repTCArea; // Optional overwriting TC2 area
		public TC_Area2D TCArea
		{ // Error here? Change TC_Area2D.instance to TC_Area2D.current for earlier versions of TC2
			get { return repTCArea == null ? TC_Area2D.instance : repTCArea; }
			set { repTCArea = TC_Area2D.instance != value ? value : null; }
		}
		public TC_TerrainArea TCTerrainArea { get {
				return TCArea != null ?
						(TCArea.currentTerrainArea != null ?
							TCArea.currentTerrainArea :
							(TCArea.terrainAreas.Length > 0 ? TCArea.terrainAreas[0] : null))
						: null;
		} }
		public bool TC2Ready { get { return TC_Generate.instance != null && TCArea != null; } }

		// Node Targets
		public TC_Node TCNode; // Node ON this object
		private TC_Node TCNode_Rep; // Cached node chosen as a representative out of all node targets
		public TC_Node TCNodeTarget { get {
				return
					TCNode != null ? TCNode : (TCNode_Rep != null ? TCNode_Rep :
					(TCNode_Rep = painter.canvasChannels.SelectMany((t) => t.targets).FirstOrDefault((n) => n != null)));
		} }

		// Terrain Setup
		private TerrainCollider[] terrainColliders;

		#endregion

		#region Size Transformations

		// Canvas
		public Bounds unitBounds { get { return TCNodeTarget != null ? TCNodeTarget.bounds : TCArea.bounds; } }
		public Vector3 canvasTargetSize { get { return TCNodeTarget != null ? TCNodeTarget.size : (TCArea != null ? TCArea.bounds.size : new Vector3 (1000, 1000, 1000)) ; } }
		public Vector3 canvasSize { get { return Vector3.Scale(canvasTargetSize, transform.lossyScale); } }
		public Vector2 canvasSize2D { get { return new Vector2(canvasSize.x, canvasSize.z); } }

		// Area
		public Int2 areaTiles { get { return TCTerrainArea != null ? TCTerrainArea.tiles : new Int2(1, 1); } }
		public Vector3 terrainSize { get { return TCTerrainArea != null ? TCTerrainArea.terrainSize : (TCArea != null ? TCArea.terrainSize : new Vector3(1024, 1000, 1024)); } }
		public Vector3 areaSize { get { return new Vector3(terrainSize.x * areaTiles.x, terrainSize.y, terrainSize.z * areaTiles.y); } }
		public Vector2 areaSize2D { get { return new Vector2(areaSize.x, areaSize.z); } }
		public Vector3 areaPos { get { return TCTerrainArea != null ? TCTerrainArea.transform.position : Vector3.zero; } }
		public Vector2 canvasRatio { get { return new Vector2(canvasSize.x / areaSize.x, canvasSize.z / areaSize.z); } }
		public float areaAspect { get { return areaSize2D.y / areaSize2D.x; } }

		// Painter
		public Vector2 scale { get { return new Vector2(transform.lossyScale.x, transform.lossyScale.z); } }
		public Vector2 position { get { return new Vector2(transform.position.x, transform.position.z); } }
		public float rotation { get { return transform.rotation.eulerAngles.y; } }

		/// <summary>
		/// The axis-aligned world-space rectangle this canvas occupies
		/// </summary>
		public Rect rect_World
		{
			get
			{
				return RotateAARect(
					new Rect(
						position.x - canvasSize.x / 2,
						position.y - canvasSize.z / 2,
						canvasSize.x, canvasSize.z),
					rotation);
			}
		}

		/// <summary>
		/// The axis-aligned area-space rectangle this canvas occupies (in range of (0,0,1,1)) used for TC2 interaction
		/// </summary>
		public Rect rect_Area
		{
			get
			{
				return RotateAARect(
					new Rect(
						(position.x - canvasSize.x / 2 - areaPos.x) / areaSize.x + 0.5f,
						(position.y - canvasSize.z / 2 - areaPos.z) / areaSize.z + 0.5f,
						canvasRatio.x, canvasRatio.y),
					rotation);
			}
		}

		#endregion

		#region Temporary Paint Record

		// Temporary paint record
		private bool buildingPaintArea;
		public Rect paintArea = new Rect();
		private float paintBrushSize;

		/// <summary>
		/// The area-space rectangle the paint strokes since the last TC2 generation covered, used for TC2 interaction
		/// </summary>
		public Rect paintStrokeArea
		{
			get
			{
				// Create local size vector due to brush size and stretching
				Vector2 offset = paintBrushSize * canvasRatio;
				offset.y = offset.y / painter.canvas_Aspect;
				// Rotate brush size with canvas, respecting area aspect
				offset.y = offset.y * areaAspect;
				offset = RotateSizeVector(offset, rotation);
				offset.y = offset.y / areaAspect;
				// Build extended area from paint positions in area space
				Rect rect = new Rect(
						paintArea.x - offset.x / 2,
						paintArea.y - offset.y / 2,
						paintArea.width + offset.x,
						paintArea.height + offset.y);
				return rect;
			}
		}

		private Rect cachedAARect;

		#endregion

		#region Deprecated
		// Only used for upgrading old painters

		[System.Serializable]
		private class ChannelNodeTargets
		{
			public List<TC_Node> targets = new List<TC_Node>();
		}
		[SerializeField]
		private List<ChannelNodeTargets> channelTargets = null;

		public void CheckChannelTargetUpgrade()
		{
			if (channelTargets != null && painter != null)
			{ // Upgrade channel target representation
				for (int ch = 0; ch < painter.canvasChannels.Count && ch < channelTargets.Count; ch++)
					painter.canvasChannels[ch].targets = channelTargets[ch].targets;
				if (painter.canvasChannels.Count != 1 || channelTargets.Count == 1)
					channelTargets = null; // Can delete old data as it has been sucessfully transferred
			}
		}

		#endregion


		#region General Methods

		/// <summary>
		/// Validates that this ID is unique to this scene
		/// </summary>
		private void ValidateID()
		{
			if (painter.ID == 0 || GameObject.FindObjectsOfType<NodePainter>().Any((NodePainter np) => np != this && np.painter.ID == painter.ID))
				painter.RecreateID();
		}

		/// <summary>
		/// Checks for older versions if the instance manager, used for saving the cache, exists
		/// </summary>
		private void CheckInstanceManager()
		{
#if UNITY_EDITOR && !UNITY_5_6_OR_NEWER
			Transform host = FindObjectOfType<TC_Settings>().transform.parent;
            if (host.FindChild("Hidden") == null)
                new GameObject("Hidden").transform.parent = host;
            host = host.FindChild("Hidden");

            PainterInstanceManager instanceManager = FindObjectOfType<PainterInstanceManager>();
			if (instanceManager.gameObject != host)
			{
				DestroyImmediate(instanceManager);
				instanceManager = null;
			}
			if (instanceManager == null)
				instanceManager = host.gameObject.AddComponent<PainterInstanceManager>();
            //host.hideFlags = HideFlags.HideInHierarchy;
#endif
		}

        private void Awake()
        {
#if !UNITY_EDITOR
            enabled = false;
#endif
            if (Application.isPlaying)
                enabled = false;
        }

        private void OnEnable()
		{
#if UNITY_EDITOR
            if (UnityEditor.BuildPipeline.isBuildingPlayer)
                return;
#endif

            // Check for any node (not necessary)
            TCNode = GetComponent<TC_Node>();

			// Assure painter
			if (painter == null)
				painter = new Painting();
			CheckChannelTargetUpgrade();
			// Set up painter callbacks
			painter.OnAssignCanvas -= AssignCanvas;
			painter.OnAssignCanvas += AssignCanvas;
			painter.OnPainting -= UpdatePaintArea;
			painter.OnPainting += UpdatePaintArea;
			painter.OnScaleCanvas -= ScaleRelative;
			painter.OnScaleCanvas += ScaleRelative;
			// Open and init painter
			painter.Open();
#if UNITY_EDITOR
			if (!UnityEditor.Selection.activeGameObject == gameObject)
#endif
				painter.Hide();

			// Gets all TC2 terrains to use as a brush hit target
			UpdateTerrains();

			// Set up cache save callbacks for newer versions
#if UNITY_EDITOR && UNITY_5_6_OR_NEWER
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= SaveCacheWithScene;
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving += SaveCacheWithScene;
#endif
			
#if UNITY_EDITOR && UNITY_2017_2_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged -= EnsureCanvasSaved;
            UnityEditor.EditorApplication.playModeStateChanged += EnsureCanvasSaved;
#elif UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= EnsureCanvasSaved;
            UnityEditor.EditorApplication.playmodeStateChanged += EnsureCanvasSaved;
#endif
		}

		private void OnDisable()
		{
			// Remove painter callbacks just in case
			painter.OnAssignCanvas -= AssignCanvas;
			painter.OnPainting -= UpdatePaintArea;
			painter.OnScaleCanvas -= ScaleRelative;
			// Close painter
			painter.Close();

			// Remove cache save callbacks for newer versions
#if UNITY_EDITOR && UNITY_5_6_OR_NEWER
			UnityEditor.SceneManagement.EditorSceneManager.sceneSaving -= SaveCacheWithScene;
#endif

#if UNITY_EDITOR && UNITY_2017_2_OR_NEWER
            UnityEditor.EditorApplication.playModeStateChanged -= EnsureCanvasSaved;
#elif UNITY_EDITOR
            UnityEditor.EditorApplication.playmodeStateChanged -= EnsureCanvasSaved;
#endif
		}

		private void Reset()
		{
			// Check for any node (not necessary)
			TCNode = GetComponent<TC_Node>();
		}

		private void Start()
		{
			// Check for any node (not necessary)
			TCNode = GetComponent<TC_Node>();
			// Validation
			CheckInstanceManager();
			ValidateID();
		}

		private void Update()
		{
			if (painter == null)
                return;

            // Validate cache asset
            painter.CheckCacheAsset();
            
			// Check node targets
            CheckNodeTargets ();

            painter.canvas_Rotation = rotation;
			if (transform.hasChanged)
			{ // Watch transform changes and update node targets aswell as trigger generation
				transform.hasChanged = false;
				UpdateNodeTargets();
				Regenerate();
			}
		}
		
		/// <summary>
		/// Check node targets and request content if necessary
		/// </summary>
		private void CheckNodeTargets () 
		{
			if (painter.canvas_Exists)
            {
                if (TCNode != null && (TCNode.inputKind != InputKind.File || TCNode.inputFile != InputFile.Image || TCNode.stampTex == null))
                {
                    painter.RequestContent();
                    TC.RefreshOutputReferences(TC.allOutput);
                }
                else
                {
                    for (int ch = 0; ch < painter.canvasChannelCount && ch < painter.canvasChannels.Count; ch++)
                    { // Update all active node targets
                        CanvasChannel channelTarget = painter.canvasChannels[ch];
                        for (int i = 0; i < channelTarget.targets.Count; i++)
                        {
                            TC_Node node = channelTarget.targets[i];
							if (node != null && (node.inputKind != InputKind.File || node.inputFile != InputFile.Image || node.stampTex == null))
							{
								painter.RequestContent();
								TC.RefreshOutputReferences(TC.allOutput);
								return;
							}
                        }
                    }
                }
            }	
		}

		/// <summary>
		/// Regenerates the whole node area
		/// </summary>
		public void Regenerate()
		{
			// Calculate changed area
			Rect oldAARect = cachedAARect;
			Rect generateRect = cachedAARect = rect_Area;
			if (oldAARect.size.x >= 0 && oldAARect.size.y >= 0)
				Mathw.EncapsulteRect(ref generateRect, oldAARect);
            // Regenerate that area
            TC.AutoGenerate(generateRect);
		}

		/// <summary>
		/// Rotates the given rectangle around it's center with the specified rotation in degrees and returns it's new Axis-Aligned bounding rectangle
		/// </summary>
		private Rect RotateAARect(Rect rect, float rotation)
		{
			Vector2 center = rect.center;
			rect.size = RotateSizeVector(rect.size, rotation);
			rect.center = center; // Keep center
			return rect;
		}

		/// <summary>
		/// Rotates the given vector around itself in such a way that the rectangle it represents is rotated
		/// </summary>
		private Vector2 RotateSizeVector (Vector2 size, float rotation)
		{
			float rotRadians = rotation * Mathf.PI / 180;
			float sinR = Mathf.Abs(Mathf.Sin(rotRadians)), cosR = Mathf.Abs(Mathf.Cos(rotRadians));
			return new Vector2(size.y * sinR + size.x * cosR, size.x * sinR + size.y * cosR);
		}

		/// <summary>
		/// Scales this transform with the given relative values (used for expanding canvas from the UI)
		/// </summary>
		private void ScaleRelative(float x, float y)
		{
			transform.localScale = new Vector3(transform.localScale.x * x, transform.localScale.y, transform.localScale.z * y);
		}

#if UNITY_EDITOR && UNITY_5_6_OR_NEWER
		private void SaveCacheWithScene(UnityEngine.SceneManagement.Scene scene, string path)
		{ // Save node painter caches from scene save callback
			if (gameObject.scene == scene)
				painter.SaveCurrentSession(true);
		}
#endif

#if UNITY_EDITOR && UNITY_2017_2_OR_NEWER
        private void EnsureCanvasSaved(UnityEditor.PlayModeStateChange change)
#else
        private void EnsureCanvasSaved()
#endif
		{
#if UNITY_EDITOR
            if (painter.hasUnsavedChanges)
			{ // Canvas is not saved
			  // Ask user if it should be saved
				if (UnityEditor.EditorUtility.DisplayDialog("Save Canvas", "Canvas data has not been saved and will be lost. Save now?", "OK"))
				{
					painter.SaveCurrentSession();
				}
			}
#endif
        }

		#endregion

        #region Canvas-Node Interaction

		/// <summary>
		/// Assigns the given textures to all node targets.
		/// Accepts only multiple in Multi-Format.
		/// </summary>
		public void AssignCanvas(params RenderTexture[] textures)
		{
#if UNITY_EDITOR // Mark scene dirty
			if (!Application.isPlaying)
#if UNITY_5_3 || UNITY_5_3_OR_NEWER
				UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty (gameObject.scene);
#else
				UnityEditor.EditorApplication.MarkSceneDirty();
#endif
#endif
			UpdateNodeTargets();

			if (textures == null || textures.Length == 0 || textures.Any((RT) => RT == null))
			{ // Assign black textures to all node targets
				if (TCNode != null)
					AssignToNode(Texture2D.blackTexture, TCNode);
				for (int ch = 0; ch < painter.canvasChannels.Count; ch++)
				{
					CanvasChannel channelTarget = painter.canvasChannels[ch];
					for (int i = 0; i < channelTarget.targets.Count; i++)
						AssignToNode(Texture2D.blackTexture, channelTarget.targets[i]);
				}
			}
			else
			{ // Assign passed textures to all node targets available
				if (TCNode != null)
					AssignToNode(textures[0], TCNode);
				for (int ch = 0; ch < painter.canvasChannelCount && ch < painter.canvasChannels.Count; ch++)
				{ // Assign to node targets per channel (also works on one channel in non-Multi-Formats)
					CanvasChannel channelTarget = painter.canvasChannels[ch];
					int texIndex = Mathf.Clamp(Mathf.FloorToInt((float)ch / 4), 0, textures.Length - 1);
					Painting.Channels texChannels = painter.canvas_Format == Painting.Format.Multi ? (Painting.Channels)(ch % 4 + 2) : Painting.Channels.RGBA;
					for (int i = 0; i < channelTarget.targets.Count; i++)
						AssignToNode(textures[texIndex], texChannels, channelTarget.targets[i]);
				}
			}

			if ((!painter.isPainting || Settings.autoGenerate) && TC2Ready)
			{ // Generate changed rect
				Rect generateRect = FinalizeGenerateRect();
				if (generateRect.size.x > 0 && generateRect.size.y > 0)
					TC.AutoGenerate(generateRect);
			}
		}

		/// <summary>
		/// Updates all node targets to keep them in sync with the painter object.
		/// This includes setup such as input image and transformation.
		/// </summary>
		public void UpdateNodeTargets()
		{
			if (TCNode != null)
				UpdateNodeTarget(TCNode);
			for (int ch = 0; ch < painter.canvasChannelCount && ch < painter.canvasChannels.Count; ch++)
			{ // Update all active node targets
				CanvasChannel channelTarget = painter.canvasChannels[ch];
				for (int i = 0; i < channelTarget.targets.Count; i++)
					UpdateNodeTarget(channelTarget.targets[i]);
			}
			for (int ch = painter.canvasChannelCount; ch < painter.canvasChannels.Count; ch++)
			{ // Clear all non-active node targets
				CanvasChannel channelTarget = painter.canvasChannels[ch];
				for (int i = 0; i < channelTarget.targets.Count; i++)
					ClearNode(channelTarget.targets[i]);
			}
		}

		/// <summary>
		/// Updates node target to keep it in sync with the painter object.
		/// This includes setup such as input image and transformation.
		/// </summary>
		private void UpdateNodeTarget(TC_Node node)
		{
			if (node == null)
				return;
			if (node.inputKind != InputKind.File || node.inputFile != InputFile.Image)
			{ // Make sure it's an image node
				node.inputKind = InputKind.File;
				node.inputFile = InputFile.Image;
				node.Init();
				TC.RefreshOutputReferences(node.outputId, true);
				//TC.RefreshOutputReferences(node.outputId);
				//node.Regenerate();
			}

			if (TCNode != null) // Sync size between all nodes
				node.size = TCNode.size;

			if (node.transform != transform)
			{ // Sync transform with painter transform
				node.transform.position = transform.position;
				node.transform.localScale = Vector3.one;
				node.transform.localScale = new Vector3(transform.lossyScale.x / node.transform.lossyScale.x, transform.lossyScale.y / node.transform.lossyScale.y, transform.lossyScale.z / node.transform.lossyScale.z);
				node.transform.rotation = transform.rotation;
			}
		}

		/// <summary>
		/// Assigns the texture to the node, forcing it as an image input
		/// </summary>
		private void AssignToNode(Texture tex, TC_Node node)
		{
			AssignToNode(tex, Painting.Channels.RGBA, node);
		}

		/// <summary>
		/// Assigns the texture to the node, forcing it as an image input with the specified channels
		/// </summary>
		private void AssignToNode(Texture tex, Painting.Channels channels, TC_Node node)
		{
			if (node == null)
				return;

			node.stampTex = tex == null ? Texture2D.blackTexture : tex;
			if (tex != null && channels != Painting.Channels.RGBA)
			{ // Enable only certain channels from mask
				if (node.imageSettings == null)
					node.imageSettings = new ImageSettings();
				int active = (int)channels - 2;
				for (int i = 0; i < node.imageSettings.colChannels.Length; i++)
					node.imageSettings.colChannels[i].active = i == active;
			}
			node.active = node.enabled = node.stampTex != null;
		}

		/// <summary>
		/// Clears this node
		/// </summary>
		private void ClearNode(TC_Node node)
		{
			if (node == null)
				return;
			
			node.stampTex = Texture2D.blackTexture;
			node.active = node.enabled = node.stampTex != null;
		}

		#endregion

		#region Paint Area

		/// <summary>
		/// Updates the intermediate paint area with the current brush position
		/// </summary>
		public void UpdatePaintArea()
		{
			if (!Settings.enablePartialGeneration)
				return;

			// Transform canvas-space brush pos into world-space
			Vector3 worldBrushPos = new Vector3((painter.state_BrushPos.x - 0.5f) * canvasTargetSize.x, 0, (painter.state_BrushPos.y - 0.5f) * canvasTargetSize.z);
			worldBrushPos = (Vector3)(transform.localToWorldMatrix * worldBrushPos) + transform.position;
			// Convert world-space brush pos into area-space
			Vector2 brushPos = new Vector2((worldBrushPos.x - areaPos.x) / areaSize.x + 0.5f, (worldBrushPos.z - areaPos.z) / areaSize.z + 0.5f);

			if (!painter.isPainting)
			{ // This usually does not happen.
				paintArea = new Rect(0, 0, 1, 1);
			}
			else if ((paintArea.position == Vector2.zero && paintArea.size == Vector2.zero) || !buildingPaintArea)
			{ // Start a new paint area with brush pos
				paintArea = new Rect(brushPos, Vector2.zero);
				paintBrushSize = 0;
			}
			else
			{ // Include brush pos into an existing paint area
				paintArea.xMin = Mathf.Min(paintArea.xMin, brushPos.x);
				paintArea.yMin = Mathf.Min(paintArea.yMin, brushPos.y);
				paintArea.xMax = Mathf.Max(paintArea.xMax, brushPos.x);
				paintArea.yMax = Mathf.Max(paintArea.yMax, brushPos.y);
			}
			// Choose max brush size to use as a border around this point rect later on
			paintBrushSize = Mathf.Max(paintBrushSize, painter.paint_Brush.size);
			buildingPaintArea = true;
		}

		/// <summary>
		/// Finalizes and returns the generate rect changed by past paint strokes for TC2 interaction
		/// </summary>
		private Rect FinalizeGenerateRect()
		{
			buildingPaintArea = false;
            if (!Settings.enablePartialGeneration || !painter.isPainting)
                return rect_Area;
			return paintStrokeArea;
		}

        #endregion

        #region Terrain Interaction

		/// <summary>
		/// Updates all registered terrains by fetching them from the current terrain area, if existant, else searches all terrains
		/// </summary>
		public void UpdateTerrains()
		{
			if (TCTerrainArea != null && TCTerrainArea.terrains != null)
			{
				terrainColliders = TCTerrainArea.terrains
					.Where((TCUnityTerrain TCT) => TCT.terrain != null)
					.Select((TCUnityTerrain TCT) => TCT.terrain.GetComponent<TerrainCollider>())
					.ToArray();
			}
			else
				terrainColliders = FindObjectsOfType<TerrainCollider>();
		}

		/// <summary>
		/// Calculates brush position of the mouse on the worldspace terrains according in the node space and returns whether it has hit a terrain at all
		/// </summary>
		public bool CalcBrushWorldPos(out Vector2 brushPos, out Vector3 worldPos)
		{
			brushPos = Vector2.zero;
			worldPos = Vector3.zero;
			if (Camera.current == null)
				return false; // Not in the scene GUI

			if (terrainColliders == null || terrainColliders.Length == 0)
				UpdateTerrains(); // Refetch terrains from scene

			// Prepare mouse raycast
			Vector2 mousePos = Event.current.mousePosition;
			#if UNITY_EDITOR && UNITY_2019_3_OR_NEWER
			mousePos.y = mousePos.y + 42f;
			mousePos *= UnityEditor.EditorGUIUtility.pixelsPerPoint;
			mousePos.y = (Screen.height - mousePos.y);
			#else
			mousePos.y = (Screen.height - mousePos.y) - 37.5f;
			#endif
			Ray mouseRay = Camera.current.ScreenPointToRay(mousePos);
			RaycastHit hit;

			foreach (TerrainCollider terrainCol in terrainColliders)
			{ // Iterate over each terrain and test mouse ray
				if (terrainCol.Raycast(mouseRay, out hit, float.PositiveInfinity))
				{ // Hit this terrain: Convert world-space brush pos to canvas-space
					worldPos = hit.point;
					Vector3 localPos = transform.worldToLocalMatrix * (worldPos - transform.position);
					brushPos.x = localPos.x / canvasTargetSize.x + 0.5f;
					brushPos.y = localPos.z / canvasTargetSize.z + 0.5f;
					return true;
				}
			}
			return false;
		}

        #endregion
	}
}