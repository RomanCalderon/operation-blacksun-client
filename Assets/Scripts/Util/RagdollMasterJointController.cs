using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollMasterJointController : MonoBehaviour
{
    [SerializeField]
    private SkinnedMeshRenderer [] m_ragdollMeshRenderers = null;
    private List<RagdollJointController> m_jointControllers = new List<RagdollJointController> ();

    private void Awake ()
    {
        // Find all RagdollJointControllers in this ragdoll
        FindJointControllersRecursive ( transform );
    }

    // Start is called before the first frame update
    void Start ()
    {
        EnableFollowTarget ( true );
        EnableMeshRenderers ( false );
    }

    public void EnableFollowTarget ( bool state )
    {
        foreach ( RagdollJointController jointController in m_jointControllers )
        {
            jointController.SetFollowTarget ( state );
        }
    }

    public void EnableMeshRenderers ( bool state )
    {
        foreach ( SkinnedMeshRenderer renderer in m_ragdollMeshRenderers )
        {
            renderer.enabled = state;
        }
    }

    /// <summary>
    /// Recursively searches for all RagdollJointControllers in transform <paramref name="parent"/>.
    /// </summary>
    /// <param name="parent">The Transform to check.</param>
    private void FindJointControllersRecursive ( Transform parent )
    {
        RagdollJointController jointController;
        if ( jointController = parent.GetComponent<RagdollJointController> () )
        {
            m_jointControllers.Add ( jointController );
        }

        if ( parent.childCount == 0 )
        {
            return;
        }
        else
        {
            foreach ( Transform child in parent )
            {
                FindJointControllersRecursive ( child );
            }
        }
    }
}
