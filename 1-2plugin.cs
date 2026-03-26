#region assembly MyPlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// C:\Users\paull\Downloads\AkCasino (1).dll
// Decompiled with ICSharpCode.Decompiler 8.2.0.7535
#endregion

using System;
using System.Linq;
using System.Reflection;
using Life;
using Life.CheckpointSystem;
using Life.DB;
using Life.Network;
using Life.UI;
using Mirror;
using ModKit.Utils;
using UnityEngine;

namespace NovaLifeCasino;

public class CasinoPlugin : Plugin
{
    private int JackpotAmount = 0;

    private readonly System.Random rng = new System.Random();

    public CasinoPlugin(IGameAPI api)
        : base(api)
    {
    }

    public override void OnPluginInit()
    {
        base.OnPluginInit();
        Debug.Log("[CasinoPlugin] Initialisation du plugin Casino");
    }

    public override void OnPlayerSpawnCharacter(Player player, NetworkConnection conn, Characters character)
    {
        base.OnPlayerSpawnCharacter(player, conn, character);
        CreateCasinoCheckpoint(player);
    }

    private void CreateCasinoCheckpoint(Player player)
    {
        NCheckpoint checkpoint = new NCheckpoint(position: new Vector3(334.4397f, 50.00305f, 823.7321f), playerId: player.netId, enterAction: delegate
        {
            OpenCasinoMenu(player);
        });
        try
        {
            player.CreateCheckpoint(checkpoint);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[CasinoPlugin] impossible de créer checkpoint: " + ex.Message);
        }
    }

