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
}
