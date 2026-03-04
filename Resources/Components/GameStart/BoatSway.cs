using UnityEngine;

public class BoatSway : MonoBehaviour
{
    public float swayAmount = 0.5f;
    public float swaySpeed = 1f;

    public float swayDistortionAmount = 0.1f;

    public float moveForwardOnZSpeed = 0.1f;

    private Quaternion _initialRotation;

    void Start()
    {
        _initialRotation = transform.rotation;
    }

    void Update()
    {
        float swayAngle = (Mathf.Sin(Time.time * swaySpeed) * swayAmount) + (Mathf.PerlinNoise(Time.time * swaySpeed, 0f) * swayDistortionAmount);
        float swayAngle1 = (Mathf.Sin(Time.time * swaySpeed * 1.3f) * (swayAmount * 0.5f)) + (Mathf.PerlinNoise(Time.time * swaySpeed * 1.3f, 0f) * swayDistortionAmount);

        transform.rotation = _initialRotation * Quaternion.Euler(swayAngle1 / 2f, swayAngle, 0f);

        transform.position += moveForwardOnZSpeed * Time.deltaTime * transform.forward;
        transform.position += 0.01f * Mathf.Sin(Time.time * swaySpeed) * moveForwardOnZSpeed * swayAmount * Time.deltaTime * transform.right;
}
}
