using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;

    void Update()
    {
        if (GameManager.Instance == null) return;
        moneyText.text = $"Geld: {GameManager.Instance.Money}$";
    }
}