using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using ECommons.GameHelpers;
using ECommons.SimpleGui;
using Dalamud.Bindings.ImGui;
using Lumina.Excel.Sheets;
using System.Numerics;
using System.Text;
using YesAlready.Interface;

namespace YesAlready.UI.Tabs;
public class YesNo
{
    private static TextFolderNode RootFolder => C.RootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "新增項目"))
        {
            var newNode = new TextEntryNode { Enabled = false, Text = "Your text goes here" };
            RootFolder.Children.Add(newNode);
            C.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.SearchPlus, "將最近偵測項目新增為項目"))
        {
            var io = ImGui.GetIO();
            var zoneRestricted = io.KeyCtrl;
            var createFolder = io.KeyShift;
            var selectNo = io.KeyAlt;

            Configuration.CreateNode<TextEntryNode>(C.RootFolder, createFolder, zoneRestricted ? GenericHelpers.GetRow<TerritoryType>(Player.Territory.RowId)?.Name.ExtractText() : null, !selectNo);
            C.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "新增資料夾"))
        {
            var newNode = new TextFolderNode { Name = "未命名資料夾" };
            RootFolder.Children.Add(newNode);
            C.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("輸入對話框中的完整或部分文字。");
        sb.AppendLine("例如：傳送確認對話中的「傳送到」。");
        sb.AppendLine();
        sb.AppendLine("也可以用斜線包住文字，將其視為正規表示式。");
        sb.AppendLine("As such: \"/Teleport to .*? for \\d+(,\\d+)? gil\\?/\"");
        sb.AppendLine("也可使用較簡單的正規表示式，但請注意可能誤判其他文字。");
        sb.AppendLine();
        sb.AppendLine("符合時會自動按下「是」，若有核取方塊也會勾選。");
        sb.AppendLine();
        sb.AppendLine("在項目上按右鍵可檢視設定。");
        sb.AppendLine("雙擊項目可快速啟用或停用。");
        sb.AppendLine("按住 Ctrl+Shift 並在項目上按右鍵，可刪除該項目及其子項目。");
        sb.AppendLine();
        sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
        sb.AppendLine("   Shift+點擊：以目前區域名稱建立或使用第一個資料夾，並限制於該區域。");
        sb.AppendLine("   Ctrl+點擊：建立限制於目前區域的項目，但不建立命名資料夾。");
        sb.AppendLine("   Alt-Click to create a \"Select No\" entry instead of \"Select Yes\"");
        sb.AppendLine("   Alt 可與 Shift／Ctrl 點擊搭配使用。");
        sb.AppendLine();
        sb.AppendLine("目前支援的文字 addon：");
        sb.AppendLine("  - SelectYesNo");

        ImGui.SameLine();
        ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());

        ImGui.SameLine();
        if (ImGuiComponents.IconButton(FontAwesomeIcon.EllipsisH))
            ImGui.OpenPopup("SelectYesno additional options");
        DrawYesnoBothers();
    }

    private static void DrawYesnoBothers()
    {
        using var popup = ImRaii.Popup("SelectYesno additional options");
        if (popup.Success)
        {
            var gimmickConfirm = C.GimmickYesNo;
            if (ImGui.Checkbox("Auto GimmickYesNo", ref gimmickConfirm))
            {
                C.GimmickYesNo = gimmickConfirm;
                C.Save();
            }
            ImGuiX.IndentedTextColored("自動確認 GimmickYesNo 資料表中的是／否對話。\n主要為迷宮內的互動，例如開門或拾取物品。", wrapped: false);

            var pfConfirm = C.PartyFinderJoinConfirm;
            if (ImGui.Checkbox("LookingForGroup x SelectYesno", ref pfConfirm))
            {
                C.PartyFinderJoinConfirm = pfConfirm;
                C.Save();
            }

            ImGuiX.IndentedTextColored("加入隊員招募隊伍時自動確認。", wrapped: false);

            var autoCollect = C.AutoCollectable;
            if (ImGui.Checkbox("Auto Collectables", ref autoCollect))
            {
                C.AutoCollectable = autoCollect;
                C.Save();
            }

            ImGuiX.IndentedTextColored("自動接受可繳交的收藏品，並拒絕價值不足的收藏品。", wrapped: false);
        }
    }

    public static void DisplayEntryNode(TextEntryNode node)
    {
        var validRegex = node.IsTextRegex && node.TextRegex != null || !node.IsTextRegex;
        var validZone = !node.ZoneRestricted || node.ZoneIsRegex && node.ZoneRegex != null || !node.ZoneIsRegex;

        if (!node.Enabled && (!validRegex || !validZone))
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validRegex || !validZone)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex || !validZone)
            ImGui.PopStyleColor();

        if (!validRegex && !validZone)
            ImGuiX.TextTooltip("文字與區域的正規表示式無效");
        else if (!validRegex)
            ImGuiX.TextTooltip("文字正規表示式無效");
        else if (!validZone)
            ImGuiX.TextTooltip("區域正規表示式無效");

        if (ImGui.IsItemHovered())
        {
            if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                node.Enabled = !node.Enabled;
                C.Save();
                return;
            }
            else if (ImGui.IsMouseClicked(ImGuiMouseButton.Right))
            {
                var io = ImGui.GetIO();
                if (io.KeyCtrl && io.KeyShift)
                {
                    if (C.TryFindParent(node, out var parent))
                    {
                        parent!.Children.Remove(node);
                        C.Save();
                    }

                    return;
                }
                else
                {
                    ImGui.OpenPopup($"{node.GetHashCode()}-popup");
                }
            }
        }

        MainWindow.TextNodePopup(node);
        MainWindow.TextNodeDragDrop(node);
    }

    public static void DrawPopup(TextEntryNode textNode, Vector2 spacing)
    {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);

        var enabled = textNode.Enabled;
        if (ImGui.Checkbox("啟用", ref enabled))
        {
            textNode.Enabled = enabled;
            C.Save();
        }

        ImGui.SameLine(100f);
        var isYes = textNode.IsYes;
        var title = isYes ? "選擇「是」" : "選擇「否」";
        if (ImGui.Button(title))
        {
            textNode.IsYes = !isYes;
            C.Save();
        }

        var trashAltWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "刪除"))
        {
            if (C.TryFindParent(textNode, out var parentNode))
            {
                parentNode!.Children.Remove(textNode);
                C.Save();
            }
        }

        var matchText = textNode.Text;
        if (ImGui.InputText($"##{textNode.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.Text = matchText;
            C.Save();
        }

        var zoneRestricted = textNode.ZoneRestricted;
        if (ImGui.Checkbox("限制區域", ref zoneRestricted))
        {
            textNode.ZoneRestricted = zoneRestricted;
            C.Save();
        }

        var searchWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.Search);
        var searchPlusWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
        if (ImGuiX.IconButton(FontAwesomeIcon.Search, "區域清單"))
            EzConfigGui.GetWindow<ZoneListWindow>()?.Toggle();

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth - searchPlusWidth - spacing.X);
        if (ImGuiX.IconButton(FontAwesomeIcon.SearchPlus, "填入目前區域"))
        {
            var currentID = Svc.ClientState.TerritoryType;
            if (P.TerritoryNames.TryGetValue(currentID, out var zoneName))
            {
                textNode.ZoneText = zoneName;
                C.Save();
            }
            else
            {
                textNode.ZoneText = "Could not find name";
                C.Save();
            }
        }

        var zoneText = textNode.ZoneText;
        if (ImGui.InputText($"##{textNode.Name}-zoneText", ref zoneText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ZoneText = zoneText;
            C.Save();
        }

        var conditionRestricted = textNode.RequiresPlayerConditions;
        if (ImGui.Checkbox("限制角色狀態", ref conditionRestricted))
        {
            textNode.RequiresPlayerConditions = conditionRestricted;
            C.Save();
        }
        ImGuiComponents.HelpMarker($"角色狀態可使用名稱（區分大小寫）或 ID。多個條件請以逗號分隔；必須全部符合才會執行。若要反轉條件，請在前方加上 \"!\"。");

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchWidth);
        if (ImGuiX.IconButton(FontAwesomeIcon.Search, "角色狀態清單"))
            EzConfigGui.GetWindow<ConditionsListWindow>()?.Toggle();

        var playerConditions = textNode.PlayerConditions;
        if (ImGui.InputText($"##{textNode.Name}-playerConditionsText", ref playerConditions, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.PlayerConditions = playerConditions;
            C.Save();
        }

        ImGui.NewLine();

        var conditional = textNode.IsConditional;
        if (ImGui.Checkbox("使用條件判斷", ref conditional))
        {
            textNode.IsConditional = conditional;
            C.Save();
        }

        ImGui.Text("目前僅支援擷取數字");

        var conditionalText = textNode.ConditionalNumberTemplate;
        if (ImGui.InputText($"##{textNode.Name}-conditionalText", ref conditionalText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ConditionalNumberTemplate = conditionalText;
            C.Save();
        }

        var comparisonType = textNode.ComparisonType;
        if (ImGui.BeginCombo($"##{textNode.Name}-comparisonType", MainWindow.ComparisonTypeToText(comparisonType)))
        {
            foreach (var c in MainWindow.ComparisonTypes)
            {
                var isSelected = comparisonType == c;
                if (ImGui.Selectable(MainWindow.ComparisonTypeToText(c), isSelected))
                {
                    textNode.ComparisonType = c;
                    C.Save();
                }

                if (isSelected)
                    ImGui.SetItemDefaultFocus();
            }
            ImGui.EndCombo();
        }

        var conditionalNumber = textNode.ConditionalNumber;
        if (ImGui.InputInt($"##{textNode.Name}-conditionalNumber", ref conditionalNumber, 1, 10, flags: ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            textNode.ConditionalNumber = conditionalNumber;
            C.Save();
        }
    }
}
