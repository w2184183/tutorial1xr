using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SpatialTracking;

public class MockController : MonoBehaviour
{
    [SerializeField] bool mockInput = true; //replace with auto lookup when possible 
    [SerializeField] Vector2 sensitivity = new Vector2(8f, 0.5f); //the multiplier for the camera movement
    [SerializeField] Vector3 mockControllerOffset = new Vector3(0.1f, 0f, 0.5f); //used to offset the virtual controllers when the game is started 
    [SerializeField] GameObject rightControllerPivot; //pivot for the right controller
    [SerializeField] GameObject leftControllerPivot; //pivot to the left controller 
    GameObject playerCamera; //reference to the main camera in the scene

    readonly Mouse mouse = Mouse.current; //reference to the currently connected mouse 
    readonly Keyboard keyboard = Keyboard.current; //reference to currently connected keyboard
    Vector2 mousePos; //the mouse position. note this is multiplied by the sensitivity and is then applied to the camera transform

    NewControls controls; //contains the input events for the mouse. Note that the type of this variable is the same as the name of the "input actions" asset that contains the action list


    // Start is called before the first frame update
    void Awake()
    {
        controls = new NewControls(); //instantiating the HMD input controller
        playerCamera = GameObject.FindGameObjectWithTag("MainCamera"); //setting the camera reference 

        if (mockInput) //replace this with automatic lookup when available
        {
            //disabling tracked pose driver. note that this will disable the first tracked pose driver it finds when searching through children
            rightControllerPivot.GetComponentInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = false;
            leftControllerPivot.GetComponentInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = false;
            playerCamera.GetComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>().enabled = false;

            rightControllerPivot.GetComponentInChildren<Transform>().GetChild(0).transform.localPosition += mockControllerOffset; //applying the offset to the right controller which is a child of the pivot 

            mockControllerOffset.x = -mockControllerOffset.x; //inverting the x axis to the left controller isnt on top of right controller

            leftControllerPivot.GetComponentInChildren<Transform>().GetChild(0).transform.localPosition += mockControllerOffset; //applying the offset to the left controller child of pivot

        }
    }


    // Update is called once per frame
    void Update()
    {
        if (mockInput) //if enabled code will run, automatic checking for mockHMD is preferable
        {
            if (mouse.rightButton.isPressed) //if user is holding right mouse button allow them to move the camera
            {
                MoveWithMouse(playerCamera, true);
            }

            if (keyboard.qKey.isPressed) //if the user is holding the 'q' key then allow them to move the left controller
            {
                MoveWithMouse(leftControllerPivot, true);
                MoveWithScrollWheel(leftControllerPivot.GetComponentInChildren<Transform>().GetChild(0).gameObject);
            }

            if (keyboard.eKey.isPressed) //if the user is holding the 'e' key then allow them to move the left controller
            {
                MoveWithMouse(rightControllerPivot, true);
                MoveWithScrollWheel(rightControllerPivot.GetComponentInChildren<Transform>().GetChild(0).gameObject);
            }

            if (!(keyboard.qKey.isPressed)&& !(keyboard.eKey.isPressed)) 
            {
                MoveWithScrollWheel(gameObject.transform.root.gameObject);
            }
        }
    }

    /// <summary>
    /// if the mouse scroll wheel is being moved, allows target object to be moved backwards 
    /// and forwards based on the main camera rotation
    /// </summary>
    /// <param name="targetObject"></param>
    private void MoveWithScrollWheel(GameObject targetObject)
    {
        Vector3 camForward = playerCamera.transform.forward;
        camForward *= 0.2f;

        if (controls.MouseInput.MouseWheelUp.ReadValue<float>() > 0) //is the scroll wheel moving up
        {
            targetObject.transform.position += camForward;
        }
        else if (controls.MouseInput.MouseWheelUp.ReadValue<float>() < 0) //is the scroll wheel moving down
        {
            targetObject.transform.position -= camForward;
        }
    }

    /// <summary>
    /// rotates target object around based on mouse input. freeze Z primarily used
    /// for mockHMD to prevent rolling
    /// </summary>
    /// <param name="targetObject"></param>
    /// <param name="freezeZAxis"></param>
    private void MoveWithMouse(GameObject targetObject, bool freezeZAxis)
    {
        mousePos.x = controls.MouseInput.MouseX.ReadValue<float>(); //reading in the X delta of the mouse
        mousePos.y = controls.MouseInput.MouseY.ReadValue<float>(); //reading the Y delta of the mouse

        mousePos *= sensitivity; //multiple the mousepos by the sensitivity to get the distance to move the camera

        targetObject.transform.Rotate(Vector3.up, mousePos.x * Time.deltaTime); //rotate the camera left and right 
        targetObject.transform.Rotate(Vector3.left, mousePos.y * Time.deltaTime); //rotate the camera up and down

        if (freezeZAxis)
        {
            Vector3 newRot = targetObject.transform.rotation.eulerAngles; //a holder for the current rotation of the camera

            targetObject.transform.rotation = Quaternion.Euler(newRot.x, newRot.y, 0f); //setting the Z axis to 0 to prevent unintended rolling of the camera
        }
    }

    /// <summary>
    /// starts the new input system so inputs can be read in
    /// </summary>
    private void OnEnable()
    {
        controls.Enable();
    }

    /// <summary>
    /// stops the input system from running once the object is destroyed
    /// </summary>
    private void OnDestroy()
    {
        controls.Disable();
    }
}
