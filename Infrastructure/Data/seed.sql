-- 初始化数据库脚本
-- Plant Health Check System

-- 创建数据库（如果不存在）
CREATE DATABASE IF NOT EXISTS plant_health_check 
CHARACTER SET utf8mb4 
COLLATE utf8mb4_unicode_ci;

USE plant_health_check;

-- 应用 EF Core 迁移（通过 Infrastructure/Data/init.sql）

-- 插入测试用户
-- 用户名: admin
-- 密码: admin123
INSERT INTO users (username, password_hash, email, phone, created_at, is_active)
VALUES (
  'admin',
  '$2a$11$hY7AXOTPVCeC80SzHdLuveCNChdUBWHVZzwXkAcXMZyQFkINwwaoW',
  'admin@planthealthcheck.com',
  '13800138000',
  UTC_TIMESTAMP(),
  1
) ON DUPLICATE KEY UPDATE username=username;

-- 用户名: testuser
-- 密码: test123
INSERT INTO users (username, password_hash, email, phone, created_at, is_active)
VALUES (
  'testuser',
  '$2a$11$TMcdaEk6Z77kIM5DIk9KZunj46MAC9O8zzBSsINTFlgYXpCkVhOHO',
  'test@planthealthcheck.com',
  '13900139000',
  UTC_TIMESTAMP(),
  1
) ON DUPLICATE KEY UPDATE username=username;

-- 显示插入的用户
SELECT id, username, email, created_at, is_active FROM users;
