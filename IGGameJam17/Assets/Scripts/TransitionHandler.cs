using UnityEngine;

public class TransitionHandler : MonoBehaviour
{
    private GameManager gameManager;

    void Update()
    {
        if (gameManager == null)
        {
            gameManager = FindFirstObjectByType<GameManager>();
        }
    }

    // Update is called once per frame
    public void MainToShop()
    {
        gameManager.EnterShop();
    }

    public void ShopToMain()
    {
        gameManager.LeaveShop();
    }

    public void MainToTitle()
    {
        gameManager.Restart();
        gameManager.QuitToTitle();
    }

    public void MainRestart()
    {
        gameManager.Restart();
    }

    public void MainNext()
    {
        gameManager.BeginNextRound();
    }
}
