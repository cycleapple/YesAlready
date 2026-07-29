using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using ECommons.ImGuiMethods;
using Dalamud.Bindings.ImGui;
using System;
using System.Linq;

namespace YesAlready.UI.Tabs;
public static class Bothers
{
    private static readonly string[] hotkeyChoices =
    [
        "無",
        "Ctrl",
        "Alt",
        "Shift",
    ];

    private static readonly VirtualKey[] hotkeyValues =
    [
        VirtualKey.NO_KEY,
        VirtualKey.CONTROL,
        VirtualKey.MENU,
        VirtualKey.SHIFT,
    ];

    public static void Draw()
    {
        using var tab = ImRaii.TabItem("內建功能##Bothers");
        if (!tab) return;
        using var idScope = ImRaii.PushId($"BothersOptions");

        #region Hotkey Settings

        if (ImGui.CollapsingHeader("快捷鍵設定"))
        {
            // 1. Disable Hotkey
            if (!hotkeyValues.Contains(C.DisableKey))
            {
                C.DisableKey = VirtualKey.NO_KEY;
                C.Save();
            }

            var disableHotkeyIndex = Array.IndexOf(hotkeyValues, C.DisableKey);

            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("停用快捷鍵", ref disableHotkeyIndex, hotkeyChoices, hotkeyChoices.Length))
            {
                C.DisableKey = hotkeyValues[disableHotkeyIndex];
                C.Save();
            }

            ImGuiX.IndentedTextColored("按住此按鍵時停用插件。");

            // 2. Forced Yes Hotkey
            if (!hotkeyValues.Contains(C.ForcedYesKey))
            {
                C.ForcedYesKey = VirtualKey.NO_KEY;
                C.Save();
            }

            var forcedYesHotkeyIndex = Array.IndexOf(hotkeyValues, C.ForcedYesKey);

            ImGui.SetNextItemWidth(85);
            if (ImGui.Combo("強制選擇「是」快捷鍵", ref forcedYesHotkeyIndex, hotkeyChoices, hotkeyChoices.Length))
            {
                C.ForcedYesKey = hotkeyValues[forcedYesHotkeyIndex];
                C.Save();
            }

            ImGui.SameLine();
            var separateForcedKeys = C.SeparateForcedKeys;
            if (ImGui.Checkbox("分開設定「是」與對話快捷鍵", ref separateForcedKeys))
            {
                C.SeparateForcedKeys = separateForcedKeys;
                C.Save();
            }

            if (C.SeparateForcedKeys)
            {
                var forcedTalkHotkeyIndex = Array.IndexOf(hotkeyValues, C.ForcedTalkKey);
                ImGui.SetNextItemWidth(85);
                if (ImGui.Combo("強制跳過對話快捷鍵", ref forcedTalkHotkeyIndex, hotkeyChoices, hotkeyChoices.Length))
                {
                    C.ForcedTalkKey = hotkeyValues[forcedTalkHotkeyIndex];
                    C.Save();
                }
            }

            ImGuiX.IndentedTextColored("按住此按鍵時，所有是／否提示都會選擇「是」，並跳過所有對話。請謹慎使用。");
        }

        #endregion
        #region Desynthesis/AetherialReduction

        if (ImGui.CollapsingHeader("分解與精選"))
        {
            // 3. SalvageDialog
            var desynthDialog = C.DesynthDialogEnabled;
            if (ImGui.Checkbox("SalvageDialog", ref desynthDialog))
            {
                C.DesynthDialogEnabled = desynthDialog;
                C.Save();
            }
            ImGuiX.IndentedTextColored("略過分解選單的確認視窗。");

            // 4. SalvageDialog (Bulk)
            //var desynthBulkDialog = C.DesynthBulkDialogEnabled;
            //if (ImGui.Checkbox("SalvageDialog (Bulk)", ref desynthBulkDialog))
            //{
            //    C.DesynthBulkDialogEnabled = desynthBulkDialog;
            //    C.Save();
            //}
            //ImGuiEx.IndentedTextColored("Check the bulk desynthesis button when using the SalvageDialog feature.");

            // 5. SalvageResults
            var desynthResultsDialog = C.DesynthesisResults;
            if (ImGui.Checkbox("SalvageResults", ref desynthResultsDialog))
            {
                C.DesynthesisResults = desynthResultsDialog;
                C.Save();
            }
            ImGuiX.IndentedTextColored("分解完成後自動關閉結果視窗。");

            var purifyResultsDialog = C.AetherialReductionResults;
            if (ImGui.Checkbox("PurifyResult", ref purifyResultsDialog))
            {
                C.AetherialReductionResults = purifyResultsDialog;
                C.Save();
            }
            ImGuiX.IndentedTextColored("精選完成後自動關閉結果視窗。");
        }

