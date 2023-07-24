/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: April 3rd, 2022                                         */
/* Class: InPlaceMovement                                        */
/* Purpose:                                                      */
/* Allow users to explore an area without having to get up       */
/* Conceptually, this works as if the user is on a segway        */
/* There is two modes                                            */
/* 1) autoWalk == false: ==> segway does not move, but can turn  */
/* 2) autoWalk == true: ==> segway moves forward, but can't      */
/*                          change directions while moving       */
/*****************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InPlaceMovement_Editor : MonoBehaviour
{

#if UNITY_EDITOR

    OnScreenDisplay onScreenDisplay;

    AudioSource audioSource;
    Rigidbody rb;

    VerbalManager_General vmg;

    public GameObject User;
    public GameObject body;
    public GameObject head;
    public GameObject grip;
    public GameObject cane;

    private float moveSpeed = 1f;
    private float turnSpeed = 20f;

    //public bool autoMove = false;
    public bool move = false;

    public Vector3 target;
    float clipLength;
    float delay;
    //float moreDelay = 0f;
    public Vector3 _currentPos;
    public Vector3 _prevPos;
    public Vector3 _diff;

    public float current_x = 0f;
    public float lastRot_y = 0f;

    private float forwardThreshold = 60f;
    private float backwardThresold = -70f;

    public bool tiltButtonMode = false;
    public bool buttonMode = false;
    public bool tiltMode = false;

    private float width;
    private float height;

    void Awake()
    {
        //destroy this object if not in correct scene,
        //if (SceneManager.GetActiveScene().name != "InPlaceExploration")
        //    Destroy(gameObject);

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InPlaceSetUp();
        onScreenDisplay = GameObject.Find("OnScreenDisplay").GetComponent<OnScreenDisplay>();
        vmg = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        rb.freezeRotation = true;
        clipLength = audioSource.clip.length;

        width = (float)Screen.width;
        height = (float)Screen.height;
    }

    //// Update is called once per frame
    void FixedUpdate()
    {
        calculateDiff();
        //current_x = grip.transform.rotation.x;
        current_x = grip.transform.localRotation.x;
        onScreenDisplay.toDisplay(head.transform.eulerAngles.ToString() + "\n" + head.transform.localEulerAngles.ToString());
        // onScreenDisplay.toDisplay(rp.transform.localEulerAngles.ToString() + "\n" + rp.transform.eulerAngles.ToString() + "\n" + temp.transform.localEulerAngles.ToString() + "\n" + temp.transform.eulerAngles.ToString());
        // onScreenDisplay.toDisplay(grip.transform.eulerAngles.ToString() + "\n" + rp.transform.localEulerAngles.ToString());

        if(Mathf.Round(head.transform.eulerAngles.y % 45) == 0)
        {
            if (!UAP_AccessibilityManager.IsSpeaking())
            {
                UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString() + " degrees", true, true, UAP_AudioQueue.EInterrupt.All);
            }
        }

        tiltButtonMovement();
        // if(buttonMode)
        //     buttonMovement();
        // else if(tiltButtonMode)
        //     tiltButtonMovement();
        // else if(tiltMode)
        //     tiltMovement();
    }
    public void TiltButtonMode()
    {
        tiltButtonMode = true;
        buttonMode = false;
        tiltMode = false;
    }

    public void ButtonMode()
    {
        tiltButtonMode = false;
        buttonMode = true;
        tiltMode = false;
    }
    
    public void TiltMode()
    {
        tiltButtonMode = false;
        buttonMode = false;
        tiltMode = true;
    }

    void OnGUI()
    {
        if(GUI.Button(new Rect(width/2 + 30, 30, 100, 100), "TiltButton"))
        {
            TiltButtonMode();
        }
        if(GUI.Button(new Rect(width/2 + 130, 30, 100, 100), "Button"))
        {
            ButtonMode();
        }

        if(buttonMode)
        {
            if (GUI.RepeatButton(new Rect(20, 40, 50, 50), "forward"))
            {
                // onScreenDisplay.toDisplay("Clicked the button with an image");
                segwayForward();
            }
            else if (GUI.RepeatButton(new Rect(20, 100, 50, 30), "backward"))
            {
                // onScreenDisplay.toDisplay("Clicked the button with text");
                segwayBackward();
            }
            else 
                segwayTurnsWithUser();
        }
    }


    /// <summary>
    /// Tank Controls: Use buttons to move forward, backward, turn left and right.
    /// </summary>
    void buttonMovement()
    {
        if (Input.GetKey(KeyCode.W))
        {
            segwayForward();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            segwayBackward();
        }
        else
        {
            segwayTurnsWithUser();

            //lastRot_y = User.transform.eulerAngles.y;
            //target = new Vector3(0, lastRot_y, 0);
            //transform.rotation = Quaternion.Euler(target);
        }

        if (Input.GetKey(KeyCode.A))
        {
            segwayTurnLeft();
        }
        else if (Input.GetKey(KeyCode.D))
        {
            segwayTurnRight();
        }
    }

    /// <summary>
    /// Use button to move forward, tilt to move backward. Avatar turns as user turns. Assumption is that user will not look around when moving backward
    /// </summary>
    void tiltButtonMovement()
    {

        if (Input.GetKey(KeyCode.W))
        {
            segwayForward();
        }
        else if ((current_x < 0.05f) && (current_x > -0.05f))
        {
            //vmg.Speak(Mathf.Round(head.transform.eulerAngles.y).ToString());
            //UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString(), true, true, UAP_AudioQueue.EInterrupt.All);
            //UAP_AccessibilityManager.StopSpeaking();

            // if (!UAP_AccessibilityManager.IsSpeaking())
            // {
            //     UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString(), true, true, UAP_AudioQueue.EInterrupt.All);
            // }
        }
        else if (current_x < -0.55f && current_x > -0.75f)
        {
            segwayBackward();
        }
        else
        {
            segwayTurnsWithUser();
        }

        
    }

    /// <summary>
    /// Attempt to use phone tilt to move avatar forward/backward. Some issues as user cannot move their cane around while moving forward/backward.
    /// </summary>
    void tiltMovement()
    {
        current_x = grip.transform.eulerAngles.x;
        if (current_x > 180) current_x -= 360f;
        if (current_x < backwardThresold)
        {
            segwayBackward();
        }
        else if (current_x > forwardThreshold)
        {
            segwayForward();
        }
        else
        {
            segwayTurnsWithUser();
            //lastRot_y = User.transform.eulerAngles.y;
            //target = new Vector3(0, lastRot_y, 0);
            //transform.rotation = Quaternion.Euler(target);
        }

    }


    /// <summary>
    /// Audio plays footsteps based on how fast the user is moving. For some reason when walking into walls, there still seems to be slight
    /// movement, so a threshold of 0.5f is needed. Basically, >=1.0 is normal audio speed, 0.5 - 0.99 is audio with delay, <0.5 is no audio
    /// </summary>
    void playAudio()
    {
        if (!audioSource.isPlaying && delay > 0.5f)
        {
            if (delay > 1) delay = 1;

            delay = Mathf.Abs(delay - 1);
            audioSource.PlayDelayed(clipLength + delay);
        }
    }

    public void segwayForward()
    {
        Debug.Log("Moving forward");
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        User.transform.position = new Vector3(transform.position.x, User.transform.position.y, transform.position.z);
        playAudio();
    }

    public void segwayBackward()
    {
        Debug.Log("Moving backward");
        onScreenDisplay.toDisplay("Backward");
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        User.transform.position = new Vector3(transform.position.x, User.transform.position.y, transform.position.z);
        playAudio();
    }

    public void segwayTurnLeft()
    {
        Debug.Log("Turning Left");
        User.transform.Rotate(0f, -turnSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void segwayTurnRight()
    {
        Debug.Log("Turning Right");
        User.transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f, Space.World);
    }

    public void segwayTurnsWithUser()
    {
        transform.rotation = Quaternion.Euler(0, User.transform.eulerAngles.y, 0);
    }

    void calculateDiff()
    {
        _currentPos = transform.position;

        _diff = _currentPos - _prevPos;
        delay = Mathf.Abs(_diff.x) + Mathf.Abs(_diff.y) + Mathf.Abs(_diff.z);
        delay *= 100f;
        //onScreenDisplay.toDisplay("Vector: " + delay);

        _prevPos = _currentPos;
    }

    //void segwayMovement()
    //{
    //    if (autoMove)
    //    {
    //        // lock y-rotation and move forward
    //        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
    //        User.transform.position = new Vector3(transform.position.x, User.transform.position.y, transform.position.z);
    //        //lastRot_y = User.transform.rotation.y;
    //    }
    //    else if (!autoMove)
    //    {
    //        // segway y-rotation follows head and does not move
    //        // i.e. user gets off segway

    //        // for editor
    //        // set the segway to whatever the head is facing
    //        lastRot_y = User.transform.eulerAngles.y;
    //        target = new Vector3(0, lastRot_y, 0);
    //        transform.rotation = Quaternion.Euler(target);
    //        //User.transform.eulerAngles = new Vector3(0, lastRot_y, 0);

    //        // for ios
    //        // target = head.transform.eulerAngles;
    //        // transform.rotation = Quaternion.Euler(target);
    //    }
    //}

    void InPlaceSetUp()
    {
        Debug.Log("In place exploration is on");

        // find gameobj user so can can configure this.gameobject position
        User = GameObject.Find("User");
        // find gameobj head to get y-rotation
        head = User.transform.GetChild(0).gameObject;
        body = User.transform.GetChild(1).gameObject;
        grip = User.transform.GetChild(2).gameObject;
        cane = grip.transform.GetChild(1).gameObject;

        // ignore the collision in the gameObj body->SurroundRadar
        GameObject surroundRadar = body.transform.GetChild(0).gameObject;
        Physics.IgnoreCollision(surroundRadar.GetComponent<Collider>(), GetComponent<Collider>());
        Physics.IgnoreCollision(cane.GetComponent<Collider>(), GetComponent<Collider>());

        // set sphere to position of User
        transform.position = new Vector3(User.transform.position.x, 0, User.transform.position.z); // move segway to User position
        // transform.rotation = Quaternion.Euler(0, User.transform.eulerAngles.y, 0); // match segway rotation to User
        segwayTurnsWithUser();
    }

    #endif

}
