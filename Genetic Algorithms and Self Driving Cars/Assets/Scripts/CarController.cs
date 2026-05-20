using UnityEngine;

public class CarController : MonoBehaviour
{
    private Vector3 startPosition;
    private Vector3 startRotation;
    private NeuralNetwork network;

    [Range(-1f, 1f)]
    public float accelerationInput;
    public float steeringInput;

    public float timeSinceStart = 0f;

    [Header("Fitness")]
    public float overallFitness;
    public float distanceMultipler = 1.4f;
    public float avgSpeedMultiplier = 0.2f;
    public float sensorMultiplier = 0.1f;

    [Header("Network Options")]
    public int layers = 1;
    public int neurons = 10;

    private Vector3 lastPosition;
    private float totalDistanceTravelled;
    private float avgSpeed;

    private float aSensor;
    private float bSensor;
    private float carSensor;
    private Vector3 interpolate;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.eulerAngles;
    }

    public void ResetNetwork(NeuralNetwork net)
    {
        network = net;
        Reset();
    }

    public void Reset()
    {
        timeSinceStart = 0f;
        totalDistanceTravelled = 0f;
        avgSpeed = 0f;
        lastPosition = startPosition;
        overallFitness = 0f;

        transform.position = startPosition;
        transform.eulerAngles = startRotation;
    }

    private void OnCollisionEnter(Collision collision)
    {
        Death();
    }

    private void FixedUpdate()
    {
        if (network == null)
        {
            return;
        }

        InputSensors();
        lastPosition = transform.position;

        (accelerationInput, steeringInput) = network.RunNetwork(aSensor, bSensor, carSensor);
        MoveCar(accelerationInput, steeringInput);

        timeSinceStart += Time.deltaTime;
        CalculateFitness();
    }

    private void Death()
    {
        FindFirstObjectByType<GeneticManager>().Death(overallFitness, network);
    }

    private void CalculateFitness()
    {
        totalDistanceTravelled += Vector3.Distance(transform.position, lastPosition);
        avgSpeed = totalDistanceTravelled / timeSinceStart;

        overallFitness = (totalDistanceTravelled * distanceMultipler)
            + (avgSpeed * avgSpeedMultiplier)
            + (((aSensor + bSensor + carSensor) / 3) * sensorMultiplier);

        if (timeSinceStart > 20 && overallFitness < 40)
        {
            Death();
        }

        if (overallFitness >= 1000)
        {
            Death();
        }
    }

    private void InputSensors()
    {
        Vector3 aDirection = transform.forward + transform.right;
        Vector3 bDirection = transform.forward;
        Vector3 cDirection = transform.forward - transform.right;

        Ray ray = new Ray(transform.position, aDirection);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            aSensor = hit.distance / 20;
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }

        ray.direction = bDirection;

        if (Physics.Raycast(ray, out hit))
        {
            bSensor = hit.distance / 20;
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }

        ray.direction = cDirection;

        if (Physics.Raycast(ray, out hit))
        {
            carSensor = hit.distance / 20;
            Debug.DrawLine(ray.origin, hit.point, Color.red);
        }
    }

    public void MoveCar(float v, float h)
    {
        interpolate = Vector3.Lerp(Vector3.zero, new Vector3(0, 0, v * 11.4f), 0.02f);
        interpolate = transform.TransformDirection(interpolate);
        transform.position += interpolate;

        transform.eulerAngles += new Vector3(0, h * 90 * 0.02f, 0);
    }
}
