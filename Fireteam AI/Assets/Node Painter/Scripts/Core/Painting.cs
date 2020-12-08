using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;

using TerrainComposer2.NodePainter.Utilities;

namespace TerrainComposer2.NodePainter
{

	[System.Serializable]
	public partial class CanvasChannel
	{ // Struct defining a canvas channel
		public Texture displayTexture = null;

		public CanvasChannel()
		{
		}
		public CanvasChannel(Texture displayTex)
		{
			displayTexture = displayTex;
		}
	}

	[System.Serializable]
	public class Painting
	{
		// Unique ID (serialized)
		[SerializeField] private int uniqueID;
		public int ID { get { return uniqueID; } }

		// VARIABLES

		#region Canvas

		// Default Canvas Formats (non-serialized)
		public const RenderTextureFormat RTFormat = RenderTextureFormat.ARGBHalf; // RenderTextureFormat.ARGBHalf; // RenderTextureFormat.ARGBFloat
		public const TextureFormat TexFormat = TextureFormat.RGBAHalf; // TextureFormat.RGBAHalf; // TextureFormat.RGBAFloat
		public const RawImportExport.BitDepth bitDepth = RawImportExport.BitDepth.Bit16; // RawImportExport.BitDepth.Bit16; // RawImportExport.BitDepth.Bit32

		// Canvas (non-serialized)
		public bool canvas_Exists { get { return tex_canvas != null && tex_canvas.Count > 0 && !tex_canvas.Any((RenderTexture RT) => RT == null); } }
		private string canvas_name;
		public string canvas_Name { get { return canvas_name; } set { if (canvas_name != value) { canvas_name = value; AssignCanvasNames(); } } }
		public Format canvas_Format { get; private set; }
		public int canvas_SizeX { get; private set; }
		public int canvas_SizeY { get; private set; }
		public float canvas_Aspect { get { return ((float)canvas_SizeY) / canvas_SizeX; } }
		public float canvas_Rotation { get; set; }

		// Canvas Channels (serialized)
		[UnityEngine.Serialization.FormerlySerializedAs("canvasChannelDefinitions")] public List<CanvasChannel> canvasChannels = new List<CanvasChannel>();
		[SerializeField] private int _canvasChannelCount = 1;
		public int canvasChannelCount { get { return canvas_Exists ? (canvas_Format == Format.Multi ? _canvasChannelCount : 1) : 0; } set { if (_canvasChannelCount != value) { _canvasChannelCount = value; MatchCanvasChannelCount(canvasChannelCount); } } }
		public int canvasTextureCount { get { return canvas_Exists ? tex_canvas.Count : 0; } }

		#endregion

		#region Painting

		// Paint Material (non-serialized)
		private Material _paintMat;
		private Material PaintMat { get { if (_paintMat == null) _paintMat = new Material(Shader.Find("Hidden/NodePainter_RTPaint")); return _paintMat; } }
		// Channel Mix Material
		private Material _channelMat;
		private Material ChannelMat { get { if (_channelMat == null) _channelMat = new Material(Shader.Find("Hidden/NP_TextureChannelMixer")); return _channelMat; } }

		// Seperate Paint Pass (non-serialized)
		public bool sepPass_Force { get; set; }
		public bool sepPass_Support { get { return paint_Brush.mode != PaintMode.Smoothen && paint_Brush.mode != PaintMode.Contrast && paint_Brush.mode != PaintMode.Replace; } }
		public bool sepPass_Need { get { return (canvas_Format == Format.Multi && paint_normChannels) || sepPass_Force; } }
		public bool sepPass_Enable { get { return sepPass_Need && sepPass_Support; } }

		// Paint State (non-serialized)
		public bool isPainting { get; private set; }
		public bool isOnCanvas { get; set; }
		public string state_UserID { get; private set; }
		public Vector2 state_BrushPos { get; set; }
		public bool state_BlockPainting { get; set; }
		public bool state_InvertBrush { get; set; }
		public int state_BrushFunc { get; private set; }

		// Temp
		private int temp_brushType = -1;
		[System.NonSerialized] public PaintMode temp_prevPaintMode = (PaintMode)(-1);
		private float state_lastPaintTime;
		private float state_lastGenerationTime;

		// Painting Callbacks (non-serialized)
		public delegate void SetCanvasFunc(params RenderTexture[] textures);
		public SetCanvasFunc OnAssignCanvas;
		public Action OnPainting;
		public Action<float, float> OnScaleCanvas;

		// Visualization
		[SerializeField] private CanvasVizState _canvasVizState = CanvasVizState.All;
		public CanvasVizState canvasVizState { get { return _canvasVizState; } set { if (_canvasVizState != value) { _canvasVizState = value; PushContent(); } } }
/*#if UNITY_5_4_OR_NEWER
		private Texture2DArray cachedMultiTexVizArray; // cached TextureArray containing all channels to visualize multi-canvases faster
#endif*/
		private static string[] channelVizColorsHex = new string[16] {
			"#ff0000", "#00FF11", "#0011FF", "#ffff00",
			"#e3683f", "#63d94c", "#5555ff", "#d7ff2d",
			"#a62994", "#92a700", "#2578a4", "#ffd231",
			"#eb76c9", "#2bb37a", "#00ddff", "#ccc829" };
		[System.NonSerialized] public Color[] channelVizColors;
		private const int shaderManualTextureCount = 4; // How many textures the shader handles manually before switching to TextureArrays

		#endregion

		#region Undo

		// Undo (non-serialized)
		private List<UndoRecord> canvasUndoList = new List<UndoRecord>();
		private List<UndoRecord> canvasRedoList = new List<UndoRecord>();

		public bool canPerformUndo { get { return Settings.enableUndo && canvasUndoList != null && canvasUndoList.Count > 1; } }
		public bool canPerformRedo { get { return Settings.enableUndo && canvasRedoList != null && canvasRedoList.Count > 0; } }

		public string getNextUndoName { get { return canvasUndoList.Count > 1 ? canvasUndoList[canvasUndoList.Count - 1].message : "None"; } }
		public string getNextRedoName { get { return canvasRedoList.Count > 0 ? canvasRedoList[canvasRedoList.Count - 1].message : "None"; } }

		#endregion

		#region Cache

        [NonSerialized] private bool isDirty = false;

		// Cache Information (serialized)
		public TextAsset cache_Asset = null;
		private bool cache_NoAsset = false;
		public bool cache_Canvas;
		public string cache_Path;
		public int cache_SizeX, cache_SizeY;
		public Format cache_Format;
		public int cache_ChannelCnt;

		#endregion

		#region Internal Definitions

		// Canvas containing intermediate, unsaved modifications used for visualization
		public RenderTexture tex_VizCanvas;

		// Internal canvas (stack) with saved content
		public RenderTexture tex_Current { get { return tex_canvas != null && curTexIndex >= 0 && tex_canvas.Count > curTexIndex ? tex_canvas[curTexIndex] : null; } }
		private List<RenderTexture> tex_canvas = new List<RenderTexture>();
		private List<RenderTexture> tex_tempCanvas;

		// Working passes - endlessly switching to provide canvases for many processing passes
		private RenderTexture tex_tempPass1, tex_tempPass2;
		private bool tex_tempSwitch = false;
		private RenderTexture tex_TempWrite { get { return tex_tempSwitch ? tex_tempPass2 : tex_tempPass1; } }
		private RenderTexture tex_TempRead { get { return tex_tempSwitch ? tex_tempPass1 : tex_tempPass2; } }

		// Intermediate result used before modifications are applied to canvas when additional steps are required
		private RenderTexture tex_tempResult;
		public RenderTexture tex_CurRawPaint { get; private set; }

		#endregion

		#region User Options

		// Painting Options
		public Color paint_Color = Color.white;
		public float paint_Intensity = 1;
		public Brush paint_Brush = new Brush { mode = PaintMode.Add, type = 0, size = 0.05f, intensity = 0.2f };
		public float paint_BrushRotation = 0;
		public float paint_SmoothenBias = 2;
		public float paint_TargetValue = 1;
		public bool paint_Clamp = true;
		public bool paint_normChannels = true;

		// Channel Selection
		private int _curChannelIndex = 0;
		public int curChannelIndex { get { return _curChannelIndex; } set { if (canvas_Exists && _curChannelIndex != value) { _curChannelIndex = canvas_Format == Format.Multi ? value : 0; PushContent(); } } }
		public int curTexIndex { get { return Mathf.FloorToInt((float)curChannelIndex / 4); } }

		// Modifications
		public Modifications mods = new Modifications(true);
		public bool applyingOngoingMods { get { return canvas_Format != Format.Multi && (mods.brightness != 0 || mods.contrast != 1 || mods.tintColor != Color.white || mods.advancedChannelMods); } }

		// Misc Options
		public bool visualizeCanvas;

		#endregion

		// MEMBERS

		#region Structure

		[System.Serializable]
		public struct Brush
		{ // Struct defining raw brush
			public PaintMode mode;
			public int type;
			public float size;
			public float intensity;
			public float falloff;
			public float hardness;
		}

		[System.Serializable]
		private class UndoRecord
		{ // Record for undo purposes
			public string name;
			public string message;
			// Canvas Format
			public int sizeX, sizeY;
			public Format format;
			public RenderTextureFormat texFormat;
			// Canvas content
			public List<CanvasChannel> channels;
			public int channelCount;
			public List<Texture2D> tex;
			// Canvas mods
			public bool writeMods;
			public Modifications mods;
		}

		[System.Serializable]
		public struct Modifications
		{ // Struct describing canvas modifications
			public float brightness;
			public float contrast;
			public Color tintColor;
			public bool advancedChannelMods;
			public ChannelMod chR, chG, chB, chA;

			public Modifications(bool initialize)
			{
				brightness = 0;
				contrast = 1;
				tintColor = Color.white;
				advancedChannelMods = false;
				chR = new ChannelMod(0);
				chG = new ChannelMod(1);
				chB = new ChannelMod(2);
				chA = new ChannelMod(3);
			}
		}

		[System.Serializable]
		public struct ChannelMod
		{ // Struct describing individual channel modifications
			public int channel;
			public float offset, scale;
			public int shuffle;
			public bool invert;

			public ChannelMod(int ch)
			{
				channel = ch;
				offset = 0;
				scale = 1;
				shuffle = ch;
				invert = false;
			}
		}

		#endregion

		#region Enums

		public enum Format { Color, Value, Multi }
		public enum CanvasVizState { None, Current, All }

		public enum Channels { RGBA, RGB, R, G, B, A, Grayscale }
		public enum ChannelValue { R, G, B, A, black, white }

		public enum BlendMode { Add, Substract, Multiply, Divide, Lerp, Overlay, Replace, Smoothen, Contrast }
		public enum PaintMode { Add = 0, Lerp = 4, Replace = 6, Smoothen = 7, Contrast = 8 }

