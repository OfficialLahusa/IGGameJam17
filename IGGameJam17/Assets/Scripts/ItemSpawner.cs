using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject leftTop;
    [SerializeField] private GameObject leftBottom;
    [SerializeField] private GameObject rightTop;
    [SerializeField] private GameObject rightBottom;

    [SerializeField] private GameObject[] foodPrefabs;
    [SerializeField] private GameObject[] garbagePrefabs;

    void Start()
    {
        
    }

    void Update()
    {
        
    }

    public void SpawnItem()
    {
        GameObject chosenPrefab = Random.value < 0.5f ? foodPrefabs[Random.Range(0, foodPrefabs.Length)] : garbagePrefabs[Random.Range(0, garbagePrefabs.Length)];

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
}
