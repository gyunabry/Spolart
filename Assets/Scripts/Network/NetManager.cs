using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Netcode.Transports.UTP;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;

public partial class NetManager : MonoBehaviour
{
    [SerializeField]
    private Button m_StartHostButton;
    [SerializeField]
    private Button m_StartClientButton;
    [SerializeField]
    private Button m_StartJoinCode;

    [SerializeField]
    private InputField inputJoinCode;
    [SerializeField]
    private TMP_Text joinCodeText;

    private int maxPlayers = 4;

    private async void Start()
    {
        await UnityServices.InitializeAsync(); // UnityService 초기화

        // 서비스 내부에 로그인되지 않았다면 로그인 실행
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        m_StartHostButton.onClick.AddListener(() => CreateNewLobby());
        m_StartClientButton.onClick.AddListener(() => StartClient());

        m_StartJoinCode.onClick.AddListener(() => JoinGameWithCode(inputJoinCode.text));
    }
}
