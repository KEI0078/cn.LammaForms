using System;
using System.Collections.Generic;

namespace cn.LammaForms.Config
{
    /// <summary>
    /// 参数值类型
    /// </summary>
    public enum ParamValueType
    {
        /// <summary>
        /// 布尔类型（开关，支持 1/0/on/off/-1 等枚举值）
        /// </summary>
        Boolean,

        /// <summary>
        /// 整数类型
        /// </summary>
        Integer,

        /// <summary>
        /// 浮点数类型
        /// </summary>
        Float,

        /// <summary>
        /// 字符串类型
        /// </summary>
        String,

        /// <summary>
        /// 枚举/选项类型
        /// </summary>
        Enum,

        /// <summary>
        /// 文件路径类型
        /// </summary>
        FilePath
    }

    /// <summary>
    /// 系统参数项定义
    /// </summary>
    public class SystemParamItem
    {
        /// <summary>
        /// 参数名称（如 --temp）
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 参数简写（如 -t）
        /// </summary>
        public string ShortName { get; set; } = string.Empty;

        /// <summary>
        /// 参数显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 参数说明,用于alt显示
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 参数值类型
        /// </summary>
        public ParamValueType ValueType { get; set; } = ParamValueType.String;

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; } = string.Empty;

        /// <summary>
        /// 最小值（数值类型有效）
        /// </summary>
        public double? MinValue { get; set; }

        /// <summary>
        /// 最大值（数值类型有效）
        /// </summary>
        public double? MaxValue { get; set; }

        /// <summary>
        /// 步长（数值类型有效）
        /// </summary>
        public double? Step { get; set; }

        /// <summary>
        /// 可选值列表（枚举类型有效）
        /// </summary>
        public List<string> Options { get; set; } = new List<string>();

        /// <summary>
        /// 是否启用
        /// </summary>
        public bool Enabled { get; set; } = false;

        /// <summary>
        /// 当前值
        /// </summary>
        public string CurrentValue { get; set; } = string.Empty;

        /// <summary>
        /// 参数分类
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 排序权重（数字越小越靠前）
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 是否只读（不可修改）
        /// </summary>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// 获取命令行参数格式
        /// Boolean 类型输出 --name value（value 可为 1/on/0/off/-1 等）
        /// 注意：不同布尔参数的输出规则不同（见 BooleanOutputStyle）
        /// </summary>
        /// <returns>命令行参数字符串</returns>
        public string GetCommandLineArg()
        {
            if (!Enabled)
                return string.Empty;

            if (ValueType == ParamValueType.Boolean)
            {
                var style = GetBooleanOutputStyle(Name);
                switch (style)
                {
                    case BooleanOutputStyle.FlashAttn:
                        // flash-attn 使用 on/off/auto（默认 auto）
                        var faVal = NormalizeFlashAttnValue(CurrentValue);
                        return $"{Name} {faVal}";
                    case BooleanOutputStyle.FlagOnly:
                        // 纯开关：--no-mmap, --jinja, --mmap 等，不需要值
                        return Name;
                    case BooleanOutputStyle.Default:
                    default:
                        // 带值的开关
                        var val = NormalizeBooleanValue(CurrentValue);
                        return $"{Name} {val}";
                }
            }

            if (string.IsNullOrEmpty(CurrentValue))
                return string.Empty;

            return $"{Name} {CurrentValue}";
        }

        #region 布尔参数输出样式枚举

        /// <summary>
        /// 布尔参数的命令行输出样式
        /// llama.cpp 不同布尔参数对值的要求不同：
        ///   - FlashAttn: 必须带值 on/off/auto（如 --flash-attn on）
        ///   - FlagOnly: 纯开关，不要值（如 --no-mmap, --jinja, --mmap）
        ///   - Default: 带值的开关（通用）
        /// </summary>
        public enum BooleanOutputStyle
        {
            Default,
            FlagOnly,
            FlashAttn
        }

        /// <summary>
        /// 获取指定布尔参数名的输出样式（公开方法，供 ParamControlFactory 使用）
        /// </summary>
        public static BooleanOutputStyle GetBooleanOutputStylePublic(string paramName) => GetBooleanOutputStyle(paramName);

