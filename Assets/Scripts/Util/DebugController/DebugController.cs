using System;
using System.Collections.Generic;
using UnityEngine;

namespace DebugConsole
{
    [RequireComponent ( typeof ( AttributeHandler ) )]
    public class DebugController : LazySingleton<DebugController>
    {
        #region Constants

        private const KeyCode TOGGLE_CONSOLE_KEY = KeyCode.BackQuote;
        private const KeyCode NAV_UP_KEY = KeyCode.UpArrow;
        private const KeyCode NAV_DOWN_KEY = KeyCode.DownArrow;
        private const KeyCode RETURN_KEY = KeyCode.Return;

        #endregion

        #region Members

        private bool m_isEnabled = true;
        private bool m_showConsole = false;
        private bool m_showHelp = false;
        private bool m_showHintsView = false;
        private Vector2 m_helpScroll;

        private string m_input = null;
        private string m_original = null;

        private AttributeHandler m_attributeHandler = null;

        // Static commands
        public static DebugCommand HELP;

        [SerializeField]
        private List<IDebugCommand> m_commands = null;
        private IDebugCommand [] m_filteredCommands = null;
        private int m_selectedCommand = -1;

        #endregion

        #region Initialization

        protected override void Awake ()
        {
            base.Awake ();

            InitializeCommands ();
        }

        private void InitializeCommands ()
        {
            // Create static commands
            // HELP command
            HELP = new DebugCommand ( "help", "list all debug commands", "help", () =>
            {
                m_showHelp = !m_showHelp;
            } );

            // Add static commands
            m_commands = new List<IDebugCommand> ()
            {
                HELP
            };

            // Get all command attributes in existing scene objects
            m_attributeHandler = GetComponent<AttributeHandler> ();
            m_attributeHandler.GetAllCommandAttributes ();
        }

        #endregion

        #region Main Methods

        #region Console Interaction

        public void EnableDebugConsole ( bool value )
        {
            m_isEnabled = value;
            if ( !m_isEnabled )
            {
                m_showConsole = false;
            }
        }

        private void OnToggleConsole ()
        {
            if ( !m_isEnabled ) return;

            m_showConsole = !m_showConsole;
            m_input = string.Empty;

            if ( m_showConsole )
            {
                // Retrieve all currently-available command attributes
                m_attributeHandler.GetAllCommandAttributes ();
            }
        }

        private void OnReturn ()
        {
            if ( !m_isEnabled ) return;

            if ( m_showConsole )
            {
                HandleInput ();
                m_input = string.Empty;
                m_selectedCommand = -1;
            }
        }

        #endregion

        #region Command Management

        #region Add/Remove Command

        public void AddCommand ( DebugCommand<float> debugCommand )
        {
            if ( debugCommand == null )
            {
                throw new ArgumentNullException ( "Couldn't add debug command: DebugCommand is null." );
            }
            if ( m_commands.Exists ( c => c.CommandId == debugCommand.CommandId ) )
            {
                return; // Command already exists
            }
            m_commands.Add ( debugCommand );
        }

        public void RemoveCommand ( string commandId )
        {
            if ( string.IsNullOrEmpty ( commandId ) )
            {
                throw new ArgumentException ( "Couldn't remove command: commandId is null or empty." );
            }
            DebugCommand command = ( DebugCommand ) m_commands.Find ( c => ( c as DebugCommand ).CommandId == commandId );
            if ( command == null )
            {
                throw new NullReferenceException ( $"Couldn't remove command: DebugCommand with commandId {commandId} does not exist." );
            }
            m_commands.Remove ( command );
        }

        #endregion

        private void HandleInput ()
        {
            string [] properties = m_input.Split ( ' ' );

            for ( int i = 0; i < m_commands.Count; i++ )
            {
                if ( properties [ 0 ] == m_commands [ i ].CommandId )
                {
                    if ( m_commands [ i ] is DebugCommand<float> commandFloat )
                    {
                        if ( float.TryParse ( properties [ 1 ], out float value ) )
                        {
                            commandFloat.Invoke ( value );
                        }
                    }
                    else if ( m_commands [ i ] is DebugCommand )
                    {
                        m_commands [ i ].Invoke ();
                    }
                    //else if ( m_commands [ i ] is DebugCommand<string> commandString )
                    //{
                    //    if ( !string.IsNullOrEmpty ( properties [ 1 ] ) )
                    //    {
                    //        commandString.Invoke ( properties [ 1 ] );
                    //    }
                    //}
                    break;
                }
            }
        }

        #endregion

        #region GUI

