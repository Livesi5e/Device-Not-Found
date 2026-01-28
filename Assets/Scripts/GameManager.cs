using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Phase { Day, Night }

    [Header("State")]
    [SerializeField] private Phase _phase = Phase.Night;

    [Header("Money")]
    [SerializeField] private int _money = 0;
    [SerializeField] private int _rentCost = 20;

    [Header("References")]
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _daySpawnPoint;

    [Header("UI")]
    [SerializeField] private NightSummaryUI _nightSummaryUI;

    private int _moneyAtNightStart;

    public Phase CurrentPhase => _phase;
    public int Money => _money;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 🔑 DAS ist der wichtige Teil
    private void Start()
    {
        StartNight();
    }

    public void StartNight()
    {
        _phase = Phase.Night;
        _moneyAtNightStart = _money;
        Debug.Log("Night started");
    }

    public void AddMoney(int amount)
    {
        _money += Mathf.Max(0, amount);
    }

    // Wird vom EnemyAI aufgerufen
    public void EndNightCaught()
    {
        if (_phase != Phase.Night) return;

        _phase = Phase.Day;

        int earnedThisNight = Mathf.Max(0, _money - _moneyAtNightStart);
        int rent = _rentCost;

        int moneyBeforeRent = _money;
        _money = Mathf.Max(0, _money - rent);

        // Gegner resetten
        foreach (var enemy in FindObjectsByType<EnemyAI>(FindObjectsSortMode.None))
            enemy.ResetToHomeAndStop();

        // Player teleportieren
        if (_player != null && _daySpawnPoint != null)
        {
            var cc = _player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            _player.position = _daySpawnPoint.position;
            _player.rotation = _daySpawnPoint.rotation;

            if (cc != null) cc.enabled = true;
        }

        // Summary anzeigen
        if (_nightSummaryUI != null)
        {
            _nightSummaryUI.Show(
                earnedThisNight,
                rent,
                moneyBeforeRent,
                _money
            );
        }
    }

    // Wird vom "Weiter"-Button im UI aufgerufen
    public void ConfirmNightSummary()
    {
        Debug.Log("Day continues");
        // hier später Shop / Tag-Gameplay
    }
}
