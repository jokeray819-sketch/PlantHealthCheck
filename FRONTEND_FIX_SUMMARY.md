# 前端访问问题修复说明

## 问题描述

前端 Blazor 页面无法访问。项目包含：
- 前端：Blazor Server
- 后端：NetCorePal (DDD 架构)
- API 文档：Swagger (访问路径 devops/swagger)

## 根本原因

`Program.cs` 缺少 Blazor Server 的必要配置：
1. 没有注册 Blazor 组件服务
2. 没有配置静态文件中间件
3. 没有映射 Blazor 组件路由
4. Swagger 路径未配置为 devops/swagger

## 修复内容

### 1. 添加 Blazor 服务注册
```csharp
// Add Blazor Server services
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
```

### 2. 配置中间件管道
```csharp
app.UseHttpsRedirection();
app.UseStaticFiles();        // 添加静态文件支持
app.UseAntiforgery();        // 添加防伪造支持
```

### 3. 映射 Blazor 组件路由
```csharp
// Map Blazor components
app.MapRazorComponents<PlantHealthCheck.Components.App>()
    .AddInteractiveServerRenderMode();
```

### 4. 更新 Swagger 路径为 devops/swagger
```csharp
app.UseSwagger(c =>
{
    c.RouteTemplate = "devops/swagger/{documentName}/swagger.json";
});
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/devops/swagger/v1/swagger.json", "Plant Health Check API v1");
    c.RoutePrefix = "devops/swagger";
});
```

## 应用架构

修复后的应用支持以下路由：

### 前端 Blazor 页面
- **主页**: `http://localhost:5291/` 或 `https://localhost:7289/`
- **计数器**: `/counter`
- **天气**: `/weather`
- 其他页面根据 Components/Pages 中的组件定义

### 后端 API
- **认证登录**: `POST /api/auth/login`
- 其他 API 端点: `/api/[controller]/[action]`

### API 文档 (Swagger)
- **Swagger UI**: `http://localhost:5291/devops/swagger`
- **Swagger JSON**: `http://localhost:5291/devops/swagger/v1/swagger.json`

## 如何运行

### 方式一：使用 Docker Compose（推荐）

1. 启动依赖服务（MySQL, Redis, RabbitMQ）：
```bash
docker-compose up -d
```

2. 等待服务启动完成后，运行应用：
```bash
dotnet run
```

3. 访问应用：
- 前端: http://localhost:5291
- Swagger: http://localhost:5291/devops/swagger

### 方式二：手动配置依赖

1. 安装并启动 MySQL 8.0+
2. 安装并启动 Redis 6.0+
3. 安装并启动 RabbitMQ 3.8+

4. 更新 `appsettings.json` 中的连接配置

5. 运行数据库迁移：
```bash
dotnet ef database update
```

6. 运行应用：
```bash
dotnet run
```

## 验证修复

1. **验证前端访问**：
   - 打开浏览器访问 http://localhost:5291
   - 应该能看到"AI植物健康检测平台"首页

2. **验证 Swagger 访问**：
   - 访问 http://localhost:5291/devops/swagger
   - 应该能看到 API 文档界面

3. **验证 API 访问**：
   - 在 Swagger 中测试 `/api/auth/login` 接口
   - 或使用 curl/Postman 测试 API

## 技术细节

### 中间件顺序
正确的中间件顺序对于应用正常工作至关重要：

1. `UseSwagger` / `UseSwaggerUI` (仅开发环境)
2. `UseHttpsRedirection`
3. `UseStaticFiles`
4. `UseAntiforgery`
5. `UseCors`
6. `UseAuthentication`
7. `UseAuthorization`
8. `MapControllers` (API 路由)
9. `MapRazorComponents` (Blazor 路由)

### 路由优先级
- API 路由 (`/api/*`) 优先
- Blazor 组件处理其他所有路由
- 404 页面由 `NotFound.razor` 处理

## 注意事项

1. **生产环境配置**：
   - 修改 JWT SecretKey 为至少 64 字符的强密钥
   - 更新数据库密码
   - 配置正确的 CORS 策略（不要使用 AllowAnyOrigin）
   - 使用环境变量存储敏感信息

2. **Swagger 安全**：
   - 当前 Swagger 仅在开发环境启用
   - 生产环境不会暴露 Swagger UI

3. **依赖服务**：
   - 应用启动需要 MySQL, Redis 和 RabbitMQ 正常运行
   - 可以使用 docker-compose 快速启动所有依赖
