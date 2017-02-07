using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class StateMachineInputManager : NetworkBehaviour {

    public struct InputFrame
    {
        public StateMachine.Input input;
        public StateMachine.Frame frame;
    }

    NetworkClient m_client;

    public void Init(NetworkClient client)
    {
        m_client = client;
    }

    public KeyCode left;
    public KeyCode right;
    public KeyCode crouch;
    public KeyCode attack;
    public int inputBuffer;
    
    public NetworkMessageManager networkMessageManager;

    private StateMachine stateMachine;
    private InputFrame[] history;
    private int currentFrame = 0;

	// Use this for initialization
	void Start ()
    {
        stateMachine = GetComponent<StateMachine>();
        history = new InputFrame[100*60]; // 100 second max, 60 frames per second
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (isLocalPlayer)
        {
            // get inputs
            StateMachine.Input input;

            int horizontalInput = 0 + (Input.GetKey(right) ? 1 : 0) - (Input.GetKey(left) ? 1 : 0); ;
            if (transform.localScale.x < 0)
            {
                horizontalInput *= -1;
            }
            input.Direction = horizontalInput;

            input.Crouch = Input.GetKey(crouch);

            input.Attack = Input.GetKeyDown(attack);

            history[currentFrame].input = input;

            networkMessageManager.ClientSendInputMessage(input, currentFrame);
        }

        if (currentFrame > inputBuffer)
        {
            history[currentFrame].frame = stateMachine.Advance(history[currentFrame - inputBuffer].input);
        }

        currentFrame++;
    }

    public void ReceiveRemoteInput(StateMachine.Input input, int frameNumber)
    {
        if (history[frameNumber].input != input)
        {
            // input doesn't match prediction, re-simulate
            history[frameNumber].input = input;
            SimulateFromFrame(frameNumber);
        }
        else
        {
            // our prediction is GADLIKE #hardreads
        }
    }

    public void SimulateFromFrame(int frameNumber)
    {
        stateMachine.JumpToFrame(history[(frameNumber == 0 ? 0 : frameNumber - 1)].frame); // reset state machine to last valid state
        for (int i = frameNumber; i <= currentFrame; i++)
        {
            history[i].frame = stateMachine.Advance(history[i].input);
        }
    }

    private void PredictFromLastInput(int lastFrame)
    {
        // TODO: better prediction than just "keep doing what you were doing"
        StateMachine.Input lastFrameInput = history[lastFrame].input;
        for (int i = lastFrame+1; i < 100*60-1; i++)
        {
            history[i].input = lastFrameInput;
        }
    }
}
