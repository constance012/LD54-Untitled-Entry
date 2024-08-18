using System;
using UnityEngine;

public enum CursorTextureType { Default, Crosshair }

public class CursorManager : PersistentSingleton<CursorManager>
{
	[Serializable]
	public struct CustomCursor
	{
		public CursorTextureType type;

		public Texture2D texture;

		[Tooltip("The normalized position of the texture to be used as a hotspot, as (0, 0) will be at the top-left corner of the texture.")]
		public Vector2 relativeHotSpot;

		public Vector2 TextureHotSpot => new Vector2(texture.width * relativeHotSpot.x, texture.height * relativeHotSpot.y);
	}

	[Header("Custom Cursors"), Space]
	[SerializeField] private CustomCursor defaultCursor;
	[SerializeField] private CustomCursor crosshairCursor;
	
	private void Start()
	{
		SwitchCursorTexture(CursorTextureType.Default);
	}

	public void SwitchCursorTexture(CursorTextureType type, CursorMode mode = CursorMode.Auto)
	{
		switch (type)
		{
			case CursorTextureType.Default:
				Cursor.SetCursor(defaultCursor.texture, defaultCursor.TextureHotSpot, mode);
				break;

			case CursorTextureType.Crosshair:
				Cursor.SetCursor(crosshairCursor.texture, crosshairCursor.TextureHotSpot, mode);
				break;
		}
	}

	public void SetVisible(bool visible)
	{
		Cursor.visible = visible;
	}
}
