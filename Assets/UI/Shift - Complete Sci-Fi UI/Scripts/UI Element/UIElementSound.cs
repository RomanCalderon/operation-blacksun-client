using UnityEngine;
using UnityEngine.EventSystems;

namespace Michsky.UI.Shift
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(AudioSource))]
    public class UIElementSound : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        [Header("Resources")]
        public UIManager UIManagerAsset;
        public AudioSource audioObject;

        [Header("Settings")]
        public bool enableHoverSound = true;
        public bool enableClickSound = true;

        void OnEnable()
        {
            if (UIManagerAsset == null)
            {
                try { UIManagerAsset = Resources.Load<UIManager>("Shift UI Manager"); }
                catch { Debug.Log("No UI Manager found. Assign it manually, otherwise it won't work properly.", this); }
            }
        }

        void Awake()
        {
            if (audioObject == null)
                audioObject = gameObject.GetComponent<AudioSource>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (enableHoverSound == true)
                audioObject.PlayOneShot(UIManagerAsset.hoverSound);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (enableClickSound == true)
                audioObject.PlayOneShot(UIManagerAsset.clickSound);
        }
    }
}