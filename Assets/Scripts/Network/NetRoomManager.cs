using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Relay;
using UnityEngine;
using System.Threading.Tasks;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;

public partial class NetManager : MonoBehaviour
{
    // async 비동기 메소드
    // await 키워드를 사용해 비동기 작업 처리

    private Lobby currentLobby;

    private async Task CreateNewLobby()
    {
        try
        {
            // LobbyService는 UGS에서 제공하는 라이브러리
            currentLobby = await LobbyService.Instance.CreateLobbyAsync("CreatedLobby", maxPlayers);
            Debug.Log("새로운 방 생성됨 " + currentLobby.Id);
            await AllocateRelayServiceAndJoin(currentLobby);
            StartHost(); // 방을 생성한 플레이어는 호스트로 시작
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("로비 생성 실패 " + e);
        }
    }

    // Relay 서버에 로비의 최대 플레이어 수를 할당
    // 할당된 ID를 joinCode로 받아와 추후 다른 클라이언트들이 방에 입장할 때 사용
    private async Task AllocateRelayServiceAndJoin(Lobby lobby)
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(lobby.MaxPlayers);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            joinCodeText.text = joinCode;
            Debug.Log("Relay 서버 할당 완료 JoinCode : " + joinCode);
        }
        catch(RelayServiceException e)
        {
            Debug.Log("Relay 서버 할당 실패 " + e);
        }
    }

    private async Task JoinLobby(string lobbyId)
    {
        try
        {
            currentLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            Debug.Log("방에 접속하였습니다." + lobbyId);
            StartClient();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("로비에 참가 실패했습니다. " + e);
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

            StartClient(); // 클라이언트로 접속 시도
            Debug.Log("JoinCode로 접속 성공");
        }
        catch (RelayServiceException e)
        {
            Debug.Log("JoinCode로 로비 접속 실패" + e.ToString());
        }
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
