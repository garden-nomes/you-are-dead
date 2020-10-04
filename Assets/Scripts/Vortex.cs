using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vortex : MonoBehaviour
{
    public Transform cameraPosition;
    public int lineCount = 8;
    public float radius = 1f;
    public float radiusVariation = .1f;
    public float speed = 1f;
    public float strength = 1f;
    public float captureRadius = 1f;
    public Material lineMaterial;

    private River river;
    private LineRenderer[] lines;
    private float[] lineTimeOffsets;
    private float[] lineRadiusOffsets;
    private float deltaTime = 0f;

    void Start()
    {
        river = FindObjectOfType<River>();

        var y = transform.position.y;
        var x = Random.Range(river.GetBank(y, true) + radius, river.GetBank(y, false) - radius);
        transform.position = new Vector3(x, y, transform.position.z);

        lines = new LineRenderer[lineCount];
        lineTimeOffsets = new float[lineCount];
        lineRadiusOffsets = new float[lineCount];

        for (int i = 0; i < lineCount; i++)
        {
            var lineObj = new GameObject();
            lineObj.transform.position = transform.position;
            lineObj.transform.SetParent(transform);
            lines[i] = lineObj.AddComponent<LineRenderer>();
            lines[i].startWidth = 1f / 16f;
            lines[i].endWidth = 1f / 16f;
            lines[i].useWorldSpace = false;
            lines[i].material = lineMaterial;
            lineTimeOffsets[i] = Random.value;
            lineRadiusOffsets[i] = Random.value;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, captureRadius);
    }

    void Update()
    {
        var camExtent = Camera.main.orthographicSize;
        var camBottom = cameraPosition.transform.position.y - camExtent;

        if (transform.position.y < camBottom - 2f)
        {
            var y = transform.position.y + camExtent * 2f + 4f;
            var x = Random.Range(river.GetBank(y, true) + radius, river.GetBank(y, false) - radius);
            transform.position = new Vector2(x, y);
        }

        deltaTime += Time.deltaTime;
        UpdateLines();
    }


    void UpdateLines()
    {
        for (int i = 0; i < lines.Length; i++)
        {
            var dt = deltaTime * speed + lineTimeOffsets[i] * 3f;
            var r = radius + (lineRadiusOffsets[i] - .5f) * radius * radiusVariation * 2f;
            var d0 = Mathf.Clamp01(2 - (dt % 3f) * .75f) * r;
            var d1 = Mathf.Clamp01(1 - (dt % 3f) * .75f) * r;
            var t = Mathf.PI * 2f / (float)lines.Length * i;

            lines[i].positionCount = 2;
            lines[i].SetPosition(0, new Vector2(Mathf.Cos(t) * d0, Mathf.Sin(t) * d0));
            lines[i].SetPosition(1, new Vector2(Mathf.Cos(t) * d1, Mathf.Sin(t) * d1));
        }
    }
}
