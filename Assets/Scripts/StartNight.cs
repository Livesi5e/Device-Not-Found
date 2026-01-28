using UnityEngine;

public class StartNight : MonoBehaviour
{
    public void StartTheNight() {
        Debug.Log("Start Night");
        GameManager.Instance.StartNight();
    }
}
