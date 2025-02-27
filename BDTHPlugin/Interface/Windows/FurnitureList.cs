using System;
using System.Numerics;

using Dalamud.Interface.Windowing;

using ImGuiNET;

namespace BDTHPlugin.Interface.Windows
{
    // 家具列表窗口类，继承自Window类
    public class FurnitureList : Window
    {
        // 获取插件内存实例
        private static PluginMemory Memory => Plugin.GetMemory();
        // 获取插件配置实例
        private static Configuration Configuration => Plugin.GetConfiguration();

        // 记录上一次活动物品的ID
        private ulong? lastActiveItem;
        // 渲染计数
        private byte renderCount;

        // 构造函数，初始化窗口名称为“Furnishing List”
        public FurnitureList() : base("家具列表")
        {

        }

        // 绘制前的预处理方法
        public override void PreDraw()
        {
            // 仅当房屋窗口打开且不在户外时才允许显示家具列表
            IsOpen &= Memory.IsHousingOpen() && !Plugin.IsOutdoors();
        }

        // 绘制窗口内容的方法
        public unsafe override void Draw()
        {
            // 获取全局字体缩放比例
            var fontScale = ImGui.GetIO().FontGlobalScale;
            // 检查是否有活动物品被选中
            var hasActiveItem = Memory.HousingStructure->ActiveItem != null;

            // 设置窗口大小约束
            SizeConstraints = new WindowSizeConstraints
            {
                MinimumSize = new Vector2(120 * fontScale, 100 * fontScale),
                MaximumSize = new Vector2(400 * fontScale, 1000 * fontScale)
            };

            // 获取是否按距离排序的配置项
            var sortByDistance = Configuration.SortByDistance;
            // 绘制一个复选框，用于切换是否按距离排序
            if (ImGui.Checkbox("按距离排序", ref sortByDistance))
            {
                // 更新配置项
                Configuration.SortByDistance = sortByDistance;
                // 保存配置
                Configuration.Save();
            }

            // 设置项目间距样式
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 8));
            // 绘制分隔线
            ImGui.Separator();
            // 恢复样式
            ImGui.PopStyleVar();

            // 开始一个子窗口，名为“FurnishingList”
            ImGui.BeginChild("家具列表");

            // 如果本地玩家为空，直接返回
            if (Plugin.ClientState.LocalPlayer == null)
                return;

            // 获取本地玩家的位置
            var playerPos = Plugin.ClientState.LocalPlayer.Position;
            // 检查是否有活动物品被选中

            // 开始一个表格，名为“FurnishingListItems”，有3列
            if (ImGui.BeginTable("家具列表项", 3))
            {
                // 设置表格列的属性
                ImGui.TableSetupColumn("图标", ImGuiTableColumnFlags.WidthFixed, 0f);
                ImGui.TableSetupColumn("名称", ImGuiTableColumnFlags.WidthStretch, 0f);
                ImGui.TableSetupColumn("距离", ImGuiTableColumnFlags.WidthFixed, 0f);

                try
                {
                    // 获取家具列表
                    if (Memory.GetFurnishings(out var items, playerPos, sortByDistance))
                    {
                        // 遍历家具列表
                        for (var i = 0; i < items.Count; i++)
                        {
                            // 开始新的表格行
                            ImGui.TableNextRow(ImGuiTableRowFlags.None, 28 * fontScale);
                            // 切换到下一个表格列
                            ImGui.TableNextColumn();
                            // 对齐文本到框架内边距
                            ImGui.AlignTextToFramePadding();

                            // 初始化家具名称和图标ID
                            var name = "";
                            ushort icon = 0;

                            // 尝试获取庭院物品信息
                            if (Plugin.TryGetYardObject(items[i].HousingRowId, out var yardObject))
                            {
                                // 获取物品名称
                                name = yardObject.Item.Value.Name.ToString();
                                // 获取物品图标ID
                                icon = yardObject.Item.Value.Icon;
                            }

                            // 尝试获取家具物品信息
                            if (Plugin.TryGetFurnishing(items[i].HousingRowId, out var furnitureObject))
                            {
                                // 获取物品名称
                                name = furnitureObject.Item.Value.Name.ToString();
                                // 获取物品图标ID
                                icon = furnitureObject.Item.Value.Icon;
                            }

                            // 如果无法获取名称或图标ID，跳过该物品
                            if (name == string.Empty || icon == 0)
                                continue;

                            // 检查当前物品是否为活动物品
                            var thisActive = hasActiveItem && items[i].Item == Memory.HousingStructure->ActiveItem;

                            // 设置项目间距样式
                            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0f, 4f));
                            // 绘制可选择项
                            if (ImGui.Selectable($"##物品{i}", thisActive, ImGuiSelectableFlags.SpanAllColumns, new(0, 20 * fontScale)))
                                // 选择物品
                                Memory.SelectItem((IntPtr)Memory.HousingStructure, (IntPtr)items[i].Item);
                            // 恢复样式
                            ImGui.PopStyleVar();

                            // 如果当前物品是活动物品，设置默认焦点
                            if (thisActive)
                                ImGui.SetItemDefaultFocus();

                            // 如果活动物品与上一次不同，滚动到该物品位置
                            if (thisActive && lastActiveItem != (ulong)Memory.HousingStructure->ActiveItem)
                            {
                                ImGui.SetScrollHereY();
                                // 记录滚动信息到日志
                                Plugin.Log.Info($"{ImGui.GetScrollY()} {ImGui.GetScrollMaxY()}");
                            }

                            // 在同一行绘制图标
                            ImGui.SameLine();
                            // 绘制物品图标
                            Plugin.DrawIcon(icon, new Vector2(24 * fontScale, 24 * fontScale));
                            // 计算物品与玩家的距离
                            var distance = Util.DistanceFromPlayer(items[i], playerPos);

                            // 切换到下一个表格列
                            ImGui.TableNextColumn();
                            // 设置下一个项目的宽度
                            ImGui.SetNextItemWidth(-1);
                            // 绘制物品名称
                            ImGui.Text(name);

                            // 切换到下一个表格列
                            ImGui.TableNextColumn();
                            // 设置文本颜色样式
                            ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(.5f, .5f, .5f, 1));
                            // 设置光标位置
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.GetColumnWidth() - ImGui.CalcTextSize(distance.ToString("F2")).X - ImGui.GetScrollX() - 2 * ImGui.GetStyle().ItemSpacing.X);
                            // 绘制物品与玩家的距离
                            ImGui.Text($"{distance:F2}");
                            // 恢复文本颜色样式
                            ImGui.PopStyleColor();
                        }

                        // 如果渲染计数达到10，记录当前活动物品的ID
                        if (renderCount >= 10)
                            lastActiveItem = (ulong)Memory.HousingStructure->ActiveItem;
                        // 如果渲染计数未达到10，增加渲染计数
                        if (renderCount != 10)
                            renderCount++;
                    }
                }
                catch (Exception ex)
                {
                    // 记录异常信息到日志
                    Plugin.Log.Error(ex, ex.Source ?? "未找到异常源");
                }
                finally
                {
                    // 结束表格绘制
                    ImGui.EndTable();
                    // 结束子窗口绘制
                    ImGui.EndChild();
                }
            }
        }
    }
}
