using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;
using Unity.VisualScripting;

public class Enemy : NetworkBehaviour
{

    public int enemyTypeID; // Bot = 0, Bigbot = 1, Rat = 2, KamiCart = 3
    Transform player;
    NetworkVariable<bool> hitN = new();
    CharacterController controller;
    Knockback knockback;
    public float speed;
    public float maxSpeed;

    public float maxHitPoints;

    public NetworkVariable<float> hitPointsN = new();
    public NetworkVariable<int> rustStacksN = new();
    public int rustStacks;
    public float hitPoints;

    public bool waveStart = false;
    public bool activeGavity = true;
    public bool regainSpeed = true;

    public float damage = 5;

    Controller main;

    void Awake()
    {
        main = Controller.instance;
        hitPointsN.OnValueChanged += OnLifeChanged;
        rustStacksN.OnValueChanged += OnStacksChanged;
    }

    private void OnLifeChanged(float previousValue, float newValue)
    {
        UpdateLifeBar();
    }

    private void OnStacksChanged(int previousValue, int newValue)
    {
        UpdateStacksIcon();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            hitPointsN.Value = maxHitPoints;
            rustStacksN.Value = 0;
        }
    }


    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        knockback = gameObject.GetComponent<Knockback>();
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
                if(activeGavity) Gravity();
                if (!inRange) Movement();
                if(regainSpeed) RegainSpeed();
            }
            if (readyAttack)
            {
                switch(enemyTypeID)
                {
                    case 0:
                    case 1:
                        LightAttack("hitEnemy");
                    break;
                    case 2:
                        LightAttack("hitEnemyRat");
                    break;
                    case 3:
                        ExplosionAttack();
                    break;
                    default:
                        Debug.Log("Sem animação de combate.");
                    break;
                }
            }
        }
    }

    public TextMeshProUGUI enemyNameText; // BERNARDO BOTA ESSA VARIAVEL DO NOME PRA CADA UM DOS INIMIGOS PREFABS
    [Rpc(SendTo.Everyone)]
    public void SetEnemyNameRpc(string username)
    {
        enemyNameText.text = username;
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
    Vector3 gravityVector;
    private bool isGrounded;
    void Gravity()
    {
        isGrounded = controller.isGrounded;
        if(isGrounded)
        {
            gravityVector = Vector3.zero;
        }
        else
        {
            gravityVector += Vector3.down * gravity * Time.deltaTime;
            controller.Move(gravityVector * Time.deltaTime);
        }

    }

    void OnTriggerEnter(Collider collider)
    {
        if (IsHost)
        {
            if (waveStart)
            {
                if(collider.CompareTag("PlayerHitbox"))
                {
                    if(!hitN.Value)
                    {
                        Player hitPlayer = collider.GetComponentInParent<Player>();
                        speed = 0;
                        hitN.Value = true;
                        StartCoroutine(StopInvulnerability());
                        TakeDamage(hitPlayer.attackDamage);
                        knockback.SetKnockback(-transform.forward, hitPlayer.attackPushback);
                        regainSpeed = true;
                    }
                }
                else if(collider.CompareTag("PlayerProjectile"))
                {
                    if(!hitN.Value)
                    {
                        Projectile hitProjectile = collider.GetComponentInParent<Projectile>();
                        speed = 0;
                        hitN.Value = true;
                        StartCoroutine(StopInvulnerability());
                        TakeDamage(hitProjectile.damage);
                        TakeRust();
                        knockback.SetKnockback(-transform.forward, hitProjectile.pushback);
                        if(hitProjectile.pType != Projectile.projectileType.BOMB) Destroy(collider.gameObject);
                        regainSpeed = true;
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

    public GameObject stacksIcon;
    public TextMeshProUGUI stacksNumber;
    void UpdateStacksIcon()
    {
        stacksNumber.text = rustStacksN.Value.ToString();
    }

    #region Modifiers
    [HideInInspector]
    public float dmgTakenMod = 1.0f, dmgDealtMod = 1.0f;

    public void SetDamageTakenModifier(float mod)
    {
        dmgTakenMod = mod;
    }

    public void SetDamageDealtModifier(float mod)
    {
        dmgDealtMod = mod;
    }

    public void ResetDamageTakenModifier()
    {
        dmgTakenMod = 1.0f;
    }

    public void ResetDamageDealtModifier()
    {
        dmgDealtMod = 1.0f;
    }

    public void ResetDamageModifiers()
    {
        ResetDamageTakenModifier();
        ResetDamageDealtModifier();
    }

    #endregion
    
    public GameObject damageTextPopUp;
    void TakeDamage(float damage)
    {
        hitPointsN.Value -= Mathf.Floor(damage*dmgTakenMod);

        GameObject dmgTxt = Instantiate(damageTextPopUp, transform.position+ Vector3.up*2, Quaternion.identity).gameObject;
        dmgTxt.GetComponent<PopUpText>().damageValue = Mathf.Floor(damage*dmgTakenMod);
        NetworkObject dmgTextNetwork = dmgTxt.GetComponent<NetworkObject>();
        dmgTextNetwork.Spawn();

        if (hitPointsN.Value <= 0) Death();
    }

    void TakeRust()
    {
        stacksIcon.SetActive(true);
        rustStacksN.Value++;
        if(rustStacksN.Value==4)
        {
            TakeDamage(100*dmgTakenMod);
            rustStacksN.Value = 0;
            stacksIcon.SetActive(false);
        }
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
    public void LightAttack(string attackName)
    {
        if (canAttack)
        {
            CallAnimation(attackName);
        }
    }
    public void ExplosionAttack()
    {
        if (canAttack)
        {
            CallAnimation("explodeSelf");
        }
    }

    public GameObject explosion;
    public void a_SpawnExplosion()
    {
        Instantiate(explosion, transform.position, explosion.transform.rotation);
        TakeDamage(this.maxHitPoints);
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
    public void RegainSpeed()
    {
        if(speed<maxSpeed) 
        {
            speed = Mathf.Clamp(speed+Time.deltaTime*2, 0, maxSpeed);
        }
        else regainSpeed = false;
    }
}
