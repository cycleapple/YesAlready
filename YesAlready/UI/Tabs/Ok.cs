using Dalamud.Interface;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using System.Text;

namespace YesAlready.UI.Tabs;
public static class Ok
{
    private static TextFolderNode OkRootFolder => C.OkRootFolder;

    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "新增項目"))
        {
            var newNode = new OkEntryNode { Enabled = false, Text = "Your text goes here" };
            OkRootFolder.Children.Add(newNode);
            C.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.SearchPlus, "將最近偵測項目新增為項目"))
        {
            var io = ImGui.GetIO();
            var createFolder = io.KeyShift;

            Configuration.CreateNode<OkEntryNode>(OkRootFolder, createFolder);
            C.Save();
        }

        ImGui.SameLine();
        if (ImGuiX.IconButton(FontAwesomeIcon.FolderPlus, "新增資料夾"))
        {
            var newNode = new TextFolderNode { Name = "未命名資料夾" };
            OkRootFolder.Children.Add(newNode);
            C.Save();
        }

        var sb = new StringBuilder();
        sb.AppendLine("輸入對話框中的完整或部分文字。");
        sb.AppendLine("For example: \"You cannot carry any more letters\" for the full mailbox dialog.");
        sb.AppendLine();
        sb.AppendLine("也可以用斜線包住文字，將其視為正規表示式。");
        sb.AppendLine("As such: \"/.* carry any more letters .*/\"");
        sb.AppendLine();
        sb.AppendLine("符合時會自動按下確定按鈕。");
        sb.AppendLine();
        sb.AppendLine("在項目上按右鍵可檢視設定。");
        sb.AppendLine("雙擊項目可快速啟用或停用。");
        sb.AppendLine("按住 Ctrl+Shift 並在項目上按右鍵，可刪除該項目及其子項目。");
        sb.AppendLine();
        sb.AppendLine("\"Add last seen as new entry\" button modifiers:");
        sb.AppendLine("   Shift+點擊：新增至新資料夾或第一個現有資料夾。");
        sb.AppendLine();
        sb.AppendLine("目前支援的文字 addon：");
        sb.AppendLine("  - SelectOk");

        ImGui.SameLine();
        ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());

        ImGui.PopStyleVar(); // ItemSpacing
    }

    public static void DisplayOkEntryNode(OkEntryNode node)
    {
        var validRegex = node.IsTextRegex && node.TextRegex != null || !node.IsTextRegex;

        if (!node.Enabled && !validRegex)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, 0, 0, 1));
        else if (!node.Enabled)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
        else if (!validRegex)
            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(1, 0, 0, 1));

        ImGui.TreeNodeEx($"{node.Name}##{node.Name}-tree", ImGuiTreeNodeFlags.Leaf);
        ImGui.TreePop();

        if (!node.Enabled || !validRegex)
            ImGui.PopStyleColor();

        if (!validRegex)
            ImGuiX.TextTooltip("文字正規表示式無效");

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

    public static void DrawPopup(OkEntryNode node, Vector2 spacing)
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

        var matchText = node.Text;
        if (ImGui.InputText($"##{node.Name}-matchText", ref matchText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.Text = matchText;
            C.Save();
        }

        ImGui.PopStyleVar(); // ItemSpacing
    }
}
