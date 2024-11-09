using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;



public class Player : NetworkBehaviour
{

    public static Player instance;

    [SerializeField]private CharacterController controller;
    public int playerNumber;

    Controller main;

    [SerializeField] public Camera playerOwnCamera;

    public string playerName;
    void Awake()
    {
        for (int i = 0;i<3;i++) characterModels[i].SetActive(false);
        playerName = Controller.instance.playerTempName;
        playerChar.OnValueChanged += LoadNewModel;
        life.OnValueChanged = OnLifeChanged;
    }

    private void LoadNewModel(characterID previousValue, characterID newValue)
    {
        if ((int)newValue < characterModels.Length)
        {
            for (int i = 0; i < 3; i++)
            {
                if ((int)newValue == i) characterModels[i].SetActive(true);
                else characterModels[i].SetActive(false);
            }
        }
        else
        {
            Debug.Log("Não existe personagem com esse valor.");
        }
        if(IsOwner)
        {
            UIController.instance.ChangeClassIcons((int)newValue);
            maxlife = 120 - (int)newValue*20;
            movSpeed = (int)newValue == 1 ? 14 : 10;
        }
    }

    void Start()
    {
        //controller = gameObject.GetComponent<CharacterController>();
        main = Controller.instance;
        if (!main.online) instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            playerOwnCamera.depth += 2;
            instance = this;
        }
        else LoadNewModel(playerChar.Value, playerChar.Value);

