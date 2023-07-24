/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: June 20th, 2023                                         */
/* Class: InPlaceMovement                                        */
/* Purpose:                                                      */
/* To display information phone screen.                          */
/*****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnScreenDisplay : MonoBehaviour
{  
    InPlaceMovement_IOS inPlaceMovement_IOS;
    GUIStyle myButtonStyle;

    private float width;
    private float height;
    private string _text;

    string mainMenu;

    void Awake()
    {
        width = (float)Screen.width;
        height = (float)Screen.height;
    }

    // Start is called before the first frame update
    void Start()
    {
        _text = "Testing";
        inPlaceMovement_IOS = GameObject.Find("segway").GetComponent<InPlaceMovement_IOS>();
        mainMenu = "MainMenu";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void toDisplay(string text)
    {
        _text = text;
    }

    void OnGUI()
    {
        // Compute a fontSize based on the size of the screen width.
        GUI.skin.label.fontSize = (int)(Screen.width / 25.0f);
        GUI.contentColor = Color.black;
        GUI.Label(new Rect(20, 80, width, height * 0.25f),
            _text);

        if(myButtonStyle == null)
        {
            myButtonStyle = new GUIStyle(GUI.skin.button);
            myButtonStyle.fontSize = 100;
        }

        // Activates different modes of movement
        if(GUI.Button(new Rect(width/2 - 50, 70, 150, 150), "Simple"))
        {
            inPlaceMovement_IOS.SimpleMode();
        }
        if(GUI.Button(new Rect(width/2 + 110, 70, 150, 150), "Button"))
        {
            inPlaceMovement_IOS.ButtonMode();
        }
        if(GUI.Button(new Rect(width/2 + 270, 70, 150, 150), "Swipe"))
        {
            inPlaceMovement_IOS.SwipeMode();
        }
        if(GUI.Button(new Rect(30, height - 160, 150, 150), "Back"))
        {
            SceneJumpHelper.ResetThenSwitchScene(mainMenu);
        }
    }
}