		#endregion

		// METHODS

		#region General Methods

		public Painting()
		{ // Creation
			if (uniqueID == 0)
				RecreateID();
		}

		/// <summary>
		/// Recreate the identity of this painter
		/// </summary>
		public void RecreateID()
		{
			uniqueID = System.Math.Abs(GetHashCode());
		}

		/// <summary>
		/// Open this painter, load the last session and prepare for painting
		/// </summary>
		public void Open()
		{
			AssureDefaultColors();
			if (canvasUndoList == null || canvasUndoList.Count <= 0)
				RegisterCanvasUndo("INITIAL", false, false);
			if (!canvas_Exists && !canPerformUndo)
				LoadLastSession();
			AssureRTs();
		}

		/// <summary>
		/// Hide this painter, exit painting
		/// </summary>
		public void Hide()
		{
			if (isPainting)
				EndPainting();
			ReleaseRTs(false, false);
		}

		/// <summary>
		/// Close this painter, save session
		/// </summary>
		public void Close(bool saveSession = false)
		{
			Hide();
			if (saveSession)
				SaveCurrentSession();
		}

		/// <summary>
		/// Assures default channel colors are parsed from HEX format
		/// </summary>
		private void AssureDefaultColors()
		{
			if (channelVizColors != null)
				return;
			channelVizColors = new Color[channelVizColorsHex.Length];
			for (int hCnt = 0; hCnt < channelVizColorsHex.Length; hCnt++)
			{
				Color hexCol;
#if UNITY_5_2 || UNITY_5_3 || UNITY_5_3_OR_NEWER
				if (ColorUtility.TryParseHtmlString (channelVizColorsHex[hCnt], out hexCol))
#else
				if (Color.TryParseHexString(channelVizColorsHex[hCnt], out hexCol))
#endif
					channelVizColors[hCnt] = hexCol;
				else
					Debug.LogError(channelVizColorsHex[hCnt] + " could not be parsed to a color!");
			}
		}

		#endregion


		#region Internal Handling

		/// <summary>
		/// Validates the current canvas settings and internal textures
		/// </summary>
		private bool AssureRTs()
		{
			if (!canvas_Exists)
			{ // No internal canvas -> nothing assigned
				ReleaseRTs(true, true);
				return false;
			}
			if (canvas_SizeX <= 0 || canvas_SizeY <= 0)
				canvas_SizeX = canvas_SizeY = 1024;

			// Check Multi format
			if (canvas_Format == Format.Multi && Mathf.CeilToInt((float)canvasChannelCount / 4) != tex_canvas.Count)
				MatchCanvasChannelCount();
			curChannelIndex = Mathf.Clamp(curChannelIndex, 0, canvasChannelCount - 1);

			// Check for Temp RTs
			if (tex_VizCanvas == null || tex_tempPass1 == null || tex_tempPass2 == null || tex_tempResult == null)
				CreateTempRTs();
			return true;
		}

		/// <summary>
		/// Recreates all temporal RTs when the canvas exists
		/// </summary>
		private void CreateTempRTs()
		{
			if (!canvas_Exists)
				return;

			// Keep Canvas Viz
            RenderTexture prevRT = RenderTexture.active;
			RenderTexture viz = RequestNewRT(canvas_Name + ":VizCanvas");
			if (tex_VizCanvas != null)
				Graphics.Blit(tex_VizCanvas, viz);
            RenderTexture.active = prevRT;

			// Make sure previous ones are released
			ReleaseRTs(true, false);

			// Create RTs
			tex_tempPass1 = RequestNewRT(canvas_Name + ":Pass1");
			tex_tempPass2 = RequestNewRT(canvas_Name + ":Pass2");
			tex_tempResult = RequestNewRT(canvas_Name + ":TempResult");
			tex_VizCanvas = viz;

			// Setup working RT mipmapping
			tex_tempPass1.useMipMap = true;
			tex_tempPass1.filterMode = FilterMode.Trilinear;
			tex_tempPass2.useMipMap = true;
			tex_tempPass2.filterMode = FilterMode.Trilinear;
		}

		/// <summary>
		/// Request a new canvas-sized render texture
		/// </summary>
		private RenderTexture RequestNewRT()
		{
			return RequestNewRT(canvas_Name + ":RT");
		}

		/// <summary>
		/// Request a new canvas-sized render texture with the given name
		/// </summary>
		private RenderTexture RequestNewRT(string name)
		{
			RenderTexture rt = new RenderTexture(canvas_SizeX, canvas_SizeY, 0, RTFormat, RenderTextureReadWrite.Linear);
			rt.name = name;
			return rt;
		}

		/// <summary>
		/// Releases the all temporary RenderTextures, and optionally the visualization- and internal canvas RTs aswell
		/// </summary>
		private void ReleaseRTs(bool releaseViz, bool releaseCanvas)
		{
			if (releaseCanvas && tex_canvas != null)
			{ // Release internal RTs which store actual canvas
				for (int RTi = 0; RTi < tex_canvas.Count; RTi++)
					ReleaseRT(tex_canvas[RTi]);
				if (tex_canvas != null)
					tex_canvas.Clear();
				tex_canvas = null;
			}

			// Release temporary RTs

			ReleaseRT(tex_tempPass1);
			ReleaseRT(tex_tempPass2);
			ReleaseRT(tex_tempResult);
			tex_tempPass1 = tex_tempPass2 = tex_tempResult = null;

			if (releaseViz)
			{
				ReleaseRT(tex_VizCanvas);
				tex_VizCanvas = null;
			}

			if (tex_tempCanvas != null)
			{
				for (int RTi = 0; RTi < tex_tempCanvas.Count; RTi++)
					ReleaseRT(tex_tempCanvas[RTi]);
				tex_tempCanvas = null;
			}

/*#if UNITY_5_4_OR_NEWER
			cachedMultiTexVizArray = null;
#endif*/
		}

		/// <summary>
		/// Releases the given RenderTexture
		/// </summary>
		private void ReleaseRT(RenderTexture RT)
		{
			if (RT != null)
			{
#if UNITY_EDITOR
				if (UnityEditor.AssetDatabase.Contains(RT))
					return;
#endif
				if (RenderTexture.active == RT)
					RenderTexture.active = null;
				RT.Release();
				GlobalPainting.DestroyObj(RT);
			}
		}

		/*/// <summary>
		/// Register a change of internal canvas texture contents
		/// </summary>
		private void CanvasChange()
		{
#if UNITY_5_4_OR_NEWER
			cachedMultiTexVizArray = null;
#endif
		}*/


		/// <summary>
		/// Assignes the current, unmodified canvas
		/// </summary>
		private void AssignCanvas()
		{
			AssignAllCanvas(tex_canvas.ToArray());
		}
		/// <summary>
		/// Assigns the current canvas with the specified, current modified texture
		/// </summary>
		private void AssignModCanvas(RenderTexture modTex)
		{
			if (canvas_Format == Format.Multi)
			{
				RenderTexture[] textures = tex_canvas.ToArray();
				textures[curTexIndex] = modTex;
				AssignAllCanvas(textures);
			}
			else
				AssignAllCanvas(modTex);
		}
		/// <summary>
		/// Assigns the specified set of canvas textures
		/// </summary>
		private void AssignAllCanvas(params RenderTexture[] tex)
		{
			if (OnAssignCanvas != null && (!canvas_Exists || tex.Length == tex_canvas.Count))
				OnAssignCanvas(tex);
		}

		/// <summary>
		/// Updates the internal texture names to fit structure
		/// </summary>
		private void AssignCanvasNames()
		{
			if (canvas_Exists)
			{
				if (tex_canvas.Count > 1)
				{ // Numerate textures
					for (int i = 0; i < tex_canvas.Count; i++)
						tex_canvas[i].name = canvas_Name + ":RT(" + i + ")";
				}
				else
					tex_canvas[0].name = canvas_Name + ":RT";
				// Name temp textures
				if (tex_tempPass1 != null) tex_tempPass1.name = canvas_Name + ":Pass1";
				if (tex_tempPass2 != null) tex_tempPass2.name = canvas_Name + ":Pass2";
				if (tex_tempResult != null) tex_tempResult.name = canvas_Name + ":TempResult";
				if (tex_VizCanvas != null) tex_VizCanvas.name = canvas_Name + ":VizCanvas";
			}
		}

		/// <summary>
		/// Match the internal channel definitions with the channel count
		/// </summary>
		private void MatchCanvasChannelCount()
		{
			if (tex_canvas == null || tex_canvas.Count <= 0)
				return;
			if (tex_canvas == null)
				MatchCanvasChannelCount(0);
			else
			{
				if (canvas_Format != Format.Multi)
					MatchCanvasChannelCount(1);
				else if (tex_canvas == null)
					MatchCanvasChannelCount(canvasChannelCount);
				else
					MatchCanvasChannelCount(Mathf.Max(canvasChannelCount, (tex_canvas.Count - 1) * 4));
			}
		}

		/// <summary>
		/// Match the internal channel definitions and count with the new channel count
		/// </summary>
		private void MatchCanvasChannelCount(int chCount)
		{
			if (canvas_Format != Format.Multi)
				chCount = 1;
			if (chCount > 16)
				chCount = 16;
			if (chCount < 0)
				chCount = 0;

			// Match RTs
			int RTCount = Mathf.CeilToInt((float)chCount / 4);
			bool changedRTs = MatchListLength(ref tex_canvas, RTCount, RequestNewRT, ReleaseRT);
			// Match Channels
			_canvasChannelCount = chCount;
			MatchListLength(ref canvasChannels, chCount, () => new CanvasChannel(), null, false);
			// Reassign names if necessary
			if (changedRTs)
				AssignCanvasNames();
		}

		/// <summary>
		/// Match list to new length with designated create/remove functions
		/// </summary>
		private bool MatchListLength<T>(ref List<T> list, int count, System.Func<T> CreateElement, System.Action<T> RemoveElement, bool removeElements = true)
		{
			if (list == null)
				list = new List<T>(count);
			int diff = count - list.Count;
			for (int i = 0; i < diff; i++)
				list.Add(CreateElement != null ? CreateElement() : default(T));
			if (removeElements)
			{
				if (RemoveElement != null)
					for (int i = count; i < list.Count; i++)
						RemoveElement(list[i]);
				list.RemoveRange(count, Mathf.Max(0, -diff));
			}
			return diff != 0;
		}

		/// <summary>
		/// Fills the specified RT with the new color
		/// </summary>
		private void Fill(RenderTexture RT, Color fillColor)
		{
			RenderTexture prevRT = RenderTexture.active;
			RenderTexture.active = RT;
			GL.Clear(true, true, fillColor);
			RenderTexture.active = prevRT;
		}

