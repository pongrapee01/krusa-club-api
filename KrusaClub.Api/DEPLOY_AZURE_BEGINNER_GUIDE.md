# Deploy `KrusaClub.Api` to Microsoft Azure (Beginner Guide)

คู่มือนี้เขียนสำหรับคนที่ "ไม่เคย deploy มาก่อน" โดยจะพาไปทีละขั้นแบบละเอียด ตั้งแต่เตรียมเครื่องจน API ออนไลน์ใช้งานได้จริง

---

## 1) สิ่งที่เราจะได้หลังทำเสร็จ

- API `.NET 8` รันบน Azure App Service
- มี URL ออนไลน์ เช่น `https://krusa-club-api.azurewebsites.net`
- ตั้งค่า Supabase ผ่าน Azure Environment Variables
- ทดสอบ endpoint ได้จาก browser / Postman

---

## 2) สิ่งที่ต้องมี (Prerequisites)

ให้เตรียมให้ครบก่อนเริ่ม:

1. บัญชี Azure (มีสิทธิ์สร้าง Resource)
2. ติดตั้ง .NET SDK 8 (`dotnet --version`)
3. โค้ดโปรเจกต์นี้อยู่ในเครื่องแล้ว
4. ค่าจาก Supabase:
   - `ConnectionString`
   - `JwtSigningMode` (เช่น `Jwks`)
   - `JwtIssuer` (เช่น `https://<project-ref>.supabase.co/auth/v1`)
   - `JwtAudience` (ปกติ `authenticated`)
5. (แนะนำ) ติดตั้ง Azure CLI: `az --version`

> ถ้ายังไม่มี Azure CLI ก็ deploy ผ่าน Azure Portal ได้ (มีวิธีให้ด้านล่าง)

---

## 3) ตรวจโปรเจกต์ก่อน deploy

เปิด terminal ที่โฟลเดอร์ `KrusaClub.Api` แล้วรัน:

```powershell
dotnet restore
dotnet build
```

ถ้า build ไม่ผ่าน ให้แก้ error ให้ผ่านก่อน deploy

---

## 4) สร้าง Azure App Service (วิธีง่ายสุดผ่าน Portal)

### 4.1 สร้าง Resource Group

