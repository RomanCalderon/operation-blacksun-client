using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.Shift
{
    public class ReadyButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header ( "Text" )]
        public string buttonTitleNormal = "READY";
        public string buttonTitleActiveHover = "CANCEL";
        public string buttonTitleDisabled = "DISABLED";
        private string buttonTitleActiveStatus;

        [Header ( "Image" )]
        public bool useCustomImage = false;
        public Sprite backgroundImage;

        [Space, SerializeField]
        private UnityEvent<bool> onReady;
        [SerializeField]
        private UnityEvent<bool> onReadyInverse;

        Button buttonComponent;
        TextMeshProUGUI titleText;
        Image image1;

        private bool buttonSelected = false;

        private void Awake ()
        {
            buttonComponent = GetComponent<Button> ();

            titleText = gameObject.transform.Find ( "Content/Title" ).GetComponent<TextMeshProUGUI> ();
            titleText.text = buttonTitleNormal;

            if ( useCustomImage == false )
            {
                image1 = gameObject.transform.Find ( "Content/Background" ).GetComponent<Image> ();
                image1.sprite = backgroundImage;
            }
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

        #region Button Behaviour

        public void UpdateButtonText ( string value )
        {
            if ( !string.IsNullOrEmpty ( value ) )
            {
                titleText.text = value;
            }
        }

        public void UpdateButtonTextActive ( string value )
        {
            if ( !string.IsNullOrEmpty ( value ) )
            {
                titleText.text = buttonTitleActiveStatus = value;
            }
        }

        public void SetButtonInteractable ( bool value )
        {
            buttonComponent.interactable = value;
            buttonSelected = false;
            UpdateButtonState ();

            if ( value )
            {
                UpdateButtonText ( buttonTitleNormal );
            }
            else
            {
                UpdateButtonText ( buttonTitleDisabled );
            }
        }

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
            onReady?.Invoke ( buttonSelected );
            onReadyInverse?.Invoke ( !buttonSelected );

            if ( buttonSelected )
            {
                buttonComponent.animator.Play ( "Pressed" );
                UpdateButtonTextActive ( "SEARCHING" );
            }
            else
            {
                buttonComponent.animator.Play ( "Normal" );
                titleText.text = buttonTitleNormal;
            }
        }

        public void OnPointerEnter ( PointerEventData eventData )
        {
            if ( buttonComponent.interactable )
            {
                if ( !buttonSelected )
                {
                    buttonComponent.animator.Play ( "Highlighted" );
                }
                else
                {
                    titleText.text = buttonTitleActiveHover;
                }
            }
        }

        public void OnPointerExit ( PointerEventData eventData )
        {
            if ( buttonComponent.interactable )
            {
                if ( !buttonSelected )
                {
                    buttonComponent.animator.Play ( "Normal" );
                }
                else
                {
                    titleText.text = buttonTitleActiveStatus;
                }
            }
        }

        #endregion
    }
}