		/// <summary>
		/// Reads the specified RT into a texture
		/// </summary>
		public Texture2D RTtoTexture(RenderTexture RT)
		{
			RenderTexture prevRT = RenderTexture.active;
			RenderTexture.active = RT;
			Texture2D tex = new Texture2D(RT.width, RT.height, TexFormat, false, true);
			tex.ReadPixels(new Rect(0, 0, RT.width, RT.height), 0, 0);
			tex.Apply();
			RenderTexture.active = prevRT;
			return tex;
		}

		#endregion


		#region Public Canvas API

		/// <summary>
		/// Deletes (Unloads) the current canvas
		/// </summary>
		public void DeleteCanvas()
		{
			string prevName = canvas_Name;
			ReleaseRTs(true, true);
			cache_Canvas = false;
			RegisterCanvasUndo("Deleted " + prevName);
		}

		/// <summary>
		/// Creates a new canvas with specified dimensions in the specified format (channelCount for Multi-format)
		/// </summary>
		public void NewCanvas(int width, int height, Format format, string name, int channelCount = 1)
		{
			if (width <= 8 || height <= 8 || width > 8192 || height > 8192)
				throw new System.ArgumentException("Could not create canvas: Invalid canvas dimensions " + width + "x" + height + "!");
			channelCount = Math.Max(channelCount, 1);
			if (format != Format.Multi && channelCount != 1)
				throw new System.ArgumentException("Could not create canvas: To use multiple channels, you must use the Multi-format!");
			if (canvas_Exists)
				DeleteCanvas();

			// Set canvas information
			canvas_SizeX = width;
			canvas_SizeY = height;
			canvas_Format = format;
			canvas_Name = name;

			// Create canvas according to these informations
			MatchCanvasChannelCount(channelCount);
			AssignCanvasNames();
			CreateTempRTs();
			PushContent();
			RegisterCanvasUndo("Created " + canvas_Name);
		}
		
		/// <summary>
		/// Imports the specified texture into the canvas (Does not work with Multi-Format).
		/// If adaptSpecs is true, canvas is recreated if specs differ, else only the content is imported.
		/// Format is always kept.
		/// </summary>
		public void ImportTexture(Texture2D tex, bool adaptSpecs)
		{
			if (canvas_Format == Format.Multi)
				throw new UnityException("Cannot import single texture into Multi-Format. Specify channel instead!");
			ImportTexture(new List<Texture2D>() { tex }, 1, adaptSpecs, canvas_Format);
		}

		/// <summary>
		/// Imports the specified textures into the canvas (Requires Multi-Format).
		/// If adaptSpecs is true, canvas is recreated if specs differ, else only the content is imported
		/// Format is always kept.
		/// </summary>
		public void ImportTexture(List<Texture2D> tex, int channelCount, bool adaptSpecs)
		{
			if (canvas_Format != Format.Multi && tex.Count > 1)
				throw new System.ArgumentException("Can't import multiple textures into non-Multi-Format!");
			ImportTexture(tex, channelCount, adaptSpecs, canvas_Format);
		}

		/// <summary>
		/// Imports the specified textures into the canvas and forces the specified format.
		/// If adaptSpecs is true or format differs from the current format, canvas is recreated in the specified format.
		/// </summary>
		public void ImportTexture(List<Texture2D> tex, int channelCount, bool adaptSpecs, Format format)
		{
			if (format != Format.Multi && tex.Count > 1)
				throw new System.ArgumentException("Can't import multiple textures into non-Multi-Format!");
			if (tex == null || tex.Count <= 0 || tex.Any((Texture2D t) => t == null))
				throw new System.ArgumentException("Trying to import null canvas!");
			if (format != canvas_Format)
				adaptSpecs = true;

			// Validate tex and channel count
			if (format != Format.Multi)
				channelCount = 1;
			else if (Mathf.CeilToInt((float)channelCount / 4) != tex.Count)
			{
				Debug.LogWarning("Trying to import " + tex.Count + " textures with " + channelCount + " channels! Correcting!");
				channelCount = tex.Count * 4;
			}

			if (adaptSpecs)
			{ // New textures with source specs
				canvas_SizeX = tex[0].width;
				canvas_SizeY = tex[0].height;
			}

			// Prepare textures and copy content
			canvas_Format = format;
            RenderTexture prevRT = RenderTexture.active;
			canvas_Name = tex[0].name;
			MatchCanvasChannelCount(channelCount);
			for (int i = 0; i < tex.Count; i++)
				Graphics.Blit(tex[i], tex_canvas[i]);
            RenderTexture.active = prevRT;

			// Apply changes
			if (adaptSpecs)
				CreateTempRTs();
			PushContent();
			RegisterCanvasUndo((adaptSpecs ? "Loaded " : "Imported ") + canvas_Name + "");
		}

		/// <summary>
		/// Resize the canvas and it's contents to the new size
		/// </summary>
		public void Resize(int newWidth, int newHeight)
		{
			if (!canvas_Exists || (newWidth == canvas_SizeX && newHeight == canvas_SizeY))
				return;
			canvas_SizeX = newWidth;
			canvas_SizeY = newHeight;

			// Render textures into higher resolution
			List<RenderTexture> renderTextures = new List<RenderTexture>(tex_canvas.Count);
			for (int i = 0; i < tex_canvas.Count; i++)
			{
				RenderTexture newRT = new RenderTexture(canvas_SizeX, canvas_SizeY, 0, RTFormat);
				Graphics.Blit(tex_canvas[i], newRT);
				renderTextures.Add(newRT);
			}

			// Update to new textures
			ReleaseRTs(true, true);
			tex_canvas = renderTextures;
			AssignCanvasNames();
			CreateTempRTs();
			PushContent();
			RegisterCanvasUndo("Resized: (" + canvas_SizeX + ", " + canvas_SizeY + ")");
		}

		/// <summary>
		/// Expands the canvas while keeping the canvas content the same
		/// When new size is larger, it creates empty space around the canvas, else it cuts the content
		/// </summary>
		public void Expand(int newWidth, int newHeight)
		{
			if (!canvas_Exists || (newWidth == canvas_SizeX && newHeight == canvas_SizeY))
				return;

			// Setup resize material
			PaintMat.SetVector("sourceRect", new Vector4(0, 0, 1, 1));
			float ratioX = (float)canvas_SizeX / newWidth, ratioY = (float)canvas_SizeY / newHeight;
			PaintMat.SetVector("targetRect", new Vector4(0.5f - ratioX / 2, 0.5f - ratioY / 2, ratioX, ratioY));

			canvas_SizeX = newWidth;
			canvas_SizeY = newHeight;

			// Render textures into higher resolution
			List<RenderTexture> renderTextures = new List<RenderTexture>(tex_canvas.Count);
			for (int i = 0; i < tex_canvas.Count; i++)
			{
				RenderTexture newRT = new RenderTexture(canvas_SizeX, canvas_SizeY, 0, RTFormat);
				PaintMat.SetTexture("_Canvas", tex_canvas[i]);
				RenderCurrentSetup(6, newRT);
				renderTextures.Add(newRT);
			}

			// Update to new textures
			ReleaseRTs(true, true);
			tex_canvas = renderTextures;
			AssignCanvasNames();
			CreateTempRTs();
			PushContent();
			RegisterCanvasUndo("Expanded: (" + canvas_SizeX + ", " + canvas_SizeY + ")");

			// Call to scale canvas parent
			OnScaleCanvas(1 / ratioX, 1 / ratioY);
			//TODO: Scale Undo when expanding canvas
		}

		/// <summary>
		/// Fills each texture in the canvas with the specified color
		/// </summary>
		public void Fill(Color fillColor)
		{
			if (!canvas_Exists)
				return;
			for (int i = 0; i < tex_canvas.Count; i++)
				Fill(tex_canvas[i], fillColor);
			PushContent();
			RegisterCanvasUndo("Filled canvas");
		}

		#endregion

		#region Public Sampling API

        /// <summary>
        /// Issues a request to push and assign the canvas content
        /// </summary>
        public void RequestContent()
        {
            if (canvas_Exists)
                PushContent(null, null);
        }
		
		/// <summary>
		/// Samples the canvas at the specified position in local 0-1 space and returns the color at that position.
		/// Fetches texture from GPU, will stall GPU! Don't call every frame!
		/// </summary>
		public Color Sample(Vector2 pos)
		{
			if (!AssureRTs())
				return Color.clear;

			pos.x = Mathf.Clamp01(pos.x) * canvas_SizeX;
			pos.y = (1 - Mathf.Clamp01(pos.y)) * canvas_SizeY;

			// Fetch texture pixel from GPU
			RenderTexture prevRT = RenderTexture.active;
			RenderTexture.active = tex_VizCanvas;
			Texture2D tex = new Texture2D(1, 1, TexFormat, false);
			tex.ReadPixels(new Rect(pos, Vector2.one), 0, 0, false);
			RenderTexture.active = prevRT;

			// Read texture pixel
			Color col = tex.GetPixel(0, 0);
			GlobalPainting.DestroyObj(tex);
			return col;
		}

		/// <summary>
		/// Samples the canvas in the specified rectangle in local 0-1 space and returns the colors in this rectangle.
		/// Fetches texture from GPU, will stall GPU! Don't call every frame!
		/// </summary>
		public Color[] Sample(Rect rect)
		{
			if (!AssureRTs())
				return new Color[] { Color.clear };

			rect.xMin = Mathf.Clamp01(rect.xMin) * canvas_SizeX;
			rect.xMax = Mathf.Clamp01(rect.xMax) * canvas_SizeX;
			rect.yMin = (1 - Mathf.Clamp01(rect.yMin)) * canvas_SizeY;
			rect.yMax = (1 - Mathf.Clamp01(rect.yMax)) * canvas_SizeY;

			if (rect.width <= 0 || rect.height <= 0)
				return new Color[] { Color.clear };

			// Fetch texture area from GPU
			RenderTexture prevRT = RenderTexture.active;
			RenderTexture.active = tex_VizCanvas;
			Texture2D tex = new Texture2D((int)Mathf.Max(1, rect.width), (int)Mathf.Max(1, rect.height), TexFormat, false);
			tex.ReadPixels(rect, 0, 0, false);
			RenderTexture.active = prevRT;
			return tex.GetPixels();
		}

		

		// SNAPSHOT

		/// <summary>
		/// Gets a snapshot snapshot from the canvas
		/// </summary>
		public Texture2D getSnapshot()
		{
			return getSnapshot(curTexIndex);
		}

