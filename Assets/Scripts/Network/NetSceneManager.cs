using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public partial class NetManager : MonoBehaviour
{
    private string gamePlaySceneName = "GamePlayScene";

    private void OnPlayerJoined()
    {
        
    }

    private void ChangeScene()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene(gamePlaySceneName, LoadSceneMode.Single);
        }
    }
}
