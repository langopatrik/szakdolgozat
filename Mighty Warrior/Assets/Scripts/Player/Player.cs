﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

    public int levelAmount = 5;

    public CharacterController2D controller;
    public Animator animator;

    public float runSpeed = 40f;
    public float tempRunSpeed;
    float horizontalMove = 0f;

    bool playerAlive; 

    bool jumpAllow = true;
    bool jump = false;

    bool crouch = false;

    public LayerMask enemyLayers;
    public Transform attackPoint;

    public float attackRate = 2f;
    public float nextAttackTime = 0f;
    public float attackRange = 0.5f;
    public int attackDamage = 20;
    public int maxHealth = 100;
    int currentHealth;

    public HealthBar healthBar;

    public Sprite openedChest;
    public Sprite openedDoor;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
        playerAlive = true;
    }

    // Update is called once per frame (Inputs)
    void Update()
    {
        if (playerAlive)
        {
            horizontalMove = Input.GetAxisRaw("Horizontal") * runSpeed;
            animator.SetFloat("Speed", Mathf.Abs(horizontalMove));

            if (Input.GetButtonDown("Jump") && jumpAllow)
            {
                jump = true;
                animator.SetBool("IsJumping", true);
            }

            if (Input.GetButtonDown("Crouch"))
            {
                crouch = true;

            }
            else if (Input.GetButtonUp("Crouch"))
            {
                crouch = false;
            }

            // Limit the time between attacks
            if (Time.time >= nextAttackTime)
            {
                // Time speed has nothing to do with the Slash animation
                if (Input.GetButtonDown("Slash") && !crouch && !animator.GetBool("IsJumping") && !PauseMenu.GameIsPaused)
                {
                    // Not allowed to jump and move during the slash
                    tempRunSpeed = runSpeed;
                    runSpeed = 0f;
                    jumpAllow = false;

                    nextAttackTime = Time.time + 1f / attackRate;
                    animator.SetTrigger("Slash");
                    Invoke("Attack", 0.3f);
                }
            }
        }
    }

    // Update the character
    void FixedUpdate()
    {

        controller.Move(horizontalMove * Time.fixedDeltaTime, crouch, jump);
        jump = false;

    }


    /*
     * 
     *  FUNCTIONS
     * 
     */


    void Attack()
    {
        // Attack animation

        // Detect enemies in range of the attack
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Damage them
        foreach (Collider2D enemy in hitEnemies)
        {

            if(enemy.tag == "Breakable")
            {
                BreakItem(enemy);
            }
            else
            {
                enemy.GetComponent<Enemy>().TakeDamage(attackDamage);
                Debug.Log("We hit " + enemy.name);
            }
            
        }

        runSpeed = tempRunSpeed;
        jumpAllow = true;
    }

    public void TakeDamage(int damage)
    {
        if (!animator.GetBool("IsCrouching"))
        {
            if (currentHealth > 0)
            {
                currentHealth -= damage;
                healthBar.SetHealth(currentHealth);

                // Hurt animation
            }

            else if (playerAlive)
            {
                Die();
            }
        }
    }

    void Die()
    {
        Debug.Log("You Died!");
        Time.timeScale = 0.3f;
        horizontalMove = 0;
        animator.SetFloat("Speed", 0);
        playerAlive = false;

        // Die animation

        Invoke("ReloadScene", 1f);
        
    }

    void ReloadScene()
    {
        Time.timeScale = 1f;
        PauseMenu.GameIsPaused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Attack range circle
    void OnDrawGizmosSelected() 
    {
        if (attackPoint == null)
            return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    public void OnLanding()
    {
        animator.SetBool("IsJumping", false);
    }

    public void OnCrouching( bool isCrouching)
    {
        animator.SetBool("IsCrouching", isCrouching);
        
    }

    void BreakItem(Collider2D collider)
    {
        collider.GetComponent<SpriteRenderer>().sprite = null;
        collider.GetComponent<Collider2D>().enabled = false;
        collider.enabled = false;

        // Break animation
        // XP, coin and health can be looted
    }

    void UseItem(Collider2D collider)
    {

        if (!animator.GetBool("IsJumping") && playerAlive)
        {
            if (collider.gameObject.tag == "Chest")
            {

                collider.GetComponent<SpriteRenderer>().sprite = openedChest;
                collider.GetComponent<Collider2D>().enabled = false;
                collider.enabled = false;

                // XP, coin and health can be looted

            }

            else if (collider.gameObject.tag == "Door")
            {

                collider.GetComponent<SpriteRenderer>().sprite = openedDoor;
                collider.GetComponent<Collider2D>().enabled = false;
                collider.enabled = false;

                // Switch levels with doors

                for (int i = 0; i < levelAmount; i++)
                {
                    if(collider.gameObject.name == "Door" + i)
                    {
                        Select(i);
                    }
                }
            }

            Debug.Log("We used " + collider.gameObject.tag);
        }
    }

    public void Select(int levelIndex)
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + levelIndex);
    }


    /*
     * 
     *  TRIGGERS, COLLIDERS
     * 
     */


    public void OnTriggerEnter2D(Collider2D collider)
    {

    }


    public void OnTriggerStay2D(Collider2D collider)
    {
        if (Input.GetButtonDown("Use"))
        {
            UseItem(collider);
        }

        if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && collider.gameObject.tag != ("Breakable"))
        {
            TakeDamage(Enemy.attackDamage);
        }
    }


    public void OnTriggerExit2D(Collider2D collider)
    {

    }


    public void OnCollisionEnter2D(Collision2D collider)
    {

    }


    public void OnCollisionStay2D(Collision2D collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer("Enemy") && collider.gameObject.tag != ("Breakable"))
        {
            TakeDamage(Enemy.attackDamage);
            //Debug.Log("Player HP:" + currentHealth);
        }
    }


    public void OnCollisionExit2D(Collision2D collider)
    {

    }
}

/*
 * Debug.Log can log twice at collisions because the player consists of 2 colliders.
 * 
 */
