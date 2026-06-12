# TokenMaxMeter v2 — 本地大模型测速与参数优化工具

> 基于 llama.cpp 的本地大模型性能测试与最优参数匹配工具，支持动态参数加载、TurboQuant、批量上下文测速、CPU 多线程测速等功能。

<div align="center">

**🚀 快速测速 | 📊 性能分析 | ⚡ 参数优化 | 🔧 动态配置 | 🔍 GPU 检测**

</div>

---

## 📋 项目简介

TokenMaxMeter v2（项目名 `cn.LammaForms`）是面向本地大模型推理的性能测试工具的重构版本。相比 v1，核心升级在于**动态参数加载架构** — 从 JSON 配置文件自动解析 llama.cpp/TurboQuant 启动参数，动态生成 UI 控件，实现参数面板的零代码扩展。

### ✨ 核心特性

| 功能模块 | 说明 |
|---------|------|
| 🔧 动态参数面板 | 从 JSON/Markdown 自动加载参数，8 大分类 35+ 参数 |
| 🎯 最优参数检测 | 调用 `llama-fit-params.exe` 获取最优启动参数 |
| ⚡ 批量上下文测速 | 支持 4K~5M 共 36 档上下文大小批量测试 |
| 🧵 CPU 多线程测速 | 遍历线程范围，找出最优线程数配置 |
| 🌐 Web 服务管理 | 一键启动/关闭 llama-server Web 服务（支持多模态） |
| 💾 配置持久化 | config.json + system.json 双层配置保存 |
| 🔄 双向参数同步 | 参数面板 ↔ 文本框实时同步 |
| 📥 导入/导出 | 支持 .txt 配置文件的导入和导出 |
| 🛡️ 路径验证 | llama.cpp 路径自动检测（颜色反馈：绿/黄/红） |
| 📋 ToolTip 提示 | 所有参数悬停显示说明和默认值 |

### 🆚 v1 → v2 核心变化

| 维度 | v1 (cn.llaManager) | v2 (cn.LammaForms) |
|------|-------------------|-------------------|
| 参数配置 | 硬编码 UI 控件 | JSON + Markdown 动态生成 |
| 参数持久化 | config.json + startWeb.txt | config.json + system.json |
| 参数面板 | 固定布局 | 折叠面板 + 分类动态加载 |
| 代码架构 | 单文件 (2个cs) | 分层架构 (Config / Controls / mainFrm) |
| TurboQuant | 不支持 | 完整支持 (turboquant-params.json) |
| 参数同步 | 手动同步 | 双向自动同步（面板↔文本框） |

---

## 🏗️ 技术架构

```
┌─────────────────────────────────────────────────────────┐
│                   表现层 (UI Layer)                      │
│  ┌────────────────────────────────────────────────────┐ │
│  │                mainFrm (主窗体)                     │ │
│  │  ·左面板(模型/参数/配置)  ·右面板(测试/日志)       │ │
│  └────────────────────────────────────────────────────┘ │
│                         │                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │            控件层 (ParamControlFactory)              │ │
│  │  ·CreateParamControl()    ·CreateCategoryPanel()   │ │
│  └────────────────────────────────────────────────────┘ │
│                         │                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │         配置层 (ConfigManager + SystemParamManager)  │ │
│  │  ·config.json管理  ·system.json管理  ·MD文档解析   │ │
│  └────────────────────────────────────────────────────┘ │
│                         │                                │
│  ┌────────────────────────────────────────────────────┐ │
│  │              数据层 (JSON配置文件)                   │ │
│  │  ·config.json  ·system.json  ·llama-params.json    │ │
│  │  ·turboquant-params.json                            │ │
│  └────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────┘
```

### 技术栈

| 技术 | 版本 | 用途 |
|------|------|------|
| .NET | 10.0 (net10.0-windows) | 运行时框架 |
| C# | 13.0 | 编程语言 |
| Windows Forms | WinExe | UI 框架 |
| llama.cpp | 外部 | 底层推理引擎 |

---

## 📁 项目结构

