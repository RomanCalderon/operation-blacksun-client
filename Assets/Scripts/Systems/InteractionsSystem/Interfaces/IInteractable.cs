using UnityEngine;

public interface IInteractable
{
    string InstanceId { get; }

    int InteractionType { get; }

    bool IsInteractable { get; }

    string InteractionContext { get; }

    Color InteractionColor { get; }

    bool IsInteracting { get; }

    float InteractTime { get; }

    bool HasAccessKey { get; }

    void Initialize ( byte [] interactableData );

    void StartHover ();

    void StartInteract ( int clientId );

    void StopInteract ();

    void StopHover ();
}
