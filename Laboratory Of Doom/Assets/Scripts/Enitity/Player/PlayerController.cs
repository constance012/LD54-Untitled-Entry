using UnityEngine;

public class PlayerController : MonoBehaviour
{
	[Header("References"), Space]
	[SerializeField] private Rigidbody2D rb2D;
	[SerializeField] private Animator animator;

	[Header("Movement Settings"), Space]
	[SerializeField] private float maxSpeed;
	[SerializeField] private float acceleration;
	[SerializeField] private float deceleration;

	public static Vector2 Position { get; private set; }

	// Private fields.
	private Vector2 _movementDirection;
	private Vector2 _previousDirection;
	private float _currentSpeed;

	private void Update()
	{
		_movementDirection = InputManager.Instance.Read2DVector(KeybindingActions.MoveLeft);

		if (_movementDirection.sqrMagnitude > .01f)
			_previousDirection = _movementDirection;
	}
	
	private void FixedUpdate()
	{
		if (GameManager.Instance.GameDone)
			return;

		UpdateVelocity();

		Position = rb2D.position;
	}

	private void UpdateVelocity()
	{
		animator.SetFloat("Speed", rb2D.velocity.sqrMagnitude);

		if (_movementDirection.sqrMagnitude > .01f)
		{
			_currentSpeed += acceleration * Time.deltaTime;
			_currentSpeed = Mathf.Min(maxSpeed, _currentSpeed);
			
			rb2D.velocity = _movementDirection * _currentSpeed;
		}

		else if (_currentSpeed > 0f)
		{
			_currentSpeed -= deceleration * Time.deltaTime;
			_currentSpeed = Mathf.Max(0f, _currentSpeed);

			rb2D.velocity = _previousDirection * _currentSpeed;
		}
	}
}
