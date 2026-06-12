# v2.4.1 — 2026-06-12

修复 v2.4.0 三个已知 bug + 项目结构整理（编译产物移入 `build-output/`），全部由 mainFrm.cs + csproj + .gitignore 三处文件完成。

## 修复清单

### B-1：批量测速缺漏 `--model-draft` MTP 模型路径

**问题**：批量上下文测速只从 `_paramManager` 动态参数面板捞参数，完全绕过独立 `tb_mtpFile` 控件，导致拼出的命令带 `--spec-type draft-mtp` 但没有 `--model-draft "..."mtp.gguf"`，MTP 在批量测速中静默失效。

**修复**（`mainFrm.cs:2403` 附近）：在拼 `fullArgs` 时补上 MTP draft model 路径，与 chatTest / saveBatchCmd / openclaw_web 三个位置保持一致。

### B-2：b9601+ llama-cli 速度输出格式变更，正则失效

**问题**：`ParseSpeedLine` 和 `ParseSpeedFromOutput` 匹配老 llama.cpp 交互式末尾的总结行 `Prompt: 1234.5 t/s | Generation: 56.6 t/s`。b9601 改为结构化输出（`prompt eval time = 8734.21 ms / 971 tokens`），正则永远不命中，导致批量测速 4 档全显示 `0.0 t/s`，右上角 token 计数器也全 0。

**修复**（`mainFrm.cs:2586` 附近）：重写 `ParseSpeedLine` 和 `ParseSpeedFromOutput`，匹配 b9601 结构化格式，把 `tokens / (ms/1000)` 换算成 t/s。

### B-3：Win32Exception "操作已被用户取消"

**问题**：`btn_chatTest_Click` 用 `UseShellExecute = true` 起 CMD 窗口，工作目录 `E:\llama.cpp\启动管理工具cn.Lamma-windows v2.4` 含 CJK + 长路径，Win11 24H2 下 ShellExecuteEx 返回 ERROR_CANCELLED (1223)。

**修复**（`mainFrm.cs:1635` 附近）：`UseShellExecute = false` + `CreateNoWindow = false` + `WorkingDirectory = Path.GetDirectoryName(cliExe)`，绕开中文路径当工作目录。

## 验证清单

1. **CMD 对话测试**：点按钮 → 应弹出 CMD 窗口并加载模型，不再有 `Win32Exception`
2. **批量测速**（启用 `--spec-type draft-mtp` 系列参数）：
   - "基础参数"echo 应含 `--model-draft "...mtp.gguf"`
   - 4 档 CTX 不再是 5 秒一档的虚假结果
   - 数字应与 b9601 实际输出匹配（如 `Prompt=111.17 t/s | Generation=17.66 t/s`）
3. **右上角 token 计数器**：跑批量测速时显示具体数字（不再全 0）
4. **聊天网页（llama-server）**：MTP 加速在批量测速的子进程里也能看到效果

## 编译

```bash
cd "E:/llama.cpp/cn.LammaForms/cn.LammaForms/project"
"C:/Program Files/dotnet/dotnet.exe" build cn.LammaForms.sln -c Release
# 0 警告 0 错误
# 编译产物输出到 `E:\llama.cpp\cn.LammaForms\cn.LammaForms\build-output\`（v2.4.1+ 起）
```

## 部署

- 编译产物统一从 `cn.LammaForms/build-output/` 复制（v2.4.1+ 起，路径变更）
- 已更新到 `cn.Lamma-windows v2.4/` 目录
- 已更新到 `启动管理工具cn.Lamma-windows v2.4/` 目录（用户双击用）
- 旧版备份：`cn.Lamma-windows v2.4/_backup_v2.4.0/`
- 启动管理工具目录的 `system.json` / `config.json` 不需删除，参数面板不会受影响

## 项目结构整理（顺带做的）

- 新建 `cn.LammaForms/build-output/` 目录收纳编译产物
- 5 个散落文件（`cn.LammaForms.exe` 等）从源码根目录移到 `build-output/`
- `csproj` 的 `OutputPath` 从 `..\..\` 改为 `..\..\build-output\`，以后编译自动落进 `build-output/`，源码根不再有散落文件
- `.gitignore` 同步改为 `/build-output/` 整体忽略 + 补 `*.deps.json` / `*.runtimeconfig.json` 全局兜底
- `README.md` 编译章节补"产物输出位置"说明

整理前 vs 整理后：

```
整理前                                  整理后
E:\llama.cpp\cn.LammaForms\            E:\llama.cpp\cn.LammaForms\
└── cn.LammaForms\                      └── cn.LammaForms\
    ├── cn.LammaForms.exe  ❌散落           ├── build-output\          ← 新建收纳
    ├── cn.LammaForms.dll  ❌散落           │   ├── cn.LammaForms.exe
    ├── cn.LammaForms.pdb  ❌散落           │   ├── cn.LammaForms.dll
    ├── cn.LammaForms.deps.json ❌         │   ├── cn.LammaForms.pdb
    ├── cn.LammaForms.runtimeconfig.json ❌│   ├── cn.LammaForms.deps.json
    ├── cn.Lamma-windows v2.4\             │   └── cn.LammaForms.runtimeconfig.json
    ├── cn.Lamma-windows v2.4.2\           ├── cn.Lamma-windows v2.4\
    ├── project\                          ├── cn.Lamma-windows v2.4.2\
    └── source-backup-v2.4.0-…\           ├── project\
                                          └── source-backup-v2.4.0-…\
