using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class OctopusBossController : MonoBehaviour
{
    [Header("References")]
    public TentacleFABRIK[] tentacles;

    [Header("Boss Stats")]
    public int maxHP = 500;
    public float enrageThreshold = 0.4f;

    [Header("Attack Coordination")]
    public float minTimeBetweenAttacks = 0.5f;
    public float maxTimeBetweenAttacks = 1.5f;
    public int maxSimultaneousAttacks = 2;

    int currentHP;
    bool isEnraged = false;
    int activeAttacks = 0;
    int lastAttackIndex = -1;
    Transform player;

    void Start()
    {
        currentHP = maxHP;
        player = GameObject.FindWithTag("Player").transform;
        StartCoroutine(AttackLoop());
    }

    IEnumerator AttackLoop()
    {
        while (currentHP > 0)
        {
            yield return new WaitForSeconds(
                Random.Range(minTimeBetweenAttacks, maxTimeBetweenAttacks)
            );

            if (activeAttacks >= maxSimultaneousAttacks) continue;

            List<TentacleFABRIK> available = GetAvailableTentacles();
            if (available.Count == 0) continue;

            // Pilih berbeda dari terakhir
            TentacleFABRIK chosen = null;
            foreach (var t in available)
            {
                if (System.Array.IndexOf(tentacles, t) != lastAttackIndex)
                {
                    chosen = t;
                    break;
                }
            }
            if (chosen == null) chosen = available[0];

            lastAttackIndex = System.Array.IndexOf(tentacles, chosen);
            StartCoroutine(TriggerAttack(chosen));
        }
    }

    IEnumerator TriggerAttack(TentacleFABRIK tentacle)
    {
        activeAttacks++;
        tentacle.TriggerAttack();
        yield return new WaitUntil(() => tentacle.IsIdle());
        activeAttacks--;
    }

    List<TentacleFABRIK> GetAvailableTentacles()
    {
        bool playerOnRight = player.position.x > transform.position.x;
        List<TentacleFABRIK> list = new();
        foreach (var t in tentacles)
        {
            bool sameSide = (t.side == TentacleFABRIK.TentacleSide.Right) == playerOnRight;
            if (t.IsIdle() && sameSide) list.Add(t);
        }
        return list;
    }

    public void TakeDamage(int dmg)
    {
        currentHP -= dmg;
        currentHP = Mathf.Max(0, currentHP);

        if (!isEnraged && (float)currentHP / maxHP <= enrageThreshold)
            Enrage();

        if (currentHP <= 0)
            Die();
    }

    void Enrage()
    {
        isEnraged = true;
        maxSimultaneousAttacks = 3;
        minTimeBetweenAttacks = 0.2f;
        maxTimeBetweenAttacks = 0.6f;

        foreach (var t in tentacles)
        {
            t.slashSpeed *= 1.5f;
            t.cooldownDuration *= 0.6f;
        }
    }

    void Die()
    {
        StopAllCoroutines();
        Destroy(gameObject, 2f);
    }
}