<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# SensitiveDetection

## Purpose
脱敏词汇（敏感词）检测与替换框架。提供 `ISensitiveDetectionProvider` 接口（默认基于 `sensitive-words.txt` 与 DFA 匹配）、`[SensitiveDetection]` 数据注解与对应的 ASP.NET Core 模型绑定器，使 API 入参在校验/绑定阶段自动校验或替换敏感词。

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `Attributes/` | `[SensitiveDetection]` ValidationAttribute 实现 (see `Attributes/AGENTS.md`) |
| `Extensions/` | `AddSensitiveDetection<T>()` DI 拓展 (see `Extensions/AGENTS.md`) |
| `ModelBinders/` | 字符串参数自动替换的模型绑定器与 Provider (see `ModelBinders/AGENTS.md`) |
| `Providers/` | `ISensitiveDetectionProvider` 接口与默认实现（基于词典 + 正则） |

## For AI Agents

### Working in this directory
- 默认 Provider 在入口程序集目录读取 `sensitive-words.txt`，每行一个词；部署时记得分发该文件。
- 注册顺序：先 `AddSensitiveDetection()` 再使用 `[SensitiveDetection]` —— 未注册 Provider 时校验逻辑会主动跳过，不抛异常（保留旧行为）。
- 结合 `Localization` 模块可对错误消息做多语言翻译（`L.Text["Characters contain sensitive words."]`）。

### Common patterns
- 校验模式：仅 `[SensitiveDetection]`，包含敏感词时返回 `ValidationResult` 失败。
- 替换模式：`[SensitiveDetection('*')]`，在模型绑定阶段把字符串参数替换为脱敏后的内容；对象属性则在 `IsValid` 中通过反射回写。

## Dependencies
### Internal
- `Poxiao.Localization`（错误消息）、`SuppressSniffer` 框架属性。
### External
- `Microsoft.AspNetCore.Mvc.ModelBinding`、`System.ComponentModel.DataAnnotations`。

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
