using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Move : MonoBehaviour
{

    public GameObject player;
    public GameObject[] enemies;
    public TMPro.TextMeshProUGUI scoreText;
    public Image youDied;
    public Image youWon;
    public Button restartButton;
    public Slider healthbar;
    public AudioSource coinPickupSound;
    public AudioSource swordSlashSound;
    public AudioSource owSound;
    public AudioSource enemyDead;
    public ParticleSystem blood;

    private int score;
    private float speed = 7f;
    private float gravity = 2000f;
    private Animator animator;
    float horizontalMovement;
    float verticalMovement;
    private CharacterController character;
    private Vector3 destination = Vector3.zero;

    private float attackTime = 1.5f;
    private float attackCounter = 0f;
    private GameObject currentEnemy;

    public int playerHealth = 200;
    public int minDmg = 1;
    public int maxDmg = 15;
    private bool isAttacking = false;

    public bool TakeHit(int minDmg = 0, int maxDmg = 15)
    {
        //hp.text = "HP: " + playerHealth;
        playerHealth -= UnityEngine.Random.Range(minDmg, maxDmg);
        blood.Play();
        healthbar.value = playerHealth;
        owSound.Play();
        bool isDead = playerHealth <= 0;
        if (isDead) _Die();
        return isDead;
    }

    private void _Die()
    {
        Destroy(player.gameObject);
        youDied.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    public void Restart() {
        SceneManager.LoadScene("Top-Down Demo 01");
    }
    void Start()
    {
        blood.Stop();
        youDied.gameObject.SetActive(false);
        youWon.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        character = GetComponent<CharacterController>();
        animator = player.GetComponent<Animator>();
        scoreText.text = "Score: 0";
        score = 0;
        isAttacking = false;
        healthbar.value = playerHealth;
    }

    void Update()
    {
        if (score >= 1400) {
            youWon.gameObject.SetActive(true);
            restartButton.gameObject.SetActive(true);
        }
        if (Input.GetKeyDown(KeyCode.Space)) // ATTACK STARTED:
        {
            animator.SetBool("isRunning", false);
            animator.SetBool("isAttacking", true);
            isAttacking = true;
        }
        if (isAttacking)
        {
            if (!currentEnemy)
            {
                // which enemy are we attacking
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (Vector3.Distance(transform.position, enemies[i].transform.position) < 3f) // enemy must be within 1m
                    {
                        currentEnemy = enemies[i];
                    }
                }
            }
            attackCounter += Time.deltaTime;
            if (attackCounter < attackTime) // STILL MID-ATTACK:
            {
                MoveController();
                return;
            }
            // ATTACK DONE:
            swordSlashSound.Play();
            attackCounter = 0f;

            EnemyManager enemyManager = currentEnemy.GetComponent<EnemyManager>();
            bool enemyIsDead = enemyManager.TakeHit(minDmg, maxDmg);

            if (enemyIsDead) // ENEMY IS DEAD:
            {
                animator.SetBool("isAttacking", false);
                animator.SetBool("isRunning", true);
                isAttacking = false;
                score += 100;
                scoreText.text = "Score: " + score;
                enemyDead.Play();
                // remove enemy
                for (int i = 0; i + 1 < enemies.Length; i++)
                {
                    if (enemies[i] == currentEnemy)
                    {
                        enemies[i] = enemies[enemies.Length - 1];
                        break;
                    }
                }
                Array.Resize(ref enemies, enemies.Length - 1);
                currentEnemy = null;
            }
        }
        else
        {
            MoveController();
        }
    }

    void MoveController()
    {

        horizontalMovement = Input.GetAxis("Horizontal");
        verticalMovement = Input.GetAxis("Vertical");
        bool isRunning = (horizontalMovement > 0.1f || horizontalMovement < -0.1f || verticalMovement > 0.1f || verticalMovement < -0.1f);
        animator.SetBool("isRunning", isRunning);
        if (isRunning) // ON THE MOVE:
        {
            isAttacking = false;
            animator.SetBool("isAttacking", false);
            destination.Set(horizontalMovement, 0, verticalMovement);
            destination = transform.TransformDirection(destination);
            destination *= speed;
            Vector3 movement = new Vector3(horizontalMovement, 0.0f, verticalMovement);
            player.transform.rotation = Quaternion.LookRotation(movement);
            destination.y -= gravity * Time.deltaTime;
            character.Move(destination * Time.deltaTime);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        // if you collided with a "Collectible" then eat coin and get points
        //if (other.gameObject.CompareTag("Collectible")) {
        coinPickupSound.Play();
        other.gameObject.SetActive(false); // eats the coin
        score += 20;
        scoreText.text = "Score: " + score;
    }
}
