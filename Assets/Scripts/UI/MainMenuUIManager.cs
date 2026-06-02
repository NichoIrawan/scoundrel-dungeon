using Assets.Scripts.SaveSystem;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIManager : MonoBehaviour
{
    [SerializeField] private GameObject _accountPanel;
    [SerializeField] private GameObject _settingsPanel;
    [SerializeField] private TMP_InputField _username;
    [SerializeField] private TMP_InputField _password;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _accountPanel.SetActive(false);
        _settingsPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnAccountClick()
    {
        _accountPanel.SetActive(true);
    }

    public void OnCloseClick()
    {
        _accountPanel.SetActive(false);
        _settingsPanel.SetActive(false);
    }

    public void OnSettingsClick()
    {
        _settingsPanel.SetActive(true);
    }

    public void OnLanguageSelected(string locale)
    {
        Assets.Scripts.Manager.LocalizationManager.Instance?.SwitchLanguage(locale);
    }

    public void OnLoginClicked()
    {
        var username = _username.text;
        var password = _password.text;
        if (Login(username, password))
        {
            Debug.Log($"[MainMenuUIManager] Login successful for user: {username}");
            _accountPanel.SetActive(false);
        }
    }

    public void OnRegisterClicked()
    {
        var username = _username.text;
        var password = _password.text;
        if (Register(username, password))
        {
            Debug.Log($"[MainMenuUIManager] Registration successful for user: {username}");
            _accountPanel.SetActive(false);
        }
    }

    public bool Register(string username, string password)
    {
        try
        {
            DataPersistenceManager.Instance.CreateAccount(username, password);
            DataPersistenceManager.Instance.Login(username, password);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenuManager] Register failed: {ex.Message}");
            return false;
        }
    }

    public bool Login(string username, string password)
    {
        try
        {
            DataPersistenceManager.Instance.Login(username, password);
            return true;
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[MainMenuManager] Login failed: {ex.Message}");
            return false;
        }
    }
}
