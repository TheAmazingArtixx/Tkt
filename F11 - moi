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

        public void ShowMainMenu(Player player)
        {
            UIPanel panel = new UIPanel("<b><color=#FF2F00>Pannel Illégal | Northfield RP", UIPanel.PanelType.TabPrice);
            panel.AddTabLine("<b><color=#FF2F00>Fouiller\n" + "<b><color=#BABABA><size=10>Fouille le joueur le plus proche.", "<size=6><i>Clique ici pour fouiller le joueur", ItemUtils.GetIconIdByItemId(38), delegate
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
            panel.AddTabLine("Voler de l'argent\n" + "<b><color=#BABABA><size=10>Vole ", "<size=6><i> Clique ici pour voler de l'argent", ItemUtils.GetIconIdByItemId(152), delegate
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
            panel.AddTabLine("Attacher", "Attacher joueur", ItemUtils.GetIconIdByItemId(6), delegate
            {
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer4 = player.GetClosestPlayer();
                    if (closestPlayer4 != null)
                    {
                        player.ClosePanel(panel);
                        Attacher(player, closestPlayer4);
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
            });
            panel.AddTabLine("Détacher", "Détacher joueur", ItemUtils.GetIconIdByItemId(152), delegate
            {
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer3 = player.GetClosestPlayer();
                    if (closestPlayer3 != null)
                    {
                        player.ClosePanel(panel);
                        Detacher(player, closestPlayer3);
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
            });
            panel.AddTabLine("Masquer", "Masquer les yeux d'un citoyen.", ItemUtils.GetIconIdByItemId(126), delegate
            {
                if (CheckActionCooldown(player))
                {
                    Player closestPlayer2 = player.GetClosestPlayer();
                    if (closestPlayer2 != null)
                    {
                        player.ClosePanel(panel);
                        Masquer(player, closestPlayer2);
                    }
                    else
                    {
                        player.Notify("Oups...", "Aucun citoyen à proximité", NotificationManager.Type.Error);
                    }
                }
            });
            panel.AddTabLine("Démasquer", "Démasquer joueur", ItemUtils.GetIconIdByItemId(126), delegate
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
                panel.AddTabLine("Assomer", "Assome un joueur pour qu'il oublie les 10 dernières minutes.", ItemUtils.GetIconIdByItemId(126), delegate
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
            panel.AddButton("Fermer", delegate
            {
                player.ClosePanel(panel);
            });
            panel.AddButton("Sélectionner", delegate
            {
                panel.SelectTab();
            });
            player.ShowPanelUI(panel);
        }

        private bool CheckActionCooldown(Player player)
        {
            string key = player.steamId.ToString();
            if (actionCooldowns.TryGetValue(key, out var value))
            {
                TimeSpan timeSpan = TimeSpan.FromSeconds(15.0) - (DateTime.Now - value);
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
            panel.AddButton("Refuser", delegate
            {
                target.ClosePanel(panel);
                player.Notify("Refus", "L'individu a refusé", NotificationManager.Type.Error);
            });
            panel.AddButton("Accepter", delegate
            {
                target.ClosePanel(panel);
                AppliquerFouille(player, target);
            });
            target.ShowPanelUI(panel);
        }

        private async void AppliquerFouille(Player fouilleur, Player cible)
        {
            fouilleur.Notify("En cours", "Vous fouillez " + cible.character.Firstname + " " + cible.character.Lastname);
            cible.Notify("En cours", "Vous êtes en train d'être fouillé");
            await Task.Delay(5000);
            fouilleur.setup.TargetOpenPlayerInventory(cible.netId);
            fouilleur.Notify("Succès", "Fouille terminée", NotificationManager.Type.Success);
        }

        private void Voler(Player player, Player target)
        {
            if (target.Health <= 0)
            {
                player.Notify("Erreur", "Impossible de voler une personne inconsciente", NotificationManager.Type.Error);
                return;
            }

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
            if (target.Health <= 0)
            {
                Task.Delay(500).ContinueWith(delegate
                {
                    target.setup.player.IsRestrain = !target.setup.player.IsRestrain;
                    string text = (target.setup.player.IsRestrain ? "attaché" : "détaché");
                    player.Notify("Action", "Vous avez " + text + " la personne", NotificationManager.Type.Success);
                });
                return;
            }

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

        private async void AppliquerAttachement(Player attacheur, Player cible)
        {
            attacheur.Notify("En cours", "Attachement en cours...", NotificationManager.Type.Info, 3f);
            cible.Notify("En cours", "Vous êtes en train d'être attaché", NotificationManager.Type.Info, 3f);
            await Task.Delay(1500);
            cible.setup.player.IsRestrain = !cible.setup.player.IsRestrain;
            string action = (cible.setup.player.IsRestrain ? "attaché" : "détaché");
            attacheur.Notify("Succès", "Vous avez " + action + " " + cible.character.Firstname + " " + cible.character.Lastname, NotificationManager.Type.Success);
            cible.Notify("Action", "Vous avez été " + action, NotificationManager.Type.Warning);
        }

        private async void Detacher(Player player, Player target)
        {
            player.Notify("En cours", "Détachement en cours...", NotificationManager.Type.Info, 3f);
            target.Notify("En cours", "On vous détache", NotificationManager.Type.Info, 3f);
            await Task.Delay(3000);
            target.setup.player.IsRestrain = false;
            player.Notify("Succès", "Vous avez détaché " + target.character.Firstname + " " + target.character.Lastname, NotificationManager.Type.Success);
            target.Notify("Détaché", "Vous avez été détaché", NotificationManager.Type.Success);
        }

        private void Masquer(Player player, Player target)
        {
            if (target.Health <= 0)
            {
                target.setup.TargetShowCenterText("<color=#000000>YEUX BANDÉS</color>", "Vous avez les yeux bandés...", 30f);
                player.Notify("Action", "Vous avez masqué la personne", NotificationManager.Type.Success);
                return;
            }

            UIPanel panel = new UIPanel("<color=#9C27B0>Demande de masque</color>", UIPanel.PanelType.Text);
            panel.SetText("Une personne souhaite vous masquer.\n\nAcceptez-vous ?");
            panel.AddButton("Refuser", delegate
            {
                target.ClosePanel(panel);
                player.Notify("Refus", "L'individu a refusé", NotificationManager.Type.Error);
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
