#region assembly NLServiceCasino, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\paull\Downloads\NLServiceCasino.dll
// Decompiled with ICSharpCode.Decompiler 8.2.0.7535
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Life;
using Life.CheckpointSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using Mirror;
using Newtonsoft.Json;
using UnityEngine;

namespace NLServiceCasino;

public class NLServiceCasino : Plugin
{
    public class Config
    {
        public List<SerializableVector3> CasinoPositions { get; set; }

        public int MinBet { get; set; }

        public int MaxBet { get; set; }

        public bool EnableDiscordLogs { get; set; }

        public string DiscordWebhook { get; set; }
    }

    public class SerializableVector3
    {
        public float X { get; set; }

        public float Y { get; set; }

        public float Z { get; set; }

        public SerializableVector3()
        {
        }

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    private Config config;

    private string configPath;

    private readonly Dictionary<ulong, DateTime> lastPlayTime = new Dictionary<ulong, DateTime>();

    private readonly Dictionary<ulong, int> dailyWinnings = new Dictionary<ulong, int>();

    private readonly Dictionary<ulong, int> dailyLosses = new Dictionary<ulong, int>();

    public NLServiceCasino(IGameAPI api)
        : base(api)
    {
    }

    public override void OnPluginInit()
    {
        base.OnPluginInit();
        configPath = Path.Combine(pluginsPath, "NLServiceCasino", "NLServiceCasino.json");
        LoadConfig();
        new SChatCommand("/setcasino", "Définir la position du casino", "/setcasino", delegate (Player player, string[] args)
        {
            if (player?.setup?.transform == null)
            {
                return;
            }

            Vector3 position = player.setup.transform.position;
            if (config.CasinoPositions == null)
            {
                config.CasinoPositions = new List<SerializableVector3>();
            }

            config.CasinoPositions.Add(new SerializableVector3(position.x, position.y, position.z));
            SaveConfig();
            player.Notify("CASINO", $"Position {config.CasinoPositions.Count} ajoutée: X:{position.x:F1} Y:{position.y:F1} Z:{position.z:F1}", NotificationManager.Type.Success);
            foreach (Player player in Nova.server.Players)
            {
                if (player != null)
                {
                    CreateCasinoCheckpoint(player);
                }
            }
        }).Register();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("  NLSERVICE CASINO SYSTEM LOADED     ");
        Console.ResetColor();
    }

    public override void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
    {
        base.OnPlayerSpawnCharacter(player, conn, character);
        CreateCasinoCheckpoint(player);
    }

    private void CreateCasinoCheckpoint(Player player)
    {
        if (player?.setup == null || config.CasinoPositions == null)
        {
            return;
        }

        foreach (SerializableVector3 casinoPosition in config.CasinoPositions)
        {
            NCheckpoint checkpoint = new NCheckpoint(position: new Vector3(casinoPosition.X, casinoPosition.Y, casinoPosition.Z), playerId: player.netId, enterAction: delegate
            {
                ShowCasinoMainMenu(player);
            });
            player.CreateCheckpoint(checkpoint);
        }
    }