        if(IsHost) SetPosition(new Vector3(-3.01f, 2.78f, 2.88155f));
        Controller.instance.ConnectedClients(this);
    }

    void Update()
    {

        if (IsOwner)
        {
            if (main.runStartedN.Value && life.Value>0.0f)
            {
                /*if(playerChar.Value != characterID.Greatsword) */GroundedMovement();
                /*else if(!attacking) GroundedMovement();*/
                VerticalMovement();
                CamMovement();
                if (Input.GetKeyDown(KeyCode.Mouse0)) LightAttack();
                if (Input.GetKeyDown(KeyCode.Mouse1)) HeavyAttack();


                //Cheats
                if(Input.GetKeyDown(KeyCode.F1)) RemoveLife(-100);
                if(Input.GetKeyDown(KeyCode.F2)) ChangeUltLoad(10);
            }
        }
    }


    // Basic WASD Movement
    [Header("Movement")]
    public bool isGrounded;
    private Vector3 playerVelocity;
    public float movSpeed, jumpStrength, gravityValue = -9.81f;
    public int extraJumps;
    public ParticleSystem jumpPS;
    void GroundedMovement() // Gets the speed (after modifiers) and the inputs to move. Uses CharacterController 
    {
        float xDir = Input.GetAxis("Horizontal");
        float zDir = Input.GetAxis("Vertical");

        Vector3 move = transform.right * xDir + transform.forward * zDir;

        controller.Move(move * Time.deltaTime * movSpeed);
    }

    void VerticalMovement()
    {
        isGrounded = Physics.Raycast(transform.GetChild(0).transform.position, Vector3.down, 0.2f);
        if (isGrounded)
        {
            extraJumps = 0;
            playerVelocity.y = 0;
        }
        else
        {
            playerVelocity.y += gravityValue * Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                playerVelocity.y = jumpStrength;
                //jumpPS.Play();
            }
        }

        Mathf.Clamp(playerVelocity.y, -9.81f, 1000);
        controller.Move(playerVelocity * Time.deltaTime);
    }

    // Simple Third-person camera with empty pivot on head. Can change sense.
    public Transform camAim;
    public float mouseSense;
    float xRotation;
    void CamMovement()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSense * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSense * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90, 90);

        camAim.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    public void SetPosition(Vector3 pos)
    {
        controller.enabled = false;
        transform.position = pos;
        controller.enabled = true;
    }


    #region Classes and Characters

    public enum characterID
    {
        Greatsword,
        Boxer,
        RustRobot,
        Empty
    }

    [HideInInspector]
    public NetworkVariable<characterID> playerChar = new(characterID.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public GameObject[] characterModels;

    #endregion


    #region Combat
    [Header("Combat")]
    public Animator currentAnimator;
    public OwnerNetworkAnimator[] networkAnimators;

    public GameObject sword, swordObj;
    public bool canAttack = true, attacking = false;

    public int ultLoad = 0;

 
    public int attackDamage = 20, attackPushback = 20;
    public List<SO_AttackProperty> swordAttack;
    public List<SO_AttackProperty> boxerAttack;
    public List<SO_AttackProperty> robotAttack;

    public void LightAttack()
    {
        if (canAttack)
        {
            string attackName = playerChar.Value switch
            {
                characterID.Greatsword => "SoldierL1",
                characterID.Boxer => "BoxerL1",
                characterID.RustRobot => "RobotL1",
                _ => null
            };
            CallAnimation("IDLE");
            CallAnimation(attackName);
            CallAttack(attackName);
            Invoke(nameof(StopAttacking), 3f);
        }
    }

    public void HeavyAttack()
    {
        if (canAttack)
        {
            string attackName = playerChar.Value switch
            {
                characterID.Greatsword => "SoldierH1",
                characterID.Boxer => "BoxerH1",
                characterID.RustRobot => "RobotH1",
                _ => null
            };
            CallAnimation("IDLE");
            CallAnimation(attackName);
            CallAttack(attackName);
            Invoke(nameof(StopAttacking), 3f);
        }
    }
    public void CallAttack(string stateName)
    {
        switch(stateName)
        {
            case "SoldierL1": 
                attackDamage = swordAttack[0].GetAttack();
                attackPushback = swordAttack[0].GetKnockback();
                 break;
            case "SoldierH1":
                attackDamage = swordAttack[1].GetAttack();
                attackPushback = swordAttack[1].GetKnockback();
                break;
            case "BoxerL1":
                attackDamage = boxerAttack[0].GetAttack();
                attackPushback = boxerAttack[0].GetKnockback();
                break;
            case "BoxerH1":
                attackDamage = boxerAttack[1].GetAttack();
                attackPushback = boxerAttack[1].GetKnockback();
                break;
            case "RobotL1":
                 break;
            case "RobotH1":
                break;
            default:
                Debug.Log("Sem informações sobre os valores desta animação.");
                break;
        }
    }

    public void CallAnimation(string stateName)
    {
        canAttack = false;
        attacking = true;
        networkAnimators[(int)playerChar.Value].Animator.Play(stateName);
    }

    public void StopAttacking()
    {
        Debug.Log("Parou o ataque.");
        attacking = false;
        canAttack = true;
    }

    public Transform[] projectileSpawn;
    public Transform bombSpawn;
    public GameObject projectile, bomb;

    [Rpc(SendTo.Server)]
    public void SpawnRegularProjectile_ServerRpc(int spawnID)
    {
        GameObject e = Instantiate(projectile, projectileSpawn[spawnID].position, projectileSpawn[spawnID].rotation).gameObject;
        NetworkObject eNetworkObject = e.GetComponent<NetworkObject>();
        eNetworkObject.Spawn();
    }

    [Rpc(SendTo.Server)]
    public void SpawnBombProjectile_ServerRpc()
    {
        GameObject e = Instantiate(bomb, bombSpawn.position, bombSpawn.rotation).gameObject;
        NetworkObject eNetworkObject = e.GetComponent<NetworkObject>();
        eNetworkObject.Spawn();
    }

    public void ChangeUltLoad(int value)
    {
        ultLoad = Math.Clamp(ultLoad+value, 0, 100);
        UIController.instance.ChangeUltLoad(ultLoad);
    }



    [Header("Life")]
    [SerializeField] NetworkVariable<float> life = new(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] float maxlife;

    public void PlayerDead()
    {
        main.OpenDeathScreen();
    }
    public void RemoveLife(float val)
    {
        if(IsOwner)
        {
            life.Value = Math.Clamp(life.Value-val, 0, maxlife);
            if (life.Value <= 0) 
            {
                PlayerDead();
            }
        }
    }
    private void OnLifeChanged(float previousValue, float newValue)
    {
        if(IsOwner)Controller.instance.hudPlayer.SetValBarLife(life.Value, maxlife);
    }
    private void OnTriggerEnter(Collider other) 
    {
        switch(other.tag)
        {
            case "Enemy":
                Enemy enemy = other.gameObject.GetComponentInParent<Enemy>();
                RemoveLife(enemy.damage*enemy.dmgDealtMod);
                break;
            case "Explosion":
                RemoveLife(20);
                break;
            case "EnemyProjectile":
                EnemyProjectile enemyP = other.gameObject.GetComponent<EnemyProjectile>();
                Debug.Log("AI AI AI TIRO TIRO");
                RemoveLife(5);
                break; 
            default:
                Debug.Log("Dano sem tag");
                break;
        }
    }


    //IEnumerator SwordGuyAttackRoutine()
    //{
    //    //canAttack = false; 
    //    //currentAnimator.Play(attackName);
    //    //attacking = true;
    //    //yield return new WaitForSeconds(swordAnim.clip.length);
    //    //canAttack = true;
    //    //attacking = false;
    //}

    //IEnumerator LeftPunch()
    //{
    //    //canAttack = false;
    //    //attacking = true;
    //    //boxerAnim.Play();
    //    //yield return new WaitForSeconds(boxerAnim.clip.length);
    //    //attacking = false;
    //    //canAttack = true;
    //}






    #endregion

}