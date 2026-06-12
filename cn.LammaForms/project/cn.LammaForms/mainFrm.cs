using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using cn.LammaForms.Config;
using cn.LammaForms.Controls;

namespace cn.LammaForms
{
    public partial class mainFrm : Form
    {
        /// <summary>
        /// 配置管理器
        /// </summary>
        private readonly ConfigManager _configManager;

        /// <summary>
        /// 系统参数管理器
        /// </summary>
        private readonly SystemParamManager _paramManager;

        /// <summary>
        /// 参数控件字典，用于快速查找
        /// </summary>
        private readonly Dictionary<string, Control> _paramControls = new Dictionary<string, Control>();

        /// <summary>
        /// 是否正在初始化（防止初始化时触发保存）
        /// </summary>
        private bool _isInitializing = false;


        /// <summary>
        /// 模型文件路径字典 Key=显示名称, Value=完整物理路径（支持子目录）
        /// </summary>
        private readonly Dictionary<string, string> _modelPathDict = new Dictionary<string, string>();

        /// <summary>
        /// 最小模型文件大小阈值（1.2GB）
        /// </summary>
        private const long MinModelFileSizeBytes = (long)(1.2 * 1024 * 1024 * 1024);



        /// <summary>
        /// llama-server 进程引用
        /// </summary>
        private Process? _llamaServerProcess;

        /// <summary>
        /// Web 服务是否正在运行
        /// </summary>
        private bool _isWebServerRunning = false;

        /// <summary>
        /// Web 服务是否正在启动中
        /// </summary>
        private bool _isServerStarting = false;

        /// <summary>
        /// 批量测速是否正在运行
        /// </summary>
        private bool _isRunning = false;

        /// <summary>
        /// 是否请求停止批量测速
        /// </summary>
        private bool _stopTest = false;

        /// <summary>
        /// 当前检测到的GPU显卡列表（从llama.cpp输出中解析）
        /// </summary>
        private readonly List<GpuInfo> _gpuInfos = new();
        private readonly object _gpuLock = new object();

        /// <summary>
        /// CMD 对话测试是否已成功执行过（用于保存启动脚本前的前置校验）
        /// </summary>
        private bool _chatTestExecuted = false;

        public mainFrm()
        {
            _isInitializing = true;
            InitializeComponent();
            
            _configManager = ConfigManager.Instance;
            _paramManager = SystemParamManager.Instance;

            // 初始化配置
            InitializeConfig();

            // 恢复分隔条位置
            RestoreSplitterPositions();
            
            // 绑定分隔条移动保存事件
            splitContainer.SplitterMoved += (s, e) => SaveSplitterPosition();
            splitContainer_left.SplitterMoved += (s, e) => SaveSplitterPositionLeft();
            
            // 初始化参数面板
            InitializeParamPanel();
            
            // 绑定事件
            BindConfigEvents();
            
            _isInitializing = false;
        }

        #region 配置初始化与加载

