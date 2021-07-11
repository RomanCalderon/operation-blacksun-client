using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;

namespace Michsky.UI.Shift
{
    [RequireComponent(typeof(Button))]
    public class CardButton : MonoBehaviour
    {
        [Header("Resources")]
        public Sprite backgroundImage;
        public string buttonTitle = "My Title";
        [TextArea] public string buttonDescription = "My Description";

        [Header("Settings")]
        public bool useCustomResources = false;

        [Header("Status")]
        public bool enableStatus;
        public StatusItem statusItem;

        [SerializeField]
        private UnityEvent<string, string> onClick = null;

        Button buttonComponent;
        Image backgroundImageObj;
        TextMeshProUGUI titleObj;
        TextMeshProUGUI descriptionObj;
        Transform statusNone;
        Transform statusLocked;
        Transform statusCompleted;

        public enum StatusItem
        {
            NONE,
            LOCKED,
            COMPLETED
        }

        private void Awake ()
        {
            buttonComponent = GetComponent<Button> ();
        }

        private void OnEnable ()
        {
            buttonComponent.onClick.AddListener ( OnClick );
        }

        private void OnDisable ()
        {
            buttonComponent.onClick.RemoveListener ( OnClick );
        }

        void Start()
        {
            if (useCustomResources == false)
            {
                backgroundImageObj = gameObject.transform.Find("Content/Background").GetComponent<Image>();
                titleObj = gameObject.transform.Find("Content/Texts/Title").GetComponent<TextMeshProUGUI>();
                descriptionObj = gameObject.transform.Find("Content/Texts/Description").GetComponent<TextMeshProUGUI>();

                backgroundImageObj.sprite = backgroundImage;
                titleObj.text = buttonTitle;
                descriptionObj.text = buttonDescription;
            }

            if (enableStatus == true)
            {
                statusNone = gameObject.transform.Find("Content/Texts/Status/None").GetComponent<Transform>();
                statusLocked = gameObject.transform.Find("Content/Texts/Status/Locked").GetComponent<Transform>();
                statusCompleted = gameObject.transform.Find("Content/Texts/Status/Completed").GetComponent<Transform>();
            }

            SetStatus ( statusItem );
        }

        public void SetStatus ( StatusItem statusItem )
        {
            if ( !enableStatus )
            {
                return;
            }

            this.statusItem = statusItem;
            buttonComponent.interactable = !( statusItem == StatusItem.LOCKED );

            switch ( statusItem )
            {
                case StatusItem.NONE:
                    statusNone.gameObject.SetActive ( true );
                    statusLocked.gameObject.SetActive ( false );
                    statusCompleted.gameObject.SetActive ( false );
                    break;
                case StatusItem.LOCKED:
                    statusNone.gameObject.SetActive ( false );
                    statusLocked.gameObject.SetActive ( true );
                    statusCompleted.gameObject.SetActive ( false );
                    break;
                case StatusItem.COMPLETED:
                    statusNone.gameObject.SetActive ( false );
                    statusLocked.gameObject.SetActive ( false );
                    statusCompleted.gameObject.SetActive ( true );
                    break;
                default:
                    break;
            }
        }

        private void OnClick ()
        {
            onClick?.Invoke ( buttonTitle, buttonDescription );
        }
    }
}