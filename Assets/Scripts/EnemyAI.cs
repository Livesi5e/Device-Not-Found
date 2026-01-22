using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform _player;

    [Header("Vision")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField] private float viewAngle = 90f;
    [SerializeField] private float visionCheckInterval = 0.1f;
    [SerializeField] private LayerMask obstacleMask;

    [Header("Movement Speeds")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 3.8f;
    [SerializeField] private float searchSpeed = 2.6f;

    [Header("Search Behaviour")]
    [SerializeField] private float loseSightMemoryTime = 3.0f;
    [SerializeField] private float searchRadius = 4.0f;
    [SerializeField] private float arriveDistance = 0.7f;

    [Header("Patrol (optional)")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Chase stopping distance")]
    [SerializeField] private float chaseStoppingDistance = 1.6f;

    private NavMeshAgent _agent;

    private Vector3 _lastKnownPlayerPos;
    private float _lastTimeSeen;
    private float _nextVisionCheckTime;

    private Vector3 _currentSearchTarget;
    private bool _hasSearchTarget;

    private int _patrolIndex;

    private enum State
    {
        Patrol,
        Chase,
        Search
    }

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

        _agent.speed = patrolSpeed;

        if (patrolPoints != null && patrolPoints.Length > 0)
            SetPatrolDestination(patrolPoints[_patrolIndex].position);
    }

    private void Update()
    {
        if (_player == null || !_agent.isOnNavMesh)
            return;

        // ---------- Vision Check ----------
        if (Time.time >= _nextVisionCheckTime)
        {
            _nextVisionCheckTime = Time.time + visionCheckInterval;

            if (CanSeePlayer())
            {
                _lastKnownPlayerPos = _player.position;
                _lastTimeSeen = Time.time;
                _hasSearchTarget = false;
                _state = State.Chase;
            }
            else if (_state == State.Chase)
            {
                _state = State.Search;
            }
        }

        // ---------- State Behaviour ----------
        switch (_state)
        {
            case State.Chase:
                _agent.speed = chaseSpeed;
                _agent.stoppingDistance = chaseStoppingDistance;
                _agent.SetDestination(_player.position);
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

    // ================= SEARCH =================
    private void HandleSearch()
    {
        // Generate a new search target around the last known player position
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

        // Reached search target -> pick a new one
        if (_hasSearchTarget && !_agent.pathPending && _agent.remainingDistance <= arriveDistance)
        {
            _hasSearchTarget = false;
        }

        // Memory expired -> go back to patrol if available
        if (Time.time - _lastTimeSeen > loseSightMemoryTime)
        {
            _hasSearchTarget = false;

            if (patrolPoints != null && patrolPoints.Length > 0)
                _state = State.Patrol;
            // else: keeps searching instead of standing still
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

        // If no valid path, set destination again
        if (!_agent.hasPath || _agent.pathStatus != NavMeshPathStatus.PathComplete)
        {
            SetPatrolDestination(patrolPoints[_patrolIndex].position);
            return;
        }

        // Arrived -> next patrol point
        if (!_agent.pathPending && _agent.remainingDistance <= arriveDistance)
        {
            _patrolIndex = (_patrolIndex + 1) % patrolPoints.Length;
            SetPatrolDestination(patrolPoints[_patrolIndex].position);
        }
    }

    private void SetPatrolDestination(Vector3 desiredPos)
    {
        // Snap patrol destination to NavMesh to avoid "weird patrol jitter"
        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            _agent.SetDestination(hit.position);
        }
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

        // If a wall/obstacle is hit first, player is not visible
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

