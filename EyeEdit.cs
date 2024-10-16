using Terraria;
using Terraria.ID;
using System;
using TShockAPI.Hooks;
using TerrariaApi.Server;
using TShockAPI;
using Microsoft.Xna.Framework;

namespace EyeOfCthulhuEvent
{
    [ApiVersion(2, 1)]
    public class EyeOfCthulhuEventPlugin : TerrariaPlugin
    {
        private NPC eyeOfCthulhu;
        private const int EyeOfCthulhuID = 4;
        private bool bossAlive = false;
        private int flamingScytheShootTimer = 0;
        private int circleShootTimer = 0;

        private int flamingScytheDamage = 20;
        private float flamingScytheSpeed = 10f;

        public override string Name => "Eye of Cthulhu Flaming Scythe Event";
        public override string Author => "ViiDuc";
        public override string Description => "An event where Eye of Cthulhu shoots triangular and circular Flaming Scythes.";
        public override Version Version => new Version(1, 1);

        public EyeOfCthulhuEventPlugin(Main game) : base(game) { }

        public override void Initialize()
        {
            ServerApi.Hooks.GameUpdate.Register(this, OnUpdate);
            ServerApi.Hooks.NpcKilled.Register(this, OnNpcKilled);
            ServerApi.Hooks.NpcDespawned.Register(this, OnNpcDespawned);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ServerApi.Hooks.GameUpdate.Deregister(this, OnUpdate);
                ServerApi.Hooks.NpcKilled.Deregister(this, OnNpcKilled);
                ServerApi.Hooks.NpcDespawned.Deregister(this, OnNpcDespawned);
            }
            base.Dispose(disposing);
        }

        private void OnNpcKilled(NpcKilledEventArgs args)
        {
            if (args.npc.netID == EyeOfCthulhuID)
            {
                TSPlayer.All.SendMessage("Eye of Cthulhu has been defeated! Event ends.", Microsoft.Xna.Framework.Color.Green);
                bossAlive = false;
                eyeOfCthulhu = null;
            }
        }

        private void OnNpcDespawned(NpcDespawnedEventArgs args)
        {
            if (args.npc.netID == EyeOfCthulhuID)
            {
                TSPlayer.All.SendMessage("Eye of Cthulhu has disappeared! Event resets.", Microsoft.Xna.Framework.Color.Yellow);
                bossAlive = false;
                eyeOfCthulhu = null;
            }
        }

        private void OnUpdate(EventArgs args)
        {
            if (!bossAlive)
            {
                StartBossEvent();
            }

            if (bossAlive && eyeOfCthulhu != null && eyeOfCthulhu.active)
            {
                flamingScytheShootTimer++;
                circleShootTimer++;

                if (flamingScytheShootTimer >= 180)
                {
                    ShootFlamingScythesAtPlayers();
                    flamingScytheShootTimer = 0;
                }

                if (circleShootTimer >= 600)
                {
                    ShootFlamingScythesInCircle();
                    circleShootTimer = 0;
                }
            }
        }

        private void StartBossEvent()
        {
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.netID == EyeOfCthulhuID)
                {
                    eyeOfCthulhu = npc;
                    bossAlive = true;
                    TSPlayer.All.SendMessage("Eye of Cthulhu has appeared! Event begins.", Microsoft.Xna.Framework.Color.Red);
                    break;
                }
            }
        }

        private void ShootFlamingScythesAtPlayers()
        {
            if (eyeOfCthulhu == null || !eyeOfCthulhu.active) return;

            Vector2 bossPosition = eyeOfCthulhu.Center;
            foreach (TSPlayer player in TShock.Players)
            {
                if (player != null && player.Active && !player.Dead)
                {
                    Vector2 direction = player.TPlayer.Center - bossPosition;
                    direction.Normalize();

                    FireFlamingScythe(bossPosition, direction, 0);
                    FireFlamingScythe(bossPosition, direction, 0.3f);
                    FireFlamingScythe(bossPosition, direction, -0.3f);
                }
            }
        }

        private void ShootFlamingScythesInCircle()
        {
            if (eyeOfCthulhu == null || !eyeOfCthulhu.active) return;

            Vector2 bossPosition = eyeOfCthulhu.Center;
            int numberOfProjectiles = 30;
            float rotationStep = MathHelper.TwoPi / numberOfProjectiles;

            for (int i = 0; i < numberOfProjectiles; i++)
            {
                Vector2 direction = new Vector2((float)Math.Cos(i * rotationStep), (float)Math.Sin(i * rotationStep));
                direction.Normalize();

                FireFlamingScythe(bossPosition, direction, 0);
            }
        }

        private void FireFlamingScythe(Vector2 position, Vector2 direction, float angleOffset)
        {
            Vector2 rotatedDirection = direction.RotatedBy(angleOffset);
            rotatedDirection *= flamingScytheSpeed;

            int projectileID = Projectile.NewProjectile(null, position, rotatedDirection, ProjectileID.FlamingScythe, flamingScytheDamage, 1f);

            var projectile = Main.projectile[projectileID];
            projectile.friendly = false;
            projectile.hostile = true;
            projectile.tileCollide = false;
        }
    }
}
