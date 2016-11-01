using MobaGame.FixedMath;

public class World: Shiftable
{
	/** Earths gravity constant */
	public static readonly VInt3 EARTH_GRAVITY = VInt3.up * -10;
	
	/** Zero gravity constant */
	public static readonly VInt3 ZERO_GRAVITY = VInt3.zero;

	/** The world id */
	protected readonly UUID id = UUID.GetNextUUID();


	/** The world gravity vector */
	protected Vector2 gravity;

	/** The {@link BroadphaseDetector} */
	protected BroadphaseDetector<Body, BodyFixture> broadphaseDetector;
	
	/** The {@link BroadphaseFilter} for detection */
	protected BroadphaseFilter<Body, BodyFixture> detectBroadphaseFilter;
	
	/** The {@link NarrowphaseDetector} */
	protected NarrowphaseDetector narrowphaseDetector;
	
	/** The {@link NarrowphasePostProcessor} */
	protected NarrowphasePostProcessor narrowphasePostProcessor;

	/** The {@link RaycastDetector} */
	protected RaycastDetector raycastDetector;

	/** The {@link ContactManager} */
	protected ContactManager contactManager;

	/** The {@link Body} list */
	private final List<Body> bodies;
	
	/** The {@link Joint} list */
	private final List<Joint> joints;


	/** The reusable island */
	private Island island;
	
	/** The accumulated time */
	private VFixedPoint time;
	
	/** Flag to find new contacts */
	private boolean updateRequired;
}