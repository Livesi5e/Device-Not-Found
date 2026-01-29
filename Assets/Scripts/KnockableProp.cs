using UnityEngine;

public class KnockableProp : MonoBehaviour
{
    public bool isFallen = false;

    [Header("Standing State")]
    public Vector3 standingPositionOffset = Vector3.zero;
    public Vector3 standingRotation = Vector3.zero;

    [Header("Fallen State")]
    public Vector3 fallenPositionOffset = Vector3.zero;
    public Vector3 fallenRotation = new Vector3(90, 0, 0);

    [Header("Interaction")]
    public KeyCode interactKey = KeyCode.E;
    public int reward = 10;

    private bool playerInRange = false;
    private Vector3 basePosition;

    void Start()
    {
        basePosition = transform.position;
        ApplyState();
    }

    void Update()
    {
        if (!playerInRange || !isFallen) return;

        if (Input.GetKeyDown(interactKey))
        {
            isFallen = false;
            ApplyState();
            GameManager.Instance.AddMoney(reward);
        }
    }

    public void SetFallen(bool fallen)
    {
        isFallen = fallen;
        ApplyState();
    }

    private void ApplyState()
    {
        if (isFallen)
        {
            transform.position = basePosition + fallenPositionOffset;
            transform.rotation = Quaternion.Euler(fallenRotation);
        }
        else
        {
            transform.position = basePosition + standingPositionOffset;
            transform.rotation = Quaternion.Euler(standingRotation);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}