        #endregion
        #region Melding

        if (ImGui.CollapsingHeader("魔晶石鑲嵌"))
        {
            var meld = C.MaterialAttachDialogEnabled;
            if (ImGui.Checkbox("MateriaAttachDialog", ref meld))
            {
                C.MaterialAttachDialogEnabled = meld;
                C.Save();
            }
            ImGuiX.IndentedTextColored("略過魔晶石鑲嵌確認。");

            var materialize = C.MaterializeDialogEnabled;
            if (ImGui.Checkbox("MaterializeDialog", ref materialize))
            {
                C.MaterializeDialogEnabled = materialize;
                C.Save();
            }
            ImGuiX.IndentedTextColored("略過魔晶石精製確認。");

            var materiaRetrieve = C.MateriaRetrieveDialogEnabled;
            if (ImGui.Checkbox("MateriaRetrieveDialog", ref materiaRetrieve))
            {
                C.MateriaRetrieveDialogEnabled = materiaRetrieve;
                C.Save();
            }
            ImGuiX.IndentedTextColored("略過魔晶石回收確認。");
        }

        #endregion
        #region Retainers & Submersibles

        if (ImGui.CollapsingHeader("僱員與潛水艇"))
        {
            var retainerTaskAsk = C.RetainerTaskAskEnabled;
            if (ImGui.Checkbox("RetainerTaskAsk", ref retainerTaskAsk))
            {
                C.RetainerTaskAskEnabled = retainerTaskAsk;
                C.Save();
            }
            ImGuiX.IndentedTextColored("派遣僱員前略過最後的確認對話。");

            var retainerTaskResult = C.RetainerTaskResultEnabled;
            if (ImGui.Checkbox("RetainerTaskResult", ref retainerTaskResult))
            {
                C.RetainerTaskResultEnabled = retainerTaskResult;
                C.Save();
            }
            ImGuiX.IndentedTextColored("領取物品時自動讓僱員再次執行相同探險。");

            var retainerListDialog = C.RetainerTransferListConfirm;
            if (ImGui.Checkbox("RetainerItemTransferList", ref retainerListDialog))
            {
                C.RetainerTransferListConfirm = retainerListDialog;
                C.Save();
            }
            ImGuiX.IndentedTextColored("將所有物品委託給僱員時略過確認。");

            var retainerProgressDialog = C.RetainerTransferProgressConfirm;
            if (ImGui.Checkbox("RetainerItemTransferProgress", ref retainerProgressDialog))
            {
                C.RetainerTransferProgressConfirm = retainerProgressDialog;
                C.Save();
            }
            ImGuiX.IndentedTextColored("物品委託完成後自動關閉進度視窗。");

            var finalize = C.AirShipExplorationResultFinalize;
            if (ImGui.Checkbox("AirShipExplorationResult - Finalize", ref finalize))
            {
                if (finalize && C.AirShipExplorationResultRedeploy)
                    C.AirShipExplorationResultRedeploy = false;
                C.AirShipExplorationResultFinalize = finalize;
                C.Save();
            }
            ImGuiX.IndentedTextColored("潛水艇探索結果視窗開啟時自動完成報告。");

            var redeploy = C.AirShipExplorationResultRedeploy;
            if (ImGui.Checkbox("AirShipExplorationResult - Redeploy", ref redeploy))
            {
                if (redeploy && C.AirShipExplorationResultFinalize)
                    C.AirShipExplorationResultFinalize = false;
                C.AirShipExplorationResultRedeploy = redeploy;
                C.Save();
            }
            ImGuiX.IndentedTextColored("潛水艇探索結果視窗開啟時自動再次派遣。");
        }

        #endregion
        #region Duties

