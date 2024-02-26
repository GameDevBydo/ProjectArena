using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{

    public static Player instance;

    private CharacterController controller;

    void Start()
    {
        instance = this;
        controller = gameObject.GetComponent<CharacterController>();
        //rope = gameObject.GetComponent<LineRenderer>();
        //rope.enabled = false;
        Cursor.lockState = CursorLockMode.Locked;
        ActivateSword();
    }

    void Update()
    {
        Movement();
        CamMovement();
        //if(Input.GetKeyDown(KeyCode.E)) CollectItens();
        if(hasSword && Input.GetKeyDown(KeyCode.Mouse0)) Attack();
        //if(hasGrapple)
        //{
        //    if(Input.GetKeyDown(KeyCode.Mouse1)) CreateGrapple();
        //    CheckGrappleIntegrity();
        //    PullGrapple(grapplingTimer);
        //    GrappleCooldown();
        //    UpdateRope();
        //}
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
            //if(isGrappled)
            //{
            //    CancelGrapple();
            //    playerVelocity.y = jumpStrength;
            //    jumpPS.Play();
            //    extraJumps--;
            //}
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

    public void ResetPosition()
    {
        Debug.Log("Resetou");
        controller.enabled = false;
        transform.position = new Vector3(-1.34697711f,8.71000004f,-13.4981785f);
        controller.enabled = true;
    }

    #region Combat
    [Header("Combat")]
    public Animation attackAnim;
    public GameObject sword, swordObj;
    bool hasSword = false, canAttack = true;

    public void ActivateSword()
    {
        swordObj.SetActive(true);
        hasSword = true;
    }
    public void Attack()
    {
        if(canAttack) StartCoroutine(AttackRoutine());
    }

    IEnumerator AttackRoutine()
    {
        canAttack = false; 
        sword.SetActive(true);
        swordObj.SetActive(false);
        sword.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().Play();
        attackAnim.Play();
        yield return new WaitForSeconds(attackAnim.clip.length);
        canAttack = true;
        sword.SetActive(false);
        swordObj.SetActive(true);
        sword.transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>().Stop();
    }

    //public void SlayEnemy(int loot)
    //{
    //    coins += loot;
    //    ControllerUI.instance.UpdateCoins(coins);
    //    kills++;
    //    ControllerUI.instance.UpdateKills(kills);
    //}

    void OnTriggerEnter(Collider col)
    {
        if(col.tag == "EnemyBlade")
        {
            
        }
    }
    #endregion

    /*[HideInInspector]
    public int coins = 0;
    [HideInInspector]
    public int kills = 0;
    [Header("Interactions")]
    public LayerMask chestLayer, itemLayer;
    [TextArea(1,3)]
    public string[] tooltips;
    void CollectItens()
    {
        Collider[] items = Physics.OverlapSphere(transform.position, 5, itemLayer);
        foreach(Collider item in items)
        {
            switch(item.name)
            {
                case "SwordItem":
                    ActivateSword();
                    Destroy(item.gameObject);
                    ControllerUI.instance.StartWriting(tooltips[0]);
                break;
                case "GrappleItem":
                    ActivateGrapple();
                    Destroy(item.gameObject);
                    ControllerUI.instance.StartWriting(tooltips[1]);
                break;
            }
        }
        Collider[] chests = Physics.OverlapSphere(transform.position, 5, chestLayer);
        foreach(Collider chest in chests)
        {
            if(!chest.GetComponent<Chest>().open)
            {
                coins+= chest.GetComponent<Chest>().loot;
                chest.GetComponent<Chest>().OpenChest();
            }
        }
        ControllerUI.instance.UpdateCoins(coins);
    }

    #region Grapple
    [Header("Grapple")]

    public Transform grapplePos;
    public float grappleMaxDis = 20;
    public LayerMask grappable;

    RaycastHit hit;
    Vector3 grappledPoint, initialPos;
    float grapplingTimer, grapplingDuration = 2, grapplingCD = 0;
    bool isGrappled, hasGrapple=false;

    LineRenderer rope;
    public GameObject grappleObj;

    public void ActivateGrapple()
    {
        hasGrapple = true;
        grappleObj.SetActive(true);
    }
    void CreateGrapple()
    {
        if(grapplingCD <= 0)
        {
            isGrappled = Physics.SphereCast(grapplePos.position, 5, Camera.main.transform.forward,out hit, grappleMaxDis, grappable);
            if(isGrappled)
            {
                grappledPoint = hit.transform.GetChild(0).transform.position;
                initialPos = transform.position;
                grapplingTimer = 0;
                DrawRope(grapplePos.position, grappledPoint);
            }
        }
    }

    void CheckGrappleIntegrity()
    {
        if(grappledPoint != null && isGrappled)
        {
            //if(Input.GetKeyUp(KeyCode.Mouse1))
            //{
            //    CancelGrapple();
            //}
            if(grapplingTimer>=1)
            {
                CancelGrapple();
            }
            else
            {
                grapplingTimer+= Time.deltaTime/grapplingDuration;
            }
        }
    }

    
    void CancelGrapple()
    {
        grappledPoint = grapplePos.position;
        isGrappled = false;
        grapplingCD = 1;
        DeleteRope();
        playerVelocity.y = jumpStrength*(grapplingTimer/2);
    }

    void PullGrapple(float time)
    {
        if(isGrappled)
        {
            transform.position = Vector3.Lerp(initialPos, grappledPoint, Mathf.Sqrt(time));
        }
    }

    void GrappleCooldown()
    {
        if(grapplingCD>0) grapplingCD-=Time.deltaTime*0.6f;
    }
    
    void DrawRope(Vector3 a, Vector3 b)
    {
        rope.enabled = true;
        rope.SetPosition(0, a);
        rope.SetPosition(1, b);
    }

    void UpdateRope()
    {
        if(isGrappled) rope.SetPosition(0, grapplePos.position);
    }

    void DeleteRope()
    {
        rope.enabled = false;
    }

    #endregion */

}