using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : Seeker
{
	private static HashSet<Rigidbody2D> alertedEnemies;

	[Header("References"), Space]
	[SerializeField] private Rigidbody2D rb2D;
	[SerializeField] private Animator animator;
	[SerializeField] private Transform pivotPoint;

	[Header("Spotting Settings")]
	[SerializeField] private float aggroRange;
	[SerializeField] private float spotTimer;
	[SerializeField] private LayerMask obstacleLayers;

	[Header("Repel Settings"), Space]
	[SerializeField] private float repelRange;
	[SerializeField] private float repelAmplitude;

	// Private fields.
	private Vector2 _targetPreviousPos;

	private bool _facingRight = true;
	private bool _spottedPlayer;
	private float _spotTimer;

	private void Start()
	{
		alertedEnemies ??= new HashSet<Rigidbody2D>();

		_targetPreviousPos = PlayerController.Position;
		_spotTimer = spotTimer;
	}

	private void OnDestroy()
	{
		alertedEnemies.Remove(rb2D);
	}

	private void FixedUpdate()
	{
		if (PlayerStats.IsDeath)
			return;
		
		if (!_spottedPlayer)
		{
			float distanceToPlayer = Vector2.Distance(rb2D.position, PlayerController.Position);
			
			if (distanceToPlayer <= aggroRange)
			{
				Ray2D visionLine = new Ray2D(rb2D.position, (PlayerController.Position - rb2D.position).normalized);
				
				if (!Physics2DExtensions.Raycast(visionLine, out RaycastHit2D _, distanceToPlayer, obstacleLayers))
				{
					_spotTimer -= Time.deltaTime;
					
					if (_spotTimer <= 0)
						Alert();
				}
				else
					_spotTimer = spotTimer;
			}
			else
				_spotTimer = spotTimer;

			return;
		}

		animator.SetFloat("Speed", rb2D.velocity.sqrMagnitude);

		// Request a path if the player has moved a certain distance fron the last position.
		if (Vector2.Distance(PlayerController.Position, _targetPreviousPos) >= maxMovementDelta)
		{
			PathRequester.Request(new PathRequestData(pivotPoint.position, PlayerController.Position, this.gameObject, OnPathFound));
			_targetPreviousPos = PlayerController.Position;
		}
	}

	public void Alert()
	{
		_spottedPlayer = true;
		_spotTimer = 0f;

		// Add this enemy to the hash set.
		alertedEnemies.Add(rb2D);
	}

	protected override IEnumerator FollowPath(int previousIndex = -1)
	{
		if (_path.Length == 0)
			yield break;

		Vector3 currentWaypoint = previousIndex == -1 ? _path[0] : _path[previousIndex];

		Debug.Log($"{gameObject.name} following path...");

		while (true)
		{
			float distanceToCurrent = Vector3.Distance(pivotPoint.position, currentWaypoint);

			if (distanceToCurrent < .05f)
			{
				_waypointIndex++;

				// If there's no more waypoints to move, then simply returns out of the coroutine.
				if (_waypointIndex >= _path.Length)
				{
					_waypointIndex = 0;
					_path = new Vector3[0];
					yield break;
				}

				currentWaypoint = _path[_waypointIndex];
			}

			Vector2 direction = (currentWaypoint - pivotPoint.position).normalized;
			Vector2 velocity = CalculateVelocity(direction);

			CheckFlip();

			rb2D.velocity = velocity;

			yield return new WaitForFixedUpdate();
		}
	}

	private void CheckFlip()
	{
		float sign = Mathf.Sign(PlayerController.Position.x -  rb2D.position.x);
		bool mustFlip = (_facingRight && sign < 0f) || (!_facingRight && sign > 0f);

		if (mustFlip)
		{
			transform.Rotate(Vector3.up * 180f);
			_facingRight = !_facingRight;
		}
	}

	/// <summary>
	/// Calculate the final velocity after applying repel force against other enemies nearby.
	/// </summary>
	/// <param name="direction"></param>
	/// <returns></returns>
	private Vector2 CalculateVelocity(Vector2 direction)
	{
		// Enemies will try to avoid each other.
		Vector2 repelForce = Vector2.zero;

		foreach (Rigidbody2D enemy in alertedEnemies)
		{
			if (enemy == rb2D)
				continue;

			if (Vector2.Distance(enemy.position, rb2D.position) <= repelRange)
			{
				Vector2 repelDirection = (rb2D.position - enemy.position).normalized;
				repelForce += repelDirection;
			}
		}

		Vector2 velocity = direction * moveSpeed;
		velocity += repelForce.normalized * repelAmplitude;

		return velocity;
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, repelRange);

		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, aggroRange);
	}
}
