
/*
 * Class ChatPanel.cs
 * Use for Chat Friends/Buddies datas provide by SFS 
 * User Data for user ui & interfaces
 * User Status, Send Message, User Input Message
 * Use it for future history chat
 * (c) OutRealXR
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sfs2X.Entities;

public class ChatPanel : MonoBehaviour {

	public Image stateIcon;
	public Button closeButton;

	public ScrollRect scrollRect;
	//public Image ChatBGImage;
	public Transform content;
	public Transform leftChatMessage;
	public Transform rightChatMessage;
	public Button sendButton;

	public InputField messageInput;
	public Text title;

	public Sprite IconAvailable;
	public Sprite IconAway;
	public Sprite IconOccupied;
	public Sprite IconOffline;
	public Sprite IconBlocked;

	private Buddy _buddy;

	public Buddy buddy
	{
		set {
			this._buddy = value;

			if (_buddy != null) {
				// Set panel name
				this.name = _buddy.Name;

				// Set panel title
				title.text = "Chat with " + (_buddy.NickName != null && _buddy.NickName != "" ? _buddy.NickName : _buddy.Name);

				// Set status icon and enable controls
				if (buddy.IsBlocked) {
					stateIcon.sprite = IconBlocked;
					messageInput.interactable = false;
					sendButton.interactable = false;
				}
				else
				{
					if (!buddy.IsOnline) {
						stateIcon.sprite = IconOffline;
						//messageInput.interactable = false;
						//sendButton.interactable = false;
					}
					else {
						string state = buddy.State;
						
						if (state == "Available")
							stateIcon.sprite = IconAvailable;
						else if (state == "Away")
							stateIcon.sprite = IconAway;
						else if (state == "Occupied")
							stateIcon.sprite = IconOccupied;

						messageInput.interactable = true;
						sendButton.interactable = true;
					}
				}
			}
		}

		get {
			return this._buddy;
		}
	}

	public void addMessage(string message,bool isItMe) {

		//chatText.text += message + "\n";
		Transform t;
		switch (isItMe)
		{
			case true:
				t = Instantiate(rightChatMessage, content.transform);
				t.transform.SetParent(content,false);
				t.GetComponent<Chat>().chatText.text = message;
				
				break;
			case false:
				t = Instantiate(leftChatMessage, content.transform);
				
				t.transform.SetParent(content,false);
				t.GetComponent<Chat>().chatText.text = message;
				break;
		}

		Debug.Log("Message Added");
		Canvas.ForceUpdateCanvases();

		// Scroll to bottom
		scrollRect.verticalNormalizedPosition = 0;
	}
}
