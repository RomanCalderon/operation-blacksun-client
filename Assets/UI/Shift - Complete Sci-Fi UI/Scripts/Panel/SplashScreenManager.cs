using UnityEngine;

namespace Michsky.UI.Shift
{
    public class SplashScreenManager : MonoBehaviour
    {
        [Header("Resources")]
        public GameObject splashScreen;
        public GameObject mainPanels;

        private Animator splashScreenAnimator;
        private Animator mainPanelsAnimator;
        private TimedEvent ssTimedEvent;

        [Header("Settings")]
        public bool enableSplashScreen;
        public bool enablePressAnyKeyScreen;
        public bool enableLoginScreen;

        MainPanelManager mpm;

        void Start()
        {
            splashScreenAnimator = splashScreen.GetComponent<Animator>();
            ssTimedEvent = splashScreen.GetComponent<TimedEvent>();
            mainPanelsAnimator = mainPanels.GetComponent<Animator>();
            mpm = gameObject.GetComponent<MainPanelManager>();

            //ToggleSplashScreen ( enableSplashScreen );

            if ( enableSplashScreen == true && enableLoginScreen == false && enablePressAnyKeyScreen == true)
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
            }

            if ( enableSplashScreen == true && enableLoginScreen == true && enablePressAnyKeyScreen == true )
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
            }

            if ( enableSplashScreen == true && enableLoginScreen == true && enablePressAnyKeyScreen == false )
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
                splashScreenAnimator.Play("Login");
            }

            if ( enableSplashScreen == true && enableLoginScreen == false && enablePressAnyKeyScreen == false )
            {
                splashScreen.SetActive(true);
                mainPanelsAnimator.Play("Invisible");
                splashScreenAnimator.Play("Loading");
                ssTimedEvent.StartIEnumerator();
            }
        }

        public void LoginScreenCheck()
        {
            if (enableLoginScreen == true && enablePressAnyKeyScreen == true)
                splashScreenAnimator.Play("Press Any Key to Login");

            else if (enableLoginScreen == false && enablePressAnyKeyScreen == true)
            {
                splashScreenAnimator.Play("Press Any Key to Loading");
                ssTimedEvent.StartIEnumerator();
            }

            else if (enableLoginScreen == false && enablePressAnyKeyScreen == false)
            {
                splashScreenAnimator.Play("Loading");
                ssTimedEvent.StartIEnumerator();
            }
        }

        public void ToggleSplashScreen ( bool state )
        {
            enableSplashScreen = state;

            if ( enableSplashScreen == true )
            {
                splashScreen.SetActive ( false );
                mainPanels.SetActive ( true );

                mainPanelsAnimator.Play ( "Start" );
                mpm.OpenFirstTab ();
            }
            else
            {
                splashScreen.SetActive ( false );
                ssTimedEvent.timerAction?.Invoke ();
            }
        }
    }
}