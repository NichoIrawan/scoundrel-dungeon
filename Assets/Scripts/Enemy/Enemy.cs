using Assets.Scripts;
using Assets.Scripts.Manager;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    [SerializeField] private TextMeshPro _powerText;

    public EnemiesScriptableObject Config;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _powerText.text = Config.Power.ToString();
        _powerText.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
