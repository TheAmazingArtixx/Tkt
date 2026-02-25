#region assembly PanelIllégalF11, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Program Files (x86)\Steam\steamapps\common\Nova-Life\Servers\NL\Plugins\PanelIllegalF11.dll
// Decompiled with ICSharpCode.Decompiler 8.2.0.7535
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Life;
using Life.InventorySystem;
using Life.Network;
using Life.UI;
using Life.VehicleSystem;
using ModKit.Utils;
using UnityEngine;

namespace PhalakaaHelper;

public class PanelIllégal : Plugin
{
    private Dictionary<string, DateTime> depouillerCooldowns = new Dictionary<string, DateTime>();

    private Dictionary<string, DateTime> ligoterCooldowns = new Dictionary<string, DateTime>();

    private readonly TimeSpan actionCooldown = TimeSpan.FromMinutes(1.0);

    public PanelIllégal(IGameAPI aPI)
        : base(aPI)
    {
    }

    public override void OnPluginInit()
    {
        base.OnPluginInit();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Le plugin [Panel Illégal] est initialisé");
        Console.ResetColor();
    }

    public override void OnPlayerInput(Player player, KeyCode keyCode, bool onUI)
    {
        base.OnPlayerInput(player, keyCode, onUI);
        if (keyCode == KeyCode.F11)
        {
            IllégalMenu(player);
        }
    }

    private bool IsOnCooldown(Dictionary<string, DateTime> cooldownDict, string playerId)
    {
        if (cooldownDict.TryGetValue(playerId, out var value))
        {
            if (DateTime.Now < value)
            {
                return true;
            }

            cooldownDict.Remove(playerId);
            return false;
        }

        return false;
    }

    private void StartCooldown(Dictionary<string, DateTime> cooldownDict, string playerId)
    {
        cooldownDict[playerId] = DateTime.Now.Add(actionCooldown);
    }