		/// <summary>
		/// Gets a snapshot snapshot from the canvas
		/// </summary>
		public Texture2D getSnapshot(int texIndex)
		{
			if (!AssureRTs())
				throw new System.NullReferenceException("No canvas existing!");
			if (texIndex < 0 || (canvas_Format != Format.Multi && texIndex != 0) || (canvas_Format == Format.Multi && canvasChannelCount <= texIndex))
				throw new System.IndexOutOfRangeException("Canvas does not contain a channel with index " + texIndex);

			// Make snapshot
			Texture2D canvasTex = RTtoTexture(tex_canvas[texIndex]);
			canvasTex.name = canvas_Name + (canvas_Format == Format.Multi ? ("(" + texIndex + ")") : "");
			return canvasTex;
		}

		/// <summary>
		/// Gets a raw snapshot in the current format (Color or Value) of the current canvas texture
		/// BitDepth is specified at a constant 16Bit (bitDepth setting)
		/// </summary>
		public byte[] getRawSnapshot()
		{
			return getRawSnapshot(curTexIndex);
		}

		/// <summary>
		/// Gets a raw snapshot in the current format (Color or Value) of the canvas texture at index
		/// BitDepth is specified at a constant 16Bit (bitDepth setting)
		/// </summary>
		public byte[] getRawSnapshot(int texIndex)
		{
			Texture2D snapshot = getSnapshot(texIndex);
			byte[] rawTex = canvas_Format == Format.Value ? RawImportExport.GetRawGrayscale(snapshot, (int)bitDepth) : snapshot.GetRawTextureData();
			GlobalPainting.DestroyObj(snapshot);
			return rawTex;
		}

		#endregion

		#region Public Channel API
		
		/// <summary>
		/// Deletes a canvas channel from the Mutli-Mask canvas
		/// </summary>
		public void DeleteCanvasChannel(int index)
		{
			if (canvas_Format != Format.Multi)
				throw new System.NotSupportedException("Cannot remove channels from canvases of format color or value!");
			if (canvasChannelCount <= index || index < 0)
				throw new System.ArgumentException("Channel index to remove does not exist!");

			// Remap internal channels to represent removal
			DoChannelRemap((List<int> channelMap) => channelMap.RemoveAt(index));

			// Update changed canvas
			PushContent();
			RegisterCanvasUndo("Removed Channel");
		}

		/// <summary>
		/// Adds a new canvas channel to the Mutli-Mask canvas at the specified index
		/// </summary>
		public void AddNewCanvasChannel(int index)
		{
			if (canvas_Format != Format.Multi)
				throw new System.NotSupportedException("Cannot add channels to canvases of format color or value!");
			if (index < 0)
				throw new System.ArgumentException("Channel index to add new channel to does not exist!");
			index = Mathf.Clamp(index, 0, canvasChannelCount);

			// Remap internal channels to represent addition
			// Add channelMap.Count to reuse potential old channels still cached
			DoChannelRemap((List<int> channelMap) => channelMap.Insert(index, channelMap.Count));

			// Update changed canvas
			PushContent();
			RegisterCanvasUndo("Added new Channel");
		}

		/// <summary>
		/// Moves the specified channel in the Mutli-Mask canvas to the new index
		/// </summary>
		public void MoveChannel(int index, int newIndex)
		{
			if (index == newIndex)
				return;
			if (canvas_Format != Format.Multi)
				throw new System.NotSupportedException("Cannot move channels in canvases of format color or value!");
			if (canvasChannelCount <= index || index < 0)
				throw new System.ArgumentException("Channel index to move from does not exist!");
			if (canvasChannelCount <= newIndex || newIndex < 0)
				throw new System.ArgumentException("Channel index to move to does not exist!");

			// Remap internal channels to represent movement
			DoChannelRemap((List<int> channelMap) =>
			{ // Move index
				channelMap.RemoveAt(index);
				channelMap.Insert(newIndex, index);
			});

			// Update changed canvas
			PushContent();
			RegisterCanvasUndo("Moved Channel");
		}

		/// <summary>
		/// Fills the specified channel with either black or white
		/// </summary>
		public void FillChannel(int index, bool black)
		{
			if (canvas_Format != Format.Multi)
				throw new System.NotSupportedException("Cannot fill channels in canvases of format color or value!");
			if (canvasChannelCount <= index || index < 0)
				throw new System.ArgumentException("Channel index to fill does not exist!");

			// Remap internal channels to represent movement
			DoChannelRemap((List<int> channelMap) =>
			{ // Fill index
				if (black || !paint_normChannels)
					channelMap[index] = black ? -2 : -1;
				else // Selected ch white, all others black
					for (int i = 0; i < channelMap.Count; i++)
						channelMap[i] = i == index ? -1 : -2;
			});

			// Update changed canvas
			PushContent();
			RegisterCanvasUndo((black ? "Cleared" : "Filled") + " Channel");
		}

		/// <summary>
		/// Composes a texture out of 4 specified canvas channels (in multi format)
		/// channelMap indexes the canvas channels (or -1/-2 for white/black) 
		/// to pack into the r, g, b and a channels of the composed texture respectively
		/// </summary>
		private RenderTexture ComposeChannels(int[] channelMap)
		{
			if (!canvas_Exists)
				return null;
			if (channelMap.Length < 4)
				throw new ArgumentException("Cannot compose channel, channelMap does nto provide enough information!");

			// Setup mapping data
			List<int> texMap = new List<int>();
			for (int i = 0; i < 4; i++)
			{
				int map = channelMap[i];
				if (map >= canvasChannelCount || map < -2)
					map = -2;
				if (map >= 0)
				{ // Enter texture
					int tex = Mathf.FloorToInt((float)map / 4);
					int ch = map % 4;
					if (!texMap.Contains(tex))
						texMap.Add(tex);
					map = texMap.IndexOf(tex) * 4 + ch;
				}
				channelMap[i] = map;
			}

			// Setup mapping on material
			ChannelMat.SetTexture("texture0", texMap.Count >= 1 ? tex_canvas[texMap[0]] : null);
			ChannelMat.SetTexture("texture1", texMap.Count >= 2 ? tex_canvas[texMap[1]] : null);
			ChannelMat.SetTexture("texture2", texMap.Count >= 3 ? tex_canvas[texMap[2]] : null);
			ChannelMat.SetTexture("texture3", texMap.Count >= 4 ? tex_canvas[texMap[3]] : null);
			ChannelMat.SetInt("shuffleR", channelMap[0]);
			ChannelMat.SetInt("shuffleG", channelMap[1]);
			ChannelMat.SetInt("shuffleB", channelMap[2]);
			ChannelMat.SetInt("shuffleA", channelMap[3]);

			// Execute channel blending
			RenderTexture composedRT = RequestNewRT(canvas_Name + ":Composed");
			RenderCurrentSetup(ChannelMat, 0, composedRT);

			return composedRT;
		}

		/// <summary>
		/// Remaps the channels in this canvas according to the change done to the channelMap representation.
		/// channelMap is a list of pointers to channels from 0-canvasChannelCount.
		/// Channel addition, removal and reordering can be easily done this way.
		/// When adding a channel pointer, don't add one already existing (no duplication)!!
		/// </summary>
		private void DoChannelRemap(Action<List<int>> changeMap)
		{
			// Create new channel map
			List<int> channelMap = new List<int>();
			for (int i = 0; i < canvasChannelCount; i++)
				channelMap.Add(i);

			// Modify channel map
			changeMap(channelMap);

			// Update values
			int newChCnt = channelMap.Count;
			int newTexCnt = Mathf.CeilToInt((float)newChCnt / 4);

			// Render new channel combination
			List<RenderTexture> newCanvasTex = new List<RenderTexture>(newTexCnt);
			for (int t = 0; t < newTexCnt; t++)
			{ // Compose new canvas textures from remaining channels
				int[] chMap = new int[] {
					TryIndex (channelMap, t*4+0, -2),
					TryIndex (channelMap, t*4+1, -2),
					TryIndex (channelMap, t*4+2, -2),
					TryIndex (channelMap, t*4+3, -2) };
				newCanvasTex.Add(ComposeChannels(chMap));
			}

			// Update channels
			List<CanvasChannel> newCanvasChannels = new List<CanvasChannel>();
			for (int i = 0; i < newChCnt; i++)
			{ // Map channels accordingly
				CanvasChannel mappedChannel = TryIndex(canvasChannels, channelMap[i] < 0 ? i : channelMap[i], new CanvasChannel(), true);
				newCanvasChannels.Add(mappedChannel);
			}
			// Fill with all remaining canvas channels as buffer / cache
			newCanvasChannels.AddRange(canvasChannels.Where(Ch => Ch != null));
			// Update channel representation
			canvasChannels = newCanvasChannels;
			MatchCanvasChannelCount(newChCnt);

			// Assign adjusted canvas and update
			tex_canvas = newCanvasTex;
		}

		#endregion


		#region Session

		public bool hasUnsavedChanges { get { return isDirty || !cache_Canvas || cache_Asset == null; } }

		/// <summary>
		/// Loads the last session from the referenced cache asset.
		/// forceLoad applies in case the painter has been told to unload the canvas but the reference to the cache asset is remained.
		/// canRefreshAssetDatabase has to be disabled in some functions like OnBeforeSerialize to prevent Unity from crashing.
		/// </summary>
		public void LoadLastSession(bool forceLoad = false, bool canRefreshAssetDatabase = true)
		{
			if (!cache_Canvas && !forceLoad)
				return;
			cache_NoAsset = false;

#if UNITY_EDITOR // Get cache path from selected cache asset
			if (cache_Asset != null && UnityEditor.AssetDatabase.Contains(cache_Asset))
				cache_Path = UnityEditor.AssetDatabase.GetAssetPath(cache_Asset);
#endif

			if (string.IsNullOrEmpty(cache_Path))
			{ // Try to fetch from default path
				string path = Settings.lastSessionCacheFolder + "/" + uniqueID + "_" + canvas_Name + ".bytes";
				if (File.Exists(path))
					cache_Path = path;
			}

			// Check if cache is existant
			cache_Canvas = !string.IsNullOrEmpty(cache_Path);
			if (!cache_Canvas)
				return;

			// Import cached canvas
			if (!ImportCanvas(cache_Path, cache_Format, cache_ChannelCnt, cache_SizeX, cache_SizeY))
				Debug.LogWarning("Failed to load cache from '" + cache_Path + "'!");

			// Try to update cache asset if possible
			CheckCacheAsset(canRefreshAssetDatabase);
			isDirty = false;
		}