```
cn.LammaForms/
├── cn.LammaForms.sln                    # 解决方案文件
├── README.md                            # 本文档
├── token.ico / token.png                # 应用图标
├── 大模型本地测速.md                     # 功能需求文档
├── docs/                                # 📚 项目文档
│   ├── ARCHITECTURE.md                  # 架构设计文档
│   └── API-REFERENCE.md                 # API & 类参考文档
│
├── cn.LammaForms/                       # 主项目
│   ├── Program.cs                       # 程序入口
│   ├── mainFrm.cs                       # 主窗体逻辑
│   ├── mainFrm.Designer.cs              # 窗体设计器代码
│   ├── cn.LammaForms.csproj             # 项目文件
│   │
│   ├── Config/                          # 配置层
│   │   ├── AppConfig.cs                 # 应用配置数据模型 (AppConfig + ServerConfig)
│   │   ├── ConfigManager.cs             # 配置管理器（单例）
│   │   ├── SystemParamItem.cs           # 参数数据模型 (SystemParamItem + ParamCategory + ParamValueType)
│   │   └── SystemParamManager.cs        # 参数管理器（单例，含 Markdown 解析引擎）
│   │
│   ├── Controls/                        # 控件层
│   │   └── ParamControlFactory.cs       # 参数控件工厂
│   │
│   └── params/                          # 参数定义
│       ├── llama-params.json            # llama.cpp 标准参数定义
│       └── turboquant-params.json       # TurboQuant 独有参数定义
│
└── all-icons/                           # 图标资源
```

---

## 🚀 快速开始

### 环境要求

- **操作系统**: Windows 10/11
- **.NET SDK**: .NET 10.0
- **Visual Studio**: 2022 (推荐) 或 VS Code + .NET SDK
- **llama.cpp**: 需要编译好的 `llama-cli.exe`、`llama-fit-params.exe`、`llama-server.exe`

### 构建步骤

```bash
# 1. 克隆项目
git clone http://192.168.199.88/bamboo/cn.llamamanager.git
cd cn.llamamanager/project

# 2. 编译项目
dotnet build cn.LammaForms.sln -c Release

# 3. 运行程序
dotnet run --project cn.LammaForms/cn.LammaForms.csproj
```

### 使用流程

#### 1. 配置 llama.cpp 路径

在界面中设置 llama.cpp 的 Build 目录和 GGUF 模型文件路径：

```
llama.cpp 路径: D:/dev/llama.cpp/build/bin/Release
模型路径: D:/dev/llama.cpp/models
```

路径验证结果以颜色反馈：
- 🟢 **浅绿色** — 验证通过（llama-server.exe 和 llama-cli.exe 均存在）
- 🟡 **浅黄色** — 部分通过（缺少其中一个可执行文件）
- 🔴 **浅红色** — 验证失败

#### 2. 动态参数配置

v2 核心升级 — 左侧参数面板从 JSON 自动加载，8 大分类 35+ 参数：

| 分类 | 图标 | 参数数量 | 代表参数 |
|------|------|---------|---------|
| 核心基础参数 | 🔧 | 12 | ctx-size, threads, flash-attn, ngl |
| KV 缓存量化类型 | ⚙️ | 2 | cache-type-k, cache-type-v |
| TriAttention 参数 | ⚙️ | 3 | tri-budget, tri-interval |
| 核心解码参数 | 🔧 | 10 | temp, top-k, top-p, repeat-penalty |
| 系统资源与内存优化 | 💾 | 2 | low-vram, mlock |
| 上下文窗口管理 | 📏 | 2 | keep, chunks |
| 多GPU加速与混合计算 | 🎮 | 2 | main-gpu, tensor-split |
| 调试与日志 | 🐛 | 4 | verbose, log-disable, log-format |

每个参数支持：
- ✅ 勾选启用/取消禁用
- 🔢 根据类型自动生成对应控件（数值/下拉/文本/文件选择）
- 💡 鼠标悬停显示描述和默认值
- ↔️ 参数面板与文本框双向同步

#### 3. 获取最优参数

1. 选择上下文大小
2. 点击"获取最优参数"
3. 工具自动调用 `llama-fit-params.exe` 分析模型
4. 返回最优参数（如 `-c 65536 -ngl 41 --flash-attn auto`）

#### 4. 批量测速

1. 切换到"上下文批量测试"标签页
2. 勾选需要测试的上下文大小（36 档可选）
3. 点击"开始批量测速"

#### 5. CPU 多线程测速

1. 切换到"多线程测试"标签页
2. 设置线程范围和步长
3. 点击"CPU 多线程测速"

#### 6. 启动 Web 服务

1. 配置监听地址、端口
2. 选择本地访问/远程访问
3. 点击"开启 llama.cpp 的 Web 访问"
4. 通过 `http://localhost:8090` 访问 API

#### 7. 导入/导出配置

