using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.Rendering.Universal;


public class Player : NetworkBehaviour
{

    public static Player instance;

    private CharacterController controller;

    Controller main;

    [SerializeField] private Camera playerOwnCamera;

    public string playerName;


    void Awake()
    {
        playerChar.OnValueChanged += LoadNewModel;
        life.OnValueChanged = OnLifeChanged;
    }

    private void LoadNewModel(characterID previousValue, characterID newValue)
    {
        SetCharacter((int)playerChar.Value);
    }

    void Start()
    {
        controller = gameObject.GetComponent<CharacterController>();
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
        else SetCharacter((int)playerChar.Value);
    }

    void Update()
    {

        if (IsOwner)
        {
            if (main.runStartedN.Value)
            {
                if (!attacking) Movement();
                CamMovement();
                if (Input.GetKeyDown(KeyCode.Mouse0)) LightAttack();
                if (Input.GetKeyDown(KeyCode.Mouse1)) HeavyAttack();
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
    void Movement() // Gets the speed (after modifiers) and the inputs to move. Uses CharacterController
    {
        isGrounded = Physics.Raycast(transform.GetChild(0).GetChild(0).transform.position, Vector3.down, 0.2f);
        if (isGrounded)
        {
            extraJumps = 5;
            playerVelocity.y -= 1 * Time.deltaTime;
        }

        float xDir = Input.GetAxis("Horizontal");
        float zDir = Input.GetAxis("Vertical");

        Vector3 move = transform.right * xDir + transform.forward * zDir;

        controller.Move(move * Time.deltaTime * movSpeed);

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                playerVelocity.y = jumpStrength;
                jumpPS.Play();
            }
            else if (extraJumps > 0)
            {
                playerVelocity.y = jumpStrength;
                jumpPS.Play();
                extraJumps--;
            }
        }

        playerVelocity.y += gravityValue * Time.deltaTime;
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
        Debug.Log("Setou a posição");
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


    public void SetCharacter(int modelID)
    {
        if (modelID < characterModels.Length)
        {
            for (int i = 0; i < 3; i++)
            {
                if (modelID == i) characterModels[i].SetActive(true);
                else characterModels[i].SetActive(false);
            }
        }
        else
        {
            Debug.Log("Não existe personagem com esse valor.");
        }
    }

    #endregion


    #region Combat
    [Header("Combat")]
    public Animator animator;

    public GameObject sword, swordObj;
    public bool canAttack = true, attacking = false;

    public void LightAttack()
    {
        if (canAttack)
        {
            string attackName = playerChar.Value switch
            {
                characterID.Greatsword => "swordL1",
                characterID.Boxer => "boxerL1",
                characterID.RustRobot => "robotL1",
                _ => null
            };

            CallAnimation(attackName);
        }
    }

    public void HeavyAttack()
    {
        if (canAttack)
        {
            string attackName = playerChar.Value switch
            {
                characterID.Greatsword => "swordH1",
                characterID.Boxer => "boxerH1",
                _ => null
            };
            CallAnimation(attackName);
        }
    }

    public void CallAnimation(string stateName)
    {
        canAttack = false;
        attacking = true;
        animator.Play(stateName);
    }

    public void StopAttacking()
    {
        attacking = false;
        canAttack = true;
    }

    public Transform projectileSpawn;
    public void SpawnProjectile(GameObject projectile)
    {
        GameObject e = Instantiate(projectile, projectileSpawn.position, projectileSpawn.rotation).gameObject;
        NetworkObject eNetworkObject = e.GetComponent<NetworkObject>();
        eNetworkObject.Spawn();
        e.GetComponent<Rigidbody>().AddForce(e.transform.forward * 10, ForceMode.Impulse);
    }


    [Header("Life")]
    [SerializeField] NetworkVariable<float> life = new();
    [SerializeField] float maxlife;

    public void RemoveLife(float val)
    {
        if (life.Value > 0) life.Value -= val;
        Controller.instance.hudPlayer.SetValBarLife(life.Value, maxlife);
    }
    private void OnLifeChanged(float previousValue, float newValue)
    {
        Controller.instance.hudPlayer.SetValBarLife(life.Value, maxlife);
    }
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            RemoveLife(10);
        }
    }


    //IEnumerator SwordGuyAttackRoutine()
    //{
    //    //canAttack = false; 
    //    //animator.Play(attackName);
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