using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {

	GameScene gameScene;

	// Use this for initialization
	void Start () {

		gameScene = new GameScene();
		gameScene.Init();
	
	}
	
	// Update is called once per frame
	void Update () {

		gameScene.Update();
	
	}
}
