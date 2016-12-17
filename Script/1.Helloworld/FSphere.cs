using UnityEngine;
using MobaGame.FixedMath;
using MobaGame.Collision;

public class FSphere : MonoBehaviour {

    CollisionObject collisionObject;
	SphereShape sphere;

	// Use this for initialization
	void Start ()
    {
        sphere = new SphereShape(VFixedPoint.One);
		collisionObject = new CollisionObject();
		collisionObject.setCollisionFlags(MobaGame.Collision.CollisionFlags.NORMAL_OBJECT);
		collisionObject.setCollisionShape(sphere);
		GameObject.Find("FEngine").GetComponent<Mainloop>().engine.GetCollisionWorld().addCollisionObject(collisionObject);
		collisionObject.setWorldTransform(new VIntTransform(transform));
    }
	
	// Update is called once per frame
	void Update ()
    {
		collisionObject.setWorldTransform(new VIntTransform(transform));
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere (transform.position, 1);//sphere.getRadius().ToFloat);
	}
}
