using UnityEngine;
using System.Collections;

public class GameStart : MonoBehaviour {

	GameScene gameScene;

	// Use this for initialization
	void Start () {

		gameScene = new GameScene();
		gameScene.Init();

		ConfigManager.Instance.GetCommmonValue("111");
	
	}
	
	// Update is called once per frame
	void Update () {

		gameScene.Update();
	
	}
}
