using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 20f;
    private Vector2 vector2;
    private Rigidbody2D playerBody;

    public Vector2 Vector2
    {
        get => vector2;
        set => vector2 = value;
    }

    private void Awake()
    {
        playerBody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Move();
    }
    public void Move()
    {
        //물리 업데이트에서 이동해 일반 콜라이더와 충돌하면 멈춤
        Vector2 dir = vector2.normalized;
        playerBody.MovePosition(playerBody.position + dir * (moveSpeed * Time.fixedDeltaTime));
    }
}