1. เข้า [Azure Portal](https://portal.azure.com)
2. ค้นหา `Resource groups`
3. กด `Create`
4. เลือก Subscription
5. ตั้งชื่อ เช่น `rg-krusa-club-api`
6. เลือก Region (แนะนำใกล้ผู้ใช้ เช่น Southeast Asia)
7. กด `Review + create` -> `Create`

### 4.2 สร้าง App Service

1. ใน Portal ค้นหา `App Services`
2. กด `Create`
3. กรอกค่า:
   - **Subscription**: ของคุณ
   - **Resource Group**: `rg-krusa-club-api`
   - **Name**: เช่น `krusa-club-api` (ต้องไม่ซ้ำทั่วโลก)
   - **Publish**: `Code`
   - **Runtime stack**: `.NET 8 (LTS)`
   - **Operating System**: `Linux` (แนะนำ)
   - **Region**: เดียวกับ Resource Group
4. สร้าง/เลือก App Service Plan (เช่น `B1` สำหรับเริ่มต้น)
5. กด `Review + create` -> `Create`

รอจน deploy resource เสร็จ

---

## 5) ตั้งค่า Environment Variables บน Azure

ไปที่ App Service ของคุณ:

1. เมนูซ้าย -> `Settings` -> `Environment variables`
2. กด `Add` แล้วเพิ่มค่าเหล่านี้:

- `Supabase__ConnectionString` = `<ค่า connection string จาก Supabase>`
- `Supabase__JwtSigningMode` = `Jwks`
- `Supabase__JwtIssuer` = `https://<project-ref>.supabase.co/auth/v1`
- `Supabase__JwtAudience` = `authenticated`

> ใน .NET ใช้ `__` (double underscore) แทน `:` เช่น `Supabase:ConnectionString` -> `Supabase__ConnectionString`

3. กด `Apply` แล้ว `Save`
4. Azure จะ restart app อัตโนมัติ

---

## 6) แก้ CORS ให้ frontend เรียกได้

ในโค้ด `Program.cs` ตอนนี้อนุญาตแค่ `http://localhost:5173`

เมื่อขึ้น production ให้เพิ่ม domain frontend จริงของคุณ เช่น:

- `https://your-frontend.vercel.app`
- หรือ domain จริงของบริษัท

ตัวอย่างแนวคิด:

```csharp
policy.WithOrigins(
    "http://localhost:5173",
    "https://your-frontend-domain.com"
)
.AllowAnyHeader()
.AllowAnyMethod();
```

จากนั้น commit และ deploy ใหม่

---

## 7) Deploy โค้ดขึ้น Azure (2 วิธี)

## วิธี A: Deploy จาก local ด้วย ZIP (เหมาะกับมือใหม่)

### 7.1 Publish ไฟล์

ใน terminal:

```powershell
dotnet publish -c Release -o .\publish
```

### 7.2 ZIP โฟลเดอร์ publish

```powershell
Compress-Archive -Path .\publish\* -DestinationPath .\publish.zip -Force
```

### 7.3 Upload ZIP ไป App Service

1. ไปที่ App Service ใน Azure Portal
2. เมนู `Deployment Center`
3. เลือกแบบ `Zip Deploy` (หรือใช้ Advanced Tools/Kudu)
4. Upload ไฟล์ `publish.zip`
5. รอจนเสร็จ

---

## วิธี B: Deploy ด้วย GitHub Actions (แนะนำระยะยาว)

ถ้าคุณใช้ GitHub:

1. ไปที่ App Service -> `Deployment Center`
2. เลือก Source = `GitHub`
3. Authorize GitHub
4. เลือก repo + branch
5. Azure จะสร้าง workflow ให้อัตโนมัติ
6. ทุกครั้งที่ push branch ที่ตั้งไว้ จะ deploy อัตโนมัติ

---

## 8) ทดสอบหลัง deploy

สมมติ URL คือ `https://krusa-club-api.azurewebsites.net`

1. ทดสอบ API ทำงาน:
   - `GET https://krusa-club-api.azurewebsites.net/api/health/db`
2. ทดสอบ endpoint auth:
   - `GET https://krusa-club-api.azurewebsites.net/api/menus/me`
   - ใส่ Header `Authorization: Bearer <token>`

ถ้าได้ `200` แปลว่า deploy สำเร็จ

---

## 9) ดู log เมื่อมีปัญหา

ที่ App Service:

1. `Monitoring` -> `Log stream` (ดู log realtime)
2. `Monitoring` -> `App Service logs` (เปิด Application logging ก่อน)
3. ถ้าต้องการ shell เข้าเครื่อง ให้ใช้ `Advanced Tools (Kudu)`

---

## 10) ปัญหาที่เจอบ่อยและวิธีแก้

### 10.1 `401 Unauthorized`

- token หมดอายุ
- `JwtIssuer` / `JwtAudience` ไม่ตรง
- ไม่ได้ส่ง `Authorization` header

### 10.2 CORS error จาก browser

- ยังไม่ได้เพิ่มโดเมน frontend ใน `WithOrigins(...)`

### 10.3 `"Exception while reading from stream"`

- ตรวจ `Supabase__ConnectionString` ถูกต้องไหม
- ถ้าใช้ Supabase pooler ให้ใช้ config ล่าสุดใน `Program.cs`
- ดู network/firewall หรือปัญหา connectivity ชั่วคราว

### 10.4 App start ไม่ขึ้น

- ตรวจ Environment Variables ใน Azure ว่าครบ
- ดู `Log stream` ทันทีหลัง restart

---

## 11) Checklist ก่อน go-live

- [ ] Endpoint `/api/health/db` ตอบ `200`
- [ ] Endpoint ที่ใช้จริงตอบถูกต้อง
- [ ] CORS ครบทุก domain ที่ต้องใช้
- [ ] ไม่มี secret อยู่ใน source code
- [ ] ใช้ Environment Variables แทน hardcode ค่า sensitive
- [ ] เปิด monitor/log ไว้พร้อมใช้งาน

---

## 12) คำแนะนำเพิ่มเติม (สำคัญ)

- ไม่ควรเก็บ secret จริงไว้ใน `appsettings.json` ใน repo
- ควรใช้ Azure App Settings หรือ Azure Key Vault
- เมื่อระบบเริ่มมีผู้ใช้มากขึ้น ค่อยขยับ App Service Plan ให้เหมาะสม

---

ถ้าต้องการ ผมช่วยต่อให้ได้อีก:
- สร้างไฟล์ `GitHub Actions workflow` สำหรับ deploy อัตโนมัติให้เลย
- แยก `Development` / `Production` config แบบปลอดภัย
