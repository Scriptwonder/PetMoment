using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PetManager : MonoBehaviour
{
    public static PetManager instance;

    [SerializeField]
    private GameObject userPosition;

    [SerializeField]
    private SpeechRecognition speechRecognition;

    public SendTextToAnalyse sentimentAnalysis;

    [SerializeField]
    private BlendTreeAIController blendTreeAIController;
    [SerializeField]
    private NavMeshAIController navMeshAIController;

    [SerializeField]
    private Button button;

    void Awake()
    {
       if (instance == null)
       {
           instance = this;
       }
       else if (instance != this)
       {
           Destroy(gameObject);
       } 
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //press F to trigger the button action
        if (Input.GetKeyDown(KeyCode.F) && speechRecognition != null)
        {
            speechRecognition.ButtonClick();
        }

        if (Input.GetKeyDown(KeyCode.P)) {
            button.onClick.Invoke();
        }
    }

    public void PredictSentiment(string text)
    {
        sentimentAnalysis.input = text;
        //sentimentAnalysis.SendPredictionText();
        
    }

    public void triggerResponse(int response) {
        switch (response)
        {
            case 1:
                Debug.Log("Positive Response");//good animation
                blendTreeAIController.PositiveReaction();
                //play animation
                break;
            case 2:
                Debug.Log("Neutral Response");//walking
                navMeshAIController.destination = userPosition.transform;
                break;
            case 3:
                Debug.Log("Negative Response");//bad animation
                blendTreeAIController.NegativeReaction();
                //play animation
                break;
            default:
                Debug.Log("Invalid Response");
                break;
        }
    }
}
