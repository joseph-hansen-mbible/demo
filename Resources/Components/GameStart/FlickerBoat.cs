using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlickerBoat : MonoBehaviour
{
    public Material mat;
    private Material _matInstance;

    public Color emissionColor = Color.white;
    public float minIntensity = 1f;
    public float maxIntensity = 5f;

    public float maxDecalIntensity = .2f;
    public float minDecalIntensity = .05f;
    public DecalProjector[] decalProjectors;

    private void Awake()
    {
        if (mat == null)
        {
            return;
        }

        _matInstance = Instantiate(mat);
        mat = _matInstance;

        // If attached to a Renderer, ensure it uses the instance so changes don't affect other objects.
        if (TryGetComponent<Renderer>(out var rend))
        {
            rend.material = _matInstance;
        }

        _matInstance.SetColor("_EmissionColor", Color.white);
    }

    private void OnDestroy()
    {
        if (_matInstance != null)
        {
            _matInstance.SetColor("_EmissionColor", Color.white);
            Destroy(_matInstance);
            _matInstance = null;
        }
    }

    // add noise to the material's emission intensity 
    // material is a URP Lit default with emission enabled
    private void Update()
    {
        if (_matInstance != null)
        {
            float noise = Mathf.PerlinNoise(Time.time * 4f, 0f);
            float intensity = Mathf.Lerp(minIntensity, maxIntensity, noise);
            Color finalEmission = emissionColor * intensity;
            _matInstance.SetColor("_EmissionColor", finalEmission);
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
