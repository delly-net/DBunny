---
name: git-commit-update
description: 自动暂存更改、生成 git 提交消息并提交推送
effort: low
user-invocable: true
disable-model-invocation: true
agent: Explore
allowed-tools: Read, Write, Bash
---

# Git 自动提交

此技能自动检测更改、暂存文件，并使用合适的提交消息创建 git 提交。

## 使用方法

```
/git-commit-update
```

## 功能

1. **清空上下文**：
   - 运行：/clear
   - 必须等待它完全执行完毕

2. **升级当前版本号**
   - 读取所有 `.csproj` 项目中的 `<Version>` 标签
   - 版本号格式为 `yyyy.MM.修订号`，找到所有项目中修订号最大的版本号
   - 将修订号加 1，yyyy替换为当前年份，MM替换为当前月份，生成{新的版本号}
   - 更新所有 `.csproj` 文件中的 `<Version>` 标签为{新的版本号}
   - 等待所有文件更新完成，再继续下一步

3. **提交代码**
   - 运行：/eazy-git-commit
   - 等待执行完成并返回结果

4. **返回结果**
   - 返回**提交代码**步骤的返回结果
   - 明确告知执行已经结束
   - 使用中文输出

## 输出

返回提交哈希和提交内容摘要。