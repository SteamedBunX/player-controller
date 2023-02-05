public interface CharacterActionScript {
  public void Setup(PlayerController playerController);
  public void Update();
  public void LeftTap(ButtonState buttonState);
  public void RightTap(ButtonState buttonState);
  public void DashInput();
  public void JumpInput();
  public void DownInput();
  public void OnDrawGizmos();
}