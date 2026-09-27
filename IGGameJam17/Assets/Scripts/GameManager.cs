using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static readonly float INITIAL_ROUND_TIME = 45f;
    public float CurrentRoundTimeLimit { get; private set; } = INITIAL_ROUND_TIME;
    public float SecondsRemaining { get; private set; } = INITIAL_ROUND_TIME;
    public int CurrentRoundScore { get; private set; }
    public int TotalScore { get; private set; }
    public bool RoundCompleted { get; private set; } = false;
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameObject roundCompletionOverlay;

    [SerializeField]
    private ItemSpawner itemSpawner;

    public HashSet<string> UnlockedUpgrades { get; private set; } = new HashSet<string>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            RoundCompleted = SceneManager.GetActiveScene().name != "MainScene";
            if (UnlockedUpgrades.Count == 0)
            {
                AddInitialUpgrades();
            }

            DontDestroyOnLoad(gameObject);
        }
    }

    private void Update()
    {
        // Lazy load the round completion overlay if it hasn't been assigned yet
        if (roundCompletionOverlay == null)
            roundCompletionOverlay = GameObject.Find("Round Completion Overlay");

        // Same for the item spawner
        if (itemSpawner == null)
            itemSpawner = FindFirstObjectByType<ItemSpawner>();

        if (roundCompletionOverlay != null)
            roundCompletionOverlay.SetActive(RoundCompleted);

        if (!RoundCompleted)
        {
            // TODO: Proper Frame Rate Independent Spawning Logic
            if (Random.value < 0.01f && itemSpawner != null) 
            {
                SpawnItem();
            }

            SecondsRemaining -= Time.deltaTime;

            if (SecondsRemaining <= 0f)
            {
                RoundCompleted = true;
                SecondsRemaining = 0f;

                // Add current round score to total score
                TotalScore += CurrentRoundScore;

                Debug.Log("Round completed!");
            }
        }
    }

    public void AddScore(int score)
    {
        // Only add score while round is still running, otherwise ignore
        if (RoundCompleted) return;

        CurrentRoundScore += score;
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
        CurrentRoundScore = 0;
        TotalScore = 0;
        SecondsRemaining = INITIAL_ROUND_TIME;
        CurrentRoundTimeLimit = INITIAL_ROUND_TIME;
        RoundCompleted = false;
        UnlockedUpgrades.Clear();
        AddInitialUpgrades();

        ClearItems();
    }

    public void BeginNextRound()
    {
        CurrentRoundScore = 0;
        SecondsRemaining = CurrentRoundTimeLimit;
        RoundCompleted = false;

        ClearItems();
    }

    private void AddInitialUpgrades()
    {
        UnlockedUpgrades.AddRange(new List<string> { "0_0_variant", "1_0_variant", "2_0_variant" });
    }

    public bool CanAfford(int cost)
    {
        return TotalScore >= cost;
    }

    public void PayPrice(int cost)
    {
        if (!CanAfford(cost))
        {
            Debug.LogError("Attempted to pay a price that cannot be afforded.");
        }
        TotalScore -= cost;
    }

    public void UnlockUpgrade(string upgradeKey)
    {
        UnlockedUpgrades.Add(upgradeKey);

        Debug.Log("Applied upgrade: " + upgradeKey);

        // TODO: Apply upgrade effects here, such as increasing round time limit or other effects
    }

    public bool HasUpgrade(string upgradeKey)
    {
        return UnlockedUpgrades.Contains(upgradeKey);
    }

    public void EnterShop()
    {
        SceneManager.LoadScene("ShopScene");
    }

    public void LeaveShop()
    {
        SceneManager.LoadScene("MainScene");

        BeginNextRound();
    }

    public void QuitToTitle()
    {
        SceneManager.LoadScene("TitleScene");
    }
}
