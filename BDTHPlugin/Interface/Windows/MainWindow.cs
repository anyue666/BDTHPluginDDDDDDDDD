using System;
using System.Numerics;

using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;

using ImGuiNET;
using ImGuizmoNET;

using BDTHPlugin.Interface.Components;

namespace BDTHPlugin.Interface.Windows
{
    public class MainWindow : Window
    {
        // 获取插件内存实例
        private static PluginMemory Memory => Plugin.GetMemory();
        // 获取插件配置实例
        private static Configuration Configuration => Plugin.GetConfiguration();

        // 定义红色颜色向量
        private static readonly Vector4 RED_COLOR = new(1, 0, 0, 1);

        // 声明一个 Gizmo 实例
        private readonly Gizmo Gizmo;
        // 声明一个物品控制组件实例
        private readonly ItemControls ItemControls = new();

        // 重置标志
        public bool Reset;

        // 主窗口构造函数
        public MainWindow(Gizmo gizmo) : base(
            "Burning Down the House##BDTH",  // 窗口标题
            ImGuiWindowFlags.NoScrollbar |  // 无滚动条
            ImGuiWindowFlags.NoScrollWithMouse |  // 鼠标无法滚动
            ImGuiWindowFlags.NoResize |  // 无法调整大小
            ImGuiWindowFlags.AlwaysAutoResize  // 始终自动调整大小
        )
        {
            Gizmo = gizmo;
        }

        // 绘制前的预处理方法
        public override void PreDraw()
        {
            if (Reset)
            {
                Reset = false;
                // 始终将窗口位置设置为 (69, 69)
                ImGui.SetNextWindowPos(new Vector2(69, 69), ImGuiCond.Always);
            }
        }

