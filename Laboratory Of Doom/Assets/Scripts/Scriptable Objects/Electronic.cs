using UnityEngine;

[CreateAssetMenu(fileName = "New Electronic", menuName = "Inventory/Electronic")]
public class Electronic : Item
{
	[Header("Battery"), Space]
	[Range(0f, 100f)] public float maxBatteryLife;
	[Range(0f, 100f)] public float currentBatteryLife;
	[Tooltip("Amount / sec")] public float batteryDepleteRate;

	public bool IsTurnedOn { get; set; }
	public bool OutOfBattery => currentBatteryLife <= 0f;

	// Private fields.

	public void UpdateBattery(float deltaTime)
	{
		if (currentBatteryLife > 0f)
		{
			currentBatteryLife -= batteryDepleteRate * deltaTime;
			currentBatteryLife = Mathf.Max(currentBatteryLife, 0f);
			Inventory.Instance.UpdateItemTooltip(slotIndex);
		}
	}

	public override string ToString()
	{
		return base.ToString() + "\n" +
				$"<b> Remaining battery: {currentBatteryLife: 0.0}% </b>\n" +
				$"<b> Press {InputManager.Instance.GetDisplayString(KeybindingActions.Flashlight)} to turn ON / OFF. </b>";
	}
}
