# v2.4.1 — 2026-06-12

修复 v2.4.0 三个已知 bug，全部由 mainFrm.cs 一处文件完成。

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
```

## 部署

- 编译产物已更新到 `cn.Lamma-windows v2.4/`
- 旧版备份：`cn.Lamma-windows v2.4/_backup_v2.4.0/`
- 用户手动复制到 `启动管理工具cn.Lamma-windows v2.4/` 即可使用
- 启动管理工具目录的 `system.json` / `config.json` 不需删除，参数面板不会受影响
