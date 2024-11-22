using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Credits : MonoBehaviour
{
    [SerializeField]List<TMP_Text> texts;
    public void ActiveText(bool active)
    {
        for(int i = 0; i < texts.Count; i++)
        {
            texts[i].gameObject.SetActive(active);
        }
    }
}