		/// <summary>
		/// Saves the current session to the referenced cache asset.
		/// canRefreshAssetDatabase has to be disabled in some functions like OnBeforeSerialize to prevent Unity from crashing.
		/// </summary>
		public void SaveCurrentSession(bool canRefreshAssetDatabase = true)
		{
			if (!canvas_Exists)
			{ // Just ignore cache if canvas was deleted
				cache_Canvas = false;
				return;
			}

            if (!hasUnsavedChanges)
				return; // If there's nothing to save, don't save anything

			if (!Directory.Exists(Settings.lastSessionCacheFolder))
				Directory.CreateDirectory(Settings.lastSessionCacheFolder);

			// Generate and check new cache path
			string curSessionPath = Settings.lastSessionCacheFolder + "/" + uniqueID + "_" + canvas_Name + ".bytes";
#if UNITY_EDITOR // Write in existing cache asset if existant
			if (cache_Asset != null && UnityEditor.AssetDatabase.Contains(cache_Asset))
				curSessionPath = UnityEditor.AssetDatabase.GetAssetPath(cache_Asset);
#endif

			// Export canvas to cache path
			if (!ExportCanvas(curSessionPath))
				Debug.LogError("Failed to save cache to '" + curSessionPath + "'!");

            // Save information about the cache
            cache_Canvas = true;
			cache_NoAsset = false;
			cache_Path = curSessionPath;
			cache_SizeX = canvas_SizeX;
			cache_SizeY = canvas_SizeY;
			cache_Format = canvas_Format;
			cache_ChannelCnt = canvasChannelCount;

			// Try to update cache asset if possible
			CheckCacheAsset(canRefreshAssetDatabase);
			isDirty = false;
		}

		/// <summary>
		/// Checks the cache asset of this painter and searches it if needed.
		/// Disable refreshDatabase in some functions like OnBeforeSerialize to prevent Unity from crashing.
		/// </summary>
		public void CheckCacheAsset(bool refreshDatabase = true)
		{
			if (cache_Asset == null && !cache_NoAsset && (canvas_Exists || cache_Canvas))
            { // Load cache asset file
#if UNITY_EDITOR
                if (refreshDatabase)  // Have to refresh database
					UnityEditor.AssetDatabase.Refresh();

				cache_Asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(cache_Path);
				if (cache_Asset != null)
					UnityEditor.EditorGUIUtility.PingObject(cache_Asset);
				else if (Application.isPlaying)
					cache_NoAsset = true; // Tag to not try searching it again
				else if (canvas_Exists)
				{
					if (refreshDatabase)
					{
						cache_Canvas = false;
						cache_NoAsset = true;
					}
					else
					{
						cache_Canvas = true;
						cache_NoAsset = false;
					}
				}
				else
				{
					cache_Canvas = false;
					cache_NoAsset = true;
                }
#endif
            }
        }

		#endregion

		#region Import/Export

		/// <summary>
		/// Exports the canvas textures to the path with extension .png, .raw or .bytes.
		/// For all types but the combined format .bytes, '(n)' is appended to the name for multiple canvas textures in the multi format.
		/// </summary>
		public bool ExportCanvas(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			bool rawEncoding = path.EndsWith(".raw") || path.EndsWith(".bytes");
			int expectedByteLength = canvas_SizeX * canvas_SizeY * (int)bitDepth * (canvas_Format == Format.Value ? 1 : 4);

			// Encode all textures
			List<byte[]> saveData = new List<byte[]>();
			for (int i = 0; i < canvasTextureCount; i++)
				saveData.Add(rawEncoding ? getRawSnapshot(i) : getSnapshot(i).EncodeToPNG());

			// Make sure there are no failures
			if (rawEncoding)
			{
				if (saveData.Any(o => o == null || o.Length != expectedByteLength))
				{ // Found invalid raw data
					Debug.LogWarning("Unexpected save data size! " +
						"Expected " + canvas_SizeX + "*" + canvas_SizeY + "*" + (int)bitDepth + "*" + (canvas_Format == Format.Value ? 1 : 4) + "=" + expectedByteLength + " Bytes!" +
						" Received " + saveData[0].Length + "Bytes!");
					return false;
				}
			}

			// Save encoded texture bytes
			if (path.EndsWith(".bytes"))
			{ // Combined file format
				int texOffset = 16, texCount = saveData.Count, texSize = expectedByteLength;
				byte[] combinedSave = new byte[texOffset + texCount * texSize];
				Array.Copy(BitConverter.GetBytes((short)texOffset), 0, combinedSave, 0, 2);
				Array.Copy(BitConverter.GetBytes((short)texCount), 0, combinedSave, 2, 2);
				Array.Copy(BitConverter.GetBytes((int)texSize), 0, combinedSave, 4, 4);
				Array.Copy(BitConverter.GetBytes((short)canvas_SizeX), 0, combinedSave, 8, 2);
				Array.Copy(BitConverter.GetBytes((short)canvas_SizeY), 0, combinedSave, 10, 2);
				Array.Copy(BitConverter.GetBytes((short)canvas_Format), 0, combinedSave, 12, 2);
				Array.Copy(BitConverter.GetBytes((short)canvasChannelCount), 0, combinedSave, 14, 2);
				// Write in tex byte blocks
				for (int i = 0; i < saveData.Count; i++)
					Array.Copy(saveData[i], 0, combinedSave, texOffset + i * texSize, texSize);
				// Write file
				WriteDataVC(path, combinedSave);
			}
			else
			{ // Potentially split files
				if (canvas_Format == Format.Multi)
				{ // Multi format -> save as subfiles name(n)
					int itInd = Path.GetFileNameWithoutExtension(path).LastIndexOf('(');
					if (itInd > 0) // Clear name from existing (n) postfix
						path = Path.GetDirectoryName(path) + Path.AltDirectorySeparatorChar + Path.GetFileNameWithoutExtension(path).Substring(0, itInd) + Path.GetExtension(path);
					int endNameIndex = path.LastIndexOf('.');
					for (int i = 0; i < saveData.Count; i++)
						WriteDataVC(path.Insert(endNameIndex, "(" + i + ")"), saveData[i]);
				}
				else if (saveData.Count > 0) // Save file
					WriteDataVC(path, saveData[0]);
			}

			return true;
		}

		/// <summary>
		/// Imports the data at the path with extension .png, .raw or .bytes into the canvas.
		/// Don't call for .raw paths, use the overload specifying the canvas information for the raw file instead.
		/// </summary>
		public bool ImportCanvas(string path)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			if (path.EndsWith(".raw"))
				Debug.LogWarning("No canvas information specified for raw import! Using default canvas data!");
			int texCount;
			if (getMultiTextureInfo(ref path, out texCount))
				return ImportCanvas(path, Format.Multi, texCount * 4, canvas_SizeX, canvas_SizeY);
			else
				return ImportCanvas(path, canvas_Format != Format.Multi ? canvas_Format : Format.Color, 1, canvas_SizeX, canvas_SizeY);
		}

		/// <summary>
		/// Imports the data at the path with extension .png, .raw or .bytes into the canvas.
		/// For all extensions but .raw, additional information does not need to be specified and is read out of the file.
		/// </summary>
		public bool ImportCanvas(string path, Format format, int chCnt, int sX, int sY)
		{
			if (string.IsNullOrEmpty(path))
				return false;
			path = ResourceManager.MakePathAbsolute(path);
			bool rawEncoding = path.EndsWith(".raw") || path.EndsWith(".bytes");

			// Load encoded texture bytes
			List<byte[]> saveData = new List<byte[]>();
			if (path.EndsWith(".bytes"))
			{ // Combined file format
				if (!File.Exists(path))
					return false;

				// Fetch save data and information
				byte[] combinedSave = File.ReadAllBytes(path);
				int texOffset = BitConverter.ToInt16(combinedSave, 0);
				int texCount = BitConverter.ToInt16(combinedSave, 2);
				int texSize = BitConverter.ToInt32(combinedSave, 4);
				sX = BitConverter.ToInt16(combinedSave, 8);
				sY = BitConverter.ToInt16(combinedSave, 10);
				format = (Format)BitConverter.ToInt16(combinedSave, 12);
				chCnt = BitConverter.ToInt16(combinedSave, 14);
				if (combinedSave.Length != (texOffset + texCount * texSize))
					return false;

				for (int i = 0; i < texCount; i++)
				{ // Read out tex byte blocks
					byte[] texBytes = new byte[texSize];
					Array.Copy(combinedSave, texOffset + i * texSize, texBytes, 0, texSize);
					saveData.Add(texBytes);
				}
			}
			else
			{ // Potentially Split files
				if (format == Format.Multi)
				{ // Multi format -> load all subfiles name(n)
					int texCount = Mathf.CeilToInt(((float)chCnt) / 4);
					int itInd = Path.GetFileNameWithoutExtension(path).LastIndexOf('(');
					if (itInd > 0) // Clear name from existing (n) postfix
						path = Path.GetDirectoryName(path) + Path.AltDirectorySeparatorChar + Path.GetFileNameWithoutExtension(path).Substring(0, itInd) + Path.GetExtension(path);
					int endNameIndex = path.LastIndexOf('.');
					for (int i = 0; i < texCount; i++)
					{ // Read bytes from subfile
						string texPath = path.Insert(endNameIndex, "(" + i + ")");
						if (File.Exists(texPath))
							saveData.Add(File.ReadAllBytes(texPath));
						else
							return false;
					}
				}
				else if (File.Exists(path)) // Read bytes
					saveData.Add(File.ReadAllBytes(path));
				else
					return false;
			}

			// Make sure there are no failures
			if (rawEncoding)
			{
				int expectedByteLength = sX * sY * (int)bitDepth * (format == Format.Value ? 1 : 4);
				if (saveData.Any(o => o == null || o.Length != expectedByteLength))
				{ // Found invalid raw data
					Debug.LogWarning("Unexpected save data size!");
					return false;
				}
			}

			// Decode all textures
			List<Texture2D> sessionTextures = new List<Texture2D>(saveData.Count);
			for (int i = 0; i < saveData.Count; i++)
			{ // Decode and rename appropriately
				Texture2D tex = null;
				if (rawEncoding)
					tex = RawImportExport.LoadRawImage(saveData[i], (int)format, sX, sY);
				else
				{
					tex = new Texture2D(sX, sY, TexFormat, false);
					tex.LoadImage(saveData[i]);
				}
				tex.name = Path.GetFileNameWithoutExtension(path);
				if (Regex.IsMatch(tex.name, @"\d+_"))
					tex.name = tex.name.Substring(tex.name.IndexOf("_") + 1);
				sessionTextures.Add(tex);
			}
			// Import all loaded textures
			ImportTexture(sessionTextures, chCnt, true, format);

			return true;
		}

