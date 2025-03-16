using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private InputField roomIDInputField;
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private Text roomIdDisplayText;
    [SerializeField] private Text statusText; // Durum mesajları için ek metin alanı

    void Start()
    {
        // Başlangıçta metinleri temizle
        if (roomIdDisplayText != null)
            roomIdDisplayText.text = "Room ID: -";

        if (statusText != null)
            statusText.text = "";

        // LobbyManager olaylarına abone ol
        if (lobbyManager != null)
        {
            lobbyManager.OnRoomIdCreated += HandleRoomCreated;
            lobbyManager.OnRoomJoinFailed += HandleRoomJoinFailed;
            lobbyManager.OnRoomJoinSuccess += HandleRoomJoinSuccess;
        }
    }

    // Oda ID oluşturulduğunda çağrılır
    private void HandleRoomCreated(string roomId)
    {
        if (!string.IsNullOrEmpty(roomId))
        {
            roomIdDisplayText.text = "Room ID: " + roomId;
            statusText.text = "Room created successfully!";
            statusText.color = Color.green;
        }
        else
        {
            roomIdDisplayText.text = "Room ID: Failed";
            statusText.text = "Failed to create room.";
            statusText.color = Color.red;
        }
    }

    // Odaya katılma başarısız olduğunda çağrılır
    private void HandleRoomJoinFailed(string error)
    {
        statusText.text = "Error: " + error;
        statusText.color = Color.red;
    }

    // Odaya başarılı şekilde katılınca çağrılır
    private void HandleRoomJoinSuccess()
    {
        statusText.text = "Successfully joined the room!";
        statusText.color = Color.green;
    }

    public void OnStartGame()
    {
        statusText.text = "Creating room...";
        statusText.color = Color.yellow;
        lobbyManager.HostGame();
    }

    public void OnJoinGame()
    {
        string roomId = roomIDInputField.text;
        if (!string.IsNullOrEmpty(roomId))
        {
            statusText.text = "Joining room...";
            statusText.color = Color.yellow;
            lobbyManager.JoinGame(roomId);
        }
        else
        {
            statusText.text = "Error: Room ID is empty!";
            statusText.color = Color.red;
        }
    }

    private void OnDestroy()
    {
        // Abone olduğumuz olaylardan çıkış
        if (lobbyManager != null)
        {
            lobbyManager.OnRoomIdCreated -= HandleRoomCreated;
            lobbyManager.OnRoomJoinFailed -= HandleRoomJoinFailed;
            lobbyManager.OnRoomJoinSuccess -= HandleRoomJoinSuccess;
        }
    }
}