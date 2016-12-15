using UnityEngine;
using MobaGame.FixedMath;
using MobaGame.Collision;

public class FBox : MonoBehaviour
{

    CollisionObject _box;


    // Use this for initialization
    void Start()
    {
        CollisionShape box = new BoxShape(VInt3.one);
        _box = new CollisionObject();
        _box.setCollisionFlags(MobaGame.Collision.CollisionFlags.NORMAL_OBJECT);
        _box.setCollisionShape(box);
        GameObject.Find("FEngine").GetComponent<Mainloop>().engine.GetCollisionWorld().addCollisionObject(_box);
        _box.setWorldTransform(new VIntTransform(transform));
    }

    // Update is called once per frame
    void Update()
    {
        _box.setWorldTransform(new VIntTransform(transform));
    }
}