		/// <summary>
		/// Returns how many textures are saved for the multi-canvas at path.
		/// Checks for the format 'name(n)' with n between 0 and 8, path following the same convention
		/// </summary>
		public static bool getMultiTextureInfo(ref string path, out int count)
		{
			// Find iterator as (n) after the name
			int itInd = Path.GetFileNameWithoutExtension(path).LastIndexOf('(');
			if (itInd > 0)
			{
				path = Path.GetDirectoryName(path) + Path.AltDirectorySeparatorChar + Path.GetFileNameWithoutExtension(path).Substring(0, itInd) + Path.GetExtension(path);
				// Try to find as much textures with this naming convention as possible
				int endNameIndex = path.LastIndexOf('.');
				for (count = 0; count < 9; count++)
				{
					if (!File.Exists(path.Insert(endNameIndex, "(" + count + ")")))
						break;
				}
				return true;
			}
			count = 1;
			return false;
		}

		/// <summary>
		/// Wrapper for asset writing to account for VersionControl
		/// </summary>
		private void WriteDataVC(string path, byte[] data)
		{
#if UNITY_EDITOR
			if (UnityEditor.VersionControl.Provider.enabled && File.Exists(path))
			{
				UnityEditor.VersionControl.Asset asset = UnityEditor.VersionControl.Provider.GetAssetByPath(path);
				if ((asset.state & UnityEditor.VersionControl.Asset.States.ReadOnly) != 0)
					UnityEditor.VersionControl.Provider.Checkout(asset, UnityEditor.VersionControl.CheckoutMode.Both).Wait ();
			}
#endif
			File.WriteAllBytes(path, data);
		}

		#endregion

		#region Undo System

		/// <summary>
		/// Registers a change to the the canvas for an undo operation.
		/// Called AFTER the change has happened.
		/// Optionally stores current mods into the undo record.
		/// </summary>
		private void RegisterCanvasUndo(string message, bool writeMods = false, bool setDirty = true)
		{
			if (canvasUndoList == null)
				canvasUndoList = new List<UndoRecord>();
			if (canvasRedoList == null)
				canvasRedoList = new List<UndoRecord>();

            isDirty = true;

			if (!Settings.enableUndo)
			{
				ClearRecordsUntil(canvasRedoList, 0);
				ClearRecordsUntil(canvasUndoList, 0);
				return;
			}
			MatchCanvasChannelCount();

			// Build undo record
			UndoRecord undoRecord = new UndoRecord();
			undoRecord.name = canvas_Name;
			undoRecord.message = message;
			if (canvas_Exists)
			{
				// Set canvas information
				undoRecord.sizeX = canvas_SizeX;
				undoRecord.sizeY = canvas_SizeY;
				undoRecord.format = canvas_Format;
				undoRecord.texFormat = RTFormat;
				
				// Save snapshot
				undoRecord.tex = new List<Texture2D>();
				for (int i = 0; i < tex_canvas.Count; i++)
				{
					Texture2D snapshot = getSnapshot(i);
					if (snapshot != null)
						snapshot.name = "TPUNDO:" + ID + ":" + message + "(" + i + ")";
					undoRecord.tex.Add(snapshot);
				}
				undoRecord.channels = new List<CanvasChannel>(canvasChannels);
				undoRecord.channelCount = canvasChannelCount;
				
				// Save mods and whether they are important
				undoRecord.writeMods = writeMods;
				undoRecord.mods = mods;
			}

			// Add undo record and clear redo list
			canvasUndoList.Add(undoRecord);
			ClearRecordsUntil(canvasUndoList, Settings.undoStackSize);
			ClearRecordsUntil(canvasRedoList, 0);
		}

		/// <summary>
		/// Performs one undo step.
		/// </summary>
		public void PerformUndo()
		{
			if (!canPerformUndo)
				return;
			
			// Take the last operation record from undo stack
			UndoRecord canvasRecord = canvasUndoList[canvasUndoList.Count - 1];
			canvasUndoList.RemoveAt(canvasUndoList.Count - 1);
			
			// And put it on the redo stack
			canvasRedoList.Add(canvasRecord);
			ClearRecordsUntil(canvasRedoList, Settings.undoStackSize);
			
			// Undo it by restoring the previous record
			canvasRecord = canvasUndoList[canvasUndoList.Count - 1];
			RestoreUndoRecord(canvasRecord);
		}

		/// <summary>
		/// Performs one redo step.
		/// </summary>
		public void PerformRedo()
		{
			if (!canPerformRedo)
				return;
			
			// Take the last undone operation from the redo stack
			UndoRecord canvasRecord = canvasRedoList[canvasRedoList.Count - 1];
			canvasRedoList.RemoveAt(canvasRedoList.Count - 1);
			
			// And put it back on the undo stack
			canvasUndoList.Add(canvasRecord);
			ClearRecordsUntil(canvasUndoList, Settings.undoStackSize);
			
			// Then restore it to redo the operation it represents
			RestoreUndoRecord(canvasRecord);
		}

		/// <summary>
		/// Clears and frees records from the list until the specified target size is reached.
		/// </summary>
		private void ClearRecordsUntil(List<UndoRecord> recordList, int targetSize)
		{
			while (recordList.Count > targetSize)
			{
				UndoRecord record = recordList[0];
				if (record.tex != null)
				{
					for (int i = 0; i < record.tex.Count; i++)
						GlobalPainting.DestroyObj(record.tex[i]);
				}
				recordList.RemoveAt(0);
			}
		}

		/// <summary>
		/// Restores the state of the specified record
		/// </summary>
		private void RestoreUndoRecord(UndoRecord rec)
		{
			if (rec.tex != null && rec.tex.Count > 0 && !rec.tex.Any((Texture2D tex) => tex == null))
			{
				bool specsChanged = !canvas_Exists || canvas_Format != rec.format || canvas_SizeX != rec.sizeX || canvas_SizeY != rec.sizeY || RTFormat != rec.texFormat;
				if (specsChanged)
				{ // Clear canvases and write specs
					ReleaseRTs(true, true);
					canvas_Format = rec.format;
					canvas_SizeX = rec.sizeX;
					canvas_SizeY = rec.sizeY;
				}
				if (rec.writeMods)
					mods = rec.mods;
				canvas_Name = rec.name;

				// Restore canvas content
				canvasChannels = new List<CanvasChannel>(rec.channels);
				MatchCanvasChannelCount(canvas_Format == Format.Multi ? rec.channelCount : 1);
				for (int i = 0; i < tex_canvas.Count; i++)
					Graphics.Blit(rec.tex[i], tex_canvas[i]);

				// Udpate temp RTs
				if (specsChanged)
					CreateTempRTs();
			}
			else
			{ // Canvas was null
				ReleaseRTs(true, true);
			}
			PushContent();
		}

		#endregion


		#region Painting

		/// <summary>
		/// Update paint state (through variables set in "state_")
		/// </summary>
		public bool PaintUpdate(string PaintUserID, int controlID)
		{
			if (!canvas_Exists)
				return false;
			//if (Event.current.type == EventType.Repaint || Event.current.type == EventType.Layout)
			//	return false;
			if (isPainting && PaintUserID != state_UserID)
				return false;
			if (state_BlockPainting && !isPainting)
				return false;

			if (controlID == 0)
				controlID = GUIUtility.GetControlID(FocusType.Passive);

			bool mouseMove = Event.current.GetTypeForControl(controlID) == EventType.MouseMove;
			bool mouseHold = Event.current.button == 0;
			bool mouseDrag = Event.current.GetTypeForControl(controlID) == EventType.MouseDrag && Event.current.button == 0;
			bool mouseDown = Event.current.GetTypeForControl(controlID) == EventType.MouseDown && Event.current.button == 0;
			bool mouseUp = Event.current.GetTypeForControl(controlID) == EventType.MouseUp && Event.current.button == 0;

			bool painted = false;

			if (!state_BlockPainting && mouseDown)
			{ // Start Painting
				if (isPainting)
					EndPainting();

				GUIUtility.hotControl = controlID;
				if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
					Event.current.Use();

				StartPainting(PaintUserID);
				if (isOnCanvas)
				{ // So you can only click once to paint
					painted = true;
					Paint(state_BrushPos);
				}
			}

			if (isPainting)
			{
				if (mouseUp || mouseMove || Event.current.button != 0)
				{ // End Painting
					GUIUtility.hotControl = 0;
					if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
						Event.current.Use();
					EndPainting();
				}

				else if (!state_BlockPainting && (mouseDown || mouseDrag || (mouseHold && Settings.continuousPaint)))
				{ // Painting
					GUIUtility.hotControl = controlID;
					if (Event.current.type != EventType.Layout && Event.current.type != EventType.Repaint)
						Event.current.Use();

					if (isOnCanvas)
					{
						painted = true;
						Paint(state_BrushPos);
					}
				}
			}

			RenderTexture.active = null;

			return painted;
		}

		/// <summary>
		/// Stop Painting
		/// </summary>
		public void StopPainting(string PaintUserID)
		{
			if (isPainting && (state_UserID == PaintUserID || (Event.current.type == EventType.MouseUp && Event.current.button == 0) || Event.current.button != 0))
				EndPainting();
		}

		/// <summary>
		/// Start Painting
		/// </summary>
		private void StartPainting(string PaintUserID)
		{ // Set tex_TempRead, which is the base to draw on, correctly
			if (!AssureRTs())
				throw new System.NullReferenceException("StartPainting: Canvas is null!");

			isPainting = true;
			state_UserID = PaintUserID;

			// Prepare temporary paint textures
			RenderTexture.active = null;
			if (sepPass_Enable) // Current stroke is seperate so result has to be blended afterwards
				Fill(tex_TempRead, Color.clear);
			else // Blit the current canvas into temp pass, directly working on that
				Graphics.Blit(tex_Current, tex_TempRead);

			// Reset Timers
			ResetTimer(ref state_lastPaintTime, Settings.targetPaintInterval);
			ResetTimer(ref state_lastGenerationTime, Settings.targetGenerationInterval);
		}

		/// <summary>
		/// Force end painting
		/// </summary>
		private void EndPainting()
		{
			if (Event.current.type == EventType.Repaint)
				return;
			if (!AssureRTs())
				throw new System.NullReferenceException("EndPainting: Canvas is null!");

			// Reset timers
			ResetTimer(ref state_lastPaintTime, Settings.targetPaintInterval);
			ResetTimer(ref state_lastGenerationTime, Settings.targetGenerationInterval);

			// Finalize textures
			RenderTexture.active = null;
			if (sepPass_Enable)
			{ // Need to merge separate paint texture with canvas textures
				if (canvas_Format == Format.Multi && paint_normChannels)
				{ // Already merged in tex_tempCanvas from PushContent
					if (tex_tempCanvas == null || tex_tempCanvas.Count != tex_canvas.Count)
						throw new System.NullReferenceException("Temp Canvas Textures are not set up on EndPainting with MultiNormalization!");
					List<RenderTexture> tTex = tex_canvas;
					tex_canvas = tex_tempCanvas;
					tex_tempCanvas = tTex;
				}
				else
				{ // Merge passes
					MatSetupBase();
					MatSetupBlend(tex_Current, tex_TempRead, (int)paint_Brush.mode, 1f);
					RenderCurrentSetup(1, tex_tempResult);
					Graphics.Blit(tex_tempResult, tex_Current);
				}
			}
			else // No blending required
				Graphics.Blit(tex_TempRead, tex_Current);

			// Push updated canvas content
			PushContent();
			RegisterCanvasUndo("" + paint_Brush.mode.ToString() + "");

			isPainting = false;
		}

