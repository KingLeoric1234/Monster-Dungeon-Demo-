using System.Collections.Generic;
using UnityEngine;
using Game.Enemy;

public class EnemyDirector : MonoBehaviour
{
    [Header("碰撞怪预制体")]
    [SerializeField] private GameObject collisionNormalPrefab;
    [SerializeField] private GameObject collisionElitePrefab;

    [Header("冲锋怪预制体")]
    [SerializeField] private GameObject chargerNormalPrefab;
    [SerializeField] private GameObject chargerElitePrefab;

    [Header("狙击怪预制体（以后填）")]
    [SerializeField] private GameObject sniperNormalPrefab;
    [SerializeField] private GameObject sniperElitePrefab;

    [Header("自爆怪预制体（以后填）")]
    [SerializeField] private GameObject bomberNormalPrefab;
    [SerializeField] private GameObject bomberElitePrefab;

    [Header("细胞怪预制体")]
    [SerializeField] private GameObject cellNormalPrefab;
    [SerializeField] private GameObject cellElitePrefab;

    [Header("引用")]
    [SerializeField] private Transform player;

    private List<GameObject> activeEnemies = new List<GameObject>();

    // ===== 碰撞怪 =====
    public void SpawnCollisionNormal(Vector2 pos) => SpawnEnemy(collisionNormalPrefab, MonsterConfig.CollisionNormal, pos);
    public void SpawnCollisionElite(Vector2 pos) => SpawnEnemy(collisionElitePrefab, MonsterConfig.CollisionElite, pos);

    // ===== 冲锋怪（用ChargingEnemyAI，不用EnemyMovement）=====
    public void SpawnChargerNormal(Vector2 pos) => SpawnCharger(chargerNormalPrefab, MonsterConfig.ChargerNormal, pos);
    public void SpawnChargerElite(Vector2 pos) => SpawnCharger(chargerElitePrefab, MonsterConfig.ChargerElite, pos);

    // ===== 狙击怪（以后填）=====
    public void SpawnSniperNormal(Vector2 pos) => SpawnEnemy(sniperNormalPrefab, MonsterConfig.SniperNormal, pos);
    public void SpawnSniperElite(Vector2 pos) => SpawnEnemy(sniperElitePrefab, MonsterConfig.SniperElite, pos);

    // ===== 自爆怪（用BomberEnemyAI，不用EnemyMovement）=====
    public void SpawnBomberNormal(Vector2 pos) => SpawnBomber(bomberNormalPrefab, MonsterConfig.BomberNormal, pos);
    public void SpawnBomberElite(Vector2 pos) => SpawnBomber(bomberElitePrefab, MonsterConfig.BomberElite, pos);

    // ===== 细胞怪（用EnemyMovement + CellSplit）=====
    public void SpawnCellNormal(Vector2 pos) => SpawnCell(cellNormalPrefab, MonsterConfig.CellNormal, pos);
    public void SpawnCellElite(Vector2 pos) => SpawnCell(cellElitePrefab, MonsterConfig.CellElite, pos);

    /// <summary>Boss召唤：随机刷一个小怪</summary>
    public void SpawnRandomEnemy(string difficulty, Vector2 pos)
    {
        int r = Random.Range(0, 3);
        if (difficulty == "Elite")
        {
            switch (r)
            {
                case 0: SpawnCollisionElite(pos); break;
                case 1: SpawnBomberElite(pos); break;
                case 2: SpawnCellElite(pos); break;
            }
        }
        else
        {
            switch (r)
            {
                case 0: SpawnCollisionNormal(pos); break;
                case 1: SpawnBomberNormal(pos); break;
                case 2: SpawnCellNormal(pos); break;
            }
        }
    }

    /// <summary>分裂出的小怪，用相同预制体但不同数据</summary>
    public void SpawnCellChild(MonsterTemplate childTemplate, Vector2 pos)
    {
        GameObject prefab = childTemplate.difficulty == "Elite" ? cellElitePrefab : cellNormalPrefab;
        if (prefab == null) return;
        SpawnCell(prefab, childTemplate, pos);
    }

    /// <summary>碰撞怪/狙击怪/自爆怪：用EnemyMovement</summary>
    private void SpawnEnemy(GameObject prefab, MonsterTemplate template, Vector2 position)
    {
        if (prefab == null) { Debug.LogWarning("[EnemyDirector] 预制体未赋值: " + template.monsterName); return; }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        EnemyHealthCalculator health = enemy.GetComponent<EnemyHealthCalculator>();
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyAttack attack = enemy.GetComponent<EnemyAttack>();

        health.Initialize(template);
        if (movement != null) { movement.SetTemplate(template); movement.SetPlayer(player); }
        attack.Initialize(template);

        activeEnemies.Add(enemy);
    }

    /// <summary>冲锋怪：用ChargingEnemyAI，不用EnemyMovement</summary>
    private void SpawnCharger(GameObject prefab, MonsterTemplate template, Vector2 position)
    {
        if (prefab == null) { Debug.LogWarning("[EnemyDirector] 预制体未赋值: " + template.monsterName); return; }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        EnemyHealthCalculator health = enemy.GetComponent<EnemyHealthCalculator>();
        ChargingEnemyAI charger = enemy.GetComponent<ChargingEnemyAI>();
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyAttack attack = enemy.GetComponent<EnemyAttack>();

        health.Initialize(template);
        charger.Initialize(template, player);
        if (movement != null) { movement.SetTemplate(template); movement.SetPlayer(player); }
        attack.Initialize(template);

        activeEnemies.Add(enemy);
    }

    /// <summary>自爆怪：用BomberEnemyAI，不用EnemyMovement</summary>
    private void SpawnBomber(GameObject prefab, MonsterTemplate template, Vector2 position)
    {
        if (prefab == null) { Debug.LogWarning("[EnemyDirector] 预制体未赋值: " + template.monsterName); return; }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        EnemyHealthCalculator health = enemy.GetComponent<EnemyHealthCalculator>();
        BomberEnemyAI bomber = enemy.GetComponent<BomberEnemyAI>();
        EnemyAttack attack = enemy.GetComponent<EnemyAttack>();

        health.Initialize(template);
        bomber.Initialize(template, player);
        attack.Initialize(template);

        activeEnemies.Add(enemy);
    }

    /// <summary>细胞怪：用EnemyMovement追击，加CellSplit分裂</summary>
    private void SpawnCell(GameObject prefab, MonsterTemplate template, Vector2 position)
    {
        if (prefab == null) { Debug.LogWarning("[EnemyDirector] 预制体未赋值: " + template.monsterName); return; }

        GameObject enemy = Instantiate(prefab, position, Quaternion.identity);
        EnemyHealthCalculator health = enemy.GetComponent<EnemyHealthCalculator>();
        EnemyMovement movement = enemy.GetComponent<EnemyMovement>();
        EnemyAttack attack = enemy.GetComponent<EnemyAttack>();
        CellSplit cellSplit = enemy.GetComponent<CellSplit>();

        health.Initialize(template);
        if (movement != null) { movement.SetTemplate(template); movement.SetPlayer(player); }
        attack.Initialize(template);
        if (cellSplit != null) cellSplit.Initialize(template);

        activeEnemies.Add(enemy);
    }

    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in activeEnemies)
            if (enemy != null) Destroy(enemy);
        activeEnemies.Clear();
    }

    public void OnEnemyDied(GameObject enemy) => activeEnemies.Remove(enemy);
}
