using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct NoiseSettings
{
    public float frequency;
    public float amplitude;
}

public class River : MonoBehaviour
{
    public Transform cameraPosition;
    public LineRenderer leftBank;
    public LineRenderer rightBank;
    public NoiseSettings noiseSettings;
    public float baseWidth = 64f;
    public float generateBuffer = 10f;
    public float updateInterval = .1f;

    public Vector2 current = new Vector2(0f, 1f);
    public float currentSpeed = 5f;

    void Start()
    {
        StartCoroutine(UpdateBanksCoroutine());
    }

    IEnumerator UpdateBanksCoroutine()
    {
        while (true)
        {
            UpdateBanks();
            yield return new WaitForSeconds(updateInterval);
        }
    }

    void UpdateBanks()
    {
        var extent = Camera.main.orthographicSize + generateBuffer;
        var cameraY = cameraPosition.position.y;
        var positionCount = Mathf.CeilToInt(extent * 16f);

        leftBank.transform.position = Vector3.up * cameraY;
        rightBank.transform.position = Vector3.up * cameraY;

        leftBank.positionCount = positionCount;
        rightBank.positionCount = positionCount;

        for (int i = 0; i < positionCount; i++)
        {
            float y = i / 8f - extent;
            leftBank.SetPosition(i, new Vector3(GetBank(cameraY + y, true), y, 0f));
            rightBank.SetPosition(i, new Vector3(GetBank(cameraY + y, false), y, 0f));
        }

        UpdateCollider(leftBank);
        UpdateCollider(rightBank);
    }

    void UpdateCollider(LineRenderer bank)
    {
        var collider = bank.GetComponent<EdgeCollider2D>();

        var newPoints = new Vector2[bank.positionCount];
        for (int i = 0; i < bank.positionCount; i++)
        {
            newPoints[i] = bank.GetPosition(i);
        }

        collider.points = newPoints;
    }

    public float GetBank(float y, bool isLeft)
    {
        return baseWidth * (isLeft ? -1f : 1f) / 2f +
            ((Mathf.PerlinNoise(
                (y + 1000f) * noiseSettings.frequency,
                isLeft ? 1000f : 2000f) * 2f - 1f) *
            noiseSettings.amplitude);
    }

    public float GetMiddle(float y)
    {
        return (GetBank(y, false) + GetBank(y, true)) / 2f;
    }

    public Vector2 GetCurrent(float y)
    {
        var sampleDistance = .1f;
        var p0 = new Vector2(GetMiddle(y + sampleDistance), y + sampleDistance);
        var p1 = new Vector2(GetMiddle(y - sampleDistance), y - sampleDistance);
        return (p0 - p1).normalized * currentSpeed;
    }
}
