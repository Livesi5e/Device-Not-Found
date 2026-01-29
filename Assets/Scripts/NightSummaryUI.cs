using TMPro;
using UnityEngine;

public class NightSummaryUI : MonoBehaviour
{
    [Header("Root Panel (this object)")]
    [SerializeField] private GameObject _panelRoot;

    [Header("Text Fields")]
    [SerializeField] private TMP_Text _earnedText;
    [SerializeField] private TMP_Text _rentText;
    [SerializeField] private TMP_Text _beforeAfterText;

    private void Awake()
    {
        Hide();
    }

    public void Show(int earned, int rent, int moneyBeforeRent, int moneyAfterRent)
    {
        if (_panelRoot != null) _panelRoot.SetActive(true);

        if (_earnedText != null) _earnedText.text = $"In dieser Nacht verdient: {earned}$";
        if (_rentText != null) _rentText.text = $"Miete: -{rent}$";
        if (_beforeAfterText != null) _beforeAfterText.text =
            $"Geld vorher: {moneyBeforeRent}$\nGeld nach Miete: {moneyAfterRent}$";

        // Optional: Cursor freischalten
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Optional: Spiel pausieren
        Time.timeScale = 0f;
    }

    public void Hide()
    {
        if (_panelRoot != null) _panelRoot.SetActive(false);

        // Optional: Spiel wieder laufen lassen
        Time.timeScale = 1f;
    }

    // Button "Weiter"
    public void OnContinueClicked()
    {
        Hide();

        // Cursor ggf. wieder sperren (je nach Controller)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        GameManager.Instance?.AfterNightContinue();
    }
}