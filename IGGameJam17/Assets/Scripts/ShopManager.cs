using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public System.Action OnShopUpdate;

    void Awake()
    {
        
    }

    void Update()
    {
        
    }

    public void UpdateShop()
    {
        OnShopUpdate?.Invoke();
    }
}
