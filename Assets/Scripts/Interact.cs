using UnityEngine;

public class Interact : MonoBehaviour
{
    public void OnInteract()
    {
        GameManager.Instance.CheckDayOver();
    }
}
