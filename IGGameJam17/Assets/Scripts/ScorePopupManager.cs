using UnityEngine;
using System.Collections.Generic;

public class ScorePopupManager : MonoBehaviour
{
    public enum PopupType { Food, Trash }

    [Header("Prefab")]
    [SerializeField] private ScoreParticle scoreParticlePrefab;

    [Header("Spawn Points — Food")]
    [SerializeField] private Transform foodPositiveSpawn;
    [SerializeField] private Transform foodNegativeSpawn;

    [Header("Spawn Points — Trash")]
    [SerializeField] private Transform trashPositiveSpawn;
    [SerializeField] private Transform trashNegativeSpawn;

    [Header("Direction per Type")]
    [SerializeField] private Vector2 foodDirection = new Vector2(0f, -1f);
    [SerializeField] private Vector2 trashDirection = new Vector2(0f, 1f);

    [Header("Behavior")]
    [SerializeField] private bool mergeWhileVisible = true;

    [Tooltip("Optional parent for spawned particles. Leave empty to use this manager's transform.")]
    [SerializeField] private Transform particleParent;

    private readonly Dictionary<(PopupType, bool), ScoreParticle> _active
        = new Dictionary<(PopupType, bool), ScoreParticle>();

    private readonly Dictionary<(PopupType, bool), int> _accumulated
        = new Dictionary<(PopupType, bool), int>();

    public void DisplayFoodValue(int value)
    {
        Handle(PopupType.Food, value);
    }

    public void DisplayTrashValue(int value)
    {
        Handle(PopupType.Trash, value);
    }

    private void Handle(PopupType type, int delta)
    {
        if (delta == 0) return;

        bool isNegative = delta < 0;
        var key = (type, isNegative);
        Vector2 dir = type == PopupType.Food ? foodDirection : trashDirection;

        if (isNegative)
            dir = -dir;

        if (mergeWhileVisible
            && _active.TryGetValue(key, out var existing)
            && existing != null)
        {
            _accumulated[key] += delta;
            existing.Init(_accumulated[key], dir);
            return;
        }

        Spawn(type, delta, isNegative, key, dir);
    }

    private void Spawn(PopupType type, int delta, bool isNegative, (PopupType, bool) key, Vector2 dir)
    {
        if (scoreParticlePrefab == null)
        {
            Debug.LogWarning($"[{nameof(ScorePopupManager)}] No particle prefab assigned.");
            return;
        }

        Transform spawnPoint = GetSpawnPoint(type, isNegative);
        if (spawnPoint == null)
        {
            Debug.LogWarning($"[{nameof(ScorePopupManager)}] Missing spawn point for {type} {(isNegative ? "negative" : "positive")}.");
            return;
        }

        Transform parent = particleParent != null ? particleParent : transform;

        var instance = Instantiate(scoreParticlePrefab, spawnPoint.position, spawnPoint.rotation, parent);
        instance.Init(delta, dir);

        _active[key] = instance;
        _accumulated[key] = delta;

        var watcher = instance.gameObject.AddComponent<ScoreParticleWatcher>();
        watcher.OnFinished = () =>
        {
            if (_active.TryGetValue(key, out var current) && current == instance)
            {
                _active.Remove(key);
                _accumulated.Remove(key);
            }
        };
    }

    private Transform GetSpawnPoint(PopupType type, bool isNegative)
    {
        return type switch
        {
            PopupType.Food => isNegative ? trashNegativeSpawn : foodPositiveSpawn,
            PopupType.Trash => isNegative ? foodNegativeSpawn : trashPositiveSpawn,
            _ => null
        };
    }
}