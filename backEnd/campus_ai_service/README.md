# Campus AI Service 后端说明

本目录用于存放校园 AI 小精灵后端服务代码。服务基于 Python 启动，提供 AI 问答、知识库检索、健康检查等接口能力，供校园探索系统的后端或前端调用。

## 一、项目结构

```text
campus_ai_service/
├── config/              # 配置文件
├── core/                # 核心服务逻辑
├── data/                # 数据文件目录
├── knowledge/           # 知识库相关文件
├── modules/             # 功能模块
├── prompts/             # 提示词模板
├── main.py              # 服务启动入口
├── requirements.txt     # Python 依赖列表
└── README.md            # 项目说明文档
```


## 二、环境准备

建议使用 Python 3.10 及以上版本。

首次运行前，进入项目目录并创建虚拟环境：

```bash
python -m venv venv
```

Windows 环境激活虚拟环境：

```bash
.\venv\Scripts\activate
```

Linux / macOS 环境激活虚拟环境：

```bash
source venv/bin/activate
```

安装依赖：

```bash
pip install -r requirements.txt
```

## 三、本地启动

进入 `campus_ai_service` 目录后执行：

```bash
python main.py
```

如果使用 Linux 服务器，也可以执行：

```bash
python3 main.py
```

服务默认端口：

```text
8000
```

健康检查接口：

```text
/api/health
```

启动成功后，可通过浏览器或接口测试工具访问：

```text
http://localhost:8000/api/health
```

## 四、服务器部署说明

服务器部署时，建议将代码放在固定目录，例如：

```text
/root/campus_ai/
```

进入服务目录：

```bash
cd /root/campus_ai
```

设置必要环境变量：

```bash
export TOKENIZERS_PARALLELISM=false
```

启动服务：

```bash
python3 main.py
```

停止已有服务进程时，可根据实际情况使用：

```bash
pkill -f "python3 main.py"
```

然后重新启动：

```bash
python3 main.py
```

## 五、systemd 后台自启动

为了避免关闭终端后服务停止，建议使用 `systemd` 配置后台运行。

创建服务文件：

```bash
sudo vim /etc/systemd/system/campus-ai.service
```

示例配置如下：

```ini
[Unit]
Description=Campus AI Service
After=network.target

[Service]
WorkingDirectory=/root/campus_ai
ExecStart=/usr/bin/python3 /root/campus_ai/main.py
Restart=always
RestartSec=5
Environment=TOKENIZERS_PARALLELISM=false

[Install]
WantedBy=multi-user.target
```

重新加载 systemd 配置：

```bash
sudo systemctl daemon-reload
```

设置开机自启动：

```bash
sudo systemctl enable campus-ai
```

启动服务：

```bash
sudo systemctl start campus-ai
```

重启服务：

```bash
sudo systemctl restart campus-ai
```

查看服务状态：

```bash
sudo systemctl status campus-ai
```

实时查看日志：

```bash
journalctl -u campus-ai -f
```

## 六、知识库文件更新

如果需要更新校园知识库文件，可将新的知识库文件上传到服务器的知识库目录。

示例目录：

```text
/root/campus_ai/data/knowledge/
```

更新前可先查看目录内容：

```bash
ls data/knowledge/
```

如果需要删除旧知识库文件：

```bash
rm data/knowledge/*.xlsx
```

上传新的知识库文件后，重启服务：

```bash
sudo systemctl restart campus-ai
```

## 七、与主后端的关系

本服务主要负责 AI 能力，包括知识库问答、文本处理和智能引导等功能。

主业务后端通常负责用户登录注册、角色信息、任务系统、成长记录、数据库读写等业务逻辑。

两者可以通过 HTTP 接口进行通信，例如主后端调用 AI 服务的问答接口或健康检查接口。

## 八、安全注意事项

以下内容不要提交到 GitHub：

```text
服务器密码
数据库密码
邮箱授权码
JWT 密钥
API Key
secret.key
.env
日志文件
```

如果相关密钥已经被上传到 GitHub，建议立即更换服务器密码、数据库密码、邮箱授权码和 JWT 密钥，并从仓库历史中清理敏感信息。

## 九、常用维护命令

进入服务目录：

```bash
cd /root/campus_ai
```

启动服务：

```bash
python3 main.py
```

停止服务：

```bash
pkill -f "python3 main.py"
```

重启 systemd 服务：

```bash
sudo systemctl restart campus-ai
```

查看运行状态：

```bash
sudo systemctl status campus-ai
```

查看实时日志：

```bash
journalctl -u campus-ai -f
```
