using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Button))]
public class ServerButton : MonoBehaviour
{
    public Button button = null;

    [ Header ( "UI" )]
    [SerializeField]
    private TMP_Text m_titleText = null;
    [SerializeField]
    private TMP_Text m_playersText = null;

    private void Awake ()
    {
        button = GetComponent<Button> ();
    }

    public void Initialize ( string name, int playerCount, int maxPlayers )
    {
        m_titleText.text = name;
        m_playersText.text = playerCount + "/" + maxPlayers;
    }
}
