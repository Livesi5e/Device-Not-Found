using System;
using System.Collections.Generic;
using UnityEngine;
using MonumentGames.PlayerInventory;
using UnityEditor;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum Phase { Day, Night }

    [Header("State")]
    [SerializeField] private Phase _phase = Phase.Day;

    [Header("Money")]
    [SerializeField] private int _money = 0;
    [SerializeField] private int _rentCost = 20;
    
    [Header("Tablet Infos")]
    [SerializeField] private List<string> mouseInfo;
    [SerializeField] private List<string> keyboardInfo;
    [SerializeField] private List<string> ps5Info;
    [SerializeField] private List<string> SNESInfo;

    [Header("References")]
    [SerializeField] private List<DropoffArea> _places;
    [SerializeField] private List<Item> _tablets;
    [SerializeField] private Transform _player;
    [SerializeField] private Transform _daySpawnPoint;
    [SerializeField] private Animator _anim;
    [SerializeField] private List<Transform> tabletsStartPositons;
    [SerializeField] private Light sun;
    [SerializeField] private Camera cam;
    [SerializeField] private GameObject flashlight;
    [SerializeField] private GameObject dayPP;
    [SerializeField] private GameObject nightPP;
    [SerializeField] private AudioSource noise;
    
    [Header("Enemies (Day/Night Swap)")]
    [SerializeField] private GameObject _mouseDay;
    [SerializeField] private GameObject _mouseNight;
    [SerializeField] private GameObject _keyboardDay;
    [SerializeField] private GameObject _keyboardNight;
    [SerializeField] private GameObject _ps5Day;
    [SerializeField] private GameObject _ps5Night;
    [SerializeField] private GameObject _SNESDay;
    [SerializeField] private GameObject _SNESNight;
    
    [Header("Audio")]
    [SerializeField] private AudioSource _heartbeatSource;
    [SerializeField] private AudioSource nightStartSource;
    
    [Header("UI")]
    [SerializeField] private NightSummaryUI _nightSummaryUI;
    [SerializeField] private PhaseScreenUI _phaseScreenUI;
    private bool _isGameOver;
    
    [Header("Night Timer")]
    [SerializeField] private float _nightDurationSeconds = 10;
    private float _nightTimer;
    private bool _nightTimerActive;
    public float NightTimeLeft => _nightTimer;
    
    

    private int _moneyAtNightStart;
    private Phase time = Phase.Day;

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

    private void Start()
    {
        if (_phase == Phase.Night)
            StartNight();
        else
        {
            StartDay();
            ResetTablets();
        }
        
    }

    public void StartDay() {
        // Set Phase to day
        _phase = Phase.Day;
        SwitchDayLight();
        
        if (_mouseDay != null) _mouseDay.SetActive(true);
        if (_mouseNight != null) _mouseNight.SetActive(false);
        if (_keyboardDay != null) _keyboardDay.SetActive(true);
        if (_keyboardNight != null) _keyboardNight.SetActive(false);
        if (_ps5Day != null) _ps5Day.SetActive(true);
        if (_ps5Night != null) _ps5Night.SetActive(false);
        if (_SNESDay != null) _SNESDay.SetActive(true);
        if (_SNESNight != null) _SNESNight.SetActive(false);

        ResetTablets();
        
        foreach (var prop in FindObjectsByType<KnockableProp>(FindObjectsSortMode.None))
        {
            prop.SetFallen(false);
        }
        
    }

    private void ResetTablets()
    {
        // Generate random tablet information
        _tablets[0].GetComponent<Tablet>().SetTabletInfo(mouseInfo[Random.Range(0, mouseInfo.Count)]);
        _tablets[1].GetComponent<Tablet>().SetTabletInfo(keyboardInfo[Random.Range(0, keyboardInfo.Count)]);
        _tablets[2].GetComponent<Tablet>().SetTabletInfo(ps5Info[Random.Range(0, ps5Info.Count)]);
        _tablets[3].GetComponent<Tablet>().SetTabletInfo(SNESInfo[Random.Range(0, SNESInfo.Count)]);

        List<Transform> disposableList = new List<Transform>();
        for (int i = 0; i < 4; i++)
        {
            disposableList.Add(tabletsStartPositons[i]);
        }
        // Randomize Order of tablets on table
        for (int i = 0; i < 4; i++)
        {
            Transform tabletStartPos = disposableList[Random.Range(0, disposableList.Count)];
            disposableList.Remove(tabletStartPos);
            _tablets[i].transform.SetParent(tabletStartPos);
        }
        
        // Set tablets on top of table
        _tablets[0].transform.localPosition = Vector3.zero;
        _tablets[1].transform.localPosition = Vector3.zero;
        _tablets[2].transform.localPosition = Vector3.zero;
        _tablets[3].transform.localPosition = Vector3.zero;
        
        // Reset rotation of tablets
        for (int i = 0; i < 4; i++)
        {
            var rotation = _tablets[i].transform.localRotation;
            rotation.eulerAngles = new Vector3(180, -90, 90);
            _tablets[i].transform.localRotation = rotation;
        }
    }

    public void CheckDayOver() {
        Debug.Log("CheckDayOver aufgerufen");
        for (int i = 0; i < 4; i++) {
            if (_places[i].GetCurrentItem() != _tablets[i])
            {
                Debug.Log("Falsch");
                return;
            }
                
        }
        
        nightStartSource.Play();
        _anim.SetTrigger("Transition");
        
    }

    public void Update()
    {
        if (_phase != Phase.Night) return;
        if (!_nightTimerActive) return;
        
        _nightTimer -= Time.deltaTime;
        if (_nightTimer <= 0)
        {
            _nightTimerActive = false;
            EndNightTimeUp();
        }
    }

    public void StartNight()
    {
        Debug.Log("Start Night");
        _phase = Phase.Night;
        _moneyAtNightStart = _money;
        SwitchNightLight();

        if (_mouseDay != null) _mouseDay.SetActive(false);
        if (_mouseNight != null) _mouseNight.SetActive(true);
        if (_keyboardDay != null) _keyboardDay.SetActive(false);
        if (_keyboardNight != null) _keyboardNight.SetActive(true);
        if (_ps5Day != null) _ps5Day.SetActive(false);
        if (_ps5Night != null) _ps5Night.SetActive(true);
        if (_SNESDay != null) _SNESDay.SetActive(false);
        if (_SNESNight != null) _SNESNight.SetActive(true);

        foreach (var prop in FindObjectsByType<KnockableProp>(FindObjectsSortMode.None))
        {
            prop.SetFallen(true);
        }
        
        _nightTimer = _nightDurationSeconds;
        _nightTimerActive = true;
    }


    public void AddMoney(int amount)
    {
        _money += Mathf.Max(0, amount);
    }
    
    public void EndNightTimeUp()
    {
        if (_phase != Phase.Night) return;

        _nightTimerActive = false;
        _phase = Phase.Day;

        int earnedThisNight = Mathf.Max(0, _money - _moneyAtNightStart);
        int rent = _rentCost;

        int moneyBeforeRent = _money;
        _money = Mathf.Max(0, _money - rent);

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


    // Wird vom EnemyAI aufgerufen
    public void EndNightCaught()
    {  
        _nightTimerActive = false;
        
        if (_phase != Phase.Night) return;

        _phase = Phase.Day;
        
        if (_heartbeatSource != null && _heartbeatSource.isPlaying)
        {
            _heartbeatSource.Stop();
        }

        int earnedThisNight = Mathf.Max(0, _money - _moneyAtNightStart);
        int rent = _rentCost;

        int moneyBeforeRent = _money;
        _isGameOver = moneyBeforeRent < _rentCost;
        _money = Mathf.Max(0, _money - rent);
        

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
    
    public void AfterNightContinue()
    {
        Debug.Log("AfterNightContinue aufgerufen");

        if (_phaseScreenUI == null)
        {
            Debug.LogError("PhaseScreenUI ist im GameManager nicht zugewiesen!");
            return;
        }

        if (_isGameOver)
        {
            _phaseScreenUI.Show(
                "GAME OVER\nDu konntest die Miete nicht bezahlen.",
                "Neustart",
                RestartGame
            );
            return;
        }

        _phaseScreenUI.Show(
            "TAG BEGINNT",
            "Weiter",
            ContinueToDay
        );
    }

    private void ContinueToDay()
    {
        _phaseScreenUI.Hide();
        ConfirmNightSummary();
        LockCursor();
    }

    private void RestartGame()
    {
        // Szene neu laden
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
        
    }


    // 
    public void ConfirmNightSummary()
    {
        StartDay();
    }

    public void LockCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void SwitchDayLight()
    {
        sun.gameObject.SetActive(true);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
        cam.clearFlags = CameraClearFlags.Skybox;
        flashlight.SetActive(false);
        dayPP.SetActive(true);
        nightPP.SetActive(false);
        noise.Pause();
    }

    public void SwitchNightLight()
    {
        sun.gameObject.SetActive(false);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        cam.clearFlags = CameraClearFlags.Color;
        flashlight.SetActive(true);
        dayPP.SetActive(false);
        nightPP.SetActive(true);
        noise.Play();
    }
}
