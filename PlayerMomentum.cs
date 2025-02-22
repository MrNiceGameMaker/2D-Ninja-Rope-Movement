using UnityEngine;

public class PlayerMomentum : MonoBehaviour
{
    public float momentom = 1f;

    // המומנטום של השחקן
    public SOVector2 playerMomentumSO;

    private Vector2 oldPosition;

    private void Start()
    {
        // איפוס המומנטום בהתחלה
        playerMomentumSO.value = Vector2.zero;
        oldPosition = transform.position;
    }

    private void FixedUpdate()
    {
        CalculateMomentum();
    }

    private void CalculateMomentum()
    {
        Vector2 currentPosition = transform.position;
        Vector2 velocity = (currentPosition - oldPosition) * momentom;

        // עדכון ה-Vector2SO
        playerMomentumSO.value = velocity;

        oldPosition = currentPosition;
    }
}
