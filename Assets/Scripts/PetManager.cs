using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utilities.Async.AwaitYieldInstructions;

public class PetManager : NetworkBehaviour
{
    //public static PetManager instance;
    private readonly Dictionary<int, Action> responseActions = new Dictionary<int, Action> {
         //1. Idle, 2.Happy, 3.Upset, 4.Angry, 5.Shock, 6.Dance, 7.Clapping
        //{1, () => blendTreeAIController.PositiveReaction()},
        
    };

    [SerializeField]
    private GameObject userPosition;

    [SerializeField]
    private SpeechRecognition speechRecognition;

    private int NetworkID;

    [SerializeField]
    private TMPro.TMP_Text textDisplay;

    [SerializeField]
    private Button summarizeButton;

    //public SendTextToAnalyse sentimentAnalysis;

    // [SerializeField]
    // private BlendTreeAIController blendTreeAIController;
    // [SerializeField]
    // private NavMeshAIController navMeshAIController;

    void Awake()
    {
    //    if (instance == null)
    //    {
    //        instance = this;
    //    }
    //    else if (instance != this)
    //    {
    //        Destroy(gameObject);
    //    } 
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkID = (int)NetworkManager.Singleton.LocalClientId;
        if (summarizeButton)
        {
            summarizeButton.onClick.AddListener(() => {
                LLManager.Instance.ConcludeMessage();
            });
        }
    }

    // Update is called once per frame
    void Update()
    {
        //press F to trigger the button action
        if (Input.GetKeyDown(KeyCode.F) && speechRecognition != null)
        {
            speechRecognition.ButtonClick();
        }
    }

    public void ButtonClick()
    {
        //Debug.Log("Button Clicked");
        speechRecognition.ButtonClick();
    }

    

    public void SendTextToAnalyse(string text)
    {
        //Debug.Log(IsLocalPlayer);
        if (IsLocalPlayer) {
            SubmitTextServerRpc(text);
        }
    }//client send each input to the server

    [ServerRpc]
    public void SubmitTextServerRpc(string text)
    {
        text = "Speaker " + NetworkManager.Singleton.LocalClientId + ": " + text;
        //Debug.Log("Text Submitted: " + text);
        LLManager.Instance.AddMessage(text);
    }

    [ServerRpc]
    public void SummaryTextServerRpc(string text)
    {
        //Debug.Log("Text Submitted: " + text);
        if (textDisplay) textDisplay.text = text;
        BroadCastSummaryClientRpc(text);
    }

    [ClientRpc]
    private void BroadCastSummaryClientRpc(string text)
    {
        if (textDisplay) textDisplay.text = text;
    }


    [ServerRpc]
    public void AnalyzeAnimationServerRpc(string text)
    {
        Debug.Log("Analyzing Text: " + text);
        if (int.TryParse(text[0].ToString(), out int response))
        {
            TriggerResponse(response);
            BroadcastAnimationClientRpc(response);
        }
        else
        {
            Debug.LogError("Invalid response format.");
        }
    }

    [ClientRpc]
    private void BroadcastAnimationClientRpc(int response)
    {
        TriggerResponse(response);
    }

    public void TriggerResponse(int response)
    {
        //1. Idle, 2.Happy, 3.Upset, 4.Angry, 5.Shock, 6.Dance, 7.Clapping
        if (responseActions.TryGetValue(response, out var action))
        {
            action.Invoke();
        }
        else
        {
            Debug.Log("Invalid Response");
        }
    }

    // public void PredictSentiment(string text)
    // {
    //     sentimentAnalysis.input = text;
    //     //sentimentAnalysis.SendPredictionText();
        
    // }

    // public void triggerResponse(int response) {
    //     switch (response)
    //     {
    //         case 1:
    //             Debug.Log("Positive Response");//good animation
    //             blendTreeAIController.PositiveReaction();
    //             //play animation
    //             break;
    //         case 2:
    //             Debug.Log("Neutral Response");//walking
    //             navMeshAIController.destination = userPosition.transform;
    //             break;
    //         case 3:
    //             Debug.Log("Negative Response");//bad animation
    //             blendTreeAIController.NegativeReaction();
    //             //play animation
    //             break;
    //         default:
    //             Debug.Log("Invalid Response");
    //             break;
    //     }
    // }
}