- **引入启动参数**: 从 .txt 文件导入参数配置（自动同步到面板）
- **保存启动参数**: 将当前参数配置导出为 .txt 文件
- **动态解析启动参数**: 手动编辑文本框后，点击解析同步到面板
- **生成 server 启动脚本**: 将当前配置保存为桌面 .bat 文件

---

## ⚙️ 配置说明

### config.json

自动保存在程序运行目录，存储应用配置：

```json
{
  "llamaPath": "D:/dev/llama.cpp/build/bin/Release",
  "ggufModelsPath": "D:/dev/llama.cpp/models",
  "selectedModel": "Qwen3.5-35B-A3B-Q4_K_M.gguf",
  "mmprojFile": "",
  "server": {
    "apiUrl": "http://127.0.0.1:8090/v1",
    "apiKey": "sk-1234567809",
    "modelId": "",
    "isLocal": true
  }
}
```

### system.json

自动保存在程序运行目录，存储参数运行时状态（启用/值）：

- 首次运行时从 `CreateDefaultParams()` 自动生成
- 后续加载保留用户的启用/禁用和值修改
- 若包含废弃标记则自动重新生成

### 参数定义文件

| 文件 | 说明 |
|------|------|
| `params/llama-params.json` | llama.cpp 标准启动参数定义（5 大分类） |
| `params/turboquant-params.json` | TurboQuant 独有参数定义（4 大分类，兼容性 v1.5.0+） |

---

## 📊 上下文大小支持

| 范围 | 步进 | 数量 |
|------|------|------|
| 4K ~ 96K | ×2 或固定步进 | 10 |
| 128K ~ 960K | 每 +32K | 27 |
| 1M ~ 5M | ×2 或固定 | 4 |

共支持 **36 档** 上下文大小，覆盖从 4K 到 5M 的范围。

---

## 🛠️ 开发指南

### 核心设计模式

| 模式 | 应用位置 | 说明 |
|------|---------|------|
| 单例模式 | ConfigManager, SystemParamManager | 双重检查锁定单例 |
| 工厂模式 | ParamControlFactory | 根据 ParamValueType 生成控件 |
| 观察者模式 | OnConfigChanged, OnParamsChanged | 事件驱动配置变更 |
| 双向同步 | 面板↔文本框 | UpdateWebConfigFromParams / SyncParamsFromWebConfig |

### 扩展开发

#### 新增参数

1. 在 `SystemParamManager.CreateDefaultParams()` 中添加 `SystemParamItem`
2. 设置 `ValueType`、`DefaultValue`、`MinValue`、`MaxValue`
3. UI 自动生成对应控件

#### 新增参数分类

1. 在 `CreateDefaultParams()` 中添加 `ParamCategory`
2. 设置 `SortOrder` 控制显示顺序
3. 在 `GetCategoryIcon()` 和 `GetCategorySortOrder()` 中添加映射

#### 新增参数类型

1. 在 `ParamValueType` 枚举中添加新类型
2. 在 `ParamControlFactory.CreateParamControl()` 的 switch 中添加控件生成逻辑
3. 在 `SystemParamManager.InferParamType()` 中添加类型推断规则

### 待实现功能

| 功能 | 位置 | 状态 |
|------|------|------|
| 获取最优启动参数 | `btn_getOptions_Click()` | 🔲 TODO |
| 开启/关闭 Web 访问 | `btn_openclaw_web_Click()` | 🔲 TODO |
| 上下文批量测速 | `btn_contentTest_Run_Click()` | 🔲 TODO |
| CPU 多线程测试 | `btn_multithreadTest_cpu_Click()` | 🔲 TODO |
| 并发请求测试 | `tb_multithreadTest_request` | 🔲 已禁用 |

---

## 📚 文档索引

| 文档 | 路径 | 说明 |
|------|------|------|
| 架构设计文档 | `docs/ARCHITECTURE.md` | 系统分层架构、数据流、设计模式详解 |
| API 参考文档 | `docs/API-REFERENCE.md` | 所有类/方法/属性的完整参考 |
| 功能需求文档 | `大模型本地测速.md` | 原始功能需求和运行逻辑说明 |

---

## 📄 License

本项目为内部工具，仅供学习和研究使用。

---

## 🙏 致谢

- [llama.cpp](https://github.com/ggerganov/llama.cpp) — 高效的本地大模型推理引擎
- [TurboQuant](https://github.com/ggml-org/llama.cpp) — KV Cache 压缩优化
- [GGUF](https://github.com/ggerganov/ggml/blob/master/docs/gguf.md) — 模型格式标准
