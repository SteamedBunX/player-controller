using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour {
  public CharacterID characterID;
  CharacterActionScript playerCharacter;
  public CharacterController playerRigidBody;
  public Transform weaponAnchor;
  Timer timer;

  PlayerInput inputControl;

  public LayerMask oneWayPlatformLayer;
  public LayerMask groundLayer;
  int groundableLayerMask;

  public Animator animator;

  void Awake() {
    timer = new Timer();
    switch (characterID) {
      case CharacterID.X:
        playerCharacter = new XActionScript();
        break;
      default:
        playerCharacter = new XActionScript();
        break;
    }
  }

  void Start() {
    groundableLayerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("OneWayPlatform");
    playerCharacter.Setup(this);
    setupInputControl();
  }

  void setupInputControl() {
    inputControl = new PlayerInput();
    inputControl.Combat.Enable();
    inputControl.Combat.JumpInput.performed += JumpInput;
    inputControl.Combat.Down.performed += DownInput;
    inputControl.Combat.Left.performed += LeftInput;
    inputControl.Combat.Right.performed += RightInput;
  }

  void Update() {
    playerCharacter.Update();
  }

  void JumpInput(InputAction.CallbackContext context) {
    playerCharacter.JumpInput();
  }

  void DownInput(InputAction.CallbackContext context) {
    playerCharacter.DownInput();
  }

  void LeftInput(InputAction.CallbackContext context) {
    if (context.performed) {
      timer.storeTime("LastRightTap", 0f);
      if (timer.getTimeSince("LastLeftTap") < 0.2f) {
        DashInput();
        timer.storeTime("LastLeftTap", 0f);
      } else {
        timer.storeTime("LastLeftTap", Time.time);
      }
      playerCharacter.LeftTap(ButtonState.PERFORMED);
    }
  }

  void RightInput(InputAction.CallbackContext context) {
    if (context.performed) {
      timer.storeTime("LastLeftTap", 0f);
      if (timer.getTimeSince("LastRightTap") < 0.2f) {
        DashInput();
        timer.storeTime("LastRightTap", 0f);
      } else {
        timer.storeTime("LastRightTap", Time.time);
      }
      playerCharacter.RightTap(ButtonState.PERFORMED);
    }
  }

  void DashInput(InputAction.CallbackContext? context = null) {
    playerCharacter.DashInput();
  }

  bool isOnOneWayPlatform(out Collider oneWayPlatformCollider) {
    RaycastHit groundHitInfo;
    Vector3 groundCheckOrigin = this.transform.position +
      Vector3.up * (playerRigidBody.radius);
    Physics.SphereCast(groundCheckOrigin, playerRigidBody.radius,
    Vector3.down, out groundHitInfo,
         0.1f, oneWayPlatformLayer);
    oneWayPlatformCollider = groundHitInfo.collider;
    return groundHitInfo.collider;
  }

  public bool isHeldDown(GameButton button) {
    switch (button) {
      case GameButton.LEFT:
        return inputControl.Combat.Left.ReadValue<float>() > 0.1f;
      case GameButton.RIGHT:
        return inputControl.Combat.Right.ReadValue<float>() > 0.1f;
      case GameButton.DOWN:
        return inputControl.Combat.Down.ReadValue<float>() > 0.3f;
      default:
        return false;
    }
  }

  void OnDrawGizmos() {
    if (playerCharacter != null) {
      playerCharacter.OnDrawGizmos();
    }
  }
}

public enum CharacterID {
  X,
}

public enum ButtonState {
  STARTED,
  PERFORMED,
  CANCELED,
}

public enum GameButton {
  LEFT,
  RIGHT,
  DOWN,
}