    private void ShowCasinoMainMenu(Player player)
    {
        if (player?.character != null)
        {
            int num = (dailyWinnings.ContainsKey(player.steamId) ? dailyWinnings[player.steamId] : 0);
            int num2 = (dailyLosses.ContainsKey(player.steamId) ? dailyLosses[player.steamId] : 0);
            UIPanel panel = new UIPanel("<b><color=#FFD700>C</color><color=#FFC700>A</color><color=#FFB700>S</color><color=#FFA700>I</color><color=#FF9700>N</color><color=#FF8700>O</color> <color=#FF7700>L</color><color=#FF6700>U</color><color=#FF5700>X</color><color=#FF4700>E</color></b> <size=9><color=#888888>By NLService</color></size>", UIPanel.PanelType.TabPrice);
            panel.AddTabLine("<b><color=#FF0000>M</color><color=#FF1A00>A</color><color=#FF3300>C</color><color=#FF4D00>H</color><color=#FF6600>I</color><color=#FF8000>N</color><color=#FF9900>E</color><color=#FFB300> </color><color=#FFCC00>A</color><color=#FFE600> </color><color=#FFFF00>S</color><color=#E6FF00>O</color><color=#CCFF00>U</color><color=#B3FF00>S</color></b>", "<size=11><color=#FFD700>Jackpot progressif</color></size>", ItemUtils.GetIconIdByItemId(2008), delegate
            {
                player.ClosePanel(panel);
                ShowSlotMachineMenu(player);
            });
            panel.AddTabLine("<b><color=#1E90FF>R</color><color=#2E8FFF>O</color><color=#3E8EFF>U</color><color=#4E8DFF>L</color><color=#5E8CFF>E</color><color=#6E8BFF>T</color><color=#7E8AFF>T</color><color=#8E89FF>E</color></b>", "<size=11><color=#FFD700>Rouge ou Noir</color></size>", ItemUtils.GetIconIdByItemId(1328), delegate
            {
                player.ClosePanel(panel);
                ShowRouletteMenu(player);
            });
            panel.AddTabLine("<b><color=#000000>B</color><color=#0D0D0D>L</color><color=#1A1A1A>A</color><color=#262626>C</color><color=#333333>K</color><color=#404040>J</color><color=#4D4D4D>A</color><color=#595959>C</color><color=#666666>K</color></b>", "<size=11><color=#FFD700>Bats le croupier</color></size>", ItemUtils.GetIconIdByItemId(1331), delegate
            {
                player.ClosePanel(panel);
                ShowBlackjackMenu(player);
            });
            panel.AddTabLine("<b><color=#32CD32>P</color><color=#3FD731>I</color><color=#4CE130>L</color><color=#59EB2F>E</color><color=#66F52E> </color><color=#73FF2D>O</color><color=#80FF2C>U</color><color=#8DFF2B> </color><color=#9AFF2A>F</color><color=#A7FF29>A</color><color=#B4FF28>C</color><color=#C1FF27>E</color></b>", "<size=11><color=#FFD700>50/50 rapide</color></size>", ItemUtils.GetIconIdByItemId(1329), delegate
            {
                player.ClosePanel(panel);
                ShowCoinFlipMenu(player);
            });
            panel.AddTabLine("<b><color=#FF1493>D</color><color=#FF2E9E>E</color><color=#FF47A9>S</color><color=#FF61B4> </color><color=#FF7ABF>M</color><color=#FF94CA>A</color><color=#FFADD5>G</color><color=#FFC7E0>I</color><color=#FFE0EB>Q</color><color=#FFFAF6>U</color><color=#FFFFFF>E</color><color=#FFFFFF>S</color></b>", "<size=11><color=#FFD700>Hasard pur</color></size>", ItemUtils.GetIconIdByItemId(1330), delegate
            {
                player.ClosePanel(panel);
                ShowDiceMenu(player);
            });
            panel.AddButton("<b><color=red>Quitter</color></b>", delegate
            {
                player.ClosePanel(panel);
            });
            panel.AddButton("<b><color=green>Jouer</color></b>", delegate
            {
                panel.SelectTab();
            });
            player.ShowPanelUI(panel);
        }
    }

    private void ShowSlotMachineMenu(Player player)
    {
        if (player?.character == null)
        {
            return;
        }

        UIPanel panel = new UIPanel("<b><color=#FFD700>MACHINE A SOUS</color></b> <size=8><color=#888888>NLService Casino</color></size>", UIPanel.PanelType.Input);
        panel.SetText($"<color=#FFFFFF>Votre argent:</color> <color=#00FF00>{player.character.Money:N0}$</color>\n\n" + $"<color=#FFFFFF>Mise (entre {config.MinBet}$ et {config.MaxBet}$):</color>");
        panel.inputPlaceholder = "Montant de la mise";
        panel.AddButton("<b><color=red>Retour</color></b>", delegate
        {
            player.ClosePanel(panel);
            ShowCasinoMainMenu(player);
        });
        panel.AddButton("<b><color=#FFD700>TOURNER</color></b>", delegate
        {
            if (int.TryParse(panel.inputText, out var result) && result >= config.MinBet && result <= config.MaxBet)
            {
                player.ClosePanel(panel);
                PlaySlotMachine(player, result);
            }
            else
            {
                player.Notify("CASINO", $"Mise invalide ! ({config.MinBet}$ - {config.MaxBet}$)", NotificationManager.Type.Error);
            }
        });
        player.ShowPanelUI(panel);
    }

