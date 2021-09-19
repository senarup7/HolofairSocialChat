﻿/*
 * Chat UI Manager
 * All Chat UI & Implemenation
 * (c) OutRealXR
 * 19-09-2006
 */


using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using Sfs2X;
using Sfs2X.Logging;
using Sfs2X.Util;
using Sfs2X.Core;
using Sfs2X.Entities;
using Sfs2X.Entities.Data;
using Sfs2X.Entities.Variables;
public class Chat_UIManager : MonoBehaviour
{

    public List<friends_details> _friends_details = new List<friends_details>();


    // This Section Use for Further integration with Database
    #region
   /* public  int Sender_ID = 1111;
    public int Receiver_ID   { get; private set; */
    #endregion



    [SerializeField]
    GameObject friends_prefab;
  
   // [SerializeField]
   // Transform content;


    // Chat  Message Panel
    #region 'Chat Panel'
    [Tooltip ("Chat Panel Container")]
    [SerializeField]
    RectTransform ChatContainer;

    [Tooltip("Chat Panel Prefab")]
    [SerializeField]
    GameObject ChatPanelPrefab;
    #endregion
    // Class Reference
    #region
    Users_Chat users_Chat;
    UserMessages userMessages;
    #endregion
    // Input & Text message
    #region
    [SerializeField]
    Text userName;
    // Not in use, use for database
   // [SerializeField]
    //InputField inputTextMessage;

    // Buddy
    // Bubby list panel components
    public InputField buddyInput;
    public RectTransform buddyListContent;
    public GameObject buddyListItemPrefab;
    #endregion

    // Player Online Status Color Declaration
    #region
  /*  private Color32 ColorOffile = new Color(233, 0, 0, 0);
    private Color32 ColorAvailable = new Color(52, 241, 0, 0);
    private Color32 ColorAway = new Color(243, 205, 0, 0);
    private Color32 ColorOccupied = new Color(0, 145, 190, 0);*/
    public void ColorOffile(Image Player_status) => Player_status.GetComponent<Image>().color = new Color32(233, 0, 0, 0);
    public void ColorAvailable(Image Player_status) => Player_status.GetComponent<Image>().color = new Color32(52, 241, 255, 255);
    public void ColorAway(Image Player_status) => Player_status.GetComponent<Image>().color = new Color32(243, 205, 0, 0);
    public void ColorOccupied(Image Player_status) => Player_status.GetComponent<Image>().color = new Color32(0, 145, 190, 0);
   

    #endregion
    private void Awake()
    {
        /*
        users_Chat = FindObjectOfType<Users_Chat>();
        userMessages = FindObjectOfType<UserMessages>();
        */
    }

    //----------------------------------------------------------
    // Public methods for UI
    //----------------------------------------------------------


    /// <summary>
    /// Initialize SFS Connection 
    /// Set User Name
    /// </summary>
    /// <param name="user"></param>
    public void InitSFXConnection(string user)
    {

        if (ConnectionManager.IsInitialized)
        {

            ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.InitBuddyListRequest());

            // Set Chat Panel On
            FindObjectOfType<ChatManager>().SetChatPanelOn();

            // User Name for Chat top view
            userName.text = user;
        } else
        {
            return;
        }

