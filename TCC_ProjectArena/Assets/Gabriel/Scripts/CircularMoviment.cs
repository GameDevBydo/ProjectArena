using UnityEngine;

public class CircularMoviment : MonoBehaviour
{
    // Raio do c�rculo
    public float radius = 5f;
    // Velocidade angular em graus por segundo
    public float angularSpeed = 30f;

    // Vari�vel para armazenar o �ngulo atual
    private float angle;

    // Ponto central do movimento circular
    public Transform centerPoint;

    void Update()
    {
        // Calcula o deslocamento angular baseado no tempo e na velocidade
        angle += angularSpeed * Time.deltaTime;

        // Converte o �ngulo para radianos
        float radians = angle * Mathf.Deg2Rad;

        // Calcula a nova posi��o do objeto no c�rculo
        float x = Mathf.Cos(radians) * radius;
        float z = Mathf.Sin(radians) * radius;

        // Atualiza a posi��o do objeto ao redor do ponto central
        Vector3 newPosition = new Vector3(x, transform.position.y, z) + centerPoint.position;
        transform.position = newPosition;

        // Define a rota��o para fixar o dirig�vel na dire��o X positiva
        transform.rotation = Quaternion.Euler(0, 90, 0); // 90 graus no eixo Y para alinhar o objeto com o eixo X
    }
}
