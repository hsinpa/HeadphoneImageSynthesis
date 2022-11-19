using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class TextureUtility
{
    TextureStructure _textureStructure = new TextureStructure();

    private const float degreeToRadian = Mathf.PI / 180;

    public TextureStructure GrabTextureRadius(int p_width, int p_height, float ratio)
    {
        int criteria = (p_width > p_height) ? p_height : p_width;
        int radius = (int)((criteria * ratio) / 2f);
        int size = radius * 2;
        Vector2Int center = new Vector2Int((int)(p_width / 2f), (int)(p_height / 2f));

        _textureStructure.width = size;
        _textureStructure.height = size;
        _textureStructure.x = center.x - radius;
        _textureStructure.y = center.y - radius;

        _textureStructure.xRatio = (float)size / p_width;
        _textureStructure.yRatio = (float)size / p_height;

        _textureStructure.xResidualRatio = 1 - _textureStructure.xRatio;
        _textureStructure.yResidualRatio = 1 - _textureStructure.yRatio;

        return _textureStructure;
    }

    public struct TextureStructure
    {
        public int width;
        public int height;
        public int x;
        public int y;

        public float xRatio;
        public float yRatio;
        public float xResidualRatio;
        public float yResidualRatio;
    }

    public static RenderTexture GetRenderTexture(int size)
    {
        return GetRenderTexture(size, size, depth:0, RenderTextureFormat.ARGB32);
    }

    public static RenderTexture GetRenderTexture(int width, int height, int depth, RenderTextureFormat format)
    {
        var rt = new RenderTexture(width, height, depth, format);
        
        rt.Create();
        return rt;
    }


    public static RenderTexture RotateAndScaleImage(Texture p_texture, RenderTexture renderer, Material rotateMat, TextureUtility.TextureStructure textureSetting, int degree)
    {
        float radian = degreeToRadian * degree;

        rotateMat.SetFloat("_EnlargeX", textureSetting.xRatio);
        rotateMat.SetFloat("_EnlargeY", textureSetting.yRatio);
        rotateMat.SetFloat("_Rotation", radian);

        Graphics.Blit(p_texture, renderer, rotateMat, 0);

        return renderer;
    }

    public static Texture2D TextureToTexture2D(RenderTexture renderTex)
    {
        Texture2D texture2D = new Texture2D(renderTex.width, renderTex.height, TextureFormat.RGBA32, false);
        Rect rectTemplate = new Rect(0, 0, renderTex.width, renderTex.height);

        RenderTexture.active = renderTex;
        texture2D.ReadPixels(rectTemplate, 0, 0);
        texture2D.Apply();
        texture2D.hideFlags = HideFlags.HideAndDontSave;

        return texture2D;
    }

    public static RenderTexture TextureToRenderTexture(Texture texture) {
        RenderTexture rt = new RenderTexture(texture.width, texture.height, 0);
        RenderTexture.active = rt;
        // Copy your texture ref to the render texture
        Graphics.Blit(texture, rt);

        return rt;
    }

    public static Texture2D GetTexture2DFromPath(string path) {
        if (!File.Exists(path)) return null;

        byte[] pngBytes = System.IO.File.ReadAllBytes(path);

        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(pngBytes);

        return tex;
    }

    public static void Dispose2D(Texture2D t) {
        Object.Destroy(t);
    }


    public struct RaycastResult
    {
        public Vector3 hitPoint;
        public Quaternion hitRotation;
        public bool hasHit;
    }
}
