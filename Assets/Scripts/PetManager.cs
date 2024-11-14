using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class PetManager : MonoBehaviour
{
    public static PetManager instance;

    [SerializeField]
    private GameObject userPosition;

    [SerializeField]
    private SpeechRecognition speechRecognition;

    public SendTextToAnalyse sentimentAnalysis;

    [SerializeField]
    private AIBlendTreeController aiBlendTreeController;
    [SerializeField]
    private AINavigationController aiNavigationController;

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
    }

    public void PredictSentiment(string text)
    {
        sentimentAnalysis.SendPredictionText(text);
    }

    public void triggerResponse(int response) {
        switch (response)
        {
            case 1:
                Debug.Log("Positive Response");//good animation
                aiBlendTreeController.resetVelocity();
                //play animation
                break;
            case 2:
                Debug.Log("Neutral Response");//walking
                aiNavigationController.destination = userPosition.transform;
                break;
            case 3:
                Debug.Log("Negative Response");//bad animation
                aiBlendTreeController.resetVelocity();
                //play animation
                break;
            default:
                Debug.Log("Invalid Response");
                break;
        }
    }
}
