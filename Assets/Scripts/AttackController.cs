using UnityEngine;

public class AttackController : MonoBehaviour {

    public enum WeaponState {
        BLADE,
        CROSSBOW
    }

    public WeaponState weaponState = WeaponState.BLADE;
    public bool attacks;

	void Start() {
		
	}
	

	void FixedUpdate() {
        handleInput();
	}

    private void handleInput() {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            weaponState = WeaponState.BLADE;
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            weaponState = WeaponState.CROSSBOW;
        else if (Input.GetKeyDown(KeyCode.F))
            attacks = true;
    }
}
