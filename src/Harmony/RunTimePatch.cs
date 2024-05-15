using System;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using HarmonyLib;

//original work done by KK
//modifications for patching weaponType and headshots added from Mustached_Maniac

namespace _7DTDWebsockets.patchs
{
    internal class RunTimePatch
    {
        public static bool IsHeadshot { get; set; } = false;

        public static void PatchAll()
        {
            Log.Out("[Websocket] Runtime patches initialized");
            Harmony harmony = new Harmony("com.gmail.kk964gaming.websockets.patch");
            harmony.PatchAll();

            foreach (var method in harmony.GetPatchedMethods())
            {
                Log.Out($"Successfully patched: {method.Name}");
            }
        }
    }

    class ReflectionUtils
    {
        public static object GetValue(object obj, string field)
        {
            if (obj == null) return null;
            if (field == null) return null;
            Type type = obj.GetType();
            FieldInfo info = type.GetField(field, BindingFlags.NonPublic | BindingFlags.Instance);
            if (info == null) return null;
            return info.GetValue(obj);
        }
    }

    public class Player
    {
        public string name;

        public Player(ClientInfo clientInfo)
        {
            this.name = clientInfo.playerName ?? "Unknown";
        }

        public Player(EntityPlayer player)
        {
            this.name = player.EntityName;
        }
    }

    class PlayerDmgEvent
    {
        public Player player;
        public string cause;
        public int damage;
        public PlayerDmgEvent(Player player, string cause, int damage)
        {
            this.player = player;
            this.cause = cause;
            this.damage = damage;
        }
    }

    class PlayerKillEntityEvent
    {
        public Player player;
        public string entity;
        public bool animal;
        public bool zombie;
        public string weaponType;
        public bool headshot;
        public PlayerKillEntityEvent(Player player, string entity, bool animal, bool zombie, string weaponType, bool headshot)
        {
            this.player = player;
            this.entity = entity;
            this.animal = animal;
            this.zombie = zombie;
            this.weaponType = weaponType;
            this.headshot = headshot;
        }
    }

    class PlayerEntityEvent
    {
        public Player player;
        public string entity;
        public PlayerEntityEvent(Player player, string entity)
        {
            this.player = player;
            this.entity = entity;
        }
    }

    class PlayerOnlyEvent
    {
        public Player player;
        public PlayerOnlyEvent(Player player)
        {
            this.player = player;
        }
    }

    class PlayerDeathEvent
    {
        public Player player;
        public PlayerDeathEvent(Player player)
        {
            this.player = player;
        }
    }

    //updated payload to include weaponType and headshot bool
    [HarmonyPatch(typeof(EntityAlive), "SetDead")]
    class PatchEntityDeath
    {
        static bool Prefix(EntityAlive __instance)
        {
            object obj = Traverse.Create(__instance).Field("entityThatKilledMe").GetValue();
            if (__instance is EntityPlayer) return true;
            if (obj == null) return true;
            EntityAlive whokilledMe = (EntityAlive)obj;
            if (whokilledMe == null) return true;
            if (!(whokilledMe is EntityPlayer)) return true;
            EntityPlayer player = whokilledMe as EntityPlayer;
            string ent = __instance.GetDebugName();

            bool animal = false;
            bool zombie = false;

            if (ent.ToLower().Contains("animal")) animal = true;
            if (ent.ToLower().Contains("zombie")) zombie = true;

            if (animal) ent = Regex.Replace(ent, "(animal)", "", RegexOptions.IgnoreCase);
            if (zombie) ent = Regex.Replace(ent, "(zombie)", "", RegexOptions.IgnoreCase);

            string weaponType = player.inventory.holdingItem.Name;
            bool headshot = RunTimePatch.IsHeadshot;

            _7DTDWebsockets.API.Send("PlayerKillEntity", JsonConvert.SerializeObject(new PlayerKillEntityEvent(new Player(player), ent, animal, zombie, weaponType, headshot)));

            if (animal && !zombie)
            {
                _7DTDWebsockets.API.Send("PlayerKillAnimal", JsonConvert.SerializeObject(new PlayerEntityEvent(new Player(player), ent)));
            }

            if (zombie)
            {
                _7DTDWebsockets.API.Send("PlayerKillZombie", JsonConvert.SerializeObject(new PlayerEntityEvent(new Player(player), ent)));
            }

            return true;
        }
    }
    //added to check for the headshots
    [HarmonyPatch(typeof(EntityAlive), "damageEntityLocal")]
    class PatchDamageEntityLocal
    {
        static void Postfix(DamageResponse __result)
        {
            RunTimePatch.IsHeadshot = __result.HitBodyPart == EnumBodyPartHit.Head;
        }
    }
    //included the headshot tag with the payload
    [HarmonyPatch]
    class DamagePatches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetPackageDamageEntity), "ProcessPackage")]
        static void DamageEntityPacketProccessPrefix(NetPackage __instance, World _world, GameManager _callbacks)
        {
            string name = NetPackageManager.GetPackageName(__instance.PackageId);
            if (name != "NetPackageDamageEntity") return;
            NetPackageDamageEntity damage = (NetPackageDamageEntity)__instance;
            int entityId = (int)ReflectionUtils.GetValue(damage, "entityId");
            Entity entity = _world.GetEntity(entityId);
            if (entity == null || !(entity is EntityPlayer)) return;
            EnumDamageTypes damageType = (EnumDamageTypes)ReflectionUtils.GetValue(damage, "damageTyp");
            int dmg = (ushort)ReflectionUtils.GetValue(damage, "strength");
            _7DTDWebsockets.API.Send("PlayerDamage", JsonConvert.SerializeObject(new PlayerDmgEvent(new Player((EntityPlayer)entity), damageType.ToString(), dmg)));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EntityAlive), "DamageEntity")]
        static void EntityAliveDamagePrefix(EntityAlive __instance, DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
        {
            if (!(__instance is EntityPlayer)) return;
            EntityPlayer player = (EntityPlayer)__instance;
            _7DTDWebsockets.API.Send("PlayerDamage", JsonConvert.SerializeObject(new PlayerDmgEvent(new Player(player), _damageSource.damageType.ToString(), _strength)));
            return;
        }
    }
}
