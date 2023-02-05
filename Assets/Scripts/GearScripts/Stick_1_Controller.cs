using UnityEngine;

public class Stick_1_Controller : MonoBehaviour, BasicWeaponScript {
  bool expanded = false;
  public Animator animator;
  string _currentAnimation = Stick_1_Animations.WEAPON_IDLE_RETRIEVED;
  string currentAnimation {
    get { return _currentAnimation; }
    set {
      if (value != _currentAnimation) {
        _currentAnimation = value;
        animator.Play(value, 0, 0.1f);
      }
    }
  }

  public void start() { }

  public void update() { }

  public void changeStance(string stance) {
    switch (stance) {
      case "EXPAND":
        currentAnimation = Stick_1_Animations.WEAPON_EXPAND;
        break;
      case "RETRIEVE":
        currentAnimation = Stick_1_Animations.WEAPON_RETRIEVE;
        break;
    }
  }

  public class Stick_1_Animations {
    public const string WEAPON_IDLE_RETRIEVED = "Weapon Idle Retrieved";
    public const string WEAPON_EXPAND = "Weapon Expand";
    public const string WEAPON_RETRIEVE = "Weapon Retrieve";
  }

}
