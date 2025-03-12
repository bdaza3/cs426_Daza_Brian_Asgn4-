using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
public class NetworkManagerUI : MonoBehaviour
{
    // [SerializeField] attribute is used to make the private variables accessible
    // within the Unity editor without making them public
    [SerializeField] private Button host_btn;
    [SerializeField] private Button client_btn;

    // after all objectes are created and initialized
    // Awake() method is called and executed
    // Awake is always called before any Start functions.

    private void Awake()
    {
        // add a listener to the host button
        host_btn.onClick.AddListener(() =>
        {
            // call the NetworkManager's StartHost() method
            NetworkManager.Singleton.StartHost();
            Debug.Log("Host button clicked.");
        });

        // add a listener to the client button
        client_btn.onClick.AddListener(() =>
        {
            // call the NetworkManager's StartClient() method
            NetworkManager.Singleton.StartClient();
            Debug.Log("Client button clicked.");
        });
    }
}