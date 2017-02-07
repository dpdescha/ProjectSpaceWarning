using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.Networking;


public class InputMessage : MessageBase
{
    public StateMachine.Input input;
    public int frameNumber;
    public static short msgType = MsgType.Highest + 1;
}

public class NetworkMessageManager : NetworkBehaviour
{
    public override void OnStartServer()
    {
        base.OnStartServer();
        NetworkServer.RegisterHandler(InputMessage.msgType, OnServerReceiveInputMessage);
    }

    void OnConnectedToServer()
    {
        NetworkManager.singleton.client.connection.RegisterHandler(InputMessage.msgType, OnClientReceiveInputMessage);
    }

    /// <summary>
    /// called on client when InputMessage received (from opponent via server)
    /// adds the input to the opponent's history at the specified frame
    /// triggers rollback if mismatch with prediction
    /// </summary>
    /// <param name="message"></param>
    void OnClientReceiveInputMessage(NetworkMessage message)
    {
        // get opponent-controlled playerobject
        // call StateMachineInputManager.ReceiveRemoteInput with message contents
    }

    /// <summary>
    /// sends an InputMessage from client to server. Will be forwarded to other clients.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="frameNumber"></param>
    public void ClientSendInputMessage(StateMachine.Input input, int frameNumber)
    {
        InputMessage message = new InputMessage();
        message.input = input;
        message.frameNumber = frameNumber;

        NetworkManager.singleton.client.connection.Send(InputMessage.msgType, message);
    }

    /// <summary>
    /// called on server when InputMessage received
    /// forwards a copy of the message to everyone but sender
    /// </summary>
    /// <param name="message"></param>
    void OnServerReceiveInputMessage(NetworkMessage message)
    {
        InputMessage inputMessage = message.ReadMessage<InputMessage>();
        ServerForwardInputMessage(message.conn.connectionId, inputMessage.input, inputMessage.frameNumber);
    }

    /// <summary>
    /// forwards a received InputMessage to all connected clients except the original sender
    /// </summary>
    /// <param name="fromConnectionId"></param>
    /// <param name="input"></param>
    /// <param name="frameNumber"></param>
    void ServerForwardInputMessage(int fromConnectionId, StateMachine.Input input, int frameNumber)
    {
        InputMessage message = new InputMessage();
        message.input = input;
        message.frameNumber = frameNumber;

        // send to all other clients (should just be one)
        foreach (NetworkConnection connection in NetworkServer.connections)
        {
            if (connection.connectionId != fromConnectionId)
            {
                NetworkServer.SendToClient(connection.connectionId, InputMessage.msgType, message);
            }
        }
    }
}

