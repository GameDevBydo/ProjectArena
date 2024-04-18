using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;

public class Enemy : NetworkBehaviour
{

    public int enemyTypeID;
    Transform player;
    NetworkVariable<bool> hitN = new();
    CharacterController controller;
    public float speed;

    public float maxHitPoints;

    public NetworkVariable<float> hitPointsN = new();
    public float hitPoints;

    public bool waveStart = false;
    public bool activegGavity = true;

    Controller main;

    void Awake()
    {
        main = Controller.instance;
        hitPointsN.OnValueChanged += OnLifeChanged;
    }

    private void OnLifeChanged(float previousValue, float newValue)
    {
        UpdateLifeBar();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) hitPointsN.Value = maxHitPoints;
    }


    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
    }

    NetworkVariable<float> locateTimer = new(0);
    void LocateNearestPlayer()
    {
        if (locateTimer.Value <= 0.0f)
        {
            float closestDistance = 1000;
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject p in players)
            {
                if (Vector3.Distance(transform.position, p.transform.position) < closestDistance)
                {
                    closestDistance = Vector3.Distance(transform.position, p.transform.position);
                    player = p.transform;
                    Debug.Log(player.name);
                }
            }
            locateTimer.Value = 5;
        }
        else
        {
            locateTimer.Value -= Time.deltaTime;
        }
    }

    void Update()
    {
        if (IsOwner)
        {
            LocateNearestPlayer();
            Rotation();
            if (waveStart)
            {
                CheckPlayerDistance();
              if(activegGavity) Gravity();
                if (!inRange) Movement();
            }
            if (readyAttack)
            {
                LightAttack();
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    public void SetEnemyNameRpc(string username)
    {
        transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = username;
    }


    void Rotation()
    {
        Vector3 rot = new Vector3(player.position.x, transform.position.y, player.position.z);
        transform.LookAt(rot);
    }

    void Movement()
    {
        controller.Move(transform.forward * speed * Time.deltaTime);
    }
    float gravity = 9.81f;
    private bool isGrounded;
    void Gravity()
    {
        isGrounded = controller.isGrounded;
        if (!isGrounded)
        {
            // Aplica a gravidade
            Vector3 gravityVector = Vector3.down * gravity;
            controller.Move(gravityVector * Time.deltaTime);
        }

    }

    void OnTriggerEnter(Collider collider)
    {
        if (IsHost)
        {
            if (waveStart)
            {
                if (collider.CompareTag("Blade") || collider.CompareTag("LeftGlove") || collider.CompareTag("RustScrap"))
                {
                    if (!hitN.Value)
                    {
                        hitN.Value = true;
                        StartCoroutine(StopInvulnerability());
                        TakeDamage(30);
                    }
                }
            }
        }
    }

    IEnumerator StopInvulnerability()
    {
        yield return new WaitForSeconds(0.3f);
        hitN.Value = false;
    }

    public Image lifeBar;
    void UpdateLifeBar()
    {
        lifeBar.fillAmount = hitPointsN.Value / maxHitPoints;
    }

    void TakeDamage(float damage)
    {
        hitPointsN.Value -= damage;
        if (hitPointsN.Value <= 0) Death();
    }

    void Death()
    {
        main.EnemyKilled();
        if (main.online) NetworkObject.Despawn();
        else Destroy(this.gameObject);
    }

    bool inRange = false, readyAttack = false;
    void CheckPlayerDistance()
    {
        inRange = Vector3.Distance(player.position, transform.position) <= 1.5f;
        if (inRange)
        {
            readyAttack = true;
        }
        else
        {
            readyAttack = false;
        }
    }
    [Header("Combat")]
    public Animator animator;
    public bool canAttack = true, attacking = false;
    public void LightAttack()
    {
        if (canAttack)
        {
            CallAnimation("hitEnemy");
        }
    }
    public void CallAnimation(string stateName)
    {
        canAttack = false;
        attacking = true;
        animator.Play(stateName);
        Debug.Log("PlayAttack");
    }
    public void StopAttacking()
    {
        attacking = false;
        canAttack = true;
        Debug.Log("Parei de atacar");
    }
}
