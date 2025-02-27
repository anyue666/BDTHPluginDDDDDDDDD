using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace BDTHPlugin.Interface.Windows
{
    // 定义一个名为 DebugWindow 的类，继承自 Window 类，用于显示调试窗口
    public class DebugWindow : Window
    {
        // 静态属性，用于获取插件的内存实例
        private static PluginMemory Memory => Plugin.GetMemory();

        // 构造函数，初始化调试窗口，设置窗口标题为 "BDTH Debug"
        public DebugWindow() : base("BDTH 调试")
        {

        }

        // 重写 Draw 方法，用于绘制调试窗口的内容
        public unsafe override void Draw()
        {
            // 显示游戏手柄模式状态
            ImGui.Text($"游戏手柄模式: {PluginMemory.GamepadMode}");
            // 显示是否可以编辑物品的状态
            ImGui.Text($"是否可编辑物品: {Memory.CanEditItem()}");
            // 显示房屋系统是否打开的状态
            ImGui.Text($"房屋系统是否打开: {Memory.IsHousingOpen()}");
            // 绘制分隔线
            ImGui.Separator();
            // 显示布局世界的内存地址
            ImGui.Text($"布局世界地址: 0x{(ulong)Memory.Layout:X}");
            // 显示房屋结构的内存地址
            ImGui.Text($"房屋结构地址: 0x{(ulong)Memory.HousingStructure:X}");
            // 显示房屋结构的模式
            ImGui.Text($"模式: {Memory.HousingStructure->Mode}");
            // 显示房屋结构的状态
            ImGui.Text($"状态: {Memory.HousingStructure->State}");
            // 显示房屋结构的另一个状态
            ImGui.Text($"状态 2: {Memory.HousingStructure->State2}");
            // 显示当前激活物品的内存地址
            ImGui.Text($"激活物品地址: 0x{(ulong)Memory.HousingStructure->ActiveItem:X}");
            // 显示当前鼠标悬停物品的内存地址
            ImGui.Text($"悬停物品地址: 0x{(ulong)Memory.HousingStructure->HoverItem:X}");
            // 显示物品是否正在旋转的状态
            ImGui.Text($"是否正在旋转: {Memory.HousingStructure->Rotating}");
            // 绘制分隔线
            ImGui.Separator();
            // 显示房屋模块的内存地址
            ImGui.Text($"房屋模块地址: 0x{(ulong)Memory.HousingModule:X}");
            // 显示当前所在区域的内存地址
            ImGui.Text($"当前区域地址: 0x{(ulong)Memory.HousingModule->CurrentTerritory:X}");
            // 显示室外区域的内存地址
            ImGui.Text($"室外区域地址: 0x{(ulong)Memory.HousingModule->OutdoorTerritory:X}");
            // 显示室内区域的内存地址
            ImGui.Text($"室内区域地址: 0x{(ulong)Memory.HousingModule->IndoorTerritory:X}");

            // 获取当前激活的物品
            var active = Memory.HousingStructure->ActiveItem;
            if (active != null)
            {
                // 如果有激活的物品，绘制分隔线
                ImGui.Separator();
                // 获取激活物品的位置
                var pos = Memory.HousingStructure->ActiveItem->Position;
                // 显示激活物品的位置信息
                ImGui.Text($"位置: {pos.X}, {pos.Y}, {pos.Z}");
            }
        }
    }
}
