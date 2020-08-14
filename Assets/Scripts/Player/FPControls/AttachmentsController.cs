using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using InventorySystem.PlayerItems;
using System.Linq;

public class AttachmentsController : MonoBehaviour
{
    #region Models

    [Serializable]
    public class AttachmentItem
    {
        [HideInInspector]
        public string Name;
        public Attachment Attachment;
        public GameObject Instance;

        public void OnValidate ()
        {
            if ( Attachment != null )
            {
                Name = Attachment.ToString ();
            }
        }
    }

    #endregion

    [SerializeField]
    private AttachmentItem [] m_barrels = null;
    [SerializeField]
    private AttachmentItem [] m_magazines = null;
    [SerializeField]
    private AttachmentItem [] m_sights = null;
    [SerializeField]
    private AttachmentItem [] m_stocks = null;


    // Start is called before the first frame update
    void Start ()
    {

    }

    public void UpdateAttachment ( Barrel barrel )
    {
        ClearBarrels ();

        if ( barrel != null )
        {
            ActivateAttachment ( barrel );
        }
    }

    public void UpdateAttachment ( Magazine magazine )
    {
        ClearMagazines ();

        if ( magazine != null )
        {
            ActivateAttachment ( magazine );
        }
    }

    public void UpdateAttachment ( Sight sight )
    {
        ClearSights ();

        if ( sight != null )
        {
            ActivateAttachment ( sight );
        }
    }

    public void UpdateAttachment ( Stock stock )
    {
        ClearStocks ();

        if ( stock != null )
        {
            ActivateAttachment ( stock );
        }
    }

    #region Attachment Activation

    private void ActivateAttachment ( Barrel barrel )
    {
        foreach ( AttachmentItem item in m_barrels )
        {
            if ( item.Attachment.Id == barrel.Id )
            {
                item.Instance.SetActive ( true );
                return;
            }
        }
    }

    private void ActivateAttachment ( Magazine magazine )
    {
        foreach ( AttachmentItem item in m_magazines )
        {
            if ( item.Attachment.Id == magazine.Id )
            {
                item.Instance.SetActive ( true );
                return;
            }
        }
    }

    private void ActivateAttachment ( Sight sight )
    {
        foreach ( AttachmentItem item in m_sights )
        {
            if ( item.Attachment.Id == sight.Id )
            {
                item.Instance.SetActive ( true );
                return;
            }
        }
    }

    private void ActivateAttachment ( Stock stock )
    {
        foreach ( AttachmentItem item in m_stocks )
        {
            if ( item.Attachment.Id == stock.Id )
            {
                item.Instance.SetActive ( true );
                return;
            }
        }
    }

    #endregion

    #region Clear Attachments

    private void ClearBarrels ()
    {
        foreach ( AttachmentItem attachmentItem in m_barrels )
        {
            if ( attachmentItem.Instance != null )
            {
                attachmentItem.Instance.SetActive ( false );
            }
        }
    }

    private void ClearMagazines ()
    {
        foreach ( AttachmentItem attachmentItem in m_magazines )
        {
            if ( attachmentItem.Instance != null )
            {
                attachmentItem.Instance.SetActive ( false );
            }
        }
    }

    private void ClearSights ()
    {
        foreach ( AttachmentItem attachmentItem in m_sights )
        {
            if ( attachmentItem.Instance != null )
            {
                attachmentItem.Instance.SetActive ( false );
            }
        }
    }

    private void ClearStocks ()
    {
        foreach ( AttachmentItem attachmentItem in m_stocks )
        {
            if ( attachmentItem.Instance != null )
            {
                attachmentItem.Instance.SetActive ( false );
            }
        }
    }

    #endregion

    private void OnValidate ()
    {
        if ( m_barrels != null )
        {
            foreach ( AttachmentItem item in m_barrels )
            {
                item.OnValidate ();
            }
        }

        if ( m_magazines != null )
        {
            foreach ( AttachmentItem item in m_magazines )
            {
                item.OnValidate ();
            }
        }

        if ( m_sights != null )
        {
            foreach ( AttachmentItem item in m_sights )
            {
                item.OnValidate ();
            }
        }

        if ( m_stocks != null )
        {
            foreach ( AttachmentItem item in m_stocks )
            {
                item.OnValidate ();
            }
        }
    }
}
