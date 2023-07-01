using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StaminaScript : MonoBehaviour
{
    [SerializeField] private float _stamina;
    [SerializeField] private Slider _staminaBar;
    [SerializeField] private float _minStamina;
    [SerializeField] private float _maxStamina;
    static public float _staminaReturn;

    void Update()
    {
        GetStamina();
    }


    private void GetStamina()
    {
        if (_maxStamina > 100f)
            _maxStamina = 100f;

        _staminaBar.value = _stamina;

    }

}