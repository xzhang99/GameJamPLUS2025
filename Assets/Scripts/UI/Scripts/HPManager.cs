using UnityEngine;
using UnityEngine.UI;

public class HPManager : MonoBehaviour
{
    public Image[] hearts;        
    public int maxHP = 3;         
    private int currentHP;

    void Start()
    {
        currentHP = maxHP;
        UpdateHeartsUI();
    }

 
    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP < 0) currentHP = 0;
        UpdateHeartsUI();
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP) currentHP = maxHP;
        UpdateHeartsUI();
    }

    void UpdateHeartsUI()
    {
        for (int i = 0; i < hearts.Length; i++)
        {
            if (i < currentHP)
                hearts[i].enabled = true;   
            else
                hearts[i].enabled = false;  
        }
    }



    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H)) 
        {
            TakeDamage(1);
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            Heal(1);
        }
    }


}
