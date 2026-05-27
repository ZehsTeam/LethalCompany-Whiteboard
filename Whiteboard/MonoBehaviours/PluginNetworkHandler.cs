using com.github.zehsteam.Whiteboard.Helpers;
using Unity.Netcode;

namespace com.github.zehsteam.Whiteboard.MonoBehaviours;

internal class PluginNetworkHandler : NetworkBehaviour
{
    public static PluginNetworkHandler Instance { get; private set; }

    private void Awake()
    {
        // Ensure there is only one instance of the Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate object
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (Instance != null && Instance != this)
        {
            // Ensure only the server can handle despawning duplicate instances
            if (IsServer)
            {
                NetworkObject.Despawn(); // Despawn the networked object
            }

            return;
        }

        Instance = this;
    }

    [ClientRpc]
    public void SetWhiteboardUnlockablePrice_ClientRpc(int price, ClientRpcParams clientRpcParams = default)
    {
        UnlockableHelper.UpdateUnlockablePrice(Assets.WhiteboardUnlockableItemDef, price);
    }
}
