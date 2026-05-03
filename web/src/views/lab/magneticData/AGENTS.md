<!-- Parent: ../AGENTS.md -->
<!-- Generated: 2026-04-30 -->

# magneticData

## Purpose
"磁性数据"页面：以 Tabs 区分数据浏览与导入向导。提供 Excel 快捷导入与分步导入向导，展示是否带 K（划痕）/ 是否有效的标签，并支持删除。

## Key Files
| File | Description |
|------|-------------|
| `index.vue` | 主页：a-tabs（数据 + 向导）+ BasicTable + 快捷导入 |
| `MagneticDataImportWizard.vue` | 多步骤导入向导容器（上传 → 校验 → 完成）|
| `MagneticDataImportQuickModal.vue` | 快捷导入弹窗（单步上传）|

## Subdirectories
| Directory | Purpose |
|-----------|---------|
| `components/` | 向导步骤组件（Step1/Step2）(see `components/AGENTS.md`) |

## For AI Agents

### Working in this directory
- API 走 `/@/api/lab/magneticData`，导入接口接受 multipart/form-data。
- "带K"=划痕，使用 `isScratched===1` 判定；"有效/无效"使用 `isValid===1`。
- 删除前 `popConfirm`。

### Common patterns
- `a-upload :before-upload` 拦截直接上传，转手动调 API。
- Tabs key='data' 与 'wizard' 之间共享 reload 触发。

## Dependencies
### Internal
- `/@/api/lab/magneticData`
- `/@/components/Table`, `/@/components/Modal`
### External
- `ant-design-vue`, `@ant-design/icons-vue`

<!-- MANUAL: Add manual notes below this line; they are preserved on regeneration -->
