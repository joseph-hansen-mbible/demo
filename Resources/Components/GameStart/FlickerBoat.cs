using UnityEngine;
using UnityEngine.Rendering.Universal;

public class Flicker : MonoBehaviour
{
    public Material mat;
    public Color emissionColor = Color.white;
    public float minIntensity = 1f;
    public float maxIntensity = 5f;

    public float maxDecalIntensity = .2f;
    public float minDecalIntensity = .05f;

    public DecalProjector[] decalProjectors;

    // add noise to the material's emission intensity 
    // material is a URP Lit default with emission enabled
    private void Update()
    {
        if (mat != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * 4f, 0f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
            Color finalEmission = emissionColor * intensity;
            mat.SetColor("_EmissionColor", finalEmission);
        }

        if (decalProjectors != null)
        {
            foreach (var decalProjector in decalProjectors)
            {
                float noise = Mathf.PerlinNoise(Time.time * 4f, 0f);
                float opacity = Mathf.Lerp(minDecalIntensity, maxDecalIntensity, noise);
                decalProjector.fadeFactor = opacity;
            }
        }
    }
}