		/// <summary>
		/// Applies a paint stroke at the specified position
		/// </summary>
		public void Paint(Vector2 pos)
		{
			if (Event.current.type == EventType.Repaint)
				return;

			float timeStep = 1;
			if (isPainting && !CheckTimer(ref state_lastPaintTime, Settings.targetPaintInterval, out timeStep))
				return; // Make sure to draw according to the target framerate

			if (Mathf.Abs(pos.x - 0.5f) - paint_Brush.size < 0.5f && Mathf.Abs(pos.y - 0.5f) - paint_Brush.size < 0.5f)
			{ // Brush position is in the bounds of the canvas

				RenderTexture.active = null;

				if (OnPainting != null)
					OnPainting.Invoke();

				// Setup Material
				PaintMat.SetTexture("_Canvas", tex_TempRead);
				PaintMat.SetFloat("_timeStep", Settings.continuousPaint ? timeStep * 20 : 1);
				MatSetupBase();
				MatSetupBrush();
				MatSetupPaint();

				// Render stroke to temporary texture
				RenderCurrentSetup(0, tex_TempWrite);
				tex_tempSwitch = !tex_tempSwitch;

				// Push updated temporary content
				tex_CurRawPaint = tex_TempRead;
				if (sepPass_Enable)
					PushContent(tex_Current, tex_TempRead);
				else
					PushContent(tex_TempRead);
			}
		}

