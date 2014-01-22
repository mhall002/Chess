using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;

public class GUIScript : MonoBehaviour {

    public int CPUSearchDepth = 4;
    public int CPUNodesSearched = 0;
    public float CPUGenerateTime = 0;
    public int TransAlphaHits = 0;
    public int TransBetaHits = 0;
    public int TransExactHits = 0;
    public int TransCollisions = 0;
    public int Alpha = 0;
    public int Beta = 0;

    public Controller Controller;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

    void OnGUI() 
    {
        float width = 200;
        float x = Screen.width - width - 40;
        float y = 10;
        GUI.Box(new Rect(x, y, width + 20, 300), "AI Controls");

        y = y+30;
        if (GUI.Button(new Rect(x + 10, y, width, 20), "Perform Turn"))
        {
            Controller.PerformCPUMove();
        }

        y = y+30;
        GUI.Label(new Rect(x + 10, y, width, 20), "CPU Search Depth: " + CPUSearchDepth);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "CPU Nodes Searched: " + CPUNodesSearched);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "CPU Search Time: " + CPUGenerateTime);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "Alpha hits: " + TransAlphaHits);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "Beta hits: " + TransBetaHits);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "Exact hits: " + TransExactHits);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "Collisions: " + TransCollisions);

        y = y + 30;
        GUI.Label(new Rect(x + 10, y, width, 20), "Alpha - Beta: " + Alpha + " - " +  Beta);
    }
}
