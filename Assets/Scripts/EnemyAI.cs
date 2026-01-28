using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;

    [Header("Day Reset / Home (optional)")]
    [SerializeField] private Transform _homePoint;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float visionCheckInterval = 0.1f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.8f;
    [SerializeField] private float searchSpeed = 2.6f;

    [Header("Chase stopping distance")]
    [SerializeField] private float chaseStoppingDistance = 1.6f;

    [Header("Search Behaviour")]
    [SerializeField] private float loseSightMemoryTime = 3.0f;
    [SerializeField] private float searchRadius = 4.0f;
    [SerializeField] private float arriveDistance = 0.7f;

    [Header("Patrol (optional)")]
    [SerializeField] private Transform[] patrolPoints;

    // ===== Horror: Chase Ramp =====
    [Header("Horror: Chase Ramp")]
    [SerializeField] private float _chaseRampTime = 4f;
    [SerializeField] private float _chaseSpeedMultiplierMax = 1.6f;
    private float _chaseTimer;

    // ===== Horror: Look Around =====
    [Header("Horror: Look Around (Search)")]
    [SerializeField] private float _lookAroundDuration = 1.8f;
    [SerializeField] private float _lookAroundTurnSpeed = 120f;
    private float _lookAroundTimer;

    private NavMeshAgent _agent;

    private Vector3 _lastKnownPlayerPos;
    private float _lastTimeSeen;
    private float _nextVisionCheckTime;

    private Vector3 _currentSearchTarget;
    private bool _hasSearchTarget;

    private int _patrolIndex;

    private enum State { Patrol, Chase, Search }
    private State _state = State.Patrol;

    private void Awake()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (_player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                _player = p.transform;
        }

        if (_agent.enabled)
        {
            _agent.speed = patrolSpeed;
            _agent.stoppingDistance = 0f;

            if (patrolPoints != null && patrolPoints.Length > 0)
                SetPatrolDestination(patrolPoints[_patrolIndex].position);
        }
    }

    private void Update()
    {
        // ===== DAY: Agent AUS =====
        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentPhase != GameManager.Phase.Night)
        {
            DisableAgentForDay();
            return;
        }

        // ===== NIGHT: Agent AN =====
        EnableAgentForNight();

        if (_player == null || !_agent.isOnNavMesh)
            return;

        // Vision check not every frame
        if (Time.time >= _nextVisionCheckTime)
        {
            _nextVisionCheckTime = Time.time + Mathf.Max(0.02f, visionCheckInterval);

            if (CanSeePlayer())
            {
                _lastKnownPlayerPos = _player.position;
                _lastTimeSeen = Time.time;
                _hasSearchTarget = false;
                _lookAroundTimer = 0f;
                _state = State.Chase;
            }
            else if (_state == State.Chase)
            {
                _state = State.Search;
            }
        }

        // Chase ramp timer
        if (_state == State.Chase)
            _chaseTimer = Mathf.Min(_chaseTimer + Time.deltaTime, _chaseRampTime);
        else
            _chaseTimer = 0f;

        switch (_state)
        {
            case State.Chase:
                HandleChase();
                break;

            case State.Search:
                _agent.speed = searchSpeed;
                _agent.stoppingDistance = 0f;
                HandleSearch();
                break;

            case State.Patrol:
                _agent.speed = patrolSpeed;
                _agent.stoppingDistance = 0f;
                HandlePatrol();
                break;
        }
    }

    // ================= DAY/NIGHT AGENT TOGGLE =================
    private void DisableAgentForDay()
    {
        if (_agent == null) return;
        if (!_agent.enabled) return;

        _agent.ResetPath();
        _agent.velocity = Vector3.zero;
        _agent.enabled = false;

        // State sauber zurücksetzen
        _state = State.Patrol;
        _hasSearchTarget = false;
        _chaseTimer = 0f;
        _lookAroundTimer = 0f;
    }

    private void EnableAgentForNight()
    {
        if (_agent == null) return;
        if (_agent.enabled) return;

        _agent.enabled = true;

        // Optional: Stabiler Start nach Enable
        if (_agent.isOnNavMesh)
            _agent.Warp(transform.position);

        _agent.speed = patrolSpeed;
        _agent.stoppingDistance = 0f;

        // Wenn Patrolpoints existieren, setze Ziel (damit er nicht "idle" bleibt)
        if (patrolPoints != null && patrolPoints.Length > 0)
            SetPatrolDestination(patrolPoints[_patrolIndex].position);
    }

    // ================= CHASE (with speed ramp) =================
    private void HandleChase()
    {
        float t = (_chaseRampTime <= 0.01f) ? 1f : (_chaseTimer / _chaseRampTime);
        float mul = Mathf.Lerp(1f, _chaseSpeedMultiplierMax, t);

        _agent.speed = chaseSpeed * mul;
        _agent.stoppingDistance = chaseStoppingDistance;

        _agent.SetDestination(_player.position);
    }

    // ================= SEARCH (with look-around) =================
    private void HandleSearch()
    {
        // Look-around phase
        if (_lookAroundTimer > 0f)
        {
            _lookAroundTimer -= Time.deltaTime;
            _agent.ResetPath();
            transform.Rotate(0f, _lookAroundTurnSpeed * Time.deltaTime, 0f);
            return;
        }

        // Memory expired -> patrol if possible
        if (Time.time - _lastTimeSeen > loseSightMemoryTime)
        {
            _hasSearchTarget = false;

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                _state = State.Patrol;
                return;
            }
        }

        // Generate a new search target around last known position
        if (!_hasSearchTarget)
        {
            Vector3 randomDir = Random.insideUnitSphere * searchRadius;
            randomDir.y = 0f;

            Vector3 candidate = _lastKnownPlayerPos + randomDir;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, searchRadius, NavMesh.AllAreas))
            {
                _currentSearchTarget = hit.position;
                _agent.SetDestination(_currentSearchTarget);
                _hasSearchTarget = true;
            }
        }

        // Reached target -> start look-around
        if (_hasSearchTarget && !_agent.pathPending && _agent.remainingDistance <= arriveDistance)
        {
            _hasSearchTarget = false;
            _lookAroundTimer = _lookAroundDuration;
        }
    }

    // ================= PATROL =================
    private void HandlePatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            _agent.ResetPath();
            return;
        }

        if (!_agent.hasPath || _agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            SetPatrolDestination(patrolPoints[_patrolIndex].position);
            return;
        }

        if (!_agent.pathPending && _agent.remainingDistance <= arriveDistance)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            SetPatrolDestination(patrolPoints[_patrolIndex].position);
        }
    }

    private void SetPatrolDestination(Vector3 desiredPos)
    {
        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
    }

    // ================= CATCH PLAYER =================
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null &&
            GameManager.Instance.CurrentPhase == GameManager.Phase.Night)
        {
            GameManager.Instance.EndNightCaught();
        }
    }

    // Optional: called by GameManager when caught
    public void ResetToHomeAndStop()
    {
        _state = State.Patrol;
        _hasSearchTarget = false;
        _chaseTimer = 0f;
        _lookAroundTimer = 0f;

        if (_agent != null && _agent.enabled)
        {
            _agent.ResetPath();
            _agent.velocity = Vector3.zero;
        }

        if (_homePoint == null) return;

        // Wenn Agent aktiv ist, Warp benutzen, sonst normal setzen
        if (_agent != null && _agent.enabled && _agent.isOnNavMesh)
            _agent.Warp(_homePoint.position);
        else
            transform.position = _homePoint.position;

        transform.rotation = _homePoint.rotation;
    }

    // ================= VISION =================
    private bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Vector3 target = _player.position + Vector3.up * 1.6f;

        Vector3 toPlayer = target - origin;
        float distance = toPlayer.magnitude;

        if (distance > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > viewAngle * 0.5f)
            return false;

        Vector3 dir = toPlayer.normalized;

        if (Physics.Raycast(origin, dir, distance, obstacleMask))
            return false;

        return true;
    }

    // ================= DEBUG =================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;

        Vector3 origin = transform.position + Vector3.up * 1.6f;
        Gizmos.DrawWireSphere(origin, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle * 0.5f, 0) * transform.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle * 0.5f, 0) * transform.forward;

        Gizmos.DrawLine(origin, origin + left * viewDistance);
        Gizmos.DrawLine(origin, origin + right * viewDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_lastKnownPlayerPos, 0.2f);
    }
}