```

---

# v2.4.2 增量 — 2026-06-12

## B-4：6 个浏览按钮点不开（v2.4.0 时代隐藏 bug）

**问题**：6 个 dialog 调 `ShowDialog()` 时**没传父窗口句柄**。在嵌套容器（Panel / TableLayoutPanel / GroupBox）里 dialog 可能弹到主窗体后面或弹不出。**v2.4.0 时代就有，v2.4.1 也没改**——只是主人 v2.4.1 开始用 UI 操作才发现。

**影响**（4 个独立控件 + 2 个参数导入/导出）：
- `btn_llamaPath_Click`（选 llama.cpp 目录）
- `btn_ggufModelsPath_Click`（选 GGUF 模型目录）
- `btn_mmprojFile_Click`（选 mmproj 文件）
- `btn_mtpFile_Click`（选 MTP 文件）
- `button_import_options_Click`（引入启动参数）
- `button_save_options_Click`（保存启动参数）

**修复**：6 个 handler 各加 1 行 `var parentForm = (sender as Control)?.FindForm();` + 把 `ShowDialog()` 改为 `ShowDialog(parentForm)`。与 `ParamControlFactory.cs:391` 的 FilePath 控件写法一致。

## B-5：MTP 闪退配置避坑（文档补充，非代码 bug）

**问题**：v2.4.1 闪退**不是 v2.4.1 代码 bug**，是配置组合问题。主人实测发现：

1. **切到 UD-Q4_K_XL 模型**（带 MTP head 的版本）—— 不能用 Q4_0（旧版无 MTP head）
2. **取消"八、调试与日志参数"分类的所有勾选**—— 去掉 `--perplexity` 等冲突项
3. **正常工作后速度**：`Prompt: 44.5 t/s | Generation: 76.7 t/s`（MTP 加速生效）

**闪退根因总结**（避免复发）：

| 触发条件 | 现象 | 原因 |
|---|---|---|
| 主模型 Q4_0（无 MTP head） + mtpFile 已填 + 启 `--spec-type draft-mtp` | `Gemma4Assistant requires ctx_other to be set` + `llama_init_from_model: failed to initialize the context` | llama.cpp b9601 MTP 模式需要 MTP 头，Q4_0 没有 |
| 启 `--perplexity`（困惑度测试） + 启 `--spec-type draft-mtp` | llama-cli 立即计算退出，窗口一闪而过 | perplexity 模式跟 chatTest 互斥 |
| `--log-file` 路径含中文/空格 + 启调试参数 | 偶尔 Win32Exception（`操作已被用户取消`） | B-3 修复点未覆盖到 log-file 路径 |

**主人 v2.4.1 验证清单**（基于实操经验）：

1. ✅ 切到 UD-Q4_K_XL 模型（不是 Q4_0）
2. ✅ MTP 文件路径 `E:\llama.cpp\model\gemma-4-12B-it-QAT-unsloth-GGUF\gemma-4-12B-it-qat-UD-Q4_K_XL_mtp.gguf`（**不是** assistant-MTP-Q8_0.gguf）
3. ✅ 取消"八、调试与日志参数"所有勾选
4. ✅ KV 缓存类型用 f16（不是 q4_0）
5. ✅ ctx-size 用 4096（不是 131072）
6. ✅ 速度 44.5 / 76.7 t/s（MTP 加速生效）

## 编译（v2.4.2）

```bash
cd "E:/llama.cpp/cn.LammaForms/cn.LammaForms/project"
"C:/Program Files/dotnet/dotnet.exe" build cn.LammaForms.sln -c Release
```

## 部署（v2.4.2）

- 已更新到 `E:\llama.cpp\cn.Lamma-windows v2.4.1\` 目录（主人的"启动管理工具"v2.4.1 实例）
- 旧版备份：`cn.Lamma-windows v2.4.1\_backup_v2.4.1\`（v2.4.1 第一次跑稳前的备份，需要时回退）
