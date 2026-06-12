using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using cn.LammaForms.Config;

namespace cn.LammaForms.Controls
{
    /// <summary>
    /// 参数控件工厂
    /// 负责根据参数类型创建对应的 UI 控件
    /// 全部使用 TableLayoutPanel + Dock 布局，确保 DPI 缩放正确
    /// </summary>
    public static class ParamControlFactory
    {
        // 颜色配置 - 适配 WinForms 默认界面
        private static readonly Color HeaderBackColor = Color.FromArgb(240, 240, 240);      // 标题栏背景
        private static readonly Color HeaderTextColor = Color.FromArgb(64, 64, 64);         // 标题栏文字
        private static readonly Color ContentBackColor = Color.FromArgb(250, 250, 250);     // 内容区背景
        private static readonly Color BorderColor = Color.FromArgb(200, 200, 200);          // 边框颜色

        // 全局 ToolTip 组件，鼠标悬停时显示参数描述
        private static readonly ToolTip _toolTip = new ToolTip
        {
            InitialDelay = 300,      // 悬停 300ms 后显示
            ReshowDelay = 100,       // 切换控件后 100ms 显示
            AutoPopDelay = 15000,    // 显示持续 15 秒
            ShowAlways = true        // 即使父控件禁用也显示
        };

        /// <summary>
        /// 创建参数控件容器
        /// 使用 TableLayoutPanel 实现 DPI 自适应布局
        /// </summary>
        public static Panel CreateParamControl(SystemParamItem param, Action<string, string, bool> onValueChanged, Action<string, bool> onEnabledChanged)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = ContentBackColor
            };