        /// <summary>
        /// 初始化配置 - 加载 config.json 并填充到 UI
        /// </summary>
        private void InitializeConfig()
        {
            var config = _configManager.Config;

            // 加载 llama.cpp 路径
            tb_llamaPath.Text = config.LlamaPath;
            ValidateLlamaPath(config.LlamaPath);

            // 加载模型路径
            tb_ggufModelsPath.Text = config.GgufModelsPath;

            // 加载并验证模型
            if (!string.IsNullOrEmpty(config.GgufModelsPath) && Directory.Exists(config.GgufModelsPath))
            {
                RefreshModelList();

                // 设置选中的模型（支持子目录路径）
                if (!string.IsNullOrEmpty(config.SelectedModel))
                {
                    // 优先精确匹配显示名称
                    var found = false;
                    for (int i = 0; i < cb_ggufModel.Items.Count; i++)
                    {
                        if (string.Equals(cb_ggufModel.Items[i]?.ToString(), config.SelectedModel, StringComparison.OrdinalIgnoreCase))
                        {
                            cb_ggufModel.SelectedIndex = i;
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                    {
                        // 尝试仅匹配文件名部分（兼容旧配置）
                        var fileNameOnly = Path.GetFileName(config.SelectedModel);
                        for (int i = 0; i < cb_ggufModel.Items.Count; i++)
                        {
                            if (string.Equals(Path.GetFileName(cb_ggufModel.Items[i]?.ToString()), fileNameOnly, StringComparison.OrdinalIgnoreCase))
                            {
                                cb_ggufModel.SelectedIndex = i;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found)
                    {
                        LogMessage($"警告: 上次选中的模型不存在: {config.SelectedModel}");
                    }
                }
            }

            // 加载多模态投影模型
            tb_mmprojFile.Text = config.MmprojFile;

            // 加载 MTP 模型文件
            tb_mtpFile.Text = config.MtpFile;

            // 加载服务器配置
            tb_apiUrl.Text = config.Server.ApiUrl;
            tb_apiKey.Text = config.Server.ApiKey;
            tb_modelID.Text = config.Server.ModelId;
            cb_startConfig_local.Checked = config.Server.IsLocal;

            // 加载启用的参数到 webConfig
            UpdateWebConfigFromParams();
        }

        /// <summary>
        /// 绑定配置变更事件
        /// </summary>
        private void BindConfigEvents()
        {
            // 路径文本框变更事件
            tb_llamaPath.TextChanged += (s, e) => SaveConfigDebounced();
            tb_ggufModelsPath.TextChanged += (s, e) => SaveConfigDebounced();
            tb_mmprojFile.TextChanged += (s, e) => SaveConfigDebounced();
            tb_mtpFile.TextChanged += (s, e) => SaveConfigDebounced();

            // 模型选择变更（自动匹配 mmproj + 保存配置 + 更新 Tooltip）
            cb_ggufModel.SelectedIndexChanged += (s, e) =>
            {
                if (!_isInitializing)
                {
                    var displayName = cb_ggufModel.SelectedItem?.ToString() ?? "";

                    // 获取模型完整路径
                    string fullModelPath = "";
                    if (_modelPathDict.TryGetValue(displayName, out var path))
                    {
                        fullModelPath = path;
                    }
                    else
                    {
                        // 兼容旧逻辑：如果字典中没有，尝试直接拼接
                        fullModelPath = Path.Combine(_configManager.Config.GgufModelsPath, displayName);
                    }

                    // 更新 Tooltip 显示完整路径
                    toolTip.SetToolTip(cb_ggufModel, string.IsNullOrEmpty(fullModelPath)
                        ? displayName
                        : $"{displayName}\n{fullModelPath}");

                    // 保存选中模型到配置
                    _configManager.UpdateAndSave(cfg => cfg.SelectedModel = displayName);

                    // 自动设置 Model ID（从文件名提取，去掉 .gguf 后缀和路径）
                    var modelFileName = Path.GetFileNameWithoutExtension(displayName);
                    if (!string.IsNullOrEmpty(modelFileName))
                    {
                        _isInitializing = true; // 防止 TextChanged 触发重复保存
                        tb_modelID.Text = modelFileName;
                        _isInitializing = false;
                        _configManager.UpdateAndSave(cfg => cfg.Server.ModelId = modelFileName);
                        LogMessage($"已自动设置 Model ID: {modelFileName}");
                    }

                    // 自动匹配 mmproj 投影文件
                    AutoMatchMmprojFile(fullModelPath);

                    // 自动匹配 MTP 模型文件
                    AutoMatchMtpFile(fullModelPath);
                    
                    // 切换模型时，如果当前 Web 服务正在运行，需要重置统计面板
                    // 如果服务未运行或已关闭，重置操作可避免影响后续状态
                    ResetTokenStats();
                }
            };

            // 服务器配置变更
            tb_apiUrl.TextChanged += (s, e) => SaveConfigDebounced();
            tb_apiKey.TextChanged += (s, e) => SaveConfigDebounced();
            tb_modelID.TextChanged += (s, e) => SaveConfigDebounced();
            cb_startConfig_local.CheckedChanged += (s, e) => SaveConfigDebounced();

            // API URL 地址和端口联动
            tb_apiUrl_address.TextChanged += (s, e) => UpdateApiUrl();
            tb_apiUrl_port.TextChanged += (s, e) => UpdateApiUrl();
        }

        /// <summary>
        /// 延迟保存配置（避免频繁写入）
        /// </summary>
        private void SaveConfigDebounced()
        {
            if (_isInitializing) return;

            // 使用简单的延迟保存，实际项目中可以使用 Timer
            _configManager.UpdateAndSave(cfg =>
            {
                cfg.LlamaPath = tb_llamaPath.Text.Trim();
                cfg.GgufModelsPath = tb_ggufModelsPath.Text.Trim();
                cfg.MmprojFile = tb_mmprojFile.Text.Trim();
                cfg.MtpFile = tb_mtpFile.Text.Trim();
                cfg.Server.ApiUrl = tb_apiUrl.Text.Trim();
                cfg.Server.ApiKey = tb_apiKey.Text.Trim();
                cfg.Server.ModelId = tb_modelID.Text.Trim();
                cfg.Server.IsLocal = cb_startConfig_local.Checked;
            });
        }

        #endregion

        #region 参数面板初始化

        /// <summary>
        /// 初始化参数面板 - 使用 Dock=Top 自动堆叠，确保 DPI 缩放正确
        /// </summary>
        private void InitializeParamPanel()
        {
            gp_model_options.Controls.Clear();
            _paramControls.Clear();

            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                BackColor = SystemColors.Control
            };

            // 按 sortOrder 升序排列，然后反转——因为面板用 Dock=Top 堆叠，
            // 列表第一个会显示在最底部，最后一个显示在最顶部。
            // 我们希望"一"在最上面、"十一"在最下面，所以反转排序结果。
            foreach (var category in _paramManager.Categories.OrderBy(c => c.SortOrder).Reverse())
            {
                var categoryPanel = ParamControlFactory.CreateCategoryPanel(
                    category,
                    OnParamValueChanged,
                    OnParamEnabledChanged
                );

                panel.Controls.Add(categoryPanel);
            }

            gp_model_options.Controls.Add(panel);
        }

        /// <summary>
        /// 参数值变更回调
        /// </summary>
        private void OnParamValueChanged(string paramName, string value, bool enabled)
        {
            _paramManager.UpdateParam(paramName, value, enabled);
            UpdateWebConfigFromParams();
        }

        /// <summary>
        /// 参数启用状态变更回调
        /// </summary>
        private void OnParamEnabledChanged(string paramName, bool enabled)
        {
            var param = _paramManager.FindParam(paramName);
            if (param != null)
            {
                param.Enabled = enabled;
                _paramManager.SaveSystemJson();
                UpdateWebConfigFromParams();
            }

            // 启用/禁用对应的值控件（NumericUpDown）
            var nudName = $"nud_{paramName.TrimStart('-').Replace("-", "_")}";
            if (gp_model_options.Controls.Find(nudName, true).FirstOrDefault() is Control nudCtrl)
            {
                nudCtrl.Enabled = enabled;
            }

            // 启用/禁用对应的值控件（TextBox）
            var tbName = $"tb_{paramName.TrimStart('-').Replace("-", "_")}";
            if (gp_model_options.Controls.Find(tbName, true).FirstOrDefault() is Control tbCtrl)
            {
                tbCtrl.Enabled = enabled;
            }

            // 启用/禁用对应的值控件（ComboBox / 枚举类型）
            var cbeName = $"cbe_{paramName.TrimStart('-').Replace("-", "_")}";
            if (gp_model_options.Controls.Find(cbeName, true).FirstOrDefault() is Control cbeCtrl)
            {
                cbeCtrl.Enabled = enabled;
            }
        }

        /// <summary>
        /// 从启用的参数更新 webConfig 文本框
        /// </summary>
        private void UpdateWebConfigFromParams()
        {
            var sb = new StringBuilder();
            
            foreach (var category in _paramManager.Categories)
            {
                var enabledParams = category.Params.Where(p => p.Enabled).ToList();
                if (enabledParams.Count == 0) continue;

                sb.AppendLine($"# {category.DisplayName}");
                
                foreach (var param in enabledParams)
                {
                    string arg = param.GetCommandLineArg();
                    if (!string.IsNullOrEmpty(arg))
                    {
                        sb.AppendLine(arg);
                    }
                }
                
                sb.AppendLine();
            }

            tb_box_webConfig.Text = sb.ToString().TrimEnd();
        }

        /// <summary>
        /// 从 webConfig 文本框解析参数并同步到面板
        /// 功能：1) 解析输入的启动参数并自动勾选/设置对应控件
        ///       2) 将原始输入自动格式化为每行一个参数 + 显示名称注释，方便后续编辑
        /// </summary>
        private void SyncParamsFromWebConfig()
        {
            // 构建参数查找表：Name -> param, ShortName -> param
            var paramLookup = new Dictionary<string, SystemParamItem>(StringComparer.OrdinalIgnoreCase);
            foreach (var category in _paramManager.Categories)
            {
                foreach (var param in category.Params)
                {
                    if (!string.IsNullOrEmpty(param.Name))
                        paramLookup[param.Name] = param;
                    if (!string.IsNullOrEmpty(param.ShortName))
                        paramLookup[param.ShortName] = param;
                }
            }

            // 第一步：解析原始文本中的所有参数 token
            var rawText = tb_box_webConfig.Text.Trim();
            if (string.IsNullOrEmpty(rawText))
            {
                LogMessage("启动参数为空，无需解析");
                return;
            }

            var parsedArgs = ParseCommandArgs(rawText);

            // 第二步：匹配已知参数并收集结果
            var matchedResults = new List<(SystemParamItem param, string longName, string value, int originalIndex)>();
            var unmatchedLines = new List<string>(); // 未识别的原始行

            foreach (var (token, value, idx) in parsedArgs)
            {
                if (paramLookup.TryGetValue(token, out var foundParam))
                {
                    // 使用长名称作为 key
                    var longName = foundParam.Name;
                    matchedResults.Add((foundParam, longName, value, idx));
                }
                else
                {
                    unmatchedLines.Add(string.IsNullOrEmpty(value)
                        ? token
                        : $"{token} {value}");
                }
            }

            // 第三步：更新参数管理器 - 先全部取消勾选，再逐个启用已识别的参数
            foreach (var category in _paramManager.Categories)
            {
                foreach (var param in category.Params)
                {
                    param.Enabled = false;
                    // 不重置 CurrentValue，保留用户之前的值
                }
            }

            // 启用识别到的参数并设置值
            foreach (var (param, _, value, _) in matchedResults)
            {
                param.Enabled = true;
                if (param.ValueType == ParamValueType.Boolean)
                {
                    // 布尔类型使用专门的解析方法，支持 1/on/0/off/-1
                    param.ParseAndSetBooleanValue(value);
                }
                else if (!string.IsNullOrEmpty(value))
                {
                    param.CurrentValue = value;
                }
            }

            _paramManager.SaveSystemJson();

            // 第四步：生成格式化后的输出（每行一个参数 + 注释）
            var sb = new StringBuilder();

            // 按分类分组输出
            var byCategory = matchedResults
                .GroupBy(r => r.param.Category)
                .OrderBy(g =>
                {
                    var cat = _paramManager.Categories.FirstOrDefault(c => c.Name == g.Key);
                    return cat?.SortOrder ?? 999;
                })
                .ToList();

            foreach (var group in byCategory)
            {
                var category = _paramManager.Categories.FirstOrDefault(c => c.Name == group.Key);
                if (category != null)
                {
                    sb.AppendLine($"# {category.DisplayName}");
                }

                foreach (var (param, longName, value, _) in group.OrderBy(r => r.param.SortOrder))
                {
                    if (param.ValueType == ParamValueType.Boolean)
                    {
                        // 布尔参数按输出样式决定是否带值
                        var style = SystemParamItem.GetBooleanOutputStylePublic(longName);
                        if (style == SystemParamItem.BooleanOutputStyle.FlashAttn)
                        {
                            // flash-attn 必须输出值：--flash-attn on / off / auto
                            var faVal = string.IsNullOrEmpty(value) ? param.CurrentValue : value;
                            sb.AppendLine($"{longName} {faVal}   # {param.DisplayName}");
                        }
                        else if (style == SystemParamItem.BooleanOutputStyle.FlagOnly)
                        {
                            // 纯开关：仅输出参数名
                            sb.AppendLine($"{longName}   # {param.DisplayName}");
                        }
                        else
                        {
                            // 默认带值开关
                            if (string.IsNullOrEmpty(value))
                                sb.AppendLine($"{longName}   # {param.DisplayName}");
                            else
                                sb.AppendLine($"{longName} {value}   # {param.DisplayName}");
                        }
                    }
                    else if (string.IsNullOrEmpty(value))
                    {
                        sb.AppendLine($"{longName}   # {param.DisplayName}");
                    }
                    else
                    {
                        sb.AppendLine($"{longName} {value}   # {param.DisplayName}");
                    }
                }

                sb.AppendLine();
            }

            // 输出未识别的参数
            if (unmatchedLines.Count > 0)
            {
                sb.AppendLine("# === 未识别的参数 ===");
                foreach (var line in unmatchedLines)
                {
                    sb.AppendLine(line);
                }
                sb.AppendLine();
            }

            // 写回文本框
            tb_box_webConfig.Text = sb.ToString().TrimEnd();

            // 刷新面板
            InitializeParamPanel();

            // 统计日志
            int totalParsed = parsedArgs.Count;
            int matched = matchedResults.Count;
            int unmatched = unmatchedLines.Count;
            LogMessage($"解析完成: 共 {totalParsed} 个参数, 匹配 {matched} 个, 未识别 {unmatched} 个");
        }

        /// <summary>
        /// 解析命令行参数字符串为 (参数名, 参数值, 原始位置索引) 列表
        /// 支持: --long-name value, --name=value, -s value, -s=value, 布尔标志(无值)
        /// </summary>
        private static List<(string token, string value, int index)> ParseCommandArgs(string text)
        {
            var result = new List<(string token, string value, int index)>();

            // 按空格分词，但保留引号内容
            var tokens = TokenizeCommandLine(text).ToList();

            for (int i = 0; i < tokens.Count; i++)
            {
                var current = tokens[i];

                // 跳过注释和空行标记
                if (current.StartsWith("#"))
                    continue;

                // 处理 = 号连接: --name=value 或 -n=value
                if (current.Contains("="))
                {
                    var eqIdx = current.IndexOf('=');
                    var name = current.Substring(0, eqIdx);
                    var val = current.Substring(eqIdx + 1);
                    result.Add((name.Trim(), val.Trim(), i));
                    continue;
                }

                // 以 - 开头的是参数名
                if (current.StartsWith("-"))
                {
                    // 检查下一个 token 是否是参数值（不以 - 开头且不为空）
                    if (i + 1 < tokens.Count &&
                        !tokens[i + 1].StartsWith("-") &&
                        !tokens[i + 1].StartsWith("#") &&
                        !string.IsNullOrWhiteSpace(tokens[i + 1]))
                    {
                        result.Add((current, tokens[i + 1], i));
                        i++; // 跳过值 token
                    }
                    else
                    {
                        // 布尔标志或无值的参数
                        result.Add((current, "", i));
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// 将命令行字符串按空格分词，保留引号内的内容
        /// </summary>
        private static IEnumerable<string> TokenizeCommandLine(string text)
        {
            var tokens = new List<string>();
            bool inQuotes = false;
            var current = new StringBuilder();

            foreach (char ch in text)
            {
                if (ch == '"' || ch == '\'')
                {
                    inQuotes = !inQuotes;
                    continue;
                }

                if (ch == '\r' || ch == '\n')
                {
                    // 行结束，提交当前 token
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }

                if (char.IsWhiteSpace(ch) && !inQuotes)
                {
                    if (current.Length > 0)
                    {
                        tokens.Add(current.ToString());
                        current.Clear();
                    }
                    continue;
                }

                current.Append(ch);
            }

            if (current.Length > 0)
                tokens.Add(current.ToString());

            return tokens;
        }

        #endregion

        #region 路径验证与模型管理

        /// <summary>
        /// 验证 llama.cpp 路径
        /// </summary>
        private void ValidateLlamaPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                tb_llamaPath.BackColor = Color.White;
                return;
            }

            var result = _configManager.ValidateLlamaPath(path);
            
            if (result.IsValid)
            {
                tb_llamaPath.BackColor = result.Message.Contains("警告") ? Color.LightYellow : Color.LightGreen;
                if (result.Message.Contains("警告"))
                {
                    LogMessage(result.Message);
                }
            }
            else
            {
                tb_llamaPath.BackColor = Color.LightCoral;
                LogMessage($"路径验证失败: {result.Message}");
            }
        }

        /// <summary>
        /// 刷新模型列表 - 递归搜索子目录，过滤大于1.2GB的GGUF模型文件（排除mmproj），自动匹配mmproj
        /// </summary>
        private void RefreshModelList()
        {
            var modelsPath = tb_ggufModelsPath.Text.Trim();

            if (string.IsNullOrEmpty(modelsPath) || !Directory.Exists(modelsPath))
            {
                cb_ggufModel.Items.Clear();
                _modelPathDict.Clear();
                return;
            }

            cb_ggufModel.Items.Clear();
            _modelPathDict.Clear();
            
            try
            {
                // 递归搜索所有子目录中的 .gguf 文件
                var allGgufFiles = Directory.GetFiles(modelsPath, "*.gguf", SearchOption.AllDirectories);

                // 过滤文件大小 > 1.2GB 的模型文件，排除 mmproj 文件
                var modelFiles = allGgufFiles
                    .Where(f =>
                    {
                        try
                        {
                            // 排除包含 mmproj 的文件（不显示在模型列表中）
                            var fileName = Path.GetFileName(f);
                            if (fileName.Contains("mmproj", StringComparison.OrdinalIgnoreCase))
                                return false;
                            
                            var length = new FileInfo(f).Length;
                            return length >= MinModelFileSizeBytes;
                        }
                        catch { return false; }
                    })
                    .OrderBy(f => f)
                    .ToArray();

                foreach (var fullPath in modelFiles)
                {
                    // 生成显示名称：如果是在子目录中，显示 "子目录名\文件名"
                    var dirPath = Path.GetDirectoryName(fullPath) ?? "";
                    var fileName = Path.GetFileName(fullPath);
                    string displayName;

                    if (string.Equals(dirPath, modelsPath, StringComparison.OrdinalIgnoreCase))
                    {
                        displayName = fileName; // 根目录，只显示文件名
                    }
                    else
                    {
                        var relativeDir = Path.GetRelativePath(modelsPath, dirPath);
                        displayName = Path.Combine(relativeDir, fileName); // 显示相对路径
                    }

                    cb_ggufModel.Items.Add(displayName);
                    _modelPathDict[displayName] = fullPath;
                }
                tb_mmprojFile.Text = "";
                cb_ggufModel.Text = "";

                LogMessage($"找到 {modelFiles.Length} 个有效 GGUF 模型文件（>{MinModelFileSizeBytes / (1024.0*1024*1024):0.0}GB），共扫描 {allGgufFiles.Length} 个 gguf 文件");
            }
            catch (Exception ex)
            {
                LogMessage($"刷新模型列表失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 自动匹配 mmproj 投影文件到 tb_mmprojFile
        /// 规则：
        /// a) 当前目录只有2个gguf文件：大的是模型文件，小的是mmproj
        /// b) 多个文件时：查找文件名包含"mmproj"的文件，并匹配主模型文件名前缀
        /// </summary>
        /// <param name="modelFullPath">选中的模型文件完整路径</param>
        private void AutoMatchMmprojFile(string modelFullPath)
        {
            if (string.IsNullOrEmpty(modelFullPath) || !File.Exists(modelFullPath))
            {
                tb_mmprojFile.Text = "";
                return;
            }

            var modelDir = Path.GetDirectoryName(modelFullPath);
            if (string.IsNullOrEmpty(modelDir)) return;

            try
            {
                // 获取同目录下所有 gguf 文件（不限制大小）
                var ggufFiles = Directory.GetFiles(modelDir, "*.gguf", SearchOption.TopDirectoryOnly);

                string? mmprojPath = null;

                if (ggufFiles.Length == 2)
                {
                    // 规则 a：目录中只有 2 个 gguf 文件，小的是 mmproj
                    var sorted = ggufFiles
                        .Select(f => new { Path = f, Size = new FileInfo(f).Length })
                        .OrderBy(x => x.Size)
                        .ToArray();

                    // 小文件即为 mmproj
                    mmprojPath = sorted[0].Path;
                    LogMessage($"自动匹配 mmproj（2文件规则）: {Path.GetFileName(mmprojPath)}");
                }
                else if (ggufFiles.Length > 2)
                {
                    // 规则 b：多文件时，查找包含 "mmproj" 的文件，并匹配主模型文件名前缀
                    var modelFileName = Path.GetFileNameWithoutExtension(modelFullPath);
                    var mmprojCandidates = ggufFiles
                        .Where(f => Path.GetFileName(f).Contains("mmproj", StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (mmprojCandidates.Count == 1)
                    {
                        // 只有一个包含 mmproj 的文件
                        mmprojPath = mmprojCandidates[0];
                        LogMessage($"自动匹配 mmproj（唯一匹配）: {Path.GetFileName(mmprojPath)}");
                    }
                    else if (mmprojCandidates.Count > 1)
                    {
                        // 多个包含 mmproj 的文件，按模型文件名前缀匹配
                        // 提取模型前缀（如 Qwen2.5-VL-7B-Instruct 从 Qwen2.5-VL-7B-Instruct-Q4_K_M.gguf）
                        var mmprojMatched = mmprojCandidates.FirstOrDefault(f =>
                        {
                            var mmprojName = Path.GetFileNameWithoutExtension(f);
                            return modelFileName.StartsWith(mmprojName.Replace("-mmproj", ""), StringComparison.OrdinalIgnoreCase)
                                || mmprojName.Replace("-mmproj", "").StartsWith(modelFileName.Split('-')[0], StringComparison.OrdinalIgnoreCase);
                        });

                        if (mmprojMatched != null)
                        {
                            mmprojPath = mmprojMatched;
                            LogMessage($"自动匹配 mmproj（前缀匹配）: {Path.GetFileName(mmprojPath)}");
                        }
                        else
                        {
                            // 最后兜底：取文件名最接近主模型的 mmproj
                            var modelPrefix = modelFileName.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";
                            mmprojPath = mmprojCandidates.FirstOrDefault(f =>
                                Path.GetFileName(f).Contains(modelPrefix, StringComparison.OrdinalIgnoreCase));
                            
                            if (mmprojPath != null)
                            {
                                LogMessage($"自动匹配 mmproj（前缀兜底）: {Path.GetFileName(mmprojPath)}");
                            }
                        }
                    }
                }

                // 更新 tb_mmprojFile 控件
                if (mmprojPath != null)
                {
                    tb_mmprojFile.Text = mmprojPath;
                    _configManager.UpdateAndSave(cfg => cfg.MmprojFile = mmprojPath);
                }
                else
                {
                    // 没有找到 mmproj，清空控件并保存
                    tb_mmprojFile.Text = "";
                    _configManager.UpdateAndSave(cfg => cfg.MmprojFile = "");
                    LogMessage("未找到匹配的 mmproj 投影文件，已自动清空");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"自动匹配 mmproj 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 自动匹配 MTP 模型文件到 tb_mtpFile
        /// 规则：在同目录下查找 mtp 或 MTP 相关的 gguf 文件
        /// 优先级：1) 文件名包含 "assistant-MTP"  2) 文件名包含 "MTP" 或 "mtp"
        /// </summary>
        /// <param name="modelFullPath">选中的模型文件完整路径</param>
        private void AutoMatchMtpFile(string modelFullPath)
        {
            if (string.IsNullOrEmpty(modelFullPath) || !File.Exists(modelFullPath))
            {
                tb_mtpFile.Text = "";
                return;
            }

            var modelDir = Path.GetDirectoryName(modelFullPath);
            if (string.IsNullOrEmpty(modelDir)) return;

            try
            {
                // 获取同目录下所有 gguf 文件
                var ggufFiles = Directory.GetFiles(modelDir, "*.gguf", SearchOption.TopDirectoryOnly);

                string? mtpPath = null;

                // 优先匹配文件名包含 "MTP" 或 "mtp" 的文件
                var mtpCandidates = ggufFiles
                    .Where(f => Path.GetFileName(f).Contains("MTP", StringComparison.OrdinalIgnoreCase) ||
                                Path.GetFileName(f).Contains("mtp", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (mtpCandidates.Count == 1)
                {
                    mtpPath = mtpCandidates[0];
                    LogMessage($"自动匹配 MTP 文件（唯一匹配）: {Path.GetFileName(mtpPath)}");
                }
                else if (mtpCandidates.Count > 1)
                {
                    // 有多个候选时，优先选包含 "assistant-MTP" 的文件
                    var assistantMtp = mtpCandidates.FirstOrDefault(f =>
                        Path.GetFileName(f).Contains("assistant-MTP", StringComparison.OrdinalIgnoreCase));
                    if (assistantMtp != null)
                    {
                        mtpPath = assistantMtp;
                        LogMessage($"自动匹配 MTP 文件（assistant-MTP 优先）: {Path.GetFileName(mtpPath)}");
                    }
                    else
                    {
                        // 取第一个
                        mtpPath = mtpCandidates[0];
                        LogMessage($"自动匹配 MTP 文件（多个中取第一个）: {Path.GetFileName(mtpPath)}");
                    }
                }

                // 更新 tb_mtpFile 控件
                if (mtpPath != null)
                {
                    tb_mtpFile.Text = mtpPath;
                    _configManager.UpdateAndSave(cfg => cfg.MtpFile = mtpPath);
                }
                else
                {
                    // 没有找到 MTP 文件，清空控件并保存
                    tb_mtpFile.Text = "";
                    _configManager.UpdateAndSave(cfg => cfg.MtpFile = "");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"自动匹配 MTP 失败: {ex.Message}");
            }
        }

        #region Web 服务管理

        /// <summary>
        /// 检查是否为快速重复点击（1秒内多次点击只执行第一次）
        /// </summary>
        private static DateTime _lastClickTime;
        private bool IsRapidClick()
        {
            var now = DateTime.Now;
            if ((now - _lastClickTime).TotalMilliseconds < 1000)
                return true;
            _lastClickTime = now;
            return false;
        }

        /// <summary>
        /// 保存 llama-server 进程引用
        /// </summary>
        private void SetServerProcess(Process process)
        {
            _llamaServerProcess = process;
        }

        /// <summary>
        /// 关闭 llama-server 进程
        /// </summary>
        private void CloseServerProcess()
        {
            try
            {
                if (_llamaServerProcess != null && !_llamaServerProcess.HasExited)
                {
                    _llamaServerProcess.Kill();
                    _llamaServerProcess.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                AppendServerLog($"⚠️ 关闭进程异常: {ex.Message}");
            }
            finally
            {
                _llamaServerProcess?.Dispose();
                _llamaServerProcess = null;
            }
        }

        /// <summary>
        /// 处理 llama-server 输出（跨线程安全调用），输出到「服务器日志」面板
        /// 同时解析 token 统计信息到 tb_tokens
        /// </summary>
        private void OnLlamaOutput(string line)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(OnLlamaOutput), line);
                return;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                // 检测服务启动成功标志
                if (line.Contains("listening") || line.Contains("HTTP server listening"))
                {
                    _isWebServerRunning = true;
                    _isServerStarting = false;
                    btn_openclaw_web.Text = "关闭 llama.cpp 的web访问";
                    AppendServerLog("✅ llama-server Web 服务启动成功！");
                }

                // 解析GPU显卡信息（首次检测到时输出）
                if (TryParseGpuInfo(line))
                {
                    // 首次检测到GPU信息时输出到服务器日志
                    var gpuLines = GetGpuDisplayLines();
                    if (gpuLines.Count > 0 && gpuLines.Count == _gpuInfos.Count)
                    {
                        foreach (var g in gpuLines)
                            AppendServerLog(g);
                    }
                }

                // 解析 token 统计信息到 tb_tokens
                ParseTokenStats(line);

                AppendServerLog(line);
            }
        }

        /// <summary>
        /// 当前正在处理 token 统计的 slot/task key（从 print_timing 行获取，release 行清空）
        /// 格式: "slotId_taskId"，如 "0_0"
        /// </summary>
        private string? _currentSlotTaskKey;

        /// <summary>
        /// 临时存储当前 slot 的统计信息，用于跨行拼接
        /// Key: "slotId_taskId"，Value: (PromptTokens, PromptSpeed, GenTokens, GenSpeed)
        /// </summary>
        private readonly Dictionary<string, (int PromptTokens, double PromptSpeed, int GenTokens, double GenSpeed)> _slotStats = new();

        // ===== Token 汇总统计字段 =====
        private int    _statTaskCount;       // 已完成任务数
        private long   _statTotalInput;      // 累计输入 tokens
        private long   _statTotalOutput;     // 累计输出 tokens
        private double _statSumPromptSpeed;  // 提示词速度累加（用于求平均）
        private double _statSumGenSpeed;     // 生成速度累加（用于求平均）
        private double _statLastPromptSpeed; // 最近一次提示词速度
        private double _statLastGenSpeed;    // 最近一次生成速度

        /// <summary>
        /// 解析 llama-server 的 print_timing 输出，提取 token 统计信息并显示到 tb_tokens
        /// 
        /// 日志格式（跨多行，逐行到达）：
        ///   slot print_timing: id  0 | task 0 |                     ← 设置 _currentSlotTaskKey
        ///   prompt eval time =     604.35 ms /    16 tokens (...)    ← 记录 prompt 统计
        ///          eval time =    3867.89 ms /   271 tokens (...)    ← 记录生成统计
        ///         total time =    4472.24 ms /   287 tokens          ← 忽略
        ///   slot      release: id  0 | task 0 | stop processing...   ← 输出汇总并清空 key
        /// </summary>
        private void ParseTokenStats(string line)
        {
            try
            {
                var trimmed = line.Trim();

                // 1) 检测 "slot print_timing" 行 → 记录当前 slot/task key
                if (trimmed.Contains("print_timing"))
                {
                    var slotTaskId = ExtractSlotTaskId(trimmed);
                    if (slotTaskId != null)
                    {
                        _currentSlotTaskKey = slotTaskId;
                        _slotStats[slotTaskId] = (0, 0, 0, 0);
                    }
                    return;
                }

                // 2) 检测 "prompt eval time" 行
                if (trimmed.Contains("prompt eval time"))
                {
                    if (_currentSlotTaskKey != null)
                    {
                        var (tokens, speed) = ExtractTokensAndSpeed(trimmed);
                        if (tokens.HasValue)
                        {
                            var existing = _slotStats.GetValueOrDefault(_currentSlotTaskKey);
                            existing.PromptTokens = tokens.Value;
                            existing.PromptSpeed = speed ?? 0;
                            _slotStats[_currentSlotTaskKey] = existing;
                        }
                    }
                    return;
                }

                // 3) 检测 "eval time" 行（生成阶段，非 prompt eval）
                if ((trimmed.StartsWith("eval time") || trimmed.Contains("eval time ="))
                    && !trimmed.Contains("prompt eval time"))
                {
                    if (_currentSlotTaskKey != null)
                    {
                        var (tokens, speed) = ExtractTokensAndSpeed(trimmed);
                        if (tokens.HasValue)
                        {
                            var existing = _slotStats.GetValueOrDefault(_currentSlotTaskKey);
                            existing.GenTokens = tokens.Value;
                            existing.GenSpeed = speed ?? 0;
                            _slotStats[_currentSlotTaskKey] = existing;
                        }
                    }
                    return;
                }

                // 4) 检测 "slot release" 行 → 完成一次请求，输出汇总到 tb_tokens
                if (trimmed.Contains("release:") && trimmed.Contains("task"))
                {
                    var slotTaskId = ExtractSlotTaskId(trimmed);
                    if (slotTaskId != null && _slotStats.TryGetValue(slotTaskId, out var stats))
                    {
                        var taskNum = ExtractTaskNumber(trimmed);
                        var taskNumStr = taskNum.HasValue ? taskNum.Value.ToString() : "?";

                        var summary = $"Task {taskNumStr}: 提示词处理：速度约{stats.PromptSpeed:F2} tokens/秒，共{stats.PromptTokens} tokens; " +
                                      $"生成阶段：速度约{stats.GenSpeed:F2} tokens/秒，共{stats.GenTokens} tokens; " +
                                      $"总输入：{stats.PromptTokens} tokens，总输出：{stats.GenTokens} tokens";

                        if (tb_tokens.TextLength > 0)
                            tb_tokens.AppendText(Environment.NewLine);

                        tb_tokens.AppendText(summary);
                        tb_tokens.ScrollToCaret();

                        // 累加汇总统计
                        _statTaskCount++;
                        _statTotalInput      += stats.PromptTokens;
                        _statTotalOutput     += stats.GenTokens;
                        _statSumPromptSpeed  += stats.PromptSpeed;
                        _statSumGenSpeed     += stats.GenSpeed;
                        _statLastPromptSpeed  = stats.PromptSpeed;
                        _statLastGenSpeed     = stats.GenSpeed;

                        // 刷新 panel_status 汇总显示
                        UpdateTokenStatusPanel();

                        _slotStats.Remove(slotTaskId);
                    }

                    // 清空当前 key
                    _currentSlotTaskKey = null;
                }
            }
            catch
            {
                // 解析失败不影响主流程
            }
        }

        /// <summary>
        /// 重置 token 汇总统计（服务重启时调用）
        /// </summary>
        private void ResetTokenStats()
        {
            _statTaskCount       = 0;
            _statTotalInput      = 0;
            _statTotalOutput     = 0;
            _statSumPromptSpeed  = 0;
            _statSumGenSpeed     = 0;
            _statLastPromptSpeed = 0;
            _statLastGenSpeed    = 0;

            if (panel_status.InvokeRequired)
                panel_status.Invoke(ResetTokenStats);
            else
                panel_status.Controls.Clear();
        }

        /// <summary>
        /// 刷新 panel_status，显示 token 汇总统计（任务数、总tokens、平均速度、最近速度）
        /// 使用自绘 Label 实现高亮显示
        /// </summary>
        private void UpdateTokenStatusPanel()
        {
            if (panel_status.InvokeRequired)
            {
                panel_status.Invoke(UpdateTokenStatusPanel);
                return;
            }

            panel_status.Controls.Clear();
            panel_status.BackColor = Color.FromArgb(28, 32, 40);  // 深色背景

            double avgPrompt = _statTaskCount > 0 ? _statSumPromptSpeed / _statTaskCount : 0;
            double avgGen    = _statTaskCount > 0 ? _statSumGenSpeed    / _statTaskCount : 0;

            // 格式化大数字：小于 1000 原样显示，1000-999999 显示为 "Xk"，1000000+ 显示为 "XM"
            string FmtNum(long n)
            {
                if (n < 1000) return n.ToString();
                if (n < 1000000) return $"{n / 1000.0:F1}k";
                return $"{n / 1000000.0:F1}M";
            }
            string FmtSpd(double s) => $"{s:F1}";

            // 各行内容：(文字, 颜色, 字号, 粗体)
            var rows = new (string Text, Color Fore, float Size, bool Bold)[]
            {
                ($"📋任务数：{_statTaskCount}  In：{FmtNum(_statTotalInput)}  Out：{FmtNum(_statTotalOutput)}",
                    Color.FromArgb(200, 210, 230), 8.5f, false),

                ($"⚡提示词  Avg：{FmtSpd(avgPrompt)} t/s  最近：{FmtSpd(_statLastPromptSpeed)} t/s",
                    Color.FromArgb(90, 210, 140), 8.5f, true),

                ($"🔄生 成    Avg：{FmtSpd(avgGen)} t/s  最近：{FmtSpd(_statLastGenSpeed)} t/s",
                    Color.FromArgb(90, 180, 255), 8.5f, true),
            };

            int y = 6;
            foreach (var (text, fore, size, bold) in rows)
            {
                var lbl = new Label
                {
                    Text      = text,
                    ForeColor = fore,
                    BackColor = Color.Transparent,
                    Font      = new System.Drawing.Font("Microsoft YaHei UI", size,
                                    bold ? System.Drawing.FontStyle.Bold : System.Drawing.FontStyle.Regular),
                    AutoSize  = false,
                    Width     = panel_status.Width - 8,
                    Height    = 26,
                    Location  = new System.Drawing.Point(4, y),
                    TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                };
                panel_status.Controls.Add(lbl);
                y += 28;
            }
        }

        /// <summary>
        /// 从 "xxx time = 604.35 ms / 16 tokens (37.77 ms per token, 26.47 tokens per second)" 中
        /// 提取 tokens 数量和 tokens/second 速度
        /// </summary>
        private static (int? Tokens, double? Speed) ExtractTokensAndSpeed(string line)
        {
            int? tokens = null;
            double? speed = null;

            // 提取 tokens 数量: "/    16 tokens" 或 "/   271 tokens"
            var tokensMatch = System.Text.RegularExpressions.Regex.Match(
                line, @"/\s*(\d+)\s+tokens");
            if (tokensMatch.Success && int.TryParse(tokensMatch.Groups[1].Value, out var t))
            {
                tokens = t;
            }

            // 提取速度: "26.47 tokens per second"
            var speedMatch = System.Text.RegularExpressions.Regex.Match(
                line, @"([\d.]+)\s+tokens\s+per\s+second");
            if (speedMatch.Success && double.TryParse(speedMatch.Groups[1].Value, out var s))
            {
                speed = s;
            }

            return (tokens, speed);
        }

        /// <summary>
        /// 从 "slot release: id  0 | task 0 |" 或类似行中提取 "slotId_taskId" 格式的 key
        /// </summary>
        private static string? ExtractSlotTaskId(string line)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                line, @"id\s+(\d+)\s*\|\s*task\s+(\d+)");
            if (match.Success)
            {
                return $"{match.Groups[1].Value}_{match.Groups[2].Value}";
            }
            return null;
        }

        /// <summary>
        /// 从 "slot release: id  0 | task 0 |" 中提取 task 编号
        /// </summary>
        private static int? ExtractTaskNumber(string line)
        {
            var match = System.Text.RegularExpressions.Regex.Match(
                line, @"task\s+(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var taskNum))
            {
                return taskNum;
            }
            return null;
        }

        #endregion

        /// <summary>
        /// 获取当前选中模型的完整物理路径
        /// </summary>
        private string GetSelectedModelFullPath()
        {
            var displayName = cb_ggufModel.SelectedItem?.ToString() ?? "";
            if (string.IsNullOrEmpty(displayName)) return "";

            // 优先从字典中查找
            if (_modelPathDict.TryGetValue(displayName, out var fullPath))
            {
                return fullPath;
            }

            // 兼容旧逻辑：直接拼接
            return Path.Combine(_configManager.Config.GgufModelsPath, displayName);
        }

        #endregion

        #region 日志输出

        /// <summary>
        /// 输出日志到运行日志面板
        /// </summary>
        private void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogMessage), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            tb_TotalInfoLogs.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            tb_TotalInfoLogs.ScrollToCaret();
        }

        /// <summary>
        /// 输出日志到服务器日志面板（tb_serverLogs），线程安全
        /// </summary>
        private void AppendServerLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendServerLog), message);
                return;
            }

            var timestamp = DateTime.Now.ToString("HH:mm:ss");
            tb_serverLogs.AppendText($"[{timestamp}] {message}{Environment.NewLine}");
            tb_serverLogs.ScrollToCaret();
        }

        /// <summary>
        /// 从 tb_box_webConfig 中提取纯净的命令行参数（去掉 # 注释行和空行）
        /// </summary>
        private string CleanWebConfigArgs()
        {
            var lines = tb_box_webConfig.Lines;
            var sb = new StringBuilder();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                // 跳过空行和注释行
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#"))
                    continue;

                // 去掉行内注释（# 后面的内容）
                var commentIdx = trimmed.IndexOf(" #", StringComparison.Ordinal);
                if (commentIdx > 0)
                {
                    trimmed = trimmed.Substring(0, commentIdx).TrimEnd();
                }

                // 也处理行末 # 注释（无空格情况，如 --param value#注释）
                var commentIdx2 = trimmed.IndexOf('#');
                if (commentIdx2 > 0 && trimmed[commentIdx2 - 1] != '=')
                {
                    // 确认 # 前面不是 = 号（排除 --cache-type-k=q#3 这种情况）
                    trimmed = trimmed.Substring(0, commentIdx2).TrimEnd();
                }

                sb.Append(trimmed).Append(' ');
            }

            return sb.ToString().Trim();
        }

        #endregion

        #region 事件处理

        /// <summary>
        /// 设置 llama.cpp 路径
        /// </summary>
        private void btn_llamaPath_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择 llama.cpp 程序目录（包含 llama-server.exe 和 llama-cli.exe）"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tb_llamaPath.Text = dialog.SelectedPath;
                ValidateLlamaPath(dialog.SelectedPath);
                SaveConfigDebounced();
            }
        }

        /// <summary>
        /// 设置 GGUF 模型目录
        /// </summary>
        private void btn_ggufModelsPath_Click(object sender, EventArgs e)
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "选择 GGUF 模型文件所在目录"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tb_ggufModelsPath.Text = dialog.SelectedPath;
                RefreshModelList();
                SaveConfigDebounced();
            }
        }

        /// <summary>
        /// 刷新模型列表
        /// </summary>
        private void btn_ggufModel_Refresh_Click(object sender, EventArgs e)
        {
            RefreshModelList();
        }

        /// <summary>
        /// 设置多模态投影模型文件
        /// </summary>
        private void btn_mmprojFile_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "GGUF文件|*.gguf|所有文件|*.*",
                Title = "选择多模态投影模型文件"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tb_mmprojFile.Text = dialog.FileName;
                SaveConfigDebounced();
            }
        }