        if (ImGui.CollapsingHeader("任務"))
        {
            var contentsFinderConfirm = C.ContentsFinderConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderConfirm", ref contentsFinderConfirm))
            {
                C.ContentsFinderConfirmEnabled = contentsFinderConfirm;

                if (!contentsFinderConfirm)
                    C.ContentsFinderOneTimeConfirmEnabled = false;

                C.Save();
            }
            ImGuiX.IndentedTextColored("任務準備完成時自動開始。");

            var contentsFinderOneTimeConfirm = C.ContentsFinderOneTimeConfirmEnabled;
            if (ImGui.Checkbox("ContentsFinderOneTimeConfirm", ref contentsFinderOneTimeConfirm))
            {
                C.ContentsFinderOneTimeConfirmEnabled = contentsFinderOneTimeConfirm;

                if (contentsFinderOneTimeConfirm)
                    C.ContentsFinderConfirmEnabled = true;

                C.Save();
            }
            ImGuiX.IndentedTextColored("任務準備完成時自動開始一次。需要啟用 ContentsFinderConfirm，觸發後會停用兩者。");

            //var dutyDifficulty = C.DifficultySelectYesNoEnabled;
            //if (ImGui.Checkbox("SelectYesNoDifficulty", ref dutyDifficulty))
            //{
            //    C.DifficultySelectYesNoEnabled = dutyDifficulty;
            //    C.Save();
            //}

            //if (C.DifficultySelectYesNoEnabled)
            //{
            //    var difficulty = C.DifficultySelectYesNo;
            //    if (ImGuiEx.EnumCombo("SelectYesNoDifficulty", ref difficulty))
            //    {
            //        C.DifficultySelectYesNo = difficulty;
            //        C.Save();
            //    }
            //}
            //ImGuiX.IndentedTextColored("Automatically commence solo duties at the selected difficulty.");
        }

        #endregion
        #region PvP

