using UnityEngine;

public class Flicker : MonoBehaviour
{
    public Material mat;
    public Color emissionColor = Color.white;
    public float minIntensity = 1f;
    public float maxIntensity = 5f;
    
    // add noise to the material's emission intensity 
    // material is a URP Lit default with emission enabled
    void Update()
    {
        if (mat != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * 4f, 0f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
            Color finalEmission = emissionColor * intensity;
            mat.SetColor("_EmissionColor", finalEmission);
        }
    }
}
