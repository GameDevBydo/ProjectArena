using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Controller : MonoBehaviour
{
    public static Controller instance;

    void Awake()
    {
        if(instance == null) instance = this;
        else Destroy(this.gameObject);
    }

    public GameObject[] enemyList;
    public void SpawnEnemy(string user, string enemyType)
    {  
        enemyType = enemyType.ToLower();
        int enemyId;
        switch(enemyType)
        {
            case "bot":
                enemyId = 0;
                break;

            case "bigbot":
                enemyId = 1;
                break;

            case "rat":
                enemyId = 2;
                break;

            default:
                enemyId = -1;
                break;
        }

        if(enemyId>=0)
        {
            Vector2 randomPos= Random.insideUnitCircle.normalized * 20;
            Vector3 spawnPos = new Vector3(randomPos.x, 0, randomPos.y);
            GameObject e = Instantiate(enemyList[enemyId], spawnPos, Quaternion.identity).gameObject;
            e.name = user + "'s " + e.name;
            e.GetComponent<Enemy>().SetEnemyName(user);
            Debug.Log(user + " selecionou " + enemyList[enemyId].name + " para a batalha.");
        }
        else 
        {
            Debug.Log("Inimigo selecionado não identificado.");
        }
    }


}
