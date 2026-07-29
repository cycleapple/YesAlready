using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;
using System.Numerics;
using System.Text;
using YesAlready.Features;

namespace YesAlready.UI.Tabs;
public static class Custom
{
    public static void DrawButtons()
    {
        var style = ImGui.GetStyle();
        var newStyle = new Vector2(style.ItemSpacing.X / 2, style.ItemSpacing.Y);
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, newStyle);

        if (ImGuiX.IconButton(FontAwesomeIcon.Plus, "新增項目"))
        {
            var newNode = new CustomEntryNode
            {
                Enabled = true,
                Addon = "AddonName",
                CallbackParams = "-1"
            };
            C.CustomRootFolder.Children.Add(newNode);
            C.Save();
            CustomAddonCallbacks.Toggle();
        }

        var sb = new StringBuilder();
        sb.AppendLine("此區可建立自訂的內建功能。");
        sb.AppendLine("有些功能只需在指定 addon 出現時傳送一組 callback 參數，此處可用來建立這類項目。");
        sb.AppendLine();
        sb.AppendLine("Callback 參數解析方式與 Something Need Doing 相同。");
        sb.AppendLine("自訂項目會透過 AddonLifeCycle 的 PostSetup 事件註冊。");
        sb.AppendLine();
        sb.AppendLine("部分功能可能需要特殊參數、等待或其他 AddonEvent，這類需求仍需由一般內建模組實作。");
        sb.AppendLine();
        sb.AppendLine("範例：");
        sb.AppendLine("   AddonName: Character");
        sb.AppendLine("   Parameters: -1");
        sb.AppendLine("   效果：開啟 Character addon 時會立即關閉，通常沒有實際用途。");

        ImGui.SameLine();
        ImGuiX.IconButton(FontAwesomeIcon.QuestionCircle, sb.ToString());
        if (ImGui.IsItemHovered()) ImGui.SetTooltip(sb.ToString());
    }

    public static void DrawPopup(CustomEntryNode node, Vector2 spacing)
    {
        using var _ = ImRaii.PushStyle(ImGuiStyleVar.ItemSpacing, spacing);
        var enabled = node.Enabled;
        if (ImGui.Checkbox("啟用", ref enabled))
        {
            node.Enabled = enabled;
            C.Save();
            CustomAddonCallbacks.Toggle();
        }

        var trashAltWidth = ImGuiX.GetIconButtonWidth(FontAwesomeIcon.TrashAlt);

        ImGui.SameLine(ImGui.GetContentRegionMax().X - trashAltWidth);
        if (ImGuiX.IconButton(FontAwesomeIcon.TrashAlt, "刪除"))
        {
            if (C.TryFindParent(node, out var parentNode))
            {
                parentNode!.Children.Remove(node);
                C.Save();
                CustomAddonCallbacks.Toggle();
            }
        }

        ImGui.TextUnformatted("備註：");
        var noteText = node.Text;
        if (ImGui.InputText($"##{node.Name}-{nameof(noteText)}", ref noteText, 10_000, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.Text = noteText;
            C.Save();
        }
        ImGui.SameLine();
        ImGuiComponents.HelpMarker("此欄位不會影響功能，僅供記錄此項目的用途。");

        ImGui.TextUnformatted("Addon 名稱：");
        var addonName = node.Addon;
        if (ImGui.InputText($"##{node.Name}-{nameof(addonName)}", ref addonName, 100, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.Addon = addonName;
            C.Save();
            CustomAddonCallbacks.Toggle();
        }

        ImGui.TextUnformatted("參數：");
        var callbackParams = node.CallbackParams;
        if (ImGui.InputText($"##{node.Name}-{nameof(callbackParams)}", ref callbackParams, 150, ImGuiInputTextFlags.AutoSelectAll | ImGuiInputTextFlags.EnterReturnsTrue))
        {
            node.CallbackParams = callbackParams;
            C.Save();
        }
    }
}
