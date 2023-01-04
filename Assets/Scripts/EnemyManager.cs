using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyManager : MonoBehaviour
{
    public Transform[] waypoints;
    public Transform player;
    public AudioSource armSwing;
    public AudioSource coinPickupSound;
    public ParticleSystem coins;
    public ParticleSystem blood;

    public Slider healthbar;
    //public AudioSource enemyDead;
    public float speed = 2f;
    public float fov = 6f;
    public float attackRange = 1f;
    public float waitTime = 2f;
    public int maxHealth = 30;
    public int _healthpoints;
    private int currentWaypointIndex;
    private Animator animator;
    private float waitCounter = 0f;
    private bool isWaiting = false;
    private float attackTime = 1.5f;
    private float attackCounter = 0f;


    private void Awake()
    {
        _healthpoints = maxHealth;
        animator = transform.GetComponent<Animator>();
        animator.SetBool("isWalking", true);
        waitCounter = 0f;
        isWaiting = false;
        healthbar.value = _healthpoints;
        coins.Stop();
        blood.Stop();
    }

    public bool TakeHit(int minDmg = 0, int maxDmg = 15)
    {
        _healthpoints -= UnityEngine.Random.Range(minDmg, maxDmg);
        healthbar.value = _healthpoints;
        bool isDead = _healthpoints <= 0;
        if (isDead) _Die();
        else blood.Play();
        return isDead;
    }

    private void _Die()
    {
        healthbar.gameObject.SetActive(false);
        coins.transform.position = transform.position;
        coins.Play();
        coinPickupSound.Play();
        gameObject.SetActive(false);
        // Destroy(gameObject);
    }

    private void Patrol()
    {
        if (isWaiting)
        {
            waitCounter += Time.deltaTime;
            if (waitCounter < waitTime)
            {
                return;
            }
            isWaiting = false;
            animator.SetBool("isWalking", true);
        }
        animator.SetBool("isWalking", true);
        Transform currentWaypoint = waypoints[currentWaypointIndex];
        if (Vector3.Distance(transform.position, currentWaypoint.position) < 0.01f)
        { // we got to a waypoint...
            transform.position = currentWaypoint.position;
            waitCounter = 0f;
            isWaiting = true;
            animator.SetBool("isWalking", false);
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, speed * Time.deltaTime);
            Vector3 lookDirection = currentWaypoint.position - transform.position;
            lookDirection.Normalize();
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), speed * Time.deltaTime);
        }
    }

    private void GoToTarget()
    {
        if (Vector3.Distance(transform.position, player.position) < attackRange)
        { // we got to enemy...
            healthbar.value = _healthpoints;
            healthbar.gameObject.SetActive(true);
            Attack();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.Normalize();
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), speed * Time.deltaTime);
        }
    }

    private void Attack()
    {
        animator.SetBool("isWalking", false);
        animator.SetBool("isAttacking", true);
        attackCounter += Time.deltaTime;
        if (attackCounter < attackTime)
        {
            return;
        }
        armSwing.Play();
        bool playerIsDead = player.gameObject.GetComponent<Move>().TakeHit(0, 15);
        if (playerIsDead)
        {
            animator.SetBool("isAttacking", false);
            animator.SetBool("isWalking", true);
            //enemyDead.Play();
        }
        else
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isAttacking", true);
            attackCounter = 0f;
        }
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) < fov)
        {
            GoToTarget();
        }
        else
        {
            Patrol();
        }
    }
}
