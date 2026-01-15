### 静态资源

## 目录结构

```bash
├── BiVisualPath               # 大屏设计
├── CodeTemp                   # 代码生成器临时目录
├── DocumentFile               # 文档
├── DocumentPreview            # 文档预览
├── EmailFile                  # 邮件附件
├── IMContentFile              # IM聊天附件
├── SystemFile                 # 系统附件
├── TemplateCode               # 代码生成器模板
│   ├── TemplateCode1          # 流程表单模板
│   ├── TemplateCode2          # 功能表单模板
│   ├── TemplateCode3          # 功能流程模板
│   └── TemplateCode4          # 纯表单模板
├── TemplateFile               # 其他模板文档
├── TemporaryFile              # 临时存放目录
├── UserAvatar                 # 用户头像
└── WebAnnexFile               # 其他

```

## 使用说明

### jnpf-boot配置说明

打开`jnpf-admin/src/main/resources/application-*.yml`,找到`文件存储配置`

> 其中`*`对应`application.yml`中的`spring.profiles.active`的值，默认为`dev`

> 特别注意：配置中的格式(yaml格式)及目录最后的结束符号

#### 本地存储

- 修改默认存储平台(`default-platform`)为`local-plus-1`
- 修改基础路径`base-path`的值
  - 本地开发为项目所在路径，如：`F:/work/jnpf-resources/`
  - 部署环境，如`/www/wwwroot/jnpf-resources/`

```yml
  file-storage: #文件存储配置，不使用的情况下可以不写
    default-platform: local-plus-1 #默认使用的存储平台
    local-plus: # 本地存储升级版
      - platform: local-plus-1 # 存储平台标识
        enable-storage: true  #启用存储
        enable-access: true #启用访问（线上请使用 Nginx 配置，效率更高）
        domain: "" # 访问域名，例如：“http://127.0.0.1:8030/”，注意后面要和 path-patterns 保持一致，“/”结尾，本地存储建议使用相对路径，方便后期更换域名
        base-path: F:/work/jnpf-resources/ # 基础路径
        path-patterns: /** # 访问路径
        storage-path:  # 存储路径
```

#### 对象存储(含minio)

> 以MinIO为例

- 修改默认存储平台(`default-platform`)为`minio-1`
- 配置`MiniIO`，涉及如下参数
  - `access-key`：access-key
  - `secret-key`：secret-key
  - `end-point`：minio服务地址
  - `bucket-name`：桶名

```yml
  file-storage: #文件存储配置，不使用的情况下可以不写
    default-platform: local-plus-1 #默认使用的存储平台
    thumbnail-suffix: ".min.jpg" #缩略图后缀，例如【.min.jpg】【.png】
    local-plus: # 本地存储升级版
      - platform: local-plus-1 # 存储平台标识
        enable-storage: true  #启用存储
        enable-access: true #启用访问（线上请使用 Nginx 配置，效率更高）
        domain: "" # 访问域名，例如：“http://127.0.0.1:8030/”，注意后面要和 path-patterns 保持一致，“/”结尾，本地存储建议使用相对路径，方便后期更换域名
        base-path: F:/work/jnpf-resources/ # 基础路径
        path-patterns: /** # 访问路径
        storage-path:  # 存储路径
    minio: # MinIO，由于 MinIO SDK 支持 AWS S3，其它兼容 AWS S3 协议的存储平台也都可配置在这里
      - platform: minio-1 # 存储平台标识
        enable-storage: true  # 启用存储
        access-key: Q9jJs2b6Tv
        secret-key: Thj2WkpLu9DhmJyJ
        end-point: http://192.168.0.207:9000/
        bucket-name: jnpfsoftoss
        domain:  # 访问域名，注意“/”结尾，例如：http://minio.abc.com/abc/
        base-path:  # 基础路径
```

### jnpf-cloud配置说明

  打开`Nacos`控制台，依次选择`配置管理-配置列表-dev`中的`resources.yaml`

#### 本地存储

> 设置为本地存储时，静态资源文件需要和文件服务在同一台服务器

- 修改默认存储平台(`default-platform`)为`local-plus-1`
- 修改基础路径`base-path`的值
  - 本地开发为项目所在路径，如：`F:/work/jnpf-resources/`
  - 部署环境，如`/www/wwwroot/jnpf-resources/`

```yml
  file-storage: #文件存储配置，不使用的情况下可以不写
    default-platform: local-plus-1 #默认使用的存储平台
    local-plus: # 本地存储升级版
      - platform: local-plus-1 # 存储平台标识
        enable-storage: true  #启用存储
        enable-access: true #启用访问（线上请使用 Nginx 配置，效率更高）
        domain: "" # 访问域名，例如：“http://127.0.0.1:8030/”，注意后面要和 path-patterns 保持一致，“/”结尾，本地存储建议使用相对路径，方便后期更换域名
        base-path: F:/work/jnpf-resources/ # 基础路径
        path-patterns: /** # 访问路径
        storage-path:  # 存储路径
```

#### 对象存储(含minio)

> 以MinIO为例

- 修改默认存储平台(`default-platform`)为`minio-1`
- 配置`MiniIO`，涉及如下参数
  - `access-key`：access-key
  - `secret-key`：secret-key
  - `end-point`：minio服务地址
  - `bucket-name`：桶名

```yml
  file-storage: #文件存储配置，不使用的情况下可以不写
    default-platform: local-plus-1 #默认使用的存储平台
    thumbnail-suffix: ".min.jpg" #缩略图后缀，例如【.min.jpg】【.png】
    local-plus: # 本地存储升级版
      - platform: local-plus-1 # 存储平台标识
        enable-storage: true  #启用存储
        enable-access: true #启用访问（线上请使用 Nginx 配置，效率更高）
        domain: "" # 访问域名，例如：“http://127.0.0.1:8030/”，注意后面要和 path-patterns 保持一致，“/”结尾，本地存储建议使用相对路径，方便后期更换域名
        base-path: F:/work/jnpf-resources/ # 基础路径
        path-patterns: /** # 访问路径
        storage-path:  # 存储路径
    minio: # MinIO，由于 MinIO SDK 支持 AWS S3，其它兼容 AWS S3 协议的存储平台也都可配置在这里
      - platform: minio-1 # 存储平台标识
        enable-storage: true  # 启用存储
        access-key: Q9jJs2b6Tv
        secret-key: Thj2WkpLu9DhmJyJ
        end-point: http://192.168.0.207:9000/
        bucket-name: jnpfsoftoss
        domain:  # 访问域名，注意“/”结尾，例如：http://minio.abc.com/abc/
        base-path:  # 基础路径
```