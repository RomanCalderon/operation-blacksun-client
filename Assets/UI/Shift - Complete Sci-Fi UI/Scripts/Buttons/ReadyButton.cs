using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.Shift
{
    public class ReadyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("Text")]
        public bool useCustomText = false;
        public string buttonTitle = "My Title";

        [Header("Image")]
        public bool useCustomImage = false;
        public Sprite backgroundImage;

        Button buttonComponent;
        TextMeshProUGUI titleText;
        Image image1;

        private bool buttonSelected = false;

        private void Awake ()
        {
            buttonComponent = GetComponent<Button> ();
        }

        private void OnEnable ()
        {
            buttonComponent.onClick.AddListener ( OnButtonClick );
            UpdateButtonState ();
        }

        private void OnDisable ()
        {
            buttonComponent.onClick.RemoveListener ( OnButtonClick );
        }

        void Start()
        {
            if (useCustomText == false)
            {
                titleText = gameObject.transform.Find("Content/Title").GetComponent<TextMeshProUGUI>();
                titleText.text = buttonTitle;
            }

            if (useCustomImage == false)
            {
                image1 = gameObject.transform.Find("Content/Background").GetComponent<Image>();
                image1.sprite = backgroundImage;
            }
        }

        #region Button Behaviour

        private void OnButtonClick ()
        {
            if ( buttonComponent.interactable )
            {
                buttonSelected = !buttonSelected;
                UpdateButtonState ();
            }
        }

        /// <summary>
        /// Refreshes button when enabled to selected state.
        /// </summary>
        private void UpdateButtonState ()
        {
            if ( buttonSelected )
            {
                Debug.Log ( "Ready button selected" );
                buttonComponent.animator.Play ( "Pressed" );
            }
            else
            {
                Debug.Log ( "Ready button deselected" );
                buttonComponent.animator.Play ( "Normal" );
            }
        }

        public void OnPointerEnter ( PointerEventData eventData )
        {
            if ( buttonComponent.interactable && !buttonSelected )
            {
                buttonComponent.animator.Play ( "Highlighted" );
            }
        }

        public void OnPointerExit ( PointerEventData eventData )
        {
            if ( buttonComponent.interactable && !buttonSelected )
            {
                buttonComponent.animator.Play ( "Normal" );
            }
        }

        #endregion
    }
}