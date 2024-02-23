using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Enemy : MonoBehaviour
{

    public int enemyTypeID;
    Transform player;
    bool hit;
    CharacterController controller;
    public float speed;

    public float maxHitPoints, hitPoints;
    
    public bool waveStart = false;

    Controller main;

    void Awake()
    {
        main = Controller.instance;
        hitPoints = maxHitPoints;
    }
    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
        player = GameObject.Find("Player").transform;
    }
    void Update()
    {
        Rotation();
        if(waveStart)
        {
            CheckPlayerDistance();
            if(!inRange) Movement();
        }
    }

    public void SetEnemyName(string username)
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

    void OnTriggerEnter(Collider collider)
    {
        if(waveStart)
        {
            if(collider.tag == "Blade")
            {
                if(!hit)
                {
                    //Instantiate(bloodPS, transform.position, bloodPS.transform.rotation);
                    hit = true;
                    StartCoroutine(StopInvulnerability());
                    TakeDamage(20);
                }
            }
        }
    }

    IEnumerator StopInvulnerability()
    {
        yield return new WaitForSeconds(0.3f);
        hit = false;
    }

    public Image lifeBar;

    void UpdateLifeBar()
    {
        lifeBar.fillAmount = hitPoints/maxHitPoints;
    }

    void TakeDamage(float damage)
    {
        hitPoints -= damage;
        UpdateLifeBar();
        if(hitPoints <=0) Death();
    }

    void Death()
    {
        main.EnemyKilled();
        Destroy(gameObject);
    }

    bool inRange = false, readyAttack = false, isAttacking = false;
    void CheckPlayerDistance()
    {
        inRange = Vector3.Distance(player.position, transform.position) <= 3;
        //if(inRange && !readyAttack)
        //{
        //    readyAttack = true;
        //}
        //else if(!inRange)
        //{
        //    readyAttack = false;
        //}
    }

    //void Attack()
    //{
    //    if(readyAttack && !isAttacking) StartCoroutine(AttackRoutine());
    //}

    //IEnumerator AttackRoutine()
    //{
    //    isAttacking = true;
    //    blade.SetActive(true);
    //    blade.transform.GetChild(2).GetChild(0).GetComponent<ParticleSystem>().Play();
    //    attackAnim.Play();
    //    yield return new WaitForSeconds(attackAnim.clip.length);
    //    blade.SetActive(false);
    //    blade.transform.GetChild(2).GetChild(0).GetComponent<ParticleSystem>().Stop();
    //    yield return new WaitForSeconds(2);
    //    isAttacking = false;
    //}


}
