using System.Collections;
using UnityEngine;

public class Dash : MonoBehaviour
{
    [SerializeField] float deshStrength = 10f;
    [SerializeField] float deshDuration = 0.2f;
    private CharacterController characterController;
    private Vector3 deshDirection;
    private float deshTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
    public void SetDash(Vector3 direction, float knockbackPower)
    {
        deshDirection = direction.normalized;
        deshStrength = knockbackPower;
        deshTimer = deshDuration;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.W))
        {
            Vector3 dir= transform.position- new Vector3(transform.position.x, transform.position.y,transform.position.z+ 2000);
            SetDash( dir, 10);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.S))
        {
            Vector3 dir= transform.position- new Vector3(transform.position.x, transform.position.y,transform.position.z- 2000);
            SetDash( dir, 10);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.A))
        {
            Vector3 dir= transform.position - new Vector3(transform.position.x - 2000, transform.position.y,transform.position.z);
            SetDash( dir, 10);
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift) && Input.GetKey(KeyCode.D))
        {
             Vector3 dir= transform.position - new Vector3(transform.position.x+ 2000, transform.position.y,transform.position.z);
            SetDash(dir, 10);

        }
        if (deshTimer > 0)
        {
            deshTimer -= Time.deltaTime;
            characterController.Move(deshDirection * deshStrength * Time.deltaTime);
        }
    }


}