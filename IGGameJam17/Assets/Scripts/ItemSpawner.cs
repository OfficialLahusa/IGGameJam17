using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject leftTop;
    [SerializeField] private GameObject leftBottom;
    [SerializeField] private GameObject rightTop;
    [SerializeField] private GameObject rightBottom;

    private float[] delayQueue;
    private int currentDelayIdx = 0;
    private float remainingDelay;

    void Awake()
    {
        Reset();
    }

    void Update()
    {
        if (!GameManager.Instance.RoundCompleted)
        {
            // Scale delay decrement by spawn rate factor
            remainingDelay -= Time.deltaTime * CalcSpawnRateMultiplier(GameManager.Instance.RoundElapsedSeconds);

            // Trigger next spawn if scaled delay has elapsed
            if (remainingDelay < 0f)
            {
                SpawnItem();

                currentDelayIdx++;

                // Re-shuffle delay queue if end is reached
                if(currentDelayIdx >= delayQueue.Length)
                {
                    delayQueue.Shuffle();
                    currentDelayIdx = 0;
                }

                // Add queued delay
                remainingDelay = delayQueue[currentDelayIdx];
            }
        }
    }

    public void SpawnItem()
    {
        // Create food or trash
        GameObject chosenPrefab = Random.value < 0.5f
            // Pick food prefab
            ? GameManager.Instance.ActiveFoodPrefabs[Random.Range(0, GameManager.Instance.ActiveFoodPrefabs.Count)]
            // Pick trash prefab
            : GameManager.Instance.ActiveGarbagePrefabs[Random.Range(0, GameManager.Instance.ActiveGarbagePrefabs.Count)];

        float speed = Random.Range(chosenPrefab.GetComponent<DraggableItem>().minSpawnSpeed, chosenPrefab.GetComponent<DraggableItem>().maxSpawnSpeed);

        (Vector3 spawnPos, Vector3 normDir) = GetRandomSpawnKinetics();
        GameObject item = Instantiate(chosenPrefab, spawnPos, Quaternion.identity);
        item.GetComponent<DraggableItem>().SetMomentum(normDir * speed); // Adjust speed as needed
    }

    private (Vector3 pos, Vector3 normDir) GetRandomSpawnKinetics()
    {
        bool onLeftHalf = Random.value > 0.5f;

        Vector3 spawnPos = GetRandomPoint(onLeftHalf);
        Vector3 targetPos = GetRandomPoint(!onLeftHalf);
        Vector3 normDir = (targetPos - spawnPos).normalized;

        //Debug.Log("Spawn Position: " + spawnPos + ", Target Position: " + targetPos + ", Normalized Direction: " + normDir);

        return (spawnPos, normDir);
    }

    private Vector3 GetRandomPoint(bool onLeftHalf)
    {
        float interpolationFactor = Random.value;
        return onLeftHalf 
            ? Vector3.Lerp(transform.TransformPoint(leftTop.transform.position), transform.TransformPoint(leftBottom.transform.position), interpolationFactor) 
            : Vector3.Lerp(transform.TransformPoint(rightTop.transform.position), transform.TransformPoint(rightBottom.transform.position), interpolationFactor);
    }

    private static float CalcSpawnRateMultiplier(float elapsedSeconds)
    {
        const float Ceiling = 5.3f;
        const float Tau = 23f;
        const float LinearTail = 0.015f;  // +1x every ~66s after the ramp

        float t = Mathf.Max(0f, elapsedSeconds);
        float ramp = 1f - Mathf.Exp(-t / Tau);
        return (1f + (Ceiling - 1f) * ramp * ramp + LinearTail * t) / 5f;
    }

    public void Reset(bool shuffle = false)
    {
        delayQueue = new float[]
        {
            1f, 0.85f, 0.2f, 0.65f, 1f, 0.9f, 0.42f, 0.3f, 0.77f, 1f
        };

        if (shuffle)
            delayQueue.Shuffle();

        currentDelayIdx = 0;
        remainingDelay = delayQueue[0];
    }
}
