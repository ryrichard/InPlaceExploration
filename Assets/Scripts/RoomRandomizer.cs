/* Randomizing room size is difficult since we cant just create a wall with x size */
/* To get around this, we take a wall and create a room from it by using it as an  */
/* extender. I used an edited wall which is about 2f in length. I take this call   */
/* and scale it to the appropriate length or width with some overlap               */
/* The room starts in (0,0). The length and width are randomly created on runtime  */
/* with a limit from 20-40 for width and 10-20 for length.                         */
/* Once the room is generated, the objects are instantiated within the boundaries  */
/* of the room. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomRandomizer : MonoBehaviour
{
    public int difficultyLevel;

    OnScreenDisplay onScreenDisplay;

    public GameObject Wall1;
    public GameObject Wall2;
    public GameObject Wall3;
    public GameObject Wall4;

    private int WIDTH = 0;
    private int LENGTH = 0;

    public List<GameObject> listOfLargeObj;
    public List<GameObject> listOfSmallObj;

    void Awake()
    {
        WIDTH = Random.Range(20, 40);
        LENGTH = Random.Range(10, 20);
    }

    // Start is called before the first frame update
    void Start()
    {
        onScreenDisplay = GameObject.Find("OnScreenDisplay").GetComponent<OnScreenDisplay>();
        BuildWall(WIDTH, LENGTH);
        RandomizeObject(WIDTH, LENGTH);
    }

    // <summary>
    // Build the walls of the room. Its always going to be rectangular shaped.
    // The "0.1f" in some of the localPositions is to move the wall behind the gridline to allow more room for objects to spawn.
    // The "+0.5f" in the localScale is to ensure there is overlap with the walls and no possibility of an open gap.
    // </summary>
    void BuildWall(int width, int length)
    {
        float midWidth  = (float)width/2.0f;
        float midLength = (float)length/2.0f;

        Wall1.transform.localPosition   = new Vector3(midWidth,     0,          -0.1f);
        Wall1.transform.localScale      = new Vector3(1,            1,          width + 0.5f);

        Wall2.transform.localPosition   = new Vector3(0,            0,          midLength);
        Wall2.transform.localScale      = new Vector3(1,            1,          length + 0.5f);

        Wall3.transform.localPosition   = new Vector3(midWidth,     0,          length + 0.1f);
        Wall3.transform.localScale      = new Vector3(1,            1,          width + 0.5f);

        Wall4.transform.localPosition   = new Vector3(width + 0.1f,        0,          midLength);
        Wall4.transform.localScale      = new Vector3(1,            1,          length + 0.5f);
    }

    // <summary>
    // Use the shape of the room to get the boundaries that the object can instantiate in. 
    // </summary>
    void RandomizeObject(int width, int length)
    {
        TODO: Add spawn limits based on difficulty level (more difficult more spawned objects)
              Randomize what is spawned
              Randomize which category (large or small) is spawned with bias towards large(i.e 0.7 for large, 0.3 for small)
              Maybe(?) work on size of object


        int SIZEOFLARGEOBJ = listOfLargeObj.Count;
        int SIZEOFSMALLOBJ = listOfSmallObj.Count;
        float limit = 5.0f;           // the length and width of the nospawn zone from (0,0). This is where the User is located. 

        float xPos;
        float zPos;
        float yRot;

        foreach (GameObject go in listOfLargeObj)
        {
            xPos = Random.Range(limit, WIDTH - 1);
            zPos = Random.Range(limit, LENGTH - 1);
            yRot = Random.Range(0, 360);
            Quaternion rot = Quaternion.Euler(0, yRot, 0);
            Vector3 vec = new Vector3(xPos, 0.0f, zPos);
            Instantiate(go, new Vector3(xPos, 0, zPos), rot);

            Debug.Log(go.name + " Postion: (" + xPos + ", " + zPos + ") \nRotation: " + yRot);
        }

        // foreach (GameObject go in listOfSmallObj)
        // {

        // }
    }
}
