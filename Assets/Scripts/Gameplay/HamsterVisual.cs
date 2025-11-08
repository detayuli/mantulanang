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


    void Start()
    {
        controller = GetComponent<HamsterController>();

        auraMat = auraParticle.GetComponent<ParticleSystemRenderer>().material;
        StopAura();

        trailMat = trailRenderer.material;
        trailMat.SetColor("_Color", trailColor);
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
}
