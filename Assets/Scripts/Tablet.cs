using UnityEngine;

public class Tablet : MonoBehaviour
{
    [SerializeField] private string tabletInfo;

    public string GetTabletInfo()
    {
        return tabletInfo;
    }

    public void SetTabletInfo(string info)
    {
        tabletInfo = info;
    }
}