        private void OnGUI ()
        {
            // Console interaction input (toggle console, return key press)
            Event currentEvent = Event.current;
            if ( currentEvent.isKey && currentEvent.type == EventType.KeyUp )
            {
                switch ( Event.current.keyCode )
                {
                    case TOGGLE_CONSOLE_KEY:
                        OnToggleConsole ();
                        break;
                    case NAV_UP_KEY:
                        NavigateUp ();
                        break;
                    case NAV_DOWN_KEY:
                        NavigateDown ();
                        break;
                    case RETURN_KEY:
                        OnReturn ();
                        break;
                }
            }

            if ( !m_showConsole ) return;

            Color defaultBackgroundColor = GUI.backgroundColor;
            float y = 0f;

            if ( m_showHelp )
            {
                GUI.Box ( new Rect ( 0, y, Screen.width, 100 ), "" );

                Rect viewport = new Rect ( 0, 0, Screen.width - 30, 20 * m_commands.Count );

                m_helpScroll = GUI.BeginScrollView ( new Rect ( 0, y + 5f, Screen.width, 90 ), m_helpScroll, viewport );

                for ( int i = 0; i < m_commands.Count; i++ )
                {
                    string label = $"{m_commands [ i ].CommandFormat} - {m_commands [ i ].CommandDescription}";
                    Rect labelRect = new Rect ( 5, 20 * i, viewport.width - 100, 20 );
                    GUI.Label ( labelRect, label );
                }

                GUI.EndScrollView ();

                y += 100;
            }

            GUI.Box ( new Rect ( 0, y, Screen.width, 30 ), "" );
            GUI.backgroundColor = new Color ( 0, 0, 0, 0 );
            GUI.SetNextControlName ( "InputField" );
            m_input = GUI.TextField ( new Rect ( 10f, y + 5f, Screen.width - 20f, 20f ), m_input );
            GUI.FocusControl ( "InputField" );
            GUI.backgroundColor = defaultBackgroundColor;

            y += 20;

            if ( m_selectedCommand == -1 )
            {
                m_original = m_input;
                m_filteredCommands = FilterCommands ( m_input );
            }
            if ( string.IsNullOrEmpty ( m_input ) )
            {
                m_filteredCommands = FilterCommands ( m_input );
            }
            HintsView ( m_filteredCommands, y );
        }

        private void HintsView ( IDebugCommand [] filteredCommands, float y )
        {
            m_showHintsView = filteredCommands != null && filteredCommands.Length > 0;
            if ( !m_showHintsView ) return;

            float viewHeight = filteredCommands.Length * 20;
            GUI.Box ( new Rect ( 0, y += 10, Screen.width, viewHeight + 5 ), "" );
            GUIStyle defaultStyle = new GUIStyle ()
            {
                fontStyle = FontStyle.Normal
            };
            GUIStyle highlight = new GUIStyle ()
            {
                fontStyle = FontStyle.Bold
            };
            defaultStyle.normal.textColor = Color.white;
            highlight.normal.textColor = Color.green;

            for ( int i = 0; i < filteredCommands.Length; i++ )
            {
                Rect labelRect = new Rect ( 15, y + 5 + ( 20 * i ), Screen.width - 5, 20 );
                string label = $"{m_commands [ i ].CommandFormat} - {m_commands [ i ].CommandDescription}";
                bool highlighted = m_input == m_commands [ i ].CommandId;
                if ( highlighted ) m_selectedCommand = i;
                GUIStyle labelStyle = highlighted ? highlight : defaultStyle;
                GUI.Label ( labelRect, label, labelStyle );
            }
        }

        #endregion

        #endregion

        #region Utility

        private IDebugCommand [] FilterCommands ( string value )
        {
            if ( string.IsNullOrEmpty ( value ) ) return null;

            List<IDebugCommand> filteredCommands = new List<IDebugCommand> ();

            for ( int i = 0; i < m_commands.Count; i++ )
            {
                IDebugCommand command = m_commands [ i ];
                if ( m_commands [ i ].CommandId.Contains ( value ) )
                {
                    filteredCommands.Add ( command );
                }
            }
            return filteredCommands.ToArray ();
        }

        private void NavigateUp ()
        {
            if ( m_filteredCommands != null && m_filteredCommands.Length > 0 )
            {
                m_selectedCommand = Mathf.Max ( m_selectedCommand - 1, -1 );
                if ( m_selectedCommand >= 0 )
                {
                    m_input = m_filteredCommands [ m_selectedCommand ].CommandId;
                }
                else
                {
                    m_input = m_original;
                }
            }
            else
            {
                m_selectedCommand = -1;
            }
        }

        private void NavigateDown ()
        {
            if ( m_filteredCommands != null && m_filteredCommands.Length > 0 )
            {
                m_selectedCommand = ( m_selectedCommand + 1 ) % m_filteredCommands.Length;
                m_input = m_filteredCommands [ m_selectedCommand ].CommandId;
            }
            else
            {
                m_selectedCommand = -1;
            }
        }

        #endregion
    }
}
