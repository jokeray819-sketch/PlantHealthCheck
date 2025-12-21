# API 使用指南

## 概述

Plant Health Check 后端 API 提供了完整的用户认证功能。

## 基础信息

- **Base URL**: `https://localhost:5001` (开发环境)
- **API Version**: v1
- **认证方式**: JWT Bearer Token

## 接口文档

### 1. 用户登录

登录接口用于验证用户凭据并返回 JWT token。

**端点**: `POST /api/Auth/login`

**请求头**:
```
Content-Type: application/json
```

**请求体**:
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**成功响应** (200 OK):
```json
{
  "success": true,
  "message": "登录成功",
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxIiwidW5pcXVlX25hbWUiOiJhZG1pbiIsImVtYWlsIjoiYWRtaW5AcGxhbnRoZWFsdGhjaGVjay5jb20iLCJqdGkiOiI5ZjM0NWU4Yy0xMjM0LTU2NzgtOWFiYy1kZWYwMTIzNDU2NzgiLCJleHAiOjE3MDM5NzEyMDAsImlzcyI6IlBsYW50SGVhbHRoQ2hlY2siLCJhdWQiOiJQbGFudEhlYWx0aENoZWNrQ2xpZW50In0.signature",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@planthealthcheck.com"
  }
}
```

**失败响应** (401 Unauthorized):
```json
{
  "success": false,
  "message": "用户名或密码错误",
  "token": null,
  "user": null
}
```

**其他错误响应**:

- 用户名和密码不能为空:
```json
{
  "success": false,
  "message": "用户名和密码不能为空"
}
```

- 账户已被禁用:
```json
{
  "success": false,
  "message": "账户已被禁用"
}
```

- 服务器错误 (500):
```json
{
  "success": false,
  "message": "登录过程中发生错误"
}
```

## 使用示例

### cURL

```bash
# 登录
curl -X POST https://localhost:5001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }' \
  -k  # 开发环境忽略 SSL 证书验证

# 保存 token
TOKEN=$(curl -X POST https://localhost:5001/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"admin123"}' \
  -k -s | jq -r '.token')

# 使用 token 访问受保护的接口 (示例)
curl -X GET https://localhost:5001/api/protected-endpoint \
  -H "Authorization: Bearer $TOKEN" \
  -k
```

### JavaScript/TypeScript (Fetch API)

```javascript
// 登录
async function login(username, password) {
  const response = await fetch('https://localhost:5001/api/Auth/login', {
    method: 'POST',
    headers: {
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ username, password })
  });
  
  const data = await response.json();
  
  if (data.success) {
    // 保存 token
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
    return data;
  } else {
    throw new Error(data.message);
  }
}

// 使用 token 请求
async function fetchProtectedData() {
  const token = localStorage.getItem('token');
  
  const response = await fetch('https://localhost:5001/api/protected-endpoint', {
    headers: {
      'Authorization': `Bearer ${token}`
    }
  });
  
  return await response.json();
}

// 使用示例
try {
  const result = await login('admin', 'admin123');
  console.log('登录成功:', result);
} catch (error) {
  console.error('登录失败:', error.message);
}
```

### C# (.NET Client)

```csharp
using System.Net.Http.Json;

public class LoginRequest
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string Token { get; set; }
    public UserInfo User { get; set; }
}

public class UserInfo
{
    public long Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}

// 登录
var client = new HttpClient();
var request = new LoginRequest 
{ 
    Username = "admin", 
    Password = "admin123" 
};

var response = await client.PostAsJsonAsync(
    "https://localhost:5001/api/Auth/login", 
    request);

if (response.IsSuccessStatusCode)
{
    var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
    if (result.Success)
    {
        // 保存 token
        var token = result.Token;
        
        // 使用 token 进行后续请求
        client.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }
}
```

### Python (requests)

```python
import requests
import json

# 登录
def login(username, password):
    url = "https://localhost:5001/api/Auth/login"
    payload = {
        "username": username,
        "password": password
    }
    headers = {
        "Content-Type": "application/json"
    }
    
    response = requests.post(url, json=payload, headers=headers, verify=False)
    data = response.json()
    
    if data['success']:
        return data['token']
    else:
        raise Exception(data['message'])

# 使用示例
try:
    token = login('admin', 'admin123')
    print(f"登录成功，Token: {token}")
    
    # 使用 token 访问受保护的接口
    headers = {
        "Authorization": f"Bearer {token}"
    }
    response = requests.get("https://localhost:5001/api/protected-endpoint", 
                           headers=headers, verify=False)
except Exception as e:
    print(f"登录失败: {str(e)}")
```

## JWT Token 说明

### Token 结构

JWT token 包含以下声明 (Claims):

- `sub`: 用户 ID
- `unique_name`: 用户名
- `email`: 邮箱
- `jti`: Token 唯一标识符
- `exp`: 过期时间
- `iss`: 签发者 (PlantHealthCheck)
- `aud`: 接收者 (PlantHealthCheckClient)

### Token 有效期

默认 24 小时，可在 `appsettings.json` 中配置。

### Token 刷新

当前版本不支持 token 刷新，过期后需要重新登录。后续版本将添加刷新 token 功能。

## 错误码说明

| HTTP 状态码 | 说明 |
|------------|------|
| 200 | 请求成功 |
| 400 | 请求参数错误 |
| 401 | 未授权 (认证失败) |
| 403 | 禁止访问 (权限不足) |
| 404 | 资源不存在 |
| 500 | 服务器内部错误 |

## 测试账号

开发环境提供以下测试账号：

| 用户名 | 密码 | 邮箱 | 说明 |
|--------|------|------|------|
| admin | admin123 | admin@planthealthcheck.com | 管理员账号 |
| testuser | test123 | test@planthealthcheck.com | 测试账号 |

## Swagger UI

访问 `https://localhost:5001` 可以查看完整的交互式 API 文档。

### 在 Swagger 中使用认证

1. 点击右上角的 "Authorize" 按钮
2. 输入: `Bearer {your-token}`
3. 点击 "Authorize"
4. 现在可以测试需要认证的接口

## 最佳实践

1. **Token 存储**: 不要在 localStorage 中存储敏感 token，生产环境建议使用 httpOnly cookie
2. **HTTPS**: 生产环境必须使用 HTTPS
3. **Token 过期处理**: 实现自动刷新或重定向到登录页面
4. **错误处理**: 统一处理 401 错误，提示用户重新登录
5. **安全性**: 不要在客户端硬编码密码或密钥

## 开发调试

### 查看请求日志

应用会在控制台输出详细的日志信息：

```bash
dotnet run --urls="https://localhost:5001"
```

### 启用详细日志

在 `appsettings.Development.json` 中设置：

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Debug"
    }
  }
}
```

## 后续功能

- [ ] 用户注册
- [ ] 密码重置
- [ ] Token 刷新
- [ ] 用户权限管理
- [ ] 用户信息修改
- [ ] 多因素认证 (MFA)
