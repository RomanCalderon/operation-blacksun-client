using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponAnimatorController : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator = null;

    private void OnEnable ()
    {
        WeaponsController.OnSetTrigger += SetTrigger;
        WeaponsController.OnSetBool += SetBool;
        WeaponsController.OnSetFloat += SetFloat;
    }

    private void OnDisable ()
    {
        WeaponsController.OnSetTrigger -= SetTrigger;
        WeaponsController.OnSetBool -= SetBool;
        WeaponsController.OnSetFloat -= SetFloat;
    }

    // Start is called before the first frame update
    private void Awake ()
    {
        Debug.Assert ( m_animator != null, "m_animator is null." );
    }

    private void SetTrigger ( string parameterName )
    {
        if ( !m_animator.isInitialized || !AnimatorHasParameter ( parameterName ) )
        {
            return;
        }
        m_animator.SetTrigger ( parameterName );
    }
    private void SetBool ( string parameterName, bool value )
    {
        if ( !m_animator.isInitialized || !AnimatorHasParameter ( parameterName ) )
        {
            return;
        }
        m_animator.SetBool ( parameterName, value );
    }

    private void SetFloat ( string parameterName, float value )
    {
        if ( !m_animator.isInitialized || !AnimatorHasParameter ( parameterName ) )
        {
            return;
        }
        m_animator.SetFloat ( parameterName, value );
    }

    /// <summary>
    /// Returns true if m_animator has a parameter with name <paramref name="parameterName"/>.
    /// </summary>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <returns></returns>
    private bool AnimatorHasParameter ( string parameterName )
    {
        AnimatorControllerParameter [] parameters = m_animator.parameters;

        foreach ( AnimatorControllerParameter parameter in parameters )
        {
            if ( parameter.name == parameterName )
            {
                return true;
            }
        }
        return false;
    }

    private float GetAnimationClipLength ( string clipName )
    {
        //Animator.StringToHash ( "" )

        RuntimeAnimatorController animController = m_animator.runtimeAnimatorController;
        for ( int i = 0; i < animController.animationClips.Length; i++ )
        {
            if ( animController.animationClips [ i ].name == clipName )
            {
                return animController.animationClips [ i ].length;
            }
        }
        Debug.LogWarning ( $"Couldn't find clip with name [{clipName}]" );
        return 0f;
    }
}
