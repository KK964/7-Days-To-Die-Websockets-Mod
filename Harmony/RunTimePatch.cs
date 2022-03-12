using HarmonyLib;
using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Newtonsoft.Json;
using GUI_2;
using UnityEngine;

namespace _7DTDWebsockets.patchs
{
    internal class RunTimePatch
    {
        public static void PatchAll()
        {
            Log.Out("[Websocket] Runtime patches intinialized");
            Harmony harmony = new Harmony("com.gmail.kk964gaming.websockets.patch");
            Harmony.DEBUG = true;
            harmony.PatchAll();

            foreach (var method in harmony.GetPatchedMethods())
            {
                Log.Out($"Successfully patched: {method.Name}");
            }
        }
    }
}

public class Player
{
    public string name;

    public Player(ClientInfo clientInfo)
    {
        this.name = clientInfo.playerName ?? "Unknwon";
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

[HarmonyPatch]
class ClientPlayerDamagePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityAlive), "DamageEntity")]
    static void Prefix(EntityAlive __instance, DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
    {
        Log.Out($"EntityAlive damage : {__instance.GetDebugName()} {_strength}");
        if (!(__instance is EntityPlayer)) return;
        EntityPlayer player = (EntityPlayer)__instance;
        _7DTDWebsockets.API.Send("PlayerDamage", JsonConvert.SerializeObject(new PlayerDmgEvent(new Player(player), _damageSource.damageType.ToString(), _strength)));
        return;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Entity), "DamageEntity")]
    static void Prefix2(Entity __instance, DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f)
    {
        Log.Out($"Entity damage : {__instance.GetDebugName()} {_strength}");
        if (!(__instance is EntityPlayer)) return;
        EntityPlayer player = (EntityPlayer)__instance;
        _7DTDWebsockets.API.Send("PlayerDamage", JsonConvert.SerializeObject(new PlayerDmgEvent(new Player(player), _damageSource.damageType.ToString(), _strength)));
        return;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EntityPlayer), "DamageEntity")]
    static void Prefix3(EntityPlayer __instance, DamageSource _damageSource, int _strength, bool _criticalHit, float _impulseScale = 1f)
    {
        Log.Out($"EntityAlive damage : {__instance.GetDebugName()} {_strength}");
        EntityPlayer player = __instance;
        _7DTDWebsockets.API.Send("PlayerDamage", JsonConvert.SerializeObject(new PlayerDmgEvent(new Player(player), _damageSource.damageType.ToString(), _strength)));
        return;
    }
}

class PlayerKillEntityEvent
{
    public Player player;
    public string entity;
    public bool animal;
    public bool zombie;
    public PlayerKillEntityEvent(Player player, string entity, bool animal, bool zombie)
    {
        this.player = player;
        this.entity = entity;
        this.animal = animal;
        this.zombie = zombie;
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

        _7DTDWebsockets.API.Send("PlayerKillEntity", JsonConvert.SerializeObject(new PlayerKillEntityEvent(new Player(player), ent, animal, zombie)));

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

