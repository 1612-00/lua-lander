using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class Lander : MonoBehaviour {

	private const float GRAVITY_NORMAL = 0.5f;
	private const float THRUST_FORCE = 10f;
	private const float ROTATION_TORQUE = 1.5f;
	private const float MAX_LINEAR_SPEED = 6f;
	private const float MAX_ANGULAR_SPEED = 90f;

	public static Lander Instance { get; private set; }

	public event EventHandler OnLeftForce;
	public event EventHandler OnUpForce;
	public event EventHandler OnRightForce;
	public event EventHandler OnBeforeForce;
	public event EventHandler OnCoinPickup;
	public event EventHandler OnFuelPickup;
	public event EventHandler<OnStateChangedEventArgs> OnStateChanged;
	public class OnStateChangedEventArgs : EventArgs {
		public State state;
	}
	public event EventHandler<OnLandedEventArgs> OnLanded;
	public class OnLandedEventArgs : EventArgs {
		public LandingType landingType;
		public int score;
		public float dotVector;
		public float landingSpeed;
		public float scoreMultiplier;
	}

	public enum LandingType {
		Success,
		WrongLandingArea,
		TooSteepAngle,
		TooFastLanding,
	}

	public enum State {
		WaitingToStart,
		Normal,
		GameOver,
	}


	private Rigidbody2D landerRigidbody2D;

	private float fuelAmount;
	private float fuelAmountMax = 10f;
	private State state;

	private void Awake() {
		Instance = this;
		fuelAmount = fuelAmountMax;
		state = State.WaitingToStart;
		landerRigidbody2D = GetComponent<Rigidbody2D>();
		landerRigidbody2D.gravityScale = 0f;
	}

	private void FixedUpdate() {
		OnBeforeForce?.Invoke(this, EventArgs.Empty);

		switch (state) {
			case State.WaitingToStart:
				if (GameInput.Instance.IsUpActionPressed() || GameInput.Instance.IsLeftActionPressed() || GameInput.Instance.IsRightActionPressed()) {
					landerRigidbody2D.gravityScale = GRAVITY_NORMAL;
					SetState(State.Normal);
				}
				break;
			case State.Normal:
				if (fuelAmount > 0f) {
					if (GameInput.Instance.IsUpActionPressed() || GameInput.Instance.IsLeftActionPressed() || GameInput.Instance.IsRightActionPressed()) {
						ConsumeFuel();
					}
					if (GameInput.Instance.IsUpActionPressed()) {
						landerRigidbody2D.AddForce(THRUST_FORCE * transform.up);
						OnUpForce?.Invoke(this, EventArgs.Empty);
					}
					if (GameInput.Instance.IsLeftActionPressed()) {
						landerRigidbody2D.AddTorque(ROTATION_TORQUE);
						OnLeftForce?.Invoke(this, EventArgs.Empty);
					}
					if (GameInput.Instance.IsRightActionPressed()) {
						landerRigidbody2D.AddTorque(-ROTATION_TORQUE);
						OnRightForce?.Invoke(this, EventArgs.Empty);
					}
				}
				break;
			case State.GameOver:
				break;
			default:
				break;
		}

		landerRigidbody2D.linearVelocity = Vector2.ClampMagnitude(
			landerRigidbody2D.linearVelocity,
			MAX_LINEAR_SPEED);
		landerRigidbody2D.angularVelocity = Mathf.Clamp(
			landerRigidbody2D.angularVelocity,
			-MAX_ANGULAR_SPEED,
			MAX_ANGULAR_SPEED);

	}

	private void OnCollisionEnter2D(Collision2D collision) {
		if (!collision.gameObject.TryGetComponent(out LandingPad landingPad)) {
			OnLanded?.Invoke(this, new OnLandedEventArgs {
				landingType = LandingType.WrongLandingArea,
				score = 0,
				dotVector = 0f,
				landingSpeed = 0f,
				scoreMultiplier = 0
			});
			SetState(State.GameOver);
			return;
		}

		float softLandingVelocityMagnitude = 4f;
		float relativeVelocityMagnitude = collision.relativeVelocity.magnitude;
		if (relativeVelocityMagnitude > softLandingVelocityMagnitude) {
			OnLanded?.Invoke(this, new OnLandedEventArgs {
				landingType = LandingType.TooFastLanding,
				score = 0,
				dotVector = 0f,
				landingSpeed = relativeVelocityMagnitude,
				scoreMultiplier = 0
			});
			SetState(State.GameOver);
			return;
		}

		float dotVector = Vector2.Dot(Vector2.up, transform.up);
		float minDotVector = .9f;
		if (dotVector < minDotVector) {
			OnLanded?.Invoke(this, new OnLandedEventArgs {
				landingType = LandingType.TooSteepAngle,
				score = 0,
				dotVector = dotVector,
				landingSpeed = relativeVelocityMagnitude,
				scoreMultiplier = 0
			});
			SetState(State.GameOver);
			return;
		}
		Debug.Log("Landed successfully!");

		float maxScoreAmountLandingAngle = 100;
		float scoreDotVectorMultiplier = 10f;
		float landingAngleScore = maxScoreAmountLandingAngle - Mathf.Abs(dotVector - 1f) * scoreDotVectorMultiplier * maxScoreAmountLandingAngle;

		float maxScoreAmountLandingSpeed = 100;
		float landingSpeedScore = (softLandingVelocityMagnitude - relativeVelocityMagnitude) * maxScoreAmountLandingSpeed;

		int score = Mathf.RoundToInt((landingAngleScore + landingSpeedScore) * landingPad.GetScoreMultiplier());

		OnLanded?.Invoke(this, new OnLandedEventArgs {
			landingType = LandingType.Success,
			score = score,
			dotVector = dotVector,
			landingSpeed = relativeVelocityMagnitude,
			scoreMultiplier = landingPad.GetScoreMultiplier()
		});
		SetState(State.GameOver);
	}

	private void OnTriggerEnter2D(Collider2D collision) {
		if (collision.gameObject.TryGetComponent(out FuelPickup fuelPickup)) {
			float addFuelAmount = 10f;
			fuelAmount += addFuelAmount;
			if (fuelAmount > fuelAmountMax) {
				fuelAmount = fuelAmountMax;
			}
			OnFuelPickup?.Invoke(this, EventArgs.Empty);
			fuelPickup.DestroySelf();
		}

		if (collision.gameObject.TryGetComponent(out CoinPickup coinPickup)) {
			OnCoinPickup?.Invoke(this, EventArgs.Empty);
			coinPickup.DestroySelf();
		}
		else {
			Debug.Log("Entered Landing Pad Trigger!");
		}
	}

	private void SetState(State newState) {
		state = newState;
		OnStateChanged?.Invoke(this, new OnStateChangedEventArgs { state = newState });
	}

	private void ConsumeFuel() {
		const float fuelConsumptionRate = 1f;
		fuelAmount -= fuelConsumptionRate * Time.fixedDeltaTime;
		Debug.Log($"Fuel Amount: {fuelAmount}");
	}

	public float GetFuelAmount() {
		return fuelAmount;
	}

	public float GetFuelAmountNomalized() {
		return fuelAmount / fuelAmountMax;
	}

	public float GetSpeedX() {
		return landerRigidbody2D.linearVelocityX;
	}

	public float GetSpeedY() {
		return landerRigidbody2D.linearVelocityY;
	}
}