		/// <summary>
		/// Renders out the current brush as a preview
		/// </summary>
		public RenderTexture getBrushPreview(int resolution, bool applySettings)
		{
			// Setup Base
			PaintMat.SetInt("_sizeX", resolution);
			PaintMat.SetInt("_sizeY", resolution);
			PaintMat.SetFloat("_timeStep", 20);
			PaintMat.SetVector("_channelMask", Vector4.one);

			// Setup Brush values
			MatSetupBrush();
			PaintMat.SetVector("_brushPos", new Vector4(0.5f, 0.5f, 0, 0));
			PaintMat.SetFloat("_brushIntensity", applySettings ? paint_Brush.intensity : 1);
			PaintMat.SetInt("_paintMode", applySettings ? (int)paint_Brush.mode : (int)PaintMode.Add);
			PaintMat.SetColor("_paintColor", Color.white * (applySettings ? paint_Intensity * paint_TargetValue : 1));

			// Render
			RenderTexture target = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.ARGB32);
			RenderCurrentSetup(0, target);
			return target;
		}

		#endregion

		#region Post-Processing

		/// <summary>
		/// Pushes the internal canvas content to the modified visualization state
		/// </summary>
		private void PushContent()
		{
			PushContent(null, null);
		}

		/// <summary>
		/// Pushes the internal canvas content with modified current texture according to the canvas_Format to the modified visualization state
		/// </summary>
		private void PushContent(RenderTexture curTexFrame)
		{
			PushContent(curTexFrame, null);
		}

		/// <summary>
		/// Pushes the internal canvas content with one modified current texture according to the canvas_Format to the modified visualization state and blended with the specified texture
		/// </summary>
		private void PushContent(RenderTexture curTexFrame, RenderTexture blendTex)
		{
			if (!canvas_Exists)
			{
				AssignAllCanvas(null);
				return;
			}
			if (isPainting && !CheckTimer(ref state_lastGenerationTime, Settings.targetGenerationInterval))
				return;
			if (canvas_Format != Format.Multi && tex_canvas.Count != 1)
				throw new System.ArgumentException("Invalid texture count: Cannot have multiple textures without Multi Format!");
			

			// PREPARATION

			if (curTexFrame == null)
				curTexFrame = tex_Current;

			// Get requirements
			bool needsSingleBlend = blendTex != null,
				needsMultiMix = canvas_Format == Format.Multi && canvasVizState == CanvasVizState.All,
				needsMods = applyingOngoingMods;
			// Discard if no special requirements
			if (!needsSingleBlend && !needsMultiMix && !needsMods)
			{ // No need to blend any textures or to modify the canvas
				Graphics.Blit(curTexFrame, tex_VizCanvas);
				AssignModCanvas(tex_VizCanvas);
				return;
			}

			// Material Setup
			MatSetupBase();
			List<RenderTexture> curCanvasRTs = tex_canvas;
			

			// PRE-PROCESSING

			if (needsSingleBlend && canvas_Format == Format.Multi)
			{ // Apply blending and normalization as pre-processing to each channel texture

				// Prepare temporary blend targets
				MatchListLength(ref tex_tempCanvas, tex_canvas.Count, RequestNewRT, ReleaseRT);

				if (paint_normChannels)
				{ // Blend and normalize all channel textures

					// Setup material
					MatSetupBlend(curTexFrame, blendTex, (int)paint_Brush.mode, paint_Brush.intensity, true);

					for (int i = 0; i < tex_canvas.Count; i++)
					{ // Render each single texture
						PaintMat.SetTexture("_Canvas", tex_canvas[i]);
						PaintMat.SetVector("_channelMask", i == curTexIndex ? getChannelMask(curChannelIndex % 4) : Vector4.zero);
						RenderCurrentSetup(5, tex_tempCanvas[i]);
					}

					// Update current textures to temporary
					curCanvasRTs = tex_tempCanvas;
					curTexFrame = curCanvasRTs[curTexIndex];
				}
				else
				{ // Only blend with current texture

					// Setup material
					MatSetupBlend(curTexFrame, blendTex, (int)paint_Brush.mode, paint_Brush.intensity, false);

					// Blend current texture
					PaintMat.SetTexture("_Canvas", curTexFrame);
					PaintMat.SetVector("_channelMask", getChannelMask(curChannelIndex % 4));
					RenderCurrentSetup(1, tex_tempCanvas[curTexIndex]);

					// Update current textures
					curCanvasRTs = new List<RenderTexture>(tex_canvas);
					curCanvasRTs[curTexIndex] = tex_tempCanvas[curTexIndex];
					curTexFrame = curCanvasRTs[curTexIndex];
				}

				needsSingleBlend = false;
			}

			// VISUALIZATION

			// Setup material based on requirements
			int pass = -1;
			if (needsMultiMix)
			{ // Apply texture color mixing
				pass = 4;
				MatSetupMultiMix(curCanvasRTs);
			}
			else if (needsMods)
			{ // Apply modifications and blending in one pass
				pass = needsSingleBlend ? 3 : 2;
				MatSetupBlend(curTexFrame, blendTex, (int)paint_Brush.mode, 1f);
				MatSetupMods(canvas_Format == Format.Color && mods.advancedChannelMods);
			}
			else if (needsSingleBlend)
			{ // Apply blending
				pass = 1;
				MatSetupBlend(curTexFrame, blendTex, (int)paint_Brush.mode, 1f);
			}

			// Render material setup
			if (pass != -1)
				RenderCurrentSetup(pass, tex_VizCanvas);
			else
				Graphics.Blit(curTexFrame, tex_VizCanvas);

			// Assign result
			if (canvas_Format == Format.Multi) // Assign pre-processed textures
				AssignAllCanvas(curCanvasRTs.ToArray ());
			else // Assign final textures
				AssignAllCanvas(tex_VizCanvas);
		}

		/// <summary>
		/// Applies all current modifications to the canvas
		/// </summary>
		public void ApplyModifications()
		{
			if (!applyingOngoingMods)
				return;
			if (canvas_Format == Format.Multi)
				throw new System.NotImplementedException("Cannot apply modifications on multi canvas!");

			// Save changes on modification values so you can go back without resetting the values
			RegisterCanvasUndo("Modification", true);

			// Setup Material
			MatSetupBase();
			MatSetupMods(canvas_Format == Format.Color && mods.advancedChannelMods);

			// Render modifications
			PaintMat.SetTexture("_Canvas", tex_Current);
			RenderCurrentSetup(2, tex_VizCanvas);

			// Reset modification values
			mods = new Modifications(true);

			// Save change
			Graphics.Blit(tex_VizCanvas, tex_Current);
			RegisterCanvasUndo("Applied", true);
		}

		/// <summary>
		/// Updates the visualized canvas by re-applying the ongoing modifications on the internal canvas
		/// </summary>
		public void UpdateModifications()
		{
			PushContent();
		}

		#endregion

		#region Material Setup

		public void MatSetupBase(Material mat = null)
		{
			mat = mat ?? PaintMat;

			// Setup Canvas
			mat.SetInt("_sizeX", canvas_SizeX);
			mat.SetInt("_sizeY", canvas_SizeY);
			mat.SetFloat("_aspect", canvas_Aspect);

			// Set work channels
			mat.SetVector("_channelMask", Vector4.one);

			// Shader Features
			if (Settings.enableGPUUniformBranching) // Whether uniform if clauses should be calculated or branched
				mat.DisableKeyword("CALC_BRANCHES");
			else
				mat.EnableKeyword("CALC_BRANCHES");
		}

		internal void MatSetupBrush(Material mat = null)
		{
			mat = mat ?? PaintMat;

			// Set Brush Base
			mat.SetVector("_brushPos", new Vector4(state_BrushPos.x, state_BrushPos.y, 0, 0));
			mat.SetFloat("_brushSize", paint_Brush.size);
			mat.SetFloat("_brushIntensity", paint_Brush.intensity);
			
			// Set Brush Texture
			Texture2D brushTex = UpdateBrushType();
			mat.SetInt("_brushType", state_BrushFunc);
			mat.SetTexture("_brushTex", brushTex);

			// Set Brush Function
			mat.SetFloat("_brushFalloff", paint_Brush.falloff);
			mat.SetFloat("_brushHardness", paint_Brush.hardness);

			// Set brush matrix
			Matrix4x4 rotMatrix = Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, paint_BrushRotation * 180 + canvas_Rotation), Vector3.one);
			mat.SetMatrix("_brushMatrix", rotMatrix);
		}

		internal void MatSetupPaint(Material mat = null)
		{
			mat = mat ?? PaintMat;

			// Set Paint Parameters
			mat.SetInt("_paintMode", sepPass_Enable ? -1 : (int)(state_InvertBrush ? InvertPaintMode(paint_Brush.mode) : paint_Brush.mode));
			mat.SetColor("_paintColor", paint_Color * (state_InvertBrush && paint_Brush.mode == PaintMode.Add ? -paint_Intensity : paint_Intensity));
			mat.SetFloat("_paintSmoothBias", paint_SmoothenBias);
			mat.SetFloat("_paintTarget", state_InvertBrush ? 1 - paint_TargetValue : paint_TargetValue);
			mat.SetInt("_clamp", paint_Clamp && !sepPass_Enable ? 1 : 0);

			// Set work channels
			if (canvas_Format == Format.Multi)
				mat.SetVector("_channelMask", getChannelMask(curChannelIndex % 4));
			else
				mat.SetVector("_channelMask", Vector4.one);
		}

		private void MatSetupMods(bool channelMods)
		{
			// Setup modification settings
			PaintMat.SetFloat("_modBrightness", mods.brightness);
			PaintMat.SetFloat("_modContrast", mods.contrast);
			PaintMat.SetColor("_modTintCol", mods.tintColor);
			PaintMat.SetInt("_clamp", paint_Clamp ? 1 : 0);

			if (channelMods)
			{
				PaintMat.EnableKeyword("MOD_CHANNEL");

				PaintMat.SetInt("_modR", mods.chR.shuffle);
				PaintMat.SetInt("_modG", mods.chG.shuffle);
				PaintMat.SetInt("_modB", mods.chB.shuffle);
				PaintMat.SetInt("_modA", mods.chA.shuffle);

				PaintMat.SetVector("_modChOffset", new Vector4(
					mods.chR.invert ? 1 + mods.chR.offset : mods.chR.offset,
					mods.chG.invert ? 1 + mods.chG.offset : mods.chG.offset,
					mods.chB.invert ? 1 + mods.chB.offset : mods.chB.offset,
					mods.chA.invert ? 1 + mods.chA.offset : mods.chA.offset));
				PaintMat.SetVector("_modChScale", new Vector4(
					mods.chR.invert ? -mods.chR.scale : mods.chR.scale,
					mods.chG.invert ? -mods.chG.scale : mods.chG.scale,
					mods.chB.invert ? -mods.chB.scale : mods.chB.scale,
					mods.chA.invert ? -mods.chA.scale : mods.chA.scale));
			}
			else
				PaintMat.DisableKeyword("MOD_CHANNEL");
		}
		
		private void MatSetupBlend(Texture baseTex, Texture blendTex, int mode, float intensity, bool normalizedMultiBlend = false)
		{
			PaintMat.SetTexture("_blendTex", blendTex);
			if (normalizedMultiBlend)
			{
				PaintMat.SetTexture("_multiChannelTex", baseTex);
				PaintMat.SetInt("_multiChannelIndex", curChannelIndex % 4);
			}
			else
				PaintMat.SetTexture("_Canvas", baseTex);

			PaintMat.SetInt("_blendMode", mode);
			PaintMat.SetFloat("_blendAmount", intensity);

			PaintMat.SetInt("_clamp", paint_Clamp ? 1 : 0);
			PaintMat.SetVector("_channelMask", Vector4.one);
		}

		private void MatSetupMultiMix(List<RenderTexture> RTs)
		{
			/*#if UNITY_5_4_OR_NEWER
				bool supportsTexArrays = SystemInfo.supports2DArrayTextures && (SystemInfo.copyTextureSupport & UnityEngine.Rendering.CopyTextureSupport.RTToTexture) != 0;
				if (RTs.Count > 4 && !supportsTexArrays)
					throw new System.NotSupportedException ("System either does not support Texture Arrays or Graphics.CopyTexture RTToTexture and thus cannot use more than 8 Channels!");
			#endif*/

			PaintMat.SetInt("_multiTexCount", RTs.Count);
			PaintMat.SetInt("_multiTexIndex", curTexIndex);

#if UNITY_5_4_OR_NEWER
			PaintMat.SetColorArray ("_multiChannelColors", channelVizColors);
#else
			for (int colCnt = 0; colCnt < channelVizColors.Length; colCnt++)
				PaintMat.SetColor("_multiChannelColors" + colCnt, channelVizColors[colCnt]);
#endif

			/*#if UNITY_5_4_OR_NEWER
				if (RTs.Count > shaderManualTextureCount)
				{ // Set three or more textures using a texture array
					if (cachedMultiTexVizArray == null)
					{ // Create new cached texture array
						cachedMultiTexVizArray = new Texture2DArray (canvas_SizeX, canvas_SizeY, RTs.Count, TexFormat, false, true);
						for (int i = 0; i < RTs.Count; i++)
							Graphics.CopyTexture (RTs[i], 0, 0, cachedMultiTexVizArray, i, 0);
					}

					PaintMat.EnableKeyword ("ENABLE_TEXTURE_ARRAYS");
					PaintMat.SetTexture ("_multiTextures", cachedMultiTexVizArray);
				}	
				else
			#endif*/
			{ // Set two or less textures as normal variables
				PaintMat.DisableKeyword("ENABLE_TEXTURE_ARRAYS");
				for (int i = 0; i < RTs.Count; i++)
					PaintMat.SetTexture("_canvasTex" + (i + 1), RTs[i]);
			}

			// Prepare active channel mask for the last texture
			int chInd = canvasChannelCount % 4; // 1 -> 0111
			Vector4 mask = new Vector4(chInd > 0 ? 0 : 1, chInd > 1 ? 0 : 1, chInd > 2 ? 0 : 1, chInd > 3 ? 0 : 1);
			PaintMat.SetVector("_multiLastTexMaskInv", mask);
			PaintMat.SetVector("_channelMask", Vector4.one);
		}

		/// <summary>
		/// Updates the brush type used by the shader specified by the paint_Brush.type in state_BrushFunc and returns the texture
		/// </summary>
		public Texture2D UpdateBrushType()
		{
			if (GlobalPainting.brushTextures.Length <= 0)
				return null;
			paint_Brush.type = Mathf.Clamp(paint_Brush.type, 0, GlobalPainting.brushTextures.Length - 1);
			Texture2D brushTex = GlobalPainting.brushTextures[paint_Brush.type];
			if (temp_brushType != paint_Brush.type)
			{
				temp_brushType = paint_Brush.type;
				if (!brushTex.name.Contains("_func"))
					state_BrushFunc = 0;
				else
				{
					string funcNumStr = brushTex.name.Substring(brushTex.name.IndexOf("_func") + 5);
					int funcNum;
					if (int.TryParse(funcNumStr, out funcNum))
						state_BrushFunc = funcNum;
					else // If texture is labeled as a function but it does not exist, still use that texture
						state_BrushFunc = 0;
				}
			}
			return brushTex;
		}

		private PaintMode InvertPaintMode(PaintMode mode)
		{
			if (mode == PaintMode.Smoothen)
				return PaintMode.Contrast;
			if (mode == PaintMode.Contrast)
				return PaintMode.Smoothen;
			return mode;
		}

		#endregion

		#region Utility

		/// <summary>
		/// Resets the timer
		/// </summary>
		private void ResetTimer(ref float lastTimePoint, float timerInterval)
		{
			lastTimePoint = Time.realtimeSinceStartup - 2 * timerInterval;
		}

		/// <summary>
		/// Returns if the timer has reached it's interval yet and updates it.
		/// </summary>
		private bool CheckTimer(ref float lastTimePoint, float timerInterval)
		{
			bool timer = Time.realtimeSinceStartup - lastTimePoint >= timerInterval;
			lastTimePoint = Time.realtimeSinceStartup - (timerInterval == 0 ? 0 : ((Time.realtimeSinceStartup - lastTimePoint) % timerInterval));
			return timer;
		}

		/// <summary>
		/// Returns if the timer has reached it's interval yet and updates it. Also outputs timeStep.
		/// </summary>
		private bool CheckTimer(ref float lastTimePoint, float timerInterval, out float timeStep)
		{
			timeStep = Time.realtimeSinceStartup - lastTimePoint;
			bool timer = timeStep >= timerInterval;
			//timeStep = timeStep%timerInterval + timerInterval;
			lastTimePoint = Time.realtimeSinceStartup - (timerInterval == 0 ? 0 : ((Time.realtimeSinceStartup - lastTimePoint) % timerInterval));
			return timer;
		}

		/// <summary>
		/// Render a quad used for rendering from a material set with SetPass to the target RT
		/// </summary>
		private void RenderQuad()
		{
			RenderQuad(new Rect(0, 0, 1, 1));
		}

		/// <summary>
		/// Render a quad used for rendering from a material set with SetPass to the target RT
		/// Also lets you specify the rect to execute only
		/// </summary>
		private void RenderQuad(Rect rect)
		{
			GL.Begin(GL.QUADS);
			GL.TexCoord2(rect.xMin, rect.yMin); GL.Vertex3(rect.xMin, rect.yMin, 0.1f);
			GL.TexCoord2(rect.xMax, rect.yMin); GL.Vertex3(rect.xMax, rect.yMin, 0.1f);
			GL.TexCoord2(rect.xMax, rect.yMax); GL.Vertex3(rect.xMax, rect.yMax, 0.1f);
			GL.TexCoord2(rect.xMin, rect.yMax); GL.Vertex3(rect.xMin, rect.yMax, 0.1f);
			GL.End();
		}

		/// <summary>
		/// Renders the canvas using the already set-up PaintMat using the pass into the specified target RT
		/// </summary>
		private void RenderCurrentSetup(int pass, RenderTexture target)
		{
			RenderCurrentSetup(PaintMat, pass, target);
		}

		/// <summary>
		/// Renders the canvas using the already set-up material using the pass into the specified target RT
		/// </summary>
		private void RenderCurrentSetup(Material mat, int pass, RenderTexture target)
		{
			RenderTexture prevRT = RenderTexture.active;
			RenderTexture.active = null;
			GL.PushMatrix();
			GL.LoadOrtho();

			Graphics.SetRenderTarget(target);
			mat.SetPass(pass);
			RenderQuad();

			GL.PopMatrix();
			RenderTexture.active = prevRT;
		}

		/// <summary>
		/// Converts the channel (0-3) to a vector mask to use in a shader
		/// </summary>
		private Vector4 getChannelMask(int channel)
		{
			return new Vector4(channel == 0 ? 1 : 0, channel == 1 ? 1 : 0, channel == 2 ? 1 : 0, channel == 3 ? 1 : 0);
		}

		/// <summary>
		/// Tries fetching an item from a list with fallback in case index is out of bounds and option to null fetched value
		/// </summary>
		private T TryIndex<T>(List<T> list, int index, T fallback, bool nullOnSuccess = false)
		{
			if (index >= list.Count || index < 0)
				return fallback;
			T item = list[index];
			if (nullOnSuccess)
				list[index] = default(T);
			return item;
		}

		#endregion
	}
}
