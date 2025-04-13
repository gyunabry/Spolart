using TMPro;
using Unity.Netcode;
using UnityEngine;

public class JoinCodeUI : MonoBehaviour
{
    [SerializeField]
    private GameObject joinCodePanel;
    [SerializeField]
    private TMP_Text joinCodeText;

    private void Start()
    {
        // 호스트일 경우에만 JoinCode 확인 가능
        if (NetworkManager.Singleton.IsHost)
        {
            joinCodePanel.SetActive(true);
            joinCodeText.text = $"Join Code: {GameSessionManager.Instance.joinCode}";
        }
        else
        {
            joinCodePanel.SetActive(false);
        }
    }
}
