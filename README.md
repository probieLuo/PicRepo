# PicRepo

PicRepo 是一个基于 Windows WPF 的图片托管工具，适合把本地图片快速上传到 GitHub / Gitee 仓库，并自动复制可直接使用的 URL、Markdown 或 HTML 代码到剪贴板。

核心功能：

- 让图片上传过程简单快速
- 支持拖拽上传和手动上传、剪切板上传（截图、文件、url、文件地址）
- 自动保存上传历史
- 支持不同复制格式（URL / Markdown / HTML）

## 主要功能

1. 图片上传
   - 拖拽图片到主窗口
   - 通过“点击上传”选择本地图片
   - 剪切板上传（截图、文件、url、文件地址）
   - 自动检测默认图床配置，上传到对应仓库

2. 图床支持
   - GitHub 仓库
   - Gitee 仓库
   - 可配置多个图床，设置某个为默认图床

3. 复制格式
   - 纯 URL
   - Markdown：`![](...)`
   - HTML：`<img src="..." />`
   - 可在设置中统一指定默认复制格式

### 界面截图

![](https://github.com/probieLuo/PicRepo/blob/master/resources/%E4%B8%BB%E7%AA%97%E5%8F%A3.png?raw=true)
![](https://github.com/probieLuo/PicRepo/blob/master/resources/%E5%8F%B3%E9%94%AE%E6%89%93%E5%BC%80%E8%8F%9C%E5%8D%95.png?raw=true)
![](https://github.com/probieLuo/PicRepo/blob/master/resources/%E6%9F%A5%E7%9C%8B%E5%A4%A7%E5%9B%BE.png?raw=true)
![](https://github.com/probieLuo/PicRepo/blob/master/resources/%E5%8E%86%E5%8F%B2%E8%AE%B0%E5%BD%95.png?raw=true)
![](https://github.com/probieLuo/PicRepo/blob/master/resources/Mini%E7%AA%97%E5%8F%A3.png?raw=true)

## 项目结构

```text
PicRepo/
├─ PicRepo.slnx
├─ README.md
├─ PicRepo.Client/
│  ├─ App.xaml.cs
│  ├─ PicRepo.Client.csproj
│  ├─ App.xaml
│  ├─ Data/
│  │  └─ AppDbContext.cs
│  ├─ Helper/
│  │  ├─ AppSettings.cs
│  │  ├─ EncryptionHelper.cs
│  │  └─ ...
│  ├─ Models/
│  │  ├─ UploadHistory.cs
│  │  ├─ CopyType.cs
│  │  └─ ...
│  ├─ Services/
│  │  └─ PicRepoService.cs
│  ├─ ViewModels/
│  │  ├─ MainWindowViewModel.cs
│  │  ├─ WinMiniViewModel.cs
│  │  ├─ Settings/
│  │  └─ ...
│  ├─ Views/
│  │  ├─ MainWindow.xaml
│  │  ├─ WinHistory.xaml
│  │  ├─ WinMini.xaml
│  │  └─ Settings/
│  ├─ Migrations/
│  └─ ...
└─ .gitignore
```

## 使用方法

### 1. 配置图床

启动程序后，打开“设置”窗口，进入图床配置页。
点击`+`按钮
可添加以下图床类型：

- GitHub
- Gitee

图床配置名任意填写

填写配置项

- 仓库名 github/gitee 仓库，最好创建公开仓库
- 用户名 
- 分支 一般来说github默认main，gitee默认master
- Token [github/gitee token 申请步骤](#github--gitee-token-申请步骤)

例：

![](https://github.com/probieLuo/PicRepo/blob/master/resources/%E6%96%B0%E5%BB%BA%E5%9B%BE%E5%BA%8A%E9%85%8D%E7%BD%AE01.png?raw=true)
![](https://github.com/probieLuo/PicRepo/blob/master/resources/%E6%96%B0%E5%BB%BA%E5%9B%BE%E5%BA%8A%E9%85%8D%E7%BD%AE02.png?raw=true)

说明：

- Token 会被程序加密后存储，不建议手动修改 `config.yaml` 中的敏感信息。
- 建议在界面中配置，避免出现格式错误。
- 设置默认图床后，后续上传会优先使用该图床。

### 2. 上传图片

将图片直接拖放到主窗口的上传区、在主窗口点击“点击上传” 选择本地图片文件、剪切板上传（截图、文件、url、文件地址）

### 3. 历史记录

主窗口右侧会显示最近上传历史。

鼠标右键执行以下操作：

- 复制 URL
- 复制 Markdown
- 复制 HTML
- 查看图片

### 4. 复制格式说明

默认复制格式可以在“设置 -> 通用”中调整。

支持：

- URL：`https://...`
- Markdown：`![](... )`
- HTML：`<img src="..." alt="..." />`

## 数据存储

程序会在运行目录生成两个本地文件：

- `app.db`：SQLite 数据库，保存上传历史
- `config.yaml`：保存程序配置（主题、默认复制格式、图床配置等）

## 典型使用场景
PicRepo 是一个轻量、实用的桌面端图片托管工具，适合需要高频上传图片并快速复制链接的场景。它特别适合：

- 个人博客
- 技术文档中的图片管理
- 社交平台图文内容发布

## 常见问题

### 上传失败

常见原因：

- Token 无效或过期
- 仓库名 / 用户名 / 分支名填写错误
- 网络异常
- 图片文件格式不支持

### 复制到剪贴板没有按预期格式输出

检查设置中的“默认复制格式”，可切换为：

- URL
- Markdown
- HTML

### 复制到剪贴板的URL无法在Web打开

检查仓库是否公开，如私有仓库是需要密钥的，无法生成公共访问的url，请设置仓库为公开。

## GitHub / Gitee Token 申请步骤

### 1. GitHub Token 申请步骤

1. 打开 GitHub 官网，登录你的账号。
2. 进入个人设置：右上角头像 -> Settings。
3. 在左侧菜单中选择 Developer settings。
4. 进入 Personal access tokens。
5. 点击 Fine-grained tokens 或 Tokens（classic）。
6. 选择“Generate new token”。
7. 设置 token 说明，例如：`PicRepo image upload`。
8. 选择适当的过期时间，例如 30 days / 90 days / No expiration。
9. 配置仓库权限：
   - 推荐选择“Only select repositories”
   - 选择用于上传图片的目标仓库
   - 至少确保该仓库具有 Contents: Read and write 权限
10. 点击 Generate token。
11. 复制生成的 token，并在程序中填入对应配置。

如果你使用的是 GitHub 图床配置，通常需要填写：

- Owner：仓库所属用户名或组织名
- Repo：仓库名
- Branch：通常为 `main` 或 `master`
- Token：刚申请的 GitHub Token

### 2. Gitee Token 申请步骤

1. 打开 Gitee 官网，登录你的账号。
2. 进入个人设置：右上角头像 -> 设置。
3. 在左侧导航中选择 私人令牌。
4. 点击生成新令牌。
5. 填写令牌名称，例如：`PicRepo image upload`。
6. 选择或确认令牌类型`仓库级私人令牌`，选择仓库的最好公开以便于访问
7. 点击提交。
8. 复制生成的 token，并保存在安全位置。
9. 在 PicRepo 中填入以下配置：
   - Owner：仓库所属用户名
   - Repo：仓库名
   - Branch：通常为 `master`，也可能是 `main`
   - Token：刚复制的 Gitee Token
