using UnityEngine;

namespace Game.Enemy
{
    /// <summary>死亡溶解粒子：EnemyDeath 显式调用（保时序），不订阅事件</summary>
    public class EnemyDeathParticles : MonoBehaviour
    {
        public void SpawnDeathParticles()
        {
            GameObject obj = new GameObject("DeathParticles");
            obj.transform.position = transform.position;
            ParticleSystem ps = obj.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.8f;
            main.startSpeed = 0.5f;
            main.startSize = 0.1f;
            main.startColor = Color.white;
            main.maxParticles = 10;
            main.loop = false;
            var emission = ps.emission;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 10) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.5f;
            var vel = ps.velocityOverLifetime;
            vel.enabled = true;
            vel.y = new ParticleSystem.MinMaxCurve(1f, 2f);      // 慢慢往上飘
            vel.x = new ParticleSystem.MinMaxCurve(-0.5f, 0.5f); // 左右随机摆动
            var col = ps.colorOverLifetime;
            col.enabled = true;
            Gradient g = new Gradient();
            g.SetKeys(
                new GradientColorKey[] { new GradientColorKey(Color.white, 0f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );
            col.color = new ParticleSystem.MinMaxGradient(g);
            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            var ad = obj.AddComponent<ParticleAutoDestroy>();
            ad.lifetime = 0.9f;
            ps.Play();
        }
    }
}
