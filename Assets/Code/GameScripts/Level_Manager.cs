using UnityEngine;

public class Level_Manager : MonoBehaviour
{
    public static Level_Manager main;

    public Transform startPoint;
    public Transform[] path;

    public int currency;
    private void Awake()
    {
        main = this;
    }

    private void Start()
    {
        currency = 100;
    }

    public void IncreaseCurrency(int amount)
    {
        currency += amount;
    }

    public bool SpendCurrency(int amount)
    {
        if (amount <= currency)
        {
            currency -= amount;
            return true;
        }
        else
        {
            Debug.Log("You do not have enough money to purchase this item");
            return false;
        }
    }

}
