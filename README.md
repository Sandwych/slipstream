# 简介

SlipStream 是一个用 C# 实现的数据库应用快速开发平台原型。

代码实现参考了 OpenERP、Orchard 等开源项目。

# 需求

* Microsoft Visual Studio 2012
* Microsoft SQL Server Express 2005+ 数据库或 PostgreSQL 8.2+ 数据库。
* ZeroMQ 消息队列库
* Silverlight 5

默认使用 SQL Server Express，需要建立用户名及密码均为 slipstream 的用户。

# 如何运行
0. 首先确保 slipstream/lib/zmq/libzmq.dll 放置到 PATH 环境变量能找到的目录中，如 windows/system32，Linux 系统则是确保系统中安装了 libzmq.so。
1. 启动 SlipStream.DevServer 项目
2. 启动 SlipStream.Client.Agos 项目
3. 默认服务器用户名及密码均为“root”。


# 版权

本项目的授权方式为 AGPL3。

版权所有 (C) 2010 至今 昆明维智众源企业管理咨询有限公司。
保留所有权利。

昆明维智众源企业管理咨询有限公司  
http://www.sandwych.com  
contact@sandwych.com  
