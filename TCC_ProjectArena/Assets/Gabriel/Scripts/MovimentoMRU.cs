using UnityEngine;

public class MovimentoMRU : MonoBehaviour
{
    // Velocidade de movimento em unidades por segundo
    public float velocidade = 5f;
    public GameObject[] Targerts;
    public int index, n;
    public Transform target;

    // Direção do movimento (pode ser qualquer vetor unitário, como para o eixo X, Y ou Z)
    public Vector3 direcao = Vector3.forward;

    private void Start()
    {
        target = Targerts[0].transform;
    }

    // Update é chamado uma vez por frame
    void Update()
    {

        direcao = target.position - transform.position;

        // Mover o objeto ao longo da direção definida com a velocidade constante
        //transform.position += direcao.normalized * velocidade * Time.deltaTime;

        
        if(direcao.magnitude > 0.1f)
        {
            transform.position += direcao.normalized * velocidade * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(direcao);
        }
        else
        {
            n++;
            index = n % Targerts.Length;
            target = Targerts[index].transform;
        }
    }
}