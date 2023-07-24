/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: April 3rd, 2022                                         */
/* Class: InPlaceMovement                                        */
/* Purpose:                                                      */
/* Allow users to explore an area without having to get up       */
/* Conceptually, this works as if the user is on a segway        */
/* There are several modes, but they all function similarly      */
/* If the User is movement forward or backwards, the segway      */
/* will carry them forward or backward and will not turn.        */
/* Users can still turn while moving.                            */
/* If the User is not moving, then the segway will turn with     */
/* the User.                                                     */
/*****************************************************************/

// TODO: add turn functions for buttonMovement and the tap-for-rotational-info
//       add audio cue for movement. Keep it consistant
//       fix the phone-points-down-and-leaves-avatar problem

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class InPlaceMovement_IOS : MonoBehaviour
{

#if UNITY_IOS 

    OnScreenDisplay onScreenDisplay;
    AudioSource audioSource;
    Rigidbody rb;
    VerbalManager_General vmg;
    ARSessionOrigin arSessionOrigin;

    private GameObject User;
    private GameObject body;
    private GameObject head;
    private GameObject grip;
    private GameObject cane;
    public List<AudioClip> motionAudioList = new List<AudioClip>();

    public GameObject TouchScreen;

    private float moveSpeed = 0.5f;
    private float turnSpeed = 20f;

    // public bool autoMove = false;
    // public bool segwayMove = false;
    public bool moving = false;
    // bool flag = false;

    public Vector3 target;
    float clipLength;
    float delay;
    public Vector3 _currentPos;
    public Vector3 _prevPos;
    public Vector3 _diff;

    public float current_x;
    public float lastRot_y = 0f;

    // private float forwardThreshold = 60f;
    // private float backwardThresold = -60f;
    private float touchThreshold = 0.2f;                    // a time threshold. If below threshold, an audio will tell users their y rotation
    private float count = 0f;

    private bool simpleMode = false;
    private bool buttonMode = false;
    private bool swipeMode = false;

    private float width;
    private float height;

    private bool forward = false;
    private bool backward = false;
    private bool left = false;
    private bool right = false;

    private Vector2 initTouch;
    private Vector2 endTouch;
    private Vector2 diffTouch;

    // GUIStyle myButtonStyle;

    void Awake()
    {
        //destroy this object if not in correct scene,
        //if (SceneManager.GetActiveScene().name != "InPlaceExploration")
        //    Destroy(gameObject);
        audioSource = GetComponent<AudioSource>();
        rb = GetComponent<Rigidbody>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InPlaceSetUp();

        onScreenDisplay = GameObject.Find("OnScreenDisplay").GetComponent<OnScreenDisplay>();
        vmg = GameObject.Find("SoundBall").GetComponent<VerbalManager_General>();
        arSessionOrigin = FindObjectOfType<ARSessionOrigin>();
        rb.freezeRotation = true;

        width = (float)Screen.width;
        height = (float)Screen.height;

        SimpleMode(); // default mode
    }

    //// Update is called once per frame
    void FixedUpdate()
    {
        calculateDiff();
        current_x = grip.transform.localEulerAngles.x;
        /*
        rotation
        localrotation
        eulerangles
        localeulerangles
        */
        // onScreenDisplay.toDisplay(grip.transform.rotation.ToString() + "\n" + grip.transform.localRotation.ToString() + "\n" + grip.transform.eulerAngles.ToString() + "\n" + grip.transform.localEulerAngles.ToString());
        // onScreenDisplay.toDisplay(grip.transform.localRotation.ToString() + "\n" + cane.transform.localRotation.ToString() + "\n" + grip.transform.localEulerAngles.ToString() + "\n" + cane.transform.localEulerAngles.ToString());
        // onScreenDisplay.toDisplay(arSessionOrigin.camera.transform.localEulerAngles.ToString() + "\n" + arSessionOrigin.camera.transform.eulerAngles.ToString());

        // if(!TouchScreen.activeSelf && buttonMode)
        //     TouchScreen.SetActive(true);
            // Debug.Log("Nothing to see here");
        if (buttonMode)
            buttonMovement();
        else if(TouchScreen.activeSelf && !buttonMode)
            TouchScreen.SetActive(false);
        else if(simpleMode)
            SimpleMovement();
        else if(swipeMode)
            swipeMovement();
        // else if(tiltMode)
        //     tiltMovement();

    }

    /// <summary>
    /// Function to for 4 different movement modes. Not all are implemented. All they do is change the boolean values.
    /// </summary>
    public void SimpleMode()
    {
        onScreenDisplay.toDisplay("SimpleMode on");
        simpleMode = true;
        buttonMode = false;
        swipeMode = false;
    }

    public void ButtonMode()
    {
        onScreenDisplay.toDisplay("ButtonMode on");
        simpleMode = false;
        buttonMode = true;
        swipeMode = false;
    }

    public void SwipeMode()
    {
        onScreenDisplay.toDisplay("SwipeMode on");
        simpleMode = false;
        buttonMode = false;
        swipeMode = true;
    }

    /// <summary>
    /// Tank Controls: Use buttons to move forward, backward, turn left and right. Originally wanted to do this with canvas buttons. 
    /// But for some reason, I cannot interact with them. Also some problems with Gui.buttons as the repeatButton function is 
    /// inadequate for what I am trying to do
    /// </summary>
    void OnGUI()
    {
        if(buttonMode)
        {
            if (GUI.RepeatButton(new Rect(width-325, height/2 - 200, 200, 200), "forward"))
            {
                // onScreenDisplay.toDisplay("Clicked the button with an image");
                // segwayForward();
                // Debug.Log(GUI.RepeatButton.position);
                forward = true;
            }
            else if (GUI.RepeatButton(new Rect(width-325, height/2 + 200, 200, 200), "backward"))
            {
                // onScreenDisplay.toDisplay("Clicked the button with text");
                // segwayBackward();
                // Debug.Log(GUI.RepeatButton.position);
                backward = true;
            }
            else if (GUI.RepeatButton(new Rect(width-500, height/2, 200, 200), "left"))
            {
                // onScreenDisplay.toDisplay("Clicked the button with text");
                // segwayBackward();
                // Debug.Log(GUI.RepeatButton.position);
                left = true;
            }
            else if (GUI.RepeatButton(new Rect(width-200, height/2, 200, 200), "right"))
            {
                // onScreenDisplay.toDisplay("Clicked the button with text");
                // segwayBackward();
                // Debug.Log(GUI.RepeatButton.position);
                right = true;
            }
        }

    }

    /// <summary>
    /// Works hand-in-hand with OnGUI. Based on what button is pressed in OnGUI, it will move the the avatar accordingly. Tap feature is implemented so users can tap the screen and figure out which direction they are facing.
    /// </summary>
    void buttonMovement()
    {
        if(forward)
            segwayForward();
        else if (backward)
            segwayBackward();
        else
        {
            segwayTurnsWithHead();
        }
        
        if(left)
            segwayTurnLeft();
        else if(right)
            segwayTurnRight();
        else
        {
            User.GetComponent<HeadphoneTracker>().NoTurn();
        }

        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            // onScreenDisplay.toDisplay("forwardButton: " + forwardButton.GetComponent<RectTransform>().ToString() + "\nbackwardButton" + backwardButton.GetComponent<RectTransform>().ToString() + "\ntouch: " + touch.position.ToString());
            onScreenDisplay.toDisplay("Tap");
            switch(touch.phase)
            {
                case TouchPhase.Began:
                    break;
                case TouchPhase.Stationary:
                    count += Time.deltaTime;
                    break;
                case TouchPhase.Ended:
                    if(count <= touchThreshold)
                    {
                        if (!UAP_AccessibilityManager.IsSpeaking())
                        {
                            UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString() + " degrees", true, true, UAP_AudioQueue.EInterrupt.All);
                        }
                    }
                    count = 0f;
                    break;
            }
        }
        else
        {
            moving = false;
            forward = false;
            backward = false;
            left = false;
            right = false;
            stopAudio();
        }
    }

    /// <summary>
    /// Using swipe to move forward, backward, and turn left or right. User can tap on screen to get y rotation. 
    /// Coding problem: the left and right turn is causing head and body to sometimes rotate independantly
    /// Usage problem: some users may not be so good with their hands, making swiping harder 
    /// </summary>
    void swipeMovement()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch(touch.phase)
            {
                case TouchPhase.Began:
                    initTouch = touch.position;
                    // onScreenDisplay.toDisplay("initial touch: " + initTouch.ToString());
                    break;

                case TouchPhase.Moved:
                    endTouch = touch.position;
                    // onScreenDisplay.toDisplay("end touch: " + endTouch.ToString());
                    break;

                case TouchPhase.Stationary:
                    count += Time.deltaTime;
                    diffTouch = endTouch - initTouch;
                    
                    //forward or backward based on init finger position and end finger position
                    if(count > touchThreshold)
                    {
                        if(Mathf.Abs(diffTouch.y) > Mathf.Abs(diffTouch.x))
                        {
                            if(diffTouch.y > 0)
                            {
                                segwayForward();
                            }
                            else if(diffTouch.y < 0)
                            {
                                segwayBackward();
                            }
                        }
                        //left or right
                        else
                        {
                            if(diffTouch.x > 0)
                            {
                                segwayTurnRight();
                            }
                            else if(diffTouch.x < 0)
                            {
                                segwayTurnLeft();
                            }
                        }
                    }
                    break;
                    
                case TouchPhase.Ended:
                    if(count <= touchThreshold)
                    {
                        if (!UAP_AccessibilityManager.IsSpeaking())
                        {
                            UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString() + " degrees", true, true, UAP_AudioQueue.EInterrupt.All);
                        }
                    }
                    break;
            }
        }
        else
        {
            moving = false;
            diffTouch = Vector2.zero;
            initTouch = Vector2.zero;
            endTouch = Vector2.zero;
            count = 0f;
            User.GetComponent<HeadphoneTracker>().NoTurn();
            segwayTurnsWithHead();
            stopAudio();
        }
    }

    /// <summary>
    /// Use button to move forward, tilt to move backward. Avatar turns as user turns. 
    /// Assumption is that user will not look around when moving backward
    /// User can tap on the screen to get y rotation or long press to move forward. 
    /// You can also swipe up to move forward, or swipe back to move backwards.
    /// Tilting the phone has been removed since it makes the phone go wonky
    /// </summary>
    void SimpleMovement()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            switch(touch.phase)
            {
                case TouchPhase.Began:
                    initTouch = touch.position;
                    break;
                case TouchPhase.Moved:
                    endTouch = touch.position;
                    // onScreenDisplay.toDisplay("end touch: " + endTouch.ToString());
                    break;
                case TouchPhase.Stationary:
                    count += Time.deltaTime;
                    diffTouch = endTouch - initTouch;
                    
                    if(count > touchThreshold)
                    {
                        if(Mathf.Abs(diffTouch.y) > Mathf.Abs(diffTouch.x))
                        {
                            if(diffTouch.y > -15)
                            {
                                // Swipe up to move forward
                                segwayForward();
                            }
                            else if (diffTouch.y < -15)
                            {
                                // Swipe down to mvoe backward
                                segwayBackward();
                            }
                        }
                        else
                        {
                            // Long press to move forward
                            segwayForward();
                        }
                    }
                    break;
                case TouchPhase.Ended:
                    // Tap to get y rotation
                    if(count <= touchThreshold)
                    {
                        if (!UAP_AccessibilityManager.IsSpeaking())
                        {
                            UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString() + " degrees", true, true, UAP_AudioQueue.EInterrupt.All);
                        }
                    }
                    
                    break;
            }
        }
        else
        {
            moving = false;
            count = 0f;
            diffTouch = Vector2.zero;
            initTouch = Vector2.zero;
            endTouch = Vector2.zero;
            segwayTurnsWithHead();
            stopAudio();
        }
        //onScreenDisplay.toDisplay(grip.transform.rotation.ToString());
    }

    /// <summary>
    /// Function to move the player forward. This moves the gameobject segway forward which moves the player forward. 
    /// Reason I didnt move the gameobject User forward is because I couldnt seperate the user turning and moving. 
    /// The way this works is when the segway moves forward, it locks the y rotation and moves straight. 
    /// </summary>
    public void segwayForward()
    {
        Debug.Log("Moving forward");
        // onScreenDisplay.toDisplay("Forward");
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);
        User.transform.position = new Vector3(transform.position.x, User.transform.position.y, transform.position.z);
        if (!UAP_AccessibilityManager.IsSpeaking() && !moving)
        {
            UAP_AccessibilityManager.Say("Forward", true, true, UAP_AudioQueue.EInterrupt.All);
            moving = true;
        }
        playAudio(0);

    }

    /// <summary>
    /// Function to move the player backward. Same setup as segwayForward().
    /// </summary>
    public void segwayBackward()
    {
        Debug.Log("Moving backward");
        // onScreenDisplay.toDisplay("Backward");
        transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        User.transform.position = new Vector3(transform.position.x, User.transform.position.y, transform.position.z);
        if (!UAP_AccessibilityManager.IsSpeaking() && !moving)
        {
            UAP_AccessibilityManager.Say("Backward", true, true, UAP_AudioQueue.EInterrupt.All);
            moving = true;
        }
        playAudio(1);
    }

    /// <summary>
    /// Function to turn the player left. This calls a function in HeadphoneTracker to turn the head while also turning the User.
    /// Needs both as just turning the User does not turn the head. 
    /// Also, when User turns a certain degree, the UAP will call out what their current rotation is. This should be improved upon
    /// such as adding an indicator that the user is turn and/or switching to a different way of telling how the user how
    /// they are oriented
    /// </summary>
    public void segwayTurnLeft()
    {
        Debug.Log("Turning Left");
        User.GetComponent<HeadphoneTracker>().TurnLeft();
        // head.transform.Rotate(0f, -turnSpeed * Time.deltaTime, 0f, Space.Self);
        User.transform.Rotate(0f, -turnSpeed * Time.deltaTime, 0f, Space.World);
        if(Mathf.Round(head.transform.eulerAngles.y % 45) == 0)
        {
            if (!UAP_AccessibilityManager.IsSpeaking())
            {
                if(!moving)
                {
                    UAP_AccessibilityManager.Say("Left", false, true, UAP_AudioQueue.EInterrupt.All); // does not talk. Probably interrupted by second UAP
                    moving = true;
                }
                UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString() + " degrees", true, true, UAP_AudioQueue.EInterrupt.All);
            }
        }
        playAudio(2);
    }

    /// <summary>
    /// Function to turn the player right. Same setup as segwayTurnLeft().
    /// </summary>
    public void segwayTurnRight()
    {
        Debug.Log("Turning Right");
        User.GetComponent<HeadphoneTracker>().TurnRight();
        // head.transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f, Space.Self);
        User.transform.Rotate(0f, turnSpeed * Time.deltaTime, 0f, Space.World);
        if(Mathf.Round(head.transform.eulerAngles.y % 45) == 0)
        {
            if (!UAP_AccessibilityManager.IsSpeaking())
            {
                if(!moving)
                {
                    UAP_AccessibilityManager.Say("Right", false, true, UAP_AudioQueue.EInterrupt.All); // does not talk. Probably interrupted by second UAP
                    moving = true;
                }
                UAP_AccessibilityManager.Say(Mathf.Round(head.transform.eulerAngles.y).ToString() + " degrees", true, true, UAP_AudioQueue.EInterrupt.All);
            }
        }
        playAudio(3);
    }

    /// <summary>
    /// Function that rotates the segway with the user's head. 
    /// </summary>    
    public void segwayTurnsWithHead()
    {
        transform.rotation = Quaternion.Euler(0, head.transform.eulerAngles.y, 0);
    }

    /// <summary>
    /// Calculate the positional difference between each frame. Mainly used to calculate the delays between each footsteps.
    /// There is some issue with current setup as Walking against walls does not always yield footstep, maybe its just 
    /// taking too long to play the footstep audio.
    /// An alternative to this appoarch could be to play the audio when certain distance is covered. 
    /// </summary>
    void calculateDiff()
    {
        _currentPos = transform.position;

        _diff = _currentPos - _prevPos;
        delay = Mathf.Abs(_diff.x) + Mathf.Abs(_diff.y) + Mathf.Abs(_diff.z);
        delay *= 100f;
        //onScreenDisplay.toDisplay("Vector: " + delay);

        _prevPos = _currentPos;
    }

    /// <summary>
    /// Audio plays footsteps based on how fast the user is moving (only for moving forward and backward). For some reason when walking into walls, there still seems to be slight
    /// movement, so a threshold of 0.5f is needed. Basically, >=1.0 is normal audio speed, 0.5 - 0.99 is audio with delay, < 0.5 is no audio
    /// </summary>
    void playAudio(int mode)
    {
        audioSource.clip = motionAudioList[mode];
        
        if(mode == 0 || mode == 1)
        {
            clipLength = audioSource.clip.length;

            if (!audioSource.isPlaying && delay > 0.5f)
            {
                if (delay > 1) delay = 1;

                delay = Mathf.Abs(delay - 1);
                audioSource.PlayDelayed(clipLength + delay);
            }
        }
        else
        {
            if(!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
    }

    void stopAudio()
    {
        if(audioSource.isPlaying)
            audioSource.Stop();
    }

    private void OnCollisionEnter(Collision collision)
    {
        string other = collision.collider.name.Substring(0, 3);
        if(other == "Wall" && !UAP_AccessibilityManager.IsSpeaking())        
        {
            UAP_AccessibilityManager.Say("Hitting Wall", true, true, UAP_AudioQueue.EInterrupt.All);
        }
    }

    /// <summary>
    /// Function for basic setup. Just grabs gameobjects, moves segways, and ignores colliders so the segway does not shake
    /// </summary>
    void InPlaceSetUp()
    {
        Debug.Log("In place exploration is on");

        // find gameobj user so can can configure this.gameobject position
        User = GameObject.Find("User");
        // find gameobj head and body
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
        transform.rotation = Quaternion.Euler(0, head.transform.eulerAngles.y, 0); // match segway rotation to User

    }
#endif

}
