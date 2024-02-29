using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.Netcode;

public class Player : NetworkBehaviour
{

    public static Player instance;

    private CharacterController controller;

    Controller main;

    void Start()
    {
        instance = this;
        controller = gameObject.GetComponent<CharacterController>();
        main = Controller.instance;
    }

    void Update()
    {
        if(main.runStarted)
        {
            Movement();
            CamMovement();
            if(Input.GetKeyDown(KeyCode.Mouse0)) Attack();
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
        if(isGrounded)
        {
            extraJumps = 5;
            playerVelocity.y -=1*Time.deltaTime;
        } 

        float xDir = Input.GetAxis("Horizontal");
        float zDir = Input.GetAxis("Vertical");

        Vector3 move = transform.right * xDir + transform.forward * zDir;

        controller.Move(move * Time.deltaTime * movSpeed);

        if (Input.GetButtonDown("Jump"))
        {
            if(isGrounded)
            {
                playerVelocity.y = jumpStrength;
                jumpPS.Play();
            }
            else if(extraJumps>0)
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
        GREATSWORD,
        BOXER,
        RUSTROBOT,
        EMPTY
    }

    [HideInInspector]
    public characterID playerChar = characterID.EMPTY;
    
    public GameObject[] characterModels;
    public void SetCharacter(int charID)
    {
        if(charID<characterModels.Length)
        {
            for(int i = 0; i<3; i++)
            {
                if(charID==i) characterModels[i].SetActive(true);
                else characterModels[i].SetActive(false);
            }
            switch(charID)
            {
                case 0:
                    playerChar = characterID.GREATSWORD;
                    break;
                case 1:
                    playerChar = characterID.BOXER;
                    break;
                case 2:
                    playerChar = characterID.RUSTROBOT;
                    break;
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
    public Animation swordAttackAnim, leftPunch;
    public GameObject sword, swordObj;
    bool canAttack = true;

    public void Attack()
    {
        if(canAttack) 
        {
            switch((int)playerChar)
            {
                case 0:
                    StartCoroutine(SwordGuyAttackRoutine());
                    break;
                case 1:
                    StartCoroutine(LeftPunch());
                    break;
            }
        }
    }

    IEnumerator SwordGuyAttackRoutine()
    {
        canAttack = false; 
        sword.SetActive(true);
        swordObj.SetActive(false);
        sword.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().Play();
        swordAttackAnim.Play();
        yield return new WaitForSeconds(swordAttackAnim.clip.length);
        canAttack = true;
        sword.SetActive(false);
        swordObj.SetActive(true);
        sword.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().Stop();
    }

    IEnumerator LeftPunch()
    {
        canAttack = false;
        leftPunch.Play();
        yield return new WaitForSeconds(leftPunch.clip.length);
        canAttack = true;
    }






    #endregion

}