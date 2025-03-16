using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuManager : MonoBehaviour
{
    // Reference to the input field for the room ID
    [SerializeField] private TMP_InputField roomIDInputField;

    // Call this method when Start Game is pressed
    public void OnStartGame()
    {
        // Call your LobbyManager or similar to host a game
        Debug.Log("Starting game as host...");
    }

    // Call this method when Join Game is pressed
    public void OnJoinGame()
    {
        string roomId = roomIDInputField.text;
        if (!string.IsNullOrEmpty(roomId))
        {
            Debug.Log("Joining game with Room ID: " + roomId);
            // Pass roomId to your LobbyManager for joining a game
        }
        else
        {
            Debug.LogWarning("Room ID is empty!");
        }
    }
}
