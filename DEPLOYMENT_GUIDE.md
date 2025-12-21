# 生产环境部署指南

## 安全检查清单

在部署到生产环境之前，请确保完成以下安全配置：

### 1. 配置强密码和密钥

- [ ] 生成强随机 JWT SecretKey (至少 64 字符)
- [ ] 更改数据库密码
- [ ] 更改 Redis 密码 (如果启用)
- [ ] 更改 RabbitMQ 用户名和密码

### 2. 环境变量配置

建议使用环境变量而不是硬编码配置：

```bash
# 数据库连接
export ConnectionStrings__DefaultConnection="Server=prod-db-server;Database=plant_health_check;User=app_user;Password=STRONG_PASSWORD_HERE"

# JWT 配置
export Jwt__SecretKey="YOUR_VERY_LONG_RANDOM_SECRET_KEY_AT_LEAST_64_CHARACTERS_LONG_HERE"
export Jwt__Issuer="PlantHealthCheck"
export Jwt__Audience="PlantHealthCheckClient"

# Redis
export Redis__Configuration="prod-redis-server:6379,password=REDIS_PASSWORD"

# RabbitMQ
export RabbitMQ__HostName="prod-rabbitmq-server"
export RabbitMQ__UserName="app_user"
export RabbitMQ__Password="RABBITMQ_PASSWORD"
```

### 3. CORS 配置

在 `Program.cs` 中更新 CORS 策略：

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins("https://yourdomain.com", "https://www.yourdomain.com")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// 使用时
app.UseCors("ProductionPolicy");
```

### 4. HTTPS 配置

- [ ] 配置有效的 SSL/TLS 证书
- [ ] 启用 HSTS (已在代码中配置)
- [ ] 确保所有 API 调用都通过 HTTPS

### 5. 数据库安全

- [ ] 创建专用的应用数据库用户（不使用 root）
- [ ] 只授予必要的权限

```sql
-- 创建专用用户
CREATE USER 'plant_app'@'%' IDENTIFIED BY 'STRONG_PASSWORD';
GRANT SELECT, INSERT, UPDATE, DELETE ON plant_health_check.* TO 'plant_app'@'%';
FLUSH PRIVILEGES;
```

### 6. 日志配置

生产环境应该：
- 将敏感信息从日志中移除
- 使用集中式日志系统
- 启用错误监控和告警

### 7. 性能优化

- [ ] 启用响应压缩
- [ ] 配置连接池大小
- [ ] 启用 Redis 缓存
- [ ] 配置数据库索引

### 8. 监控和健康检查

添加健康检查端点：

```csharp
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>()
    .AddRedis(builder.Configuration["Redis:Configuration"])
    .AddRabbitMQ();

app.MapHealthChecks("/health");
```

## Docker 生产部署

### 1. 创建生产 Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PlantHealthCheck.csproj", "./"]
RUN dotnet restore "PlantHealthCheck.csproj"
COPY . .
RUN dotnet build "PlantHealthCheck.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlantHealthCheck.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlantHealthCheck.dll"]
```

### 2. 创建生产 docker-compose.yml

```yaml
version: '3.8'

services:
  api:
    build: .
    ports:
      - "5001:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=${DB_CONNECTION_STRING}
      - Jwt__SecretKey=${JWT_SECRET_KEY}
    depends_on:
      - mysql
      - redis
      - rabbitmq
    restart: always

  mysql:
    image: mysql:8.0
    environment:
      - MYSQL_ROOT_PASSWORD=${MYSQL_ROOT_PASSWORD}
      - MYSQL_DATABASE=plant_health_check
    volumes:
      - mysql_data:/var/lib/mysql
    restart: always

  redis:
    image: redis:7-alpine
    command: redis-server --requirepass ${REDIS_PASSWORD}
    volumes:
      - redis_data:/data
    restart: always

  rabbitmq:
    image: rabbitmq:3-management-alpine
    environment:
      - RABBITMQ_DEFAULT_USER=${RABBITMQ_USER}
      - RABBITMQ_DEFAULT_PASS=${RABBITMQ_PASSWORD}
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    restart: always

volumes:
  mysql_data:
  redis_data:
  rabbitmq_data:
```

### 3. 使用 .env 文件

创建 `.env` 文件（不要提交到 git）：

```env
# Database
MYSQL_ROOT_PASSWORD=your_strong_mysql_password
DB_CONNECTION_STRING=Server=mysql;Database=plant_health_check;User=root;Password=your_strong_mysql_password

# JWT
JWT_SECRET_KEY=your_very_long_random_secret_key_at_least_64_characters

# Redis
REDIS_PASSWORD=your_strong_redis_password

# RabbitMQ
RABBITMQ_USER=admin
RABBITMQ_PASSWORD=your_strong_rabbitmq_password
```

## Kubernetes 部署

### 1. 创建 Secret

```bash
kubectl create secret generic plant-health-secrets \
  --from-literal=db-password='YOUR_DB_PASSWORD' \
  --from-literal=jwt-secret='YOUR_JWT_SECRET' \
  --from-literal=redis-password='YOUR_REDIS_PASSWORD' \
  --from-literal=rabbitmq-password='YOUR_RABBITMQ_PASSWORD'
```

### 2. 部署配置

参考 `k8s/` 目录中的配置文件（需要创建）。

## 备份策略

### 数据库备份

```bash
# 每天自动备份
0 2 * * * mysqldump -u root -p${MYSQL_ROOT_PASSWORD} plant_health_check > /backup/plant_health_check_$(date +\%Y\%m\%d).sql
```

### Redis 备份

配置 Redis RDB 持久化和 AOF。

## 监控建议

- 使用 Prometheus + Grafana 监控应用指标
- 配置日志聚合 (ELK Stack 或 Loki)
- 设置告警通知 (PagerDuty, Slack等)
- 监控数据库性能和连接池

## 性能调优

### 数据库

```sql
-- 添加索引
CREATE INDEX idx_username ON users(username);
CREATE INDEX idx_email ON users(email);
CREATE INDEX idx_created_at ON users(created_at);
```

### 应用配置

```csharp
// 连接池配置
services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion, mysqlOptions =>
    {
        mysqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});
```

## 安全加固

1. 启用 API 速率限制
2. 实现请求日志记录
3. 配置防火墙规则
4. 定期更新依赖包
5. 进行安全扫描
6. 实施最小权限原则

## 测试清单

- [ ] 负载测试
- [ ] 安全性测试
- [ ] 灾难恢复演练
- [ ] 备份恢复测试
- [ ] 监控告警测试
