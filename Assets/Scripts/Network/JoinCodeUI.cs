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
        // ȣ��Ʈ�� ��쿡�� JoinCode Ȯ�� ����
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
