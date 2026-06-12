using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace cn.LammaForms.Config
{
    /// <summary>
    /// 应用程序配置数据模型
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// llama.cpp 程序目录路径
        /// </summary>
        public string LlamaPath { get; set; } = string.Empty;

        /// <summary>
        /// GGUF 模型文件所在目录
        /// </summary>
        public string GgufModelsPath { get; set; } = string.Empty;

        /// <summary>
        /// 当前选中的模型文件名
        /// </summary>
        public string SelectedModel { get; set; } = string.Empty;

        /// <summary>
        /// 多模态投影模型文件路径
        /// </summary>
        public string MmprojFile { get; set; } = string.Empty;

        /// <summary>
        /// MTP（Multi-Token Prediction）模型文件路径
        /// </summary>
        public string MtpFile { get; set; } = string.Empty;

        /// <summary>
        /// 当前启用的启动参数配置
        /// Key: 参数名, Value: 参数值
        /// </summary>
        public Dictionary<string, string> ActiveParams { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// 服务器配置
        /// </summary>
        public ServerConfig Server { get; set; } = new ServerConfig();

        /// <summary>
        /// 右侧分隔条位置（高级工具 Tab 与日志区之间）
        /// </summary>
        public int SplitterDistance { get; set; } = 427;

        /// <summary>
        /// 左侧分隔条位置（模型参数与启动配置区之间）
        /// </summary>
        public int SplitterDistanceLeft { get; set; } = 433;
    }

    /// <summary>
    /// 服务器配置
    /// </summary>
    public class ServerConfig
    {
        /// <summary>
        /// API URL
        /// </summary>
        public string ApiUrl { get; set; } = "http://localhost:8090/v1";

        /// <summary>
        /// API Key
        /// </summary>
        public string ApiKey { get; set; } = "sk-1234567809";

        /// <summary>
        /// Model ID
        /// </summary>
        public string ModelId { get; set; } = string.Empty;

        /// <summary>
        /// 是否本地访问
        /// </summary>
        public bool IsLocal { get; set; } = true;
    }
}