        /// <summary>
        /// 选择 MTP 模型文件
        /// </summary>
        private void btn_mtpFile_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Filter = "GGUF文件|*.gguf|所有文件|*.*",
                Title = "选择 MTP 模型文件（assistant-MTP 或 mtp- 前缀的 GGUF）"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                tb_mtpFile.Text = dialog.FileName;
                SaveConfigDebounced();
            }
        }

        /// <summary>
        /// 获取最优启动参数（调用 llama-fit-params.exe）
        /// 全程异步，避免 UI 卡死；日志输出到「测试结果」面板并自动激活该标签页
        /// </summary>
        private async void btn_getOptions_Click(object sender, EventArgs e)
        {
            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            if (string.IsNullOrEmpty(llamaPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                MessageBox.Show("请先选择有效的模型文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var fitParamsExe = Path.Combine(llamaPath, "llama-fit-params.exe");
            if (!File.Exists(fitParamsExe))
            {
                MessageBox.Show($"未找到 llama-fit-params.exe\n路径: {fitParamsExe}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 禁用按钮防止重复点击
            btn_getOptions.Enabled = false;

            // 切换到「测试结果」标签页，方便用户实时查看日志
            tabControl_logs.SelectedTab = tabPage_log_runLogs;

            LogMessage("开始获取最优启动参数...");
            LogMessage($"执行: \"{fitParamsExe}\" -m \"{modelPath}\"");

            AppendRunningLog("========================================================");
            AppendRunningLog("🔍 开始获取最优启动参数");
            AppendRunningLog($"📁 模型: {Path.GetFileName(modelPath)}");
            AppendRunningLog($"▶️  执行: \"{fitParamsExe}\" -m \"{modelPath}\"");
            AppendRunningLog("========================================================");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fitParamsExe,
                    Arguments = $"-m \"{modelPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = llamaPath
                };

                var process = new Process { StartInfo = psi, EnableRaisingEvents = true };

                // 异步读取输出，同时输出到运行日志和测试结果日志
                process.OutputDataReceived += (s, ea) =>
                {
                    if (!string.IsNullOrEmpty(ea.Data))
                    {
                        LogMessage(ea.Data);
                        AppendRunningLog(ea.Data);
                    }
                };
                process.ErrorDataReceived += (s, ea) =>
                {
                    if (!string.IsNullOrEmpty(ea.Data))
                    {
                        LogMessage($"[STDERR] {ea.Data}");
                        AppendRunningLog($"[STDERR] {ea.Data}");
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 真异步等待，不阻塞 UI 线程
                await process.WaitForExitAsync();

                int exitCode = process.ExitCode;
                var exitMsg = $"✅ llama-fit-params.exe 执行完成，退出码: {exitCode}";
                LogMessage(exitMsg);
                AppendRunningLog(exitMsg);
                AppendRunningLog("========================================================");
                AppendRunningLog("💡 提示：如需将上方参数应用到配置面板，请复制后粘贴到「启动参数配置」文本框，再点击「动态解析」");

                process.Dispose();
            }
            catch (Exception ex)
            {
                var errMsg = $"❌ 获取最优参数失败: {ex.Message}";
                LogMessage(errMsg);
                AppendRunningLog(errMsg);
            }
            finally
            {
                btn_getOptions.Enabled = true;
            }
        }

        /// <summary>
        /// CMD 窗口对话测试（启动 llama-cli.exe 交互对话）
        /// 启动前完整记录命令到「测试结果」日志，失败时记录详细错误
        /// </summary>
        private void btn_chatTest_Click(object sender, EventArgs e)
        {
            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            // 切换到「测试结果」标签页
            tabControl_logs.SelectedTab = tabPage_log_testLogs;

            if (string.IsNullOrEmpty(llamaPath))
            {
                var errMsg = "❌ 请先设置 llama.cpp 路径";
                LogMessage(errMsg);
                AppendRunningLog(errMsg);
                MessageBox.Show("请先设置 llama.cpp 路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrEmpty(modelPath))
            {
                var errMsg = "❌ 请先选择模型文件";
                LogMessage(errMsg);
                AppendRunningLog(errMsg);
                MessageBox.Show("请先设置 llama.cpp 路径和模型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(modelPath))
            {
                var errMsg = $"❌ 模型文件不存在: {modelPath}";
                LogMessage(errMsg);
                AppendRunningLog(errMsg);
                MessageBox.Show($"模型文件不存在: {modelPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var cliExe = Path.Combine(llamaPath, "llama-cli.exe");
            if (!File.Exists(cliExe))
            {
                var errMsg = $"❌ 未找到 llama-cli.exe，路径: {cliExe}";
                LogMessage(errMsg);
                AppendRunningLog(errMsg);
                MessageBox.Show("未找到 llama-cli.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 组装启动参数：过滤掉 # 注释行
            var cleanArgs = CleanWebConfigArgs();

            // MTP 模型文件
            var mtpFile = tb_mtpFile.Text.Trim();
            if (!string.IsNullOrEmpty(mtpFile) && File.Exists(mtpFile))
            {
                cleanArgs += $" --model-draft \"{mtpFile}\"";
            }

            var args = $"-m \"{modelPath}\" {cleanArgs}";

            AppendRunningLog("========================================================");
            AppendRunningLog("💬 启动 CMD 对话测试（llama-cli.exe）");
            AppendRunningLog($"📁 模型: {Path.GetFileName(modelPath)}");
            AppendRunningLog($"▶️  命令: \"{cliExe}\" {args}");
            AppendRunningLog("========================================================");
            AppendRunningLog("⏳ 正在启动 CMD 窗口，请在弹出的窗口中进行对话测试...");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = cliExe,
                    Arguments = args,
                    UseShellExecute = true   // 弹出可见的 CMD 窗口
                };

                var process = Process.Start(psi);

                if (process == null)
                {
                    var errMsg = "❌ 启动 llama-cli.exe 失败（Process.Start 返回 null）";
                    LogMessage(errMsg);
                    AppendRunningLog(errMsg);
                    return;
                }

                // 标记已执行过 chatTest（用于保存启动脚本前的前置校验）
                _chatTestExecuted = true;

                LogMessage($"✅ 对话测试已启动 (PID: {process.Id})");
                AppendRunningLog($"✅ CMD 对话测试窗口已打开 (PID: {process.Id})");
                AppendRunningLog("💡 请在 CMD 窗口中完成对话测试后，再点击「生成 server 启动脚本」保存脚本");
            }
            catch (Exception ex)
            {
                var errMsg = $"❌ 启动失败: {ex.GetType().Name}: {ex.Message}";
                LogMessage(errMsg);
                AppendRunningLog(errMsg);
                if (ex.InnerException != null)
                    AppendRunningLog($"   InnerException: {ex.InnerException.Message}");
                AppendRunningLog($"   命令: \"{cliExe}\" {args}");
                AppendRunningLog("💡 请检查：1) llama.cpp 路径是否正确  2) 模型文件是否有效  3) 参数格式是否正确");
            }
        }

        /// <summary>
        /// 生成 server 启动脚本（.bat 保存到桌面）
        /// 前置条件：必须先执行过「CMD 窗口对话测试」并确认成功，才允许保存脚本
        /// 组合：llama-server路径 + 模型路径 + mmproj + 启动参数(去注释) + 端口 + 跨域 + apikey
        /// </summary>
        private void btn_SaveBatchCmd_Click(object sender, EventArgs e)
        {
            // 前置校验：必须先运行过 CMD 对话测试
            if (!_chatTestExecuted)
            {
                tabControl_logs.SelectedTab = tabPage_log_testLogs;
                AppendRunningLog("⚠️ 保存启动脚本失败：请先点击「CMD 窗口对话测试」，确认参数正确后再保存脚本");
                MessageBox.Show(
                    "请先运行「CMD 窗口对话测试」，确认参数正确后再保存启动脚本！\n\n" +
                    "操作步骤：\n1. 点击「CMD 窗口对话测试」按钮\n2. 在弹出的窗口中测试对话正常\n3. 关闭测试窗口后再点击「生成 server 启动脚本」",
                    "请先完成对话测试",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            if (string.IsNullOrEmpty(llamaPath) || string.IsNullOrEmpty(modelPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径和模型", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!File.Exists(modelPath))
            {
                MessageBox.Show($"模型文件不存在: {modelPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var serverExe = Path.Combine(llamaPath, "llama-server.exe");
            if (!File.Exists(serverExe))
            {
                MessageBox.Show("未找到 llama-server.exe", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 组装参数
            var args = new StringBuilder();
            args.Append($"-m \"{modelPath}\"");

            // mmproj 投影模型
            var mmprojFile = tb_mmprojFile.Text.Trim();
            if (!string.IsNullOrEmpty(mmprojFile) && File.Exists(mmprojFile))
            {
                args.Append($" --mmproj \"{mmprojFile}\"");
            }

            // MTP 模型文件
            var mtpFile = tb_mtpFile.Text.Trim();
            if (!string.IsNullOrEmpty(mtpFile) && File.Exists(mtpFile))
            {
                args.Append($" --model-draft \"{mtpFile}\"");
            }

            // 启动参数（过滤 # 注释行）
            var cleanArgs = CleanWebConfigArgs();
            if (!string.IsNullOrEmpty(cleanArgs))
            {
                args.Append($" {cleanArgs}");
            }

            // 端口参数
            var port = tb_apiUrl_port.Text.Trim();
            if (!string.IsNullOrEmpty(port) && int.TryParse(port, out _))
            {
                args.Append($" --port {port}");
            }

            // 跨域支持
            if (cb_startConfig_cors.Checked)
            {
                args.Append(" --cors");
            }

            // API Key
            var apiKey = tb_apiKey.Text.Trim();
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "sk-1234567809")
            {
                args.Append($" --api-key {apiKey}");
            }

            // 生成 bat 脚本
            var modelName = Path.GetFileNameWithoutExtension(modelPath);
            var script = new StringBuilder();
            script.AppendLine("@echo off");
            script.AppendLine("chcp 65001 >nul");
            script.AppendLine($"title Llama Server - {modelName}");
            script.AppendLine("echo ============================================");
            script.AppendLine($"echo   Llama.cpp Server - {modelName}");
            script.AppendLine("echo ============================================");
            script.AppendLine($"echo   模型: {modelPath}");
            script.AppendLine($"echo   端口: {port}");
            script.AppendLine($"echo   API : http://localhost:{port}");
            script.AppendLine("echo ============================================");
            script.AppendLine("echo.");
            script.AppendLine($"\"{serverExe}\" {args}");
            script.AppendLine("echo.");
            script.AppendLine("echo 服务已停止运行");
            script.AppendLine("pause");

            var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            var scriptPath = Path.Combine(desktopPath, $"LlamaServer_{modelName}.bat");

            try
            {
                // 使用 GBK 编码保存 bat 文件，避免中文乱码
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var gbk = Encoding.GetEncoding("GBK");
                File.WriteAllText(scriptPath, script.ToString(), gbk);
                LogMessage($"启动脚本已保存到: {scriptPath}");
                MessageBox.Show($"启动脚本已保存到桌面！\n{scriptPath}", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                LogMessage($"保存脚本失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 清空所有日志
        /// </summary>
        private void btn_CleanLogs_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("确认清空所有日志？\n\n• 运行日志\n• 测速结果\n• 服务日志",
                "清空确认", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result != DialogResult.Yes) return;

            tb_TotalInfoLogs.Clear();
            tb_runningLogs.Clear();
            tb_serverLogs.Clear();
            LogMessage("所有日志已清空");
        }

        /// <summary>
        /// 引入启动参数：默认打开程序根目录，读取 txt 文件中的单行参数（空格分隔），
        /// 填入 tb_box_webConfig 后自动执行动态解析（SyncParamsFromWebConfig）
        /// </summary>
        private void button_import_options_Click(object sender, EventArgs e)
        {
            // 默认初始目录：程序根目录（exe 所在目录）
            var startupDir = AppDomain.CurrentDomain.BaseDirectory;

            using var dialog = new OpenFileDialog
            {
                Filter = "启动参数文件|*.txt|所有文件|*.*",
                Title = "引入启动参数配置文件",
                InitialDirectory = startupDir
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 读取文件所有行，过滤空行和 # 注释行，拼接为单行（空格分隔）
                    var lines = File.ReadAllLines(dialog.FileName, Encoding.UTF8);
                    var singleLine = string.Join(" ",
                        lines
                            .Select(l => l.Trim())
                            .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#")));

                    if (string.IsNullOrWhiteSpace(singleLine))
                    {
                        LogMessage($"⚠️ 文件内容为空或全为注释: {Path.GetFileName(dialog.FileName)}");
                        MessageBox.Show("文件内容为空或全部是注释行，请检查文件！", "提示",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // 写入 tb_box_webConfig（单行原始参数）
                    tb_box_webConfig.Text = singleLine;

                    // 自动触发动态解析（等同于点击「解析启动参数」按钮）
                    SyncParamsFromWebConfig();

                    LogMessage($"✅ 已引入启动参数: {Path.GetFileName(dialog.FileName)}");
                    LogMessage($"   参数内容: {(singleLine.Length > 120 ? singleLine.Substring(0, 120) + "..." : singleLine)}");
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ 引入失败: {ex.Message}");
                    MessageBox.Show($"引入启动参数失败！\n{ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 保存启动参数：将 tb_box_webConfig 中的配置转换为单行（空格分隔），
        /// 默认保存到程序根目录，文件名以当前选中模型命名
        /// </summary>
        private void button_save_options_Click(object sender, EventArgs e)
        {
            // 将 tb_box_webConfig 的多行注释格式转为单行（过滤 # 注释行和空行）
            var configLines = tb_box_webConfig.Lines;
            var singleLine = string.Join(" ",
                configLines
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l) && !l.StartsWith("#")));

            if (string.IsNullOrWhiteSpace(singleLine))
            {
                MessageBox.Show("当前启动参数为空，请先配置参数！", "提示",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 默认文件名：使用当前选中模型名（去掉 .gguf 后缀），若无则用 llama_server_config
            var modelDisplay = cb_ggufModel.SelectedItem?.ToString() ?? "";
            var defaultFileName = string.IsNullOrEmpty(modelDisplay)
                ? "llama_server_config"
                : Path.GetFileNameWithoutExtension(modelDisplay).Replace(" ", "_");
            defaultFileName += ".txt";

            // 默认保存路径：程序根目录
            var startupDir = AppDomain.CurrentDomain.BaseDirectory;

            using var dialog = new SaveFileDialog
            {
                Filter = "启动参数文件|*.txt|所有文件|*.*",
                Title = "保存启动参数配置文件",
                InitialDirectory = startupDir,
                FileName = defaultFileName
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 写入单行（末尾无换行）
                    File.WriteAllText(dialog.FileName, singleLine, Encoding.UTF8);
                    LogMessage($"✅ 启动参数已保存: {dialog.FileName}");
                    LogMessage($"   内容: {(singleLine.Length > 120 ? singleLine.Substring(0, 120) + "..." : singleLine)}");
                    MessageBox.Show($"启动参数已保存！\n\n文件: {Path.GetFileName(dialog.FileName)}\n路径: {Path.GetDirectoryName(dialog.FileName)}",
                        "保存成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    LogMessage($"❌ 保存失败: {ex.Message}");
                    MessageBox.Show($"保存失败！\n{ex.Message}", "错误",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        /// <summary>
        /// 解析启动参数
        /// </summary>
        private void btn_analyze_options_Click(object sender, EventArgs e)
        {
            SyncParamsFromWebConfig();
            LogMessage("已解析启动参数并同步到参数面板");
        }

        /// <summary>
        /// 复制 API URL
        /// </summary>
        private void btn_apiUrl_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tb_apiUrl.Text);
            LogMessage("API URL 已复制到剪贴板");
        }

        /// <summary>
        /// 复制 API Key
        /// </summary>
        private void btn_apiKey_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tb_apiKey.Text);
            LogMessage("API Key 已复制到剪贴板");
        }

        /// <summary>
        /// 复制 Model ID
        /// </summary>
        private void btn_modelID_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(tb_modelID.Text);
            LogMessage("Model ID 已复制到剪贴板");
        }

        /// <summary>
        /// 开启/关闭 llama.cpp Web 访问
        /// 所有日志输出到「服务器日志」面板（tb_serverLogs）
        /// </summary>
        private async void btn_openclaw_web_Click(object sender, EventArgs e)
        {
            // 检查误点击（1秒内多次点击只执行第一次）
            if (IsRapidClick()) return;

            // 切换到「服务器日志」标签页
            tabControl_logs.SelectedTab = tabPage_log_serverLogs;

            // 直接检测 llama-server 进程的实际运行状态
            var existingServers = Process.GetProcessesByName("llama-server");
            var isServerRunning = existingServers.Length > 0;

            // 根据实际状态决定是关闭还是启动
            if (isServerRunning)
            {
                // 服务正在运行，点击则关闭
                AppendServerLog("⚠️ 正在关闭 Web 服务...");
                CloseServerProcess();
                foreach (var p in existingServers)
                {
                    try { p.Kill(); } catch { }
                }
                _isWebServerRunning = false;
                _isServerStarting = false;
                btn_openclaw_web.Text = "开启 llama.cpp 的web访问";
                AppendServerLog("✅ Web 服务已关闭");
                return;
            }

            // 服务未运行，点击则启动
            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();
            var mmprojPath = tb_mmprojFile.Text.Trim();

            if (string.IsNullOrEmpty(llamaPath) || string.IsNullOrEmpty(modelPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径和选择模型！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isServerStarting = false;
                _isWebServerRunning = false;
                btn_openclaw_web.Text = "开启 llama.cpp 的web访问";
                return;
            }

            // 检查模型文件是否存在
            if (!File.Exists(modelPath))
            {
                MessageBox.Show($"模型文件不存在！\n{modelPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isServerStarting = false;
                _isWebServerRunning = false;
                btn_openclaw_web.Text = "开启 llama.cpp 的web访问";
                return;
            }

            var serverPath = Path.Combine(llamaPath, "llama-server.exe").Replace("/", "\\");
            if (!File.Exists(serverPath))
            {
                MessageBox.Show($"llama-server.exe 不存在！\n{serverPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _isServerStarting = false;
                _isWebServerRunning = false;
                btn_openclaw_web.Text = "开启 llama.cpp 的web访问";
                return;
            }

            // 从 tb_box_webConfig 提取启动参数（去注释、去空行）
            var cleanArgs = CleanWebConfigArgs();

            // 构建完整命令行参数
            var args = new StringBuilder();
            args.Append($"-m \"{modelPath}\"");

            // mmproj 投影模型
            if (!string.IsNullOrEmpty(mmprojPath) && File.Exists(mmprojPath))
            {
                args.Append($" --mmproj \"{mmprojPath}\"");
            }

            // MTP 模型文件
            var mtpFile = tb_mtpFile.Text.Trim();
            if (!string.IsNullOrEmpty(mtpFile) && File.Exists(mtpFile))
            {
                args.Append($" --model-draft \"{mtpFile}\"");
            }

            // 用户配置的启动参数（来自 _paramManager / tb_box_webConfig）
            if (!string.IsNullOrEmpty(cleanArgs))
            {
                args.Append($" {cleanArgs}");
            }

            // 端口参数
            var port = tb_apiUrl_port.Text.Trim();
            if (!string.IsNullOrEmpty(port) && int.TryParse(port, out _))
            {
                args.Append($" --port {port}");
            }

            // 跨域支持
            if (cb_startConfig_cors.Checked)
            {
                args.Append(" --cors");
            }

            // API Key
            var apiKey = tb_apiKey.Text.Trim();
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "sk-1234567809")
            {
                args.Append($" --api-key {apiKey}");
            }

            // host 参数（根据本地/远程切换）
            var address = cb_startConfig_local.Checked ? "127.0.0.1" : "0.0.0.0";
            args.Append($" --host {address}");

            var serverArgs = args.ToString();

            AppendServerLog("========================================================");
            AppendServerLog("🚀 启动 llama-server Web 服务（后台启动中...）");
            AppendServerLog("========================================================");
            AppendServerLog($"📁 模型: {Path.GetFileName(modelPath)}");
            if (!string.IsNullOrEmpty(mmprojPath))
                AppendServerLog($"📁 多模态投影: {Path.GetFileName(mmprojPath)}");
            AppendServerLog($"🌐 访问地址: http://{address}:{port}");
            AppendServerLog($"📋 基础配置: {cleanArgs}");
            AppendServerLog("========================================================");
            AppendServerLog($"▶️  执行命令:\n{serverPath} {serverArgs}");
            AppendServerLog("========================================================");

            var startInfo = new ProcessStartInfo
            {
                FileName = serverPath,
                Arguments = serverArgs,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = llamaPath
            };

            var serverProcess = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            serverProcess.OutputDataReceived += (s, ea) =>
            {
                if (!string.IsNullOrEmpty(ea.Data))
                    OnLlamaOutput(ea.Data);
            };
            serverProcess.ErrorDataReceived += (s, ea) =>
            {
                if (!string.IsNullOrEmpty(ea.Data))
                    OnLlamaOutput(ea.Data);
            };

            // 监听进程退出事件（模型加载失败时会触发）
            serverProcess.Exited += (s, ea) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (_isServerStarting && !_isWebServerRunning)
                    {
                        // 服务器启动后立即退出（可能是模型加载错误或进程崩溃）
                        _isServerStarting = false;
                        _isWebServerRunning = false;
                        btn_openclaw_web.Text = "开启 llama.cpp 的web访问";
                        AppendServerLog("⚠️ llama-server 启动失败（模型加载错误或进程退出）");
                    }
                }));
            };

            try
            {
                serverProcess.Start();
                serverProcess.BeginOutputReadLine();
                serverProcess.BeginErrorReadLine();

                // 保存进程引用
                SetServerProcess(serverProcess);

                // 标记为正在启动
                _isServerStarting = true;
                _isWebServerRunning = false;

                // 更新按钮文本
                btn_openclaw_web.Text = "关闭 llama.cpp 的web访问";

                // 重置 token 汇总统计（新服务启动时清空历史）
                ResetTokenStats();

                AppendServerLog("⏳ 服务正在启动中...");
                AppendServerLog("💡 请耐心等待，服务启动后会显示成功提示");
                AppendServerLog("========================================================");

                // 立即返回，不等待服务启动
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                AppendServerLog($"❌ 启动失败: {ex.Message}");
                _isServerStarting = false;
                _isWebServerRunning = false;
                btn_openclaw_web.Text = "开启 llama.cpp 的web访问";

                MessageBox.Show($"启动 llama-server 失败！\n\n{ex.Message}", "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 本地访问切换
        /// </summary>
        private void cb_startConfig_local_CheckedChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            if (cb_startConfig_local.Checked)
            {
                cb_startConfig_local.Text = "本地访问";
                tb_apiUrl_address.Text = "127.0.0.1";
            }
            else
            {
                cb_startConfig_local.Text = "远程访问";
                tb_apiUrl_address.Text = "0.0.0.0";
            }
            SaveConfigDebounced();
        }

        /// <summary>
        /// 根据地址和端口更新 API URL
        /// </summary>
        private void UpdateApiUrl()
        {
            if (_isInitializing) return;

            var address = tb_apiUrl_address.Text.Trim();
            var port = tb_apiUrl_port.Text.Trim();

            // 验证地址是否有效
            if (!string.IsNullOrEmpty(address) && !IsValidIpAddress(address))
            {
                // 地址无效，不更新 URL，可以显示提示
                tb_apiUrl_address.BackColor = Color.LightCoral;
                return;
            }
            else
            {
                tb_apiUrl_address.BackColor = Color.White;
            }

            // 验证端口是否有效
            if (!string.IsNullOrEmpty(port) && (!int.TryParse(port, out int portNum) || portNum < 1 || portNum > 65535))
            {
                tb_apiUrl_port.BackColor = Color.LightCoral;
                return;
            }
            else
            {
                tb_apiUrl_port.BackColor = Color.White;
            }

            // 生成 API URL
            if (!string.IsNullOrEmpty(address) && !string.IsNullOrEmpty(port))
            {
                tb_apiUrl.Text = $"http://{address}:{port}/v1";
            }
        }

        /// <summary>
        /// 验证 IP 地址是否有效
        /// </summary>
        private bool IsValidIpAddress(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
                return false;

            // 支持 localhost
            if (address.Equals("localhost", StringComparison.OrdinalIgnoreCase))
                return true;

            // 验证 IPv4 地址
            var parts = address.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int num))
                    return false;
                if (num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 全选上下文大小
        /// </summary>
        private void btn_contentTest_selectAll_Click(object sender, EventArgs e)
        {
            SetAllContentCheckBoxes(true);
        }

        /// <summary>
        /// 反选上下文大小
        /// </summary>
        private void btn_contentTest_selectOther_Click(object sender, EventArgs e)
        {
            SetAllContentCheckBoxes(null); // null 表示反选
        }

        /// <summary>
        /// 设置所有上下文复选框状态
        /// </summary>
        private void SetAllContentCheckBoxes(bool? state)
        {
            var checkBoxes = gp_content.Controls.OfType<CheckBox>();
            foreach (var cb in checkBoxes)
            {
                if (state.HasValue)
                {
                    cb.Checked = state.Value;
                }
                else
                {
                    cb.Checked = !cb.Checked;
                }
            }
        }

        /// <summary>
        /// 开始上下文批量测速
        /// 逐个用选中的上下文大小替换 -c 参数，运行 llama-cli 测速
        /// </summary>
        private async void btn_contentTest_Run_Click(object sender, EventArgs e)
        {
            // 检查误点击
            if (IsRapidClick()) return;

            // 如果正在运行，点击按钮则停止
            if (_isRunning)
            {
                LogMessage("⚠️ 正在停止批量测速...");
                _stopTest = true;
                _isRunning = false;
                btn_contentTest_Run.Text = "开始批量测速";
                return;
            }

            // 检测并关闭已运行的 llama-cli.exe 进程
            var existingCli = Process.GetProcessesByName("llama-cli");
            if (existingCli.Length > 0)
            {
                LogMessage("⚠️ 检测到 llama-cli 已在运行，正在关闭...");
                foreach (var p in existingCli)
                {
                    try { p.Kill(); } catch { }
                }
                await Task.Delay(1000);
            }

            // 获取选中的上下文大小
            var selectedContexts = GetSelectedContextSizes();
            if (selectedContexts.Count == 0)
            {
                MessageBox.Show("请至少选择一个上下文大小！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            if (string.IsNullOrEmpty(llamaPath) || string.IsNullOrEmpty(modelPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径和选择模型！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var cliExe = Path.Combine(llamaPath, "llama-cli.exe");
            if (!File.Exists(cliExe))
            {
                MessageBox.Show($"llama-cli.exe 不存在！\n{cliExe}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(modelPath))
            {
                MessageBox.Show($"模型文件不存在！\n{modelPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            _isRunning = true;
            _stopTest = false;
            btn_contentTest_Run.Text = "停止批量测速";

            var results = new List<SpeedTestResult>();

            try
            {
                // 清空测速结果日志和GPU缓存
                tb_runningLogs.Clear();
                ClearGpuInfos();

                // 切换到运行日志标签页（在开始测试前）
                tabControl_logs.SelectedTab = tabPage_log_runLogs;

                // 从 _paramManager 获取所有已启用参数（不含 -c 和 -m，这两个由测速流程自行指定）
                var baseArgs = BuildSpeedTestBaseArgs();

                // 显示测试配置信息
                LogMessage("========================================================");
                LogMessage("🚀 开始批量上下文测速");
                LogMessage("========================================================");
                LogMessage($"📁 模型：{Path.GetFileName(modelPath)}");
                LogMessage($"📋 基础参数：{baseArgs}");
                LogMessage($"📊 测试列表：{string.Join(", ", selectedContexts.OrderBy(x => x).Select(x => GetContextName(x)))}");
                LogMessage("========================================================");

                // 测速结果面板也输出配置信息
                AppendRunningLog("========================================================");
                AppendRunningLog("🚀 开始批量上下文测速");
                AppendRunningLog($"📁 模型：{Path.GetFileName(modelPath)}");
                AppendRunningLog($"📊 测试列表：{string.Join(", ", selectedContexts.OrderBy(x => x).Select(x => GetContextName(x)))}");
                AppendRunningLog("========================================================");

                var totalTests = selectedContexts.Count;
                var currentTest = 0;

                foreach (var contextSize in selectedContexts.OrderBy(x => x))
                {
                    // 检查停止标志
                    if (_stopTest)
                    {
                        LogMessage("⚠️ 批量测速已停止");
                        AppendRunningLog("⚠️ 批量测速已停止");
                        break;
                    }

                    currentTest++;
                    var ctxName = GetContextName(contextSize);
                    LogMessage($"\n进度：[{currentTest}/{totalTests}] CTX={ctxName}");

                    // 组装完整命令：基础参数 + -c 上下文 + -m 模型 + -p 提示词
                    var fullArgs = $"-m \"{modelPath}\" -c {contextSize} {baseArgs} -p \"hello\" -n 128";

                    var result = await RunSingleSpeedTestAsync(cliExe, fullArgs, contextSize, llamaPath);
                    results.Add(result);

                    // 内部日志记录单条结果
                    LogMessage($"✅ CTX={ctxName}: Prompt={result.PromptSpeed:F1} t/s | Generation={result.GenerationSpeed:F1} t/s");

                    // 等待显存释放
                    if (currentTest < totalTests && !_stopTest)
                    {
                        LogMessage("⏳ 等待显存释放...");
                        await Task.Delay(3000);
                    }
                }

                // 生成汇总报告（先清空面板，只输出最终汇总）
                if (!_stopTest && results.Count > 0)
                {
                    tb_runningLogs.Clear();
                    GenerateBatchReport(results);
                    LogMessage("========================================================");
                    LogMessage("✅ 批量测速完成");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 批量测速错误：{ex.Message}");
                AppendRunningLog($"❌ 批量测速错误：{ex.Message}");
            }
            finally
            {
                _isRunning = false;
                _stopTest = false;
                btn_contentTest_Run.Text = "开始批量测速";
            }
        }

        /// <summary>
        /// 从 _paramManager 构建测速基础参数（排除 -c/-m/-p/-n，这些由测速流程自行指定）
        /// </summary>
        /// <param name="excludeThread">是否排除 -t/--threads 参数（CPU多线程测速时需排除，由测速流程自行指定）</param>
        private string BuildSpeedTestBaseArgs(bool excludeThread = false)
        {
            var sb = new StringBuilder();

            foreach (var category in _paramManager.Categories)
            {
                foreach (var param in category.Params)
                {
                    if (!param.Enabled) continue;

                    // 排除由测速流程自行指定的参数，以及会导致测速结果为0的参数
                    var name = param.Name.ToLower();
                    if (name == "--ctx-size" || name == "-c" ||
                        name == "-m" || name == "--model" ||
                        name == "-p" || name == "--prompt" ||
                        name == "-n" || name == "--n-predict" ||
                        name == "--cont-batching")
                        continue;

                    // CPU多线程测速时排除 -t/--threads，由测速流程遍历指定
                    if (excludeThread && (name == "--threads" || name == "-t"))
                        continue;

                    var arg = param.GetCommandLineArg();
                    if (!string.IsNullOrEmpty(arg))
                    {
                        sb.Append($" {arg}");
                    }
                }
            }

            return sb.ToString().Trim();
        }

        /// <summary>
        /// 运行单个上下文的速度测试
        /// </summary>
        private async Task<SpeedTestResult> RunSingleSpeedTestAsync(string cliExe, string args, int contextSize, string workingDir)
        {
            var result = new SpeedTestResult { ContextSize = contextSize };
            var outputBuilder = new StringBuilder();
            var testCompleted = false;
            var lastPromptSpeed = 0.0;
            var lastGenSpeed = 0.0;

            var startInfo = new ProcessStartInfo
            {
                FileName = cliExe,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                WorkingDirectory = workingDir
            };

            Process? process = null;

            try
            {
                process = new Process { StartInfo = startInfo };

                process.OutputDataReceived += (s, ea) =>
                {
                    if (string.IsNullOrEmpty(ea.Data)) return;
                    outputBuilder.AppendLine(ea.Data);
                    ParseSpeedLine(ea.Data, ref lastPromptSpeed, ref lastGenSpeed, ref testCompleted, process);
                    TryParseGpuInfo(ea.Data);  // 解析GPU显卡信息
                };

                process.ErrorDataReceived += (s, ea) =>
                {
                    if (string.IsNullOrEmpty(ea.Data)) return;
                    outputBuilder.AppendLine(ea.Data);
                    ParseSpeedLine(ea.Data, ref lastPromptSpeed, ref lastGenSpeed, ref testCompleted, process);
                    TryParseGpuInfo(ea.Data);  // 解析GPU显卡信息
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                // 等待进程完成，最多10分钟
                using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromMinutes(10));
                try
                {
                    await process.WaitForExitAsync(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // 超时
                }

                result.RawOutput = outputBuilder.ToString();

                // 解析速度结果
                if (lastPromptSpeed > 0) result.PromptSpeed = lastPromptSpeed;
                if (lastGenSpeed > 0) result.GenerationSpeed = lastGenSpeed;

                // 如果没解析到，再尝试从完整输出解析
                if (result.PromptSpeed == 0 || result.GenerationSpeed == 0)
                {
                    ParseSpeedFromOutput(result);
                }

                // 兜底：从完整输出中扫描 GPU 信息（防止实时回调时序遗漏）
                if (_gpuInfos.Count == 0)
                {
                    foreach (var outputLine in outputBuilder.ToString().Split('\n', StringSplitOptions.RemoveEmptyEntries))
                    {
                        if (TryParseGpuInfo(outputLine))
                            break; // 找到一条就说明 llama.cpp 有 GPU 输出
                    }
                }
            }
            catch (Exception ex)
            {
                result.RawOutput = $"Error: {ex.Message}\n{outputBuilder}";
            }
            finally
            {
                try
                {
                    if (process != null && !process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit(3000);
                    }
                    process?.Dispose();
                }
                catch { }
            }

            return result;
        }

        /// <summary>
        /// 解析速度输出行（实时检测，检测到最终结果则终止进程）
        /// </summary>
        private void ParseSpeedLine(string line, ref double lastPromptSpeed, ref double lastGenSpeed, ref bool testCompleted, Process process)
        {
            var trimmed = line.Trim();

            // 检测速度信息
            if (trimmed.Contains("t/s"))
            {
                var promptMatch = System.Text.RegularExpressions.Regex.Match(trimmed, @"Prompt:?\s*([\d.]+)\s*t/s");
                var genMatch = System.Text.RegularExpressions.Regex.Match(trimmed, @"Generation:?\s*([\d.]+)\s*t/s");
                if (promptMatch.Success && double.TryParse(promptMatch.Groups[1].Value, out var p))
                    lastPromptSpeed = p;
                if (genMatch.Success && double.TryParse(genMatch.Groups[1].Value, out var g))
                    lastGenSpeed = g;
            }

            // 检测最终结果标志
            if (!testCompleted)
            {
                var finalMatch = System.Text.RegularExpressions.Regex.Match(trimmed,
                    @"\[?\s*Prompt:\s*[\d.]+\s*t/s\s*\|\s*Generation:\s*[\d.]+\s*t/s\s*\]?",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (finalMatch.Success)
                {
                    testCompleted = true;
                    try
                    {
                        if (!process.HasExited) process.Kill();
                    }
                    catch { }
                }
            }
        }

        /// <summary>
        /// 从完整输出中解析速度结果（后备解析）
        /// </summary>
        private void ParseSpeedFromOutput(SpeedTestResult result)
        {
            if (string.IsNullOrEmpty(result.RawOutput)) return;

            var lines = result.RawOutput.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.Contains("prompt", StringComparison.OrdinalIgnoreCase))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"([\d.]+)\s*t/s",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success && result.PromptSpeed == 0 && double.TryParse(match.Groups[1].Value, out var speed))
                        result.PromptSpeed = speed;
                }

                if (trimmed.Contains("eval", StringComparison.OrdinalIgnoreCase) || trimmed.Contains("generation", StringComparison.OrdinalIgnoreCase))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(trimmed, @"([\d.]+)\s*t/s",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success && result.GenerationSpeed == 0 && double.TryParse(match.Groups[1].Value, out var speed))
                        result.GenerationSpeed = speed;
                }
            }
        }

        /// <summary>
        /// 从 llama.cpp 输出行中尝试解析 GPU 显卡信息
        /// 匹配格式: llama_model_load_from_file_impl: using device CUDA0 (NVIDIA ...) (0000:...) - XXXXX MiB free
        /// 成功时返回 true 并添加到 _gpuInfos 列表，重复设备不重复添加
        /// </summary>
        private bool TryParseGpuInfo(string line)
        {
            if (string.IsNullOrEmpty(line)) return false;

            var trimmed = line.TrimStart();

            // 模式1（主要）: 匹配 ggml_cuda_init 设备行
            //   Device 0: NVIDIA GeForce RTX 3080 Laptop GPU, compute capability 8.6, VMM: yes, VRAM: 16383 MiB
            //   Device 1: NVIDIA GeForce RTX 3060 Laptop GPU, compute capability 8.6, VMM: yes, VRAM: 12287 MiB
            if (trimmed.StartsWith("Device ", StringComparison.Ordinal))
            {
                var m = System.Text.RegularExpressions.Regex.Match(
                    trimmed,
                    @"Device\s+(\d+):\s*(.+?),\s*compute\s+capability\s+\S+,\s*VMM:\s*\w+,\s*VRAM:\s*(\d+)\s*MiB");
                if (m.Success)
                {
                    var devId = int.Parse(m.Groups[1].Value);
                    var gpuName = m.Groups[2].Value.Trim();
                    var vramMB = long.Parse(m.Groups[3].Value);

                    lock (_gpuLock)
                    {
                        if (_gpuInfos.Any(g => g.DeviceId == devId))
                            return true;

                        _gpuInfos.Add(new GpuInfo
                        {
                            DeviceId = devId,
                            Name = gpuName,
                            PciBusId = "",
                            FreeMemoryMB = vramMB
                        });

                        var displayLine = $"🟢 显卡{devId + 1}：{gpuName}    ({vramMB} MiB)";
                        this.BeginInvoke(new Action(() => AppendRunningLog(displayLine)));
                    }
                    return true;
                }
                return false;
            }

            // 模式2（兼容旧版本 llama.cpp）: using device CUDA0 (name) (pci) - X MiB free
            {
                var pattern = @"using device\s+CUDA(\d+)\s+\((.+?)\)\s+\((.+?)\)\s*-\s*(\d+)\s+MiB\s+free";
                var match = System.Text.RegularExpressions.Regex.Match(line, pattern);
                if (!match.Success)
                {
                    pattern = @"using device\s+CUDA:?(\d+).*?\(([^)]+)\).*?\(([^)]+)\).*?(\d+)\s+MiB";
                    match = System.Text.RegularExpressions.Regex.Match(line, pattern);
                }
                if (match.Success)
                {
                    var devId = int.Parse(match.Groups[1].Value);
                    var gpuName = match.Groups[2].Value.Trim();
                    var pciBusId = match.Groups[3].Value.Trim();
                    var freeMemoryMB = long.Parse(match.Groups[4].Value);

                    lock (_gpuLock)
                    {
                        if (_gpuInfos.Any(g => g.DeviceId == devId))
                            return true;

                        _gpuInfos.Add(new GpuInfo
                        {
                            DeviceId = devId,
                            Name = gpuName,
                            PciBusId = pciBusId,
                            FreeMemoryMB = freeMemoryMB
                        });

                        var displayLine = $"🟢 显卡{devId + 1}：{gpuName}    ({freeMemoryMB} MiB)";
                        this.BeginInvoke(new Action(() => AppendRunningLog(displayLine)));
                    }
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 清空已缓存的GPU信息（每次测试前调用）
        /// </summary>
        private void ClearGpuInfos()
        {
            lock (_gpuLock)
            {
                _gpuInfos.Clear();
            }
        }

        /// <summary>
        /// 获取GPU信息显示文本列表（用于日志输出）
        /// </summary>
        private List<string> GetGpuDisplayLines()
        {
            lock (_gpuLock)
            {
                var lines = new List<string>();
                foreach (var gpu in _gpuInfos.OrderBy(g => g.DeviceId))
                {
                    lines.Add($"{gpu}    ({gpu.FreeMemoryMB} MiB)");
                }
                return lines;
            }
        }


        /// <summary>
        /// 获取选中的上下文大小列表
        /// </summary>
        private List<int> GetSelectedContextSizes()
        {
            var selected = new List<int>();

            if (cb_contentTest_4k.Checked) selected.Add(4096);
            if (cb_contentTest_8k.Checked) selected.Add(8192);
            if (cb_contentTest_16k.Checked) selected.Add(16384);
            if (cb_contentTest_32k.Checked) selected.Add(32768);
            if (cb_contentTest_64k.Checked) selected.Add(65536);
            if (cb_contentTest_96k.Checked) selected.Add(98304);
            if (cb_contentTest_128k.Checked) selected.Add(131072);
            if (cb_contentTest_160k.Checked) selected.Add(163840);
            if (cb_contentTest_192k.Checked) selected.Add(196608);
            if (cb_contentTest_224k.Checked) selected.Add(229376);
            if (cb_contentTest_256k.Checked) selected.Add(262144);
            if (cb_contentTest_288k.Checked) selected.Add(294912);
            if (cb_contentTest_320k.Checked) selected.Add(327680);
            if (cb_contentTest_352k.Checked) selected.Add(360448);
            if (cb_contentTest_384k.Checked) selected.Add(393216);
            if (cb_contentTest_416k.Checked) selected.Add(425984);
            if (cb_contentTest_448k.Checked) selected.Add(458752);
            if (cb_contentTest_480k.Checked) selected.Add(491520);
            if (cb_contentTest_512k.Checked) selected.Add(524288);
            if (cb_contentTest_544k.Checked) selected.Add(557056);
            if (cb_contentTest_576k.Checked) selected.Add(589824);
            if (cb_contentTest_608k.Checked) selected.Add(622592);
            if (cb_contentTest_640k.Checked) selected.Add(655360);
            if (cb_contentTest_672k.Checked) selected.Add(688128);
            if (cb_contentTest_704k.Checked) selected.Add(720896);
            if (cb_contentTest_736k.Checked) selected.Add(753664);
            if (cb_contentTest_768k.Checked) selected.Add(786432);
            if (cb_contentTest_800k.Checked) selected.Add(819200);
            if (cb_contentTest_832k.Checked) selected.Add(851968);
            if (cb_contentTest_864k.Checked) selected.Add(884736);
            if (cb_contentTest_896k.Checked) selected.Add(917504);
            if (cb_contentTest_928k.Checked) selected.Add(950272);
            if (cb_contentTest_960k.Checked) selected.Add(983040);
            if (cb_contentTest_1M.Checked) selected.Add(1048576);
            if (cb_contentTest_2M.Checked) selected.Add(2097152);
            if (cb_contentTest_5M.Checked) selected.Add(5242880);

            return selected;
        }

        /// <summary>
        /// 获取上下文的显示名称
        /// </summary>
        private static string GetContextName(int contextSize)
        {
            return contextSize switch
            {
                4096 => "4K",
                8192 => "8K",
                16384 => "16K",
                32768 => "32K",
                65536 => "64K",
                98304 => "96K",
                131072 => "128K",
                163840 => "160K",
                196608 => "192K",
                229376 => "224K",
                262144 => "256K",
                294912 => "288K",
                327680 => "320K",
                360448 => "352K",
                393216 => "384K",
                425984 => "416K",
                458752 => "448K",
                491520 => "480K",
                524288 => "512K",
                557056 => "544K",
                589824 => "576K",
                622592 => "608K",
                655360 => "640K",
                688128 => "672K",
                720896 => "704K",
                753664 => "736K",
                786432 => "768K",
                819200 => "800K",
                851968 => "832K",
                884736 => "864K",
                917504 => "896K",
                950272 => "928K",
                983040 => "960K",
                1048576 => "1M",
                2097152 => "2M",
                5242880 => "5M",
                _ => contextSize.ToString()
            };
        }

        /// <summary>
        /// 生成批量测试汇总报告
        /// </summary>
        private void GenerateBatchReport(List<SpeedTestResult> results)
        {
            if (results.Count == 0) return;

            var modelPath = GetSelectedModelFullPath();
            var modelName = string.IsNullOrEmpty(modelPath) ? "未知" : Path.GetFileName(modelPath);
            var baseArgs = BuildSpeedTestBaseArgs();
            var testList = string.Join(", ", results.OrderBy(x => x.ContextSize).Select(x => GetContextName(x.ContextSize)));

            var report = new StringBuilder();
            var testTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            report.AppendLine("========================================================");
            report.AppendLine($"📊 批量测速汇总报告  测试时间：{testTime}");
            report.AppendLine("========================================================");
            report.AppendLine($"📁 模型：{modelName}");
            report.AppendLine($"📋 基础参数：{baseArgs}");
            report.AppendLine($"📊 测试列表：{testList}");
            report.AppendLine();

            // GPU显卡信息（如果有，带图标）
            if (_gpuInfos.Count > 0)
            {
                foreach (var gpu in _gpuInfos.OrderBy(g => g.DeviceId))
                {
                    report.AppendLine($"🟢 显卡{gpu.DeviceId + 1}：{gpu.Name}    ({gpu.FreeMemoryMB} MiB)");
                }
                report.AppendLine();
            }

            // 简洁行格式汇总（与单条结果格式一致）
            foreach (var r in results.OrderBy(x => x.ContextSize))
            {
                var ctxName = GetContextName(r.ContextSize);
                report.AppendLine($"✅ CTX={ctxName}: Prompt={r.PromptSpeed:F1} t/s | Generation={r.GenerationSpeed:F1} t/s");
            }

            // 统计信息
            var validResults = results.Where(r => r.GenerationSpeed > 0).ToList();
            if (validResults.Count > 0)
            {
                report.AppendLine();
                report.AppendLine("📈 统计信息：");
                report.AppendLine($"  平均提示词速度：{validResults.Average(x => x.PromptSpeed):F1} t/s");
                report.AppendLine($"  平均生成速度：{validResults.Average(x => x.GenerationSpeed):F1} t/s");
                report.AppendLine($"  最大生成速度：{validResults.Max(x => x.GenerationSpeed):F1} t/s ({GetContextName(validResults.First(x => x.GenerationSpeed == validResults.Max(y => y.GenerationSpeed)).ContextSize)})");
                report.AppendLine($"  最小生成速度：{validResults.Min(x => x.GenerationSpeed):F1} t/s ({GetContextName(validResults.First(x => x.GenerationSpeed == validResults.Min(y => y.GenerationSpeed)).ContextSize)})");
            }

            report.AppendLine("========================================================");

            // 输出到测速结果日志和运行日志
            var reportText = report.ToString();
            AppendRunningLog(reportText);
            LogMessage(reportText);
        }

        /// <summary>
        /// 追加日志到测速结果面板（不带时间戳前缀，由调用方自行控制格式）
        /// </summary>
        private void AppendRunningLog(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(AppendRunningLog), message);
                return;
            }

            tb_runningLogs.AppendText($"{message}{Environment.NewLine}");
            tb_runningLogs.ScrollToCaret();
        }

        /// <summary>
        /// CPU 多线程测试
        /// </summary>
        private async void btn_multithreadTest_cpu_Click(object sender, EventArgs e)
        {
            // 检查误点击
            if (IsRapidClick()) return;

            // 如果正在运行，点击按钮则停止
            if (_isRunning)
            {
                LogMessage("⚠️ 正在停止 CPU 多线程测试...");
                AppendRunningLog("⚠️ 正在停止 CPU 多线程测试...");
                _stopTest = true;
                _isRunning = false;
                btn_multithreadTest_cpu.Text = "CPU多线程测速";
                return;
            }

            // 获取线程范围和步长
            if (!int.TryParse(tb_multithreadTest_start.Text, out var startThread) ||
                !int.TryParse(tb_multithreadTest_end.Text, out var endThread) ||
                !int.TryParse(tb_multithreadTest_step.Text, out var stepThread))
            {
                MessageBox.Show("请输入有效的线程数范围！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (startThread < 1 || endThread < startThread || stepThread < 1)
            {
                MessageBox.Show("线程数范围输入不合法！\n起始线程 ≥ 1，结束线程 ≥ 起始线程，步长 ≥ 1", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            if (string.IsNullOrEmpty(llamaPath) || string.IsNullOrEmpty(modelPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径和选择模型！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var cliExe = Path.Combine(llamaPath, "llama-cli.exe");
            if (!File.Exists(cliExe))
            {
                MessageBox.Show($"llama-cli.exe 不存在！\n{cliExe}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!File.Exists(modelPath))
            {
                MessageBox.Show($"模型文件不存在！\n{modelPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 获取上下文大小
            var contextSizeText = cb_multithreadTest_contentLength.Text;
            var contextSize = ParseContextSizeFromText(contextSizeText);

            _isRunning = true;
            _stopTest = false;
            btn_multithreadTest_cpu.Text = "停止CPU测试";

            var results = new List<SpeedTestResult>();

            try
            {
                // 清空测速结果日志
                tb_runningLogs.Clear();

                // 从 _paramManager 获取基础参数（排除 -c/-m/-p/-n/-t，-t 由测速流程遍历指定）
                var baseArgs = BuildSpeedTestBaseArgs(excludeThread: true);

                // 显示测试配置信息
                LogMessage("========================================================");
                LogMessage("🚀 开始 CPU 多线程测速");
                LogMessage("========================================================");
                LogMessage($"📁 模型：{Path.GetFileName(modelPath)}");
                LogMessage($"🔧 上下文：{GetContextName(contextSize)} ({contextSize:N0})");
                LogMessage($"📊 线程范围：{startThread} - {endThread}，步长：{stepThread}");
                LogMessage($"📋 基础参数：{baseArgs}");
                LogMessage("========================================================");

                AppendRunningLog("========================================================");
                AppendRunningLog("🚀 开始 CPU 多线程测速");
                AppendRunningLog($"📁 模型：{Path.GetFileName(modelPath)}");
                AppendRunningLog($"🔧 上下文：{GetContextName(contextSize)} ({contextSize:N0})");
                AppendRunningLog($"📊 线程范围：{startThread} - {endThread}，步长：{stepThread}");
                AppendRunningLog("========================================================");

                var totalTests = ((endThread - startThread) / stepThread) + 1;
                var currentTest = 0;

                // 遍历不同线程数进行测试
                for (var threadCount = startThread; threadCount <= endThread; threadCount += stepThread)
                {
                    // 检查停止标志
                    if (_stopTest)
                    {
                        LogMessage("⚠️ CPU 多线程测试已停止");
                        AppendRunningLog("⚠️ CPU 多线程测试已停止");
                        break;
                    }

                    currentTest++;
                    LogMessage($"\n进度：[{currentTest}/{totalTests}] 线程数={threadCount}");

                    // 组装完整命令：基础参数 + -c 上下文 + -t 线程数 + -m 模型 + -p 提示词
                    var fullArgs = $"-m \"{modelPath}\" -c {contextSize} -t {threadCount} {baseArgs} -p \"hello\" -n 128";

                    var result = await RunSingleSpeedTestAsync(cliExe, fullArgs, contextSize, llamaPath);
                    result.ThreadCount = threadCount;
                    results.Add(result);

                    // 输出结果
                    var resultLine = $"✅ 线程={threadCount}: Prompt={result.PromptSpeed:F1} t/s | Generation={result.GenerationSpeed:F1} t/s";
                    LogMessage(resultLine);
                    AppendRunningLog(resultLine);

                    // 等待显存释放
                    if (currentTest < totalTests && !_stopTest)
                    {
                        LogMessage("⏳ 等待资源释放...");
                        await Task.Delay(2000);
                    }
                }

                // 生成汇总报告
                if (!_stopTest && results.Count > 0)
                {
                    GenerateCpuThreadReport(results, contextSize);
                    LogMessage("========================================================");
                    LogMessage("✅ CPU 多线程测试完成");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"❌ CPU 多线程测试错误：{ex.Message}");
                AppendRunningLog($"❌ CPU 多线程测试错误：{ex.Message}");
            }
            finally
            {
                _isRunning = false;
                _stopTest = false;
                btn_multithreadTest_cpu.Text = "CPU多线程测速";
            }
        }

        /// <summary>
        /// 从 ComboBox 文本中解析上下文大小（如 "8192 (8K)" → 8192）
        /// </summary>
        private static int ParseContextSizeFromText(string text)
        {
            if (string.IsNullOrEmpty(text)) return 4096;

            // 提取括号前的数字部分
            var match = System.Text.RegularExpressions.Regex.Match(text, @"^(\d+)");
            if (match.Success && int.TryParse(match.Groups[1].Value, out var size))
                return size;

            return 4096;
        }

        /// <summary>
        /// 生成 CPU 多线程测试汇总报告
        /// </summary>
        private void GenerateCpuThreadReport(List<SpeedTestResult> results, int contextSize)
        {
            if (results.Count == 0) return;

            var modelPath = GetSelectedModelFullPath();
            var modelName = string.IsNullOrEmpty(modelPath) ? "未知" : Path.GetFileName(modelPath);

            var report = new StringBuilder();
            report.AppendLine("========================================================");
            report.AppendLine("📊 CPU 多线程测速汇总报告");
            report.AppendLine("========================================================");
            report.AppendLine($"测试模型：{modelName}");
            report.AppendLine($"上下文大小：{GetContextName(contextSize)} ({contextSize:N0})");
            report.AppendLine();

            // 表格化报告
            report.AppendLine("┌──────────┬────────────┬────────────┐");
            report.AppendLine("│ 线程数   │ 提示速度   │ 生成速度   │");
            report.AppendLine("├──────────┼────────────┼────────────┤");

            foreach (var r in results.OrderBy(x => x.ThreadCount))
            {
                var promptSpeed = r.PromptSpeed.ToString("F1");
                var genSpeed = r.GenerationSpeed.ToString("F1");
                report.AppendLine($"│ {r.ThreadCount,-8} │ {promptSpeed,10} │ {genSpeed,10} │");
            }

            report.AppendLine("└──────────┴────────────┴────────────┘");

            // 统计信息
            var validResults = results.Where(r => r.GenerationSpeed > 0).ToList();
            if (validResults.Count > 0)
            {
                var bestResult = validResults.OrderByDescending(x => x.GenerationSpeed).First();
                report.AppendLine();
                report.AppendLine("📈 统计信息：");
                report.AppendLine($"  最优线程数：{bestResult.ThreadCount}，生成速度：{bestResult.GenerationSpeed:F1} t/s");
                report.AppendLine($"  平均提示词速度：{validResults.Average(x => x.PromptSpeed):F1} t/s");
                report.AppendLine($"  平均生成速度：{validResults.Average(x => x.GenerationSpeed):F1} t/s");
                report.AppendLine($"  最大生成速度：{validResults.Max(x => x.GenerationSpeed):F1} t/s (线程={validResults.First(x => x.GenerationSpeed == validResults.Max(x => x.GenerationSpeed)).ThreadCount})");
                report.AppendLine($"  最小生成速度：{validResults.Min(x => x.GenerationSpeed):F1} t/s (线程={validResults.First(x => x.GenerationSpeed == validResults.Min(x => x.GenerationSpeed)).ThreadCount})");
            }

            report.AppendLine("========================================================");

            // 输出到测速结果日志和运行日志
            var reportText = report.ToString();
            AppendRunningLog(reportText);
            LogMessage(reportText);
        }

        #endregion

        // 系统托盘相关事件处理方法
        private void mainFrm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
                notifyIcon.Visible = true;
                notifyIcon.ShowBalloonTip(1000, "LlamaForms", "程序已最小化到系统托盘", ToolTipIcon.Info);
            }
        }

        private void 显示主窗口ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            Application.Exit();
        }

        /// <summary>
        /// 窗口关闭前清理：终止所有 llama-server / llama-cli 子进程
        /// </summary>
        private void mainFrm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // 1. 关闭已跟踪的 llama-server 进程
            CloseServerProcess();

            // 2. 扫描并终止所有残留的 llama-server / llama-cli 进程
            KillRemainingProcesses("llama-server");
            KillRemainingProcesses("llama-cli");
        }

        /// <summary>
        /// 按进程名查找并终止所有匹配的进程
        /// </summary>
        private void KillRemainingProcesses(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var p in processes)
                {
                    try
                    {
                        if (!p.HasExited)
                        {
                            p.Kill();
                            p.WaitForExit(3000);
                        }
                    }
                    catch { }
                    finally
                    {
                        p.Dispose();
                    }
                }
            }
            catch { }
        }

        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            Show();
            WindowState = FormWindowState.Normal;
            notifyIcon.Visible = false;
        }

        // ── 分隔条位置保存与恢复 ─────────────────────────────────────────

        /// <summary>
        /// 从配置恢复分隔条位置
        /// </summary>
        private void RestoreSplitterPositions()
        {
            var cfg = _configManager.Config;
            try
            {
                if (cfg.SplitterDistance > 0 && cfg.SplitterDistance < splitContainer.Height - 50)
                    splitContainer.SplitterDistance = cfg.SplitterDistance;
            }
            catch { }
            try
            {
                if (cfg.SplitterDistanceLeft > 0 && cfg.SplitterDistanceLeft < splitContainer_left.Height - 50)
                    splitContainer_left.SplitterDistance = cfg.SplitterDistanceLeft;
            }
            catch { }
        }

        /// <summary>
        /// 保存右侧分隔条位置
        /// </summary>
        private void SaveSplitterPosition()
        {
            try
            {
                _configManager.Config.SplitterDistance = splitContainer.SplitterDistance;
                _configManager.SaveConfig();
            }
            catch { }
        }

        /// <summary>
        /// 保存左侧分隔条位置
        /// </summary>
        private void SaveSplitterPositionLeft()
        {
            try
            {
                _configManager.Config.SplitterDistanceLeft = splitContainer_left.SplitterDistance;
                _configManager.SaveConfig();
            }
            catch { }
        }

        // ─────────────────────────────────────────────────────────────────
        //  v2.4.0+ 新增功能事件 handler
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 高级工具 tab 上的「开始并发压测」按钮 —— 转发到 tb_multithreadTest_request_Click
        /// </summary>
        private async void btn_runConcurrentTest_Click(object sender, EventArgs e)
        {
            await RunConcurrentTestAsync();
        }

        /// <summary>
        /// 多线程测试 tab 上的「并发请求测速」按钮（HTTP 并发打 llama-server）
        /// </summary>
        private async void tb_multithreadTest_request_Click(object sender, EventArgs e)
        {
            await RunConcurrentTestAsync();
        }

        /// <summary>
        /// HTTP 并发请求 llama-server 的 /v1/chat/completions，统计吞吐
        /// </summary>
        private async Task RunConcurrentTestAsync()
        {
            if (IsRapidClick()) return;

            // 切换到运行日志标签页
            tabControl_logs.SelectedTab = tabPage_log_runLogs;

            var llamaPath = tb_llamaPath.Text.Trim();
            if (string.IsNullOrEmpty(llamaPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!Directory.Exists(llamaPath))
            {
                MessageBox.Show($"llama.cpp 目录不存在：{llamaPath}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 确保 llama-server 在跑
            var running = Process.GetProcessesByName("llama-server");
            if (running.Length == 0)
            {
                MessageBox.Show("未检测到 llama-server 进程！\n请先到「启动配置」页点击「开启 llama.cpp 的 web 访问」并等待 server 完全启动。",
                    "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(tb_concurrent_count.Text, out var concurrency) || concurrency < 1 || concurrency > 64)
            {
                MessageBox.Show("并发线程数必须是 1~64 的整数", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var apiUrl = tb_concurrent_url.Text.Trim();
            var prompt = tb_concurrent_prompt.Text.Trim();

            if (string.IsNullOrEmpty(apiUrl))
            {
                MessageBox.Show("API 地址不能为空", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            tb_runningLogs.Clear();
            LogMessage("========================================================");
            LogMessage($"🚀 开始并发请求压测（{concurrency} 并发 × 1 请求）");
            LogMessage($"📡 API: {apiUrl}");
            LogMessage($"📝 Prompt: {prompt}");
            LogMessage("========================================================");

            var apiKey = tb_apiKey.Text.Trim();
            var successCount = 0;
            var failCount = 0;
            var totalTokens = 0;
            var totalElapsedMs = 0L;
            var swTotal = Stopwatch.StartNew();
            var concurrencyLevel = new SemaphoreSlim(concurrency);
            var tasks = new List<Task<(bool ok, int tokens, long elapsedMs, string err)>>();

            using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
            if (!string.IsNullOrEmpty(apiKey) && apiKey != "***")
            {
                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
            }

            for (var i = 0; i < concurrency; i++)
            {
                var idx = i;
                tasks.Add(Task.Run(async () =>
                {
                    await concurrencyLevel.WaitAsync();
                    var sw = Stopwatch.StartNew();
                    try
                    {
                        var body = $"{{\"model\":\"any\",\"stream\":false,\"messages\":[{{\"role\":\"user\",\"content\":\"{EscapeJson(prompt)}\"}}]}}";
                        var resp = await httpClient.PostAsync(apiUrl,
                            new StringContent(body, Encoding.UTF8, "application/json"));
                        var text = await resp.Content.ReadAsStringAsync();
                        sw.Stop();
                        if (!resp.IsSuccessStatusCode)
                        {
                            return (false, 0, sw.ElapsedMilliseconds, $"HTTP {(int)resp.StatusCode}: {Truncate(text, 200)}");
                        }
                        // 尝试解析 usage.total_tokens
                        var tokens = ExtractTotalTokens(text);
                        return (true, tokens, sw.ElapsedMilliseconds, "");
                    }
                    catch (Exception ex)
                    {
                        sw.Stop();
                        return (false, 0, sw.ElapsedMilliseconds, ex.Message);
                    }
                    finally
                    {
                        concurrencyLevel.Release();
                    }
                }));
            }

            try
            {
                var results = await Task.WhenAll(tasks);
                swTotal.Stop();

                LogMessage("--------------------------------------------------------");
                foreach (var (ok, tokens, elapsedMs, err) in results.Select((r, i) => (r.ok, r.tokens, r.elapsedMs, r.err)))
                {
                    if (ok)
                    {
                        successCount++;
                        totalTokens += tokens;
                        totalElapsedMs += elapsedMs;
                        var tps = elapsedMs > 0 ? tokens * 1000.0 / elapsedMs : 0;
                        LogMessage($"  ✅ 请求成功  tokens={tokens}  用时={elapsedMs}ms  吞吐={tps:F1} t/s");
                    }
                    else
                    {
                        failCount++;
                        LogMessage($"  ❌ 请求失败  用时={elapsedMs}ms  错误={err}");
                    }
                }
                LogMessage("========================================================");
                LogMessage($"📊 汇总: 成功 {successCount}/{concurrency}  失败 {failCount}");
                LogMessage($"⏱️  总耗时: {swTotal.ElapsedMilliseconds}ms");
                LogMessage($"🧮 累计生成 tokens: {totalTokens}");
                if (successCount > 0)
                {
                    var avgTps = totalElapsedMs > 0 ? totalTokens * 1000.0 / totalElapsedMs : 0;
                    LogMessage($"🚀 平均吞吐: {avgTps:F1} t/s");
                }
                LogMessage("========================================================");
            }
            catch (Exception ex)
            {
                LogMessage($"❌ 并发压测异常: {ex.Message}");
            }
        }

        /// <summary>
        /// llama-bench 性能基准测试
        /// </summary>
        private async void btn_runBench_Click(object sender, EventArgs e)
        {
            if (IsRapidClick()) return;

            tabControl_logs.SelectedTab = tabPage_log_runLogs;
            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            if (string.IsNullOrEmpty(llamaPath) || !Directory.Exists(llamaPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath))
            {
                MessageBox.Show("请先选择有效的模型文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var benchExe = Path.Combine(llamaPath, "llama-bench.exe");
            if (!File.Exists(benchExe))
            {
                MessageBox.Show($"未找到 llama-bench.exe\n{benchExe}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // 关闭已运行的 bench
            foreach (var p in Process.GetProcessesByName("llama-bench"))
            {
                try { p.Kill(); } catch { }
            }

            var args = $"-m \"{modelPath}\" -p 512 -n 128";

            LogMessage("========================================================");
            LogMessage("📊 llama-bench 性能基准测试");
            LogMessage($"📁 模型: {Path.GetFileName(modelPath)}");
            LogMessage($"▶️  执行: \"{benchExe}\" {args}");
            LogMessage("========================================================");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = benchExe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = llamaPath
                };
                var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
                proc.OutputDataReceived += (s, ea) => { if (!string.IsNullOrEmpty(ea.Data)) LogMessage(ea.Data); };
                proc.ErrorDataReceived += (s, ea) => { if (!string.IsNullOrEmpty(ea.Data)) LogMessage($"[STDERR] {ea.Data}"); };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                await proc.WaitForExitAsync();
                LogMessage($"✅ llama-bench 完成，退出码: {proc.ExitCode}");
                proc.Dispose();
            }
            catch (Exception ex)
            {
                LogMessage($"❌ llama-bench 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// llama-template-analysis 聊天模板分析
        /// </summary>
        private async void btn_runTemplateAnalysis_Click(object sender, EventArgs e)
        {
            if (IsRapidClick()) return;

            tabControl_logs.SelectedTab = tabPage_log_runLogs;
            var llamaPath = tb_llamaPath.Text.Trim();
            var modelPath = GetSelectedModelFullPath();

            if (string.IsNullOrEmpty(llamaPath) || !Directory.Exists(llamaPath))
            {
                MessageBox.Show("请先设置 llama.cpp 路径", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var tplExe = Path.Combine(llamaPath, "llama-template-analysis.exe");
            if (!File.Exists(tplExe))
            {
                MessageBox.Show($"未找到 llama-template-analysis.exe\n{tplExe}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            foreach (var p in Process.GetProcessesByName("llama-template-analysis"))
            {
                try { p.Kill(); } catch { }
            }

            var args = string.IsNullOrEmpty(modelPath) || !File.Exists(modelPath)
                ? ""
                : $"-m \"{modelPath}\"";

            LogMessage("========================================================");
            LogMessage("🧪 llama-template-analysis 模板分析");
            if (!string.IsNullOrEmpty(modelPath))
                LogMessage($"📁 模型: {Path.GetFileName(modelPath)}");
            LogMessage($"▶️  执行: \"{tplExe}\" {args}");
            LogMessage("========================================================");

            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = tplExe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    WorkingDirectory = llamaPath
                };
                var proc = new Process { StartInfo = psi, EnableRaisingEvents = true };
                proc.OutputDataReceived += (s, ea) => { if (!string.IsNullOrEmpty(ea.Data)) LogMessage(ea.Data); };
                proc.ErrorDataReceived += (s, ea) => { if (!string.IsNullOrEmpty(ea.Data)) LogMessage($"[STDERR] {ea.Data}"); };
                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                await proc.WaitForExitAsync();
                LogMessage($"✅ llama-template-analysis 完成，退出码: {proc.ExitCode}");
                proc.Dispose();
            }
            catch (Exception ex)
            {
                LogMessage($"❌ llama-template-analysis 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// llama-results 汇总展示 —— 直接跳到测试日志 tab
        /// </summary>
        private void btn_runResults_Click(object sender, EventArgs e)
        {
            tabControl_logs.SelectedTab = tabPage_log_testLogs;
            LogMessage("========================================================");
            LogMessage("📋 查看历史测试汇总报告");
            LogMessage("💡 请翻阅上方的「测试结果」日志中的批量测速 / 多线程 / 并发测试汇总");
            LogMessage("========================================================");
        }

        // ── 辅助函数 ───────────────────────────────────────────────────────
        private static string EscapeJson(string s) => s.Replace(@"\", @"\\").Replace("\"", "\\\"").Replace(new string((char)13, 1), "\\" + "r").Replace(new string((char)10, 1), "\\" + "n").Replace(new string((char)9, 1), "\\" + "t");

        private static string Truncate(string s, int max) =>
            string.IsNullOrEmpty(s) || s.Length <= max ? s : s.Substring(0, max) + "...";

        /// <summary>
        /// 从 OpenAI 兼容响应 JSON 中尽力抽取 usage.total_tokens
        /// 避免引入完整 JSON 解析器依赖
        /// </summary>
        private static int ExtractTotalTokens(string json)
        {
            if (string.IsNullOrEmpty(json)) return 0;
            var key = "\"total_tokens\"";
            var idx = json.IndexOf(key, StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return 0;
            var colon = json.IndexOf(':', idx + key.Length);
            if (colon < 0) return 0;
            // 跳过空白
            var p = colon + 1;
            while (p < json.Length && (json[p] == ' ' || json[p] == '\t')) p++;
            var end = p;
            while (end < json.Length && (char.IsDigit(json[end]) || json[end] == '-')) end++;
            if (end == p) return 0;
            return int.TryParse(json.Substring(p, end - p), out var n) ? n : 0;
        }
    }
}

/// <summary>
/// Token 速度测试结果
/// </summary>

/// <summary>
/// GPU 显卡信息（从 llama.cpp 输出日志中解析）
/// </summary>
public class GpuInfo
{
    /// <summary>CUDA 设备编号 (0, 1, ...)</summary>
    public int DeviceId { get; set; }

    /// <summary>显卡名称 (如 NVIDIA GeForce RTX 3080 Laptop GPU)</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>PCI 总线地址 (如 0000:01:00.0)</summary>
    public string PciBusId { get; set; } = string.Empty;

    /// <summary>可用显存 (MiB)</summary>
    public long FreeMemoryMB { get; set; }

    /// <summary>格式化显示文本</summary>
    public override string ToString() => $"显卡{DeviceId + 1}：{Name}";
}

/// <summary>
/// Token 速度测试结果
/// </summary>
public class SpeedTestResult
{
    public int ContextSize { get; set; }
    public int ThreadCount { get; set; }
    public double PromptSpeed { get; set; }       // tokens/s (提示词速度)
    public double GenerationSpeed { get; set; }    // tokens/s (生成速度)
    public string FullCommand { get; set; } = string.Empty;
    public string RawOutput { get; set; } = string.Empty;
}
