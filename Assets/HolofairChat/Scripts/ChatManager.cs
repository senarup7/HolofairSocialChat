/*
 * Class ChatManager.cs
 * Login Panel, Chat Panes On /Off
 * 
 * (c) OutRealXR
 */


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChatManager : MonoBehaviour
{

    [Tooltip ("Login Panel")]
    [SerializeField]
    public GameObject LoginPanel;

    [Tooltip("Chat Panel")]
    [SerializeField]
    public GameObject ChatPanel;

    [Tooltip ("Panels Add here")]
    public List<Transform> Panels = new List<Transform>();

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        // Login Panel On while enter
        SetPanelOffOn(LoginPanel.transform); 
    }


    /// <summary>
    /// Panel On  Off 
    /// Panels List set
    /// On Panel based on parameter
    /// </summary>
    /// <param name="panelOn"></param>
    public void SetPanelOffOn(Transform panelOn)
    {

        foreach(Transform t in Panels)
        {
            t.gameObject.SetActive(false);

        }
        
        panelOn.gameObject.SetActive(true);
    }

    /// <summary>
    /// Chat Panel On
    /// </summary>
    public void SetChatPanelOn()
    {
        SetPanelOffOn(ChatPanel.transform);
    }


    /// <summary>
    /// Login Panel ON
    /// </summary>
    public void SetLoginPanelOn()
    {
        SetPanelOffOn(LoginPanel.transform);
    }

    
}
