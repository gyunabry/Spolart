using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    private Vector2 curPos; // 플레이어의 현재 위치

    private float speed = 3.0f;
    private float rayRange = 0.7f;

    private Animator animator;
    private Vector2 dir;
    private Vector2 curDir = Vector2.down; // 아래 방향으로 초기화

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
        Vector3 centerOfPlayer = transform.position + new Vector3(0, 0.2f, 0); // 플레이어 캐릭터 콜라이더의 중앙

        // 레이캐스트로 Object, Enemy 레이어의 오브젝트 탐지
        RaycastHit2D hit = Physics2D.Raycast(centerOfPlayer, curDir, rayRange, LayerMask.GetMask("Object", "Enemy"));
        if (hit.collider == null) return;

        if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Object"))
        {
            // 오브젝트 탐지
            Debug.Log("오브젝트 발견" + hit.collider.name);
        }
        else if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            // 적 탐지
            Debug.Log("적 발견" + hit.collider.name);
        }
    }

    private void PlayerControll()
    {
        // curDir에 현재 이동 방향을 입력해 레이캐스트 동작
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

        // 캐릭터가 이동할 때 curDir 갱신
        if (dir != Vector2.zero)
        {
            curDir = dir.normalized;
        }

        dir.Normalize();
        animator.SetBool("IsMoving", dir.magnitude > 0);

        GetComponent<Rigidbody2D>().linearVelocity = speed * dir;

        // 디버그용
        Debug.DrawRay(transform.position + new Vector3(0, 0.2f, 0), curDir * rayRange, Color.green);
    }
}
