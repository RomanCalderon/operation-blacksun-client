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
    }

    private void OnDisable ()
    {
        WeaponsController.OnSetTrigger -= SetTrigger;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void SetTrigger ( string parameterName )
    {
        m_animator.SetTrigger ( parameterName );
    }
}