        /// <summary>
        /// 获取指定布尔参数名的输出样式
        /// </summary>
        private static BooleanOutputStyle GetBooleanOutputStyle(string paramName)
        {
            var name = paramName.ToLower();
            return name switch
            {
                "--flash-attn" or "--fit" or "--reasoning"
                    => BooleanOutputStyle.FlashAttn,
                "--no-mmap" or "--jinja" or "--mmap"
                or "--mlock" or "--low-vram" or "--verbose" or "--log-disable"
                or "--cont-batching" or "--echo" or "--perplexity"
                or "--no-kv-offload" or "--no-warmup"
                or "--simple-io" or "--lora-without-llama" or "--use-mlock"
                or "--no-seed" or "--conversation" or "--special"
                or "--no-mmproj"
                // v2.4.0+ 新增纯开关 (b9596)
                or "--swa-full" or "--repack" or "--no-repack"
                or "--op-offload" or "--no-op-offload"
                or "--no-host" or "--direct-io" or "--no-direct-io"
                or "--spec-draft-backend-sampling" or "--no-spec-draft-backend-sampling"
                or "--offline" or "--no-display-prompt" or "--no-show-timings"
                or "--no-context-shift" or "--log-prefix" or "--no-log-prefix"
                or "--log-timestamps" or "--no-log-timestamps"
                or "--no-mmproj-auto" or "--no-mmproj-offload"
                or "--check-tensors" or "--skip-chat-parsing" or "--no-skip-chat-parsing"
                or "--ignore-eos" or "--perf" or "--no-perf"
                or "--backend-sampling" or "--no-jinja"
                or "--cpu-moe"
                or "--mmproj-auto" or "--mmproj-offload"
                or "--multiline-input"
                or "--display-prompt" or "--show-timings"
                or "--context-shift" or "--warmup"
                    => BooleanOutputStyle.FlagOnly,
                _ => BooleanOutputStyle.Default
            };
        }

        #endregion

        /// <summary>
        /// 将三态布尔值标准化为 llama.cpp 接受的格式：on / off / auto
        /// 用于 --flash-attn、--fit 等接受 on/off/auto 的参数
        /// </summary>
        private static string NormalizeFlashAttnValue(string? value)
        {
            var v = (value ?? "auto").ToLower().Trim();
            return v switch
            {
                "on" or "1" or "true" => "on",
                "off" or "0" or "false" => "off",
                "auto" => "auto",
                _ => "auto"   // 未识别时默认 auto
            };
        }

        /// <summary>
        /// 将布尔值标准化为 llama.cpp 接受的格式：1/on/0/off/-1
        /// </summary>
        private static string NormalizeBooleanValue(string? value)
        {
            var v = (value ?? "true").ToLower().Trim();
            switch (v)
            {
                case "1":
                case "on":
                case "true":
                    return "on";       // 默认开启
                case "-1":
                    return "-1";        // 强制模式（如 flash-attn -1）
                case "0":
                case "off":
                case "false":
                    return "off";
                default:
                    return "on";         // 未识别时默认开启
            }
        }

        /// <summary>
        /// 从命令行解析布尔参数值并设置 CurrentValue
        /// 支持: --flash-attn, --flash-attn 1, --flash-attn on, --flash-attn -1 等
        /// 注意：--no-mmap on / --jinja on 等纯开关参数，值部分会被忽略（llama.cpp 不识别 on）
        /// </summary>
        /// <param name="rawValue">从命令行解析出的原始值（空串表示无显式值）</param>
        public void ParseAndSetBooleanValue(string rawValue)
        {
            var style = GetBooleanOutputStyle(Name);

            // 纯开关参数：忽略值，只要出现就视为启用
            if (style == BooleanOutputStyle.FlagOnly)
            {
                CurrentValue = "true";   // 内部标记为启用
                Enabled = true;
                return;
            }

            // flash-attn 特殊处理：必须带值 on/off/auto
            if (style == BooleanOutputStyle.FlashAttn)
            {
                var v = rawValue.Trim().ToLower();
                CurrentValue = v switch
                {
                    "" or "1" or "on" or "true" => "on",
                    "0" or "off" or "false" => "off",
                    "auto" => "auto",
                    _ => "auto"  // 默认 auto
                };
                Enabled = true;
                return;
            }

            // 默认带值开关
            var val = rawValue.Trim().ToLower();
            switch (val)
            {
                case "":
                case "1":
                case "on":
                case "true":
                    CurrentValue = "on";
                    Enabled = true;
                    break;
                case "-1":
                    CurrentValue = "-1";
                    Enabled = true;
                    break;
                case "0":
                case "off":
                case "false":
                    CurrentValue = "off";
                    Enabled = true;
                    break;
                default:
                    CurrentValue = rawValue;
                    Enabled = true;
                    break;
            }
        }
    }

    /// <summary>
    /// 参数分类
    /// </summary>
    public class ParamCategory
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类显示名称
        /// </summary>
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>
        /// 分类描述
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// 排序权重（数字越小越靠前）
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// 分类图标（Unicode 字符）
        /// </summary>
        public string Icon { get; set; } = "⚙️";

        /// <summary>
        /// 参数列表
        /// </summary>
        public List<SystemParamItem> Params { get; set; } = new List<SystemParamItem>();

        /// <summary>
        /// 是否展开
        /// </summary>
        public bool IsExpanded { get; set; } = false;
    }
}
