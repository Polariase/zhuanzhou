using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using MyPool;

public class MonsterAI : CreatureController
{
    [Header("AI 参数")]
    [SerializeField] private float idleRadius = 10f;
    [SerializeField] private float fleeDistance = 20f;
    [SerializeField] private float fleeSpeed = 8f;
    [SerializeField] private float maxHealth = 3;
    [SerializeField] private float fleeDuration = 3f;

    [Header("玩家")]
    [SerializeField] private Transform player;

    [Header("受击反馈")]
    [SerializeField] private Color hitColor = Color.red;
    [SerializeField] private float flashDuration = 0.1f;

    private NavMeshAgent agent;
    private int currentHealth;
    private float idleTimer;
    private bool isFleeing = false;
    private float fleeTimer = 0f;

    private Material[] originalMaterials;
    private Material[] hitMaterials;
    private Renderer[] renderers;
    private Coroutine flashCoroutine;

    public int itemId;

    protected override void Awake()
    {
        base.Awake();

        // ---- NavMeshAgent ----
        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        agent.autoBraking = false;
        agent.stoppingDistance = 0f;
        agent.speed = moveSpeed;

        // ---- 材质缓存 ----
        renderers = GetComponentsInChildren<Renderer>();
        if (renderers.Length > 0)
        {
            originalMaterials = new Material[renderers.Length];
            hitMaterials = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
                hitMaterials[i] = new Material(renderers[i].material);
                hitMaterials[i].color = hitColor;
            }
        }

        // ---- 血量 ----
        currentHealth = Mathf.RoundToInt(maxHealth);

        // ---- 初始目标 ----
        SetRandomTarget();

        // ===== 强制初始化动画（解决初始平移问题） =====
        if (_anim != null)
        {
            _anim.SetFloat("Speed", moveSpeed);
            _anim.SetBool("IsFleeing", false);
        }
    }

    void Update()
    {
        if (player == null && PlayerController.Instance != null)
            player = PlayerController.Instance.transform;
        if (isDead) return;

        // ---- 逃跑状态 ----
        if (isFleeing)
        {
            FleeUpdate();
            fleeTimer -= Time.deltaTime;
            if (fleeTimer <= 0f)
            {
                isFleeing = false;
                SetRandomTarget();
            }
        }
        else
        {
            IdleUpdate();
        }

        // ---- 动画 ----
        isMoving = agent.velocity.magnitude > 0.1f;
        if (_anim != null)
        {
            _anim.SetFloat("Speed", agent.velocity.magnitude);
            _anim.SetBool("IsFleeing", isFleeing);
        }
    }

    // ---- 闲逛 ----
    private void IdleUpdate()
    {
        if (!agent.hasPath || agent.remainingDistance < 1f || idleTimer <= 0)
        {
            SetRandomTarget();
            idleTimer = 2f;
        }
        idleTimer -= Time.deltaTime;
        agent.speed = moveSpeed;
    }

    private void SetRandomTarget()
    {
        Vector3 randomDir = Random.insideUnitSphere * idleRadius;
        randomDir.y = 0;
        Vector3 targetPos = transform.position + randomDir;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, idleRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    // ---- 逃跑 ----
    private void FleeUpdate()
    {
        if (player == null) return;

        Vector3 dir = (transform.position - player.position).normalized;
        Vector3 targetPos = transform.position + dir * fleeDistance;

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            agent.speed = fleeSpeed;
        }
        else
        {
            Vector3 altDir = Quaternion.Euler(0, Random.Range(-30f, 30f), 0) * dir;
            Vector3 altTarget = transform.position + altDir * fleeDistance;
            if (NavMesh.SamplePosition(altTarget, out hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                agent.speed = fleeSpeed;
            }
        }
    }

    private void TriggerFlee()
    {
        isFleeing = true;
        fleeTimer = fleeDuration;
        FleeUpdate();
    }

    // ---- 受伤 ----
    public override bool TakeDamage(float damage, Vector3 hitPoint, bool isElementAdvantage)
    {
        if (isDead) return false;

        currentHealth -= Mathf.RoundToInt(damage);
        PoolManager.Instance.popup.GetAndSet("DamageText", hitPoint, Mathf.RoundToInt(damage), isElementAdvantage);
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // ---- 受击动作 ----
            if (_anim != null)
            {
                _anim.ResetTrigger("Hit");
                _anim.SetTrigger("Hit");
            }

            // ---- 逃跑 ----
            TriggerFlee();

            // ---- 闪烁 ----
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
                flashCoroutine = null;
            }
            flashCoroutine = StartCoroutine(FlashCoroutine());
        }
        return true;
    }

    // ---- 闪烁 ----
    private IEnumerator FlashCoroutine()
    {
        if (renderers == null || renderers.Length == 0) yield break;

        // 变红
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = hitMaterials[i];
        yield return new WaitForSeconds(flashDuration);

        // 恢复
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].material = originalMaterials[i];

        flashCoroutine = null;
    }

    // ---- 死亡 ----
    public async override void Die()
    {
        if (isDead) return;
        isDead = true;
        agent.isStopped = true;

        if (_anim != null)
        {
            // 直接播放 Death 状态（更可靠）
            _anim.Play("Death", 0, 0f);
            _anim.SetBool("IsFleeing", false);
            _anim.SetFloat("Speed", 0f);
        }

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;
        await PoolManager.Instance.item.SpawnAndThrowItemAsync(
        new(DataManager.Instance.GetItemData(itemId), 1),
        HitPoint(),
        player.position - HitPoint());

        float r = Random.Range(0f, 1f);
        if (r < 0.2f)
            await PoolManager.Instance.item.SpawnAndThrowItemAsync(
        new(DataManager.Instance.GetItemData(1001), 3),
        HitPoint(),
        player.position - HitPoint());
        else if (r > 0.8f)
            await PoolManager.Instance.item.SpawnAndThrowItemAsync(
        new(DataManager.Instance.GetItemData(1002), 3),
        HitPoint(),
        player.position - HitPoint());

        foreach (var x in PlayerController.Instance.stats.elementStats)
        {
            x.Value.GainExp(10);
        }
        Invoke(nameof(DisableMonster), 2f);
    }

    private void DisableMonster()
    {
        gameObject.SetActive(false);
    }

    // ---- 调试 ----
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, idleRadius);
    }
}