using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField] private float _maxHealth;
    private float _currentHealth;
    private Animator _animator;

    void Start()
    {
        _animator = GetComponent<Animator>();
        _currentHealth = _maxHealth;  
    }

    public void TakeDamage(float damage)
    {
        _currentHealth -= damage;
        _animator.SetTrigger("Hurt");

        if(_currentHealth <= 0)
            Die();
    }

    void Die()
    {
        _animator.SetBool("IsDeath", true);

        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
    }
}
