using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InPlaceInstructions : MonoBehaviour
{
    VerbalManager_General verbalManager_General;
    int DialogueOrder = 0;
    int currentDialogueOrder = 0;
    string instructions = "";

    // Start is called before the first frame update
    void Start()
    {
        verbalManager_General = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        instructions = "Dont move mothafucka";
    }

    // Update is called once per frame
    void Update()
    {
        if(DialogueOrder != currentDialogueOrder)
            Dialogue(DialogueOrder);
    }

    void Dialogue(int DialogueOrder)
    {
        currentDialogueOrder = DialogueOrder;
        switch(DialogueOrder)
        {
            case 0:
                instructions = "wassup";
                break;
            case 1:
                instructions = "wazzup";
                break;
        }
        try
        {
            verbalManager_General.StopSpeak();
            verbalManager_General.Speak(instructions);
        }
        catch (InvalidOperationException e)
        {
            Debug.Log("Error exists when open the Menu: " + e);
        }
    }
}
