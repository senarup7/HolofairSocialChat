using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserMessages : MonoBehaviour
{

    public List<chatMessage> _chatMessage ;

    Users_Chat users_Chat;

    #region
    [Tooltip("Text Message Left Message Prefab here")]
    [SerializeField]
    Transform leftMessagePrefab;

    [Tooltip("Text Message Right Message Prefab here")]
    [SerializeField]
    Transform rightMessagePrefab;

    [Tooltip("Scroll view content game object")]
    [SerializeField]
    Transform content;

    public MSG_SENDER _MSG_SENDER;
    #endregion


    #region 


    #endregion
    private void Awake()
    {
        _MSG_SENDER = MSG_SENDER.NONE;

        users_Chat = FindObjectOfType<Users_Chat>();
       // LoadHistoryChat();
    }

    /// <summary>
    /// Load History Chat
    /// </summary>
    /// <param name="receiver_id"></param>
    /// <param name="sender_id"></param>
    public void LoadHistoryChat(int receiver_id,int sender_id)
    {
        DestroyChildObject(content);
    
       
        for (int i = 0; i < users_Chat.chat_IDs.Count; i++)
        {
           // int temp_user_id = _chatMessage[i].user_id;
           if(users_Chat.chat_IDs[i].receiver_id==receiver_id && users_Chat.chat_IDs[i].sender_id == sender_id) {

                for (int n = 0; n < _chatMessage.Count; n++)
                {
                    if (users_Chat.chat_IDs[i].chat_id == _chatMessage[n].chat_id)
                    {

                        LoadHistoryMessages(_chatMessage[n]._messages,sender_id,receiver_id);
                        break;
                    }

                }
            }
        }
    }

    void DestroyChildObject(Transform t)
    {
        foreach (Transform child in t.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    void LoadHistoryMessages(List<messgages> msgList,int sender_id, int receiver_id)
    {
        Transform chatmessage;
        for (int msg=0;msg< msgList.Count; msg++)
        {
            if (msgList[msg].msg_sender_id == sender_id)
            {
                _MSG_SENDER = MSG_SENDER.SENDER;
            }
            else
            {
                _MSG_SENDER = MSG_SENDER.RECEIVER;
            }
            switch (_MSG_SENDER)
            {
                case MSG_SENDER.SENDER:
                    Debug.Log("Chat Initiator");
                    chatmessage = Instantiate(leftMessagePrefab.transform);
                    chatmessage.transform.SetParent(content);
                    chatmessage.GetComponent<Chat>().chatText.text = msgList[msg].chat_text[0];
                    break;
                case MSG_SENDER.RECEIVER:
                    Debug.Log("Chat Receiver");
                    chatmessage = Instantiate(rightMessagePrefab.transform);
                    chatmessage.transform.SetParent(content);
                    chatmessage.GetComponent<Chat>().chatText.text = msgList[msg].chat_text[0];
                    break;
            }
        }
    }

    public void ShowMessagesOnChat(string message)
    {
        Transform chatmessage;
        
        Debug.Log("Chat Initiator");
        chatmessage = Instantiate(rightMessagePrefab.transform);
        chatmessage.transform.SetParent(content);
        chatmessage.GetComponent<Chat>().chatText.text = message;

    }
}
[System.Serializable]
public class chatMessage {

    public int chat_id;
    public int user_id;
    public string chat_date;
    public List<messgages> _messages=new List<messgages>();
}
[System.Serializable]
public class messgages
{
    public int msg_sender_id;
    public List<string> chat_text;
}
public enum MSG_SENDER {NONE, SENDER,RECEIVER}