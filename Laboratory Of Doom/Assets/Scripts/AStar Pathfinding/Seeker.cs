using System.Collections;
using UnityEngine;

public class Seeker : MonoBehaviour
{
	[Header("Debug-only"), Space]
	[SerializeField] private bool seekOnStart;

	[Header("Target Transform"), Space]
	public Transform target;
	[SerializeField] protected float maxMovementDelta;

	[Header("Movement Settings"), Space]
	[SerializeField] protected float moveSpeed;

	// Private fields.
	protected Vector3[] _path;
	protected Coroutine _followCoroutine;
	protected int _waypointIndex;
	protected bool _finishedFollowingPath;

	private IEnumerator Start()
	{
		if (seekOnStart && target != null)
		{
			yield return new WaitForSeconds(.1f);
			PathRequester.Request(new PathRequestData(transform.position, target.position, this.gameObject, OnPathFound));
		}
	}

	private void OnEnable()
	{
		// Resume the previous coroutine if it hasn't finished yet.
		if (!_finishedFollowingPath && _followCoroutine != null)
			_followCoroutine = StartCoroutine(FollowPath(_waypointIndex));
	}

	protected void OnPathFound(Vector3[] newPath, bool pathFound)
	{
		// Only start following the found path if this gameobject has not been destroyed yet.
		if (pathFound && gameObject != null)
		{
			_path = newPath;
			_waypointIndex = 0;

			if (_followCoroutine != null)
				StopCoroutine(_followCoroutine);
			
			_followCoroutine = StartCoroutine(FollowPath());
		}
	}

	protected virtual IEnumerator FollowPath(int previousIndex = -1)
	{
		Vector3 currentWaypoint = previousIndex == -1 ? _path[0] : _path[previousIndex];
		_finishedFollowingPath = false;

		Debug.Log("Following path...");

		while (true)
		{
			if (transform.position == currentWaypoint)
			{
				_waypointIndex++;

				// If there's no more waypoints to move, then simply returns out of the coroutine.
				if (_waypointIndex >= _path.Length)
				{
					_waypointIndex = 0;
					_path = new Vector3[0];

					_finishedFollowingPath = true;
					yield break;
				}

				currentWaypoint = _path[_waypointIndex];
			}

			// Move this seeker closer to the current waypoint.
			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, moveSpeed * Time.deltaTime);
			yield return null;
		}
	}

	private void OnDrawGizmos()
	{
		if (_path != null)
		{	
			for (int i = _waypointIndex; i < _path.Length; i++)
			{
				Gizmos.color = Color.green;
				Gizmos.DrawCube(_path[i], Vector3.one);

				Gizmos.color = Color.black;
				if (i == _waypointIndex)
					Gizmos.DrawLine(transform.position, _path[i]);
				else
					Gizmos.DrawLine(_path[i - 1], _path[i]);
			}
		}
	}
}
