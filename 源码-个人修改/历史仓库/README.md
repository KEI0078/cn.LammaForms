# 历史仓库（cn.LammaForms v2.4.0 之前的开发历史）

## 这是什么

`cn.LammaForms/` 目录下曾经有个隐藏的 git 仓库 `.git_old/`（位于 `cn.LammaForms/project/.git_old/`），是主人在 v2.4.0 初始化主仓库（`E:\llama.cpp\cn.LammaForms\.git`）**之前**的开发用 git 仓库。

## 为什么单独保存

- 主仓库（v2.4.0 起的 `E:\llama.cpp\cn.LammaForms\.git`）只有 **8 个 commit**（v2.4.0 初始化时塞进去的快照式 commit，没有保留开发历史）
- `.git_old/` 仓库有 **18 个 commit**，包含主人从原作者代码 fork 后所有的开发过程：
  - 修复多模态文件识别 bug
  - mmproj 多模态投影文件阈值调整
  - **新增 MTP 推测解码参数**（v2.4.0 关键功能）
  - 新增多卡拆分 / spec-type 多选控件
  - DPI 自适应优化
  - 批量测试标签页切换
  - 多卡拆分模式
  - Token 汇总统计面板
  - CPU 多线程测试
  - 等等

`.git_old/` 是主人**v2.4.0 之前开发历史的唯一备份**，丢了就再也找不回来，所以专门移出来保管。

## 位置历史

| 时间 | 位置 | 备注 |
|---|---|---|
| v2.4.0 之前 → 2026-06-12 23:29 | `cn.LammaForms/project/.git_old/` | 隐藏目录，被项目清理误判为垃圾 |
| 2026-06-12 23:30 起 | `源码-个人修改/历史仓库/` | 移出源码目录，避免干扰 git status |

## 怎么查看历史

```bash
# 进入历史仓库目录
cd "E:/llama.cpp/cn.LammaForms/源码-个人修改/历史仓库/cn.LammaForms"

# 查看所有 commit
git log --oneline --all

# 看某个 commit 的具体改动（比如 "新增MTP推测解码参数"）
git show c954fa0

# 跟主仓库对比（v2.4.0 起的功能）
# 主仓库在 E:/llama.cpp/cn.LammaForms/.git
# 历史仓库在当前目录
```

## 是否合并到主仓库？

**目前没有**。如果想抢救历史到主仓库（例如 `git remote add history ./cn.LammaForms` 然后 `git fetch history`），需要主人拍板。详见 `cn.LammaForms/CHANGELOG-v2.4.1.md` 末尾的讨论。

---

**创建时间**：2026-06-12（小A 代为整理）
**操作人**：主人 + 小A
**关联 commit**：主仓库 `6b90a38`（v2.4.1 修复三个 bug）
