using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private GameObject leftTop;
    [SerializeField] private GameObject leftBottom;
    [SerializeField] private GameObject rightTop;
    [SerializeField] private GameObject rightBottom;

    [SerializeField] private GameObject itemPrefab;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Random.value < 0.01f) // Adjust spawn chance as needed
        {
            SpawnItem();
        }
    }

    public void SpawnItem()
    {
        float speed = Random.Range(3, 6);

        (Vector3 spawnPos, Vector3 normDir) = GetRandomSpawnKinetics();
        GameObject item = Instantiate(itemPrefab, spawnPos, Quaternion.identity);
        item.GetComponent<DraggableItem>().SetMomentum(normDir * speed); // Adjust speed as needed
    }

    private (Vector3 pos, Vector3 normDir) GetRandomSpawnKinetics()
    {
        bool onLeftHalf = Random.value > 0.5f;

        Vector3 spawnPos = GetRandomPoint(onLeftHalf);
        Vector3 targetPos = GetRandomPoint(!onLeftHalf);
        Vector3 normDir = (targetPos - spawnPos).normalized;

        Debug.Log("Spawn Position: " + spawnPos + ", Target Position: " + targetPos + ", Normalized Direction: " + normDir);

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
