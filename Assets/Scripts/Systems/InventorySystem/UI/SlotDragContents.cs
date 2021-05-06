using UnityEngine;
using UnityEngine.UI;
using InventorySystem.PlayerItems;

public class SlotDragContents : MonoBehaviour
{
    public Image ContentImage;
    public Text ContentDragAmountText;

    public void Initialize ( PlayerItem playerItem, int contentDragAmount = 1 )
    {
        ContentImage.sprite = playerItem.Image;
        ContentImage.color = Constants.RarityToColor ( playerItem.Rarity );

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
