using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotDragContents : MonoBehaviour
{
    public Image ContentImage;
    public Text ContentDragAmountText;

    public void Initialize ( Sprite contentImage, int contentDragAmount = 1 )
    {
        ContentImage.sprite = contentImage;

        contentDragAmount = Mathf.Max ( 1, contentDragAmount );

        if ( contentDragAmount == 1 )
        {
            ContentDragAmountText.enabled = false;
        }
        else
        {
            ContentDragAmountText.text = contentDragAmount.ToString ();
        }
    }
}
