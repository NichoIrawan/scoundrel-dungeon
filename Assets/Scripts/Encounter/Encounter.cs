using Assets.Scripts;
using Assets.Scripts.Manager;
using Assets.Scripts.ScriptableObjects;
using TMPro;
using UnityEngine;

public class Encounter : MonoBehaviour
{
    [SerializeField] private TextMeshPro _powerText;
    [SerializeField] private SpriteRenderer _spriteRenderer;


    public EncounterScriptableObject Config;


    void Start()
    {
        _spriteRenderer.sprite = Config.Artwork;
        castEncounterToType();
    }

    void Update()
    {
    }

    private void castEncounterToType()
    {
        if (Config is EnemiesScriptableObject enemies)
        {
            var config = (EnemiesScriptableObject)Config;
            _powerText.text = config.Strength.ToString();
        }
        else if (Config is ConsumablesScriptableObject consumableConfig)
        {
            var config = (ConsumablesScriptableObject)Config;
            _powerText.text = config.HealValue.ToString();
        }
        else if (Config is EquipmentsScriptableObject equipmentConfig)
        {
            var config = (EquipmentsScriptableObject)Config;
            _powerText.text = config.Power.ToString();
        }
        _powerText.gameObject.SetActive(true);
    }
}

