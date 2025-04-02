using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public partial class NetManager : MonoBehaviour
{
    // async �񵿱� �޼ҵ�
    // await Ű���带 ����� �񵿱� �۾� ó��

    private Lobby currentLobby;

    private async Task CreateNewLobby()
    {
        try
        {
            // LobbyService�� UGS���� �����ϴ� ���̺귯��
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("CreatedLobby", maxPlayers);
            Debug.Log("���ο� �� ������ " + currentLobby.Id);
            await AllocateRelayServiceAndJoin(currentLobby);
            StartHost(); // ���� ������ �÷��̾�� ȣ��Ʈ�� ����
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("�κ� ���� ���� " + e);
        }
    }

    // Relay ������ �κ��� �ִ� �÷��̾� ���� �Ҵ�
    // �Ҵ�� ID�� joinCode�� �޾ƿ� ���� �ٸ� Ŭ���̾�Ʈ���� �濡 ������ �� ���
    private async Task AllocateRelayServiceAndJoin(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = joinCode;
            Debug.Log("Relay ���� �Ҵ� �Ϸ� JoinCode : " + joinCode);
        }
        catch(RelayServiceException e)
        {
            Debug.Log("Relay ���� �Ҵ� ���� " + e);
        }
    }

    private async Task JoinLobby(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log("�濡 �����Ͽ����ϴ�." + lobbyId);
            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("�κ� ���� �����߽��ϴ�. " + e);
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

            StartClient(); // Ŭ���̾�Ʈ�� ���� �õ�
            Debug.Log("JoinCode�� ���� ����");
        }
        catch (RelayServiceException e)
        {
            Debug.Log("JoinCode�� �κ� ���� ����" + e.ToString());
        }
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