    private void OpenCasinoMenu(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#00d9ff><b>CASINO <i><color=white>by AKSIL<i></b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>FERMER</color>", delegate (UIPanel ui)
        {
            SafeClosePanel(player, ui);
        });
        uIPanel.AddButton("<color=#78ff00>CONTINUER</color>", delegate (UIPanel ui)
        {
            ui.SelectTab();
        });
        uIPanel.AddTabLine("<color=#ffcc00><b>Machine à sous</b></color>", "<size=12>Tourner la machine</size>", ItemUtils.GetIconIdByItemId(58), delegate
        {
            ShowSlotsPanel(player);
        });
        uIPanel.AddTabLine("<color=#00ffaa><b>Pile ou Face</b></color>", "<size=12>Mise x2</size>", ItemUtils.GetIconIdByItemId(1327), delegate
        {
            ShowCoinflipPanel(player);
        });
        uIPanel.AddTabLine("<color=#ff4444><b>Roulette</b></color>", "<size=12>Rouge / Noir / Zones</size>", ItemUtils.GetIconIdByItemId(1569), delegate
        {
            ShowRoulettePanel(player);
        });
        uIPanel.AddTabLine("<color=#22ff66><b>Blackjack (simple)</b></color>", "<size=12>Rapide</size>", ItemUtils.GetIconIdByItemId(1280), delegate
        {
            ShowBlackjackPanel(player);
        });
        uIPanel.AddTabLine("<color=#aa66ff><b>Jeu de dés</b></color>", "<size=12>1 - 6</size>", ItemUtils.GetIconIdByItemId(1273), delegate
        {
            ShowDicePanel(player);
        });
        uIPanel.AddTabLine("<color=#ffaa00><b>Jackpot</b></color>", $"<size=12>Pot : {JackpotAmount}$</size>", ItemUtils.GetIconIdByItemId(1418), delegate
        {
            ShowJackpotPanel(player);
        });
        SafeShowPanel(player, uIPanel);
    }

    private void SafeShowPanel(Player player, UIPanel panel)
    {
        try
        {
            player.ShowPanelUI(panel);
        }
        catch (Exception ex)
        {
            Debug.LogError("[CasinoPlugin] ShowPanelUI failed: " + ex.Message);
        }
    }

    private void SafeClosePanel(Player player, UIPanel ui)
    {
        try
        {
            player.ClosePanel(ui);
        }
        catch
        {
        }
    }

    private void ShowSlotsPanel(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#ffcc00><b>Machine à sous</b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>Retour</color>", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
            OpenCasinoMenu(player);
        });
        uIPanel.AddButton("<color=#00ff00>Miser 100$</color>", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 100, PlaySlots);
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("<color=#00c8ff>Miser 500$</color>", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, PlaySlots);
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("<color=#008cff>Miser 1000$</color>", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 1000, PlaySlots);
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("<color=#ffd500>Mise perso : utiliser /mise &lt;montant&gt;</color>", delegate (UIPanel ui)
        {
            player.SendText("<color=#ffd500>Utilise : /mise <montant></color>");
            player.ClosePanel(ui);
        });
        SafeShowPanel(player, uIPanel);
    }

    private void PlaySlots(Player player, int bet)
    {
        string[] array = new string[4] { "CHERRY", "STAR", "DIAMOND", "7" };
        string text = array[rng.Next(array.Length)];
        string text2 = array[rng.Next(array.Length)];
        string text3 = array[rng.Next(array.Length)];
        int num = 0;
        if (text == text2 && text2 == text3)
        {
            num = bet * 10;
        }
        else if (text == text2 || text == text3 || text2 == text3)
        {
            num = bet * 2;
        }

        if (num > 0)
        {
            AddMoney(player, num);
            SendPlayerText(player, $"<color=#00ff74>\ud83c\udf89 Slots : {text} | {text2} | {text3} → Gagné {num}$</color>");
        }
        else
        {
            SendPlayerText(player, "<color=red>Slots : " + text + " | " + text2 + " | " + text3 + " → Perdu</color>");
        }

        OpenCasinoMenu(player);
    }

    private void ShowCoinflipPanel(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#00ffaa><b>Pile ou Face</b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>Retour</color>", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
            OpenCasinoMenu(player);
        });
        uIPanel.AddButton("<color=#ffaa00>Miser 200$</color>", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 200, PlayCoinflip);
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("<color=#ffaa00>Miser 500$</color>", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, PlayCoinflip);
            player.ClosePanel(ui);
        });
        SafeShowPanel(player, uIPanel);
    }

    private void PlayCoinflip(Player player, int bet)
    {
        if (rng.Next(0, 2) == 0)
        {
            AddMoney(player, bet * 2);
            SendPlayerText(player, $"<color=#00ff74>Gagné ! Tu remportes {bet * 2}$</color>");
        }
        else
        {
            SendPlayerText(player, "<color=red>Perdu !</color>");
        }

        OpenCasinoMenu(player);
    }

    private void ShowRoulettePanel(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#ff4444><b>Roulette</b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>Retour</color>", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
            OpenCasinoMenu(player);
        });
        uIPanel.AddButton("1-12 (x3)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, delegate (Player pl, int b)
            {
                PlayRoulette(pl, b, "1-12");
            });
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("13-24 (x3)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, delegate (Player pl, int b)
            {
                PlayRoulette(pl, b, "13-24");
            });
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("25-36 (x3)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, delegate (Player pl, int b)
            {
                PlayRoulette(pl, b, "25-36");
            });
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("Rouge (x2)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, delegate (Player pl, int b)
            {
                PlayRoulette(pl, b, "red");
            });
            player.ClosePanel(ui);
        });
        uIPanel.AddButton("Noir (x2)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, delegate (Player pl, int b)
            {
                PlayRoulette(pl, b, "black");
            });
            player.ClosePanel(ui);
        });
        SafeShowPanel(player, uIPanel);
    }

    private void PlayRoulette(Player player, int bet, string betType)
    {
        int num = rng.Next(1, 37);
        bool flag = num % 2 == 0;
        bool flag2 = false;
        int num2 = 1;
        if (betType == "1-12" && num <= 12)
        {
            flag2 = true;
            num2 = 3;
        }
        else if (betType == "13-24" && num >= 13 && num <= 24)
        {
            flag2 = true;
            num2 = 3;
        }
        else if (betType == "25-36" && num >= 25 && num <= 36)
        {
            flag2 = true;
            num2 = 3;
        }
        else if (betType == "red" && flag)
        {
            flag2 = true;
            num2 = 2;
        }
        else if (betType == "black" && !flag)
        {
            flag2 = true;
            num2 = 2;
        }

        if (flag2)
        {
            AddMoney(player, bet * num2);
            SendPlayerText(player, $"<color=#00ff74>Roulette : {num} → Gagné {bet * num2}$</color>");
        }
        else
        {
            SendPlayerText(player, $"<color=red>Roulette : {num} → Perdu</color>");
        }

        OpenCasinoMenu(player);
    }

    private void ShowBlackjackPanel(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#22ff66><b>Blackjack (simple)</b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>Retour</color>", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
            OpenCasinoMenu(player);
        });
        uIPanel.AddButton("Jouer 500$", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, PlayBlackjackSimple);
            player.ClosePanel(ui);
        });
        SafeShowPanel(player, uIPanel);
    }

    private void PlayBlackjackSimple(Player player, int bet)
    {
        int num = rng.Next(2, 22);
        int num2 = rng.Next(2, 22);
        if (num > num2)
        {
            AddMoney(player, bet * 2);
            SendPlayerText(player, $"<color=#00ff74>Blackjack : {num} vs {num2} → GAGNÉ {bet * 2}$</color>");
        }
        else if (num == num2)
        {
            AddMoney(player, bet);
            SendPlayerText(player, $"<color=#ffd500>Blackjack : {num} vs {num2} → ÉGALITÉ (remboursement)</color>");
        }
        else
        {
            SendPlayerText(player, $"<color=red>Blackjack : {num} vs {num2} → PERDU</color>");
        }

        OpenCasinoMenu(player);
    }

    private void ShowDicePanel(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#aa66ff><b>Jeu de Dés</b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>Retour</color>", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
            OpenCasinoMenu(player);
        });
        for (int j = 1; j <= 6; j++)
        {
            int i = j;
            uIPanel.AddButton($"Choisir {i}", delegate (UIPanel ui)
            {
                TryPlaceBet(player, 200, delegate (Player pl, int bet)
                {
                    PlayDice(pl, bet, i);
                });
                player.ClosePanel(ui);
            });
        }

        SafeShowPanel(player, uIPanel);
    }

    private void PlayDice(Player player, int bet, int chosen)
    {
        int num = rng.Next(1, 7);
        if (num == chosen)
        {
            AddMoney(player, bet * 5);
            SendPlayerText(player, $"<color=#00ff74>Dés : {num} — GAGNÉ {bet * 5}$</color>");
        }
        else
        {
            SendPlayerText(player, $"<color=red>Dés : {num} — PERDU</color>");
        }

        OpenCasinoMenu(player);
    }

    private void ShowJackpotPanel(Player player)
    {
        UIPanel uIPanel = new UIPanel("<color=#ffaa00><b>Jackpot</b></color>", UIPanel.PanelType.TabPrice);
        uIPanel.AddButton("<color=red>Retour</color>", delegate (UIPanel ui)
        {
            player.ClosePanel(ui);
            OpenCasinoMenu(player);
        });
        uIPanel.AddButton($"Miser 100$ (Pot : {JackpotAmount}$)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 100, PlayJackpot);
            player.ClosePanel(ui);
        });
        uIPanel.AddButton($"Miser 500$ (Pot : {JackpotAmount}$)", delegate (UIPanel ui)
        {
            TryPlaceBet(player, 500, PlayJackpot);
            player.ClosePanel(ui);
        });
        SafeShowPanel(player, uIPanel);
    }

    private void PlayJackpot(Player player, int bet)
    {
        JackpotAmount += bet;
        if (rng.Next(0, 25) == 0 && JackpotAmount > 0)
        {
            int jackpotAmount = JackpotAmount;
            AddMoney(player, jackpotAmount);
            JackpotAmount = 0;
            SendPlayerText(player, $"<color=#00ff74>\ud83d\udc8e JACKPOT GAGNÉ ! Tu remportes {jackpotAmount}$</color>");
        }
        else
        {
            SendPlayerText(player, "<color=yellow>Tu as participé au Jackpot. Bonne chance !</color>");
        }

        OpenCasinoMenu(player);
    }

    private bool HasEnoughMoney(Player player, int amount)
    {
        long playerMoney = GetPlayerMoney(player);
        return playerMoney >= amount;
    }

    private long GetPlayerMoney(Player player)
    {
        try
        {
            object obj = TryGetPropertyOrField(player, "character") ?? TryGetPropertyOrField(player, "Character") ?? null;
            if (obj != null)
            {
                object obj2 = TryGetPropertyOrField(obj, "Money") ?? TryGetPropertyOrField(obj, "money");
                if (obj2 != null && long.TryParse(obj2.ToString(), out var result))
                {
                    return result;
                }

                MethodInfo method = obj.GetType().GetMethod("GetMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    object obj3 = method.Invoke(obj, null);
                    if (obj3 != null && long.TryParse(obj3.ToString(), out var result2))
                    {
                        return result2;
                    }
                }
            }

            object obj4 = TryGetPropertyOrField(player, "wallet") ?? TryGetPropertyOrField(player, "Wallet");
            if (obj4 != null)
            {
                object obj5 = TryGetPropertyOrField(obj4, "Money") ?? TryGetPropertyOrField(obj4, "money");
                if (obj5 != null && long.TryParse(obj5.ToString(), out var result3))
                {
                    return result3;
                }
            }

            MethodInfo method2 = player.GetType().GetMethod("GetMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (method2 != null)
            {
                object obj6 = method2.Invoke(player, null);
                if (obj6 != null && long.TryParse(obj6.ToString(), out var result4))
                {
                    return result4;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[CasinoPlugin] GetPlayerMoney reflection error: " + ex.Message);
        }

        return 0L;
    }

    private void AddMoney(Player player, long amount)
    {
        if (amount == 0)
        {
            return;
        }

        try
        {
            object obj = TryGetPropertyOrField(player, "character") ?? TryGetPropertyOrField(player, "Character");
            if (obj != null)
            {
                MethodInfo method = obj.GetType().GetMethod("AddMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    method.Invoke(obj, new object[1] { Convert.ChangeType(amount, method.GetParameters()[0].ParameterType) });
                    return;
                }

                object obj2 = TryGetPropertyOrField(obj, "Money") ?? TryGetPropertyOrField(obj, "money");
                if (obj2 != null)
                {
                    object obj3 = TryGetPropertyOrField(obj, "Money") ?? TryGetPropertyOrField(obj, "money");
                    if (obj3 != null && long.TryParse(obj3.ToString(), out var result))
                    {
                        SetPropertyOrField(obj, "Money", result + amount);
                        return;
                    }
                }
            }

            object obj4 = TryGetPropertyOrField(player, "wallet") ?? TryGetPropertyOrField(player, "Wallet");
            if (obj4 != null)
            {
                MethodInfo method2 = obj4.GetType().GetMethod("AddMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (method2 != null)
                {
                    method2.Invoke(obj4, new object[1] { Convert.ChangeType(amount, method2.GetParameters()[0].ParameterType) });
                    return;
                }

                object obj5 = TryGetPropertyOrField(obj4, "Money") ?? TryGetPropertyOrField(obj4, "money");
                if (obj5 != null && long.TryParse(obj5.ToString(), out var result2))
                {
                    SetPropertyOrField(obj4, "Money", result2 + amount);
                    return;
                }
            }

            MethodInfo method3 = player.GetType().GetMethod("AddMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (method3 != null)
            {
                method3.Invoke(player, new object[1] { Convert.ChangeType(amount, method3.GetParameters()[0].ParameterType) });
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[CasinoPlugin] AddMoney error: " + ex.Message);
        }
    }

    private bool RemoveMoney(Player player, long amount)
    {
        if (amount == 0)
        {
            return true;
        }

        try
        {
            object obj = TryGetPropertyOrField(player, "character") ?? TryGetPropertyOrField(player, "Character");
            if (obj != null)
            {
                MethodInfo method = obj.GetType().GetMethod("RemoveMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (method != null)
                {
                    method.Invoke(obj, new object[1] { Convert.ChangeType(amount, method.GetParameters()[0].ParameterType) });
                    return true;
                }

                object obj2 = TryGetPropertyOrField(obj, "Money") ?? TryGetPropertyOrField(obj, "money");
                if (obj2 != null && long.TryParse(obj2.ToString(), out var result))
                {
                    if (result < amount)
                    {
                        return false;
                    }

                    SetPropertyOrField(obj, "Money", result - amount);
                    return true;
                }
            }

            object obj3 = TryGetPropertyOrField(player, "wallet") ?? TryGetPropertyOrField(player, "Wallet");
            if (obj3 != null)
            {
                MethodInfo method2 = obj3.GetType().GetMethod("RemoveMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
                if (method2 != null)
                {
                    method2.Invoke(obj3, new object[1] { Convert.ChangeType(amount, method2.GetParameters()[0].ParameterType) });
                    return true;
                }

                object obj4 = TryGetPropertyOrField(obj3, "Money") ?? TryGetPropertyOrField(obj3, "money");
                if (obj4 != null && long.TryParse(obj4.ToString(), out var result2))
                {
                    if (result2 < amount)
                    {
                        return false;
                    }

                    SetPropertyOrField(obj3, "Money", result2 - amount);
                    return true;
                }
            }

            MethodInfo method3 = player.GetType().GetMethod("RemoveMoney", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (method3 != null)
            {
                method3.Invoke(player, new object[1] { Convert.ChangeType(amount, method3.GetParameters()[0].ParameterType) });
                return true;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[CasinoPlugin] RemoveMoney error: " + ex.Message);
        }

        return false;
    }

    private object TryGetPropertyOrField(object obj, string name)
    {
        if (obj == null)
        {
            return null;
        }

        Type type = obj.GetType();
        PropertyInfo propertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((PropertyInfo p) => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo != null)
        {
            return propertyInfo.GetValue(obj);
        }

        FieldInfo fieldInfo = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((FieldInfo f) => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        if (fieldInfo != null)
        {
            return fieldInfo.GetValue(obj);
        }

        return null;
    }

    private void SetPropertyOrField(object obj, string name, object value)
    {
        if (obj == null)
        {
            return;
        }

        Type type = obj.GetType();
        PropertyInfo propertyInfo = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((PropertyInfo p) => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            try
            {
                propertyInfo.SetValue(obj, Convert.ChangeType(value, propertyInfo.PropertyType));
                return;
            }
            catch
            {
            }
        }

        FieldInfo fieldInfo = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FirstOrDefault((FieldInfo f) => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        if (!(fieldInfo != null))
        {
            return;
        }

        try
        {
            fieldInfo.SetValue(obj, Convert.ChangeType(value, fieldInfo.FieldType));
        }
        catch
        {
        }
    }

    private void TryPlaceBet(Player player, int amount, Action<Player, int> onResult)
    {
        if (amount <= 0)
        {
            SendPlayerText(player, "<color=red>Mise invalide.</color>");
            return;
        }

        long playerMoney = GetPlayerMoney(player);
        if (playerMoney < amount)
        {
            SendPlayerText(player, "<color=red>Tu n'as pas assez d'argent.</color>");
            return;
        }

        if (!RemoveMoney(player, amount))
        {
            SendPlayerText(player, "<color=red>Impossible de retirer l'argent (erreur interne).</color>");
            return;
        }

        Debug.Log($"[CasinoPlugin] {GetPlayerName(player)} mise {amount}$");
        try
        {
            onResult?.Invoke(player, amount);
        }
        catch (Exception ex)
        {
            AddMoney(player, amount);
            SendPlayerText(player, "<color=red>Erreur lors du jeu. Mise remboursée.</color>");
            Debug.LogError("[CasinoPlugin] Erreur dans onResult: " + ex);
        }
    }

    private void SendPlayerText(Player player, string message)
    {
        try
        {
            MethodInfo method = player.GetType().GetMethod("SendText", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (method != null)
            {
                method.Invoke(player, new object[1] { message });
                return;
            }

            MethodInfo method2 = player.GetType().GetMethod("SendTextNotification", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (method2 != null)
            {
                method2.Invoke(player, new object[1] { message });
            }
            else
            {
                Debug.Log("[CasinoPlugin -> MSG] " + GetPlayerName(player) + " : " + message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[CasinoPlugin] SendPlayerText reflection failed: " + ex.Message);
        }
    }

    private string GetPlayerName(Player player)
    {
        try
        {
            MethodInfo method = player.GetType().GetMethod("GetName", BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.Public);
            if (method != null)
            {
                object obj = method.Invoke(player, null);
                if (obj != null)
                {
                    return obj.ToString();
                }
            }

            PropertyInfo propertyInfo = player.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((PropertyInfo p) => string.Equals(p.Name, "Name", StringComparison.OrdinalIgnoreCase));
            if (propertyInfo != null)
            {
                object value = propertyInfo.GetValue(player);
                if (value != null)
                {
                    return value.ToString();
                }
            }

            FieldInfo fieldInfo = player.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public).FirstOrDefault((FieldInfo f) => string.Equals(f.Name, "name", StringComparison.OrdinalIgnoreCase));
            if (fieldInfo != null)
            {
                object value2 = fieldInfo.GetValue(player);
                if (value2 != null)
                {
                    return value2.ToString();
                }
            }
        }
        catch
        {
        }

        return "Unknown";
    }
}
#if false // Journal de décompilation
'25' éléments dans le cache
------------------
Résoudre : 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Un seul assembly trouvé : 'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Charger à partir de : 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\mscorlib.dll'
------------------
Résoudre : 'Assembly-CSharp, Version=1.5.1.4, Culture=neutral, PublicKeyToken=821f1be12317f5ff'
Un seul assembly trouvé : 'Assembly-CSharp, Version=1.4.2.5, Culture=neutral, PublicKeyToken=0e2080cf6d9dd5d5'
AVERTISSEMENT : Incompatibilité de version. Attendu : '1.5.1.4'. Reçu : '1.4.2.5'
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
Résoudre : 'ModKit, Version=2.3.1.0, Culture=neutral, PublicKeyToken=null'
Un seul assembly trouvé : 'ModKit, Version=2.3.1.0, Culture=neutral, PublicKeyToken=null'
Charger à partir de : 'C:\Program Files (x86)\Steam\steamapps\common\Nova-Life\Nova-Life_Data\Managed\ModKit.dll'
------------------
Résoudre : 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Un seul assembly trouvé : 'System.Core, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'
Charger à partir de : 'C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.7.2\System.Core.dll'
#endif
