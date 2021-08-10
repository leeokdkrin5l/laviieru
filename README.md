# SCF - SenparcCoreFramework

SenparcCoreFramework(SCF) 是一整套可用于构建基础项目的框架，包含了基础的缓存、数据库、模型、验证及配套管理后台，模块化，具有高度的可扩展性。

> 说明：SCF 由盛派（Senparc）团队经过多年优化迭代的自用系统底层框架 SenparcCore 整理而来，经历了 .NET 3.5/4.5 众多系统的实战检验，并最终移植到 .NET Core，目前已在多个 .NET Core 系统中稳定运行，在将其转型为开源项目的过程中，需要进行一系列的重构、注释完善和兼容性升级，目前尚处于雏形阶段，希望大家多提意见，我们会争取在最短的时间内优化并发布第一个试用版。感谢大家一直以来的支持！

SCF 将提供完善的项目自动生成服务（参考 [WeChatSampleBuilder](http://sdk.weixin.senparc.com/Home/WeChatSampleBuilder)），为开发者提供项目定制生成服务。

## 环境要求

- Visual Studio 2017 15.7 版本以上或 VS Code 最新版本

- .NET Core 2.1.4+ （未来将支持更多版本），SDK下载地址：https://dotnet.microsoft.com/download/dotnet-core/2.1

## 如何安装

SCF 将提供全自动的安装程序，目前正在整理阶段，您可以通过以下手动方法开始使用：

### 第一步：准备命令行工具

#### 方法一（推荐）：
使用命令行工具或 PowerShell 进入 `src/Senparc.Web` 路径，例如：`E:\SenparcCoreFramework\SCF\src\Senparc.Web`

#### 方法二（要看运气）：
1. 同步源代码到本地后，使用 Visual Studio 打开 `/src/SCF.sln`

2. 在 VS 菜单中选择【工具】>【Nuget包管理器】>【程序包管理器控制台】，打开命令窗口

3. 在【程序包管理器控制台】中的【默认项目】列表中选中 `Senparc.Web`（默认就是），在 `PM>` 符号后准备输入下一步命令



### 第二步：安装数据库

1. 确保已经安装 SQL Server 2008 以上，系统登录用户具有数据库创建权限（可以不需要使用sa等账号登录），如果必须要使用账号登录，[请看这里](https://github.com/SenparcCoreFramework/SCF/wiki/%E5%A6%82%E4%BD%95%E4%BF%AE%E6%94%B9%E9%BB%98%E8%AE%A4%E6%95%B0%E6%8D%AE%E5%BA%93%E8%BF%9E%E6%8E%A5%E5%AD%97%E7%AC%A6%E4%B8%B2%EF%BC%9F)

2. 输入命令：`dotnet ef database update` 回车

3. 稍等片刻（会自动编译一次项目，因此请勿修改项目代码），完成后输出如下结果，表示数据库安装成功：

```
Applying migration '20181130085128_init'.
Done.
```

### 第三步：初始化数据库

 1. 将 `Senparc.Web` 项目设为启动项目，并运行，地址如：http://localhost:11946/

 2. 打开 http://localhost:11946/Install ，数据库将会自动初始化

 3. 完成后，保存页面上显示的账号和密码，根据提示进入管理员后台。




## 待办事项：

 [ ] 前端包管理器要从Bower切换为LibMan
