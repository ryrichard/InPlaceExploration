/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: April 3rd, 2022                                         */
/* Class: UserMovement_Editor                                    */
/* Purpose:                                                      */
/* The class controls Avatar's movement (translation & rotation) */
/* in Unity Editor. So the team can debug for interaction        */
/* related functions without build to phone. Thus, the following */
/* script only works in the Unity Editor. For "movement control  */
/* on phone", please visit "UserMovement_IOS" class instead.     */
/*****************************************************************/


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UserMovement_Editor : MonoBehaviour
{

#if UNITY_EDITOR

    public bool lockTranslation = false;              // an boolean variable allow disabling avatar's movement (rotation still available)

    GameObject gripPoint;
    GameObject head;                                  // will rotate head use keyboard to test spatial sounds in editor

    Vector3 userRot;                                  // For dynamically recording local euler angle (rotation) of user obj (Mouse movement will continuously update its rotation)
    Vector3 gripRot;                                  // ... of grip point obj ...

    public float moveSpeed = 10f;       // speed of user's translation
    public float rotSpeed = 50f;        // speed of user's rotation
    float updatedMoveSpeed;             // for enabling slow down movement

    public float sensitivity = 10f;     // control the sensitivity of mouse in the game

    public bool inPlace = false;
    public float turnSpeed = 10f;

    public float y_Rot = 0f;

    /// <summary>
    /// Start function initialize variables and setting when using Unity Editor
    /// </summary>
    private void Start()
    {
        string s = SceneManager.GetActiveScene().name.Substring(0, 18);
        if (s == "InPlaceExploration")
            InPlaceSetUp();
        
        MovementSetup();
    }


    /// <summary>
    /// Update user's movement when using Unity Editor (use Mouse + Keyboard control)
    /// </summary>
    void Update()
    {
        MovementControl();
    }


    /// <summary>
    /// Setups to do specifically for running this program on Unity Editor
    /// </summary>
    void MovementSetup()
    {
        /* Assigning "gripPoint" object */
        gripPoint = transform.Find("GripPoint").gameObject;
        head = transform.Find("Head").gameObject;

        /* Initialize these Vector 3 with the starting local euler angle (an representation of rotation) in the scene */
        userRot = transform.localEulerAngles;
        gripRot = gripPoint.transform.localEulerAngles;

        /* Lock the mouse into game once start */
        Cursor.lockState = CursorLockMode.Locked;
    }


    /// <summary>
    /// Method for controlling user's movement when running the program in Unity Editor
    /// </summary>
    void MovementControl()
    {
        if (!SceneManager.GetActiveScene().name.Contains("Replay"))
        {
            if(!inPlace)
                PositionControl();

            RotationControl();
        }

    }


    /// <summary>
    /// Method for controlling user's position when running on Unity Editor
    /// </summary>
    void PositionControl()
    {
        /* Update avatar's movement only when movement is not locked */
        if (!lockTranslation)
        {
            /* Press "Shift" to slow down the move speed when needed */
            if (Input.GetKey(KeyCode.LeftShift))
            {
                updatedMoveSpeed = moveSpeed * 0.1f;
            }
            else { updatedMoveSpeed = moveSpeed; }

            /* Using WASD to control User body's translation */
            if (Input.GetKey(KeyCode.W))
            {
                transform.Translate(Vector3.forward * updatedMoveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.S))
            {
                transform.Translate(Vector3.back * updatedMoveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.A))
            {
                transform.Translate(Vector3.left * updatedMoveSpeed * Time.deltaTime);
            }
            if (Input.GetKey(KeyCode.D))
            {
                transform.Translate(Vector3.right * updatedMoveSpeed * Time.deltaTime);
            }
        }
    }


    /// <summary>
    /// Method for controlling user's rotation when running on Unity Editor
    /// </summary>
    void RotationControl()
    {
        /* Using Mouse to control all the rotations 
         *
         * NOTE - Rotation rules:
         * 1. Horizontal rotation is y-axis
         * 2. Forward & Back rotation is x-axis
         * 3. Right to Left Side rotation is z-axis
         *
         */


         /*
         Reason for seperation:
         Original code (in If) made it so that User turns with mouse position. This meant if I wanted to turn User with 'A' or 'D', User would not
         turn as they are locked in with mouse.
         New changes (in else) made it so that User turns based on mouse movement, rather than based on position. This way both mouse movement and
         keyboard moves User y-rotation.
         */
         if(!inPlace)
         {
             userRot.y += Input.GetAxis("Mouse X") * sensitivity;        // Let user's horizontal rotation to be controlled by left & right movement of mouse
             transform.localEulerAngles = userRot;                       // Assigning the most updated ROT to user, head, and grip's localRotation
         }
         else
         {
             transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivity, 0f, Space.World);
         }

        gripRot.x -= Input.GetAxis("Mouse Y") * sensitivity;        // Let gripPoint's forward & back rotation to be controlled by fron & back movement of mouse. We use Why using "-=": For similar reason as above

        gripPoint.transform.localEulerAngles = gripRot;

        /* Rotate Avatar' head horizontally to test 3D audio */
        if (Input.GetKey(KeyCode.Q))
        {
            head.transform.localEulerAngles += new Vector3(0, -1, 0);
        }
        if (Input.GetKey(KeyCode.E))
        {
            head.transform.localEulerAngles += new Vector3(0, 1, 0);
        }

        /* Rotate Avatar's head vertically */
        if (Input.GetKey(KeyCode.R))
        {
            head.transform.localEulerAngles += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.F))
        {
            head.transform.localEulerAngles += new Vector3(1, 0, 0);
        }

        //if(Input.GetKey(KeyCode.A))
        //{
        //    turnLeft();
        //}
        //if(Input.GetKey(KeyCode.D))
        //{
        //    turnRight();
        //}

    }

    /// Inplace Code

    void InPlaceSetUp()
    {
        Debug.Log("In place exploration is on");
        inPlace = true;

        //// find gameobj user so can can configure this.gameobject position
        //User = GameObject.Find("User");
        //// find gameobj head to get y-rotation
        //head = User.transform.GetChild(0).gameObject;
        //// set sphere to position of User
        //transform.position = new Vector3(User.transform.position.x, 0, User.transform.position.z); // move segway to User position

        // need to make User a child of segway
    }

#endif

}

