using System;
using UnityEngine;
using UnityEngine.UI;

public class PaintCanvas
{
    private float _aspectRatio;
    private Vector2 _position;
    private Rect _rect;
    private Texture2D _texture;

    public float aspectRatio
    {
        get { return _aspectRatio; }
        set
        {
            _aspectRatio = value;
            _rect.height = _rect.width * aspectRatio;
        }
    }

    public Vector2 position
    {
        get { return _position; }
        set { _position = value; }
    }

    public Rect rect
    {
        get { return _rect; }
        set { _rect = value; }
    }

    public Texture2D texture
    {
        get { return _texture; }
        set { _texture = value; }
    }

    public PaintCanvas()
    {
        position = Vector2.zero;
        rect = new Rect(position.x, position.y, 256, 256);
        aspectRatio = rect.width / rect.height;

        texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
        texture.alphaIsTransparency = true;

        Color[] textureColors = new Color[texture.width * texture.height];
        Array.Fill(textureColors, new Color(0, 0, 0, 0));
        texture.SetPixels(textureColors);
        texture.Apply();
    }

    public PaintCanvas(Rect rect)
    {
        this.rect = rect;
        position = rect.position;
        aspectRatio = rect.width / rect.height;

        texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
        texture.alphaIsTransparency = true;

        Color[] textureColors = new Color[texture.width * texture.height];
        Array.Fill(textureColors, new Color(0, 0, 0, 0));
        texture.SetPixels(textureColors);
        texture.Apply();
    }

    public PaintCanvas(Rect rect, Texture2D texture)
    {
        this.rect = rect;
        position = rect.position;
        aspectRatio = rect.width / rect.height;

        this.texture = texture;
        texture.alphaIsTransparency = true;

        Color[] textureColors = new Color[texture.width * texture.height];
        Array.Fill(textureColors, new Color(0, 0, 0, 0));
        texture.SetPixels(textureColors);
        texture.Apply();
    }

    public PaintCanvas(float x, float y, float width, float height)
    {
        position = new Vector2(x, y);
        rect = new Rect(position.x, position.y, width, height);

        texture = new Texture2D((int)rect.width, (int)rect.height, TextureFormat.ARGB32, true, false);
        texture.alphaIsTransparency = true;

        Color[] textureColors = new Color[texture.width * texture.height];
        Array.Fill(textureColors, new Color(0, 0, 0, 0));
        texture.SetPixels(textureColors);
        texture.Apply();
    }

    public PaintCanvas(float x, float y, float width, float height, Texture2D texture)
    {
        position = new Vector2(x, y);
        rect = new Rect(position.x, position.y, width, height);

        this.texture = texture;
        texture.alphaIsTransparency = true;

        Color[] textureColors = new Color[texture.width * texture.height];
        Array.Fill(textureColors, new Color(0, 0, 0, 0));
        texture.SetPixels(textureColors);
        texture.Apply();
    }
}
