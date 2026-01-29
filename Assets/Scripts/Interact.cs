using UnityEngine;

public class Interact : MonoBehaviour
{
    public void OnInteract()
    {
        Debug.Log("On Interact");
        GameManager.Instance.CheckDayOver();
    }
}
