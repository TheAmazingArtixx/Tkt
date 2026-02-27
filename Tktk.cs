using Life;
using Life.Network;
using Life.UI;
using ModKit.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace NLServiceKit
{
    public class IllegalTab_ : Plugin
    {
        public static IllegalTab_ Instance { get; private set; }
        private static Dictionary<string, DateTime> actionCooldowns = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> depouillerCooldowns = new Dictionary<string, DateTime>();
        private Dictionary<string, DateTime> ligoterCooldowns = new Dictionary<string, DateTime>();
        private readonly TimeSpan actionCooldown = TimeSpan.FromMinutes(1.0);
        private const int ActionCooldownSeconds = 15;
        private const float MasquerDurationSeconds = 30f;

        public IllegalTab_(IGameAPI api) : base(api)
        {
            Instance = this;
        }

        public override void OnPlayerInput(Player player, KeyCode keyCode, bool onUI)
        {
            base.OnPlayerInput(player, keyCode, onUI);
            if (onUI)
                return;
            if (keyCode == KeyCode.F11)
            {
                ShowMainMenu(player);

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
        
        public void ShowMainMenu(Player player)
        {
            UIPanel panel = new UIPanel("<b><color=#FF2F00>Pannel Illégal | Northfield RP", UIPanel.PanelType.TabPrice);
            panel.AddTabLine("<b><color=#FF8C00>Fouiller\n" + "<b><color=#BABABA><size=10>Fouille le joueur le plus proche.", "<color=#BABABA><size=8><i>Clique ici pour fouiller le joueur", ItemUtils.GetIconIdByItemId(38), delegate
            {
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer6 = player.GetClosestPlayer();
                    if (closestPlayer6 != null)
                    {
                        player.ClosePanel(panel);
                        Fouiller(player, closestPlayer6);
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
            });
            panel.AddTabLine("<b><color=#FF2F00>Voler de l'argent\n" + "<b><color=#BABABA><size=10>Vole ", "<color=#BABABA><size=8><i>Clique ici pour voler de l'argent", ItemUtils.GetIconIdByItemId(152), delegate
            {
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer5 = player.GetClosestPlayer();
                    if (closestPlayer5 != null)
                    {
                        player.ClosePanel(panel);
                        Voler(player, closestPlayer5);
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
            });
            panel.AddTabLine("<b><color=#8A2BE2>Attacher/détacher", "<color=#BABABA><size=8><i>Atache ou détache un citoyen.", ItemUtils.GetIconIdByItemId(6), delegate
            {
                Player target = player.GetClosestPlayer();
                if (target != null)
                {
                    string playerId2 = player.FullName + "-" + target.FullName;
                    if (IsOnCooldown(ligoterCooldowns, playerId2))
                    {
                        player.Notify("Cooldown", "Tu doit attendre un peu avant de pouvoir attacher/détacher ce citoyen...", NotificationManager.Type.Warning);
                    }
                    else
                    {
                        StartCooldown(ligoterCooldowns, playerId2);
                        Task.Delay(5).ContinueWith(delegate
                        {
                            if (target.setup.player.IsRestrain)
                            {
                                target.setup.player.IsRestrain = false;
                                player.Notify($"<color={LifeServer.COLOR_GREEN}Succès", "Vous avez détaché l'individu !", NotificationManager.Type.Success);
                                player.ClosePanel(panel);
                            }
                            else
                            {
                                Attacher(player, target);
                                player.ClosePanel(panel);
                            }
                        });
                    }
                }
                else
                {
                    player.Notify($"<color={LifeServer.COLOR_RED}Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                }
            });
            
            panel.AddTabLine("<b><color=#FF2F00>Masquer", "<color=#BABABA><size=8><i>Masquer les yeux d'un citoyen.", ItemUtils.GetIconIdByItemId(126), delegate
            {
                bool masquer = false;
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer2 = player.GetClosestPlayer();
                    if (closestPlayer2 != null)
                    {
                        if (masquer == false)
                        {
                            Masquer(player, closestPlayer2);
                            player.Notify("Information", "Le citoyen est attaché, impossible de le masquer", NotificationManager.Type.Warning);
                            masquer = true;
                            player.ClosePanel(panel);
                            return;
                        }
                        else if (masquer == true)
                        {
                            Demasquer(player, closestPlayer2);
                            player.ClosePanel(panel);
                            return;
                        }
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
            });
            panel.AddTabLine("<b><color=#FF2F00>Démasquer", "<color=#BABABA><size=8><i>Démasquer joueur", ItemUtils.GetIconIdByItemId(126), delegate
            {
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer = player.GetClosestPlayer();
                    if (closestPlayer != null)
                    {
                        player.ClosePanel(panel);
                        Demasquer(player, closestPlayer);
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
                panel.AddTabLine("<b><color=#FF2F00>Assomer", "<color=#BABABA><size=8><i>Assome un joueur pour qu'il oublie les 10 dernières minutes.", ItemUtils.GetIconIdByItemId(126), delegate
                {
                    if (CheckActionCooldown(player))
                    {
                        Player closestPlayer = player.GetClosestPlayer();
                        if (closestPlayer != null)
                        {
                            player.ClosePanel(panel);
                            player.Notify("Informations", "Vous avez assomer quelqu'un", NotificationManager.Type.Info);
                            closestPlayer.setup.TargetShowCenterText("Vous êtes assomer", "Vous oubliez les 10 dernières minutes", 10f);
                        }
                        else
                        {
                            player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                        }
                    }
                });
            });
            


            panel.AddButton("<b><color=#F54927>Fermer", delegate
            {
                player.ClosePanel(panel);
            }); //Bouton pa touché
            panel.AddButton("<b><color=#56EB4B>Sélectionner", delegate
            {
                panel.SelectTab();
            }); //Bouton pa touché
            player.ShowPanelUI(panel);
        }

        private bool CheckActionCooldown(Player player)
        {
            string key = player.steamId.ToString();
            if (actionCooldowns.TryGetValue(key, out var value))
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(10.0) - (DateTime.Now - value);
                if (timeSpan.TotalSeconds > 0.0)
                {
                    player.Notify("Cooldown", $"Veuillez attendre encore {Math.Ceiling(timeSpan.TotalSeconds)} seconde(s)", NotificationManager.Type.Warning);
                    return false;
                }
            }

            actionCooldowns[key] = DateTime.Now;
            return true;
        }

        private void Fouiller(Player player, Player target)
        {
            if (target.Health <= 0)
            {
                player.setup.TargetOpenPlayerInventory(target.netId);
                player.Notify("Fouille", "Vous fouillez un corps inconscient");
                return;
            }

            UIPanel panel = new UIPanel("Demande de fouille", UIPanel.PanelType.Text);
            panel.SetText("Une personne souhaite vous fouiller.\n\nAcceptez-vous ?");
            panel.AddButton("<b><color=#F54927>Refuser", delegate
            {
                target.ClosePanel(panel);
                player.Notify("Refus", "L'individu a refusé", NotificationManager.Type.Error);
            });
            panel.AddButton("<b><color=#56EB4B>Accepter", delegate
            {
                target.ClosePanel(panel);
                AppliquerFouille(player, target);
            });
            target.ShowPanelUI(panel);
        }

        private void AppliquerFouille(Player fouilleur, Player cible)
        {
            fouilleur.setup.TargetOpenPlayerInventory(cible.netId);
        }

        private void Voler(Player player, Player target)
        {
            UIPanel panel = new UIPanel("Demande de vol", UIPanel.PanelType.Text);
            panel.SetText("Une personne souhaite vous voler.\n\nAcceptez-vous ?");
            panel.AddButton("Refuser", delegate
            {
                target.ClosePanel(panel);
                player.Notify("Refus", "L'individu a refusé", NotificationManager.Type.Error);
            });
            panel.AddButton("Accepter", delegate
            {
                target.ClosePanel(panel);
                AppliquerVol(player, target);
            });
            target.ShowPanelUI(panel);
        }

        private async void AppliquerVol(Player voleur, Player victime)
        {
            voleur.Notify("En cours", "Vol en cours...");
            victime.Notify("En cours", "Vous vous faites voler");
            await Task.Delay(5000);
            double montant = victime.character.Money;
            if (montant > 0.0)
            {
                victime.character.Money -= montant;
                voleur.character.Money += montant;
                victime.character.Save();
                voleur.character.Save();
                voleur.Notify("Succès", $"Vous avez volé {montant:N0}$", NotificationManager.Type.Success);
                victime.Notify("Vol", $"Vous vous êtes fait voler {montant:N0}$", NotificationManager.Type.Error);
            }
            else
            {
                voleur.Notify("Information", "La personne n'avait pas d'argent sur elle");
            }
        }


        private void Attacher(Player player, Player target)
        {

            UIPanel panel = new UIPanel("Demande d'attachement", UIPanel.PanelType.Text);
            panel.SetText("Une personne souhaite vous attacher.\n\nAcceptez-vous ?");
            panel.AddButton("Refuser", delegate
            {
                target.ClosePanel(panel);
                player.Notify("Refus", "L'individu a refusé", NotificationManager.Type.Error);
            });
            panel.AddButton("Accepter", delegate
            {
                target.ClosePanel(panel);
                AppliquerAttachement(player, target);
            });
            target.ShowPanelUI(panel);
        }
        private void AppliquerAttachement(Player attacheur, Player cible)
        {
            cible.setup.player.IsRestrain = !cible.setup.player.IsRestrain;
            string action = (cible.setup.player.IsRestrain ? "attaché" : "détaché");
            attacheur.Notify("Succès", "Vous avez " + action + " " + cible.character.Firstname + " " + cible.character.Lastname, NotificationManager.Type.Success);
            cible.Notify("Action", "Vous avez été " + action, NotificationManager.Type.Warning);
        }
        
        private void Masquer(Player player, Player target)
        {
            UIPanel panel = new UIPanel("<color=#9C27B0>Demande de masque</color>", UIPanel.PanelType.Text);
            panel.SetText("Une personne souhaite vous masquer.\n\nAcceptez-vous ?");
            panel.AddButton("Refuser", delegate
            {
                target.ClosePanel(panel);
                player.Notify($"<color={LifeServer.COLOR_RED}Refus", "L'individu a refusé de se faire masquer", NotificationManager.Type.Error);
            });
            panel.AddButton("Accepter", delegate
            {
                target.ClosePanel(panel);
                AppliquerMasque(player, target);
            });
            target.ShowPanelUI(panel);
        }

        private async void AppliquerMasque(Player masqueur, Player cible)
        {
            masqueur.Notify("En cours", "Application du masque...", NotificationManager.Type.Info, 3f);
            cible.Notify("En cours", "On vous masque", NotificationManager.Type.Info, 3f);
            await Task.Delay(1500);
            cible.setup.TargetShowCenterText("<color=#000000>YEUX BANDÉS</color>", "Vous avez les yeux bandés...", 30f);
            masqueur.Notify("Succès", "Vous avez masqué " + cible.character.Firstname + " " + cible.character.Lastname, NotificationManager.Type.Success);
            cible.Notify("Masqué", "Vous avez été masqué", NotificationManager.Type.Warning);
        }

        private async void Demasquer(Player player, Player target)
        {
            player.Notify("En cours", "Retrait du masque...", NotificationManager.Type.Info, 3f);
            target.Notify("En cours", "On vous démasque", NotificationManager.Type.Info, 3f);
            await Task.Delay(1500);
            target.setup.TargetShowCenterText("<color=#FFFFFF>LIBERTÉ</color>", "Vos yeux sont libérés", 3f);
            player.Notify("Succès", "Vous avez démasqué " + target.character.Firstname + " " + target.character.Lastname, NotificationManager.Type.Success);
            target.Notify("Démasqué", "Vous avez été démasqué", NotificationManager.Type.Success);
        }
    }
}
z
