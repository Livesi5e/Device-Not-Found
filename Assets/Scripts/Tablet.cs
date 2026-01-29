using TMPro;
using UnityEngine;

public class Tablet : MonoBehaviour
{
    [SerializeField] private string tabletInfo;
    
    [Header("3D Tablet Text")]
    [SerializeField] private TMP_Text worldText;

    public string GetTabletInfo()
    {
        return tabletInfo;
    }

    public void SetTabletInfo(string info)
    {
        tabletInfo = info;
        worldText.text = info;
    }
}
