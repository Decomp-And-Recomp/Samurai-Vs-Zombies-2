using System;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Glui/Text")]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class GluiText : GluiWidget
{
	public class Glyph
	{
		public GluiFont.Glyph glyph;

		public Vector3 position;

		public Vector3 rotation;

		public Vector3 scale;

		public Color color;

		public bool rendered;

		public Glyph()
		{
			glyph = new GluiFont.Glyph();
			position = Vector3.zero;
			rotation = Vector3.zero;
			scale = Vector3.one;
			color = Color.white;
			rendered = false;
		}

		public float Width()
		{
			return (float)glyph.width * scale.x;
		}

		public float Height()
		{
			return (float)glyph.height * scale.y;
		}

		public float xAdvance()
		{
			return (float)glyph.xAdvance * scale.x;
		}

		public float xOffset()
		{
			return (float)glyph.xOffset * scale.x;
		}

		public float yOffset()
		{
			return (float)glyph.yOffset * scale.y;
		}

		public float Kerning(ushort next)
		{
			short value;
			if (glyph.kernings != null && glyph.kernings.TryGetValue(next, out value))
			{
				return (float)value * scale.y;
			}
			return 0f;
		}

		private Vector3 GlyphSpace(float x, float y, float z)
		{
			return Vector3.Scale(new Vector3(x, y, z), scale);
		}
	}

	public class TextBlock
	{
		public int first;

		public int last;

		public int trailing;

		public float width;

		public float trailingOffset;

		public int Length()
		{
			return last - first + 1;
		}

		public string ToString(string WholeText)
		{
			if (first + Length() > WholeText.Length)
			{
				return string.Empty;
			}
			if (Length() > 0)
			{
				return WholeText.Substring(first, Length());
			}
			return string.Empty;
		}
	}

	public class LineTextBlock : TextBlock
	{
		public int firstWord;

		public int lastWord;
	}

	public enum Alignment
	{
		Left = 0,
		Center = 1,
		Right = 2,
		Justified = 3
	}

	public class Tag : TextBlock
	{
		public enum TagType
		{
			None = 0,
			Color = 1,
			NewLine = 2,
			Image = 3,
			EndTag = 4
		}

		public TagType tagType;

		public Color color = Color.magenta;

		public GameObject spriteGOB;

		public int firstActive = -1;

		public int lastActive = -1;

		public bool ParseColor(string text, int startIndex)
		{
			tagType = TagType.Color;
			first = startIndex;
			last = startIndex + "<COLOR=".Length + 9 - 1;
			if (text.Length <= last || text[last] != '>')
			{
				return false;
			}
			string value = text.Substring(startIndex + "<COLOR=".Length, 2);
			string value2 = text.Substring(startIndex + "<COLOR=".Length + 2, 2);
			string value3 = text.Substring(startIndex + "<COLOR=".Length + 4, 2);
			string value4 = text.Substring(startIndex + "<COLOR=".Length + 6, 2);
			try
			{
				int num = Convert.ToInt32(value, 16);
				int num2 = Convert.ToInt32(value2, 16);
				int num3 = Convert.ToInt32(value3, 16);
				int num4 = Convert.ToInt32(value4, 16);
				color = new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f, (float)num4 / 255f);
			}
			catch (FormatException)
			{
				return false;
			}
			catch (ArgumentException)
			{
				return false;
			}
			return true;
		}

		public bool ParseImage(string text, int startIndex)
		{
			if (DataBundleRuntime.Instance == null)
			{
				return false;
			}
			tagType = TagType.Image;
			first = startIndex;
			last = -1;
			int num = startIndex + "<IMG=".Length;
			for (int i = num; i < text.Length; i++)
			{
				if (text[i] == '>')
				{
					last = i;
					break;
				}
			}
			if (last == -1)
			{
				return false;
			}
			int num2 = last - num;
			if (num2 > 0)
			{
				string recordKey = text.Substring(num, num2);
				DataBundleRecordHandle<GluiText_ImageSchema> dataBundleRecordHandle = new DataBundleRecordHandle<GluiText_ImageSchema>("ImageTags", recordKey);
				if (dataBundleRecordHandle != null && dataBundleRecordHandle.Data != null)
				{
					dataBundleRecordHandle.Data.Initialize("ImageTags");
					SharedResourceLoader.SharedResource sharedResource = ResourceCache.Cache(dataBundleRecordHandle.Data.IconPath);
					if (sharedResource != null)
					{
						spriteGOB = new GameObject();
						spriteGOB.name = "GluiSprite: " + dataBundleRecordHandle.Data.id;
						GluiSprite gluiSprite = spriteGOB.AddComponent<GluiSprite>();
						gluiSprite.Texture = sharedResource.Resource as Texture2D;
						return true;
					}
				}
			}
			return false;
		}
	}

	public delegate void TextChangedAction(ref string textToModify, string originalText);

	public const string NewLineTag = "<N>";

	public const string ImageTag = "<IMG=";

	public const string ColorTag = "<COLOR=";

	public const string TagOut = "/>";

	private static char kFallbackGlyph = ' ';

	[SerializeField]
	private string fontName = string.Empty;

	[SerializeField]
	private Alignment justification = Alignment.Center;

	[SerializeField]
	private AnchorType verticalAnchor;

	[SerializeField]
	private bool localize = true;

	[SerializeField]
	private float glyphScale = 1f;

	[SerializeField]
	private int kerningOffset;

	[SerializeField]
	private int leadingOffset;

	[SerializeField]
	private string text = string.Empty;

	[SerializeField]
	private string taggedStringReference = string.Empty;

	[SerializeField]
	private bool wordWrap = true;

	[SerializeField]
	private bool wordTrim;

	[SerializeField]
	private bool shrink = true;

	[SerializeField]
	private bool dropshadow;

	[SerializeField]
	private bool dontCreateInitialText;

	[SerializeField]
	private bool initialTextChanged;

	[SerializeField]
	private Vector3 shadowOffset = new Vector3(2f, -2f, 1f);

	[SerializeField]
	private GameObject shadowGOB;

	private Mesh shadowMesh;

	[SerializeField]
	private Color shadowColor = Color.black;

	protected Texture2D fontTexture;

	private bool redrawing;

	private bool outline;

	public bool kernText;

	public bool ignoreNewlineTags;

	private float unformattedLength;

	public int renderedGlyphs;

	public Glyph[] Glyphs;

	public List<TextBlock> Words;

	public List<Tag> Tags = new List<Tag>();

	public List<LineTextBlock> Lines;

	public int InsertionPoints;

	public GluiFont font;

	public Vector3 fontScale;

	private Vector3[] vertices;

	private Color[] colors;

	private Vector2[] uv1s;

	private Vector2[] uv2s;

	private int[] triangles;

	private Vector2[] channels;

	protected string renderString;

	private bool mTextSetManually;

	private Mesh shadowMeshBufferA;

	private int failed;

	private static Vector2[] ChannelGlyphMap = new Vector2[9]
	{
		Vector2.zero,
		new Vector2(0.625f, 0f),
		new Vector2(0.375f, 0f),
		Vector2.zero,
		new Vector2(0.125f, 0f),
		Vector2.zero,
		Vector2.zero,
		Vector2.zero,
		new Vector2(0.875f, 0f)
	};

	public TextChangedAction onTextChanged;

	private Mesh meshBufferA;

	private Mesh meshBufferB;

	private bool mbuffer;

	public string FontName
	{
		get
		{
			return fontName;
		}
		set
		{
			if (fontName != value)
			{
				fontName = value;
				LoadFont(true);
				UpdateText();
			}
		}
	}

	public Alignment Justification
	{
		get
		{
			return justification;
		}
		set
		{
			if (justification != value)
			{
				justification = value;
				UpdateText();
			}
		}
	}

	public AnchorType VerticalAnchor
	{
		get
		{
			return verticalAnchor;
		}
		set
		{
			if (verticalAnchor != value)
			{
				verticalAnchor = value;
				UpdateText();
			}
		}
	}

	public bool Localize
	{
		get
		{
			return localize;
		}
		set
		{
			if (localize != value)
			{
				localize = value;
				UpdateText();
			}
		}
	}

	public float GlyphScale
	{
		get
		{
			return glyphScale;
		}
		set
		{
			if (glyphScale != value)
			{
				glyphScale = value;
				UpdateText();
			}
		}
	}

	public int KerningOffset
	{
		get
		{
			return kerningOffset;
		}
		set
		{
			if (kerningOffset != value)
			{
				kerningOffset = value;
				UpdateText();
			}
		}
	}

	public int LeadingOffset
	{
		get
		{
			return leadingOffset;
		}
		set
		{
			if (leadingOffset != value)
			{
				leadingOffset = value;
				UpdateText();
			}
		}
	}

	public string TaggedStringReference
	{
		get
		{
			return taggedStringReference;
		}
		set
		{
			taggedStringReference = value;
			mTextSetManually = false;
			UpdateText();
		}
	}

	public string Text
	{
		get
		{
			return text;
		}
		set
		{
			SetText(value);
		}
	}

	public bool WordWrap
	{
		get
		{
			return wordWrap;
		}
		set
		{
			if (wordWrap != value)
			{
				wordWrap = value;
				UpdateText();
			}
		}
	}

	public bool WordTrim
	{
		get
		{
			return wordTrim;
		}
		set
		{
			if (wordTrim != value)
			{
				wordTrim = value;
				UpdateText();
			}
		}
	}

	public bool Shrink
	{
		get
		{
			return shrink;
		}
		set
		{
			if (shrink != value)
			{
				shrink = value;
				UpdateText();
			}
		}
	}

	public bool DropShadow
	{
		get
		{
			return dropshadow;
		}
		set
		{
			if (dropshadow != value)
			{
				dropshadow = value;
				UpdateShadow();
			}
		}
	}

	public Vector3 DropShadowOffset
	{
		get
		{
			return shadowOffset;
		}
		set
		{
			shadowOffset = value;
			if (shadowGOB != null)
			{
				shadowGOB.transform.localPosition = shadowOffset;
			}
		}
	}

	public Color ShadowColor
	{
		get
		{
			return shadowColor;
		}
		set
		{
			if (shadowColor != value)
			{
				shadowColor = value;
				UpdateShadowMeshColors();
			}
		}
	}

	public bool DontCreateInitialText
	{
		get
		{
			return dontCreateInitialText;
		}
		set
		{
			dontCreateInitialText = value;
		}
	}

	public override Texture2D Texture
	{
		get
		{
			return fontTexture;
		}
		set
		{
			if (fontTexture != value)
			{
				fontTexture = value;
				UpdateMaterial(true);
				OnTextureChanged();
			}
		}
	}

	public int TextLength
	{
		get
		{
			return (int)unformattedLength;
		}
	}

	public override Color Color
	{
		get
		{
			return base.Color;
		}
		set
		{
			if (!(color != value))
			{
				return;
			}
			base.Color = value;
			if (colors == null)
			{
				return;
			}
			for (int i = 0; i < colors.Length; i++)
			{
				colors[i] = value;
			}
			if (Application.isPlaying)
			{
				if (mbuffer)
				{
					meshBufferA.colors = colors;
				}
				else
				{
					meshBufferB.colors = colors;
				}
			}
			else
			{
				mesh.colors = colors;
			}
		}
	}

	public override IEnumerable<Color> Colors
	{
		get
		{
			return new Color[2] { Color, shadowColor };
		}
		set
		{
			if (value == null)
			{
				return;
			}
			IEnumerator<Color> enumerator = value.GetEnumerator();
			if (enumerator != null)
			{
				if (enumerator.MoveNext())
				{
					Color = enumerator.Current;
				}
				if (enumerator.MoveNext())
				{
					shadowColor = enumerator.Current;
					UpdateShadowMeshColors();
				}
			}
		}
	}

	protected override void OnTextureChanged()
	{
		base.OnTextureChanged();
	}

	protected override void UpdateMaterial(bool replace)
	{
		base.UpdateMaterial(replace);
		if (font == null)
		{
			LoadFont();
		}
		if (font != null && font.isPacked)
		{
			SharedResourceLoader.SharedResource sharedResource = ResourceCache.Cache("Framework/GluiCore/Resources/Textures/Glui4x8bitChannelLookup");
			if (sharedResource != null && material != null)
			{
				material.SetTexture("_ChannelLookup", sharedResource.Resource as Texture2D);
			}
		}
	}

	protected override void OnInit()
	{
		GluiLocalizationSupport.Init();
	}

	protected override void OnResize()
	{
		UpdateText();
	}

	protected override void OnEnableChanged()
	{
		UpdateMaterial(false);
		base.OnEnableChanged();
	}

	protected override void OnCreate()
	{
		GluiTexInit();
	}

	protected void GluiTexInit()
	{
		InitWidget();
		UpdateText();
	}

	private void UpdateText()
	{
		if (!dontCreateInitialText || initialTextChanged || !Application.isPlaying)
		{
			renderString = UpdateLoc();
			if (!redrawing)
			{
				redrawing = true;
				LoadFont();
				CreateTextTags(renderString);
				CreateTextWords(renderString);
				CreateTextLines();
				CreateTextMesh();
				Texture = font.texture;
				UpdateShadow();
				redrawing = false;
			}
		}
	}

	private void UpdateShadow()
	{
		if (!dropshadow)
		{
			if (shadowGOB != null)
			{
				ObjectUtils.DestroyImmediate(shadowGOB);
			}
		}
		else if (vertices != null && vertices.Length >= 1)
		{
			if (shadowGOB == null)
			{
				shadowGOB = new GameObject("dropshadow (auto generated)");
				shadowGOB.layer = base.gameObject.layer;
				shadowGOB.transform.parent = base.transform;
				shadowGOB.transform.localEulerAngles = Vector3.zero;
				shadowGOB.transform.localScale = Vector3.one;
				shadowGOB.transform.localPosition = DropShadowOffset;
				shadowGOB.AddComponent<MeshFilter>();
				MeshRenderer meshRenderer = shadowGOB.AddComponent<MeshRenderer>();
				meshRenderer.receiveShadows = false;
				meshRenderer.castShadows = false;
			}
			if (shadowMesh == null)
			{
				shadowMesh = new Mesh();
				shadowMesh.name = "dropshadow mesh (auto generated)";
			}
			shadowGOB.GetComponent<MeshRenderer>().GetComponent<Renderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
			if (shadowMesh.vertices != null && shadowMesh.vertices.Length != vertices.Length)
			{
				shadowMesh.Clear();
			}
			if (shadowMesh != null)
			{
				shadowMesh.vertices = vertices;
				shadowMesh.uv = uv1s;
				shadowMesh.uv2 = channels;
				UpdateShadowMeshColors();
				shadowMesh.triangles = triangles;
				shadowGOB.GetComponent<MeshFilter>().sharedMesh = shadowMesh;
			}
		}
	}

	private void UpdateShadowMeshColors()
	{
		if (shadowMesh != null && colors != null)
		{
			Color[] array = new Color[colors.Length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = shadowColor;
			}
			shadowMesh.colors = array;
			if (array.Length == vertices.Length)
			{
			}
		}
	}

	private Tag IsInsideTag(int glyphIndex)
	{
		foreach (Tag tag in Tags)
		{
			if (tag.first <= glyphIndex && tag.last >= glyphIndex)
			{
				return tag;
			}
		}
		return null;
	}

	private void CreateTextTags(string textStr)
	{
		foreach (Tag tag6 in Tags)
		{
			if (tag6.spriteGOB != null)
			{
				ObjectUtils.DestroyImmediate(tag6.spriteGOB);
			}
		}
		if (Tags != null)
		{
			Tags.Clear();
		}
		for (int i = 0; i < textStr.Length; i++)
		{
			if (TagMatches(textStr, i, "<N>"))
			{
				Tag tag = new Tag();
				tag.tagType = Tag.TagType.NewLine;
				tag.first = i;
				tag.last = tag.first + 2;
				while (textStr.Length > tag.last + 1 && char.IsWhiteSpace(textStr[tag.last + 1]))
				{
					tag.last++;
				}
				Tags.Add(tag);
			}
			if (TagMatches(textStr, i, "<COLOR="))
			{
				Tag tag2 = new Tag();
				if (tag2.ParseColor(textStr, i))
				{
					tag2.firstActive = i;
					Tags.Add(tag2);
				}
			}
			if (TagMatches(textStr, i, "<IMG="))
			{
				Tag tag3 = new Tag();
				if (tag3.ParseImage(textStr, i))
				{
					Tags.Add(tag3);
					tag3.spriteGOB.transform.parent = base.transform;
				}
			}
			if (!TagMatches(textStr, i, "/>") || Tags.Count <= 0)
			{
				continue;
			}
			Tag tag4 = new Tag();
			tag4.tagType = Tag.TagType.EndTag;
			tag4.first = i;
			tag4.last = tag4.first + "/>".Length - 1;
			Tags.Add(tag4);
			for (int num = Tags.Count - 2; num >= 0; num--)
			{
				Tag tag5 = Tags[num];
				if (tag5.tagType == Tag.TagType.Color && tag5.lastActive == -1)
				{
					tag5.lastActive = i;
					break;
				}
			}
		}
	}

	private Color GetTagColor(int glyphIndex)
	{
		for (int i = 0; i < Tags.Count; i++)
		{
			Tag tag = Tags[i];
			if (tag.first > glyphIndex)
			{
				break;
			}
			if (tag.tagType == Tag.TagType.Color && tag.firstActive <= glyphIndex && tag.lastActive >= glyphIndex)
			{
				return tag.color;
			}
		}
		return Color;
	}

	private void CreateTextWords(string textStr)
	{
		if (textStr == null)
		{
			return;
		}
		if (Glyphs == null || Glyphs.Length < textStr.Length)
		{
			Glyphs = new Glyph[textStr.Length];
		}
		for (int i = 0; i < Glyphs.Length; i++)
		{
			Glyphs[i] = new Glyph();
		}
		renderedGlyphs = 0;
		int num = 0;
		int num2 = 0;
		float num3 = 0f;
		Words = new List<TextBlock>();
		Words.Add(new TextBlock());
		unformattedLength = 0f;
		bool isSpecialSplitLanguage = GluiLocalizationSupport.IsSpecialSplitLanguage();
		float num4 = 94.34f;
		Vector3 one = Vector3.one;
		float num5 = glyphScale;
		float lineBase = font.lineBase;
		Vector4 glyphPadding = font.glyphPadding;
		float num6 = lineBase - glyphPadding.x;
		Vector4 glyphPadding2 = font.glyphPadding;
		fontScale = one * (num5 / (num6 - glyphPadding2.z)) * num4;
		for (int j = 0; j < textStr.Length; j++)
		{
			Glyph glyph = Glyphs[j];
			glyph.scale = fontScale;
			Tag tag = IsInsideTag(j);
			if (tag == null)
			{
				glyph.color = GetTagColor(j);
			}
			if (tag != null && tag.tagType == Tag.TagType.Image && tag.spriteGOB != null)
			{
				tag.spriteGOB.GetComponent<GluiSprite>().Color = GetTagColor(j);
			}
			GluiFont.Glyph value;
			if (tag != null)
			{
				glyph.glyph = new GluiFont.Glyph();
				glyph.glyph.id = textStr[j];
				glyph.scale = fontScale;
			}
			else if (font.glyphs.TryGetValue(textStr[j], out value))
			{
				glyph.glyph = value;
			}
			else if (font.glyphs.TryGetValue(kFallbackGlyph, out value))
			{
				glyph.glyph = value;
			}
			if (tag != null)
			{
				glyph.rendered = false;
				if (!ignoreNewlineTags && tag.tagType == Tag.TagType.NewLine && tag.first == j)
				{
					Words[num].trailing = num2;
					Words[num].trailingOffset = num3;
					Words[num].last = j - 1;
					num3 = 0f;
					num2 = 0;
					Words.Add(new TextBlock());
					num++;
					Words[num].first = j;
					Words[num].last = tag.last;
					Words.Add(new TextBlock());
					num++;
					Words[num].first = tag.last + 1;
				}
				if (tag.tagType == Tag.TagType.Image && tag.first == j)
				{
					font.glyphs.TryGetValue(32, out value);
					glyph.glyph = new GluiFont.Glyph();
					glyph.glyph.width = value.width;
					glyph.glyph.xAdvance = (short)(num4 / 10f * (float)(glyph.glyph.width + 1));
					glyph.glyph.xOffset = (short)((float)glyph.glyph.xAdvance / 2f);
					glyph.glyph.yOffset = value.yOffset;
					float num7 = num4 / 10f * (float)glyph.glyph.width * fontScale.x;
					tag.spriteGOB.GetComponent<GluiSprite>().Size = new Vector2(num7, num7);
					Words[num].width += glyph.xAdvance();
				}
			}
			else if (char.IsWhiteSpace(textStr[j]))
			{
				glyph.rendered = false;
				num2++;
				num3 += glyph.xAdvance();
			}
			else
			{
				if (num2 > 0)
				{
					Words[num].trailing = num2;
					Words[num].trailingOffset = num3;
					Words[num].last = j - 1;
					num3 = 0f;
					num2 = 0;
					Words.Add(new TextBlock());
					num++;
					Words[num].first = j;
				}
				else if (j > 0 && CanSpecialLanguageSplit(textStr[j - 1], textStr[j], isSpecialSplitLanguage))
				{
					Words[num].trailing = 0;
					Words[num].trailingOffset = 0f;
					Words[num].last = j - 1;
					num3 = 0f;
					num2 = 0;
					Words.Add(new TextBlock());
					num++;
					Words[num].first = j;
				}
				Words[num].width += glyph.xAdvance();
				if (kernText && glyph.glyph != null && j < Glyphs.Length - 1)
				{
					Words[num].width += glyph.Kerning(Glyphs[j + 1].glyph.id);
				}
				glyph.rendered = true;
				renderedGlyphs++;
			}
		}
		Words[num].last = textStr.Length - 1;
		for (int k = 0; k < Words.Count; k++)
		{
			unformattedLength += Words[k].width + Words[k].trailingOffset;
		}
		failed = 0;
	}

	private void RescaleText(float factor)
	{
		Glyph[] glyphs = Glyphs;
		foreach (Glyph glyph in glyphs)
		{
			glyph.scale *= factor;
		}
		foreach (TextBlock word in Words)
		{
			word.width *= factor;
			word.trailingOffset *= factor;
		}
		foreach (LineTextBlock line in Lines)
		{
			line.width *= factor;
		}
		unformattedLength *= factor;
		fontScale *= factor;
	}

	private void CreateTextLines()
	{
		float num = font.lineHeight * fontScale.y + (float)leadingOffset;
		int num2 = 1;
		if (num > 0f)
		{
			num2 = Mathf.FloorToInt(base.Size.y / num);
		}
		if (num2 <= 0)
		{
			num2 = 1;
		}
		Lines = new List<LineTextBlock>();
		Lines.Add(new LineTextBlock());
		Lines[0].first = 0;
		Lines[0].firstWord = 0;
		int num3 = 0;
		for (int i = 0; i < Words.Count; i++)
		{
			TextBlock textBlock = Words[i];
			if (textBlock.ToString(renderString).Contains("<N>") && !ignoreNewlineTags)
			{
				Lines[num3].last = textBlock.last;
				Lines[num3].lastWord = i;
				Lines.Add(new LineTextBlock());
				num3++;
				Lines[num3].first = textBlock.last + 1;
				Lines[num3].firstWord = i + 1;
			}
			else
			{
				Lines[num3].width += textBlock.width + textBlock.trailingOffset;
				Lines[num3].last = textBlock.last;
				Lines[num3].lastWord = i;
			}
		}
		if (WordWrap)
		{
			for (int j = 0; j < Lines.Count; j++)
			{
				LineTextBlock lineTextBlock = Lines[j];
				if (!(lineTextBlock.width > base.Size.x))
				{
					continue;
				}
				float num4 = 0f;
				for (int k = lineTextBlock.firstWord; k <= lineTextBlock.lastWord; k++)
				{
					TextBlock textBlock2 = Words[k];
					num4 += textBlock2.width + textBlock2.trailingOffset;
					if (num4 > base.Size.x && k != lineTextBlock.firstWord)
					{
						LineTextBlock lineTextBlock2 = new LineTextBlock();
						lineTextBlock2.firstWord = k;
						lineTextBlock2.first = textBlock2.first;
						lineTextBlock2.last = lineTextBlock.last;
						lineTextBlock2.lastWord = lineTextBlock.lastWord;
						lineTextBlock2.width = lineTextBlock.width - num4 + textBlock2.width + textBlock2.trailingOffset;
						lineTextBlock.last = lineTextBlock2.first - 1;
						lineTextBlock.lastWord = lineTextBlock2.firstWord - 1;
						lineTextBlock.width -= lineTextBlock2.width;
						Lines.Insert(j + 1, lineTextBlock2);
					}
				}
			}
		}
		foreach (LineTextBlock line in Lines)
		{
			if (line.lastWord < Words.Count)
			{
				line.width -= Words[line.lastWord].trailingOffset;
			}
		}
		if (WordTrim && !Shrink)
		{
			Trim(num2);
		}
		if (Shrink)
		{
			if (WordWrap && Lines.Count > num2)
			{
				if ((float)Lines.Count / 2f > (float)num2)
				{
					RescaleText(0.85f);
				}
				else if (Lines[Lines.Count - 1].width > base.Size.x / 2f)
				{
					RescaleText(0.95f);
				}
				else
				{
					RescaleText(0.98f);
				}
				failed++;
				CreateTextLines();
				return;
			}
			float num5 = 0f;
			foreach (LineTextBlock line2 in Lines)
			{
				if (line2.width > num5)
				{
					num5 = line2.width;
				}
			}
			if (num5 > 0f && num5 > base.Size.x)
			{
				RescaleText(base.Size.x / num5);
				failed++;
				CreateTextLines();
				return;
			}
		}
		float num6 = 1f;
		if (Glyphs.Length > 0)
		{
			num6 = Glyphs[0].scale.y;
		}
		float num7 = font.lineHeight * num6 + (float)leadingOffset;
		for (num3 = 0; num3 < Lines.Count; num3++)
		{
			Vector3 zero = Vector3.zero;
			zero.x = LineReturn(num3);
			zero.y = AnchorVertical() - (float)num3 * num7;
			LineTextBlock lineTextBlock3 = Lines[num3];
			int num8 = lineTextBlock3.firstWord;
			int num9 = lineTextBlock3.lastWord - lineTextBlock3.firstWord;
			for (int l = lineTextBlock3.first; l <= lineTextBlock3.last; l++)
			{
				Glyph glyph = Glyphs[l];
				if (num8 < Words.Count && l > Words[num8].last)
				{
					if (justification == Alignment.Justified)
					{
						zero.x += (base.Size.x - lineTextBlock3.width) / (float)num9;
					}
					num8++;
				}
				if (l <= Words[lineTextBlock3.lastWord].last)
				{
					glyph.position = zero + glyph.xOffset() * Vector3.right;
					Tag tag = IsInsideTag(l);
					if (tag != null && tag.tagType == Tag.TagType.Image && tag.first == l)
					{
						tag.spriteGOB.transform.position = glyph.position + base.transform.position;
					}
					zero += Vector3.right * (glyph.xAdvance() + (float)kerningOffset);
					if (kernText && Glyphs[l].glyph != null && l + 1 < Glyphs.Length)
					{
						zero += Vector3.right * glyph.Kerning(Glyphs[l + 1].glyph.id);
					}
				}
				else
				{
					glyph.rendered = false;
				}
			}
		}
		if (failed <= 0)
		{
		}
	}

	private void Trim(int maxLines)
	{
		for (int i = 0; i < Lines.Count; i++)
		{
			LineTextBlock lineTextBlock = Lines[i];
			float num = 0f;
			for (int j = lineTextBlock.firstWord; j <= lineTextBlock.lastWord; j++)
			{
				TextBlock textBlock = Words[j];
				if (num + textBlock.width + textBlock.trailingOffset >= base.Size.x || (j == lineTextBlock.lastWord && i + 1 < Lines.Count && i + 1 >= maxLines))
				{
					Glyph glyph = new Glyph();
					if (!font.glyphs.TryGetValue(8230, out glyph.glyph))
					{
						font.glyphs.TryGetValue(45, out glyph.glyph);
					}
					if (glyph != null)
					{
						int first = textBlock.first;
						glyph.scale = Glyphs[first].scale;
						lineTextBlock.width = num + glyph.xAdvance();
						Glyphs[first].glyph = glyph.glyph;
						Glyphs[first].rendered = true;
						textBlock.last = first;
						for (int k = first + 1; k <= lineTextBlock.last; k++)
						{
							Glyphs[k].rendered = false;
						}
						lineTextBlock.last = first;
						lineTextBlock.lastWord = j;
					}
					break;
				}
				num += textBlock.width + textBlock.trailingOffset;
			}
		}
		while (Lines.Count > maxLines)
		{
			int first2 = Lines[Lines.Count - 1].first;
			int last = Lines[Lines.Count - 1].last;
			for (int l = first2; l <= last; l++)
			{
				Glyphs[l].rendered = false;
			}
			Lines.RemoveAt(Lines.Count - 1);
		}
	}

	private float LineReturn(int index)
	{
		if (justification == Alignment.Center)
		{
			return (0f - Lines[index].width) / 2f;
		}
		if (justification == Alignment.Left || justification == Alignment.Justified)
		{
			return (0f - base.Size.x) / 2f;
		}
		return base.Size.x / 2f - Lines[index].width;
	}

	private float AnchorVertical()
	{
		float num = 1f;
		for (int i = 0; i < Glyphs.Length; i++)
		{
			if (IsInsideTag(i) == null)
			{
				num = Glyphs[i].scale.y;
				break;
			}
		}
		if (verticalAnchor == AnchorType.Positive)
		{
			return base.Size.y / 2f - font.lineBase * num;
		}
		if (verticalAnchor == AnchorType.Negative)
		{
			return (0f - base.Size.y) / 2f + (font.lineHeight * num + (float)leadingOffset) * ((float)Lines.Count - 1f * (font.lineBase / font.lineHeight));
		}
		if (Lines.Count > 1)
		{
			return (font.lineHeight * num + (float)leadingOffset) * ((float)Lines.Count - 1f) / 2f;
		}
		return 0f;
	}

	private void CreateTextMesh()
	{
		if (vertices == null || vertices.Length < renderedGlyphs * 4)
		{
			vertices = new Vector3[renderedGlyphs * 4];
		}
		if (uv1s == null || uv1s.Length < renderedGlyphs * 4)
		{
			uv1s = new Vector2[renderedGlyphs * 4];
		}
		if (channels == null || channels.Length < renderedGlyphs * 4)
		{
			channels = new Vector2[renderedGlyphs * 4];
		}
		if (colors == null || colors.Length < renderedGlyphs * 4)
		{
			colors = new Color[renderedGlyphs * 4];
		}
		triangles = new int[renderedGlyphs * 6];
		int num = 0;
		for (int i = 0; i < Glyphs.Length; i++)
		{
			if (num >= renderedGlyphs)
			{
				break;
			}
			Glyph glyph = Glyphs[i];
			if (glyph.rendered)
			{
				MakeQuadVert(num, glyph.position.x, glyph.position.y - glyph.yOffset() - LineAnchor() * glyph.scale.y, glyph.Width(), 0f - glyph.Height());
				MakeQuadUV1(num, glyph.glyph.texPos.x, glyph.glyph.texPos.y, glyph.glyph.texArea.x, glyph.glyph.texArea.y);
				MakeQuadColors(num, glyph.color);
				MakeQuadTriangles(num);
				MakeQuadChannel(num, glyph.glyph.chnl);
				num++;
			}
		}
		UpdateMesh(vertices, uv1s, triangles, channels, null, colors);
	}

	private float LineAnchor()
	{
		if (verticalAnchor == AnchorType.None)
		{
			return (0f - font.lineHeight) / 2f;
		}
		return 0f - font.lineBase;
	}

	private void MakeQuadVert(int index, float x, float y, float w, float h)
	{
		vertices[index * 4] = new Vector3(x, y, 0f);
		vertices[index * 4 + 1] = new Vector3(x + w, y, 0f);
		vertices[index * 4 + 2] = new Vector3(x + w, y + h, 0f);
		vertices[index * 4 + 3] = new Vector3(x, y + h, 0f);
	}

	private void MakeQuadColors(int index, Color color)
	{
		colors[index * 4] = color;
		colors[index * 4 + 1] = color;
		colors[index * 4 + 2] = color;
		colors[index * 4 + 3] = color;
	}

	private void MakeQuadUV1(int index, float x, float y, float w, float h)
	{
		uv1s[index * 4 + 3] = new Vector2(x, y);
		uv1s[index * 4 + 2] = new Vector2(x + w, y);
		uv1s[index * 4 + 1] = new Vector2(x + w, y + h);
		uv1s[index * 4] = new Vector2(x, y + h);
	}

	private void MakeQuadTriangles(int index)
	{
		triangles[index * 6] = index * 4;
		triangles[index * 6 + 1] = index * 4 + 1;
		triangles[index * 6 + 2] = index * 4 + 2;
		triangles[index * 6 + 3] = index * 4 + 2;
		triangles[index * 6 + 4] = index * 4 + 3;
		triangles[index * 6 + 5] = index * 4;
	}

	private void MakeQuadChannel(int index, int chnl)
	{
		if (chnl > 8)
		{
			chnl = 8;
		}
		for (int i = 0; i < 4; i++)
		{
			channels[index * 4 + i] = ChannelGlyphMap[chnl];
		}
	}

	private void LoadFont(bool force = false)
	{
		if (Application.isPlaying && !force)
		{
			string systemLanguage = BundleUtils.GetSystemLanguage();
			GluiFont gluiFont = null;
			gluiFont = GluiSettings.LoadOverrideFontForLanguage(systemLanguage);
			if (gluiFont != null)
			{
				font = gluiFont;
			}
		}
		if (font == null || force)
		{
			if (string.IsNullOrEmpty(fontName))
			{
				if (GluiSettings.FontsConfig.Length <= 0)
				{
					return;
				}
				fontName = GluiSettings.FontsConfig[0].name;
			}
			if (!GluiSettings.Fonts.TryGetValue(fontName, out font))
			{
				font = GluiSettings.LoadFontByName(fontName);
				if (font == null)
				{
					if (GluiSettings.FontsConfig.Length <= 0)
					{
						return;
					}
					fontName = GluiSettings.FontsConfig[0].name;
					GluiSettings.Fonts.TryGetValue(fontName, out font);
				}
			}
		}
		if (font != null)
		{
			if (font.isPacked)
			{
				base.AutoShader = ShaderType.Auto_GluiText_AlphaBlend_VertexColor_4x8bitChannel;
			}
			else
			{
				base.AutoShader = ShaderType.Auto_GluiVertexLit;
			}
		}
	}

	private bool CanSpecialLanguageSplit(char prevChar, char currentChar, bool isSpecialSplitLanguage)
	{
		if (prevChar == '-')
		{
			return true;
		}
		if (IsAlphaNumeric(prevChar) && IsAlphaNumeric(currentChar))
		{
			return false;
		}
		if (isSpecialSplitLanguage)
		{
			return !GluiLocalizationSupport.CantEndCharacters.Contains(prevChar) && !GluiLocalizationSupport.CantBeginCharacters.Contains(currentChar);
		}
		return false;
	}

	private bool IsAlphaNumeric(char c)
	{
		return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9');
	}

	private string UpdateLoc()
	{
		if (mTextSetManually)
		{
			return text;
		}
		string result = null;
		if (Application.isPlaying)
		{
			result = ((!string.IsNullOrEmpty(taggedStringReference) && DataBundleRuntime.Instance != null) ? StringUtils.GetStringFromStringRef(taggedStringReference) : text);
		}
		else result = text; // FIX
		return result;
	}

	protected override void Awake()
	{
		gluiTexture.Texture = null;
		base.Awake();
	}

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(new Vector3(0f, 0f, 0f), new Vector3(base.Size.x, base.Size.y, 0f));
	}

	protected override void MeshErrorTest()
	{
	}

	private void SetText(string newText)
	{
		if (!GluiWidget.inspectorGUIUpdating && onTextChanged != null)
		{
			onTextChanged(ref newText, newText);
		}
		if ((dontCreateInitialText && !initialTextChanged) || !(text == newText))
		{
			text = newText;
			initialTextChanged = true;
			if (Application.isPlaying)
			{
				mTextSetManually = true;
			}
			UpdateText();
		}
	}

	protected void UpdateMesh(Vector3[] verts, Vector2[] uvs, int[] tris, Vector2[] chnl, Vector2[] uv2s, Color[] cols)
	{
		MeshFilter component = GetComponent<MeshFilter>();
		if (component == null)
		{
			return;
		}
		Mesh mesh = ((!Application.isPlaying || !mbuffer) ? meshBufferA : meshBufferB);
		if (mesh != null)
		{
			if (GluiWidget.cloneResources && Application.isEditor)
			{
				if (ownMesh)
				{
					ObjectUtils.DestroyImmediate(mesh);
				}
				mesh = new Mesh();
				ownMesh = true;
			}
		}
		else
		{
			mesh = new Mesh();
		}
		mesh.Clear();
		mesh.vertices = verts;
		if (cols != null)
		{
			mesh.colors = cols;
		}
		mesh.colors = cols;
		mesh.uv = uvs;
		if (uv2s != null)
		{
			mesh.uv2 = uv2s;
		}
		mesh.triangles = tris;
		if (chnl != null)
		{
			mesh.uv2 = chnl;
		}
		component.sharedMesh = mesh;
		if (Application.isPlaying)
		{
			if (mbuffer)
			{
				meshBufferB = mesh;
				mbuffer = false;
			}
			else
			{
				meshBufferA = mesh;
				mbuffer = true;
			}
		}
		else
		{
			base.mesh = mesh;
		}
	}

	private bool TagMatches(string text, int offset, string tag)
	{
		for (int i = 0; i < tag.Length; i++)
		{
			if (i + offset >= text.Length)
			{
				return false;
			}
			if (text[i + offset] != tag[i])
			{
				return false;
			}
		}
		return true;
	}
}
