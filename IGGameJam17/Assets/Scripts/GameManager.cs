using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static readonly float INITIAL_ROUND_TIME = 60f;
    public int HandStackSize { get; private set; } = 2;
    public int CurrentStackedItems { get; private set; } = 0;
    public float CurrentRoundTimeLimit { get; private set; } = INITIAL_ROUND_TIME;
    public float SecondsRemaining { get; private set; } = INITIAL_ROUND_TIME;
    public int CurrentRoundScore { get; private set; }
    public int TotalScore { get; private set; } = 0;
    public bool RoundCompleted { get; private set; } = false;
    public int RoundCorrectSorts { get; private set; } = 0;
    public int RoundWrongSorts { get; private set; } = 0;
    public int RoundMissedSorts { get; private set; } = 0;
    public static GameManager Instance { get; private set; }

    [SerializeField]
    private GameObject roundCompletionOverlay;

    [SerializeField]
    private ItemSpawner itemSpawner;

    public List<GameObject> ActiveFoodPrefabs { get; private set; } = new List<GameObject>();
    public List<GameObject> ActiveGarbagePrefabs { get; private set; } = new List<GameObject>();

    [SerializeField] private GameObject apple;
    [SerializeField] private GameObject cakeSlice;
    [SerializeField] private GameObject can;
    [SerializeField] private GameObject cheese;
    [SerializeField] private GameObject chicken;
    [SerializeField] private GameObject cigarette;
    [SerializeField] private GameObject hotdog;
    [SerializeField] private GameObject jello;
    [SerializeField] private GameObject paper;
    [SerializeField] private GameObject roulade;
    [SerializeField] private GameObject onigiri;
    [SerializeField] private GameObject shroom;
    [SerializeField] private GameObject takoyaki;

    private HashSet<string> unlockedUpgrades = new HashSet<string>();
    public float RoundElapsedSeconds => CurrentRoundTimeLimit - SecondsRemaining;


    private void Awake()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            AudioManager.Instance.PlayTitleMusic();
        }
        else if (SceneManager.GetActiveScene().name == "MaínScene")
        {
            AudioManager.Instance.PlayMainMusic();
        }
        else
        {
            AudioManager.Instance.PlayShopMusic();
        }

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            RoundCompleted = SceneManager.GetActiveScene().name != "MainScene";
            if (unlockedUpgrades.Count == 0)
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
        HandStackSize = 2;
        CurrentRoundScore = 0;
        TotalScore = 0;
        SecondsRemaining = INITIAL_ROUND_TIME;
        CurrentRoundTimeLimit = INITIAL_ROUND_TIME;
        
        RoundCompleted = false;

        RoundCorrectSorts = 0;
        RoundWrongSorts = 0;
        RoundMissedSorts = 0;

        unlockedUpgrades.Clear();
        ActiveFoodPrefabs.Clear();
        ActiveGarbagePrefabs.Clear();

        AddInitialUpgrades();

        if (itemSpawner != null)
            itemSpawner.Reset();

        ClearItems();
        ClearStackCount();
    }

    public void BeginNextRound()
    {
        CurrentRoundScore = 0;
        SecondsRemaining = CurrentRoundTimeLimit;
        RoundCompleted = false;

        RoundCorrectSorts = 0;
        RoundWrongSorts = 0;
        RoundMissedSorts = 0;

        if (itemSpawner != null)
            itemSpawner.Reset(true); // Reset + Shuffle

        ClearItems();
        ClearStackCount();
    }

    private void AddInitialUpgrades()
    {
        foreach (string key in new List<string> { "0_0_variant", "1_0_variant", "2_0_variant" })
        {
            UnlockUpgrade(key);
        }

        ActiveGarbagePrefabs.Add(can);
        ActiveGarbagePrefabs.Add(cigarette);
        ActiveGarbagePrefabs.Add(paper);
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
        unlockedUpgrades.Add(upgradeKey);

        Debug.Log("Applied upgrade: " + upgradeKey);

        // TODO: Apply upgrade effects here, such as increasing round time limit or other effects
        switch (upgradeKey)
        {
            case "0_0_variant":
                ActiveFoodPrefabs.Add(cheese);
                break;
            case "0_2_variant":
                ActiveFoodPrefabs.Add(apple);
                break;
            case "0_5_variant":
                ActiveFoodPrefabs.Add(jello);
                break;
            case "1_0_variant":
                ActiveFoodPrefabs.Add(shroom);
                break;
            case "2_0_variant":
                ActiveFoodPrefabs.Add(takoyaki);
                break;
            case "1_3_variant":
                ActiveFoodPrefabs.Add(onigiri);
                break;
            case "1_5_variant":
                ActiveFoodPrefabs.Add(chicken);
                break;
            case "2_1_variant":
                ActiveFoodPrefabs.Add(cakeSlice);
                break;
            case "2_4_variant":
                ActiveFoodPrefabs.Add(hotdog);
                break;
            case "0_3_stack":
            case "1_1_stack":
            case "1_4_stack":
            case "2_2_stack":
            case "2_5_stack":
                HandStackSize += 1;
                break;
            case "0_1_time":
            case "0_4_time":
            case "1_2_time":
            case "2_3_time":
                CurrentRoundTimeLimit += 10f;
                break;
        }
    }

    public bool HasUpgrade(string upgradeKey)
    {
        return unlockedUpgrades.Contains(upgradeKey);
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

    public void AddCorrectSort()
    {
        RoundCorrectSorts++;
    }

    public void AddWrongSort()
    {
        RoundWrongSorts++;
    }

    public void AddMissedSort()
    {
        RoundMissedSorts++;
    }

    public void ClearStackCount()
    {
        CurrentStackedItems = 0;
    }

    public void AddStackItem()
    {
        if (IsStackFull())
            Debug.LogError("Cannot add item to stack since it is already full.");
        CurrentStackedItems++;
    }

    public bool IsStackFull()
    {
        return CurrentStackedItems >= HandStackSize;
    }
}