    public void IllégalMenu(Player player)
    {
        UIPanel panel = new UIPanel("<b>Panel Illégal</b>\r\n", UIPanel.PanelType.Text);
        panel.SetTitle("<b>Panel Illégal</b>\r\n");
        panel.text = "Choisis L'action à réaliser !";
        panel.AddButton("<color=#D20103><b>Fermer</b></color>\r\n", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
        });
        panel.AddButton("<color=#FF8C00><b>Fouiller</b></color>", delegate
        {
            Player closestPlayer4 = player.GetClosestPlayer();
            if (closestPlayer4 != null)
            {
                FouillerRequest(closestPlayer4);
            }
            else
            {
                player.Notify("Erreur !", "Il n'y à personne à proximité !", NotificationManager.Type.Error);
            }
        });
        panel.AddButton("<color=#1E90FF><b>Assomer</b></color>\r\n", delegate
        {
            Player closestPlayer3 = player.GetClosestPlayer();
            if (closestPlayer3 != null)
            {
                AssomerRequest(closestPlayer3);
            }
            else
            {
                player.Notify("Erreur", "Personne n'est à proximité", NotificationManager.Type.Error);
            }
        });
        panel.AddButton("<color=#32CD32><b>Masquer</b></color>\r\n", delegate
        {
            Player closestPlayer2 = player.GetClosestPlayer();
            if (closestPlayer2 != null)
            {
                closestPlayer2.setup.TargetShowCenterText("Yeux Bander", "Vous avez les yeux bandés...", 15f);
            }
            else
            {
                player.Notify("Erreur", "Personne n'est à proximité", NotificationManager.Type.Error);
            }
        });
        panel.AddButton("<color=#FFD700><b>Déverrouiller</b></color>\r\n", delegate
        {
            Vehicle closestVehicle = player.GetClosestVehicle();
            if ((UnityEngine.Object)(object)closestVehicle == null)
            {
                player.Notify("Erreur", "Aucun véhicule à proximité", NotificationManager.Type.Error);
            }
            else if (!closestVehicle.NetworkisLocked)
            {
                player.Notify("Attention", "Le véhicule est déjà déverrouillé", NotificationManager.Type.Warning);
            }
            else if (!PlayerHasItem(player, 1580))
            {
                player.Notify("Erreur", "Vous avez besoin d'un pied-de-biche pour déverrouiller ce véhicule !", NotificationManager.Type.Error);
            }
            else
            {
                System.Random random2 = new System.Random();
                int num6 = random2.Next(1, 7);
                if (num6 == 2)
                {
                    closestVehicle.NetworkisLocked = false;
                    player.Notify("Succès", "Véhicule déverrouillé ! Le pied-de-biche s'est cassé.", NotificationManager.Type.Success);
                    InventoryUtils.RemoveFromInventory(player, 1580, 1);
                }
                else
                {
                    player.Notify("Échec", "Tentative échouée. Le pied-de-biche reste utilisable.", NotificationManager.Type.Error);
                }

                player.ClosePanel(panel);
            }
        });
        panel.AddButton("<color=#8A2BE2><b>Ligoter</b></color>\r\n", delegate
        {
            Player target = player.GetClosestPlayer();
            if (target != null)
            {
                string playerId2 = player.FullName + "-" + target.FullName;
                if (IsOnCooldown(ligoterCooldowns, playerId2))
                {
                    player.Notify("Cooldown", "Vous devez attendre avant de pouvoir ligoter/déligoter ce joueur à nouveau.", NotificationManager.Type.Warning);
                }
                else
                {
                    StartCooldown(ligoterCooldowns, playerId2);
                    Task.Delay(5000).ContinueWith(delegate
                    {
                        if (target.setup.player.IsRestrain)
                        {
                            target.setup.player.IsRestrain = false;
                            player.Notify("Succès", "Vous avez détaché l'individu !", NotificationManager.Type.Success);
                        }
                        else
                        {
                            target.setup.player.IsRestrain = true;
                            player.Notify("Succès", "Vous avez attaché l'individu !", NotificationManager.Type.Success);
                        }
                    });
                }
            }
            else
            {
                player.Notify("Erreur", "Aucun joueur à proximité", NotificationManager.Type.Error);
            }
        });
        panel.AddButton("<color=#FF1493><b>Dépouiller</b></color>\r\n", delegate
        {
            Player closestPlayer = player.GetClosestPlayer();
            if (closestPlayer != null)
            {
                string playerId = player.FullName + "-" + closestPlayer.FullName;
                if (IsOnCooldown(depouillerCooldowns, playerId))
                {
                    player.Notify("Cooldown", "Vous devez attendre avant de dépouiller ce joueur à nouveau.", NotificationManager.Type.Warning);
                }
                else if (closestPlayer.character.Money > 0.0)
                {
                    System.Random random = new System.Random();
                    int num = 10;
                    int num2 = 33;
                    int num3 = random.Next(0, 101);
                    if (num3 <= num2)
                    {
                        double num4 = (double)random.Next(1, num + 1) / 100.0;
                        int num5 = (int)Math.Floor(closestPlayer.character.Money * num4);
                        if (num5 > 0)
                        {
                            closestPlayer.AddMoney(-num5, "steal");
                            player.AddMoney(num5, "steal");
                            player.Notify("Interaction", $"Vous venez de voler {num5}€", NotificationManager.Type.Success, 6f);
                            StartCooldown(depouillerCooldowns, playerId);
                        }
                        else
                        {
                            player.Notify("Interaction", "Il n'y avait rien à voler", NotificationManager.Type.Warning, 6f);
                        }
                    }
                    else
                    {
                        closestPlayer.Notify("Interaction", "Quelqu'un vient de glisser sa main dans votre poche !", NotificationManager.Type.Warning, 6f);
                        player.Notify("Interaction", "Votre cible s'aperçoit qu'une main vient de se glisser dans sa poche", NotificationManager.Type.Warning, 6f);
                        StartCooldown(depouillerCooldowns, playerId);
                    }
                }
                else
                {
                    player.Notify("Interaction", "Il n'y avait rien à voler", NotificationManager.Type.Warning, 6f);
                }
            }
            else
            {
                player.Notify("Échec", "Aucun citoyen à proximité", NotificationManager.Type.Error);
            }
        });
        player.ShowPanelUI(panel);
    }

    public void FouillerRequest(Player player)
    {
        Player target = player.GetClosestPlayer();
        if (target != null)
        {
            if (target.Health <= 0)
            {
                InventoryUtils.TargetOpenPlayerInventory(target, player);
                player.Notify("Information", "Vous fouillez un joueur inconscient");
                return;
            }

            UIPanel uIPanel = new UIPanel("Demande", UIPanel.PanelType.Text);
            uIPanel.SetText("Une personne souhaite vous fouiller");
            uIPanel.AddButton("<color=#D20103>Fermer</color>", delegate (UIPanel ui)
            {
                player.ClosePanel(ui);
            });
            uIPanel.AddButton("<color=#4BD913>Accepter</color>", delegate (UIPanel ui)
            {
                player.ClosePanel(ui);
                InventoryUtils.TargetOpenPlayerInventory(target, player);
                target.Notify("Information", "Vous vous faites fouiller");
            });
            player.ShowPanelUI(uIPanel);
        }
        else
        {
            player.Notify("Erreur", "Aucun joueur à proximité", NotificationManager.Type.Error);
        }
    }

    public void AssomerRequest(Player player)
    {
        Player target = player.GetClosestPlayer();
        if (target != null)
        {
            if (target.Health <= 0)
            {
                player.Notify("Erreur", "Vous ne pouvez pas assommer un joueur inconscient", NotificationManager.Type.Error);
                return;
            }

            UIPanel uIPanel = new UIPanel("Demande", UIPanel.PanelType.Text);
            uIPanel.SetText("Une personne souhaite vous assomer");
            uIPanel.AddButton("<color=#D20103>Fermer</color>", delegate (UIPanel ui)
            {
                player.ClosePanel(ui);
            });
            uIPanel.AddButton("<color=#4BD913>Accepter</color>", async delegate (UIPanel ui)
            {
                player.ClosePanel(ui);
                player.Health = 0;
                player.setup.TargetShowCenterText("ASSOMER", "Une personne vous a assomer et vous oublier tout...", 5f);
                target.Notify("Information", "Vous avez assomer la personne");
                await Task.Delay(15000);
                player.Health = 5;
                player.Notify("Information", "Vous vous relevez mais vous avez tout oublier");
            });
            player.ShowPanelUI(uIPanel);
        }
        else
        {
            player.Notify("Erreur", "Aucun joueur à proximité", NotificationManager.Type.Error);
        }
    }

    internal static bool PlayerHasItem(Player player, int itemId)
    {
        return ((IEnumerable<ItemInventory>)player.setup.inventory.items).Any((ItemInventory x) => x.itemId == itemId);
    }
}