    private void PlaySlotMachine(Player player, int bet)
    {
        if (player.character.Money < (double)bet)
        {
            player.Notify("CASINO", "Pas assez d'argent !", NotificationManager.Type.Error);
            ShowSlotMachineMenu(player);
            return;
        }

        player.character.Money -= bet;
        player.character.Save();
        string[] array = new string[7] { "1", "2", "3", "4", "5", "6", "7" };
        int num = UnityEngine.Random.Range(0, array.Length);
        int num2 = UnityEngine.Random.Range(0, array.Length);
        int num3 = UnityEngine.Random.Range(0, array.Length);
        int num4 = 0;
        string text = "";
        if (array[num] == "7\ufe0f" && array[num2] == "7\ufe0f" && array[num3] == "7\ufe0f")
        {
            num4 = bet * 50;
            text = "<color=#FFD700>JACKPOT 777 !!!</color>";
        }
        else if (num == num2 && num2 == num3)
        {
            num4 = bet * 10;
            text = "<color=#00FF00>TRIO GAGNANT !</color>";
        }
        else if (num == num2 || num2 == num3 || num == num3)
        {
            num4 = bet * 2;
            text = "<color=#FFA500>PAIRE !</color>";
        }
        else
        {
            text = "<color=#FF0000>PERDU</color>";
        }

        UIPanel resultPanel = new UIPanel("<b><color=#FFD700>RESULTAT</color></b>", UIPanel.PanelType.Text);
        string text2 = "<b><size=20>" + array[num] + " " + array[num2] + " " + array[num3] + "</size></b>\n\n" + text + "\n\n" + $"<color=#FFFFFF>Mise:</color> <color=#FF0000>-{bet:N0}$</color>\n";
        if (num4 > 0)
        {
            player.character.Money += num4;
            player.character.Save();
            text2 += $"<color=#FFFFFF>Gain:</color> <color=#00FF00>+{num4:N0}$</color>\n";
            text2 += $"<color=#FFFFFF>Profit:</color> <color=#00FF00>+{num4 - bet:N0}$</color>";
            TrackWinnings(player.steamId, num4 - bet);
            LogGame(player, "SlotMachine", bet, num4 - bet);
        }
        else
        {
            text2 += "<color=#FFFFFF>Gain:</color> <color=#FF0000>0$</color>";
            TrackLosses(player.steamId, bet);
            LogGame(player, "SlotMachine", bet, -bet);
        }

        resultPanel.SetText(text2);
        resultPanel.AddButton("<b><color=#FFD700>Rejouer</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowSlotMachineMenu(player);
        });
        resultPanel.AddButton("<b><color=red>Quitter</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowCasinoMainMenu(player);
        });
        player.ShowPanelUI(resultPanel);
    }

    private void ShowRouletteMenu(Player player)
    {
        if (player?.character != null)
        {
            UIPanel panel = new UIPanel("<b><color=#1E90FF>ROULETTE</color></b> <size=8><color=#888888>NLService Casino</color></size>", UIPanel.PanelType.TabPrice);
            panel.AddTabLine("<b><color=#FF0000>ROUGE</color></b>", "<size=11><color=#FFD700>x2 - 50% de chances</color></size>", ItemUtils.GetIconIdByItemId(1327), delegate
            {
                player.ClosePanel(panel);
                ShowRouletteInputMenu(player, "ROUGE");
            });
            panel.AddTabLine("<b><color=#000000>NOIR</color></b>", "<size=11><color=#FFD700>x2 - 50% de chances</color></size>", ItemUtils.GetIconIdByItemId(1331), delegate
            {
                player.ClosePanel(panel);
                ShowRouletteInputMenu(player, "NOIR");
            });
            panel.AddTabLine("<b><color=#00FF00>VERT (0)</color></b>", "<size=11><color=#FFD700>x14 - 7% de chances</color></size>", ItemUtils.GetIconIdByItemId(1330), delegate
            {
                player.ClosePanel(panel);
                ShowRouletteInputMenu(player, "VERT");
            });
            panel.AddButton("<b><color=red>Retour</color></b>", delegate
            {
                player.ClosePanel(panel);
                ShowCasinoMainMenu(player);
            });
            panel.AddButton("<b><color=green>Choisir</color></b>", delegate
            {
                panel.SelectTab();
            });
            player.ShowPanelUI(panel);
        }
    }

    private void ShowRouletteInputMenu(Player player, string choice)
    {
        UIPanel panel = new UIPanel("<b><color=#1E90FF>ROULETTE - " + choice + "</color></b>", UIPanel.PanelType.Input);
        panel.SetText($"<color=#FFFFFF>Votre argent:</color> <color=#00FF00>{player.character.Money:N0}$</color>\n\n" + $"<color=#FFFFFF>Mise (entre {config.MinBet}$ et {config.MaxBet}$):</color>");
        panel.inputPlaceholder = "Montant de la mise";
        panel.AddButton("<b><color=red>Retour</color></b>", delegate
        {
            player.ClosePanel(panel);
            ShowRouletteMenu(player);
        });
        panel.AddButton("<b><color=#1E90FF>LANCER</color></b>", delegate
        {
            if (int.TryParse(panel.inputText, out var result) && result >= config.MinBet && result <= config.MaxBet)
            {
                player.ClosePanel(panel);
                PlayRoulette(player, result, choice);
            }
            else
            {
                player.Notify("CASINO", $"Mise invalide ! ({config.MinBet}$ - {config.MaxBet}$)", NotificationManager.Type.Error);
            }
        });
        player.ShowPanelUI(panel);
    }

    private void PlayRoulette(Player player, int bet, string choice)
    {
        if (player.character.Money < (double)bet)
        {
            player.Notify("CASINO", "Pas assez d'argent !", NotificationManager.Type.Error);
            ShowRouletteMenu(player);
            return;
        }

        player.character.Money -= bet;
        player.character.Save();
        int num = UnityEngine.Random.Range(0, 100);
        string text = "";
        int num2 = 0;
        text = ((num < 7) ? "VERT" : ((num >= 53) ? "NOIR" : "ROUGE"));
        string text2 = ((text == "ROUGE") ? "#FF0000" : ((text == "NOIR") ? "#000000" : "#00FF00"));
        string text3 = "";
        if (text == choice)
        {
            if (choice == "VERT")
            {
                num2 = bet * 14;
                text3 = "<color=#00FF00>JACKPOT VERT x14 !!!</color>";
            }
            else
            {
                num2 = bet * 2;
                text3 = "<color=" + text2 + ">GAGNE x2 !</color>";
            }
        }
        else
        {
            text3 = "<color=#FF0000>PERDU</color>";
        }

        UIPanel resultPanel = new UIPanel("<b><color=#1E90FF>RESULTAT ROULETTE</color></b>", UIPanel.PanelType.Text);
        string text4 = "<b><size=18>La bille s'arrête sur...</size></b>\n<b><size=24><color=" + text2 + ">" + text + "</color></size></b>\n\n" + text3 + "\n\n<color=#FFFFFF>Votre choix:</color> <b>" + choice + "</b>\n" + $"<color=#FFFFFF>Mise:</color> <color=#FF0000>-{bet:N0}$</color>\n";
        if (num2 > 0)
        {
            player.character.Money += num2;
            player.character.Save();
            text4 += $"<color=#FFFFFF>Gain:</color> <color=#00FF00>+{num2:N0}$</color>\n";
            text4 += $"<color=#FFFFFF>Profit:</color> <color=#00FF00>+{num2 - bet:N0}$</color>";
            TrackWinnings(player.steamId, num2 - bet);
            LogGame(player, "Roulette", bet, num2 - bet);
        }
        else
        {
            text4 += "<color=#FFFFFF>Gain:</color> <color=#FF0000>0$</color>";
            TrackLosses(player.steamId, bet);
            LogGame(player, "Roulette", bet, -bet);
        }

        resultPanel.SetText(text4);
        resultPanel.AddButton("<b><color=#1E90FF>Rejouer</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowRouletteMenu(player);
        });
        resultPanel.AddButton("<b><color=red>Quitter</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowCasinoMainMenu(player);
        });
        player.ShowPanelUI(resultPanel);
    }

    private void ShowBlackjackMenu(Player player)
    {
        if (player?.character == null)
        {
            return;
        }

        UIPanel panel = new UIPanel("<b><color=#000000>BLACKJACK</color></b> <size=8><color=#888888>NLService Casino</color></size>", UIPanel.PanelType.Input);
        panel.SetText($"<color=#FFFFFF>Votre argent:</color> <color=#00FF00>{player.character.Money:N0}$</color>\n\n" + $"<color=#FFFFFF>Mise (entre {config.MinBet}$ et {config.MaxBet}$):</color>");
        panel.inputPlaceholder = "Montant de la mise";
        panel.AddButton("<b><color=red>Retour</color></b>", delegate
        {
            player.ClosePanel(panel);
            ShowCasinoMainMenu(player);
        });
        panel.AddButton("<b><color=#000000>JOUER</color></b>", delegate
        {
            if (int.TryParse(panel.inputText, out var result) && result >= config.MinBet && result <= config.MaxBet)
            {
                player.ClosePanel(panel);
                PlayBlackjack(player, result);
            }
            else
            {
                player.Notify("CASINO", $"Mise invalide ! ({config.MinBet}$ - {config.MaxBet}$)", NotificationManager.Type.Error);
            }
        });
        player.ShowPanelUI(panel);
    }

    private void PlayBlackjack(Player player, int bet)
    {
        if (player.character.Money < (double)bet)
        {
            player.Notify("CASINO", "Pas assez d'argent !", NotificationManager.Type.Error);
            ShowBlackjackMenu(player);
            return;
        }

        player.character.Money -= bet;
        player.character.Save();
        int num = UnityEngine.Random.Range(16, 26);
        int num2 = UnityEngine.Random.Range(16, 26);
        int num3 = 0;
        string text = "";
        if (num > 21)
        {
            text = "<color=#FF0000>VOUS AVEZ DEPASSE 21 !</color>";
        }
        else if (num2 > 21)
        {
            num3 = bet * 2;
            text = "<color=#00FF00>LE CROUPIER A DEPASSE 21 !</color>";
        }
        else if (num == 21)
        {
            num3 = bet * 3;
            text = "<color=#FFD700>BLACKJACK ! x3</color>";
        }
        else if (num > num2)
        {
            num3 = bet * 2;
            text = "<color=#00FF00>VOUS GAGNEZ !</color>";
        }
        else if (num == num2)
        {
            num3 = bet;
            text = "<color=#FFA500>EGALITE - REMBOURSEMENT</color>";
        }
        else
        {
            text = "<color=#FF0000>LE CROUPIER GAGNE</color>";
        }

        UIPanel resultPanel = new UIPanel("<b><color=#000000>RESULTAT BLACKJACK</color></b>", UIPanel.PanelType.Text);
        string text2 = "<b><size=16>BLACKJACK</size></b>\n\n" + string.Format("<color=#FFFFFF>Votre score:</color> <b><size=18><color={0}>{1}</color></size></b>\n", (num <= 21) ? "#00FF00" : "#FF0000", num) + string.Format("<color=#FFFFFF>Score croupier:</color> <b><size=18><color={0}>{1}</color></size></b>\n\n", (num2 <= 21) ? "#FFA500" : "#FF0000", num2) + text + "\n\n" + $"<color=#FFFFFF>Mise:</color> <color=#FF0000>-{bet:N0}$</color>\n";
        if (num3 > 0)
        {
            player.character.Money += num3;
            player.character.Save();
            text2 += $"<color=#FFFFFF>Gain:</color> <color=#00FF00>+{num3:N0}$</color>\n";
            text2 += $"<color=#FFFFFF>Profit:</color> <color=#00FF00>+{num3 - bet:N0}$</color>";
            TrackWinnings(player.steamId, num3 - bet);
            LogGame(player, "Blackjack", bet, num3 - bet);
        }
        else
        {
            text2 += "<color=#FFFFFF>Gain:</color> <color=#FF0000>0$</color>";
            TrackLosses(player.steamId, bet);
            LogGame(player, "Blackjack", bet, -bet);
        }

        resultPanel.SetText(text2);
        resultPanel.AddButton("<b><color=#000000>Rejouer</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowBlackjackMenu(player);
        });
        resultPanel.AddButton("<b><color=red>Quitter</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowCasinoMainMenu(player);
        });
        player.ShowPanelUI(resultPanel);
    }

    private void ShowCoinFlipMenu(Player player)
    {
        if (player?.character != null)
        {
            UIPanel panel = new UIPanel("<b><color=#32CD32>PILE OU FACE</color></b> <size=8><color=#888888>NLService Casino</color></size>", UIPanel.PanelType.TabPrice);
            panel.AddTabLine("<b><color=#FFD700>PILE</color></b>", "<size=11><color=#FFD700>50% de chances</color></size>", ItemUtils.GetIconIdByItemId(1330), delegate
            {
                player.ClosePanel(panel);
                ShowCoinFlipInputMenu(player, "PILE");
            });
            panel.AddTabLine("<b><color=#C0C0C0>FACE</color></b>", "<size=11><color=#FFD700>50% de chances</color></size>", ItemUtils.GetIconIdByItemId(1331), delegate
            {
                player.ClosePanel(panel);
                ShowCoinFlipInputMenu(player, "FACE");
            });
            panel.AddButton("<b><color=red>Retour</color></b>", delegate
            {
                player.ClosePanel(panel);
                ShowCasinoMainMenu(player);
            });
            panel.AddButton("<b><color=green>Choisir</color></b>", delegate
            {
                panel.SelectTab();
            });
            player.ShowPanelUI(panel);
        }
    }

    private void ShowCoinFlipInputMenu(Player player, string choice)
    {
        UIPanel panel = new UIPanel("<b><color=#32CD32>PILE OU FACE - " + choice + "</color></b>", UIPanel.PanelType.Input);
        panel.SetText($"<color=#FFFFFF>Votre argent:</color> <color=#00FF00>{player.character.Money:N0}$</color>\n\n" + $"<color=#FFFFFF>Mise (entre {config.MinBet}$ et {config.MaxBet}$):</color>");
        panel.inputPlaceholder = "Montant de la mise";
        panel.AddButton("<b><color=red>Retour</color></b>", delegate
        {
            player.ClosePanel(panel);
            ShowCoinFlipMenu(player);
        });
        panel.AddButton("<b><color=#32CD32>LANCER</color></b>", delegate
        {
            if (int.TryParse(panel.inputText, out var result) && result >= config.MinBet && result <= config.MaxBet)
            {
                player.ClosePanel(panel);
                PlayCoinFlip(player, result, choice);
            }
            else
            {
                player.Notify("CASINO", $"Mise invalide ! ({config.MinBet}$ - {config.MaxBet}$)", NotificationManager.Type.Error);
            }
        });
        player.ShowPanelUI(panel);
    }

    private void PlayCoinFlip(Player player, int bet, string choice)
    {
        if (player.character.Money < (double)bet)
        {
            player.Notify("CASINO", "Pas assez d'argent !", NotificationManager.Type.Error);
            ShowCoinFlipMenu(player);
            return;
        }

        player.character.Money -= bet;
        player.character.Save();
        string text = ((UnityEngine.Random.Range(0, 2) == 0) ? "PILE" : "FACE");
        int num = 0;
        string text2 = "";
        if (text == choice)
        {
            num = bet * 2;
            text2 = "<color=#00FF00>GAGNE !</color>";
        }
        else
        {
            text2 = "<color=#FF0000>PERDU</color>";
        }

        string text3 = ((text == "PILE") ? "1" : "2");
        UIPanel resultPanel = new UIPanel("<b><color=#32CD32>RESULTAT PILE OU FACE</color></b>", UIPanel.PanelType.Text);
        string text4 = "<b><size=20>" + text3 + " " + text + "</size></b>\n\n" + text2 + "\n\n<color=#FFFFFF>Votre choix:</color> <b>" + choice + "</b>\n" + $"<color=#FFFFFF>Mise:</color> <color=#FF0000>-{bet:N0}$</color>\n";
        if (num > 0)
        {
            player.character.Money += num;
            player.character.Save();
            text4 += $"<color=#FFFFFF>Gain:</color> <color=#00FF00>+{num:N0}$</color>\n";
            text4 += $"<color=#FFFFFF>Profit:</color> <color=#00FF00>+{num - bet:N0}$</color>";
            TrackWinnings(player.steamId, num - bet);
            LogGame(player, "CoinFlip", bet, num - bet);
        }
        else
        {
            text4 += "<color=#FFFFFF>Gain:</color> <color=#FF0000>0$</color>";
            TrackLosses(player.steamId, bet);
            LogGame(player, "CoinFlip", bet, -bet);
        }

        resultPanel.SetText(text4);
        resultPanel.AddButton("<b><color=#32CD32>Rejouer</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowCoinFlipMenu(player);
        });
        resultPanel.AddButton("<b><color=red>Quitter</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowCasinoMainMenu(player);
        });
        player.ShowPanelUI(resultPanel);
    }

    private void ShowDiceMenu(Player player)
    {
        if (player?.character == null)
        {
            return;
        }

        UIPanel panel = new UIPanel("<b><color=#FF1493>DES MAGIQUES</color></b> <size=8><color=#888888>NLService Casino</color></size>", UIPanel.PanelType.Input);
        panel.SetText($"<color=#FFFFFF>Votre argent:</color> <color=#00FF00>{player.character.Money:N0}$</color>\n\n" + $"<color=#FFFFFF>Mise (entre {config.MinBet}$ et {config.MaxBet}$):</color>");
        panel.inputPlaceholder = "Montant de la mise";
        panel.AddButton("<b><color=red>Retour</color></b>", delegate
        {
            player.ClosePanel(panel);
            ShowCasinoMainMenu(player);
        });
        panel.AddButton("<b><color=#FF1493>LANCER</color></b>", delegate
        {
            if (int.TryParse(panel.inputText, out var result) && result >= config.MinBet && result <= config.MaxBet)
            {
                player.ClosePanel(panel);
                PlayDice(player, result);
            }
            else
            {
                player.Notify("CASINO", $"Mise invalide ! ({config.MinBet}$ - {config.MaxBet}$)", NotificationManager.Type.Error);
            }
        });
        player.ShowPanelUI(panel);
    }

    private void PlayDice(Player player, int bet)
    {
        if (player.character.Money < (double)bet)
        {
            player.Notify("CASINO", "Pas assez d'argent !", NotificationManager.Type.Error);
            ShowDiceMenu(player);
            return;
        }

        player.character.Money -= bet;
        player.character.Save();
        int num = UnityEngine.Random.Range(1, 7);
        int num2 = UnityEngine.Random.Range(1, 7);
        int num3 = num + num2;
        int num4 = 0;
        string text = "";
        if (num == 6 && num2 == 6)
        {
            num4 = bet * 10;
            text = "<color=#FFD700>DOUBLE 6 ! JACKPOT x10 !!!</color>";
        }
        else if (num == num2)
        {
            num4 = bet * 5;
            text = "<color=#00FF00>DOUBLE ! x5</color>";
        }
        else if (num3 > 7)
        {
            num4 = bet * 2;
            text = "<color=#FFA500>Plus de 7 ! x2</color>";
        }
        else
        {
            text = "<color=#FF0000>PERDU</color>";
        }

        UIPanel resultPanel = new UIPanel("<b><color=#FF1493>RESULTAT DES</color></b>", UIPanel.PanelType.Text);
        string text2 = $"<b><size=18>■ {num} + ■ {num2} = {num3}</size></b>\n\n" + text + "\n\n" + $"<color=#FFFFFF>Mise:</color> <color=#FF0000>-{bet:N0}$</color>\n";
        if (num4 > 0)
        {
            player.character.Money += num4;
            player.character.Save();
            text2 += $"<color=#FFFFFF>Gain:</color> <color=#00FF00>+{num4:N0}$</color>\n";
            text2 += $"<color=#FFFFFF>Profit:</color> <color=#00FF00>+{num4 - bet:N0}$</color>";
            TrackWinnings(player.steamId, num4 - bet);
            LogGame(player, "Dice", bet, num4 - bet);
        }
        else
        {
            text2 += "<color=#FFFFFF>Gain:</color> <color=#FF0000>0$</color>";
            TrackLosses(player.steamId, bet);
            LogGame(player, "Dice", bet, -bet);
        }

        resultPanel.SetText(text2);
        resultPanel.AddButton("<b><color=#FF1493>Rejouer</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowDiceMenu(player);
        });
        resultPanel.AddButton("<b><color=red>Quitter</color></b>", delegate
        {
            player.ClosePanel(resultPanel);
            ShowCasinoMainMenu(player);
        });
        player.ShowPanelUI(resultPanel);
    }

    private void TrackWinnings(ulong steamId, int amount)
    {
        if (!dailyWinnings.ContainsKey(steamId))
        {
            dailyWinnings[steamId] = 0;
        }

        dailyWinnings[steamId] += amount;
    }

    private void TrackLosses(ulong steamId, int amount)
    {
        if (!dailyLosses.ContainsKey(steamId))
        {
            dailyLosses[steamId] = 0;
        }

        dailyLosses[steamId] += amount;
    }

    private void LogGame(Player player, string gameName, int bet, int profit)
    {
        if (player?.character != null)
        {
            string arg = $"[CASINO] {gameName} | Player: {player.character.Firstname} {player.character.Lastname} ({player.steamId}) | Bet: {bet}$ | Profit: {profit}$";
            Console.ForegroundColor = ((profit > 0) ? ConsoleColor.Green : ConsoleColor.Red);
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {arg}");
            Console.ResetColor();
            string path = Path.Combine(pluginsPath, "NLServiceCasino", "casino.log");
            try
            {
                File.AppendAllText(path, $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {arg}\n");
            }
            catch
            {
            }

            if (config.EnableDiscordLogs && (profit >= 50000 || profit <= -50000))
            {
                SendDiscordLog(player, gameName, bet, profit);
            }
        }
    }

    private async void SendDiscordLog(Player player, string gameName, int bet, int profit)
    {
        if (string.IsNullOrEmpty(config.DiscordWebhook) || config.DiscordWebhook == "VOTRE_WEBHOOK_ICI")
        {
            return;
        }

        try
        {
            var embed = new
            {
                embeds = new[]
                {
                    new
                    {
                        title = ((profit > 0) ? " GROS GAIN AU CASINO" : " GROSSE PERTE AU CASINO"),
                        color = ((profit > 0) ? 3066993 : 15158332),
                        fields = new[]
                        {
                            new
                            {
                                name = "Joueur",
                                value = player.character.Firstname + " " + player.character.Lastname,
                                inline = true
                            },
                            new
                            {
                                name = "Jeu",
                                value = gameName,
                                inline = true
                            },
                            new
                            {
                                name = "Mise",
                                value = $"{bet:N0}$",
                                inline = true
                            },
                            new
                            {
                                name = "Profit",
                                value = $"{profit:N0}$",
                                inline = true
                            }
                        },
                        timestamp = DateTime.UtcNow.ToString("o"),
                        footer = new
                        {
                            text = "NLService Casino"
                        }
                    }
                }
            };
            string jsonPayload = JsonConvert.SerializeObject(embed);
            using HttpClient client = new HttpClient();
            await client.PostAsync(content: new StringContent(jsonPayload, Encoding.UTF8, "application/json"), requestUri: config.DiscordWebhook);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Casino] Erreur Discord: " + ex.Message);
        }
    }

    private void LoadConfig()
    {
        string path = Path.Combine(pluginsPath, "NLServiceCasino");
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        try
        {
            if (File.Exists(configPath))
            {
                string value = File.ReadAllText(configPath);
                config = JsonConvert.DeserializeObject<Config>(value);
                return;
            }

            config = new Config
            {
                CasinoPositions = new List<SerializableVector3>(),
                MinBet = 1000,
                MaxBet = 100000,
                EnableDiscordLogs = true,
                DiscordWebhook = "VOTRE_WEBHOOK_ICI"
            };
            SaveConfig();
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Casino] Erreur config: " + ex.Message);
            config = new Config
            {
                CasinoPositions = new List<SerializableVector3>(),
                MinBet = 1000,
                MaxBet = 100000,
                EnableDiscordLogs = true,
                DiscordWebhook = "VOTRE_WEBHOOK_ICI"
            };
        }
    }

    private void SaveConfig()
    {
        try
        {
            string contents = JsonConvert.SerializeObject(config, Formatting.Indented);
            File.WriteAllText(configPath, contents);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[Casino] Erreur sauvegarde config: " + ex.Message);
        }
    }
}
#if false // Journal de décompilation
'25' éléments dans le cache
------------------
Résoudre : 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Un seul assembly trouvé : 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Charger à partir de : 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll'
------------------
Résoudre : 'Assembly-CSharp, Version=1.4.2.5, Culture=neutral, PublicKeyToken=0e2080cf6d9dd5d5'
Un seul assembly trouvé : 'Assembly-CSharp, Version=1.4.2.5, Culture=neutral, PublicKeyToken=0e2080cf6d9dd5d5'
Charger à partir de : 'C:\Program Files (x86)\Steam\steamapps\common\Nova-Life\Nova-Life_Data\Managed\Assembly-CSharp.dll'
------------------
Résoudre : 'Mirror, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'Mirror, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'C:\Program Files (x86)\Steam\steamapps\common\Nova-Life\Nova-Life_Data\Managed\Mirror.dll'
------------------
Résoudre : 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'UnityEngine.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'C:\Program Files (x86)\Steam\steamapps\common\Nova-Life\Nova-Life_Data\Managed\UnityEngine.CoreModule.dll'
------------------
Résoudre : 'System.Net.Http, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Un seul assembly trouvé : 'System.Net.Http, Version=4.2.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a'
Charger à partir de : 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Net.Http.dll'
------------------
Résoudre : 'Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'
Un seul assembly trouvé : 'Newtonsoft.Json, Version=13.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed'
Charger à partir de : 'C:\Program Files (x86)\Steam\steamapps\common\Nova-Life\Nova-Life_Data\Managed\Newtonsoft.Json.dll'
------------------
Résoudre : 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Un seul assembly trouvé : 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Charger à partir de : 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Core.dll'
#endif
