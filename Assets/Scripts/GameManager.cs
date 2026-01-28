using System.Collections.Generic;
using UnityEngine;
using MonumentGames.PlayerInventory;
using UnityEditor;

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
    [SerializeField] private List<DropoffArea> _places;
    [SerializeField] private List<Item> _tablets;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _daySpawnPoint;
    [SerializeField] private Animator _anim;
    
    [Header("Enemies (Day/Night Swap)")]
    [SerializeField] private GameObject _mouseDay;
    [SerializeField] private GameObject _mouseNight;
    [SerializeField] private GameObject _keyboardDay;
    [SerializeField] private GameObject _keyboardNight;
    [SerializeField] private GameObject _ps5Day;
    [SerializeField] private GameObject _ps5Night;
    [SerializeField] private GameObject _SNESDay;
    [SerializeField] private GameObject _SNESNight;
    

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

    public void StartDay() {
        _phase = Phase.Day;
        
        if (_mouseDay != null) _mouseDay.SetActive(true);
        if (_mouseNight != null) _mouseNight.SetActive(false);
        if (_keyboardDay != null) _keyboardDay.SetActive(true);
        if (_keyboardNight != null) _keyboardNight.SetActive(false);
        if (_ps5Day != null) _ps5Day.SetActive(true);
        if (_ps5Night != null) _ps5Night.SetActive(false);
        if (_SNESDay != null) _SNESDay.SetActive(true);
        if (_SNESNight != null) _SNESNight.SetActive(false);
    }

    public void CheckDayOver() {
        for (int i = 0; i < 4; i++) {
            if (_places[i].GetCurrentItem() != _tablets[i])
                return;
        }

        _anim.SetTrigger("Transition");
    }

    public void StartNight()
    {
        Debug.Log("Start Night");
        _phase = Phase.Night;
        _moneyAtNightStart = _money;

        if (_mouseDay != null) _mouseDay.SetActive(false);
        if (_mouseNight != null) _mouseNight.SetActive(true);
        if (_keyboardDay != null) _keyboardDay.SetActive(false);
        if (_keyboardNight != null) _keyboardNight.SetActive(true);
        if (_ps5Day != null) _ps5Day.SetActive(false);
        if (_ps5Night != null) _ps5Night.SetActive(true);
        if (_SNESDay != null) _SNESDay.SetActive(false);
        if (_SNESNight != null) _SNESNight.SetActive(true);
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
        StartDay();
    }
}
