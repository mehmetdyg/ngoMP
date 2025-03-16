using UnityEngine;
using Unity.Netcode;
using Steamworks;

public class LobbyManager : MonoBehaviour
{
    // Optional: Reference to an InputField if you want to input lobby ID manually
    public UnityEngine.UI.InputField roomIdInput;

    // Store the current lobby id from Steam
    private CSteamID currentLobbyId;

    // Callbacks for Steamworks lobby events
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<LobbyEnter_t> lobbyEntered;

    private void Start()
    {
        // Register callbacks from Steamworks
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    /// <summary>
    /// Called when the "Start Game" button is pressed.
    /// Creates a new Steam lobby and then starts hosting the game.
    /// </summary>
    public void HostGame()
    {
        // Create a private lobby with a maximum of 4 players
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 4);
    }

    /// <summary>
    /// Callback triggered by Steam when a lobby is created.
    /// </summary>
    /// <param name="callback">Lobby creation callback data</param>
    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            Debug.Log("Lobby created with Steam ID: " + currentLobbyId);

            // Store host info in the lobby metadata
            SteamMatchmaking.SetLobbyData(currentLobbyId, "HostAddress", SteamUser.GetSteamID().ToString());

            // Start hosting the game through Netcode
            NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.LogError("Lobby creation failed with result: " + callback.m_eResult);
        }
    }

    /// <summary>
    /// Called when the "Join Game" button is pressed.
    /// Joins an existing lobby using a provided lobby (or room) ID.
    /// </summary>
    public void JoinGame()
    {
        // Try to parse the lobby ID from the input field
        if (ulong.TryParse(roomIdInput.text, out ulong lobbyId))
        {
            SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
        }
        else
        {
            Debug.LogError("Invalid lobby ID provided!");
        }
    }

    /// <summary>
    /// Callback triggered when joining a lobby.
    /// </summary>
    /// <param name="callback">Lobby entering callback data</param>
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log("Joined lobby with Steam ID: " + currentLobbyId);

        // Retrieve host connection data (if needed) from the lobby metadata
        string hostSteamId = SteamMatchmaking.GetLobbyData(currentLobbyId, "HostAddress");
        Debug.Log("Host Steam ID from lobby metadata: " + hostSteamId);

        // Here, you might configure your network connection data (e.g., set the connection address)
        // For simple setups, simply start the client:
        NetworkManager.Singleton.StartClient();
    }
}
