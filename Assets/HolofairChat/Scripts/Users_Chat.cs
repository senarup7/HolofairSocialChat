using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Users_Chat : MonoBehaviour
{
    public List<Chat_IDS> chat_IDs = new List<Chat_IDS>();
}
[System.Serializable]
public class Chat_IDS
{
    public int chat_id;
    public int sender_id;
    public int receiver_id;
    public string chat_date;
}