        // 绘制窗口内容的方法
        public unsafe override void Draw()
        {
            // 开始一个 ImGui 组
            ImGui.BeginGroup();

            // 获取是否允许任意放置的配置项
            var placeAnywhere = Configuration.PlaceAnywhere;
            // 绘制一个复选框
            if (ImGui.Checkbox("任意放置", ref placeAnywhere))
            {
                // 根据复选框状态设置任意放置功能
                Memory.SetPlaceAnywhere(placeAnywhere);
                Configuration.PlaceAnywhere = placeAnywhere;
                // 保存配置
                Configuration.Save();
            }
            // 绘制复选框的提示信息
            DrawTooltip("允许在游戏引擎无限制的情况下放置物体。");

            // 在同一行继续绘制
            ImGui.SameLine();

            // 获取是否使用 Gizmo 的配置项
            var useGizmo = Configuration.UseGizmo;
            // 绘制一个复选框
            if (ImGui.Checkbox("Gizmo", ref useGizmo))
            {
                Configuration.UseGizmo = useGizmo;
                // 保存配置
                Configuration.Save();
            }
            // 绘制复选框的提示信息
            DrawTooltip("在选中的物品上显示一个移动 Gizmo，以便在游戏中进行全轴移动。");

            // 在同一行继续绘制
            ImGui.SameLine();

            // 获取是否启用吸附的配置项
            var doSnap = Configuration.DoSnap;
            // 绘制一个复选框
            if (ImGui.Checkbox("吸附", ref doSnap))
            {
                Configuration.DoSnap = doSnap;
                // 保存配置
                Configuration.Save();
            }
            // 绘制复选框的提示信息
            DrawTooltip("根据下面设置的拖动值启用 Gizmo 移动的吸附功能。");

            // 在同一行继续绘制
            ImGui.SameLine();
            // 绘制一个图标按钮
            if (ImGuiComponents.IconButton(1, Gizmo.Mode == MODE.LOCAL ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
                // 切换 Gizmo 的模式
                Gizmo.Mode = Gizmo.Mode == MODE.LOCAL ? MODE.WORLD : MODE.LOCAL;

            // 绘制按钮的提示信息
            DrawTooltip(
                new string[]
                {
                    $"模式: {(Gizmo.Mode == MODE.LOCAL ? "本地" : "世界")}",
                    "在本地和世界移动模式之间切换 Gizmo 模式。"
                });

            // 绘制分隔线
            ImGui.Separator();

            // 如果当前不在房屋布置模式下
            if (Memory.HousingStructure->Mode == HousingLayoutMode.None)
                // 绘制错误信息
                DrawError("进入房屋布置模式以开始操作");
            // 如果当前使用游戏手柄模式
            else if (PluginMemory.GamepadMode)
                // 绘制错误信息
                DrawError("不支持游戏手柄");
            // 如果没有选中房屋物品或者不在旋转模式下
            else if (Memory.HousingStructure->ActiveItem == null || Memory.HousingStructure->Mode != HousingLayoutMode.Rotate)
            {
                // 绘制错误信息
                DrawError("在旋转模式下选择一个房屋物品");
                // 绘制帮助标记
                ImGuiComponents.HelpMarker("你是否操作正确？尝试使用 /bdth debug 命令并在 Discord 上报告此问题！");
            }
            else
                // 绘制物品控制组件
                ItemControls.Draw();

            // 绘制分隔线
            ImGui.Separator();

            // 获取拖动值的配置项
            var drag = Configuration.Drag;
            // 绘制一个浮点输入框
            if (ImGui.InputFloat("拖动值", ref drag, 0.05f))
            {
                // 限制拖动值在 0.001 到 10 之间
                drag = Math.Min(Math.Max(0.001f, drag), 10f);
                Configuration.Drag = drag;
                // 保存配置
                Configuration.Save();
            }
            // 绘制输入框的提示信息
            DrawTooltip("设置拖动控件时的变化量，也会影响 Gizmo 的吸附功能。");

            // 判断游戏内家具列表是否可见
            var dummyHousingGoods = PluginMemory.HousingGoods != null && PluginMemory.HousingGoods->IsVisible;
            // 判断库存是否可见
            var dummyInventory = Memory.InventoryVisible;

            // 绘制一个复选框
            if (ImGui.Checkbox("显示游戏内列表", ref dummyHousingGoods))
            {
                // 根据复选框状态显示或隐藏家具列表
                Memory.ShowFurnishingList(dummyHousingGoods);

                Configuration.DisplayFurnishingList = dummyHousingGoods;
                // 保存配置
                Configuration.Save();
            }
            // 在同一行继续绘制
            ImGui.SameLine();

            // 绘制一个复选框
            if (ImGui.Checkbox("显示库存", ref dummyInventory))
            {
                // 根据复选框状态显示或隐藏库存
                Memory.ShowInventory(dummyInventory);

                Configuration.DisplayInventory = dummyInventory;
                // 保存配置
                Configuration.Save();
            }

            // 绘制一个按钮
            if (ImGui.Button("打开家具列表"))
                // 处理命令以打开家具列表
                Plugin.CommandManager.ProcessCommand("/bdth list");
            // 绘制按钮的提示信息
            DrawTooltip(
                new string[]
                {
                    "打开一个家具列表，你可以按距离排序并点击选择物品。",
                    "注意：目前在户外不可用！"
                });

            // 获取自动显示的配置项
            var autoVisible = Configuration.AutoVisible;
            // 绘制一个复选框
            if (ImGui.Checkbox("自动打开", ref autoVisible))
            {
                Configuration.AutoVisible = autoVisible;
                // 保存配置
                Configuration.Save();
            }

            // 结束 ImGui 组
            ImGui.EndGroup();
        }

        // 绘制多行提示信息的方法
        private static void DrawTooltip(string[] text)
        {
            // 如果鼠标悬停在物品上
            if (ImGui.IsItemHovered())
            {
                // 开始绘制提示框
                ImGui.BeginTooltip();
                foreach (var t in text)
                    // 逐行绘制提示信息
                    ImGui.Text(t);
                // 结束绘制提示框
                ImGui.EndTooltip();
            }
        }

        // 绘制单行提示信息的方法
        private static void DrawTooltip(string text)
        {
            DrawTooltip(new string[] { text });
        }

        // 绘制错误信息的方法
        private void DrawError(string text)
        {
            // 设置文本颜色为红色
            ImGui.PushStyleColor(ImGuiCol.Text, RED_COLOR);
            // 绘制文本
            ImGui.Text(text);
            // 恢复文本颜色
            ImGui.PopStyleColor();
        }
    }
}
