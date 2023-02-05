using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XActionScript : CharacterActionScript {
  public CharacterController rigidBody;
  public Transform weaponAnchor;
  public BasicWeaponScript weapon;

  public LayerMask groundLayer;
  public LayerMask oneWayPlatformLayer;
  int groundLayerMask;
  PlayerController playerController;
  Animator animator;
  Transform mainTransform;
  List<int> currentedlyIgnoredPlatform = new List<int>();
  bool jumpingUp { get { return bmc.velocityY > 0; } }

  // Objects
  Timer timer = new Timer();
  // Variables
  float walkSpeed = 3f;
  float dashSpeed = 10f;
  float runSpeed = 7f;
  float jumpPower = 12f;
  // Flag
  CharacterAction currentAction = CharacterAction.IDLE;
  // Movement Flags
  bool newAction = false;
  float dashCD = 0.4f;
  float currentActionDirection = 0;
  bool dashedInAir = false;
  public bool movingUp { get; set; }
  MovementController bmc;
  string _currentAnimation = XAnimations.STANDING_STILL;
  string currentAnimation {
    get { return _currentAnimation; }
    set {
      if (value != _currentAnimation) {
        _currentAnimation = value;
        animator.Play(value, 0, 0.1f);
      }
    }
  }
  Direction _facing = Direction.RIGHT;
  Direction facing {
    get {
      return _facing;
    }
    set {
      if (_facing != value) {
        _facing = value;
        updateFacingDirection();
      }
    }
  }

  void updateFacingDirection() {
    mainTransform.localScale = new Vector3(facing == Direction.RIGHT ? 1 : -1, mainTransform.localScale.y, mainTransform.localScale.z);
  }

  public void Setup(PlayerController playerController) {
    this.playerController = playerController;
    groundLayer = playerController.groundLayer;
    oneWayPlatformLayer = playerController.oneWayPlatformLayer;
    groundLayerMask = 1 << LayerMask.NameToLayer("Ground") | 1 << LayerMask.NameToLayer("OneWayPlatform");
    rigidBody = playerController.playerRigidBody;
    animator = playerController.animator;
    mainTransform = playerController.transform;
    weaponAnchor = playerController.weaponAnchor;
    bmc = new MovementController(
        rigidBody,
        gravity: 21f,
        gravityDescendRatio: 2.0f,
        horizontalDecelerationRate: 40f,
        airHorizontalDecelerationRate: 10f);
    loadWeapon();
  }

  public void loadWeapon() {
    GameObject weaponInstance = GameObject.Instantiate(Resources.Load("Prefabs/Weapons/XWeaponStick1") as GameObject);
    weaponInstance.transform.parent = weaponAnchor;
    weaponInstance.transform.localPosition = new Vector3(0, 0, 0);
    weaponInstance.transform.localRotation = Quaternion.identity;
    weapon = weaponInstance.GetComponent<Stick_1_Controller>();
  }

  public void Update() {
    applyAction();
    if (currentAction <= CharacterAction.RUN) {
      DropDownCheck();
    }
    bmc.update(Time.deltaTime, isGrounded());
  }

  void applyAction() {
    if (currentAction == CharacterAction.IDLE) {
      weapon.changeStance("RETRIEVE");
    } else {
      weapon.changeStance("EXPAND");
    }
    switch (currentAction) {
      case CharacterAction.IDLE:

        idle();
        break;
      case CharacterAction.WALK:
        walk();
        break;
      case CharacterAction.DASH:
      case CharacterAction.JUMPDASH:
        dash();
        break;
      case CharacterAction.RUN:
        run();
        break;
    }
  }

  public void LeftTap(ButtonState buttonState) {
    switch (buttonState) {
      case ButtonState.PERFORMED:
        switch (currentAction) {
          case CharacterAction.IDLE:
            idle(InputCommand.LEFTTAP);
            break;
          case CharacterAction.WALK:
            walk(InputCommand.LEFTTAP);
            break;
        }
        break;
    }
  }

  public void RightTap(ButtonState buttonState) {
    switch (buttonState) {
      case ButtonState.PERFORMED:
        switch (currentAction) {
          case CharacterAction.IDLE:
            idle(InputCommand.RIGHTTAP);
            break;
          case CharacterAction.WALK:
            walk(InputCommand.RIGHTTAP);
            break;
        }
        break;
    }
  }

  public void DashInput() {
    switch (currentAction) {
      case CharacterAction.IDLE:
      case CharacterAction.WALK:
        if (timer.getTimeSince("LastDash") > 0.4f) {
          beginAction(CharacterAction.DASH);
        }
        break;
    }
  }

  public void JumpInput() {
    tryJump();
  }

  void tryJump() {
    if (isGrounded()) {
      switch (currentAction) {
        case CharacterAction.IDLE:
        case CharacterAction.WALK:
        case CharacterAction.RUN:
        case CharacterAction.DASH:
          beginAction(CharacterAction.IDLE);
          bmc.setYVelocity(jumpPower);
          break;
      }
    }
  }

  public void DownInput() {
    timer.storeTime("LastDownInput", Time.time);
  }

  public void DropDownCheck() {
    if (isHeldDown(GameButton.DOWN) && bmc.velocityY <= 0) {
      tryDropDown();
    }
  }

  void tryDropDown() {
    Collider oneWayPlatformCollider;
    if (isOnOneWayPlatform(out oneWayPlatformCollider)) {
      Debug.Log("Dropping Down");
      Physics.IgnoreCollision(rigidBody, oneWayPlatformCollider, true);
    }
  }

  public void applyDownwardMomentum() {

  }

  void idle() {
    float horizontal = getHorizontal();
    if (isGrounded()) {
      currentAnimation = XAnimations.STANDING_STILL;
      if (horizontal != 0) {
        beginAction(CharacterAction.WALK);
        updateFacing(horizontal);
      }
    } else {
      if (jumpingUp) {
        currentAnimation = XAnimations.JUMP_UP;
      } else {
        currentAnimation = XAnimations.JUMP_DOWN;
      }
      if (horizontal != 0) {
        bmc.setXVelocity(walkSpeed * horizontal);
        updateFacing(horizontal);
      }
    }
  }

  void idle(InputCommand inputCommand) {
    if (isGrounded()) {
      switch (inputCommand) {
        case InputCommand.LEFTTAP:
        case InputCommand.RIGHTTAP:
          facing = inputCommand == InputCommand.LEFTTAP ? Direction.LEFT : Direction.RIGHT;
          beginAction(CharacterAction.WALK);
          break;
      }
    }
  }

  void walk() {
    float horizontal = getHorizontal();
    if (isTheBeginningOfAction()) {
      currentAnimation = XAnimations.WALKING;
    }
    if (horizontal != 0) {
      bmc.setXVelocity(walkSpeed * horizontal);
      updateFacing(horizontal);
    } else {
      endAction();
    }
  }

  void walk(InputCommand inputCommand) {
    switch (inputCommand) {

    }
  }

  void dash() {
    float horizontal = getHorizontal();
    if (isTheBeginningOfAction()) {
      timer.storeTime("LastDash", Time.time);
      currentAnimation = XAnimations.DASHING;
      currentActionDirection = horizontal;
      updateFacing(horizontal);
    }
    if (timer.getTimeSince("CurrentActionStart") < 0.3f) {
      if (timer.getTimeSince("LastDownInput") < 0.3f) {
        applyDownwardMomentum();
        timer.storeTime("LastDownInput", 0f);
      }
      bmc.setXVelocity(currentActionDirection * dashSpeed * (currentAction == CharacterAction.JUMPDASH ? 1.8f : 1f));
    } else {
      if (horizontal == 0) {
        endAction();
      } else if (currentActionDirection == horizontal) {
        beginAction(CharacterAction.RUN);
      } else {
        beginAction(CharacterAction.WALK);
      }
    }
  }

  void run() {
    float horizontal = getHorizontal();
    if (isTheBeginningOfAction()) {
      currentAnimation = XAnimations.RUNNING;
      currentActionDirection = horizontal;
    }
    if (horizontal == currentActionDirection) {
      bmc.setXVelocity(horizontal * runSpeed);
    } else if (horizontal == 0) {
      endAction();
    } else {
      beginAction(CharacterAction.WALK);
    }
  }

  void dash(InputCommand inputCommand) {
    switch (inputCommand) {
      case InputCommand.JUMPTAP:
        if (timer.getTimeSince("CurrentActionStart") < 0.1f && isGrounded()) {
          beginAction(CharacterAction.JUMPDASH);
        } else {
          tryJump();
        }
        break;
    }
  }

  void jump(InputCommand inputCommand) {
    switch (inputCommand) {
      case InputCommand.DASH:
        if (timer.getTimeSince("CurrentActionStart") < 0.1f) {
          beginAction(CharacterAction.JUMPDASH);
        } else {
          beginAction(CharacterAction.DASH);
        }
        break;
    }
  }

  void endAction() {
    // if (!isGrounded())
    // {
    //     beginAction(CharacterAction.JUMP);
    // }
    if (getHorizontal() != 0) {
      beginAction(CharacterAction.WALK);
    } else {
      beginAction(CharacterAction.IDLE);
    }
  }

  bool isTheBeginningOfAction() {
    if (newAction == true) {
      newAction = false;
      timer.storeTime("CurrentActionStart", Time.time);
      return true;
    }
    return false;
  }

  float getHorizontal() {
    return isHeldDown(GameButton.LEFT) ? -1 : isHeldDown(GameButton.RIGHT) ? 1 : 0;
  }

  void beginAction(CharacterAction newCharacterAction) {
    // Debug.Log($"{Time.time}, Begin {newCharacterAction}");
    currentAction = newCharacterAction;
    newAction = true;
  }

  bool isHeldDown(GameButton button) {
    return playerController.isHeldDown(button);
  }

  bool isGrounded() {
    if (bmc.velocityY > 0f) {
      return false;
    }
    RaycastHit groundHitInfo;
    Vector3 groundCheckOrigin = mainTransform.position + rigidBody.radius * Vector3.up;
    Physics.SphereCast(groundCheckOrigin, rigidBody.radius,
    Vector3.down, out groundHitInfo,
         0.1f, groundLayerMask);
    return groundHitInfo.collider != null;
  }

  bool isOnOneWayPlatform(out Collider oneWayPlatformCollider) {
    RaycastHit groundHitInfo;
    Vector3 groundCheckOrigin = mainTransform.position + rigidBody.radius * Vector3.up;
    Physics.SphereCast(groundCheckOrigin, rigidBody.radius,
    Vector3.down, out groundHitInfo,
         0.1f, oneWayPlatformLayer);
    oneWayPlatformCollider = groundHitInfo.collider;
    return groundHitInfo.collider != null;
  }

  void updateFacing(float horizontal) {
    if (horizontal != 0) {
      facing = horizontal == -1 ? Direction.LEFT : Direction.RIGHT;
    }
  }

  public void OnDrawGizmos() {
    Gizmos.color = isGrounded() ? Color.green : Color.red;
    Gizmos.DrawWireSphere(
        mainTransform.position + Vector3.up * rigidBody.radius,
        rigidBody.radius
    );
  }

  enum CharacterAction {
    IDLE,
    WALK,
    DASH,
    JUMPDASH,
    RUN,
    JUMP,
  }

  enum InputCommand {
    LEFTTAP,
    LEFTTAP_CANCELED,
    RIGHTTAP,
    RIGHTTAP_CANCELED,
    JUMPTAP,
    DASH,
  }
}

public class XAnimations {
  public const string STANDING_STILL = "Standing Still";
  public const string WALKING = "Walking";
  public const string DASHING = "Dashing";
  public const string RUNNING = "Running";
  public const string JUMP_UP = "Jump Up";
  public const string JUMP_DOWN = "Jump Down";
}


