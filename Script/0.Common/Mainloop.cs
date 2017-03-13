using UnityEngine;
using System.Collections.Generic;
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

    private void OnDrawGizmos()
    {
        if (engine == null)
            return;

        List<BroadphasePair> manifolds = engine.getOverlappingPairCache().getOverlappingPairArray();
        for(int i = 0; i < manifolds.Count; i++)
        {
            PersistentManifold manifold = manifolds[i].manifold;
            for(int j = 0; j < manifold.getContactPointsNum(); j++)
            {
                ManifoldPoint apoint = manifold.getManifoldPoint(j);
                Gizmos.DrawSphere(apoint.positionWorldOnA.Vec3, 0.05f);
                Gizmos.DrawSphere(apoint.positionWorldOnB.Vec3, 0.05f);
            }
        }
    }
}
