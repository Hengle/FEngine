using UnityEngine;
using MobaGame.Collision;
using MobaGame.FixedMath;

public class Mainloop : MonoBehaviour {

    public FEngine engine;
    public VFixedPoint dt;

	// Use this for initialization
	void Awake () {
        engine = new FEngine();
        dt = VFixedPoint.One / VFixedPoint.Create(20);
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        engine.Tick(dt);
	}
}
