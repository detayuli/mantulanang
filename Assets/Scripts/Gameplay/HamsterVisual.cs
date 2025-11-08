using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HamsterController))]
public class HamsterVisual : MonoBehaviour
{
    private HamsterController controller;

    // Aura Effect
    [Header("Aura Effect")]
    [SerializeField] private ParticleSystem auraParticle;
    [Range(0, 1)]
    [SerializeField] private float defaultActivationPower;
    private Material auraMat;
    private bool auraActive = true;

    // Trail Effect
    [Header("Trail Effect")]
    [SerializeField] private TrailRenderer trailRenderer;
    [SerializeField] private Color trailColor;
    private Material trailMat;

    // Hit Effect
    [Header("Hit Effect")]
    [SerializeField] private ParticleSystem hitParticle;
    [SerializeField] private Transform particleParent;

    // Shake Camera
    [Header("Shake Camera")]
    [SerializeField] private AnimationCurve shakeCurve;
    [SerializeField] private float shakeDamper;
    [SerializeField] private float shakeTime;
    Vector3 originalPos;

    void Start()
    {
        controller = GetComponent<HamsterController>();

        auraMat = auraParticle.GetComponent<ParticleSystemRenderer>().material;
        StopAura();

        trailMat = trailRenderer.material;
        trailMat.SetColor("_Color", trailColor);

        controller.OnHit = (pos) => SpawnHit(pos);

        Vector3 originalPos = Camera.main.transform.localPosition;
    }

    void Update()
    {
        if (controller.isDragging)
            StartAura();
        else
            StopAura();

        if (auraActive)
        {
            // rotate aura base on target trajectory
        }
    }

    public void StartAura()
    {
        if (auraActive) return;
        auraMat.SetFloat("_AuraPower", defaultActivationPower);
        auraActive = true;
    }

    public void StopAura()
    {
        if (!auraActive) return;
        auraMat.SetFloat("_AuraPower", 0);
        auraActive = false;
    }

    public void SpawnHit(Vector3 pos)
    {
        ParticleSystem hit = Instantiate(hitParticle, pos, Quaternion.identity, particleParent);
        Destroy(hit.gameObject, hit.main.startLifetime.constantMax);
        ShakeCamera(shakeDamper, shakeTime);
    }

    public void ShakeCamera(float power, float duration)
    {
        StartCoroutine(SimpleShake(power, duration));
    }

    IEnumerator SimpleShake(float power, float duration)
    {
        Transform cam = Camera.main.transform;
        Vector3 originalPos = cam.localPosition;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float offsetX = Random.Range(-1f, 1f) * power;
            float offsetY = Random.Range(-1f, 1f) * power;

            cam.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);

            yield return null;
        }

        cam.localPosition = originalPos;
    }


}
