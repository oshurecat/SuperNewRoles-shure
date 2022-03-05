using HarmonyLib;
using Hazel;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using SuperNewRoles.Patches;
using SuperNewRoles.CustomOption;
using SuperNewRoles.Roles;
using SuperNewRoles.CustomCosmetics.ShareCosmetics;
using System.Collections;
using BepInEx.IL2CPP.Utils;
using SuperNewRoles.EndGame;
using InnerNet;
using static SuperNewRoles.EndGame.FinalStatusPatch;

namespace SuperNewRoles.CustomRPC
{
    public enum RoleId
    {
        DefaultRole,
        SoothSayer,
        Jester,
        Lighter,
        EvilLighter,
        EvilScientist,
        Sheriff,
        MeetingSheriff,
        Jackal,
        Sidekick,
        Teleporter,
        SpiritMedium,
        SpeedBooster,
        EvilSpeedBooster,
        Tasker,
        Doorr,
        EvilDoorr,
        Sealdor,
        Speeder,
        Freezer,
        Guesser,
        EvilGuesser,
        Vulture,
        NiceScientist,
        Clergyman,
        MadMate,
        Bait,
        HomeSecurityGuard,
        StuntMan,
        Moving,
        Opportunist,
        NiceGambler,
        EvilGambler,
        Bestfalsecharge,
        Researcher,
        SelfBomber,
        God,
        AllCleaner,
        NiceNekomata,
        EvilNekomata,
        JackalFriends,
        Doctor,
        CountChanger,
        Pursuer,
        Minimalist,
        Hawk,
        Egoist,
        NiceRedRidingHood,
        //RoleId
    }

    public enum CustomRPC
    {
        ShareOptions = 91,
        ShareSNRVersion,
        SetRole,
        SetQuarreled,
        RPCClergymanLightOut,
        SheriffKill,
        MeetingSheriffKill,
        CustomRPCKill,
        ReportDeadBody,
        CleanBody,
        ExiledRPC,
        RPCMurderPlayer,
        ShareWinner,
        TeleporterTP,
        SidekickPromotes,
        CreateSidekick,
        SetSpeedBoost,
        ShareCosmetics,
        SetShareNamePlate,
        AutoCreateRoom,
        BomKillRPC,
        ByBomKillRPC,
        NekomataExiledRPC,
        CountChangerSetRPC,
        SetRoomTimerRPC,
        SetScientistRPC,
        ReviveRPC,
        SetHaison,
        SetWinCond
    }
    public static class RPCProcedure
    {

