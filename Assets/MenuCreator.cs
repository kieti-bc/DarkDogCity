using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuCreator
{
	private Vector2Int drawPosition;
	private Vector2Int elementSize;

	public MenuCreator(int x, int y, int width, int height)
	{
		drawPosition = new Vector2Int(x, y);
		elementSize = new Vector2Int(width, height);
	}

	public void Label(string text)
	{
		GUI.Label(new Rect(drawPosition, elementSize), text);
		drawPosition.y += elementSize.y;
	}
	public void CheckBox(string text, bool value)
	{
		GUI.Toggle(new Rect(drawPosition, elementSize), value, text);
		drawPosition.y += elementSize.y;
	}
}
