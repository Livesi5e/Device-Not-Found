using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField] private float dayTime = 10;
    private Light light;
    private float currentTime = 0;

    void Awake()
    {
        light = GetComponent<Light>();
    }

    // Update is called once per frame
    void Update()
    {
        currentTime += 1 * Time.deltaTime;
        light.transform.rotation = Quaternion.Euler(new Vector3(180 / dayTime * currentTime, light.transform.rotation.eulerAngles.y, light.transform.rotation.eulerAngles.z));
    }
}
