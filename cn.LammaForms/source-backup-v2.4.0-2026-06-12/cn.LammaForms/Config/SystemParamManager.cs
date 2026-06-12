using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace cn.LammaForms.Config
{
    /// <summary>
    /// 系统参数管理器
    /// 负责解析 llama.cpp 启动参数文档，生成和管理 system.json
    /// </summary>
    public class SystemParamManager
    {
        private static SystemParamManager? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// system.json 文件路径
        /// </summary>
        private readonly string _systemJsonPath;

        /// <summary>
        /// llama.cpp 启动参数文档路径
        /// </summary>
        private readonly string _paramDocPath;

        /// <summary>
        /// 参数分类列表
        /// </summary>
        public List<ParamCategory> Categories { get; private set; } = new List<ParamCategory>();

        /// <summary>
        /// 所有参数的字典（快速查找）
        /// </summary>
        public Dictionary<string, SystemParamItem> ParamDict { get; private set; } = new Dictionary<string, SystemParamItem>();

        /// <summary>
        /// 参数变更事件
        /// </summary>
        public event Action? OnParamsChanged;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static SystemParamManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new SystemParamManager();
                    }
                }
                return _instance;
            }
        }

        private SystemParamManager()
        {
            _systemJsonPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "system.json");
            _paramDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "llama.cpp启动参数.md");
            
            // 如果上面的路径不存在，尝试其他可能的路径
            if (!File.Exists(_paramDocPath))
            {
                _paramDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "llama.cpp启动参数.md");
            }

            LoadOrGenerateSystemJson();
        }

        /// <summary>
        /// 加载或生成 system.json
        /// 数据源优先级：
        ///   1. 如果 system.json 存在且有效 → 直接加载（保留用户的启用/禁用、当前值等修改）
        ///   2. 如果 system.json 不存在或损坏 → 从 CreateDefaultParams() 生成（唯一数据源）
        /// </summary>
        public void LoadOrGenerateSystemJson()
        {
            if (File.Exists(_systemJsonPath))
            {
                try
                {
                    string json = File.ReadAllText(_systemJsonPath);
                    // 如果包含已废弃的分类或已知 bug 残留，强制重新生成
                    if (json.Contains("服务端专属参数") || json.Contains("\"Name\":\"--model\"") ||
                        json.Contains("\"name\":\"---") || json.Contains("\"Name\":\"---"))
                    {
                        RegenerateFromDefaults();
                        return;
                    }

                    // 尝试加载
                    LoadFromSystemJson();
                    return;
                }
                catch
                {
                    // JSON 损坏，重新生成
                }
            }

            // system.json 不存在或损坏，从 CreateDefaultParams 生成
            RegenerateFromDefaults();
        }

        /// <summary>
        /// 从 CreateDefaultParams() 重新生成 system.json
        /// </summary>
        private void RegenerateFromDefaults()
        {
            try
            {
                if (File.Exists(_systemJsonPath))
                {
                    File.Delete(_systemJsonPath);
                }
            }
            catch { }

            CreateDefaultParams();
            SaveSystemJson();
        }

        /// <summary>
        /// 从 system.json 加载参数
        /// </summary>
        private void LoadFromSystemJson()
        {
            try
            {
                string json = File.ReadAllText(_systemJsonPath);
                var categories = JsonSerializer.Deserialize<List<ParamCategory>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (categories != null)
                {
                    Categories = categories;
                    // 为缺少 SortOrder 的分类和参数补上权重
                    EnsureSortOrders();
                    SortCategories();
                    BuildParamDict();
                    // 修正已知文件路径参数类型（兼容旧 system.json）
                    FixKnownParamTypes();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载 system.json 失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                RegenerateFromDefaults();
            }
        }

        /// <summary>
        /// 从 llama.cpp启动参数.md 文档生成 system.json
        /// </summary>
        public void GenerateFromDocument()
        {
            try
            {
                if (!File.Exists(_paramDocPath))
                {
                    // 如果文档不存在，创建默认参数配置
                    CreateDefaultParams();
                    SaveSystemJson();
                    return;
                }

                string content = File.ReadAllText(_paramDocPath);
                ParseDocument(content);
                EnsureSortOrders();
                SortCategories();
                BuildParamDict();
                SaveSystemJson();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"解析参数文档失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                CreateDefaultParams();
                SaveSystemJson();
            }
        }

        /// <summary>
        /// 解析 Markdown 文档内容
        /// </summary>
        /// <param name="content">文档内容</param>
        private void ParseDocument(string content)
        {
            Categories = new List<ParamCategory>();

            // 按 ## 章节分割文档
            var sections = Regex.Split(content, @"^##\s+", RegexOptions.Multiline);

            foreach (var section in sections)
            {
                if (string.IsNullOrWhiteSpace(section))
                    continue;

                var lines = section.Split('\n');
                var titleLine = lines[0].Trim();

                // 跳过服务端分类（已在右侧服务器启动参数中配置）
                if (titleLine.Contains("服务"))
                    continue;

                // 提取主分类
                var category = new ParamCategory
                {
                    Name = titleLine,
                    DisplayName = titleLine,
                    Description = ""
                };

                // 设置图标和排序权重
                category.Icon = GetCategoryIcon(titleLine);
                category.SortOrder = GetCategorySortOrder(titleLine);

                // 解析主分类下的参数（只取 ### 之前的部分）
                int subSectionStart = section.IndexOf("\n### ", StringComparison.Ordinal);
                string mainSectionText = subSectionStart > 0 
                    ? section.Substring(0, subSectionStart) 
                    : section;
                ParseSectionParams(mainSectionText, category, ref _paramDictRef);

                // 处理子章节（### 开头），每个子章节作为独立分类
                var subSections = Regex.Split(section, @"^###\s+", RegexOptions.Multiline);
                foreach (var subSection in subSections.Skip(1)) // 跳过第一个（主分类内容）
                {
                    if (string.IsNullOrWhiteSpace(subSection))
                        continue;

                    var subLines = subSection.Split('\n');
                    var subTitle = subLines[0].Trim();

                    var subCategory = new ParamCategory
                    {
                        Name = subTitle,
                        DisplayName = subTitle,
                        Description = "",
                        Icon = GetCategoryIcon(subTitle),
                        SortOrder = GetCategorySortOrder(subTitle)
                    };

                    ParseSectionParams(subSection, subCategory, ref _paramDictRef);
                    
                    if (subCategory.Params.Count > 0)
                    {
                        Categories.Add(subCategory);
                    }
                }

                // 跳过核心基础参数中的模型文件参数（已在模型选择中配置）
                category.Params.RemoveAll(p => p.Name == "--model" || p.Name == "-m");

                if (category.Params.Count > 0)
                {
                    Categories.Add(category);
                }
            }

            // 如果没有解析到任何参数，使用默认参数
            if (Categories.Count == 0)
            {
                CreateDefaultParams();
            }
        }

        /// <summary>
        /// 解析一个章节中的表格参数
        /// </summary>
        private void ParseSectionParams(string sectionText, ParamCategory category, ref int sortIndex)
        {
            // 按行分割，找到所有表格行
            var allLines = sectionText.Split('\n').Select(l => l.Trim()).ToList();
            
            // 找到表格行
            var tableLineIndices = new List<int>();
            for (int i = 0; i < allLines.Count; i++)
            {
                if (allLines[i].StartsWith("|") && allLines[i].EndsWith("|"))
                    tableLineIndices.Add(i);
            }

            if (tableLineIndices.Count == 0)
                return;

            // 找表头行，推断列含义
            var headerLine = "";
            int headerIndex = -1;
            foreach (var idx in tableLineIndices)
            {
                var cells = ParseTableCells(allLines[idx]);
                if (cells.Any(c => CleanMarkdownFormatting(c).Contains("参数")))
                {
                    headerLine = allLines[idx];
                    headerIndex = idx;
                    break;
                }
            }

            // 推断各列的语义索引
            int nameCol = 0;      // 参数名列
            int shortNameCol = -1; // 简写列（-1 表示无）
            int descCol = -1;      // 描述列
            int defaultCol = -1;   // 默认值列
            int extraCol = -1;     // 附加说明列（注意事项/取值范围/建议取值/适用场景等）

            if (!string.IsNullOrEmpty(headerLine))
            {
                var headerCells = ParseTableCells(headerLine);
                for (int i = 0; i < headerCells.Count; i++)
                {
                    var h = CleanMarkdownFormatting(headerCells[i]);
                    if (h.Contains("参数") && nameCol == 0) nameCol = i;
                    else if (h.Contains("简写")) shortNameCol = i;
                    else if (h.Contains("功能") || h.Contains("说明")) descCol = i;
                    else if (h.Contains("默认")) defaultCol = i;
                    else if (h.Contains("建议") || h.Contains("取值") || h.Contains("注意") || h.Contains("可选") || h.Contains("压缩") || h.Contains("适用") || h.Contains("场景") || h.Contains("备注") || h.Contains("关系")) extraCol = i;
                }

                // 如果没有找到描述列和默认值列，按位置推断
                if (descCol < 0 && headerCells.Count > 2) descCol = 2;
                if (defaultCol < 0 && extraCol < 0 && headerCells.Count > 3) defaultCol = 3;
            }

            int paramSortIndex = 1;

            foreach (var idx in tableLineIndices)
            {
                var line = allLines[idx];

                // 跳过表格分隔行
                if (Regex.IsMatch(line, @"^\|[\s\-|]+\|$"))
                    continue;

                // 跳过表头行
                if (idx == headerIndex)
                    continue;

                var cells = ParseTableCells(line);
                if (cells.Count < 2)
                    continue;

                // 从参数名列提取参数名
                if (nameCol >= cells.Count) continue;
                string rawName = CleanMarkdownFormatting(cells[nameCol]);
                string paramName = ExtractParamName(rawName);

                if (string.IsNullOrEmpty(paramName) || !paramName.StartsWith("--"))
                    continue;

                // 跳过纯横线（分隔行残留）
                if (paramName.Trim().All(c => c == '-'))
                    continue;

                // 提取简写
                string shortName = "";
                if (shortNameCol >= 0 && shortNameCol < cells.Count)
                {
                    shortName = CleanMarkdownFormatting(cells[shortNameCol]);
                    shortName = ExtractParamName(shortName);
                    if (shortName == "无") shortName = "";
                }

                // 提取描述
                string description = "";
                if (descCol >= 0 && descCol < cells.Count)
                {
                    description = CleanMarkdownFormatting(cells[descCol]);
                }

                // 提取附加说明（注意事项/建议取值/适用场景等）
                string extraInfo = "";
                if (extraCol >= 0 && extraCol < cells.Count)
                {
                    extraInfo = CleanMarkdownFormatting(cells[extraCol]);
                }

                // 提取默认值
                string defaultValue = "";
                if (defaultCol >= 0 && defaultCol < cells.Count)
                {
                    defaultValue = CleanMarkdownFormatting(cells[defaultCol]);
                    // 从描述性默认值中提取真实默认值
                    defaultValue = ExtractRealDefaultValue(defaultValue);
                }

                // 如果没有专门的默认值列，但 extraCol 中包含默认值信息，尝试提取
                if (string.IsNullOrEmpty(defaultValue) && !string.IsNullOrEmpty(extraInfo))
                {
                    defaultValue = ExtractRealDefaultValue(extraInfo);
                }

                // 附加说明合并到描述（去掉已提取为默认值的部分，或者直接追加）
                if (!string.IsNullOrEmpty(extraInfo))
                {
                    description = string.IsNullOrEmpty(description) 
                        ? extraInfo 
                        : $"{description}（{extraInfo}）";
                }

                var param = new SystemParamItem
                {
                    Name = paramName,
                    ShortName = shortName,
                    Description = description,
                    DefaultValue = defaultValue,
                    Category = category.Name,
                    CurrentValue = defaultValue,
                    SortOrder = paramSortIndex++
                };

                // 推断参数类型
                InferParamType(param);

                // 为已知参数覆盖默认值和类型（Markdown文档中部分表格缺少"默认值"列）
                ApplyKnownDefaults(param);

                category.Params.Add(param);
                ParamDict[param.Name] = param;
            }
        }

        /// <summary>
        /// 从描述性默认值文本中提取真实默认值
        /// 例如 "0.1-1.2（默认0.8）" → "0.8"
        /// 例如 "40（默认），设置为0禁用" → "40"
        /// 例如 "0（全部保留）" → "0"
        /// 例如 "1（不拆分）" → "1"
        /// 例如 "无（必填）" → ""
        /// 例如 "无" → ""
        /// 例如 "关闭" → ""
        /// 例如 "开启" → ""
        /// </summary>
        private string ExtractRealDefaultValue(string rawDefault)
        {
            if (string.IsNullOrEmpty(rawDefault))
                return "";

            var text = rawDefault.Trim();

            // 无默认值标记
            if (text == "无" || text == "无（必填）" || text == "关闭" || text == "开启")
                return "";

            // 尝试匹配 "（默认X）" 或 "(默认X)" 模式
            var defaultMatch = Regex.Match(text, @"[（(]默认\s*([\d.\w]+)[）)]");
            if (defaultMatch.Success)
            {
                return defaultMatch.Groups[1].Value;
            }

            // 特殊标记：破折号 "—" 表示无默认值
            if (text == "—" || text == "-" || text == "—")
                return "";

            // 尝试提取括号前的数字：如 "40（默认），设置为0禁用" → "40"
            var numBeforeParen = Regex.Match(text, @"^([\d.]+)\s*[（(]");
            if (numBeforeParen.Success)
            {
                return numBeforeParen.Groups[1].Value;
            }

            // 纯数字直接返回
            if (double.TryParse(text, out _))
                return text;

            // 以数字开头，后面是说明文字：如 "0（全部保留）" 已被上面匹配
            // "1（不拆分）" 也已被上面匹配

            // "0 (关闭)" 格式
            var numWithSpaceParen = Regex.Match(text, @"^([\d.]+)\s*[\(（]");
            if (numWithSpaceParen.Success)
            {
                return numWithSpaceParen.Groups[1].Value;
            }

            // 包含中文的描述性文字（非数值），清空
            if (Regex.IsMatch(text, @"[\u4e00-\u9fff]"))
            {
                // 尝试从中提取数字
                var anyNum = Regex.Match(text, @"([\d.]+)");
                if (anyNum.Success)
                    return anyNum.Groups[1].Value;
                return "";
            }

            return text;
        }
        private bool IsTableHeader(string line)
        {
            var headerKeywords = new[] { "参数", "功能", "说明", "简写", "默认值", "取值", "建议", "注意", "可选值", "压缩比", "适用", "类型", "全称", "位数", "技术", "场景", "备注", "模型", "head_dim" };
            var cells = ParseTableCells(line);
            if (cells.Count == 0) return false;
            
            // 检查第一个单元格是否包含标题关键词
            string firstCell = CleanMarkdownFormatting(cells[0]);
            return headerKeywords.Any(k => firstCell.Contains(k));
        }

        /// <summary>
        /// 解析表格行中的单元格
        /// </summary>
        private List<string> ParseTableCells(string line)
        {
            var result = new List<string>();
            // 去掉首尾的 |
            var trimmed = line.Trim('|');
            var parts = trimmed.Split('|');
            foreach (var part in parts)
            {
                result.Add(part.Trim());
            }
            return result;
        }

        /// <summary>
        /// 清除 Markdown 格式标记（加粗、反引号、斜体等）
        /// </summary>
        private string CleanMarkdownFormatting(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            // 去除加粗 **text**
            text = Regex.Replace(text, @"\*\*(.+?)\*\*", "$1");
            // 去除斜体 *text*
            text = Regex.Replace(text, @"\*(.+?)\*", "$1");
            // 去除反引号 `text`
            text = Regex.Replace(text, @"`(.+?)`", "$1");
            
            return text.Trim();
        }

        /// <summary>
        /// 从带占位符的参数名中提取纯参数名
        /// 例如 "--model FNAME" → "--model"，"-ngl N" → "-ngl"
        /// </summary>
        private string ExtractParamName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return rawName;
            
            // 匹配 --xxx 或 -xxx 格式，忽略后面的占位符
            var match = Regex.Match(rawName, @"(-{1,2}[\w-]+)");
            return match.Success ? match.Groups[1].Value : rawName.Trim();
        }

        // 用于 ParseSectionParams 的引用参数（不需要外部传入）
        private int _paramDictRef = 0;

        /// <summary>
        /// 推断参数类型
        /// </summary>
        /// <param name="param">参数项</param>
        private void InferParamType(SystemParamItem param)
        {
            var defaultValue = param.DefaultValue.ToLower();
            var description = param.Description.ToLower();
            var paramName = param.Name.ToLower();

            // 布尔类型判断：必须明确是开关参数
            // 通过参数名和描述判断：如果参数不需要值（纯开关），才是布尔
            if (IsBooleanParam(param.Name, param.Description, param.DefaultValue))
            {
                param.ValueType = ParamValueType.Boolean;
                param.DefaultValue = "false";
                param.CurrentValue = "false";
                return;
            }

            // 文件路径类型
            if (param.Name.Contains("model") || 
                param.Name.Contains("file") || 
                param.Name.Contains("path") ||
                paramName.Contains("triattention"))
            {
                param.ValueType = ParamValueType.FilePath;
                return;
            }

            // 枚举类型（特定参数有固定的可选值）
            if (paramName.Contains("cache-type") || paramName.Contains("log-format") || paramName.Contains("reasoning-format"))
            {
                param.ValueType = ParamValueType.Enum;
                // 为枚举类型设置选项
                if (paramName.Contains("cache-type"))
                {
                    param.Options = new List<string> { "f16", "q8_0", "q4_0", "tbq3", "tbq4", "tbqp3", "tbqp4" };
                    if (string.IsNullOrEmpty(param.DefaultValue) || !param.Options.Contains(param.DefaultValue))
                        param.DefaultValue = "f16";
                    param.CurrentValue = param.DefaultValue;
                }
                else if (paramName.Contains("log-format"))
                {
                    param.Options = new List<string> { "text", "json" };
                    param.DefaultValue = "text";
                    param.CurrentValue = "text";
                }
                else if (paramName.Contains("reasoning-format"))
                {
                    param.Options = new List<string> { "auto", "deepthink", "none" };
                    param.DefaultValue = "auto";
                    param.CurrentValue = "auto";
                }
                return;
            }

            // 尝试解析为数字
            if (double.TryParse(param.DefaultValue, out double numValue))
            {
                if (numValue == Math.Floor(numValue))
                {
                    param.ValueType = ParamValueType.Integer;
                    param.MinValue = 0;
                    param.MaxValue = numValue * 10 > 0 ? numValue * 10 : 999999;
                    param.Step = 1;
                    // 整数参数如果没有默认值，设定合理默认值
                    if (string.IsNullOrEmpty(param.CurrentValue) || param.CurrentValue == "")
                    {
                        // 根据参数名推断合理默认值
                        if (paramName.Contains("n-predict") || paramName.Contains("n-predict"))
                        {
                            param.DefaultValue = "-1";
                            param.CurrentValue = "-1";
                            param.MinValue = -1;
                            param.MaxValue = 32768;
                        }
                    }
                }
                else
                {
                    param.ValueType = ParamValueType.Float;
                    param.MinValue = 0;
                    param.MaxValue = Math.Max(numValue * 2, 1);
                    param.Step = 0.1;
                }
                return;
            }

            // 字符串类型（如 --stop, --prompt 等）
            // 对于没有默认值的非布尔参数，默认为字符串类型
            param.ValueType = ParamValueType.String;
            if (string.IsNullOrEmpty(param.CurrentValue))
            {
                param.CurrentValue = "";
            }
        }

        /// <summary>
        /// 为已知参数名设置合理的默认值和类型推断
        /// Markdown 文档中某些表格没有"默认值"列，需要硬编码已知参数的实际默认值
        /// </summary>
        private void ApplyKnownDefaults(SystemParamItem param)
        {
            switch (param.Name)
            {
                case "--threads":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "0";  // 0 = 自动检测
                    param.CurrentValue = "0";
                    param.MinValue = 0;
                    param.MaxValue = 256;
                    param.Step = 1;
                    break;
                case "--n-gpu-layers":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "0";
                    param.CurrentValue = "0";
                    param.MinValue = 0;
                    param.MaxValue = 999;
                    param.Step = 1;
                    break;
                case "--main-gpu":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "0";
                    param.CurrentValue = "0";
                    param.MinValue = 0;
                    param.MaxValue = 16;
                    param.Step = 1;
                    break;
                case "--tensor-split":
                    param.ValueType = ParamValueType.Float;
                    param.DefaultValue = "";
                    param.CurrentValue = "";
                    param.MinValue = 0;
                    param.MaxValue = 1;
                    param.Step = 0.1;
                    break;
                case "--ctx-size":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "4096";
                    param.CurrentValue = "4096";
                    param.MinValue = 512;
                    param.MaxValue = 5242880;
                    param.Step = 512;
                    break;
                case "--keep":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "0";
                    param.CurrentValue = "0";
                    param.MinValue = 0;
                    param.MaxValue = 65536;
                    param.Step = 1;
                    break;
                case "--chunks":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "1";
                    param.CurrentValue = "1";
                    param.MinValue = 1;
                    param.MaxValue = 16;
                    param.Step = 1;
                    break;
                case "--temp":
                    param.ValueType = ParamValueType.Float;
                    param.DefaultValue = "0.8";
                    param.CurrentValue = "0.8";
                    param.MinValue = 0;
                    param.MaxValue = 2;
                    param.Step = 0.1;
                    break;
                case "--top-k":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "40";
                    param.CurrentValue = "40";
                    param.MinValue = 0;
                    param.MaxValue = 100;
                    param.Step = 1;
                    break;
                case "--top-p":
                    param.ValueType = ParamValueType.Float;
                    param.DefaultValue = "0.95";
                    param.CurrentValue = "0.95";
                    param.MinValue = 0;
                    param.MaxValue = 1;
                    param.Step = 0.05;
                    break;
                case "--repeat-penalty":
                    param.ValueType = ParamValueType.Float;
                    param.DefaultValue = "1.1";
                    param.CurrentValue = "1.1";
                    param.MinValue = 0.5;
                    param.MaxValue = 2;
                    param.Step = 0.1;
                    break;
                case "--n-predict":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "-1";
                    param.CurrentValue = "-1";
                    param.MinValue = -1;
                    param.MaxValue = 32768;
                    param.Step = 1;
                    break;
                case "--stop":
                    param.ValueType = ParamValueType.String;
                    param.DefaultValue = "";
                    param.CurrentValue = "";
                    break;
                case "--triattention":
                    param.ValueType = ParamValueType.FilePath;
                    param.DefaultValue = "";
                    param.CurrentValue = "";
                    break;
                case "--tri-budget":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "0";
                    param.CurrentValue = "0";
                    param.MinValue = 0;
                    param.MaxValue = 1024;
                    param.Step = 1;
                    break;
                case "--batch-size":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "512";
                    param.CurrentValue = "512";
                    param.MinValue = 1;
                    param.MaxValue = 4096;
                    param.Step = 1;
                    break;
                case "--ubatch-size":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "512";
                    param.CurrentValue = "512";
                    param.MinValue = 1;
                    param.MaxValue = 4096;
                    param.Step = 1;
                    break;
                case "--parallel":
                    param.ValueType = ParamValueType.Integer;
                    param.DefaultValue = "1";
                    param.CurrentValue = "1";
                    param.MinValue = 1;
                    param.MaxValue = 64;
                    param.Step = 1;
                    break;
                case "--min-p":
                    param.ValueType = ParamValueType.Float;
                    param.DefaultValue = "0.0";
                    param.CurrentValue = "0.0";
                    param.MinValue = 0;
                    param.MaxValue = 1;
                    param.Step = 0.01;
                    break;
                case "--presence-penalty":
                    param.ValueType = ParamValueType.Float;
                    param.DefaultValue = "0.0";
                    param.CurrentValue = "0.0";
                    param.MinValue = 0;
                    param.MaxValue = 2;
                    param.Step = 0.1;
                    break;
            }
        }

        /// <summary>
        /// 判断参数是否为布尔（开关）类型
        /// 规则：参数名以大写占位符结尾（如 --threads N）的不是布尔
        /// </summary>
        private bool IsBooleanParam(string rawParamName, string description, string defaultValue)
        {
            var name = rawParamName.ToLower();
            var desc = description.ToLower();

            // 检查原始参数名是否带值占位符（如 --threads N, --temp N, --n-gpu-layers N）
            // 带占位符的一定不是布尔
            if (Regex.IsMatch(rawParamName, @"-[a-z]\s+[A-Z]", RegexOptions.IgnoreCase))
                return false;

            // 明确的布尔开关参数列表
            var booleanParams = new HashSet<string>
            {
                "--mmap", "--no-mmap", "--mlock", "--low-vram",
                "--verbose", "--log-disable", "--flash-attn",
                "--cont-batching", "--echo", "--perplexity",
                "--jinja",
                "--no-kv-offload", "--no-affinity", "--no-warmup",
                "--simple-io", "--lora-without-llama", "--use-mlock",
                "--no-seed", "--conversation", "--special"
            };

            if (booleanParams.Contains(name))
                return true;

            // 通过默认值判断：如果默认值明确是布尔型
            if (defaultValue == "true" || defaultValue == "false" || 
                defaultValue == "开启" || defaultValue == "关闭")
                return true;

            // 如果默认值为空，且参数名不带占位符，且描述中包含"启用"/"禁用"
            if (string.IsNullOrEmpty(defaultValue) && 
                (desc.Contains("启用") || desc.Contains("禁用")))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 创建默认参数配置
        /// 与 system.json 保持完全一致（由 llama.cpp启动参数.md 解析生成）
        /// </summary>
        private void CreateDefaultParams()
        {
            Categories = new List<ParamCategory>
            {
                // 一、核心基础参数 (SortOrder=1) ──────────────────────────────
                new ParamCategory
                {
                    Name = "一、核心基础参数",
                    DisplayName = "一、核心基础参数",
                    Icon = "🔧",
                    SortOrder = 1,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                         new SystemParamItem { Name = "--ctx-size", ShortName = "-c", DisplayName = "上下文窗口大小", Description = "设置上下文窗口大小（token数）（建议不超过模型支持的最大值（如8192、32768等））", ValueType = ParamValueType.Integer, DefaultValue = "4096", CurrentValue = "4096", MinValue = 512,    MaxValue = 5242880, Step = 1024, Category = "一、核心基础参数", SortOrder = 11 },
                          
                        new SystemParamItem { Name = "--threads",    ShortName = "-t",  DisplayName = "CPU线程数",      Description = "设置推理使用的CPU线程数",                             ValueType = ParamValueType.Integer, DefaultValue = "4",  CurrentValue = "4",  MinValue = 1,    MaxValue = 256,  Step = 1,   Category = "一、核心基础参数", SortOrder = 10 },
                           
                        new SystemParamItem { Name = "--flash-attn",    DisplayName = "Flash Attn", Description = "启用 Flash Attention（强烈建议开启，TurboQuant 所有优化基于 FA）",       ValueType = ParamValueType.Boolean, DefaultValue = "auto", CurrentValue = "auto", Category = "一、核心基础参数", SortOrder = 9 },
                          
                        new SystemParamItem { Name = "--no-mmap",       DisplayName = "禁用内存映射",   Description = "禁用内存映射（内存充足时建议开启，避免磁盘 I/O 瓶颈）",              ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "一、核心基础参数", SortOrder = 8 },

                        new SystemParamItem { Name = "--mmap",                         DisplayName = "内存映射加载",   Description = "使用内存映射加载模型，减少内存占用",                 ValueType = ParamValueType.Boolean,  DefaultValue = "false", CurrentValue = "false", Category = "一、核心基础参数", SortOrder = 7 },
                        
                        new SystemParamItem { Name = "--parallel",                     DisplayName = "并行请求序列数",     Description = "设置并行推理的序列数量（配合 --cont-batching 连续批处理 使用）",ValueType = ParamValueType.Integer, DefaultValue = "1",   CurrentValue = "1",   MinValue = 1,    MaxValue = 64,   Step = 1,   Category = "一、核心基础参数", SortOrder = 6 },

                        new SystemParamItem { Name = "--prompt",      ShortName = "-p",  DisplayName = "提示文本",       Description = "设置初始提示文本",                                   ValueType = ParamValueType.String,   Category = "一、核心基础参数", SortOrder = 5},
                       
                        new SystemParamItem { Name = "--batch-size", ShortName = "-b",  DisplayName = "批处理大小",     Description = "设置批处理大小，影响提示处理速度",                   ValueType = ParamValueType.Integer, DefaultValue = "512", CurrentValue = "512", MinValue = 1,    MaxValue = 4096, Step = 1,   Category = "一、核心基础参数", SortOrder = 4 },
                        
                        new SystemParamItem { Name = "--ubatch-size",ShortName = "-ub", DisplayName = "微批处理大小",   Description = "设置微批处理大小（ubatch），用于细粒度批处理调度",   ValueType = ParamValueType.Integer, DefaultValue = "512", CurrentValue = "512", MinValue = 1,    MaxValue = 4096, Step = 1,   Category = "一、核心基础参数", SortOrder = 3 },

                        new SystemParamItem { Name = "--jinja",         DisplayName = "Jinja模板引擎",  Description = "启用 Jinja2 聊天模板引擎（支持复杂对话模板格式）",                    ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "一、核心基础参数", SortOrder = 2},

                        new SystemParamItem { Name = "--cont-batching", DisplayName = "连续批处理",     Description = "连续批处理（与 TurboQuant 兼容，提升吞吐）",                           ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "一、核心基础参数", SortOrder = 1 },

                        new SystemParamItem { Name = "--n-gpu-layers", ShortName = "-ngl", DisplayName = "GPU卸载层数",   Description = "指定卸载到GPU的模型层数（设置为999可尝试卸载所有层）", ValueType = ParamValueType.Integer, DefaultValue = "0", CurrentValue = "0", MinValue = 0, MaxValue = 999, Step = 1, Category = "一、核心基础参数", SortOrder = 8 },

                        new SystemParamItem
                        {
                            Name = "--split-mode", DisplayName = "多卡拆分模式",
                            Description = "none=单卡; layer(默认)=按层切分流水线并行; row=按行切分权重; tensor(实验性)=张量并行",
                            ValueType = ParamValueType.Enum,
                            DefaultValue = "layer", CurrentValue = "layer",
                            Options = new List<string> { "none", "layer", "row", "tensor" },
                            Category = "一、核心基础参数", SortOrder = 7
                        },

                        new SystemParamItem { Name = "--reasoning",               DisplayName = "关闭思考模式",  Description = "关闭思考/推理token输出（启用后输出 --reasoning off）",    ValueType = ParamValueType.Boolean, DefaultValue = "off",   CurrentValue = "off", Category = "一、核心基础参数", SortOrder = 6 },
                        new SystemParamItem { Name = "--no-mmproj", DisplayName = "禁用多模态",     Description = "关闭多模态投影功能（不使用 --mmproj 多模态模型时启用）",            ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "一、核心基础参数", SortOrder = 6 },

                        // ── v2.4.0+ CPU 调度与亲和性参数 (b9596) ──────────────
                        new SystemParamItem { Name = "--threads-batch", ShortName = "-tb", DisplayName = "批处理线程数", Description = "批处理/prompt 阶段使用的线程数（默认同 --threads）", ValueType = ParamValueType.Integer, DefaultValue = "-1", CurrentValue = "-1", MinValue = -1, MaxValue = 256, Step = 1, Category = "一、核心基础参数", SortOrder = 22 },
                        new SystemParamItem { Name = "--cpu-mask", ShortName = "-C", DisplayName = "CPU 亲和性掩码", Description = "CPU 亲和性掩码（16 进制字符串），与 --cpu-range 互补", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "一、核心基础参数", SortOrder = 23 },
                        new SystemParamItem { Name = "--cpu-range", ShortName = "-Cr", DisplayName = "CPU 亲和性范围", Description = "CPU 亲和性范围 lo-hi（如 0-7 表示使用 0~7 号核心）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "一、核心基础参数", SortOrder = 24 },
                        new SystemParamItem { Name = "--cpu-strict", DisplayName = "严格 CPU 绑定", Description = "使用严格的 CPU 绑定（0=宽松，1=严格）", ValueType = ParamValueType.Integer, DefaultValue = "0", CurrentValue = "0", MinValue = 0, MaxValue = 1, Step = 1, Category = "一、核心基础参数", SortOrder = 25 },
                        new SystemParamItem { Name = "--prio", DisplayName = "进程优先级", Description = "进程优先级：-1=低，0=正常，1=中，2=高，3=实时", ValueType = ParamValueType.Integer, DefaultValue = "0", CurrentValue = "0", MinValue = -1, MaxValue = 3, Step = 1, Category = "一、核心基础参数", SortOrder = 26 },
                        new SystemParamItem { Name = "--poll", DisplayName = "轮询级别", Description = "轮询等待工作的级别（0=不轮询，0~100，默认 50）", ValueType = ParamValueType.Integer, DefaultValue = "50", CurrentValue = "50", MinValue = 0, MaxValue = 100, Step = 1, Category = "一、核心基础参数", SortOrder = 27 },
                        new SystemParamItem { Name = "--numa", DisplayName = "NUMA 优化", Description = "NUMA 系统优化策略：distribute=均匀分布，isolate=仅本地节点，numactl=按 numactl", ValueType = ParamValueType.Enum, DefaultValue = "distribute", CurrentValue = "distribute", Options = new List<string> { "distribute", "isolate", "numactl" }, Category = "一、核心基础参数", SortOrder = 28 },
                        new SystemParamItem { Name = "--warmup", DisplayName = "空跑预热", Description = "启动后先空跑一次预热（默认启用）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "一、核心基础参数", SortOrder = 29 },

                    }
                },

                // ── 二、KV 缓存量化类型 (SortOrder=8) ────────────────────────────
                new ParamCategory
                {
                    Name = "二、KV 缓存量化类型",
                    DisplayName = "二、KV 缓存量化类型",
                    Icon = "⚙️",
                    SortOrder = 2,
                    IsExpanded = true,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--cache-type-k", ShortName = "-ctk", DisplayName = "K缓存压缩类型",   Description = "K 缓存压缩类型（1.0x ~ 5.2x）",    ValueType = ParamValueType.Enum, DefaultValue = "q4_0", CurrentValue = "q4_0", Options = new List<string> { "f16", "q8_0", "q4_0", "q5_1", "q5_0", "q4_1", "iq4_nl" }, Category = "1. KV 缓存量化类型", SortOrder = 1 },
                        new SystemParamItem { Name = "--cache-type-v", ShortName = "-ctv", DisplayName = "V缓存压缩类型",   Description = "V 缓存压缩类型（1.0x ~ 5.2x）",    ValueType = ParamValueType.Enum, DefaultValue = "q4_0", CurrentValue = "q4_0", Options = new List<string> { "f16", "q8_0", "q4_0", "q5_1", "q5_0", "q4_1", "iq4_nl" }, Category = "1. KV 缓存量化类型", SortOrder = 2 },
                        // ── v2.4.0+ 新增 KV 缓存控制 (b9596) ──────────────
                        new SystemParamItem { Name = "--swa-full", DisplayName = "全尺寸 SWA 缓存", Description = "使用全尺寸 Sliding Window Attention 缓存（默认 false）", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "1. KV 缓存量化类型", SortOrder = 3 },
                        new SystemParamItem { Name = "--ctx-checkpoints", ShortName = "-ctxcp", DisplayName = "上下文检查点数", Description = "每个 slot 最大上下文检查点数量（默认 32，0=禁用）", ValueType = ParamValueType.Integer, DefaultValue = "32", CurrentValue = "32", MinValue = 0, MaxValue = 1024, Step = 1, Category = "1. KV 缓存量化类型", SortOrder = 4 },
                        new SystemParamItem { Name = "--cache-ram", ShortName = "-cram", DisplayName = "RAM 缓存上限(MiB)", Description = "设置 RAM 缓存上限 MiB（-1=无限制，0=禁用，默认 8192）", ValueType = ParamValueType.Integer, DefaultValue = "8192", CurrentValue = "8192", MinValue = -1, MaxValue = 1048576, Step = 256, Category = "1. KV 缓存量化类型", SortOrder = 5 },
                        new SystemParamItem { Name = "--kv-offload", ShortName = "-kvo", DisplayName = "KV 缓存卸载到 GPU", Description = "是否启用 KV 缓存到 GPU 的卸载（默认启用）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "1. KV 缓存量化类型", SortOrder = 6 },
                        new SystemParamItem { Name = "--repack", DisplayName = "权重重新打包", Description = "是否启用权重重打包以提升内存效率（默认启用）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "1. KV 缓存量化类型", SortOrder = 7 },
                        new SystemParamItem { Name = "--no-host", DisplayName = "绕过 host 缓冲", Description = "绕过 host buffer，允许使用更多 device buffer", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "1. KV 缓存量化类型", SortOrder = 8 },
                        new SystemParamItem { Name = "--direct-io", ShortName = "-dio", DisplayName = "DirectIO 加载", Description = "启用 DirectIO 加载模型文件（若可用）", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "1. KV 缓存量化类型", SortOrder = 9 },
                    }
                },

                // ── 三、TriAttention 参数 (SortOrder=9) ─────────────────────────
                new ParamCategory
                {
                    Name = "三、TriAttention 参数（v1.7.0+）",
                    DisplayName = "三、 TriAttention 参数（v1.7.0+）",
                    Icon = "⚙️",
                    SortOrder = 3,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--tri-budget",     DisplayName = "Top-B保留数",     Description = "每层触发后保留的 Top-B slot 数量",                            ValueType = ParamValueType.Integer,  DefaultValue = "0",   CurrentValue = "0",   MinValue = 0,    MaxValue = 1024, Step = 1,   Category = "三、TriAttention 参数（v1.7.0+）", SortOrder = 2 },
                        new SystemParamItem { Name = "--tri-interval",   DisplayName = "裁剪触发间隔",   Description = "每 N 个解码 token 触发一次评分和裁剪（论文 β）",              ValueType = ParamValueType.Integer,  DefaultValue = "128", CurrentValue = "128", MinValue = 0,    MaxValue = 1280, Step = 1,   Category = "三、TriAttention 参数（v1.7.0+）", SortOrder = 3 },
                        new SystemParamItem { Name = "--tri-keep-first", DisplayName = "AttentionSink数", Description = "Attention Sink — 始终保留前 N 个 slot",                     ValueType = ParamValueType.Integer,  DefaultValue = "4",   CurrentValue = "4",   MinValue = 0,    MaxValue = 40,   Step = 1,   Category = "三、TriAttention 参数（v1.7.0+）", SortOrder = 4 }
                    }
                },

                // ── 四. 核心解码参数 (SortOrder=4) ────────────────────────────────
                new ParamCategory
                {
                    Name = "四、核心解码参数",
                    DisplayName = "四、核心解码参数",
                    Icon = "🔧",
                    SortOrder = 4,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--temp",              DisplayName = "温度系数",       Description = "控制输出随机性，值越高越有创意（0.1-1.2（默认0.8））",       ValueType = ParamValueType.Float,   DefaultValue = "0.8",  CurrentValue = "0.8",  MinValue = 0.0,   MaxValue = 2.0,    Step = 0.1,  Category = "四、核心解码参数", SortOrder = 1 },
                        new SystemParamItem { Name = "--top-k",             DisplayName = "Top-K采样",      Description = "只考虑概率最高的N个token（40（默认），设置为0禁用）",       ValueType = ParamValueType.Integer, DefaultValue = "40",   CurrentValue = "40",   MinValue = 0,     MaxValue = 100,   Step = 1,    Category = "四、核心解码参数", SortOrder = 2 },
                        new SystemParamItem { Name = "--top-p",             DisplayName = "Top-P核采样",    Description = "核采样，累积概率超过N即停止（0.95（默认），设置为1.0禁用）", ValueType = ParamValueType.Float,   DefaultValue = "0.95", CurrentValue = "0.95", MinValue = 0.0,   MaxValue = 1.0,    Step = 0.05, Category = "四、核心解码参数", SortOrder = 3 },
                        new SystemParamItem { Name = "--min-p",             DisplayName = "Min-P最小概率",  Description = "最小概率阈值，过滤概率低于此值的token（0.0（默认，禁用））", ValueType = ParamValueType.Float,   DefaultValue = "0.0",  CurrentValue = "0.0",  MinValue = 0.0,   MaxValue = 1.0,    Step = 0.01, Category = "四、核心解码参数", SortOrder = 4 },
                        new SystemParamItem { Name = "--repeat-penalty",    DisplayName = "重复惩罚系数",   Description = "重复惩罚系数，值越高越不容易重复（1.1（默认））",           ValueType = ParamValueType.Float,   DefaultValue = "1.1",  CurrentValue = "1.1",  MinValue = 0.5,   MaxValue = 2.0,    Step = 0.1,  Category = "四、核心解码参数", SortOrder = 5 },
                        new SystemParamItem { Name = "--presence-penalty",  DisplayName = "存在惩罚系数",   Description = "存在惩罚系数，已出现token的概率衰减（0.0（默认，禁用））", ValueType = ParamValueType.Float,   DefaultValue = "0.0",  CurrentValue = "0.0",  MinValue = 0.0,   MaxValue = 2.0,    Step = 0.1,  Category = "四、核心解码参数", SortOrder = 6 },
                        new SystemParamItem { Name = "--n-predict",         ShortName = "-n", DisplayName = "最大生成token数", Description = "设置最大生成token数",                                      ValueType = ParamValueType.Integer, DefaultValue = "-1",   CurrentValue = "-1",   MinValue = -1,    MaxValue = 32768, Step = 1,    Category = "四、核心解码参数", SortOrder = 7 },
                        new SystemParamItem { Name = "--reasoning-format",  DisplayName = "推理格式",       Description = "控制推理/思维链输出格式（auto/deepthink/none）",           ValueType = ParamValueType.Enum,    DefaultValue = "auto", CurrentValue = "auto", Options = new List<string> { "auto", "deepthink", "none" }, Category = "四、核心解码参数", SortOrder = 8 },
                        new SystemParamItem { Name = "--stop",                              DisplayName = "停止序列",       Description = "设置停止序列，遇到即停止生成",                              ValueType = ParamValueType.String,  DefaultValue = "",     CurrentValue = "",     Category = "四、核心解码参数", SortOrder = 9 },
                        new SystemParamItem { Name = "--echo",                              DisplayName = "回显提示文本",   Description = "在输出中包含输入的提示文本",                              ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "四、核心解码参数", SortOrder = 10 },


                        // ── MTP 推测解码相关参数 ──────────────────────────────
                        new SystemParamItem
                        {
                            Name = "--spec-type", DisplayName = "mtp-推测解码类型",
                            Description = "推测解码类型（逗号分隔支持多类型，none=禁用; draft-mtp=推荐）",
                            ValueType = ParamValueType.Enum,
                            DefaultValue = "draft-simple,draft-mtp", CurrentValue = "draft-simple,draft-mtp",
                            Options = new List<string> { "none", "draft-simple", "draft-eagle3", "draft-mtp", "ngram-simple", "ngram-map-k", "ngram-map-k4v", "ngram-mod", "ngram-cache" },
                            Category = "四、核心解码参数", SortOrder = 12
                        },
                        new SystemParamItem { Name = "--spec-draft-n-max",          DisplayName = "mtp-最大Draft Token数", Description = "推测解码每步预测的最大候选token数量",                  ValueType = ParamValueType.Integer, DefaultValue = "2",   CurrentValue = "2",   MinValue = 1,     MaxValue = 16,    Step = 1,   Category = "四、核心解码参数", SortOrder = 11 },
                        new SystemParamItem { Name = "--spec-draft-n-min",          DisplayName = "mtp-最小Draft Token数", Description = "推测解码每步预测的最小候选token数量（0=不限制）",          ValueType = ParamValueType.Integer, DefaultValue = "0",   CurrentValue = "0",   MinValue = 0,     MaxValue = 16,    Step = 1,   Category = "四、核心解码参数", SortOrder = 11 },
                        new SystemParamItem { Name = "--spec-draft-p-min", ShortName = "--draft-p-min",   DisplayName = "mtp-接受概率阈值",     Description = "推测解码接受候选token的最低概率阈值（0.90=默认）",          ValueType = ParamValueType.Float,   DefaultValue = "0.90",CurrentValue = "0.90",MinValue = 0.0,   MaxValue = 1.0,    Step = 0.05,Category = "四、核心解码参数", SortOrder = 11 },
                        new SystemParamItem { Name = "--spec-draft-p-split",ShortName = "--draft-p-split", DisplayName = "mtp-分割概率阈值",     Description = "推测解码分割概率，控制推测与验证的切换点（0.50=默认）",      ValueType = ParamValueType.Float,   DefaultValue = "0.50",CurrentValue = "0.50",MinValue = 0.0,   MaxValue = 1.0,    Step = 0.05,Category = "四、核心解码参数", SortOrder = 11 },

                        // ── v2.4.0+ 采样参数补充 (b9596) ──────────────────
                        new SystemParamItem { Name = "--repeat-last-n", DisplayName = "重复惩罚窗口", Description = "用于重复惩罚的最近 N 个 token（0=禁用，-1=ctx_size，默认 64）", ValueType = ParamValueType.Integer, DefaultValue = "64", CurrentValue = "64", MinValue = -1, MaxValue = 524288, Step = 1, Category = "四、核心解码参数", SortOrder = 20 },
                        new SystemParamItem { Name = "--frequency-penalty", DisplayName = "频率惩罚", Description = "频率惩罚系数，已出现 token 越多惩罚越大（0.0=禁用）", ValueType = ParamValueType.Float, DefaultValue = "0.0", CurrentValue = "0.0", MinValue = 0.0, MaxValue = 2.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 21 },
                        new SystemParamItem { Name = "--typical-p", DisplayName = "典型采样 P", Description = "局部典型采样（1.0=禁用，默认 1.0）", ValueType = ParamValueType.Float, DefaultValue = "1.0", CurrentValue = "1.0", MinValue = 0.0, MaxValue = 1.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 22 },
                        new SystemParamItem { Name = "--top-n-sigma", DisplayName = "Top-N-Sigma", Description = "Top-N-Sigma 采样（-1.0=禁用，0=动态）", ValueType = ParamValueType.Float, DefaultValue = "-1.0", CurrentValue = "-1.0", MinValue = -1.0, MaxValue = 5.0, Step = 0.1, Category = "四、核心解码参数", SortOrder = 23 },
                        new SystemParamItem { Name = "--xtc-probability", DisplayName = "XTC 概率", Description = "XTC 采样概率（0.0=禁用）", ValueType = ParamValueType.Float, DefaultValue = "0.0", CurrentValue = "0.0", MinValue = 0.0, MaxValue = 1.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 24 },
                        new SystemParamItem { Name = "--xtc-threshold", DisplayName = "XTC 阈值", Description = "XTC 采样阈值（1.0=禁用，默认 0.10）", ValueType = ParamValueType.Float, DefaultValue = "0.10", CurrentValue = "0.10", MinValue = 0.0, MaxValue = 1.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 25 },
                        new SystemParamItem { Name = "--dynatemp-range", DisplayName = "动态温度范围", Description = "动态温度范围（0.0=禁用）", ValueType = ParamValueType.Float, DefaultValue = "0.0", CurrentValue = "0.0", MinValue = 0.0, MaxValue = 5.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 26 },
                        new SystemParamItem { Name = "--dynatemp-exp", DisplayName = "动态温度指数", Description = "动态温度分布指数（默认 1.0）", ValueType = ParamValueType.Float, DefaultValue = "1.0", CurrentValue = "1.0", MinValue = 0.1, MaxValue = 5.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 27 },
                        new SystemParamItem { Name = "--adaptive-target", DisplayName = "Adaptive-P 目标概率", Description = "Adaptive-P 目标概率（0~1，负数=禁用）", ValueType = ParamValueType.Float, DefaultValue = "-1.0", CurrentValue = "-1.0", MinValue = -1.0, MaxValue = 1.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 28 },
                        new SystemParamItem { Name = "--adaptive-decay", DisplayName = "Adaptive-P 衰减率", Description = "Adaptive-P 衰减率（0~0.99，默认 0.90）", ValueType = ParamValueType.Float, DefaultValue = "0.90", CurrentValue = "0.90", MinValue = 0.0, MaxValue = 0.99, Step = 0.01, Category = "四、核心解码参数", SortOrder = 29 },
                        new SystemParamItem { Name = "--mirostat", DisplayName = "Mirostat 模式", Description = "Mirostat 采样（0=禁用，1=v1，2=v2）", ValueType = ParamValueType.Integer, DefaultValue = "0", CurrentValue = "0", MinValue = 0, MaxValue = 2, Step = 1, Category = "四、核心解码参数", SortOrder = 30 },
                        new SystemParamItem { Name = "--mirostat-lr", DisplayName = "Mirostat 学习率", Description = "Mirostat 学习率 eta（默认 0.10）", ValueType = ParamValueType.Float, DefaultValue = "0.10", CurrentValue = "0.10", MinValue = 0.0, MaxValue = 1.0, Step = 0.01, Category = "四、核心解码参数", SortOrder = 31 },
                        new SystemParamItem { Name = "--mirostat-ent", DisplayName = "Mirostat 目标熵", Description = "Mirostat 目标熵 tau（默认 5.0）", ValueType = ParamValueType.Float, DefaultValue = "5.0", CurrentValue = "5.0", MinValue = 0.0, MaxValue = 10.0, Step = 0.1, Category = "四、核心解码参数", SortOrder = 32 },
                        new SystemParamItem { Name = "--seed", ShortName = "-s", DisplayName = "随机种子", Description = "RNG 种子（默认 -1=随机）", ValueType = ParamValueType.Integer, DefaultValue = "-1", CurrentValue = "-1", MinValue = -1, MaxValue = 2147483647, Step = 1, Category = "四、核心解码参数", SortOrder = 33 },

                        // ── DRY 采样 (5 个) ────────────────────────────────────────
                        new SystemParamItem { Name = "--dry-multiplier", DisplayName = "DRY 倍率", Description = "DRY 采样倍率（0.0=禁用，默认 0.0）", ValueType = ParamValueType.Float, DefaultValue = "0.0", CurrentValue = "0.0", MinValue = 0.0, MaxValue = 5.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 40 },
                        new SystemParamItem { Name = "--dry-base", DisplayName = "DRY 基础值", Description = "DRY 采样基础值（默认 1.75）", ValueType = ParamValueType.Float, DefaultValue = "1.75", CurrentValue = "1.75", MinValue = 0.0, MaxValue = 10.0, Step = 0.05, Category = "四、核心解码参数", SortOrder = 41 },
                        new SystemParamItem { Name = "--dry-allowed-length", DisplayName = "DRY 允许长度", Description = "DRY 采样允许的重复长度（默认 2）", ValueType = ParamValueType.Integer, DefaultValue = "2", CurrentValue = "2", MinValue = 0, MaxValue = 32, Step = 1, Category = "四、核心解码参数", SortOrder = 42 },
                        new SystemParamItem { Name = "--dry-penalty-last-n", DisplayName = "DRY 惩罚窗口", Description = "DRY 惩罚最近 N 个 token（-1=上下文大小，0=禁用，默认 -1）", ValueType = ParamValueType.Integer, DefaultValue = "-1", CurrentValue = "-1", MinValue = -1, MaxValue = 524288, Step = 1, Category = "四、核心解码参数", SortOrder = 43 },
                        new SystemParamItem { Name = "--dry-sequence-breaker", DisplayName = "DRY 序列中断符", Description = "DRY 序列中断符（清除默认中断符，'none'=不使用中断符）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "四、核心解码参数", SortOrder = 44 },
                    }
                },

                


                // ── 五、系统资源与内存优化参数 (SortOrder=5) ───────────────────
                new ParamCategory
                {
                    Name = "五、系统资源与内存优化参数",
                    DisplayName = "五、系统资源与内存优化参数",
                    Icon = "💾",
                    SortOrder = 5,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--low-vram", DisplayName = "低VRAM模式",     Description = "启用低VRAM模式，减少GPU内存占用（GPU显存不足时）",              ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "五、系统资源与内存优化参数", SortOrder = 1 },

                        new SystemParamItem { Name = "--mlock",    DisplayName = "内存锁定防换页", Description = "将模型锁定在内存中，避免换页（服务器环境）",                   ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "五、系统资源与内存优化参数", SortOrder = 2 },

                        
                    }
                },


                // ── 六、上下文窗口管理参数 (SortOrder=6) ───────────────────────
                new ParamCategory
                {
                    Name = "六、上下文窗口管理参数",
                    DisplayName = "六、上下文窗口管理参数",
                    Icon = "📏",
                    SortOrder = 6,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {

                        new SystemParamItem { Name = "--keep",     ShortName = "-k", DisplayName = "保留初始token", Description = "保留的初始上下文token数，用于多轮对话（0（全部保留））",  ValueType = ParamValueType.Integer, DefaultValue = "0",   CurrentValue = "0",   MinValue = 0,     MaxValue = 65536,  Step = 1,   Category = "六、上下文窗口管理参数", SortOrder = 2 },
                        new SystemParamItem { Name = "--chunks",               DisplayName = "模型分块加载数", Description = "将模型分成N个chunk加载，降低峰值内存占用（1（不拆分））",     ValueType = ParamValueType.Integer, DefaultValue = "1",   CurrentValue = "1",   MinValue = 1,     MaxValue = 16,     Step = 1,   Category = "六、上下文窗口管理参数", SortOrder = 3 }
                    }
                },

                // ── 七、GPU加速与混合计算参数 (SortOrder=7) ─────────────────────
                new ParamCategory
                {
                    Name = "七、多GPU加速与混合计算参数",
                    DisplayName = "七、多GPU加速与混合计算参数",
                    Icon = "🎮",
                    SortOrder = 7,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {

                        new SystemParamItem { Name = "--main-gpu",                DisplayName = "主GPU编号",     Description = "指定主要使用的GPU设备编号（多GPU环境下有效）",        ValueType = ParamValueType.Integer, DefaultValue = "0", CurrentValue = "0", MinValue = 0, MaxValue = 16,  Step = 1, Category = "七、多GPU加速与混合计算参数", SortOrder = 1 },

                        new SystemParamItem { Name = "--tensor-split",           DisplayName = "GPU张量拆分比", Description = "多GPU之间的张量拆分比例（例如0.6,0.4表示60%在GPU0，40%在GPU1）", ValueType = ParamValueType.Float, DefaultValue = "", CurrentValue = "", MinValue = 0.0, MaxValue = 1.0, Step = 0.1, Category = "七、多GPU加速与混合计算参数", SortOrder = 2 },

                        // ── v2.4.0+ 设备调度与混合计算 (b9596) ──────────────
                        new SystemParamItem { Name = "--device", ShortName = "-dev", DisplayName = "offload 设备列表", Description = "逗号分隔的设备列表（CUDA0,CUDA1,Vulkan0,...；none=不卸载）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "七、多GPU加速与混合计算参数", SortOrder = 10 },
                        new SystemParamItem { Name = "--split-mode", ShortName = "-sm", DisplayName = "多卡拆分模式", Description = "none=单卡; layer=按层流水线; row=按行并行; tensor=张量并行(实验)", ValueType = ParamValueType.Enum, DefaultValue = "layer", CurrentValue = "layer", Options = new List<string> { "none", "layer", "row", "tensor" }, Category = "七、多GPU加速与混合计算参数", SortOrder = 11 },
                        new SystemParamItem { Name = "--fit", ShortName = "-fit", DisplayName = "自动适配显存", Description = "自动调整未设置参数以适应设备内存（on/off，默认 on）", ValueType = ParamValueType.Boolean, DefaultValue = "on", CurrentValue = "on", Category = "七、多GPU加速与混合计算参数", SortOrder = 12 },
                        new SystemParamItem { Name = "--fit-target", ShortName = "-fitt", DisplayName = "显存预留(MiB)", Description = "--fit 模式每个设备预留显存 MiB（单值广播，默认 1024）", ValueType = ParamValueType.String, DefaultValue = "1024", CurrentValue = "1024", Category = "七、多GPU加速与混合计算参数", SortOrder = 13 },
                        new SystemParamItem { Name = "--fit-ctx", ShortName = "-fitc", DisplayName = "最小 ctx (--fit)", Description = "--fit 可设置的最小 ctx size（默认 4096）", ValueType = ParamValueType.Integer, DefaultValue = "4096", CurrentValue = "4096", MinValue = 512, MaxValue = 524288, Step = 512, Category = "七、多GPU加速与混合计算参数", SortOrder = 14 },
                        new SystemParamItem { Name = "--cpu-moe", ShortName = "-cmoe", DisplayName = "CPU 保留 MoE", Description = "所有 MoE 专家权重保留在 CPU（适合显存紧张）", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "七、多GPU加速与混合计算参数", SortOrder = 15 },
                        new SystemParamItem { Name = "--n-cpu-moe", ShortName = "-ncmoe", DisplayName = "前 N 层 MoE 留 CPU", Description = "前 N 层 MoE 专家权重保留在 CPU", ValueType = ParamValueType.Integer, DefaultValue = "0", CurrentValue = "0", MinValue = 0, MaxValue = 999, Step = 1, Category = "七、多GPU加速与混合计算参数", SortOrder = 16 },
                        new SystemParamItem { Name = "--op-offload", DisplayName = "host op 卸载", Description = "将 host tensor 操作卸载到 device（默认 true）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "七、多GPU加速与混合计算参数", SortOrder = 17 },
                        new SystemParamItem { Name = "--override-tensor", ShortName = "-ot", DisplayName = "Tensor buffer 覆盖", Description = "按 tensor 名覆盖 buffer 类型（pattern=type,...，如 blk\\.0\\.ffn_\\.=Q8_0）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "七、多GPU加速与混合计算参数", SortOrder = 18 },
                        new SystemParamItem { Name = "--check-tensors", DisplayName = "tensor 数据校验", Description = "加载时检查 tensor 数据是否含 NaN/Inf", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "七、多GPU加速与混合计算参数", SortOrder = 19 },
                        new SystemParamItem { Name = "--override-kv", DisplayName = "元数据 KV 覆盖", Description = "按 KEY=TYPE:VALUE 覆盖模型元数据（可多次）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "七、多GPU加速与混合计算参数", SortOrder = 20 },
                    }
                },

                 // ── 八、调试与日志参数 (SortOrder=8) ────────────────────────────
                new ParamCategory
                {
                    Name = "八、调试与日志参数",
                    DisplayName = "八、调试与日志参数",
                    Icon = "🐛",
                    SortOrder = 8,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--verbose",      ShortName = "-v", DisplayName = "详细日志输出",     Description = "启用详细日志输出",                               ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "八、调试与日志参数", SortOrder = 1 },
                        new SystemParamItem { Name = "--log-disable",               DisplayName = "禁用日志输出",     Description = "禁用日志输出",                                       ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "八、调试与日志参数", SortOrder = 2 },
                        new SystemParamItem { Name = "--log-format",               DisplayName = "日志格式",         Description = "设置日志格式",                                       ValueType = ParamValueType.Enum,    DefaultValue = "text", CurrentValue = "text", Options = new List<string> { "text", "json" }, Category = "八、调试与日志参数", SortOrder = 3 },
                        new SystemParamItem { Name = "--perplexity",               DisplayName = "困惑度评估",       Description = "计算输入文本的困惑度，用于模型质量评估",               ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "八、调试与日志参数", SortOrder = 4 },

                        // ── v2.4.0+ 日志控制 (b9596) ─────────────────────
                        new SystemParamItem { Name = "--log-file", DisplayName = "日志输出文件", Description = "将日志写入指定文件路径", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "八、调试与日志参数", SortOrder = 10 },
                        new SystemParamItem { Name = "--log-colors", DisplayName = "彩色日志", Description = "彩色日志输出（on/off/auto）", ValueType = ParamValueType.Enum, DefaultValue = "auto", CurrentValue = "auto", Options = new List<string> { "on", "off", "auto" }, Category = "八、调试与日志参数", SortOrder = 11 },
                        new SystemParamItem { Name = "--log-verbosity", ShortName = "-lv", DisplayName = "日志级别阈值", Description = "日志阈值（0=generic, 1=error, 2=warning, 3=info, 4=trace, 5=debug）", ValueType = ParamValueType.Integer, DefaultValue = "1", CurrentValue = "1", MinValue = 0, MaxValue = 5, Step = 1, Category = "八、调试与日志参数", SortOrder = 12 },
                        new SystemParamItem { Name = "--log-prefix", DisplayName = "日志前缀", Description = "启用日志消息前缀（默认启用）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "八、调试与日志参数", SortOrder = 13 },
                        new SystemParamItem { Name = "--log-timestamps", DisplayName = "日志时间戳", Description = "启用日志消息时间戳（默认启用）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "八、调试与日志参数", SortOrder = 14 },
                        new SystemParamItem { Name = "--log-prompts-dir", DisplayName = "日志提示词目录", Description = "将 prompt 写入该目录（仅调试用）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "八、调试与日志参数", SortOrder = 15 },
                        new SystemParamItem { Name = "--offline", DisplayName = "离线模式", Description = "离线模式：强制使用缓存，禁止网络访问", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "八、调试与日志参数", SortOrder = 16 },
                    }
                },

                // ── 九、推理与聊天模板 (SortOrder=9，新分类 v2.4.0+) ────────────────
                new ParamCategory
                {
                    Name = "九、推理与聊天模板（v2.4.0+）",
                    DisplayName = "九、推理与聊天模板",
                    Icon = "🧠",
                    SortOrder = 9,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--reasoning-format", DisplayName = "推理格式", Description = "推理/思维链输出格式：none=不解析；deepseek=分离 reasoning；deepseek-legacy=保留 标签；auto=自动", ValueType = ParamValueType.Enum, DefaultValue = "auto", CurrentValue = "auto", Options = new List<string> { "none", "deepseek", "deepseek-legacy", "auto" }, Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 1 },
                        new SystemParamItem { Name = "--reasoning-budget", DisplayName = "推理 token 预算", Description = "推理 token 预算：-1=不限制，0=立即结束，N>0=token 数上限", ValueType = ParamValueType.Integer, DefaultValue = "-1", CurrentValue = "-1", MinValue = -1, MaxValue = 524288, Step = 1, Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 2 },
                        new SystemParamItem { Name = "--reasoning-budget-message", DisplayName = "推理预算耗尽提示", Description = "推理预算耗尽时注入到 end-of-thinking 标签前的提示消息", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 3 },
                        new SystemParamItem { Name = "--chat-template-kwargs", DisplayName = "模板 kwargs (JSON)", Description = "传给 jinja 模板解析器的额外参数（合法 JSON 字符串）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 4 },
                        new SystemParamItem { Name = "--chat-template", DisplayName = "自定义 Jinja 模板", Description = "自定义 jinja 聊天模板（内建名：llama3, chatml, gemma 等）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 5 },
                        new SystemParamItem { Name = "--chat-template-file", DisplayName = "模板文件路径", Description = "自定义 jinja 聊天模板文件路径", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 6 },
                        new SystemParamItem { Name = "--system-prompt", ShortName = "-sys", DisplayName = "系统提示词", Description = "使用的 system prompt（由 chat template 控制）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 7 },
                        new SystemParamItem { Name = "--system-prompt-file", ShortName = "-sysf", DisplayName = "系统提示词文件", Description = "包含 system prompt 的文件路径", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 8 },
                        new SystemParamItem { Name = "--reverse-prompt", ShortName = "-r", DisplayName = "反向停止词", Description = "在交互模式中遇到此 token 时停止生成", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 9 },
                        new SystemParamItem { Name = "--conversation", ShortName = "-cnv", DisplayName = "对话模式", Description = "以对话模式运行（禁用特殊 token，自动启用交互）", ValueType = ParamValueType.Boolean, DefaultValue = "true", CurrentValue = "true", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 10 },
                        new SystemParamItem { Name = "--single-turn", ShortName = "-st", DisplayName = "单轮对话", Description = "仅运行单轮对话后退出", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "九、推理与聊天模板（v2.4.0+）", SortOrder = 11 },
                    }
                },

                // ── 十、HuggingFace 一键加载（v2.4.0+）─────────────────────────────
                new ParamCategory
                {
                    Name = "十、HuggingFace 一键加载（v2.4.0+）",
                    DisplayName = "十、HF 一键加载",
                    Icon = "🤗",
                    SortOrder = 10,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--hf-repo", ShortName = "-hf", DisplayName = "HF 仓库", Description = "HuggingFace 仓库（user/model[:quant]），可自动下载 GGUF 与 mmproj", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 1 },
                        new SystemParamItem { Name = "--hf-file", ShortName = "-hff", DisplayName = "HF 文件名", Description = "指定 HF 仓库里的具体文件名（覆盖 --hf-repo 的 quant）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 2 },
                        new SystemParamItem { Name = "--hf-token", ShortName = "-hft", DisplayName = "HF Token", Description = "HuggingFace 访问令牌（也可来自环境变量 HF_TOKEN）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 3 },
                        new SystemParamItem { Name = "--hf-repo-v", ShortName = "-hfv", DisplayName = "HF 声码器仓库", Description = "TTS 声码器 HF 仓库", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 4 },
                        new SystemParamItem { Name = "--hf-file-v", ShortName = "-hffv", DisplayName = "HF 声码器文件", Description = "声码器模型文件名", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 5 },
                        new SystemParamItem { Name = "--model-url", ShortName = "-mu", DisplayName = "模型 URL", Description = "从指定 URL 下载模型", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 6 },
                        new SystemParamItem { Name = "--mmproj-url", ShortName = "-mmu", DisplayName = "mmproj URL", Description = "从指定 URL 下载多模态投影文件", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 7 },
                        new SystemParamItem { Name = "--docker-repo", ShortName = "-dr", DisplayName = "Docker Hub 仓库", Description = "Docker Hub 仓库（[repo/]model[:quant]，如 gemma3）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十、HuggingFace 一键加载（v2.4.0+）", SortOrder = 8 },
                    }
                },

                // ── 十一、特殊功能（v2.4.0+）────────────────────────────────────
                new ParamCategory
                {
                    Name = "十一、特殊功能（v2.4.0+）",
                    DisplayName = "十一、特殊功能",
                    Icon = "✨",
                    SortOrder = 11,
                    IsExpanded = false,
                    Params = new List<SystemParamItem>
                    {
                        new SystemParamItem { Name = "--context-shift", DisplayName = "无限生成时滑动上下文", Description = "无限文本生成时自动滑动上下文（默认禁用）", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 1 },
                        new SystemParamItem { Name = "--cont-batching", DisplayName = "连续批处理", Description = "允许生成过程中同时处理多个请求（并发场景）", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 2 },
                        new SystemParamItem { Name = "--parallel", ShortName = "-np", DisplayName = "并行解码槽位数", Description = "并行解码的序列数（服务端并发槽位）", ValueType = ParamValueType.Integer, DefaultValue = "1", CurrentValue = "1", MinValue = 1, MaxValue = 64, Step = 1, Category = "十一、特殊功能（v2.4.0+）", SortOrder = 3 },
                        new SystemParamItem { Name = "--rpc", DisplayName = "RPC 服务器列表", Description = "逗号分隔的 RPC 服务器列表（host:port）", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 4 },
                        new SystemParamItem { Name = "--lora", DisplayName = "LoRA 适配器", Description = "LoRA 适配器路径（逗号分隔加载多个）", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 5 },
                        new SystemParamItem { Name = "--control-vector", DisplayName = "控制向量", Description = "control vector 文件路径（逗号分隔多个）", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 6 },
                        new SystemParamItem { Name = "--grammar", DisplayName = "语法约束", Description = "BNF 语法约束生成", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 7 },
                        new SystemParamItem { Name = "--grammar-file", DisplayName = "语法约束文件", Description = "BNF 语法文件路径", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 8 },
                        new SystemParamItem { Name = "--json-schema", ShortName = "-j", DisplayName = "JSON Schema 约束", Description = "用 JSON Schema 约束生成", ValueType = ParamValueType.String, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 9 },
                        new SystemParamItem { Name = "--json-schema-file", ShortName = "-jf", DisplayName = "JSON Schema 文件", Description = "JSON Schema 文件路径", ValueType = ParamValueType.FilePath, DefaultValue = "", CurrentValue = "", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 10 },
                        new SystemParamItem { Name = "--backend-sampling", ShortName = "-bs", DisplayName = "后端采样", Description = "启用后端采样（实验性）", ValueType = ParamValueType.Boolean, DefaultValue = "false", CurrentValue = "false", Category = "十一、特殊功能（v2.4.0+）", SortOrder = 11 },
                    }
                },



            };

            SortCategories();
            BuildParamDict();
        }

        /// <summary>
        /// 按排序权重排序分类及其内部参数（数字越小越靠前）
        /// </summary>
        private void SortCategories()
        {
            Categories = Categories.OrderBy(c => c.SortOrder).ToList();
            foreach (var category in Categories)
            {
                category.Params = category.Params.OrderBy(p => p.SortOrder).ToList();
            }
        }

        /// <summary>
        /// 为缺少 SortOrder 的分类和参数补上权重
        /// 从 JSON 加载时旧数据可能没有 sortOrder 字段，默认值为 0
        /// </summary>
        private void EnsureSortOrders()
        {
            int categoryIndex = 1;
            foreach (var category in Categories)
            {
                if (category.SortOrder == 0)
                {
                    // 尝试根据分类名推断权重
                    int inferred = GetCategorySortOrder(category.Name);
                    category.SortOrder = inferred < 99 ? inferred : categoryIndex;
                }

                int paramIndex = 1;
                foreach (var param in category.Params)
                {
                    if (param.SortOrder == 0)
                    {
                        param.SortOrder = paramIndex;
                    }
                    paramIndex++;
                }
                categoryIndex++;
            }
        }

        /// <summary>
        /// 构建参数字典
        /// </summary>
        private void BuildParamDict()
        {
            ParamDict = new Dictionary<string, SystemParamItem>();
            foreach (var category in Categories)
            {
                foreach (var param in category.Params)
                {
                    ParamDict[param.Name] = param;
                    if (!string.IsNullOrEmpty(param.ShortName))
                    {
                        ParamDict[param.ShortName] = param;
                    }
                }
            }
        }

        /// <summary>
        /// 修正已知文件路径参数的类型（兼容旧 system.json 缓存）
        /// 旧版 system.json 中以下参数存为 String，新版应为 FilePath 以显示浏览按钮
        /// </summary>
        private void FixKnownParamTypes()
        {
            var filePathParamNames = new HashSet<string>
            {
                "--chat-template-file", "--system-prompt-file",
                "--log-file",
                "--lora", "--control-vector", "--grammar-file", "--json-schema-file"
            };

            foreach (var category in Categories)
            {
                foreach (var param in category.Params)
                {
                    if (filePathParamNames.Contains(param.Name) &&
                        param.ValueType != ParamValueType.FilePath)
                    {
                        param.ValueType = ParamValueType.FilePath;
                    }
                }
            }
        }

        /// <summary>
        /// 保存 system.json
        /// </summary>
        public void SaveSystemJson()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                string json = JsonSerializer.Serialize(Categories, options);
                File.WriteAllText(_systemJsonPath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存 system.json 失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// 更新参数值
        /// </summary>
        /// <param name="paramName">参数名称</param>
        /// <param name="value">参数值</param>
        /// <param name="enabled">是否启用</param>
        public void UpdateParam(string paramName, string value, bool enabled)
        {
            if (ParamDict.TryGetValue(paramName, out var param))
            {
                param.CurrentValue = value;
                param.Enabled = enabled;
                SaveSystemJson();
                OnParamsChanged?.Invoke();
            }
        }

        /// <summary>
        /// 获取启用的参数命令行字符串
        /// </summary>
        /// <returns>命令行参数字符串</returns>
        public string GetEnabledParamsCommandLine()
        {
            var args = new List<string>();
            foreach (var category in Categories)
            {
                foreach (var param in category.Params)
                {
                    if (param.Enabled)
                    {
                        string arg = param.GetCommandLineArg();
                        if (!string.IsNullOrEmpty(arg))
                        {
                            args.Add(arg);
                        }
                    }
                }
            }
            return string.Join(" ", args);
        }

        /// <summary>
        /// 获取分类图标
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <returns>图标字符</returns>
        private string GetCategoryIcon(string categoryName)
        {
            if (categoryName.Contains("核心")) return "🔧";
            if (categoryName.Contains("GPU") || categoryName.Contains("加速")) return "🎮";
            if (categoryName.Contains("上下文")) return "📏";
            if (categoryName.Contains("文本") || categoryName.Contains("生成")) return "📝";
            if (categoryName.Contains("资源") || categoryName.Contains("内存")) return "💾";
            if (categoryName.Contains("服务")) return "🌐";
            if (categoryName.Contains("调试") || categoryName.Contains("日志")) return "🐛";
            return "⚙️";
        }

        /// <summary>
        /// 获取分类排序权重（数字越小越靠前）
        /// </summary>
        /// <param name="categoryName">分类名称</param>
        /// <returns>排序权重</returns>
        private int GetCategorySortOrder(string categoryName)
        {
            // 子章节分类（### 开头的编号分类）排在对应主分类之后
            if (categoryName.StartsWith("1.") && categoryName.Contains("核心解码")) return 4;  // 四、文本生成 → 子章节
            if (categoryName.StartsWith("1.") && categoryName.Contains("KV")) return 8;        // 七、TurboQuant → 子章节
            if (categoryName.StartsWith("2.") && categoryName.Contains("TriAttention")) return 9;
            if (categoryName.StartsWith("3.") && categoryName.Contains("其他")) return 10;

            // 主分类
            if (categoryName.Contains("核心基础") || (categoryName.Contains("核心") && categoryName.Contains("一"))) return 1;
            if (categoryName.Contains("GPU") || categoryName.Contains("加速")) return 2;
            if (categoryName.Contains("上下文")) return 3;
            if (categoryName.Contains("文本") || categoryName.Contains("生成")) return 4;
            if (categoryName.Contains("资源") || categoryName.Contains("内存")) return 5;
            if (categoryName.Contains("服务")) return 6;
            if (categoryName.Contains("调试") || categoryName.Contains("日志")) return 7;
            if (categoryName.Contains("采样") || categoryName.Contains("量化")) return 8;
            if (categoryName.Contains("远程") || categoryName.Contains("RPC")) return 9;
            if (categoryName.Contains("TurboQuant") || categoryName.Contains("KV缓存")) return 8;
            return 99;
        }

        /// <summary>
        /// 根据参数名查找参数
        /// </summary>
        /// <param name="paramName">参数名（支持长名或短名）</param>
        /// <returns>参数项，未找到返回 null</returns>
        public SystemParamItem? FindParam(string paramName)
        {
            if (ParamDict.TryGetValue(paramName, out var param))
            {
                return param;
            }
            return null;
        }
    }
}
