using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public float SecondsRemaining { get; private set; } = 45f;
    public int Score { get; private set; }
    public bool RoundCompleted { get; private set; } = false;
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameObject roundCompletionOverlay;

    [SerializeField]
    private ItemSpawner itemSpawner;


    private void Awake()
    {

        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private void Update()
    {
        roundCompletionOverlay.SetActive(RoundCompleted);

        if (!RoundCompleted)
        {
            // TODO: Proper Frame Rate Independent Spawning Logic
            if (Random.value < 0.01f) 
            {
                SpawnItem();
            }

            SecondsRemaining -= Time.deltaTime;

            if (SecondsRemaining <= 0f)
            {
                RoundCompleted = true;
                SecondsRemaining = 0f;

                Debug.Log("Round completed!");
            }
        }
    }

    public void AddScore(int score)
    {
        // Only add score while round is still running, otherwise ignore
        if (RoundCompleted) return;

        Score += score;
    }

    public void SpawnItem()
    {
        itemSpawner.SpawnItem();
    }

    public void ClearItems()
    {
        foreach (var item in GameObject.FindGameObjectsWithTag("Item"))
        {
            Destroy(item);
        }
    }

    public void Restart()
    {
        Score = 0;
        SecondsRemaining = 45f;
        RoundCompleted = false;

        ClearItems();
        SpawnItem();
    }

    public void Quit()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
