
using UnityEngine;

public class Knockback : MonoBehaviour
{
    [SerializeField] float knockbackStrength = 10f;
    [SerializeField] float knockbackDuration = 0.2f;
    [SerializeField] float jumpSize;

    private CharacterController characterController;
    private Vector3 knockbackDirection;
    private float knockbackTimer;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
    public void SetKnockback(Vector3 direction)
    {
        knockbackDirection = direction.normalized;
        knockbackDirection.y = jumpSize;
        knockbackTimer = knockbackDuration;
    }
    void Update()
    {
        if (knockbackTimer > 0)
        {
            knockbackTimer -= Time.deltaTime;    
            characterController.Move(knockbackDirection * knockbackStrength * Time.deltaTime);
        }
    }
}
