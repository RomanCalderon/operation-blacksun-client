using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

namespace DebugConsole
{
    public class AttributeHandler : MonoBehaviour
    {
        public void GetAllCommandAttributes ()
        {
            // Get all MonoBehaviours in scene
            MonoBehaviour [] scriptComponents = FindObjectsOfType<MonoBehaviour> ();

            foreach ( MonoBehaviour mono in scriptComponents )
            {
                Type monoType = mono.GetType ();

                foreach ( MethodInfo method in monoType.GetMethods ( BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance ) )
                {
                    object [] attributes = method.GetCustomAttributes ( typeof ( DebugCommandAttribute ), true );
                    if ( attributes.Length > 0 )
                    {
                        // Attribute info
                        string commandID = ( attributes [ 0 ] as DebugCommandAttribute ).CommandId;
                        string commandDescription = ( attributes [ 0 ] as DebugCommandAttribute ).CommandDescription;
                        string commandFormat = ( attributes [ 0 ] as DebugCommandAttribute ).CommandFormat;

                        // Method info
                        Type [] arguments = new Type [ 1 ] { typeof ( float ) };
                        MethodInfo info = UnityEventBase.GetValidMethodInfo ( mono, method.Name, arguments );

                        void execute ( float _ ) => info.Invoke ( mono, null );
                        DebugController.Instance.AddCommand ( new DebugCommand<float> ( commandID, commandDescription, commandFormat, execute ) );
                    }
                }
            }
        }
    }
}

[AttributeUsage ( AttributeTargets.Method, AllowMultiple = true )]
public class DebugCommandAttribute : Attribute
{
    public string CommandId;
    public string CommandDescription;
    public string CommandFormat;

    public DebugCommandAttribute ( string command_id, string description, string format = "command_id <optional_param_value>" )
    {
        CommandId = command_id;
        CommandDescription = description;
        CommandFormat = format;
    }
}
