using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public float showTime = 10f;
    public Vector2 slide = new Vector2(0f, -10f);
    public float slideDuration = 0.3f;

    private float timer;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > showTime)
        {
            StartCoroutine(SlideCoroutine());
        }
    }

    IEnumerator SlideCoroutine()
    {
        var originalPosition = transform.position;
        var targetPosition = originalPosition + (Vector3)slide;

        for (float t = 0; t <= 1f; t += Time.deltaTime / slideDuration)
        {
            transform.position = Vector3.Lerp(originalPosition, targetPosition, t * t);
            yield return null;
        }

        transform.position = targetPosition;
        gameObject.SetActive(false);
    }
}
