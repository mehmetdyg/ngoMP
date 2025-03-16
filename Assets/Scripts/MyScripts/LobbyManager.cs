using UnityEngine;
using Unity.Netcode;
using Steamworks;
using System;

public class LobbyManager : MonoBehaviour
{
    private CSteamID currentLobbyId;
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<LobbyEnter_t> lobbyEntered;

    // Zamanaşımı kontrolü için değişkenler
    private float joinTimeout = 10f;
    private float joinTimer;
    private bool isJoining = false;
    private string pendingLobbyId = "";

    // Oda ID'si oluşturulduğunda ya da hata oluştuğunda bildirim için event
    public event Action<string> OnRoomIdCreated;
    public event Action<string> OnRoomJoinFailed;
    public event Action OnRoomJoinSuccess;

    private void Start()
    {
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    private void Update()
    {
        // Odaya katılma zamanaşımı kontrolü
        if (isJoining)
        {
            joinTimer += Time.deltaTime;
            if (joinTimer > joinTimeout)
            {
                // Zamanaşımı oldu, bağlantı başarısız
                isJoining = false;
                Debug.LogError("Join lobby timed out for ID: " + pendingLobbyId);
                OnRoomJoinFailed?.Invoke("Connection timed out. Room may not exist.");
            }
        }
    }

    public string GetCurrentLobbyId()
    {
        if (currentLobbyId.IsValid())
        {
            return currentLobbyId.m_SteamID.ToString();
        }
        return string.Empty;
    }

    public void HostGame()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePrivate, 4);
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult == EResult.k_EResultOK)
        {
            currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
            Debug.Log("Lobby created with Steam ID: " + currentLobbyId);

            SteamMatchmaking.SetLobbyData(currentLobbyId, "HostAddress", SteamUser.GetSteamID().ToString());
            NetworkManager.Singleton.StartHost();

            // Oda ID'sini bildir
            OnRoomIdCreated?.Invoke(currentLobbyId.m_SteamID.ToString());
        }
        else
        {
            Debug.LogError("Lobby creation failed with result: " + callback.m_eResult);
            OnRoomIdCreated?.Invoke(string.Empty); // Boş string ile başarısız olduğunu bildirir
        }
    }

    public void JoinGame(string lobbyIdText)
    {
        if (ulong.TryParse(lobbyIdText, out ulong lobbyId))
        {
            // Odaya katılma denemesi başlat
            pendingLobbyId = lobbyIdText;
            isJoining = true;
            joinTimer = 0f;

            Debug.Log("Attempting to join lobby: " + lobbyIdText);

            try
            {
                SteamMatchmaking.JoinLobby(new CSteamID(lobbyId));
            }
            catch (Exception e)
            {
                Debug.LogError("Error joining lobby: " + e.Message);
                isJoining = false;
                OnRoomJoinFailed?.Invoke("Error joining room: " + e.Message);
            }
        }
        else
        {
            Debug.LogError("Invalid lobby ID provided!");
            OnRoomJoinFailed?.Invoke("Invalid lobby ID format");
        }
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        // Katılma işlemi tamamlandı
        isJoining = false;

        // Eğer onResult başarısız gösteriyorsa
        if (callback.m_EChatRoomEnterResponse != (uint)EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
        {
            Debug.LogError("Failed to join lobby with response: " + callback.m_EChatRoomEnterResponse);
            OnRoomJoinFailed?.Invoke("Failed to join room with error: " + callback.m_EChatRoomEnterResponse);
            return;
        }

        currentLobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        Debug.Log("Joined lobby with Steam ID: " + currentLobbyId);

        string hostSteamId = SteamMatchmaking.GetLobbyData(currentLobbyId, "HostAddress");

        if (string.IsNullOrEmpty(hostSteamId))
        {
            Debug.LogError("Host data not found in lobby!");
            OnRoomJoinFailed?.Invoke("Invalid room: host data not found");
            return;
        }

        Debug.Log("Host Steam ID from lobby metadata: " + hostSteamId);
        NetworkManager.Singleton.StartClient();

        // Başarılı girişi bildir
        OnRoomJoinSuccess?.Invoke();
    }
}