        // Add event listeners
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_LIST_INIT, OnBuddyListInit);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_ERROR, OnBuddyError);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnObjectMessage);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_ONLINE_STATE_UPDATE, OnBuddyListUpdate);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_VARIABLES_UPDATE, OnBuddyListUpdate);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_ADD, OnBuddyListUpdate);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_REMOVE, OnBuddyListUpdate);
        ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_BLOCK, OnBuddyListUpdate);
        // ConnectionManager.sfsServer.AddEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnBuddyMessage);

        // Init Buddy List
        initBuddyList();
    }


    /// <summary>
    /// Initialization
    /// Retrieve History Chat
    /// </summary>
    private void initBuddyList()
    {
        // Remove current list content
        for (int i = buddyListContent.childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(buddyListContent.GetChild(i).gameObject);
        }
        buddyListContent.DetachChildren();


        // Generate Buddy List

        foreach (Buddy buddy in ConnectionManager.sfsServer.BuddyManager.BuddyList)
        {
            

            string buddyName = buddy.Name; // Required or the listeners will always receive the last buddy name

            // Close All Chat Panel
            
            GameObject t = Instantiate(friends_prefab);
            t.transform.SetParent(buddyListContent);
            t.GetComponent<ChatFriends_UI>().userName.text = buddyName;
            t.GetComponent<ChatFriends_UI>().userStatus.text = buddy.State;
            OnCloseChatPanelClick(buddyName);

        }
       


    }
    /// <summary>
    /// OPen Chat Panel On click User
    /// </summary>
    /// <param name="receverid"></param>
    void OpenChatMessages(string buddyName)
    {
        // TO DO - History Chat
        /*Receiver_ID = receverid;
       
        FindObjectOfType<UserMessages>().LoadHistoryChat(receverid, Sender_ID); // For future history chat implementation
        */


    }

    /// <summary>
    /// ChatPanel Open
    /// </summary>
    /// <param name="buddyName"></param>
    void OnOpenChatPanelClick(string buddyName)
    {
        // Get panel
        Transform panel = ChatContainer.Find(buddyName);

        if (panel != null)
        {
            panel.transform.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// ChatPanelClose
    /// </summary>
    /// <param name="buddyName"></param>
    void OnCloseChatPanelClick(string buddyName)
    {
        // Get panel
        Transform panel = ChatContainer.Find(buddyName);

        if (panel != null)
        {
            panel.transform.gameObject.SetActive(false);
        }
    }
    /**
	 * Sends a chat message to a buddy when SEND button is pressed.
	 */
    void OnSendMessageButtonClick(string buddyName)
    {
        // Get panel
        Transform panel = ChatContainer.Find(buddyName);

        if (panel != null)
        {

            Debug.Log(">>>>>>> OnSendMessageButtonClick    ");
            ChatPanel chatPanel = panel.GetComponent<ChatPanel>();

            string message = chatPanel.messageInput.text;
            

            // Add a custom parameter containing the recipient name,
            // so that we are able to write messages in the proper chat tab
            ISFSObject _params = new SFSObject();
            _params.PutUtfString("recipient", buddyName);

            Buddy buddy = ConnectionManager.sfsServer.BuddyManager.GetBuddyByName(buddyName);

            ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.BuddyMessageRequest(message, buddy, _params));

            chatPanel.messageInput.text = "";
            chatPanel.messageInput.ActivateInputField();
            chatPanel.messageInput.Select();
        }
    }
    /// <summary>
    /// Chat Message Send
    /// Use while message save into databasde
    /// </summary>
  /*  public void OnSendChatMessage()
    {
        int tempChatID=-1;
        bool isChatIDExists = false;
        // User Chat 
        for(int n=0;n< users_Chat.chat_IDs.Count; n++)
        {
            if ((Receiver_ID == users_Chat.chat_IDs[n].receiver_id && Sender_ID == users_Chat.chat_IDs[n].sender_id)
              || (Receiver_ID == users_Chat.chat_IDs[n].sender_id && Sender_ID == users_Chat.chat_IDs[n].receiver_id))
            {
                // Chat ID Already Exists 
                tempChatID = users_Chat.chat_IDs[n].chat_id;
                isChatIDExists = true;
                Debug.Log("Chat_ID Exists");
                break;
            }
            
            isChatIDExists = false;
        }

        // Already user have chat
        if (!isChatIDExists)
        {
            // Add Chat ID Records
            AddChatID(users_Chat.chat_IDs.Count+1);
            AddChatMessage();
        }
       /// Loop through messages between users
       for (int countMessage=0;countMessage< userMessages._chatMessage.Count; countMessage++)
        {
            
            if (tempChatID== userMessages._chatMessage[countMessage].chat_id)
            {
                
                if (System.DateTime.Now.ToString("MMMM dd yyyy") == userMessages._chatMessage[countMessage].chat_date)
                {
                    Debug.Log("..........." );
                    // User already have chat on that day
                    // Add Only Message here
                    AddMessage();
                }
                else
                {
                    // User new Message
                    // New Message with new date
                    AddChatMessage();
                }
            }
        }

     }
    /// <summary>
    /// 
    /// </summary>
    void AddChatID(int chID)
    {
        users_Chat.chat_IDs.Add(new Chat_IDS());

        
        users_Chat.chat_IDs[users_Chat.chat_IDs.Count - 1].chat_id = chID;
        users_Chat.chat_IDs[users_Chat.chat_IDs.Count - 1].sender_id = Sender_ID;
        users_Chat.chat_IDs[users_Chat.chat_IDs.Count - 1].receiver_id = Receiver_ID;
        users_Chat.chat_IDs[users_Chat.chat_IDs.Count - 1].chat_date = System.DateTime.Now.ToString("MMMM dd");
    }

    void AddMessage()
    {
        // Add Message

        int chatMSGCount = userMessages._chatMessage.Count;
        int msgCount = userMessages._chatMessage[userMessages._chatMessage.Count - 1]._messages.Count;
       // userMessages._chatMessage[userMessages._chatMessage.Count - 1]._messages = new List<messgages>();
        userMessages._chatMessage[chatMSGCount - 1]._messages.Add(new messgages());
        userMessages._chatMessage[chatMSGCount - 1]._messages[msgCount].msg_sender_id = Sender_ID;
        userMessages._chatMessage[chatMSGCount - 1]._messages[msgCount].chat_text = new List<string>();
        userMessages._chatMessage[chatMSGCount - 1]._messages[msgCount].chat_text.Add(inputTextMessage.text);

        // show messags on chat box
        userMessages.ShowMessagesOnChat(inputTextMessage.text.ToString());


    }


    void AddChatMessage()
    {

        // Chat Messages
        userMessages._chatMessage.Add(new chatMessage());
        userMessages._chatMessage[userMessages._chatMessage.Count - 1].chat_id = userMessages._chatMessage.Count;
        userMessages._chatMessage[userMessages._chatMessage.Count - 1].chat_date = System.DateTime.Now.ToString("MMMM dd yyyy");
        AddMessage();
    }*/

        /**
         * Initializes interface when buddy list data is received.
         */
    private void OnBuddyListInit(BaseEvent evt)
    {
        // Populate list of buddies
        OnBuddyListUpdate(evt);
        // Nick
      //  userName.text = (ConnectionManager.sfsServer.BuddyManager.MyNickName != null ? ConnectionManager.sfsServer.BuddyManager.MyNickName : "");
       


    }

    /**
 * Adds a buddy to the current user's buddy list.
 */
    public void OnAddBuddyButtonClick()
    {
        if (buddyInput.text == "")
        {
            Debug.LogError("Buddy Name Missing");
            return;
        }
        ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.AddBuddyRequest(buddyInput.text.ToString()));
        
        //StartCoroutine(SendRequest());

    }

    /// <summary>
    /// SendRequest send message to receipent 
    /// </summary>
    /// <returns></returns>
    IEnumerator SendRequest()
    {
        yield return new WaitForSeconds(0.1f);
        ISFSObject _params = new SFSObject();
        _params.PutUtfString("recipient", buddyInput.text);

        Buddy buddy = ConnectionManager.sfsServer.BuddyManager.GetBuddyByName(buddyInput.text);

        ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.BuddyMessageRequest("", buddy, _params));


    }

    /*
     * Clear All Buddy List
     * If Required to clear all buddylist
     * Interface UI not created 
     */
    public void ClearAllBuddyList()
    {
        foreach (Buddy buddy in ConnectionManager.sfsServer.BuddyManager.BuddyList)
        {
            ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.RemoveBuddyRequest(buddy.Name));
        }


    }

    /// <summary>
    /// Object Message Event Listener
    /// Request Send to Friend
    /// </summary>
    /// <param name="e"></param>
    void OnObjectMessage(BaseEvent evt)
    {

        
        bool isItMe = (bool)evt.Params["isItMe"];
        Buddy sender = (Buddy)evt.Params["buddy"];
        string message = (string)evt.Params["message"];
   
        Buddy buddy;
        if (isItMe)
        {
            string buddyName = (evt.Params["data"] as ISFSObject).GetUtfString("recipient");
            buddy = ConnectionManager.sfsServer.BuddyManager.GetBuddyByName(buddyName);
        }
        else
            buddy = sender;

        if (buddy != null)
        {
            // Open panel if needed
            OnChatBuddyButtonClick(buddy.Name);

            // Print message
            Transform panel = ChatContainer.Find(buddy.Name);
            ChatPanel chatPanel = panel.GetComponent<ChatPanel>();
            chatPanel.addMessage("<b>" + (isItMe ? "You" : buddy.Name) + ":</b> " + message,isItMe);

        }
    }
    /**
    * Populates the buddy list.
    */
    private void OnBuddyListUpdate(BaseEvent evt)
    {


       // Debug.Log("Buddy List Update");
        // Remove current list content
        for (int i = buddyListContent.childCount - 1; i >= 0; --i)
        {
            GameObject.Destroy(buddyListContent.GetChild(i).gameObject);
        }
        buddyListContent.DetachChildren();

        // Recreate list content
        foreach (Buddy buddy in ConnectionManager.sfsServer.BuddyManager.BuddyList)
        {
            //Debug.Log("Buddy Name "+ buddy.Name);
            GameObject newListItem = Instantiate(friends_prefab);
            newListItem.transform.SetParent(buddyListContent, false);
            ChatFriends_UI friendsUI = newListItem.GetComponent<ChatFriends_UI>();
            // Icon
            if (buddy.IsBlocked)
            {
                friendsUI.FriendsSelectButton.interactable = false;
                friendsUI.blockButton.transform.GetComponent<Image>().sprite = friendsUI.stateBlock; 
            }
            else
            {
                friendsUI.FriendsSelectButton.interactable = true;
                friendsUI.blockButton.transform.GetComponent<Image>().sprite = friendsUI.stateUnBlock; 

            }
            if (!buddy.IsOnline)
            {
               
                friendsUI.FriendsSelectButton.GetComponent<Button>().interactable = false;
                friendsUI.userStatus.text = "Offline";
              
            }
            else
            {
                string state = buddy.State;

                if (state == "Available")
                {
                    friendsUI.userStatus.text = "Available";
                 
                }
                else if (state == "Away") { 
                    friendsUI.userStatus.text = "Away";
                   
                }
                else if (state == "Occupied")
                {
                    friendsUI.userStatus.text = "Occupied";
                  
                }

            }
            // Buttons
            string buddyName = buddy.Name; // Required or the listeners will always receive the last buddy name
            friendsUI.GetComponent<ChatFriends_UI>().userName.text = buddyName;
            friendsUI.removeButton.onClick.AddListener(() => OnRemoveBuddyButtonClick(buddyName));
            friendsUI.blockButton.onClick.AddListener(() => OnBlockBuddyButtonClick(buddyName));
            friendsUI.FriendsSelectButton.GetComponent<Button>().onClick.AddListener(() => OnChatBuddyButtonClick(buddyName));
            

            // Add item to list
            newListItem.transform.SetParent(buddyListContent, false);

            // Also update chat panel if open
            Transform panel = ChatContainer.Find(buddyName);

            if (panel != null)
            {
                ChatPanel chatPanel = panel.GetComponent<ChatPanel>();
                chatPanel.buddy = buddy;
                chatPanel.closeButton.onClick.AddListener(() => OnCloseChatPanelClick(buddyName));
            }

        }
    }

    /**
 * Blocks/unblocks a buddy.
 */
    public void OnBlockBuddyButtonClick(string buddyName)
    {
        bool isBlocked = ConnectionManager.sfsServer.BuddyManager.GetBuddyByName(buddyName).IsBlocked;

        ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.BlockBuddyRequest(buddyName, !isBlocked));
    }
    /**
 * Removes a user from the buddy list.
 */
    public void OnRemoveBuddyButtonClick(string buddyName)
    {
        ConnectionManager.sfsServer.Send(new Sfs2X.Requests.Buddylist.RemoveBuddyRequest(buddyName));
    }


    /**
	 * Start a chat with a buddy.
	 */
    public void OnChatBuddyButtonClick(string buddyName)
    {

        Debug.Log("Buddy Chat Clicked " + buddyName);
        // Check if panel is already open; if yes bring it to front
        Transform panel = ChatContainer.Find(buddyName);



        if (panel == null)
        {
            GameObject newChatPanel = Instantiate(ChatPanelPrefab) as GameObject;
            ChatPanel chatPanel = newChatPanel.GetComponent<ChatPanel>();

            chatPanel.buddy = ConnectionManager.sfsServer.BuddyManager.GetBuddyByName(buddyName);
            //chatPanel.closeButton.onClick.AddListener(() => OnChatCloseButtonClick(buddyName));
            chatPanel.sendButton.onClick.AddListener(() => OnSendMessageButtonClick(buddyName));
            //chatPanel.messageInput.onEndEdit.AddListener(val => OnSendMessageKeyPress(buddyName));

            /*
			chatPanel.messageInput.onEndEdit.AddListener(val =>
				{
					if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
						Debug.Log("End edit on enter");
				});
	*/
            newChatPanel.transform.SetParent(ChatContainer, false);

        }
        else
        {
            OnOpenChatPanelClick(buddyName);
            panel.SetAsLastSibling();
        }
    }

    //----------------------------------------------------------
    // SmartFoxServer event listeners
    //----------------------------------------------------------
    /// <summary>
    /// OnConnectionLost
    /// </summary>
    /// <param name="evt"></param>
    private void OnConnectionLost(BaseEvent evt)
    {
        Debug.LogWarning("Connection was lost; reason is: " + (string)evt.Params["reason"]);

        // Remove SFS2X listeners and re-enable button
        reset();
    }


    //----------------------------------------------------------
    // SmartFoxServer event listeners
    //----------------------------------------------------------
    /// <summary>
    /// Buddy Error
    /// </summary>
    /// <param name="evt"></param>
    private void OnBuddyError(BaseEvent evt)
    {
        Debug.LogError("The following error occurred in the buddy list system: " + (string)evt.Params["errorMessage"]);
    }


    /// <summary>
    /// Reset / Remove Event Listeneter
    /// </summary>
    public void reset()
    {
        // Remove SFS2X listeners

        ConnectionManager.sfsServer.RemoveEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_LIST_INIT, OnBuddyListInit);
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_ONLINE_STATE_UPDATE, OnBuddyListUpdate);
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_MESSAGE, OnObjectMessage);
        // Add buddy-related event listeners during the SmartFox instance setup
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_VARIABLES_UPDATE, OnBuddyListUpdate);
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_ADD, OnBuddyListUpdate);
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_REMOVE, OnBuddyListUpdate);
        ConnectionManager.sfsServer.RemoveEventListener(SFSBuddyEvent.BUDDY_BLOCK, OnBuddyListUpdate);
        ConnectionManager.sfsServer.Disconnect();


    }

}
[System.Serializable]
public class friends_details
{
    public int user_id;
    public string avtar_image_url;
    public string user_name;
    public string user_Status;
    public string date;
    public int onlineStatus;
    public Button blockButton;
    public Button chatButton;
    public Button removeButton;
}