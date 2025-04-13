using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay.Models;

public partial class NetManager : MonoBehaviour
{
    // async �񵿱� �޼ҵ�
    // await Ű���带 ����� �񵿱� �۾� ó��

    private Lobby currentLobby;
    private string joinCode;

    public async void CreateLobby()
    {
        try
        {
            // LobbyService�� UGS���� �����ϴ� ���̺귯��
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyGameLobby", maxPlayers);
            Debug.Log("�κ� ���� �Ϸ�: " + currentLobby.Id);
            await SetupRelayForHost(currentLobby);
            StartHost();
            ChangeScene();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("�κ� ���� ����: " + e);
        }
    }

    // Relay ������ �κ��� �ִ� �÷��̾� ���� �Ҵ�
    // �Ҵ�� ID�� joinCode�� �޾ƿ� ���� �ٸ� Ŭ���̾�Ʈ���� �濡 ������ �� ���
    private async Task SetupRelayForHost(Lobby lobby)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameSessionManager.Instance.joinCode = joinCode; // ������ JoinCode�� GameSessionManager�� ���ڿ��� ����

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData // ȣ��Ʈ�� �ڽ��� connectionData�� hostConnectionData�ε� ��
            );

            joinCodeText.text = "Join Code: " + joinCode;
            Debug.Log("Relay �Ҵ� �Ϸ� - JoinCode: " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay �Ҵ� ����: " + e);
        }
    }

    public async void JoinGameWithCode(string inputJoinCode)
    {
        if (string.IsNullOrEmpty(inputJoinCode))
        {
            Debug.Log("��ȿ���� ���� JoinCode�Դϴ�.");
            return;
        }
        try
        {
            // �Էµ� JoinCode�� ����� ����Ƽ ������ ������ ���� ��û
            // await�� �۾��� ���� ������ ���
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);

            // UnityTransport�� ����Ƽ ������ ���� ������ ����
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                 joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            Debug.Log("Relay ���� ����: " + joinAllocation.RelayServer.IpV4 + ":" + joinAllocation.RelayServer.Port);

            StartClient(); // Ŭ���̾�Ʈ�� ���� �õ�
            Debug.Log("JoinCode�� ���� ����");
        }
        catch (RelayServiceException e)
        {
            Debug.Log("JoinCode�� �κ� ���� ����" + e.ToString());
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log($"spawn : client {clientId}");

            if (NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject)
            {
                GameObject player = Instantiate(playerPrefab, GetSpawnPosition(clientId), Quaternion.identity);
                player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
            }
        }
    }

    // JoinCode�� Text UI�� �����ֱ� ���� �Լ�
    public string GetJoinCode()
    {
        return joinCode.ToString();
    }

    private Vector3 GetSpawnPosition(ulong clientId)
    {
        return new Vector3(clientId * 2, 0, 0);
    }

    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        Debug.Log("Ŭ���̾�Ʈ�� ����");  
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("ȣ��Ʈ�� ����");
    }
}
