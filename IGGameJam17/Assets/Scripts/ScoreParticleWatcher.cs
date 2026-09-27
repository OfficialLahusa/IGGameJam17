using UnityEngine;

public class ScoreParticleWatcher : MonoBehaviour
{
    public System.Action OnFinished;

    private void OnDestroy()
    {
        OnFinished?.Invoke();
    }
}