using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhaseScreenUI : MonoBehaviour
{
    [SerializeField] private GameObject _root;
    [SerializeField] private TMP_Text _text;
    [SerializeField] private Button _button;
    [SerializeField] private TMP_Text _buttonText;

    private void Awake()
    {
        if (_root == null) _root = gameObject;
        Hide();
    }

    public void Show(string message, string buttonLabel, UnityEngine.Events.UnityAction onClick)
    {
        _root.SetActive(true);

        if (_text != null) _text.text = message;

        if (_button != null)
        {
            _button.onClick.RemoveAllListeners();
            if (onClick != null) _button.onClick.AddListener(onClick);
        }

        if (_buttonText != null) _buttonText.text = buttonLabel;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Hide()
    {
        if (_root != null) _root.SetActive(false);
        Time.timeScale = 1f;
    }
}