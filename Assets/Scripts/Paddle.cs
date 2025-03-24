using UnityEngine;

public class Paddle : MonoBehaviour
{
  public float Speed = 2.0f;
  public float MaxMovement = 2.0f;

  private void Update()
  {
    float input = Input.GetAxis("Horizontal");

    Vector3 pos = transform.position;
    pos.x += input * Speed * Time.deltaTime;

    if (pos.x > MaxMovement)
      pos.x = MaxMovement;
    else if (pos.x < -MaxMovement)
      pos.x = -MaxMovement;

    transform.position = pos;
  }

  private void OnCollisionEnter(Collision other)
  {
    if (other.gameObject.CompareTag("Ball"))
    {
      MainManager manager = FindObjectOfType<MainManager>();
      if (manager != null && manager.bricksNeedRespawn)
      {
        manager.RespawnBricks(); // Respawn bricks to continue the game
        manager.SetBricksNeedRespawn(false); // Reset the flag
      }
    }
  }
}