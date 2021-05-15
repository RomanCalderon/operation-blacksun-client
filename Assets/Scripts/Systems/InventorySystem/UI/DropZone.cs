using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public const string ID = "drop-zone";

    [SerializeField]
    private GameObject m_view = null;

    // Start is called before the first frame update
    void Start ()
    {
        m_view.SetActive ( false );
    }

    #region Interface

    public void OnPointerEnter ( PointerEventData eventData )
    {
        if ( InventoryManager.Instance.IsSlotSelected )
        {
            m_view.SetActive ( true );
        }
    }

    public void OnDrop ( PointerEventData eventData )
    {
        InventoryManager.Instance.SlotOnDrop ( ID, eventData );
        m_view.SetActive ( false );
    }

    public void OnPointerExit ( PointerEventData eventData )
    {
        m_view.SetActive ( false );
    }

    #endregion
}
