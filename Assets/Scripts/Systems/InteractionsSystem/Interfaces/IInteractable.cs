internal interface IInteractable
{
    bool IsInteractable { get; set; }

    bool IsInteracting { get; }

    float InteractTime { get; set; }

    string AccessKey { get; set; }

    void Initialize ( byte [] interactableData );

    void StartHover ();

    void StartInteract ( int clientId, string accessKey = null );

    void StartInteract ( int clientId, string [] accessKeys );

    void StopInteract ();

    void StopHover ();
}
