using UnityEngine;

public class CircularMoviment : MonoBehaviour
{
    // Raio do círculo
    public float radius = 5f;
    // Velocidade angular em graus por segundo
    public float angularSpeed = 30f;

    // Variável para armazenar o ângulo atual
    private float angle;

    // Ponto central do movimento circular
    public Transform centerPoint;

    void Update()
    {
        // Calcula o deslocamento angular baseado no tempo e na velocidade
        angle += angularSpeed * Time.deltaTime;

        // Converte o ângulo para radianos
        float radians = angle * Mathf.Deg2Rad;

        // Calcula a nova posição do objeto no círculo
        float x = Mathf.Cos(radians) * radius;
        float z = Mathf.Sin(radians) * radius;

        // Atualiza a posição do objeto ao redor do ponto central
        Vector3 newPosition = new Vector3(x, transform.position.y, z) + centerPoint.position;
        transform.position = newPosition;

        // Define a rotação para fixar o dirigível na direção X positiva
        transform.rotation = Quaternion.Euler(0, 90, 0); // 90 graus no eixo Y para alinhar o objeto com o eixo X
    }
}
