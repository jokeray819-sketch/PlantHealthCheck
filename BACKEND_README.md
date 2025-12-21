# Plant Health Check 后端系统

基于 .NET 10.0 的植物健康检查系统后端 API，使用 DDD（领域驱动设计）架构。

## 技术栈

- **.NET 10.0** - 核心框架
- **Entity Framework Core** - ORM
- **MySQL** - 数据库
- **Redis** - 缓存
- **RabbitMQ** - 消息队列
- **MediatR** - CQRS 模式实现
- **DotNetCore.CAP** - 分布式事务
- **JWT** - 身份认证
- **Swagger** - API 文档

## 项目结构

```
PlantHealthCheck/
├── Domain/              # 领域层
│   ├── Entities/        # 实体
│   └── Repositories/    # 仓储接口
├── Application/         # 应用层
│   ├── Commands/        # 命令
│   ├── Handlers/        # 命令处理器
│   ├── DTOs/            # 数据传输对象
│   └── Services/        # 应用服务接口
├── Infrastructure/      # 基础设施层
│   ├── Data/            # 数据库上下文和迁移
│   ├── Repositories/    # 仓储实现
│   └── Services/        # 服务实现
└── API/                 # API 层
    └── Controllers/     # 控制器
```

## 环境要求

- .NET 10.0 SDK
- MySQL 8.0+
- Redis 6.0+
- RabbitMQ 3.8+

## 配置说明

在 `appsettings.json` 中配置以下内容：

### 数据库连接

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=plant_health_check;User=root;Password=yourpassword;"
}
```

### JWT 配置

```json
"Jwt": {
  "SecretKey": "your-secret-key-min-32-characters-long-for-security",
  "Issuer": "PlantHealthCheck",
  "Audience": "PlantHealthCheckClient",
  "ExpirationHours": "24"
}
```

### Redis 配置

```json
"Redis": {
  "Configuration": "localhost:6379",
  "InstanceName": "PlantHealthCheck:"
}
```

### RabbitMQ 配置

```json
"RabbitMQ": {
  "HostName": "localhost",
  "Port": 5672,
  "UserName": "guest",
  "Password": "guest"
}
```

## 快速开始

### 方式一：使用 Docker Compose (推荐)

这是最简单的方式，会自动启动 MySQL、Redis 和 RabbitMQ。

```bash
# 启动所有服务
docker-compose up -d

# 查看服务状态
docker-compose ps

# 查看日志
docker-compose logs -f

# 停止所有服务
docker-compose down

# 停止并删除数据卷
docker-compose down -v
```

服务启动后：
- MySQL: localhost:3306 (用户: root, 密码: yourpassword)
- Redis: localhost:6379
- RabbitMQ: localhost:5672 (管理界面: http://localhost:15672, 用户: guest, 密码: guest)

数据库和测试用户会自动创建。

### 方式二：手动安装

如果不使用 Docker，需要手动安装和配置各个服务。

#### 1. 安装必要的服务

- MySQL 8.0+
- Redis 6.0+
- RabbitMQ 3.8+

#### 2. 创建数据库

```bash
mysql -u root -p
CREATE DATABASE plant_health_check CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

### 2. 运行数据库迁移

```bash
cd PlantHealthCheck
dotnet ef database update
```

或者使用生成的 SQL 脚本：

```bash
mysql -u root -p plant_health_check < Infrastructure/Data/init.sql
```

### 3. 创建测试用户

```sql
USE plant_health_check;

-- 密码是 "password123" 的 BCrypt 哈希值
INSERT INTO users (username, password_hash, email, created_at, is_active)
VALUES ('admin', '$2a$11$XKV6zYqG4vHN/lYBJQwwX.ZJGJQQHqZ1vQQXYqGqGqGqGqGqGqGqG', 'admin@example.com', UTC_TIMESTAMP(), 1);
```

注意：实际使用时需要用正确的 BCrypt 哈希值替换上面的密码哈希。

### 4. 启动应用

```bash
dotnet run
```

应用将在 `https://localhost:5001` 启动，Swagger UI 可在根路径访问。

## API 文档

启动应用后，访问 `https://localhost:5001` 查看 Swagger API 文档。

### 主要接口

#### 用户登录

**POST** `/api/Auth/login`

请求体：

```json
{
  "username": "admin",
  "password": "password123"
}
```

响应：

```json
{
  "success": true,
  "message": "登录成功",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@example.com"
  }
}
```

### 使用 JWT Token

在后续的 API 请求中，在 HTTP Header 中添加：

```
Authorization: Bearer {token}
```

## 开发工具

### 添加新的迁移

```bash
dotnet ef migrations add MigrationName --output-dir Infrastructure/Data/Migrations
```

### 更新数据库

```bash
dotnet ef database update
```

### 生成 SQL 脚本

```bash
dotnet ef migrations script --output script.sql
```

## 项目特性

### 1. DDD 架构

- **Domain Layer**: 包含业务实体和领域逻辑
- **Application Layer**: 包含业务用例和应用逻辑
- **Infrastructure Layer**: 包含数据访问和外部服务
- **API Layer**: 包含 HTTP 接口

### 2. CQRS 模式

使用 MediatR 实现命令查询职责分离：

- Commands: 修改系统状态
- Queries: 查询系统状态

### 3. 分布式事务

使用 DotNetCore.CAP 支持：

- 本地消息表模式
- 最终一致性保证

### 4. 安全性

- JWT 身份认证
- BCrypt 密码哈希
- CORS 配置
- HTTPS 强制

## 测试

### 使用 curl 测试登录

```bash
curl -X POST https://localhost:5001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "password123"
  }'
```

### 使用 Postman 测试

1. 导入项目根目录下的 Postman Collection
2. 设置环境变量
3. 执行登录请求获取 token
4. 使用 token 访问受保护的接口

## 故障排除

### MySQL 连接失败

检查：
- MySQL 服务是否运行
- 连接字符串是否正确
- 数据库是否已创建

### Redis 连接失败

检查：
- Redis 服务是否运行
- Redis 配置是否正确

### RabbitMQ 连接失败

检查：
- RabbitMQ 服务是否运行
- RabbitMQ 配置是否正确
- 用户权限是否正确

## 许可证

MIT
