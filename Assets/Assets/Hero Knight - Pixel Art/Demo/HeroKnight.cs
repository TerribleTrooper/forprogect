﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class HeroKnight : MonoBehaviour
{

    [Header("Для выносливости")]
    [SerializeField] private float m_stamina;
    [SerializeField] private Slider m_staminaBar;
    [SerializeField] private float m_staminaReturn;
    private bool m_isShield = false; // Проверка с щитом персонаж

    [Header("Для ударов")]
    [SerializeField] private Transform[] m_attackPoints;
    [SerializeField] private float m_attackRange = 0.5f;
    [SerializeField] private LayerMask m_enemyLayers;
    [SerializeField] private float m_attackDamage = 20;

    [Header("Для персонажа")]
    [SerializeField] float m_speed = 4.0f;
    [SerializeField] float m_jumpForce = 7.5f;
    [SerializeField] float m_rollForce = 6.0f;
    [SerializeField] bool m_noBlood = false;
    [SerializeField] GameObject m_slideDust;

    private Animator m_animator;
    private Rigidbody2D m_body2d;
    private Sensor_HeroKnight m_groundSensor;
    private Sensor_HeroKnight m_wallSensorR1;
    private Sensor_HeroKnight m_wallSensorR2;
    private Sensor_HeroKnight m_wallSensorL1;
    private Sensor_HeroKnight m_wallSensorL2;
    private bool m_isWallSliding = false;
    private bool m_grounded = false;
    private bool m_rolling = false;
    private int m_facingDirection = 1;
    private int m_currentAttack = 0;
    private float m_timeSinceAttack = 0.0f;
    private float m_delayToIdle = 0.0f;
    private float m_rollDuration = 8.0f / 14.0f;
    private float m_rollCurrentTime;

    // Use this for initialization
    void Start()
    {
        m_animator = GetComponent<Animator>();
        m_body2d = GetComponent<Rigidbody2D>();
        m_groundSensor = transform.Find("GroundSensor").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR1 = transform.Find("WallSensor_R1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorR2 = transform.Find("WallSensor_R2").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL1 = transform.Find("WallSensor_L1").GetComponent<Sensor_HeroKnight>();
        m_wallSensorL2 = transform.Find("WallSensor_L2").GetComponent<Sensor_HeroKnight>();
    }

    void Update()
    {
        // Increase timer that controls attack combo
        m_timeSinceAttack += Time.deltaTime;

        // Increase timer that checks roll duration
        if (m_rolling)
            m_rollCurrentTime += Time.deltaTime;

        // Disable rolling if timer extends duration
        if (m_rollCurrentTime > m_rollDuration)
            m_rolling = false;

        //Check if character just landed on the ground
        if (!m_grounded && m_groundSensor.State())
        {
            m_grounded = true;
            m_animator.SetBool("Grounded", m_grounded);
        }

        //Check if character just started falling
        if (m_grounded && !m_groundSensor.State())
        {
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
        }

        // -- Handle input and movement --
        float inputX = Input.GetAxis("Horizontal");

        // Добавление выпосливости
        if (inputX == 0 && m_isShield == false)
        {
            m_stamina += m_staminaReturn * Time.deltaTime;
        }

        // Swap direction of sprite depending on walk direction
        if (inputX > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
            m_facingDirection = 1;

        }

        else if (inputX < 0)
        {
            GetComponent<SpriteRenderer>().flipX = true;
            m_facingDirection = -1;
        }

        // Move
        if (!m_rolling && m_stamina >= 1f)
        {
            m_body2d.velocity = new Vector2(inputX * m_speed, m_body2d.velocity.y);
            m_stamina -= 1f * Time.deltaTime;
        }

        //Set AirSpeed in animator
        m_animator.SetFloat("AirSpeedY", m_body2d.velocity.y);

        // -- Handle Animations --
        //Wall Slide
        m_isWallSliding = (m_wallSensorR1.State() && m_wallSensorR2.State()) || (m_wallSensorL1.State() && m_wallSensorL2.State());
        m_animator.SetBool("WallSlide", m_isWallSliding);

        //Death
        if (Input.GetKeyDown("p") && !m_rolling)
        {
            m_animator.SetBool("noBlood", m_noBlood);
            m_animator.SetTrigger("Death");
        }

        //Attack
        else if (Input.GetMouseButtonDown(0) && m_timeSinceAttack > 0.25f && !m_rolling && m_stamina >= 5f)
        {
            Attack();
            m_currentAttack++;

            // Loop back to one after third attack
            if (m_currentAttack > 3)
                m_currentAttack = 1;

            // Reset Attack combo if time since last attack is too large
            if (m_timeSinceAttack > 1.0f)
                m_currentAttack = 1;

            // Call one of three attack animations "Attack1", "Attack2", "Attack3"
            m_animator.SetTrigger("Attack" + m_currentAttack);

            // Reset timer
            m_timeSinceAttack = 0.0f;

            m_stamina -= 5f;
        }

        // Block
        else if (Input.GetMouseButtonDown(1) && !m_rolling && m_stamina >= 1f)
        {
            m_animator.SetTrigger("Block");
            m_animator.SetBool("IdleBlock", true);
            m_isShield = true;
            m_stamina -= 30f * Time.deltaTime;
        }

        else if (Input.GetMouseButtonUp(1))
        {
            m_animator.SetBool("IdleBlock", false);
            m_isShield = false;
        }

        // Roll
        else if (Input.GetKeyDown("left shift") && !m_rolling && !m_isWallSliding && m_stamina >= 10f)
        {
            m_rolling = true;
            m_animator.SetTrigger("Roll");
            m_body2d.velocity = new Vector2(m_facingDirection * m_rollForce, m_body2d.velocity.y);
            m_stamina -= 10f;
        }

        //Jump
        else if (Input.GetKeyDown("space") && m_grounded && !m_rolling && m_stamina >= 10f)
        {
            m_animator.SetTrigger("Jump");
            m_grounded = false;
            m_animator.SetBool("Grounded", m_grounded);
            m_body2d.velocity = new Vector2(m_body2d.velocity.x, m_jumpForce);
            m_groundSensor.Disable(0.2f);
            m_stamina -= 10f;
        }

        //Run
        else if (Mathf.Abs(inputX) > Mathf.Epsilon)
        {
            // Reset timer
            m_delayToIdle = 0.05f;
            m_animator.SetInteger("AnimState", 1);
        }

        //Idle
        else
        {
            // Prevents flickering transitions to idle
            m_delayToIdle -= Time.deltaTime;
            if (m_delayToIdle < 0)
                m_animator.SetInteger("AnimState", 0);
        }
        // For Stamina
        ControleStamina();
    }

    // Animation Events
    // Called in slide animation.
    void AE_SlideDust()
    {
        Vector3 spawnPosition;

        if (m_facingDirection == 1)
            spawnPosition = m_wallSensorR2.transform.position;
        else
            spawnPosition = m_wallSensorL2.transform.position;

        if (m_slideDust != null)
        {
            // Set correct arrow spawn position
            GameObject dust = Instantiate(m_slideDust, spawnPosition, gameObject.transform.localRotation) as GameObject;
            // Turn arrow in correct direction
            dust.transform.localScale = new Vector3(m_facingDirection, 1, 1);
        }
    }
    // For Stamina
    void ControleStamina()
    {
        if (m_stamina > 100f)
            m_stamina = 100f;

        else if (m_stamina < 0f)
            m_stamina = 0f;

        m_staminaBar.value = m_stamina;
    }

    void Attack()
    {
        foreach (Transform _attackPoint in m_attackPoints)
        {
            Collider2D[] _hitEnemies = Physics2D.OverlapCircleAll(_attackPoint.position, m_attackRange, m_enemyLayers);
            foreach (Collider2D enemy in _hitEnemies)
            {
                enemy.GetComponent<EnemyScript>().TakeDamage(m_attackDamage);
            }
        }

    }

    void OnDrawGizmosSelected()
    {
        foreach (Transform _attackPoint in m_attackPoints)
        {
            if (m_attackPoints == null)
                return;
            Gizmos.DrawWireSphere(_attackPoint.position, m_attackRange);
        }

    }
}