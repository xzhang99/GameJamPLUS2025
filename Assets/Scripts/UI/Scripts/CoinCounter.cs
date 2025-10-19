using UnityEngine;
using UnityEngine.UI;



/// <summary>
/// call AddCoin() or RemoveCoin() to update the UI.
/// </summary>

public class CoinCounter : MonoBehaviour
{
    [Header("Assign the coin icon Image and the number Text")]
    public Image coinIcon;    
    public Text coinText;     

    [Header("Initial coin count")]
    public int currentCoins = 0;

    void Start()
    {
        UpdateCoinUI();
    }

    /// <summary>
    /// Call this to add coins. Example: CoinCounterInstance.AddCoin(1);
    /// </summary>
    public void AddCoin(int amount)
    {
        currentCoins += amount;
        UpdateCoinUI();
    }

    /// <summary>
    /// Call this to remove coins. Example: CoinCounterInstance.RemoveCoin(1);
    /// </summary>
    public void RemoveCoin(int amount)
    {
        currentCoins -= amount;
        if (currentCoins < 0) currentCoins = 0;
        UpdateCoinUI();
    }

    /// <summary>
    /// Update the displayed number
    /// </summary>
    private void UpdateCoinUI()
    {
        if (coinText != null)
            coinText.text = currentCoins.ToString();
    }


    // Testing keys: I to add coin, O to remove coin
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) AddCoin(1);
        if (Input.GetKeyDown(KeyCode.O)) RemoveCoin(1);
    }

}
