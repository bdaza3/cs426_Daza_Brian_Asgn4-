using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class ScoreManager : NetworkBehaviour
{
    public static ScoreManager instance;

    public NetworkVariable<int> score = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public Text scoreText;

    private NetworkVariable<bool> player1Pressed = new NetworkVariable<bool>(false);
    private NetworkVariable<bool> player2Pressed = new NetworkVariable<bool>(false);

    private bool isPlayer1 = false;
    private bool isPlayer2 = false;

    void Awake()
    {
        if (instance == null)
            instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) // Ensure each client runs this for themselves
        {
            AssignPlayerRoleServerRpc(NetworkManager.LocalClientId);
        }

        score.OnValueChanged += (oldValue, newValue) => UpdateScoreUI();
    }

    [ServerRpc]
    private void AssignPlayerRoleServerRpc(ulong clientId, ServerRpcParams rpcParams = default)
    {
        // Assign first connecting client as Player 1, second as Player 2
        if (!player1Pressed.Value)
        {
            player1Pressed.Value = false; // Ensure reset
            isPlayer1 = true;
        }
        else if (!player2Pressed.Value)
        {
            player2Pressed.Value = false;
            isPlayer2 = true;
        }
    }

    void Update()
    {
        if (!IsOwner) return; // Prevents input processing for other players

        if (Input.GetButtonDown("Fire1")) // Left Mouse Button
        {
            SendButtonPressServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendButtonPressServerRpc(ServerRpcParams rpcParams = default)
    {
        if (isPlayer1)
            player1Pressed.Value = true;
        else if (isPlayer2)
            player2Pressed.Value = true;

        CheckScoreCondition();
    }

    private void CheckScoreCondition()
    {
        if (player1Pressed.Value && player2Pressed.Value)
        {
            score.Value += 1; // NetworkVariable syncs automatically

            // Reset button presses
            player1Pressed.Value = false;
            player2Pressed.Value = false;

            if (score.Value >= 5)
            {
                ShowWinMessageClientRpc();
            }
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score.Value;
    }

    [ClientRpc]
    private void ShowWinMessageClientRpc()
    {
        if (scoreText != null)
            scoreText.text = "You Won!";
    }
}
