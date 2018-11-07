Thank for purchase!




//!!IMPORTANT!!///////////////////////////////
You need download "Photon Unity Networking 2 / Pun 2" in the Asset store for this asset if not big errors appears!
Here download => https://assetstore.unity.com/packages/tools/network/pun-2-free-119922

Need TextMeshPro in the Asset store for this asset if not big errors appears!
Here download => https://assetstore.unity.com/packages/essentials/beta-projects/textmesh-pro-84126


//!!IMPORTANT!!///////////////////////////////
For joining lobby in photon, you must not erase "PhotonNetwork.JoinLobby();" in "Scripts->Photon->Connexion.cs" line 58.




//!!CHANGE MAX PLAYERS BY ROOMS!!/////////////
Load the scene "index" in the folder named "Scene".
For change the number Maximum of player by room, go to the Hierarchy in asset and look the GameObject named "ConnexionPhoton". 
Look Component "Connexion.cs" in "ConnexionPhoton" and change, line 81, the "MaxPlayers = 8" by the desired number.

Then go to folder named "Resources" and drag and drop of the gameObject 
named "Btn_Room" to Hierarchy in "Canvas->ListRoom_panel->Scroll_View_Table->Viewport->Content_Room_panel". 
Then open the droped gameObject named "Btn_Room" and open in hierarchy the component and look gameobject "PlayerInRoom" and create or delete the gameObjects for by your number of maximum 
player in room. Example, for 6 players, delete the gameObjects "P8 and P7" so that only "06" gameObject in present, "P1, P2, P3, P4, P5 and P6". 
06 players = 06 gameobjects or 04 players = 04 gameobjects, etc.... Now add GetComponents "Image" of all "P1, P2, P3, P4, P5 or P6" to 
script "PlayerInRoomIcons.cs" in "PlayerInRoom" in Array "Avatar". Example, avatar[0] = P1; avatar[1] = P2; avatar[2] = P3; etc... 
Now, ajust the size Width and Height of the window of gameobject "PlayerInRoom" for adapt to size of the gameObject "Btn_Room". Is finish? 
Ok, now Drag and drop your gameobject named "Btn_Room" of the Hierarchy to the folder named "Resources" and overwrite existing "Btn_Room" 
for saving change.



//!!LOGIN PANEL!!/////////////////////////////
To modify login panel, go to Hierarchy and look in "Canvas->Login_panel".



//!!LOGIN!!///////////////////////////////////
After clicked the "Button" to login panel with your pseudo, launches the command "OnLoginButtonClicked()" line 21 in "Connexion.cs" which 
launches the command line 29 "PhotonNetwork.ConnectUsingSettings();" for connect to Photon server and add your pseudo line 28 with "PhotonNetwork.LocalPlayer.NickName".
After you connected to Photon, launches automatically line 66 "OnConnectedToMaster()" which launches line 71 "PhotonNetwork.JoinLobby();" for joining automatically lobby.




//!!DISPLAY ROOMS LIST!!////////////////////////
To modify "Rooms list panel", go to Hierarchy and look in "Canvas->ListRoom_panel".
As soon as you are logged in in the Lobby, launches automatically "OnJoinedLobby()" line 40 in "Connexion.cs" and with the command line 42
"templateUIClass.BtnCreatRoom.interactable = true;", unlock the button for "Create Room".




//!!CREATE ROOM!!/////////////////////////////
After clicked the "Create Room",  launches automatically command "OnCreateRoomButtonClicked()" in "Connexion.cs" line 76.
After create room, you automatically join the room and launches command automatically "OnJoinedRoom()" line 22 in "ListPlayer.cs".
Go to hierarchy and look named gameobject "GetPlayerList".





//!!LEAVE BUTTON IN ROOM!!//////////////////////
After click button "LEAVE ROOM", launches command "OnLeaveGameButtonClicked()" to "Connexion.cs" line 87 and you exit the room with command 
"PhotonNetwork.LeaveRoom ();" line 89.
As soon exit room, launches automatically command "OnLeftRoom()" in "ListPlayer.cs" line 59 and automatically join Lobby which 
launches "OnJoinedLobby()" line 71 in "ListPlayer.cs".





//!!ADD SMILEYS IN CHAT!!///////////////////////////////
In Asset, go to "Assets/TextMesh Pro/Resources/" and select "TMP Settings". Look "Default Sprite Asset" and add "EmojiOne" in "Assets/Lobby System Photon/Images/SmileyForTextMeshPro".
For change smiley's :
	-Edit texture "emojiOne" in "Assets/Lobby System Photon/Images/SmileyForTextMeshPro".
	-Click in "Assets/Lobby System Photon/Images/SmileyForTextMeshPro/EmojiOne" and open "SpriteList".
	-Each sprite has an ID, move "up" or "down" sprite for change ID.
	-Go To in hierarchy to "Room_panel/Chat/" and look "TChat.cs" component, the variable "shortcut smileys" and adjust shortcut in function of sprite ID.
	     Example "ShortcutSmileys[0] he will correspond Sprite with ID = 0".
	-Go to child of "Chat" in hierarchy and open gameObject "BtnSmileys" and look the "Buttons" of smiley's images. OnClick event with id correspond has an ID of 
	     variable "ShortcutSmileys" in "TChat.cs" and the Sprite ID.
	-Look in "TChat.cs" component, line 116 to 119. After client receive the variable "msg",  sending to all by MasterClient. The variable string "msg" is verif and if words 
		 correspond at words saved in variable "ShortcutSmileys", so replace word by smiley.







//!!CHAT!!//////////////////////////////////////
To modify "Chat", go to hierarchy in "Canvas->Room_panel->Chat" and look component "TChat.cs".
In the component "TChat.cs", the "Update()" function is used to pressed touch "ENTER" for send message to chat.
Line 28, if pressed touche keyboard "ENTER" and "isSelect" is "false" so "EventSystem.current.SetSelectedGameObject (TextSend.gameObject, null);" line 20, 
select the InputText for say.
Line 34, if pressed touche keyboard "ENTER" and "isSelect" is "true" and "InputText.text.Length > 0" so send the text in "InputText.text" to chat.
Line 42, if pressed touche keyboard "ENTER" and "isSelect" is "true" and "InputText.text.Length == 0" so unselected InputText, erase "InputText.text" and
"isSelect" is "false".
Line 39, send text to Master in function "sendChatOfMaster()" line 56.
Line 56 to 68, function send your pseudo and your message to Master with touche keyboard "ENTER".
Line 61 to 64, verify if you is Master.
Line 70 to 83, function send your pseudo and your message to Master with button "SEND".
Line 86 to 92, Master receives messages and pseudos and the send at all members in room.
Line 95 to 106, receives new connection in room and send to Master for saying in chat.
