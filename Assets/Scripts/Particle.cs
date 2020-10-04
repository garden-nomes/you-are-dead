using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Particle : MonoBehaviour
{
    public River river;
    public Transform cameraPosition;

    private TrailRenderer trailRenderer;

    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    void Update()
    {
        var current = river.GetCurrent(transform.position.y);
        transform.position += (Vector3)current * 2f * Time.deltaTime;

        var cameraSize = Camera.main.orthographicSize;
        var cameraTop = cameraPosition.position.y + cameraSize;
        var cameraBottom = cameraPosition.position.y - cameraSize;

        if (transform.position.y > cameraTop + 2f)
        {
            var y = cameraBottom;
            var x = Random.Range(river.GetBank(y, true), river.GetBank(y, false));
            MoveTo(new Vector3(x, y, transform.position.z));
        }
        else if (transform.position.y < cameraBottom - 2f)
        {
            var y = cameraTop;
            var x = Random.Range(river.GetBank(y, true), river.GetBank(y, false));
            MoveTo(new Vector3(x, y, transform.position.z));
        }
    }

    void MoveTo(Vector3 to)
    {
        StartCoroutine(MoveToCoroutine(to));
    }

    IEnumerator MoveToCoroutine(Vector3 to)
    {
        trailRenderer.emitting = false;
        yield return null;
        transform.position = to;
        yield return new WaitForSeconds(.1f);
        trailRenderer.emitting = true;
    }
}
