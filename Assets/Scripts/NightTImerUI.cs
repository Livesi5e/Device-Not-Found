using TMPro;
using UnityEngine;

public class NightTimerUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _timerText;

    void Update()
    {
        if (GameManager.Instance.CurrentPhase != GameManager.Phase.Night)
        {
            _timerText.text = "";
            return;
        }

        float timeLeft = GameManager.Instance.NightTimeLeft;
        timeLeft = Mathf.Max(0, timeLeft);
        
        int minutes = Mathf.FloorToInt(timeLeft / 60);
        int seconds = Mathf.FloorToInt(timeLeft % 60);

        _timerText.text = $"{minutes:00}:{seconds:00}";
    }
}
