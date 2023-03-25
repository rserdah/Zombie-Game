using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkingController : Controller
{
    //Movement
    public float speed = 5f;
    private Vector3 walkVelocity;

    //Events
    //public delegate void InteractHandler();
    //public event InteractHandler OnInteract;

    //Temp. - used for testing switching controllers
    public Controller carController;
    private InputManager inputManager;

    //Maybe have a list of delegates that represent what the specific controller should do for a given InputData
    //somehow have to make it so each index of the delegate list matches with the indices of the InputData (e.g. if the first index of the InputData is the shoot button, then the first index of the delegate list must be the action to occur
    //when the player presses shoot)
    //See:
    //https://stackoverflow.com/questions/3813261/how-to-store-delegates-in-a-list
    //Maybe:
    //System.Collections.Generic.Dictionary<string, System.Delegate>
    //Or (simpler but more restrictive as you can only have a certain function signature):
    //public delegate double MethodDelegate(double a);
    //    var delList = new List<MethodDelegate> { Foo, FooBar };


    //    Console.WriteLine(delList[0](12.34));
    //    Console.WriteLine(delList[1](16.34));


    public override void ReadInput(InputData data)
    {
        if(active)
        {
            walkVelocity = Vector3.zero;

            //Set vertical movement
            if(data.axes[0].input != 0f)
                walkVelocity += Vector3.forward * data.axes[0].input;

            //Set horizontal movement
            if(data.axes[1].input != 0f)
                walkVelocity += Vector3.right * data.axes[1].input;

            //if(Input.GetKeyDown(KeyCode.E))
            //if(data.buttons[1])
            if(data.buttons[1].keyDown)
            {
                inputManager.RequestTransfer(carController);
                Debug.LogError("Exitting player");
                active = false;
            }

            Debug.Log("(" + data.buttons[1] + ")");

            //Debug.Log("(" + data.axes[0].lastInput + ", " + data.axes[0].input + ", " + data.axes[0].timeDelta + ")\n(lastInput, input, timeDelta)");

            ////Working
            //if(data.axes[0].GetKeyDown())
            //    Debug.LogError("Key Down");

            ////Working
            //if(data.axes[0].GetKey())
            //    Debug.LogError("Key");

            ////Not Working - Seems to maybe skip over the frame where input goes from 1 to 0 so key up is not registered(maybe it is skipping passing input on that frame?)
            //if(data.axes[0].GetKeyUp())
            //    Debug.LogError("Key Up");


            newInput = true;
        }
    }

    public override void Enable(InputManager manager)
    {
        inputManager = manager;
        active = true;
        cinemachineCamera.Priority = 10;
    }


    public override void Disable()
    {
        active = false;
        newInput = false;
        cinemachineCamera.Priority = 0;
    }

    private void LateUpdate()
    {
        if(!newInput)
        {
            walkVelocity = Vector3.zero;
        }

        transform.position += walkVelocity * Time.deltaTime * speed;
        newInput = false;
    }
}
