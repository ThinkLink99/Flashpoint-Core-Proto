// Placeholder interface for network abstraction. Implement with your networking library (Mirror/Netcode/Photon/etc.)
public interface INetworkLayer
{
    // When running single-player this can be a no-op implementation that directly calls GameManager.
    void SendSelectModelRequest(Model model, PlayerController requester);
    void SendSelectDestinationRequest(UnityEngine.Vector3 destination, PlayerController requester);
    void SendSelectTargetRequest(Model target, PlayerController requester);
}
