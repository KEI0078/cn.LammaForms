using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace cn.LammaForms.Config
{
    /// <summary>
    /// 配置管理器 - 单例模式
    /// 负责 config.json 的加载、保存和验证
    /// </summary>
    public class ConfigManager
    {
        private static ConfigManager? _instance;
        private static readonly object _lock = new object();

        /// <summary>
        /// 配置文件路径
        /// </summary>
        private readonly string _configPath;

        /// <summary>
        /// 当前配置
        /// </summary>
        public AppConfig Config { get; private set; }

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event Action? OnConfigChanged;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        _instance ??= new ConfigManager();
                    }
                }
                return _instance;
            }
        }

        private ConfigManager()
        {
            _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            Config = new AppConfig();
            LoadConfig();
        }

        /// <summary>
        /// 加载配置文件
        /// </summary>
        public void LoadConfig()
        {
            try
            {
                if (File.Exists(_configPath))
                {
                    string json = File.ReadAllText(_configPath);
                    var config = JsonSerializer.Deserialize<AppConfig>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    });

                    if (config != null)
                    {
                        Config = config;
                    }
                }
                else
                {
                    // 配置文件不存在，创建默认配置
                    Config = new AppConfig();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载配置文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Config = new AppConfig();
            }
        }

        /// <summary>
        /// 保存配置文件
        /// </summary>
        public bool SaveConfig()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                string json = JsonSerializer.Serialize(Config, options);
                File.WriteAllText(_configPath, json);
                OnConfigChanged?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存配置文件失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        /// <summary>
        /// 验证 llama.cpp 路径是否有效
        /// </summary>
        /// <param name="path">llama.cpp 目录路径</param>
        /// <returns>验证结果信息</returns>
        public (bool IsValid, string Message) ValidateLlamaPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return (false, "路径不能为空");
            }

            if (!Directory.Exists(path))
            {
                return (false, "目录不存在");
            }

            var serverExe = Path.Combine(path, "llama-server.exe");
            var cliExe = Path.Combine(path, "llama-cli.exe");

            bool hasServer = File.Exists(serverExe);
            bool hasCli = File.Exists(cliExe);

            if (!hasServer && !hasCli)
            {
                return (false, "未找到 llama-server.exe 和 llama-cli.exe");
            }

            if (!hasServer)
            {
                return (true, "警告: 未找到 llama-server.exe");
            }

            if (!hasCli)
            {
                return (true, "警告: 未找到 llama-cli.exe");
            }

            return (true, "验证通过");
        }

        /// <summary>
        /// 验证模型文件是否存在
        /// </summary>
        /// <param name="modelPath">模型文件完整路径</param>
        /// <returns>是否存在</returns>
        public bool ValidateModelExists(string modelPath)
        {
            return !string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath);
        }

        /// <summary>
        /// 获取模型完整路径
        /// 支持：1) 绝对路径  2) 相对于 GgufModelsPath 的子目录路径  3) 仅文件名（根目录）
        /// </summary>
        /// <returns>模型完整路径</returns>
        public string GetModelFullPath()
        {
            if (string.IsNullOrWhiteSpace(Config.GgufModelsPath) || string.IsNullOrWhiteSpace(Config.SelectedModel))
            {
                return string.Empty;
            }

            // 如果已经是绝对路径且存在，直接返回
            if (Path.IsPathRooted(Config.SelectedModel) && File.Exists(Config.SelectedModel))
            {
                return Config.SelectedModel;
            }

            // 拼接 GgufModelsPath + SelectedModel（可能含子目录如 "subdir\model.gguf"）
            return Path.Combine(Config.GgufModelsPath, Config.SelectedModel);
        }

        /// <summary>
        /// 更新配置并自动保存
        /// </summary>
        /// <param name="updateAction">更新操作</param>
        public void UpdateAndSave(Action<AppConfig> updateAction)
        {
            updateAction(Config);
            SaveConfig();
        }
    }
}
