using UnityEngine;
using MobaGame.FixedMath;
using MobaGame.Collision;

public class FSphere : MonoBehaviour {

    CollisionObject _sphere;


	// Use this for initialization
	void Start ()
    {
        CollisionShape sphere = new SphereShape(VFixedPoint.One);
        _sphere = new CollisionObject();
        _sphere.setCollisionFlags(MobaGame.Collision.CollisionFlags.NORMAL_OBJECT);
        _sphere.setCollisionShape(sphere);
        GameObject.Find("FEngine").GetComponent<Mainloop>().engine.GetCollisionWorld().addCollisionObject(_sphere);
        _sphere.setWorldTransform(new VIntTransform(transform));
    }
	
	// Update is called once per frame
	void Update ()
    {
        _sphere.setWorldTransform(new VIntTransform(transform));
	}
}
