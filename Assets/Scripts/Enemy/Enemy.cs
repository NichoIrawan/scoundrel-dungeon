using Assets.Scripts;
using Assets.Scripts.Manager;
using TMPro;
using UnityEngine;

public class Enemies : MonoBehaviour
{
    [SerializeField] private TextMeshPro _powerText;

    public EnemiesScriptableObject Config;

    void Start()
    {
        _powerText.text = Config.Strength.ToString();
        _powerText.gameObject.SetActive(true);
    }

    void Update()
    {
    }
}

