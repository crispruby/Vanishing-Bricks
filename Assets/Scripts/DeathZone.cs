using UnityEngine;

public class DeathZone : MonoBehaviour
{
  public MainManager Manager;

  private void OnCollisionEnter(Collision other)
  {
    if (other.gameObject.CompareTag("Ball"))
    {
      Rigidbody ballRigidbody = other.gameObject.GetComponent<Rigidbody>();
      if (ballRigidbody != null)
      {
        ballRigidbody.velocity = Vector3.zero;
        other.transform.position = new Vector3(0, 1, 0); // Reset ball position
      }

      Manager.GameOver(); // End game when ball hits death zone

      // Respawn bricks for the next game regardless of brick state
      Manager.RespawnBricks();
    }
  }
}