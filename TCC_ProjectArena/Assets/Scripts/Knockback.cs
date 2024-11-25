
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] float knockbackStrength = 10f;
    [SerializeField] float knockbackDuration = 0.2f;
    [SerializeField] float jumpSize;

    private CharacterController characterController;
    private Vector3 knockbackDirection;
    private float knockbackTimer;

    float knockbackResult;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
    public void SetKnockback(Vector3 direction, float knockbackPower)
    {
        knockbackDirection = direction.normalized;
        knockbackDirection.y = jumpSize;
        knockbackResult = knockbackStrength*knockbackPower;
        knockbackTimer = knockbackDuration;
    }
    void Update()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;    
            characterController.Move(knockbackDirection * knockbackResult * Time.deltaTime);
        }
    }
}
