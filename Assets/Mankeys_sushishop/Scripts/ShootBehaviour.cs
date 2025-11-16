using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShootBehaviour : MonoBehaviour 
{
	// The position of the ammo right before it is shot
	public List<Transform> AmmoPosition;
	// The prefab of the object we are going to shoot
	public List<GameObject> AmmoPrefab;
	// Target to shoot at
	private Vector3 Target;
	// Will the shot have a timer or will it be animation-controlled?
	public bool TimeControlledShoot = false;
	// Shoot interval (when time-controlled)
	public float ShootIntervalTime;
    //
    public float strenght = 1.0f;

    // Elapsed time since the last shot
    private float _elapsedTime;
	// Instantiated ammo
	private List<GameObject> _ammo;
	// Ammo thet we need to destroy
	private List<GameObject> _ammo2Destroy;

	void Start()
	{
		// Is it a time-controlled monkey?
		if (TimeControlledShoot)
		{
			// Initialize the elapsed time
			_elapsedTime = 0.0f;
		}

		// Initialize the lists for future use
		if (_ammo == null)
		{
			_ammo = new List<GameObject>();
			_ammo2Destroy = new List<GameObject>();
		}

       
	}

	void Update()
	{

        
		// Is it a time-controlled shot?
		if (TimeControlledShoot)
		{
			// Update the elapsed time from the last frame
			_elapsedTime += Time.deltaTime;
			// Do we have to shoot this frame?
			if (_elapsedTime >= ShootIntervalTime)
			{
				// Shoot!
				Shoot();
				// Initialize the elapsed time to start over
				_elapsedTime = 0.0f;
			}
		}
	}

	public void Shoot()
	{

        Target = (transform.forward * strenght) + transform.position;

        // Fill the list of objects to be destroyed with the ammo we alredy shot. Needed because objects can't be destroyed inside a foreach block and we need to empty the _ammo list.
        foreach (GameObject ammo in _ammo)
		{
			_ammo2Destroy.Add(ammo);
		}

		// Empty the list (once the components were stored to be destroyed)
		_ammo.Clear();

		// Destroy the objects in the list
		foreach (GameObject ammo in _ammo2Destroy)
		{
			Destroy(ammo);
		}

		// Clear the lists
		_ammo2Destroy.Clear();
		_ammo.Clear();

		// For every projectile shot by the current monkey (some monkeys shoot more than 1 projectile)
		for (int i=0; i<AmmoPrefab.Count; i++)
		{
			// Instantiate the ammo
			GameObject ammo = Instantiate(AmmoPrefab[i]);

			// Set the parent to its position transform
			ammo.transform.parent = AmmoPosition[i];

			// Rest position and rotation
			ammo.transform.localPosition = Vector3.zero;
			ammo.transform.localRotation = Quaternion.identity;

			// Unset the parent (to avoid the animation to affect the physics)
			ammo.transform.parent = null;

			// Reset scale of the projectile to avoid parent's animation deformation
			ammo.transform.localScale = Vector3.one;

			// Add to the list of existing projectiles (to delete it later)
			_ammo.Add(ammo);
		}

		// For each instantiated projectile...
		foreach (GameObject ammo in _ammo)
		{
			// Calculate the force needed to reach the target
			Vector3 force = Target - ammo.transform.position;
            Debug.DrawRay(ammo.transform.position, Target, Color.white, 5f);
			// Random factor to make it go faster
			force *= 2.0f;
			// And apply the force!
			ammo.GetComponent<Rigidbody>().AddForce(force, ForceMode.Impulse);
		}
	}
}
