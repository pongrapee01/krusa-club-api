# School Web Application Project Plan

## Overview
This project is a web application for teachers and students to manage classroom activities, assignments, educational games, and progress tracking.

### Goals
- Support around 150 students
- Allow teachers to create and manage assignments
- Allow students to play games and submit homework
- Track learning progress and scores
- Use a modern, scalable, and cost-effective stack

## Recommended Tech Stack

### Frontend
- React
- TypeScript
- Vite
- React Router
- TanStack Query
- Tailwind CSS
- shadcn/ui
- React Hook Form
- Zod

### Backend
- ASP.NET Core 8 Web API
- Npgsql + Dapper (direct PostgreSQL; EF Core optional if you prefer migrations/ORM later)
- Microsoft.AspNetCore.Authentication.JwtBearer (validate Supabase-issued JWTs)
- FluentValidation (recommended, not wired yet)
- Serilog (recommended, not wired yet)
- Swagger/OpenAPI

### Platform Services
- Supabase Auth
- Supabase PostgreSQL
- Supabase Storage

## Architecture

```text
[ React Web App ]
   |
   | HTTPS + Authorization: Bearer <Supabase access_token>
   v
[ ASP.NET Core Web API ]
   |
   | Npgsql + Dapper
   v
[ Supabase PostgreSQL ]

[ Supabase Auth ] <---- React login/session
[ Supabase Storage ] <---- file upload/download
```

## Core Architecture Rules
- React handles UI and client-side interactions
- Supabase Auth handles authentication
- .NET API handles business logic and authorization
- PostgreSQL is the source of truth
- React should not directly handle critical business queries against the database

---

## Next steps after Supabase registration (KrusaClub.Api)

ทำตามลำดับนี้หลังสร้างโปรเจกต์ Supabase แล้ว:

1. **เก็บค่าจาก Supabase Dashboard**
   - **Database connection string**: `Project Settings` → `Database` → URI หรือสร้าง connection string เอง (Host, DB name `postgres`, user `postgres`, password, port `5432`, SSL เปิดกับ production)
   - **JWT Secret**: `Project Settings` → `API` → **JWT Secret** (ใช้เป็นค่าเดียวกับที่ Supabase ใช้ sign access token — ไม่ใช่ anon/service role key)
   - **Project URL / anon key**: ใช้ฝั่ง React เท่านั้น (ไม่ใส่ใน repo ฝั่ง API ถ้าไม่จำเป็น)

2. **ใส่ค่าใน API**
   - แก้ `KrusaClub.Api/appsettings.json` (หรือ `appsettings.Development.json` สำหรับเครื่องตัวเอง) ในส่วน `Supabase`:
     - `ConnectionString` — string เชื่อม Postgres ของโปรเจกต์นั้น
     - `JwtSecret` — จากขั้นตอนข้างบน
     - `JwtIssuer` / `JwtAudience` — ค่าเริ่มต้นในโปรเจกต์คือ `supabase` และ `authenticated` (ตรงกับ JWT ของ user ที่ login ผ่าน Supabase Auth โดยทั่วไป)
   - **Production**: ใช้ environment variables / secrets (`Supabase__ConnectionString`, `Supabase__JwtSecret`, …) แทนการ commit รหัสผ่านลง git

3. **เตรียม schema ใน Postgres (ถ้ายังไม่มี)**
   - ตัวอย่างใน repo ใช้ตาราง `public.profiles` และคอลัมน์ `id` ตรงกับ `auth.users.id` (UUID ของ user)
   - สร้างตาราง/นโยบาย RLS ใน Supabase ตามที่ต้องการ — ถ้า API เชื่อมด้วย role `postgres` + connection string แบบเต็ม จะข้าม RLS ได้ตามสิทธิ์ของ role นั้น (ออกแบบสิทธิ์และ connection user ให้เหมาะกับ production)

4. **รัน API**
   - จาก root: `dotnet run --project KrusaClub.Api`
   - เปิด Swagger ตาม URL ที่ console แสดง (เช่น `https://localhost:7xxx/swagger`)

5. **ทดสอบกับ JWT จริง**
   - ใน React: หลัง login ด้วย Supabase ให้ส่ง `session.access_token` ใน header `Authorization: Bearer <token>`
   - เรียก `GET /api/me/id` — ควรได้ JSON ที่มี `userId` ตรงกับ `sub` ใน JWT
   - เรียก `GET /api/me/profile` — ต้องมีแถวใน `public.profiles` ที่ `id` = user นั้น มิฉะนั้นจะได้ 404

6. **CORS (เมื่อ React คนละ origin)**
   - เพิ่ม `AddCors` + `UseCors` ใน `Program.cs` ให้ตรงกับ URL ของ frontend (ทำเมื่อเริ่มยิงจาก browser จริง)

รายละเอียดสั้นๆ ใน repo: `KrusaClub.Api/README.md`