        // Main Controls
        public static void AutoCreateRoom() {
            if (!ConfigRoles.IsAutoRoomCreate.Value) return;
            AmongUsClient.Instance.StartCoroutine(CREATEROOMANDJOIN());
            static IEnumerator CREATEROOMANDJOIN()
            {
                var gameid = AmongUsClient.Instance.GameId;
                yield return new WaitForSeconds(8);
                try
                {
                    AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);
                    SceneChanger.ChangeScene("MainMenu");
                }
                catch
                {
                }
                AmongUsClient.Instance.CoJoinOnlineGameFromCode(gameid);
            }
        }
        public static void SetHaison()
        {
            EndGameManagerSetUpPatch.IsHaison = true;
        }
        public static void ShareCosmetics(byte id, string url)
        {/**
            
            if (ModHelpers.playerById(id) == null) return;
            if (!SharePatch.PlayerUrl.ContainsKey(id))
            {
                SharePatch.PlayerUrl[id] = url;
                HttpConnect.ShareCosmeticDateDownload(id,url);
            }
            **/
        }
        public static void SetRoomTimerRPC (byte min,byte seconds){
            Patch.ShareGameVersion.timer = (min * 60 )+ seconds;
        }
        public static void CountChangerSetRPC(byte sourceid,byte targetid)
        {
            var source = ModHelpers.playerById(sourceid);
            var target = ModHelpers.playerById(targetid);
            if (source == null || target == null) return;
            if (CustomOptions.CountChangerNextTurn.getBool()) { 
                RoleClass.CountChanger.Setdata[source.PlayerId] = target.PlayerId; 
            } else {
                RoleClass.CountChanger.ChangeData[source.PlayerId] = target.PlayerId;
            }
        }
        public static void SetShareNamePlate(byte playerid,byte id) {
        }
        public static void ShareOptions(int numberOfOptions, MessageReader reader)
        {
            try
            {
                for (int i = 0; i < numberOfOptions; i++)
                {
                    uint optionId = reader.ReadPackedUInt32();
                    uint selection = reader.ReadPackedUInt32();
                    CustomOption.CustomOption option = CustomOption.CustomOption.options.FirstOrDefault(option => option.id == (int)optionId);
                    option.updateSelection((int)selection);
                }
            }
            catch (Exception e)
            {
                SuperNewRolesPlugin.Logger.LogError("Error while deserializing options: " + e.Message);
            }
        }
        public static void ShareSNRversion(int major, int minor, int build, int revision, Guid guid, int clientId)
        {
            System.Version ver;
            if (revision < 0)
                ver = new System.Version(major, minor, build);
            else
                ver = new System.Version(major, minor, build, revision);
            Patch.ShareGameVersion.GameStartManagerUpdatePatch.VersionPlayers[clientId] = new Patch.PlayerVersion(ver, guid);
            //SuperNewRolesPlugin.Logger.LogInfo("PATCHES:"+ Patch.ShareGameVersion.playerVersions);
        }
        public static void SetRole(byte playerid,byte RPCRoleId)
        {
            ModHelpers.playerById(playerid).setRole((RoleId)RPCRoleId);
        }
        public static void SetQuarreled(byte playerid1,byte playerid2)
        {
            var player1 = ModHelpers.playerById(playerid1);
            var player2 = ModHelpers.playerById(playerid2);
            RoleHelpers.SetQuarreled(player1,player2);
        }
        public static void SheriffKill(byte SheriffId,byte TargetId,bool MissFire)
        {
            PlayerControl sheriff = ModHelpers.playerById(SheriffId);
            PlayerControl target = ModHelpers.playerById(TargetId);
            if (sheriff == null || target == null) return;

            if (MissFire)
            {
                sheriff.MurderPlayer(sheriff);
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.SheriffMisFire;
            } else
            {
                sheriff.MurderPlayer(target);
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.SheriffKill;
            }

        }
        public static void MeetingSheriffKill(byte SheriffId, byte TargetId, bool MissFire)
        {
            PlayerControl sheriff = ModHelpers.playerById(SheriffId);
            PlayerControl target = ModHelpers.playerById(TargetId);
            if (Constants.ShouldPlaySfx()) SoundManager.Instance.PlaySound(target.KillSfx, false, 0.8f);
            if (sheriff == null || target == null) return;
            if (!PlayerControl.LocalPlayer.isAlive())
            {
                DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, sheriff.name + "は" + target.name + "をシェリフキルした！");
                if (MissFire)
                {
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, sheriff.name + "は誤爆した！");
                } else
                {
                    DestroyableSingleton<HudManager>.Instance.Chat.AddChat(sheriff, sheriff.name + "は成功した！");
                }
            }
            if (MissFire)
            {
                sheriff.Data.IsDead = true;
                sheriff.Exiled();
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.MeetingSheriffMisFire;
                if (PlayerControl.LocalPlayer == sheriff)
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(sheriff.Data, sheriff.Data);
                }
                
            }
            else
            {
                target.Data.IsDead = true;
                target.Exiled();
                FinalStatusData.FinalStatuses[sheriff.PlayerId] = FinalStatus.MeetingSheriffKill;
                if (PlayerControl.LocalPlayer == target)
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(target.Data,sheriff.Data);
                }
            }
            if (MeetingHud.Instance)
            {
                foreach (PlayerVoteArea pva in MeetingHud.Instance.playerStates)
                {
                    if (pva.TargetPlayerId ==　SheriffId && MissFire)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    } else if(pva.TargetPlayerId == TargetId && !MissFire)
                    {
                        pva.SetDead(pva.DidReport, true);
                        pva.Overlay.gameObject.SetActive(true);
                    }
                }
                if (AmongUsClient.Instance.AmHost)
                    MeetingHud.Instance.CheckForEndVoting();
            }

        }
        public static void CustomRPCKill(byte notTargetId,byte targetId)
        {
            if (notTargetId == targetId)
            {
                PlayerControl Player = ModHelpers.playerById(targetId);
                Player.MurderPlayer(Player);
            }
            else
            {
                PlayerControl notTargetPlayer = ModHelpers.playerById(notTargetId);
                PlayerControl TargetPlayer = ModHelpers.playerById(targetId);
                notTargetPlayer.MurderPlayer(TargetPlayer);
            }
        }
        public static void RPCClergymanLightOut(bool Start)
        {
            if (AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started) return;
            if (Start)
            {
                Roles.Clergyman.LightOutStartRPC();
            }
            else
            {
            }
        }
        public static void SetSpeedBoost(bool Is,byte id)
        {
            var player = ModHelpers.playerById(id);
            if (player == null) return;
            if (player.Data.Role.IsImpostor)
            {
                RoleClass.EvilSpeedBooster.IsBoostPlayers[id] = Is;
            } else
            {
                RoleClass.SpeedBooster.IsBoostPlayers[id] = Is;
            }
        }
        public static void ReviveRPC(byte playerid) {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            player.Revive();
            FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.Alive;
        }
        public static void SetScientistRPC(bool Is, byte id)
        {
            SuperNewRolesPlugin.Logger.LogInfo(id+":"+Is);
            RoleClass.NiceScientist.IsScientistPlayers[id] = Is;
        }
        public static void ReportDeadBody(byte sourceId, byte targetId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null) source.ReportDeadBody(target.Data);
        }
        public static void CleanBody(byte playerId)
        {
            DeadBody[] array = UnityEngine.Object.FindObjectsOfType<DeadBody>();
            for (int i = 0; i < array.Length; i++)
            {
                if (GameData.Instance.GetPlayerById(array[i].ParentId).PlayerId == playerId)
                {
                    UnityEngine.Object.Destroy(array[i].gameObject);
                }
            }
        }
        public static void SidekickPromotes() {
            for (int i = 0; i < RoleClass.Jackal.SidekickPlayer.Count; i++) {
                RoleClass.Jackal.JackalPlayer.Add(RoleClass.Jackal.SidekickPlayer[i]);
                RoleClass.Jackal.SidekickPlayer.RemoveAt(i);
            }
            PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
        }
        public static void CreateSidekick(byte playerid) {
            var player = ModHelpers.playerById(playerid);
            if (player == null) return;
            DestroyableSingleton<RoleManager>.Instance.SetRole(player, RoleTypes.Crewmate);
            player.ClearRole();
            RoleClass.Jackal.SidekickPlayer.Add(player);
            PlayerControlHepler.refreshRoleDescription(PlayerControl.LocalPlayer);
        }
        public static void BomKillRPC(byte sourceId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            if (source != null)
            {
                KillAnimationCoPerformKillPatch.hideNextAnimation = false;
                source.MurderPlayer(source);
                FinalStatusData.FinalStatuses[source.PlayerId] = FinalStatus.SelfBomb;
            }
        }
        public static void ByBomKillRPC(byte sourceId, byte targetId)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null)
            {
                source.MurderPlayer(target);
                FinalStatusData.FinalStatuses[target.PlayerId] = FinalStatus.BySelfBomb;
            }
        }
        public static void ExiledRPC(byte playerid) {
            var player = ModHelpers.playerById(playerid);
            if (player != null) {
                player.Exiled();
            }
        }
        public static void NekomataExiledRPC(byte playerid)
        {
            var player = ModHelpers.playerById(playerid);
            if (player != null)
            {
                player.Exiled();
                FinalStatusData.FinalStatuses[player.PlayerId] = FinalStatus.NekomataExiled;
            }
        }
        [HarmonyPatch(typeof(KillAnimation), nameof(KillAnimation.CoPerformKill))]
        class KillAnimationCoPerformKillPatch
        {
            public static bool hideNextAnimation = false;

            public static void Prefix(KillAnimation __instance, [HarmonyArgument(0)] ref PlayerControl source, [HarmonyArgument(1)] ref PlayerControl target)
            {
                if (hideNextAnimation)
                    source = target;
                hideNextAnimation = false;
            }
        }
        public static void RPCMurderPlayer(byte sourceId, byte targetId, byte showAnimation)
        {
            PlayerControl source = ModHelpers.playerById(sourceId);
            PlayerControl target = ModHelpers.playerById(targetId);
            if (source != null && target != null)
            {
                if (showAnimation == 0) KillAnimationCoPerformKillPatch.hideNextAnimation = true;
                source.MurderPlayer(target);
                FinalStatusData.FinalStatuses[source.PlayerId] = FinalStatus.Kill;
            }
        }
        public static void ShareWinner(byte playerid)
        {
            PlayerControl player = ModHelpers.playerById(playerid);
            EndGame.OnGameEndPatch.WinnerPlayer = player;
        }
        public static void TeleporterTP(byte playerid)
        {
            var p = ModHelpers.playerById(playerid);
            PlayerControl.LocalPlayer.transform.position = p.transform.position;
            new CustomMessage(string.Format(ModTranslation.getString("TeleporterTPTextMessage"),p.nameText.text), 3);
        }
        public static void SetWinCond(byte Cond)
        {
            OnGameEndPatch.EndData = (CustomGameOverReason)Cond;
        }
        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartEndGame))]
        class STARTENDGAME
        {
            static void Postfix()
            {
                SuperNewRolesPlugin.Logger.LogInfo("STARTENDGAME!!!");
            }
        }
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
        class RPCHandlerPatch
        {
            static void Postfix([HarmonyArgument(0)] byte callId, [HarmonyArgument(1)] MessageReader reader)
            {
                    byte packetId = callId;
                    switch (packetId)
                    {

                    // Main Controls

                        case (byte)CustomRPC.ShareOptions:
                            RPCProcedure.ShareOptions((int)reader.ReadPackedUInt32(), reader);
                            break;
                        case (byte)CustomRPC.ShareSNRVersion:
                            byte major = reader.ReadByte();
                            byte minor = reader.ReadByte();
                            byte patch = reader.ReadByte();
                            int versionOwnerId = reader.ReadPackedInt32();
                            byte revision = 0xFF;
                            Guid guid;
                            if (reader.Length - reader.Position >= 17)
                            { // enough bytes left to read
                                revision = reader.ReadByte();
                                // GUID
                                byte[] gbytes = reader.ReadBytes(16);
                                guid = new Guid(gbytes);
                            }
                            else
                            {
                                guid = new Guid(new byte[16]);
                            }
                            RPCProcedure.ShareSNRversion(major, minor, patch, revision == 0xFF ? -1 : revision, guid, versionOwnerId);
                            break;
                        case (byte)CustomRPC.SetRole:
                            RPCProcedure.SetRole(reader.ReadByte(), reader.ReadByte());
                            break;
                        case (byte)CustomRPC.SheriffKill:
                            RPCProcedure.SheriffKill(reader.ReadByte(),reader.ReadByte(),reader.ReadBoolean());
                            break;
                        case (byte)CustomRPC.MeetingSheriffKill:
                            RPCProcedure.MeetingSheriffKill(reader.ReadByte(), reader.ReadByte(), reader.ReadBoolean());
                            break;
                    case (byte)CustomRPC.CustomRPCKill:
                        RPCProcedure.CustomRPCKill(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRPC.RPCClergymanLightOut:
                        RPCProcedure.RPCClergymanLightOut(reader.ReadBoolean());
                        break;
                    case (byte)CustomRPC.ReportDeadBody:
                        RPCProcedure.ReportDeadBody(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRPC.CleanBody:
                        RPCProcedure.CleanBody(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.RPCMurderPlayer:
                        byte source = reader.ReadByte();
                        byte target = reader.ReadByte();
                        byte showAnimation = reader.ReadByte();
                        RPCProcedure.RPCMurderPlayer(source, target, showAnimation);
                        break;
                    case (byte)CustomRPC.ExiledRPC:
                        RPCProcedure.ExiledRPC(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ShareWinner:
                        RPCProcedure.ShareWinner(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.TeleporterTP:
                        RPCProcedure.TeleporterTP(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetQuarreled:
                        RPCProcedure.SetQuarreled(reader.ReadByte(),reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SidekickPromotes:
                        RPCProcedure.SidekickPromotes();
                        break;
                    case (byte)CustomRPC.CreateSidekick:
                        RPCProcedure.CreateSidekick(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetSpeedBoost:
                        RPCProcedure.SetSpeedBoost(reader.ReadBoolean(),reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ShareCosmetics:
                        RPCProcedure.ShareCosmetics(reader.ReadByte(),reader.ReadString());
                        break;
                    case (byte)CustomRPC.SetShareNamePlate:
                        RPCProcedure.SetShareNamePlate(reader.ReadByte(),reader.ReadByte());
                        break;
                    case (byte)CustomRPC.AutoCreateRoom:
                        RPCProcedure.AutoCreateRoom();
                        break;
                    case (byte)CustomRPC.BomKillRPC:
                        RPCProcedure.BomKillRPC(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ByBomKillRPC:
                        RPCProcedure.ByBomKillRPC(reader.ReadByte(),reader.ReadByte());
                        break;
                    case (byte)CustomRPC.NekomataExiledRPC:
                        RPCProcedure.NekomataExiledRPC(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.CountChangerSetRPC:
                        RPCProcedure.CountChangerSetRPC(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetRoomTimerRPC:
                        RPCProcedure.SetRoomTimerRPC(reader.ReadByte(), reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetScientistRPC:
                        RPCProcedure.SetScientistRPC(reader.ReadBoolean(), reader.ReadByte());
                        break;
                    case (byte)CustomRPC.ReviveRPC:
                        RPCProcedure.ReviveRPC(reader.ReadByte());
                        break;
                    case (byte)CustomRPC.SetHaison:
                        SetHaison();
                        break;
                    case (byte)CustomRPC.SetWinCond:
                        RPCProcedure.SetWinCond(reader.ReadByte());
                        break;
                    }
            }
        }
        
    }
}
