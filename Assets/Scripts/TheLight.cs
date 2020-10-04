using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TheLight : MonoBehaviour
{
    public float goal = 60f;

    public float exp = 5f;
    public float minimum = .1f;
    public float slope = .15f;
    public int pixelsPerUnit = 8;
    public float fps = 12f;
    public Transform rowboat;

    public Action OnEscape;

    private AudioSource audioSource;

    private float startY;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        var cameraExtent = Camera.main.orthographicSize;
        var cameraAspect = Camera.main.aspect;

        var textureHeight = Mathf.CeilToInt(cameraExtent * 2f * pixelsPerUnit);
        var textureWidth = Mathf.CeilToInt(cameraExtent * cameraAspect * 2f * pixelsPerUnit);
        var texture = CreateNoiseGradientTexture(textureWidth, textureHeight);
        texture.filterMode = FilterMode.Point;

        var spriteRect = new Rect(0f, 0f, textureWidth, textureHeight);
        var spritePivot = new Vector2(.5f, 1f);
        var sprite = Sprite.Create(texture, spriteRect, spritePivot, pixelsPerUnit);

        startY = rowboat.position.y;

        GetComponent<SpriteRenderer>().sprite = sprite;

        StartCoroutine(AnimationCoroutine());
    }

    Texture2D CreateNoiseGradientTexture(int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.ARGB32, false);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var opacity = 1.5f * (float)y / (float)height;
                var color = opacity > Random.value ? Color.white : Color.clear;
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        return texture;
    }

    void Update()
    {
        audioSource.volume = GetScale() / 4f;

        if (GetScale() >= 4f && OnEscape != null)
        {
            OnEscape.Invoke();
        }
    }

    public float GetScale()
    {
        var t = (rowboat.position.y - startY) / goal;
        var floor = minimum + t * slope;
        return Mathf.Clamp((1f - floor) * Mathf.Pow(t, exp) + floor, 0f, 4f);
    }

    IEnumerator AnimationCoroutine()
    {
        while (true)
        {
            transform.localScale = new Vector3(
                transform.localScale.x,
                GetScale(),
                transform.localScale.z);

            yield return new WaitForSeconds(1f / fps);
        }
    }
}
