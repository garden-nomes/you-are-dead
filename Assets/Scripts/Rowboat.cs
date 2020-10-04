using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UnityEngine;
using UnityEngine.Audio;

public class Rowboat : MonoBehaviour
{
    public float rowForce = 10f;
    public float rowInterval = 1f;
    public float turnSpeed = 10f;
    public float maxVortexForce = 10f;
    public float currentForce = .5f;
    public float vortexTime = 1f;
    public float oarRestingAngle = 30f;
    public float oarForwardAngle = -30f;
    public float oarStrokeTime = .1f;
    public float oarResetTime = .3f;
    public float vortexSpinSpeed = 720f;
    public float fadeAudioTime = 2f;
    public float vortexNoiseStrength = 1f;
    public River river;

    public Transform leftOar;
    public Transform rightOar;
    public Transform leftTrail;
    public Transform rightTrail;

    public AudioMixer masterMixer;
    public CinemachineVirtualCamera camera;

    public Action OnRebirth;

    private Rigidbody2D rb;
    private new Collider2D collider;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isInVortex = false;

    private float oarStroke = 0f;
    private float rowTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        isInVortex = false;

        FadeInAudio();
    }

    void Update()
    {
        var areOarsForward = rowTimer > 0.5f;
        oarStroke += Time.deltaTime / (areOarsForward ? oarStrokeTime : -oarResetTime);
        oarStroke = Mathf.Clamp01(oarStroke);

        var oarAngle = Mathf.LerpAngle(oarRestingAngle, oarForwardAngle, oarStroke);
        var mirroredOarAngle = Mathf.LerpAngle(-oarRestingAngle, -oarForwardAngle, oarStroke);
        rightOar.localRotation = Quaternion.AngleAxis(oarAngle, Vector3.forward);
        leftOar.localRotation = Quaternion.AngleAxis(mirroredOarAngle, Vector3.forward);

        leftTrail.position =
            collider.ClosestPoint(transform.position + Vector3.left * 10f + Vector3.forward * 1f);
        rightTrail.position =
            collider.ClosestPoint(transform.position + Vector3.right * 10f + Vector3.forward * 1f);
    }

    void FixedUpdate()
    {
        if (rowTimer > 0f)
        {
            rowTimer -= Time.deltaTime;
        }

        rb.AddForce((river.current - rb.velocity) * currentForce);
        ApplyVortexForces();

        if (isInVortex)
        {
            return;
        }

        rb.rotation += Input.GetAxis("Horizontal") * -turnSpeed * Time.deltaTime;

        if (Input.GetKey(KeyCode.UpArrow) && rowTimer <= 0f)
        {
            rb.AddForce(transform.up * rowForce, ForceMode2D.Impulse);
            rowTimer = rowInterval;
            audioSource.Play();
        }
    }

    void IntoVortex()
    {
        if (isInVortex)
        {
            return;
        }

        isInVortex = true;
        rb.velocity = Vector2.zero;
        StartCoroutine(FadeAudioCoroutine(1f, 0f, vortexTime));
        StartCoroutine(VortexCoroutine());
    }

    IEnumerator VortexCoroutine()
    {
        for (float t = 0f; t <= 1f; t += Time.deltaTime / vortexTime)
        {
            rb.rotation += vortexSpinSpeed * Time.deltaTime;
            transform.localScale = new Vector3(1f - t, 1f - t, 1f);
            yield return null;
        }

        transform.localScale = new Vector3(0f, 0f, 1f);

        if (OnRebirth != null)
        {
            OnRebirth.Invoke();
        }

        gameObject.SetActive(false);
    }

    void ApplyVortexForces()
    {
        var vortexes = FindObjectsOfType<Vortex>();
        var result = new List<GameObject>(vortexes.Length);

        var noise = 0f;

        foreach (var vortex in vortexes)
        {
            var toVortex = (Vector2)(vortex.transform.position - transform.position);
            var distance = toVortex.magnitude;

            if (distance < vortex.strength * 5f)
            {
                noise += Mathf.Max(0f, vortex.radius * 2f - distance) * vortexNoiseStrength;

                var force = vortex.strength / Mathf.Pow(distance, 1.5f);
                force = Mathf.Min(force, maxVortexForce);
                rb.AddForce(toVortex.normalized * force);

                if (toVortex.sqrMagnitude < vortex.captureRadius * vortex.captureRadius)
                {
                    IntoVortex();
                }
            }
        }

        camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = noise;
    }

    void FadeInAudio()
    {
        StartCoroutine(FadeAudioCoroutine(0f, 1f, fadeAudioTime));
    }

    IEnumerator FadeAudioCoroutine(float from, float to, float duration)
    {
        masterMixer.SetFloat("GameVolume", VolumeToGain(from));

        for (float t = 0f; t <= 1f; t += Time.deltaTime / duration)
        {
            masterMixer.SetFloat("GameVolume", VolumeToGain(Mathf.Lerp(from, to, t)));
            yield return null;
        }

        masterMixer.SetFloat("GameVolume", VolumeToGain(to));
    }

    float VolumeToGain(float volume)
    {
        volume = Mathf.Clamp(volume, 0.01f, 1f);
        return Mathf.Log(volume) * 20f;
    }
}
