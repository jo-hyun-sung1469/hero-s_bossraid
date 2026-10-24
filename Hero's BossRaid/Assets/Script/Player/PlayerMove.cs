using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    private Vector2 vector2;

    public Vector2 Vector2
    {
        get => vector2;
        set => vector2 = value;
    }

    void Update()
    {
        Move();
    }
    public void Move()
    {
        Vector3 dir = new Vector3(vector2.x, vector2.y, 0).normalized;
        transform.position += dir * (moveSpeed * Time.deltaTime);
    }
}
