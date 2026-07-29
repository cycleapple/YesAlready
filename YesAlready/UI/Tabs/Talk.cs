using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using System.Text;

namespace YesAlready.UI.Tabs;
public static class Talk
{
    private static TextFolderNode TalkRootFolder => C.TalkRootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "新增項目"))
        {
            var newNode = new TalkEntryNode { Enabled = false, TargetText = "Your text goes here" };
            TalkRootFolder.Children.Add(newNode);
            C.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.SearchPlus, "將目前目標新增為項目"))
        {
            var target = Svc.Targets.Target;
            if (target != null)
            {
                var targetName = Service.Watcher.LastSeenTalkTarget = target.Name.TextValue;
                var newNode = new TalkEntryNode { Enabled = true, TargetText = targetName };
                TalkRootFolder.Children.Add(newNode);
                C.Save();
            }
            else
                Svc.Toasts.ShowError("Unable to add entry: no target selected.");
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "新增資料夾"))
        {
            var newNode = new TextFolderNode { Name = "未命名資料夾" };
            TalkRootFolder.Children.Add(newNode);
            C.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("Enter into the input all or part of the selected taret name while in a talk dialog.");
        sb.AppendLine("For example: \"Moyce\" in the Crystarium.");
        sb.AppendLine();
        sb.AppendLine("也可以用斜線包住文字，將其視為正規表示式。");
        sb.AppendLine("As such: \"/(Moyce|Eirikur)/\"");
        sb.AppendLine();
        sb.AppendLine("To skip your retainers, add the summoning bell.");
        sb.AppendLine();
        sb.AppendLine("在項目上按右鍵可檢視設定。");
        sb.AppendLine("雙擊項目可快速啟用或停用。");
        sb.AppendLine("按住 Ctrl+Shift 並在項目上按右鍵，可刪除該項目及其子項目。");
        sb.AppendLine();
        sb.AppendLine("目前支援的清單 addon：");
        sb.AppendLine("  - Talk");

        ImGui.SameLine();
        ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    public static void DisplayTalkEntryNode(TalkEntryNode node)
    {
        var validTarget = node.TargetIsRegex && node.TargetRegex != null || !node.TargetIsRegex;

        if (!node.Enabled && !validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validTarget)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validTarget)
            ImGui.PopStyleColor();

        if (!validTarget)
            ImGuiX.TextTooltip("目標正規表示式無效");

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

    public static void DrawPopup(TalkEntryNode node, Vector2 spacing)
    {
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, spacing);

        var enabled = node.Enabled;
        if (ImGui.Checkbox("啟用", ref enabled))
        {
            node.Enabled = enabled;
            C.Save();
        }

        var trashAltWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "刪除"))
        {
            if (C.TryFindParent(node, out var parentNode))
            {
                parentNode!.Children.Remove(node);
                C.Save();
            }
        }

        var searchPlusWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.SearchPlus);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - searchPlusWidth - trashAltWidth - spacing.X);
        if (ImGuiX.IconButton(FontAwesomeIcon.SearchPlus, "填入目前目標"))
        {
            var target = Svc.Targets.Target;
            var name = target?.Name?.TextValue ?? string.Empty;

            if (!string.IsNullOrEmpty(name))
            {
                node.TargetText = name;
                C.Save();
            }
            else
            {
                node.TargetText = "Could not find target";
                C.Save();
            }
        }

        ImGui.PopStyleVar(); // ItemSpacing

        var targetText = node.TargetText;
        if (ImGui.InputText($"##{node.Name}-targetText", ref targetText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.TargetText = targetText;
            C.Save();
        }
    }
}
