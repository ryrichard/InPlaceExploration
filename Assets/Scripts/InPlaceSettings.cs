/*****************************************************************/
/* Programmer: MRCane Development Team                           */
/* Date: June 20, 2022                                           */
/* Class: InPlaceMovement                                        */
/* Purpose:                                                      */
/* Settings needs to be adjusted for this to work                */
/* We want to make this protrait mode, so cameras needs to be    */
/* adjusted to make full use of a screen                         */
/*****************************************************************/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InPlaceSettings : MonoBehaviour
{
    [SerializeField] Camera mainCamera;
    [SerializeField] Camera headCamera;
    // [SerializeField] Camera caneCamera;
    [SerializeField] Camera caneCamera2;

    private void Awake()
    {
        // lock to autorotation
        // https://docs.unity3d.com/ScriptReference/ScreenOrientation.AutoRotation.html
        //Screen.autorotateToPortrait = true;
        //Screen.orientation = ScreenOrientation.AutoRotation;

        // Lock to Portrait mode so screen does not switch to landscrape. This will help lock the cane in place. 
        Screen.orientation = ScreenOrientation.Portrait;
    }

    // Start is called before the first frame update
    void Start()
    {


        mainCamera = GameObject.Find("Main Camera").GetComponent<Camera>();

        GameObject user = GameObject.Find("User");
        headCamera = user.transform.GetChild(0).GetChild(1).gameObject.GetComponent<Camera>();

        // caneCamera = user.transform.GetChild(2).GetChild(0).gameObject.GetComponent<Camera>();
        caneCamera2 = user.transform.GetChild(2).GetChild(3).gameObject.GetComponent<Camera>();

        Screen.orientation = ScreenOrientation.Portrait;

        mainCamera.rect = new Rect(0f, 0f, 1.0f, 0.5f);
        caneCamera2.rect = new Rect(0f, 0.5f, 0.5f, 0.5f);

    }
}
