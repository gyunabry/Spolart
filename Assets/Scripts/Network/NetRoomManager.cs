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
    // async 비동기 메소드
    // await 키워드를 사용해 비동기 작업 처리

    private Lobby currentLobby;
    private string joinCode;

    public async void CreateLobby()
    {
        try
        {
            // LobbyService는 UGS에서 제공하는 라이브러리
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("MyGameLobby", maxPlayers);
            Debug.Log("로비 생성 완료: " + currentLobby.Id);
            await SetupRelayForHost(currentLobby);
            StartHost();
            ChangeScene();
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("로비 생성 실패: " + e);
        }
    }

    // Relay 서버에 로비의 최대 플레이어 수를 할당
    // 할당된 ID를 joinCode로 받아와 추후 다른 클라이언트들이 방에 입장할 때 사용
    private async Task SetupRelayForHost(Lobby lobby)
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            GameSessionManager.Instance.joinCode = joinCode; // 생성된 JoinCode를 GameSessionManager의 문자열에 저장

            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            transport.SetRelayServerData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData,
                allocation.ConnectionData // 호스트는 자신의 connectionData를 hostConnectionData로도 씀
            );

            joinCodeText.text = "Join Code: " + joinCode;
            Debug.Log("Relay 할당 완료 - JoinCode: " + joinCode);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay 할당 실패: " + e);
        }
    }

    public async void JoinGameWithCode(string inputJoinCode)
    {
        if (string.IsNullOrEmpty(inputJoinCode))
        {
            Debug.Log("유효하지 않은 JoinCode입니다.");
            return;
        }
        try
        {
            // 입력된 JoinCode를 사용해 유니티 릴레이 서버에 접속 요청
            // await로 작업이 끝날 때까지 대기
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputJoinCode);

            // UnityTransport에 유니티 릴레이 서버 정보를 설정
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(
                 joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            Debug.Log("Relay 연결 정보: " + joinAllocation.RelayServer.IpV4 + ":" + joinAllocation.RelayServer.Port);

            StartClient(); // 클라이언트로 접속 시도
            Debug.Log("JoinCode로 접속 성공");
        }
        catch (RelayServiceException e)
        {
            Debug.Log("JoinCode로 로비 접속 실패" + e.ToString());
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

    // JoinCode를 Text UI에 보여주기 위한 함수
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
        Debug.Log("클라이언트로 시작");  
    }

    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        Debug.Log("호스트로 시작");
    }
}
