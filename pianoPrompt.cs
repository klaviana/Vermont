using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class pianoPrompt : MonoBehaviour
{

    private Transform playerTransform;
    private Transform myTransform;
    private float dist;
    private float distanceTravelled;
    private bool hideGUIs = false;

    // Use this for initialization
    void Start () {
        // find the player object - just do this once when we start up - assumes you've Tagged your player object "Player"
        GameObject playerOb = GameObject.FindWithTag("Player");
        if (playerOb != null)
        {
            playerTransform = playerOb.transform;
        }
        else
        {
            Debug.LogWarning("Could not find player object!  Turning off!");
            this.enabled = false;
        }
        myTransform = this.transform;
    }
	
	// Update is called once per frame
	void Update () {
        dist = Vector3.Distance(playerTransform.position, myTransform.position);
        //print("Distance = " + dist);
        distanceTravelled += Vector3.Distance(playerTransform.position, myTransform.position);
        //print("DistanceTravelled = " + distanceTravelled);
        if (distanceTravelled > 500) {
            hideGUIs = false;
        }
    }

    void OnGUI()
    {
        // Create style for a button
        GUIStyle myStyle = new GUIStyle(GUI.skin.button);
        myStyle.fontSize = 30;
        myStyle.wordWrap = true;
        myStyle.normal.textColor = Color.yellow;
        myStyle.hover.textColor = Color.yellow;
        GUIStyle buttonStyle = myStyle;

        // Control appearance of prompts
        if (dist < 3 & hideGUIs == false)
        {
            GUI.Box(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 300, 400, 200), 
            "Click on the chair to have a seat at the piano", myStyle);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Button Clicked");
        Initiate.Fade("MidiVisualizer", Color.white, 0.75f);
    }

}
