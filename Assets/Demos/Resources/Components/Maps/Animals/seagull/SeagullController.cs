using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class SeagullController : MonoBehaviour
{
    public Animator[] seagulls;
    private bool[] glideStates;
    public GameObject[] paths;
    private Dictionary<Animator, SplineContainer> seagullPaths = new();
    private float[] seagullT;
    private float[] seagullFlapSpeeds;
    private float[] seagullGlideSpeeds;
    public float minFlapSpeed = 0.8f;
    public float maxFlapSpeed = 1.2f;
    public float minGlideSpeed = 0.5f;
    public float maxGlideSpeed = 0.9f;
    public float minGlideDuration = 2f;
    public float maxGlideDuration = 5f;
    public float minFlapDuration = 1f;
    public float maxFlapDuration = 3f;
    private float[] glideTimers;
    private float[] flapTimers;

    void Start()
    {
        glideStates = new bool[seagulls.Length];
        glideTimers = new float[seagulls.Length];
        flapTimers = new float[seagulls.Length];
        seagullT = new float[seagulls.Length];
        seagullFlapSpeeds = new float[seagulls.Length];
        seagullGlideSpeeds = new float[seagulls.Length];
        for (int i = 0; i < seagulls.Length; i++)
        {
            glideStates[i] = Random.value > 0.5f;
            glideTimers[i] = Random.Range(minGlideDuration, maxGlideDuration);
            flapTimers[i] = Random.Range(minFlapDuration, maxFlapDuration);
            seagullPaths[seagulls[i]] = paths[Random.Range(0, paths.Length)]
                .GetComponent<SplineContainer>();
            seagullT[i] = Random.Range(0f, 1f);
            seagullFlapSpeeds[i] = Random.Range(minFlapSpeed, maxFlapSpeed);
            seagullGlideSpeeds[i] = Random.Range(minGlideSpeed, maxGlideSpeed);
            SplineContainer startContainer = seagullPaths[seagulls[i]];
            seagulls[i].transform.position = startContainer.transform.TransformPoint(
                startContainer.Spline.EvaluatePosition(seagullT[i])
            );
        }
    }

    void Update()
    {
        for (int i = 0; i < seagulls.Length; i++)
        {
            Animator seagull = seagulls[i];
            SplineContainer container = seagullPaths[seagull];

            if (glideStates[i])
            {
                // Glide mode
                glideTimers[i] -= Time.deltaTime;
                if (glideTimers[i] <= 0)
                {
                    glideStates[i] = false;
                    glideTimers[i] = Random.Range(minGlideDuration, maxGlideDuration);
                    seagull.SetBool("glide", false);
                }
                else
                {
                    MoveAlongPath(i, seagull, container, seagullGlideSpeeds[i]);
                }
            }
            else
            {
                // Flap mode
                flapTimers[i] -= Time.deltaTime;
                if (flapTimers[i] <= 0)
                {
                    glideStates[i] = true;
                    flapTimers[i] = Random.Range(minFlapDuration, maxFlapDuration);
                    seagull.SetBool("glide", true);
                }
                else
                {
                    MoveAlongPath(i, seagull, container, seagullFlapSpeeds[i]);
                }
            }
        }
    }

    void MoveAlongPath(int index, Animator seagull, SplineContainer container, float speed)
    {
        Spline path = container.Spline;
        // Advance normalized t (0-1) based on speed and actual path length
        float pathLength = path.GetLength();
        seagullT[index] = (seagullT[index] + (speed * Time.deltaTime) / pathLength) % 1f;

        // Convert local spline position to world space
        seagull.transform.position = container.transform.TransformPoint(
            path.EvaluatePosition(seagullT[index])
        );

        // Rotate to face movement direction (tangent also needs to be in world space)
        Vector3 direction = container.transform.TransformDirection(
            ((Vector3)path.EvaluateTangent(seagullT[index])).normalized
        );
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            seagull.transform.rotation = Quaternion.Slerp(
                seagull.transform.rotation,
                targetRotation,
                Time.deltaTime * 8f
            );
        }
    }
}