        if (ImGui.CollapsingHeader("PvP"))
        {
            var ccquit = C.MKSRecordQuit;
            if (ImGui.Checkbox("MKSRecord", ref ccquit))
            {
                C.MKSRecordQuit = ccquit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("水晶衝突結果顯示後自動離開。");

            var flquit = C.FrontlineRecordQuit;
            if (ImGui.Checkbox("FrontlineRecord", ref flquit))
            {
                C.FrontlineRecordQuit = flquit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("紛爭前線結果顯示後自動離開。");
        }

        #endregion
        #region Gold Saucer

        if (ImGui.CollapsingHeader("小遊戲與特殊活動"))
        {
            var lotto = C.LotteryWeeklyInput;
            if (ImGui.Checkbox("LotteryWeeklyInput", ref lotto))
            {
                C.LotteryWeeklyInput = lotto;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動以隨機號碼購買仙人微彩彩券。");

            // 19. HWDLottery
            var kupo = C.KupoOfFortune;
            if (ImGui.Checkbox("HWDLottery", ref kupo))
            {
                C.KupoOfFortune = kupo;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動選擇庫啵好運道獎勵。會立即完成一張彩券，但無法自動進行下一張。");

            var lovQuit = C.LordOfVerminionQuit;
            if (ImGui.Checkbox("LovmResult", ref lovQuit))
            {
                C.LordOfVerminionQuit = lovQuit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("萌寵之王結果畫面顯示後自動離開。");

            var fgsEnter = C.FallGuysRegisterConfirm;
            if (ImGui.Checkbox("FGSEnterDialog", ref fgsEnter))
            {
                C.FallGuysRegisterConfirm = fgsEnter;
                C.Save();
            }
            ImGuiX.IndentedTextColored("與活動登記員對話時自動報名金碟遊樂場大挑戰。");

            var fgsExit = C.FallGuysExitConfirm;
            if (ImGui.Checkbox("FGSExitDialog", ref fgsExit))
            {
                C.FallGuysExitConfirm = fgsExit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("離開金碟遊樂場大挑戰時自動確認。");

            var fashionQuit = C.FashionCheckQuit;
            if (ImGui.Checkbox("FashionCheck", ref fashionQuit))
            {
                C.FashionCheckQuit = fashionQuit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動確認時尚品鑑結果。");

            var chocoboQuit = C.ChocoboRacingQuit;
            if (ImGui.Checkbox("RaceChocoboResult", ref chocoboQuit))
            {
                C.ChocoboRacingQuit = chocoboQuit;
                C.Save();
            }
            ImGuiX.IndentedTextColored("陸行鳥競賽結果畫面顯示後自動離開。");

            var shopCard = C.ShopCardDialog;
            if (ImGui.Checkbox("ShopCardDialog", ref shopCard))
            {
                C.ShopCardDialog = shopCard;
                C.Save();
            }
            ImGuiX.IndentedTextColored("在金碟遊樂場出售九宮幻卡時自動確認。");
        }

        #endregion
        #region Shops

        if (ImGui.CollapsingHeader("商店"))
        {
            var inclusionShopRemember = C.InclusionShopRememberEnabled;
            if (ImGui.Checkbox("InclusionShopRemember", ref inclusionShopRemember))
            {
                C.InclusionShopRememberEnabled = inclusionShopRemember;
                C.Save();
            }
            ImGuiX.IndentedTextColored("記住票據交易視窗上次瀏覽的分類。");

            var shopItemExchange = C.ShopExchangeItemDialogEnabled;
            if (ImGui.Checkbox("ShopExchangeItemDialog", ref shopItemExchange))
            {
                C.ShopExchangeItemDialogEnabled = shopItemExchange;
                C.Save();
            }
            ImGuiX.IndentedTextColored("在各類商店中自動交換物品或貨幣（例如票據交易員）。");
        }
        #endregion
        #region Other

        if (ImGui.CollapsingHeader("其他"))
        {
            var deliveries = C.CustomDeliveries;
            if (ImGui.Checkbox("SatisfactionSupply", ref deliveries))
            {
                C.CustomDeliveries = deliveries;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動繳交可用的收藏品以完成老主顧交易。");

            var grandCompanySupplyReward = C.GrandCompanySupplyReward;
            if (ImGui.Checkbox("GrandCompanySupplyReward", ref grandCompanySupplyReward))
            {
                C.GrandCompanySupplyReward = grandCompanySupplyReward;
                C.Save();
            }
            ImGuiX.IndentedTextColored("繳交軍隊籌備的稀有品時略過確認。");

            var journalResultComplete = C.JournalResultCompleteEnabled;
            if (ImGui.Checkbox("JournalResultComplete", ref journalResultComplete))
            {
                C.JournalResultCompleteEnabled = journalResultComplete;
                C.Save();
            }
            ImGuiX.IndentedTextColored("任務獎勵沒有選項時自動確認領取。");

            var guildLeveDifficulty = C.GuildLeveDifficultyConfirm;
            if (ImGui.Checkbox("GuildLeveDifficulty", ref guildLeveDifficulty))
            {
                C.GuildLeveDifficultyConfirm = guildLeveDifficulty;
                C.Save();
            }
            ImGuiX.IndentedTextColored("開始行會理符時自動以最高難度確認。");

            var dkt = C.DataCentreTravelConfirmEnabled;
            if (ImGui.Checkbox("DataCentreTravelConfirm", ref dkt))
            {
                C.DataCentreTravelConfirmEnabled = dkt;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動接受跨資料中心旅行確認。");

            var mpr = C.MiragePrismRemoveDispel;
            if (ImGui.Checkbox("MiragePrismRemoveDispel", ref mpr))
            {
                C.MiragePrismRemoveDispel = mpr;
                C.Save();
            }
            ImGuiX.IndentedTextColored("使用幻象稜晶解除器時自動解除投影。");

            var mpe = C.MiragePrismExecuteCast;
            if (ImGui.Checkbox("MiragePrismExecuteCast", ref mpe))
            {
                C.MiragePrismExecuteCast = mpe;
                C.Save();
            }
            ImGuiX.IndentedTextColored("使用幻象稜晶時自動執行投影。");

            var bpu = C.BannerPreviewUpdate;
            if (ImGui.Checkbox("BannerPreviewUpdate", ref bpu))
            {
                C.BannerPreviewUpdate = bpu;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動更新肖像。");
        }

        #endregion

        #region Forays
        if (ImGui.CollapsingHeader("特殊探索"))
        {
            var itemInspection = C.ItemInspectionResultEnabled;
            if (ImGui.Checkbox("ItemInspectionResult", ref itemInspection))
            {
                C.ItemInspectionResultEnabled = itemInspection;
                C.Save();
            }
            ImGuiX.IndentedTextColored("適用於優雷卡／博茲雅寶箱、失傳碎晶等。警告：不會檢查物品是否已達上限。可設定處理 N 個物品後暫停。");

            if (itemInspection)
            {
                ImGui.Indent();
                var rateLimit = C.ItemInspectionResultRateLimiter;
                if (ImGui.InputInt(string.Empty, ref rateLimit))
                {
                    C.ItemInspectionResultRateLimiter = rateLimit;
                    C.Save();
                }
                ImGui.Unindent();
                ImGuiX.IndentedTextColored("速率限制（每處理 N 個物品後暫停，設為 0 可停用）。");
            }

            var wksAnnounceHide = C.WKSAnnounceHide;
            if (ImGui.Checkbox("WKSAnnounceHide", ref wksAnnounceHide))
            {
                C.WKSAnnounceHide = wksAnnounceHide;
                C.Save();
            }
            ImGuiX.IndentedTextColored("隱藏宇宙探索公告。");

            var wksRewardClose = C.WKSRewardClose;
            if (ImGui.Checkbox("WKSRewardHide", ref wksRewardClose))
            {
                C.WKSRewardClose = wksRewardClose;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動關閉宇宙探索獎勵視窗。");
        }
        #endregion
    }
}
