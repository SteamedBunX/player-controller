using System;
using UnityEngine;

public class MovementController {
  public float velocityX = 0;
  public float velocityY = 0;
  float timeDelta = 0;
  public float gravityAscend;
  public float gravityDescend;
  public float horizontalDecelerationRate;
  public float airHorizontalDecelerationRate;
  public bool useGravity = true;
  public bool useXDeceleration = true;
  public bool ignoreGravityThisFrame = false;
  public bool ignoreXDecelerationThisFrame = false;
  public bool useAirXDecelerationThisFrame = false;
  bool isGrounded = false;
  CharacterController rigidBody;
  int nodeIndex;

  public MovementController(
      CharacterController rigidBody,
      float gravity = 1f,
      float gravityDescendRatio = 1f,
      float horizontalDecelerationRate = 1f,
      float airHorizontalDecelerationRate = 1f
      ) {
    gravityAscend = gravity;
    gravityDescend = gravity * gravityDescendRatio;
    this.horizontalDecelerationRate = horizontalDecelerationRate;
    this.airHorizontalDecelerationRate = airHorizontalDecelerationRate;
    this.rigidBody = rigidBody;
  }

  public void update(float timeDelta, bool isGrounded) {
    this.isGrounded = isGrounded;
    this.timeDelta = timeDelta;
    useAirXDecelerationThisFrame = !isGrounded;
    movementX();
    movementY();
    ignoreGravityThisFrame = false;
    ignoreXDecelerationThisFrame = false;
  }

  public void movementX() {
    if (useXDeceleration && !ignoreXDecelerationThisFrame) {
      if (velocityX != 0) {
        float newX = (Math.Abs(velocityX)
         - (useAirXDecelerationThisFrame ? airHorizontalDecelerationRate : horizontalDecelerationRate) * timeDelta) *
         (velocityX > 0 ? 1 : -1);
        if (isSameSideNumber(velocityX, newX)) {
          rigidBody.Move(Vector3.right * (newX + velocityX) / 2 * timeDelta);
          setXVelocity(newX);
        } else {
          setXVelocity(0);
        }
      }
    } else {
      rigidBody.Move(Vector3.right * velocityX * timeDelta);
    }
  }

  public void movementY() {
    if (useGravity && !ignoreGravityThisFrame) {
      if (isGrounded && velocityY < 0) {
        setYVelocity(-1f);
        rigidBody.Move(Vector3.up * velocityY * timeDelta);
      } else {
        float newY = velocityY - gravityAscend * timeDelta;
        float dy = (newY + velocityY) / 2 * timeDelta;
        velocityY = newY;
        rigidBody.Move(Vector3.up * velocityY * timeDelta);
      }

    }
  }

  public void move(Vector2 movementDelta) {
    rigidBody.Move(movementDelta);
  }

  public void applyXAcceleration(float x, float timeDelta) {
    float newX = velocityX + x * timeDelta;
    setXVelocity(newX);
  }

  public void setXVelocity(float x) {
    velocityX = x;
  }

  public void setYVelocity(float y) {
    velocityY = y;
  }

  public void setVelocity(float x, float y) {
    velocityX = x;
    velocityY = y;
  }

  bool isSameSideNumber(float num1, float num2) {
    return num1 * num2 >= 0;
  }
}