using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMenu : MonoBehaviour
{
    public void ExecuteEvent()
    {
        Controller.instance.PlayAsHost();
        UIController.instance.ActivePanelLobby(true);
    }
}
