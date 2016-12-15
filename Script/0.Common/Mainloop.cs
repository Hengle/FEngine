using UnityEngine;
using MobaGame.Collision;

public class Mainloop : MonoBehaviour {

    public FEngine engine;

	// Use this for initialization
	void Awake () {
        engine = new FEngine();
	}
	
	// Update is called once per frame
	void Update () {
        engine.Tick();
	
	}
}
