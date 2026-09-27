using UnityEngine;

public class MonsterHeadController : MonoBehaviour
{
    [SerializeField] private GameObject normalHead;
    [SerializeField] private GameObject angryHead;
    [SerializeField] private GameObject happyHead;

    [SerializeField] private float happyTimer = 0f;
    [SerializeField] private float angryTimer = 0f;

    void Update()
    {
        happyTimer -= Time.deltaTime;
        angryTimer -= Time.deltaTime;

        if (happyTimer < 0f) happyTimer = 0f;
        if (angryTimer < 0f) angryTimer = 0f;

        happyHead.SetActive(happyTimer > 0f && happyTimer > angryTimer);
        angryHead.SetActive(angryTimer > 0f);
        normalHead.SetActive(happyTimer <= 0f && angryTimer <= 0f);
    }

    public void HappyPulse()
    {
        if (angryTimer <= 0f)
            happyTimer += 2f;
    }

    public void AngryPulse()
    {
        angryTimer += 2f;
        happyTimer = 0f;
    }
}
