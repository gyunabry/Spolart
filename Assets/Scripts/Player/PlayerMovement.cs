using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private Vector2 curPos; // �÷��̾��� ���� ��ġ

    private float speed = 3.0f;
    private float rayRange = 0.7f;

    private Animator animator;
    private Vector2 dir;
    private Vector2 curDir = Vector2.down; // �Ʒ� �������� �ʱ�ȭ

    private void Start()
    {
        animator = GetComponent<Animator>(); 
    }

    private void Update()
    {
        if (!IsOwner || !IsSpawned) return;

        PlayerControll();
        DetectObjectsAndEnemies();
    }

    private void DetectObjectsAndEnemies()
    {
        Vector3 centerOfPlayer = transform.position + new Vector3(0, 0.2f, 0); // �÷��̾� ĳ���� �ݶ��̴��� �߾�

        // ����ĳ��Ʈ�� Object, Enemy ���̾��� ������Ʈ Ž��
        RaycastHit2D hit = Physics2D.Raycast(centerOfPlayer, curDir, rayRange, LayerMask.GetMask("Object", "Enemy"));
        if (hit.collider == null) return;

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Object"))
        {
            // ������Ʈ Ž��
            Debug.Log("������Ʈ �߰�" + hit.collider.name);
        }
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // �� Ž��
            Debug.Log("�� �߰�" + hit.collider.name);
        }
    }

    private void PlayerControll()
    {
        // curDir�� ���� �̵� ������ �Է��� ����ĳ��Ʈ ����
        dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
        {
            dir.x = -1;
            animator.SetInteger("Direction", 3);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            dir.x = 1;
            animator.SetInteger("Direction", 2);
        }

        if (Input.GetKey(KeyCode.W))
        {
            dir.y = 1;
            animator.SetInteger("Direction", 1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            dir.y = -1;
            animator.SetInteger("Direction", 0);
        }

        // ĳ���Ͱ� �̵��� �� curDir ����
        if (dir != Vector2.zero)
        {
            curDir = dir.normalized;
        }

        dir.Normalize();
        animator.SetBool("IsMoving", dir.magnitude > 0);

        GetComponent<Rigidbody2D>().linearVelocity = speed * dir;

        // ����׿�
        Debug.DrawRay(transform.position + new Vector3(0, 0.2f, 0), curDir * rayRange, Color.green);
    }
}