            // 使用 TableLayoutPanel 实现 DPI 自适应布局
            var tlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(5, 0, 5, 0)
            };
            // 列宽: （CheckBox 固定25px）其余用百分比，DPI缩放时自动适配
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 25));   // CheckBox
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));    // Label（名称）
            tlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));    // 值控件（输入框/按钮）
            tlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            // 左侧勾选框
            var checkBox = new CheckBox
            {
                Name = $"cb_{param.Name.TrimStart('-').Replace("-", "_")}",
                Text = "",
                Checked = param.Enabled,
                Tag = param.Name,
                FlatStyle = FlatStyle.Standard,
                Dock = DockStyle.None,
                Anchor = AnchorStyles.Left | AnchorStyles.Top,
                Margin = new Padding(2, 12, 0, 0)
            };
            checkBox.CheckedChanged += (s, e) =>
            {
                var cb = (CheckBox)s!;
                onEnabledChanged(param.Name, cb.Checked);
            };

            // 参数名称标签
            var label = new Label
            {
                Text = string.IsNullOrEmpty(param.DisplayName) ? param.Name : param.DisplayName,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            tlp.Controls.Add(checkBox, 0, 0);
            tlp.Controls.Add(label, 1, 0);

            // 根据参数类型创建值输入控件
            Control? valueControl = null;

            switch (param.ValueType)
            {
                case ParamValueType.Boolean:
                    // 根据布尔参数的输出样式决定 UI 控件类型
                    bool isFlagOnly = SystemParamItem.GetBooleanOutputStylePublic(param.Name)
                        == SystemParamItem.BooleanOutputStyle.FlagOnly;
                    bool isFlashAttn = SystemParamItem.GetBooleanOutputStylePublic(param.Name)
                        == SystemParamItem.BooleanOutputStyle.FlashAttn;

                    if (isFlagOnly)
                    {
                        // 纯开关（--no-mmap, --jinja 等）：仅 CheckBox，不需要额外控件
                        break;
                    }

                    if (isFlashAttn)
                    {
                        // flash-attn：显示 ComboBox（on / off / auto）
                        var cbFlash = new ComboBox
                        {
                            Name = $"cb_{param.Name.TrimStart('-').Replace("-", "_")}",
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            FlatStyle = FlatStyle.Standard,
                            Items = { "on", "off", "auto" },
                            Tag = param.Name,
                            Dock = DockStyle.Fill,
                            Margin = new Padding(2, 4, 2, 4)
                        };
                        var currentFlashVal = (param.CurrentValue ?? "auto").ToLower().Trim();
                        if (cbFlash.Items.Contains(currentFlashVal))
                            cbFlash.SelectedItem = currentFlashVal;
                        else
                            cbFlash.SelectedIndex = 2;  // 默认 auto

                        cbFlash.SelectedIndexChanged += (s, e) =>
                        {
                            var cb = (ComboBox)s!;
                            var pName = (string)cb.Tag!;
                            var selVal = cb.SelectedItem?.ToString() ?? "auto";
                            onValueChanged(pName, selVal, true);
                        };
                        valueControl = cbFlash;
                        break;
                    }

                    // 其他带值布尔：显示 ComboBox（on / -1 / off）
                    var cbBool = new ComboBox
                    {
                        Name = $"cb_{param.Name.TrimStart('-').Replace("-", "_")}",
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        FlatStyle = FlatStyle.Standard,
                        Items = { "on", "-1", "off" },
                        Tag = param.Name,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2, 4, 2, 4)
                    };

                    // 设置当前选中项
                    var currentVal = (param.CurrentValue ?? "on").ToLower().Trim();
                    if (cbBool.Items.Contains(currentVal))
                        cbBool.SelectedItem = currentVal;
                    else
                        cbBool.SelectedIndex = 0;  // 默认 on

                    cbBool.SelectedIndexChanged += (s, e) =>
                    {
                        var cb = (ComboBox)s!;
                        var pName = (string)cb.Tag!;
                        var selVal = cb.SelectedItem?.ToString() ?? "on";
                        onValueChanged(pName, selVal, true);
                    };
                    valueControl = cbBool;
                    break;

                case ParamValueType.Integer:
                    if (param.Name == "--ctx-size")
                    {
                        // 上下文窗口大小：加宽输入框 + 快速选择下拉 + 加大行间距
                        // 使用 FlowLayoutPanel 自动排列，不再依赖固定 Left 坐标
                        panel.Height = 38;

                        var flowPanel = new FlowLayoutPanel
                        {
                            Dock = DockStyle.Fill,
                            FlowDirection = FlowDirection.LeftToRight,
                            WrapContents = false,
                            BackColor = ContentBackColor
                        };

                        // 加宽后的 NumericUpDown（110px 已足够显示 5242880）
                        var nudCtx = new NumericUpDown
                        {
                            Name = $"nud_{param.Name.TrimStart('-').Replace("-", "_")}",
                            Minimum = (decimal)(param.MinValue ?? 0),
                            Maximum = (decimal)(param.MaxValue ?? 999999),
                            Value = decimal.TryParse(param.CurrentValue, out var intCtxVal) ? (decimal)intCtxVal : (decimal)(param.MinValue ?? 0),
                            Enabled = param.Enabled,
                            Tag = param.Name,
                            BorderStyle = BorderStyle.FixedSingle,
                            Width = 120,
                            Margin = new Padding(3, 6, 4, 4)
                        };
                        nudCtx.ValueChanged += (s, e) =>
                        {
                            var nud = (NumericUpDown)s!;
                            onValueChanged(param.Name, nud.Value.ToString(), param.Enabled);
                        };

                        // 快速选择下拉框
                        var cbCtxQuick = new ComboBox
                        {
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            FlatStyle = FlatStyle.Standard,
                            IntegralHeight = false,
                            MaxDropDownItems = 15,
                            Width = 170,
                            Margin = new Padding(0, 6, 2, 4)
                        };

                        // 生成选项
                        var ctxValues = GenerateContextSizeOptions();
                        foreach (var v in ctxValues)
                        {
                            cbCtxQuick.Items.Add(FormatContextSize(v));
                        }

                        // 选中当前值
                        int currentCtxVal = int.TryParse(param.CurrentValue, out var pv) ? pv : 4096;
                        string currentDisplay = FormatContextSize(currentCtxVal);
                        if (cbCtxQuick.Items.Contains(currentDisplay))
                            cbCtxQuick.SelectedItem = currentDisplay;

                        cbCtxQuick.SelectedIndexChanged += (s, e) =>
                        {
                            if (cbCtxQuick.SelectedItem != null)
                            {
                                var selectedText = cbCtxQuick.SelectedItem.ToString()!;
                                var startIndex = selectedText.LastIndexOf('(') + 1;
                                var endIndex = selectedText.LastIndexOf(')');
                                if (startIndex > 0 && endIndex > startIndex)
                                {
                                    var valStr = selectedText.Substring(startIndex, endIndex - startIndex);
                                    if (decimal.TryParse(valStr, out var ctxVal))
                                    {
                                        nudCtx.Value = Math.Max(nudCtx.Minimum, Math.Min(nudCtx.Maximum, ctxVal));
                                        onValueChanged("--ctx-size", ctxVal.ToString(), true);
                                        onEnabledChanged("--ctx-size", true);
                                    }
                                }
                            }
                        };

                        flowPanel.Controls.Add(nudCtx);
                        flowPanel.Controls.Add(cbCtxQuick);
                        valueControl = flowPanel;
                    }
                    else
                    {
                        var nudInt = new NumericUpDown
                        {
                            Name = $"nud_{param.Name.TrimStart('-').Replace("-", "_")}",
                            Minimum = (decimal)(param.MinValue ?? 0),
                            Maximum = (decimal)(param.MaxValue ?? 999999),
                            Value = decimal.TryParse(param.CurrentValue, out var intVal) ? (decimal)intVal : (decimal)(param.MinValue ?? 0),
                            Enabled = param.Enabled,
                            Tag = param.Name,
                            BorderStyle = BorderStyle.FixedSingle,
                            Anchor = AnchorStyles.Left,
                            Width = 90,
                            Margin = new Padding(3, 4, 0, 4)
                        };
                        nudInt.ValueChanged += (s, e) =>
                        {
                            var nud = (NumericUpDown)s!;
                            onValueChanged(param.Name, nud.Value.ToString(), param.Enabled);
                        };
                        valueControl = nudInt;
                    }
                    break;

                case ParamValueType.Float:
                    // 张量拆分使用文本框支持多GPU比例（如 0.6,0.4）
                    if (param.Name == "--tensor-split")
                    {
                        var tbFloat = new TextBox
                        {
                            Name = $"tb_{param.Name.TrimStart('-').Replace("-", "_")}",
                            Text = param.CurrentValue ?? "",
                            Enabled = param.Enabled,
                            Tag = param.Name,
                            BorderStyle = BorderStyle.FixedSingle,
                            Dock = DockStyle.Fill,
                            Margin = new Padding(2, 4, 2, 4)
                        };
                        tbFloat.TextChanged += (s, e) =>
                        {
                            var textBox = (TextBox)s!;
                            var text = textBox.Text.Trim();

                            // 全角逗号转半角逗号
                            text = text.Replace('，', ',');
                            if (textBox.Text != text)
                            {
                                textBox.Text = text;
                                return; // 文本已变更，等待下一次事件
                            }

                            onValueChanged(param.Name, text, param.Enabled);
                        };
                        valueControl = tbFloat;
                    }
                    else
                    {
                        var nudFloat = new NumericUpDown
                        {
                            Name = $"nud_{param.Name.TrimStart('-').Replace("-", "_")}",
                            Minimum = (decimal)(param.MinValue ?? 0),
                            Maximum = (decimal)(param.MaxValue ?? 999999),
                            DecimalPlaces = 2,
                            Increment = (decimal)(param.Step ?? 0.1),
                            Value = decimal.TryParse(param.CurrentValue, out var floatVal) ? (decimal)floatVal : (decimal)(param.MinValue ?? 0),
                            Enabled = param.Enabled,
                            Tag = param.Name,
                            BorderStyle = BorderStyle.FixedSingle,
                            Anchor = AnchorStyles.Left,
                            Width = 90,
                            Margin = new Padding(3, 4, 0, 4)
                        };
                        nudFloat.ValueChanged += (s, e) =>
                        {
                            var nud = (NumericUpDown)s!;
                            onValueChanged(param.Name, nud.Value.ToString(), param.Enabled);
                        };
                        valueControl = nudFloat;
                    }
                    break;

                case ParamValueType.String:
                    var tb = new TextBox
                    {
                        Name = $"tb_{param.Name.TrimStart('-').Replace("-", "_")}",
                        Text = param.CurrentValue ?? "",
                        Enabled = param.Enabled,
                        Tag = param.Name,
                        BorderStyle = BorderStyle.FixedSingle,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2, 4, 2, 4)
                    };
                    tb.TextChanged += (s, e) =>
                    {
                        var textBox = (TextBox)s!;
                        onValueChanged(param.Name, textBox.Text, param.Enabled);
                    };
                    valueControl = tb;
                    break;

                case ParamValueType.FilePath:
                    var panelFile = new Panel
                    {
                        BackColor = ContentBackColor,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(2, 4, 2, 4)
                    };

                    var tbFile = new TextBox
                    {
                        Name = $"tb_{param.Name.TrimStart('-').Replace("-", "_")}",
                        Text = param.CurrentValue ?? "",
                        Enabled = param.Enabled,
                        Tag = param.Name,
                        ReadOnly = false,
                        BorderStyle = BorderStyle.FixedSingle,
                        Dock = DockStyle.Fill,
                        Margin = new Padding(0)
                    };
                    tbFile.TextChanged += (s, e) =>
                    {
                        var textBox = (TextBox)s!;
                        onValueChanged(param.Name, textBox.Text, param.Enabled);
                    };

                    var btnBrowse = new Button
                    {
                        Name = $"btn_{param.Name.TrimStart('-').Replace("-", "_")}",
                        Text = "浏览",
                        Enabled = param.Enabled,
                        Tag = param.Name,
                        Dock = DockStyle.Right,
                        Width = 50,
                        Margin = new Padding(2, 0, 0, 0),
                        FlatStyle = FlatStyle.Standard
                    };
                    btnBrowse.Click += (s, e) =>
                    {
                        using var dialog = new OpenFileDialog
                        {
                            Filter = "所有文件|*.*",
                            Title = $"选择 {param.DisplayName}"
                        };
                        var parentForm = (s as Control)?.FindForm();
                        if (dialog.ShowDialog(parentForm) == DialogResult.OK)
                        {
                            tbFile.Text = dialog.FileName;
                            onValueChanged(param.Name, dialog.FileName, param.Enabled);
                        }
                    };

                    panelFile.Controls.Add(tbFile);
                    panelFile.Controls.Add(btnBrowse);
                    valueControl = panelFile;
                    break;

                case ParamValueType.Enum:
                    if (param.Name == "--spec-type")
                    {
                        // 多选模式：可编辑文本框 + 下拉勾选弹窗
                        var multiPanel = new Panel
                        {
                            Dock = DockStyle.Fill,
                            BackColor = ContentBackColor,
                            Margin = new Padding(0)
                        };

                        var tbSpec = new TextBox
                        {
                            Name = "tb_spec_type",
                            Text = param.CurrentValue ?? "none",
                            BorderStyle = BorderStyle.FixedSingle,
                            Margin = new Padding(2, 4, 0, 4)
                        };
                        // 文本框 Fill，按钮 Dock=Right
                        var btnDrop = new Button
                        {
                            Text = "▾",
                            Width = 20,
                            Dock = DockStyle.Right,
                            FlatStyle = FlatStyle.Flat,
                            FlatAppearance = { BorderSize = 1 },
                            Margin = new Padding(0, 4, 2, 4),
                            Padding = new Padding(0),
                            Cursor = Cursors.Hand
                        };

                        tbSpec.Dock = DockStyle.Fill;
                        multiPanel.Controls.Add(btnDrop);
                        multiPanel.Controls.Add(tbSpec);

                        // 文本框编辑时同步到参数
                        tbSpec.TextChanged += (s, e) =>
                        {
                            onValueChanged(param.Name, tbSpec.Text, param.Enabled);
                        };

                        // 弹出多选勾选列表
                        btnDrop.Click += (s, e) =>
                        {
                            var currentValues = tbSpec.Text
                                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                .Select(x => x.Trim())
                                .ToHashSet();

                            var popup = new Form
                            {
                                StartPosition = FormStartPosition.Manual,
                                Location = btnDrop.PointToScreen(new Point(0, btnDrop.Height)),
                                FormBorderStyle = FormBorderStyle.FixedToolWindow,
                                ShowInTaskbar = false,
                                TopMost = true,
                                Size = new Size(200, 280),
                                ControlBox = false,
                                Padding = new Padding(4)
                            };
                            popup.Deactivate += (_, __) => popup.Close();

                            var clb = new CheckedListBox
                            {
                                Dock = DockStyle.Fill,
                                CheckOnClick = true,
                                IntegralHeight = false,
                                BorderStyle = BorderStyle.None
                            };

                            var opts = param.Options ?? new List<string>();
                            foreach (var opt in opts)
                            {
                                clb.Items.Add(opt, currentValues.Contains(opt));
                            }

                            var _syncing = false;
                            clb.ItemCheck += (_, ev) =>
                            {
                                if (_syncing) return;

                                var clickedItem = clb.Items[ev.Index]?.ToString() ?? "";
                                bool willBeChecked = ev.NewValue == CheckState.Checked;

                                // 规则：如果勾选了 "none"，则清除所有其他选项
                                //       如果勾选了其他选项，则必须取消 "none"
                                var selected = new List<string>();
                                for (int i = 0; i < clb.Items.Count; i++)
                                {
                                    var item = clb.Items[i]?.ToString() ?? "";
                                    bool isChecked;

                                    if (i == ev.Index)
                                        isChecked = willBeChecked;
                                    else
                                        isChecked = clb.GetItemChecked(i);

                                    if (!isChecked) continue;

                                    if (clickedItem == "none" && willBeChecked)
                                    {
                                        // 勾选 none → 只保留 none
                                        selected.Clear();
                                        selected.Add("none");
                                        break;
                                    }

                                    if (item != "none")
                                        selected.Add(item);
                                }

                                tbSpec.Text = selected.Count > 0 ? string.Join(",", selected) : "none";

                                // 延时同步 CLB 视觉勾选状态（ItemCheck 之后 CLB 才会真正切换）
                                clb.BeginInvoke(new Action(() =>
                                {
                                    _syncing = true;
                                    bool hasNone = selected.Contains("none");
                                    for (int i = 0; i < clb.Items.Count; i++)
                                    {
                                        var item = clb.Items[i]?.ToString() ?? "";
                                        bool shouldCheck = hasNone ? (item == "none") : selected.Contains(item);
                                        if (clb.GetItemChecked(i) != shouldCheck)
                                            clb.SetItemChecked(i, shouldCheck);
                                    }
                                    _syncing = false;
                                }));
                            };

                            popup.Controls.Add(clb);
                            popup.Show();
                        };

                        valueControl = multiPanel;
                    }
                    else
                    {
                        // 原有的单选下拉框模式
                        var cbEnum = new ComboBox
                        {
                            Name = $"cbe_{param.Name.TrimStart('-').Replace("-", "_")}",
                            DropDownStyle = ComboBoxStyle.DropDownList,
                            Enabled = param.Enabled,
                            Tag = param.Name,
                            FlatStyle = FlatStyle.Standard,
                            IntegralHeight = false,
                            MaxDropDownItems = 8,
                            Dock = DockStyle.Fill,
                            Margin = new Padding(2, 4, 2, 4)
                        };
                        if (param.Options != null)
                        {
                            cbEnum.Items.AddRange(param.Options.ToArray());
                        }
                        if (!string.IsNullOrEmpty(param.CurrentValue) && cbEnum.Items.Contains(param.CurrentValue))
                        {
                            cbEnum.SelectedItem = param.CurrentValue;
                        }
                        else if (cbEnum.Items.Count > 0)
                        {
                            cbEnum.SelectedIndex = 0;
                        }
                        cbEnum.SelectedIndexChanged += (s, ev) =>
                        {
                            var combo = (ComboBox)s!;
                            onValueChanged(param.Name, combo.SelectedItem?.ToString() ?? "", param.Enabled);
                        };
                        valueControl = cbEnum;
                    }
                    break;
            }

            if (valueControl != null)
            {
                tlp.Controls.Add(valueControl, 2, 0);
            }

            panel.Controls.Add(tlp);

            // 底部细分隔线
            var separator = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 1,
                BackColor = Color.FromArgb(225, 225, 225)
            };
            panel.Controls.Add(separator);

            // 添加鼠标悬停提示（ToolTip）
            if (!string.IsNullOrEmpty(param.Description))
            {
                string tipText = $"{param.DisplayName ?? param.Name}\n{param.Description}";
                if (!string.IsNullOrEmpty(param.DefaultValue))
                {
                    tipText += $"\n默认值: {param.DefaultValue}";
                }

                _toolTip.SetToolTip(label, tipText);
                if (valueControl != null)
                {
                    _toolTip.SetToolTip(valueControl, tipText);
                }
                _toolTip.SetToolTip(checkBox, tipText);
                _toolTip.SetToolTip(panel, tipText);
            }

            return panel;
        }

        /// <summary>
        /// 创建分类折叠面板（Expander）
        /// 使用 TableLayoutPanel 实现 DPI 自适应标题栏
        /// </summary>
        public static Panel CreateCategoryPanel(ParamCategory category, Action<string, string, bool> onValueChanged, Action<string, bool> onEnabledChanged)
        {
            // 主容器
            var mainPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                Padding = new Padding(0),
                Margin = new Padding(0, 1, 0, 1),
                BackColor = Color.White,
                BorderStyle = BorderStyle.None
            };

            // 标题栏 - 使用 TableLayoutPanel 实现 DPI 自适应
            var headerPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = HeaderBackColor,
                Cursor = Cursors.Hand
            };

            var headerTlp = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3,
                RowCount = 1,
                Padding = new Padding(8, 0, 8, 0)
            };
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 26));   // 图标列
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));   // 标题列
            headerTlp.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 30));   // 箭头列
            headerTlp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

            var iconLabel = new Label
            {
                Text = category.Icon,
                Font = new Font("Segoe UI Emoji", 11F),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };

            var titleLabel = new Label
            {
                Text = category.DisplayName,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = HeaderTextColor,
                Font = new Font("Microsoft YaHei UI", 9.5F, FontStyle.Bold)
            };

            var arrowLabel = new Label
            {
                Name = "arrow",
                Text = category.IsExpanded ? "▼" : "▶",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI Emoji", 10F, FontStyle.Regular)
            };

            headerTlp.Controls.Add(iconLabel, 0, 0);
            headerTlp.Controls.Add(titleLabel, 1, 0);
            headerTlp.Controls.Add(arrowLabel, 2, 0);
            headerPanel.Controls.Add(headerTlp);

            // 内容面板 - 初始隐藏
            var contentPanel = new Panel
            {
                Name = "contentPanel",
                Dock = DockStyle.Top,
                Visible = category.IsExpanded,
                BackColor = ContentBackColor,
                BorderStyle = BorderStyle.None
            };

            // 添加上下文大小快速选择（如果是上下文窗口分类）
            if (category.Name.Contains("核心基础"))
            {
                var quickSelect = CreateContextSizeQuickSelect(onValueChanged, onEnabledChanged, contentPanel);
                contentPanel.Controls.Add(quickSelect);
            }

            // 添加参数控件
            foreach (var param in category.Params)
            {
                var paramControl = CreateParamControl(param, onValueChanged, onEnabledChanged);
                contentPanel.Controls.Add(paramControl);
            }

            // 计算内容面板高度
            int contentHeight = 0;
            foreach (Control ctrl in contentPanel.Controls)
            {
                contentHeight += ctrl.Height;
            }
            contentHeight += 3;
            contentPanel.Height = contentHeight;

            mainPanel.Controls.Add(contentPanel);
            mainPanel.Controls.Add(headerPanel);

            // 展开/折叠功能
            void ToggleExpand()
            {
                category.IsExpanded = !category.IsExpanded;
                contentPanel.Visible = category.IsExpanded;
                arrowLabel.Text = category.IsExpanded ? "▼" : "▶";

                if (category.IsExpanded)
                {
                    mainPanel.Height = headerPanel.Height + contentPanel.Height;
                }
                else
                {
                    mainPanel.Height = headerPanel.Height;
                }

                mainPanel.PerformLayout();
                mainPanel.Parent?.PerformLayout();
            }

            void AttachClickEvents(Control control)
            {
                control.Click += (s, e) => ToggleExpand();
                foreach (Control child in control.Controls)
                {
                    // 避免递归进入内容面板的子控件（CheckBox 等不需要触发折叠）
                    if (child.Name != "contentPanel")
                    {
                        AttachClickEvents(child);
                    }
                }
            }

            // 为标题栏及其所有子控件添加点击展开/折叠
            AttachClickEvents(headerPanel);

            // 初始状态
            if (category.IsExpanded)
            {
                mainPanel.Height = headerPanel.Height + contentPanel.Height;
            }
            else
            {
                mainPanel.Height = headerPanel.Height;
            }

            return mainPanel;
        }

        /// <summary>
        /// 创建上下文大小快速选择面板
        /// </summary>
        private static Panel CreateContextSizeQuickSelect(Action<string, string, bool> onValueChanged, Action<string, bool> onEnabledChanged, Panel contentPanel)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 38,
                BackColor = ContentBackColor
            };

            // 使用 TableLayoutPanel 实现 DPI 自适应布局

            return panel;
        }

        /// <summary>
        /// 递归查找并更新 NumericUpDown 控件
        /// </summary>
        private static void FindAndUpdateNumericUpDown(Control parent, string name, string value)
        {
            if (parent.Name == name && parent is NumericUpDown nud)
            {
                if (decimal.TryParse(value, out var decValue))
                {
                    nud.Value = Math.Max(nud.Minimum, Math.Min(nud.Maximum, decValue));
                }
                return;
            }

            foreach (Control child in parent.Controls)
            {
                FindAndUpdateNumericUpDown(child, name, value);
            }
        }

        /// <summary>
        /// 递归查找并更新 CheckBox 控件
        /// </summary>
        private static void FindAndUpdateCheckBox(Control parent, string name)
        {
            if (parent.Name == name && parent is CheckBox cb)
            {
                cb.Checked = true;
                return;
            }

            foreach (Control child in parent.Controls)
            {
                FindAndUpdateCheckBox(child, name);
            }
        }

        /// <summary>
        /// 生成上下文大小快速选择下拉选项值列表
        /// 包含：4K, 8K, 16K, 32K, 64K~960K(每32K), 1M, 1.125M~3M(每128K), 5M
        /// </summary>
        private static int[] GenerateContextSizeOptions()
        {
            var list = new System.Collections.Generic.List<int>();

            // 固定项：4K, 8K, 16K, 32K
            list.Add(4096);
            list.Add(8192);
            list.Add(16384);
            list.Add(32768);

            // 64K ~ 960K 每 32K 步进
            for (int k = 64; k <= 960; k += 32)
                list.Add(k * 1024);

            // 1M
            list.Add(1048576);

            // 1.125M ~ 3M 每 128K 步进
            for (int k = 1152; k <= 3072; k += 128)
                list.Add(k * 1024);

            // 5M
            list.Add(5242880);

            return list.ToArray();
        }

        /// <summary>
        /// 格式化上下文大小值为显示字符串
        /// &lt;1M: "128K (131072)"  =1M: "1M (1048576)"  &gt;1M: "1.125M (1179648)"
        /// </summary>
        private static string FormatContextSize(int value)
        {
            if (value < 1048576)
            {
                // 小于 1M，显示为 K
                return $"{value / 1024}K ({value})";
            }
            else if (value == 1048576)
            {
                // 正好 1M
                return $"1M ({value})";
            }
            else
            {
                // 大于 1M，显示为带小数的 M
                double mVal = value / 1048576.0;
                return $"{mVal:F3}M ({value})";
            }
        }
    }
}
