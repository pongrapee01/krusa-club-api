# Database Design: Menu + Permission (Role-based, Flexible Add/Remove)

เอกสารนี้ออกแบบฐานข้อมูลสำหรับระบบเมนูที่มี:
- Top menu
- Side menu
- Sub menu (ภายใต้ side menu)
- การกำหนดสิทธิ์ตาม Role ว่าเข้าถึงเมนูใดได้บ้าง
- รองรับการเพิ่ม/ลดเมนูและสิทธิ์ได้ง่ายในอนาคต

Tech stack ที่รองรับ: `.NET 8 + Supabase(PostgreSQL) + Dapper`

---

## 1) แนวคิดการออกแบบ

ใช้แนวทาง **menu node + relationship table + role permission mapping**:

- เก็บเมนูทั้งหมดในตารางเดียว (`app_menu`) แต่ไม่ผูก top/side ด้วย parent ตรงๆ
- ใช้ `menu_type` แยก top / side / sub
- ใช้ตารางสัมพันธ์ `top_side_menu_map` เพื่อรองรับ **1 side menu อยู่ได้หลาย top menu**
- ใช้ `parent_id` เฉพาะ `sub -> side` เท่านั้น
- ใช้ `role_menu_permission` เป็น many-to-many ระหว่าง role กับ menu
- การแสดงผล:
  - Top menu: ดึงจาก `menu_type = top`
  - Side menu: ดึงผ่าน `top_side_menu_map`
  - Sub menu: ดึงจาก `parent_id` ของ side menu

ข้อดี:
- ขยายง่าย ไม่ต้องเพิ่มคอลัมน์เวลามีเมนูใหม่
- ไม่ lock โครงสร้างตายตัว
- ใช้ query เดียวคืน tree ให้ frontend ได้

---

## 2) ตารางหลักที่แนะนำ

### 2.1 `app_role`
- เก็บ role ของระบบ
- เช่น `super_admin`, `teacher`, `student`, `guardian`

```sql
create table if not exists public.app_role (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,              -- machine key
    name text not null,                     -- display name
    description text null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);
```

### 2.2 `app_menu`
- เก็บ menu node ทั้งหมด
- `top` และ `side` จะมี `parent_id = null`
- `sub` เท่านั้นที่อ้าง `parent_id -> side`
- ความสัมพันธ์ `top -> side` แยกไปอีกตาราง

```sql
do $$
begin
    if not exists (select 1 from pg_type where typname = 'menu_type') then
        create type public.menu_type as enum ('top', 'side', 'sub');
    end if;
end$$;

create table if not exists public.app_menu (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,                 -- เช่น TOP_DASHBOARD, SIDE_STUDENT, SUB_ASSIGNMENT_LIST
    name text not null,
    menu_type public.menu_type not null,
    parent_id uuid null references public.app_menu(id) on delete cascade,
    route_path text null,                      -- เช่น /dashboard, /students, /students/assignments
    icon text null,                            -- ชื่อ icon key
    sort_order int not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint chk_menu_parent_type check (
        (menu_type in ('top', 'side') and parent_id is null) or
        (menu_type = 'sub' and parent_id is not null)
    )
);

create index if not exists idx_app_menu_parent_sort on public.app_menu(parent_id, sort_order);
create index if not exists idx_app_menu_type_active on public.app_menu(menu_type, is_active);
```

### 2.3 `top_side_menu_map`
- ตาราง map ความสัมพันธ์ top menu กับ side menu
- รองรับ side menu เดียวอยู่หลาย top menu

```sql
create table if not exists public.top_side_menu_map (
    top_menu_id uuid not null references public.app_menu(id) on delete cascade,
    side_menu_id uuid not null references public.app_menu(id) on delete cascade,
    sort_order int not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    primary key (top_menu_id, side_menu_id)
);

create index if not exists idx_top_side_menu_map_side on public.top_side_menu_map(side_menu_id, sort_order);
```

> หมายเหตุ: แนะนำบังคับ `top_menu_id` ต้องเป็น `menu_type=top` และ `side_menu_id` ต้องเป็น `menu_type=side` ผ่าน trigger/function

### 2.4 `role_menu_permission`
- ตารางสิทธิ์ของ role ต่อเมนู
- อนาคตขยาย action-level ได้ (view/create/update/delete)

```sql
create table if not exists public.role_menu_permission (
    role_id uuid not null references public.app_role(id) on delete cascade,
    menu_id uuid not null references public.app_menu(id) on delete cascade,
    can_view boolean not null default true,
    created_at timestamptz not null default now(),
    created_by uuid null,
    primary key (role_id, menu_id)
);

create index if not exists idx_role_menu_permission_menu on public.role_menu_permission(menu_id);
```

### 2.5 `user_role`
- mapping ผู้ใช้ (Supabase `auth.users.id`) กับ role

```sql
create table if not exists public.user_role (
    user_id uuid not null, -- supabase auth.users.id
    role_id uuid not null references public.app_role(id) on delete cascade,
    assigned_at timestamptz not null default now(),
    assigned_by uuid null,
    primary key (user_id, role_id)
);

create index if not exists idx_user_role_role on public.user_role(role_id);
```

---

## 3) กติกาธุรกิจที่ควรบังคับ

1. ถ้า role มีสิทธิ์ `sub menu` ควรมีสิทธิ์ `side` parent ด้วย และ side นั้นต้องถูก map กับอย่างน้อย 1 top menu  
   - ทำได้ 2 แบบ:
     - บังคับใน service layer (.NET) เวลา grant permission
     - หรือ view/query ช่วยเติม parent อัตโนมัติ

2. ห้ามลบเมนู hard-delete ถ้าเคยใช้งานจริง  
   - แนะนำ soft delete ด้วย `is_active=false`

3. `code` ของ role/menu ไม่ควรเปลี่ยนบ่อย  
   - ใช้ `code` เป็น stable key ในโค้ด frontend/backend

4. sort menu ด้วย `sort_order` และ fallback ด้วย `name`

---

## 4) Query สำคัญสำหรับ Dapper

### 4.1 ดึงเมนูทั้งหมดที่ user ใช้ได้ (รองรับ side อยู่หลาย top)

```sql
with user_roles as (
    select ur.role_id
    from public.user_role ur
    where ur.user_id = @UserId
),
allowed_menu as (
    select distinct m.id, m.parent_id, m.code, m.name, m.menu_type, m.route_path, m.icon, m.sort_order
    from public.role_menu_permission rp
    join user_roles ur on ur.role_id = rp.role_id
    join public.app_menu m on m.id = rp.menu_id
    where rp.can_view = true
      and m.is_active = true
),
allowed_side as (
    select *
    from allowed_menu
    where menu_type = 'side'
),
allowed_sub as (
    select *
    from allowed_menu
    where menu_type = 'sub'
),
allowed_top as (
    -- top ที่ user มีสิทธิ์ตรง
    select *
    from allowed_menu
    where menu_type = 'top'
    union
    -- top ที่โยงกับ side ที่ user เข้าถึงได้
    select distinct t.*
    from public.top_side_menu_map ts
    join public.app_menu t on t.id = ts.top_menu_id
    join allowed_side s on s.id = ts.side_menu_id
    where ts.is_active = true
      and t.is_active = true
),
resolved_sub as (
    -- sub ที่มีสิทธิ์ตรง
    select *
    from allowed_sub
    union
    -- ถ้ามีสิทธิ์ side ให้เห็น sub ลูกทั้งหมด (ถ้าธุรกิจต้องการ)
    select distinct sm.*
    from public.app_menu sm
    join allowed_side s on sm.parent_id = s.id
    where sm.menu_type = 'sub'
      and sm.is_active = true
),
result_nodes as (
    select * from allowed_top
    union
    select * from allowed_side
    union
    select * from resolved_sub
)
select
    t.id as top_menu_id,
    t.code as top_code,
    t.name as top_name,
    t.sort_order as top_sort,
    s.id as side_menu_id,
    s.code as side_code,
    s.name as side_name,
    coalesce(ts.sort_order, s.sort_order) as side_sort,
    sub.id as sub_menu_id,
    sub.code as sub_code,
    sub.name as sub_name,
    sub.sort_order as sub_sort
from result_nodes t
left join public.top_side_menu_map ts on ts.top_menu_id = t.id and ts.is_active = true
left join result_nodes s on s.id = ts.side_menu_id and s.menu_type = 'side'
left join result_nodes sub on sub.parent_id = s.id and sub.menu_type = 'sub'
where t.menu_type = 'top'
order by top_sort, side_sort, sub_sort, top_name, side_name, sub_name;
```

> จุดเด่น: side menu เดียวสามารถโผล่ใต้หลาย top menu ได้ด้วย `top_side_menu_map`

### 4.2 เพิ่มสิทธิ์เมนูให้ role

```sql
insert into public.role_menu_permission (role_id, menu_id, can_view, created_by)
values (@RoleId, @MenuId, true, @AdminUserId)
on conflict (role_id, menu_id) do update
set can_view = excluded.can_view;
```

### 4.3 ถอนสิทธิ์เมนูจาก role

```sql
delete from public.role_menu_permission
where role_id = @RoleId
  and menu_id = @MenuId;
```

---

## 5) แนวทาง API ที่ควรมีใน .NET

- `GET /api/auth/me` -> ได้ `sub` (user id)
- `GET /api/menus/me` -> ดึงเมนูที่ user นี้เข้าถึงได้
- `GET /api/admin/roles` -> รายการ role
- `GET /api/admin/roles/{roleId}/menus` -> เมนูที่ role นี้เข้าถึง
- `POST /api/admin/roles/{roleId}/menus/{menuId}` -> grant
- `DELETE /api/admin/roles/{roleId}/menus/{menuId}` -> revoke

Repository pattern (Dapper):
- `IMenuRepository`
- `IRolePermissionRepository`
- `IUserRoleRepository`

---

## 6) รีวิวความเสี่ยงและข้อเสนอแนะ

1. **เสี่ยง mapping ไม่สอดคล้องกัน**  
   - side menu active แต่ไม่ได้ map กับ top ใดเลย
   - หรือ map ไป top ที่ inactive
   - แนะนำ service check หรือ nightly consistency query

2. **เสี่ยง role ซ้อนหลายตัวแล้วสิทธิ์เยอะเกิน**
   - ใช้ policy “allow by union” ชัดเจน
   - เพิ่มหน้า audit role permission diff

3. **RLS กับตารางสิทธิ์**
   - ถ้า API ใช้ connection role ที่ bypass RLS ให้ควบคุมด้วย business logic ใน API
   - ถ้าจะเปิด RLS: แยก policy admin กับ user-read-only ให้ชัด

4. **การ cache เมนู**
   - เมนูไม่เปลี่ยนบ่อย ควร cache ต่อ user 1-5 นาที (memory/distributed)
   - invalidate ตอน grant/revoke

---

## 7) ข้อสรุป

โครงสร้าง `app_menu + top_side_menu_map + app_role + role_menu_permission + user_role` เหมาะกับ requirement นี้ที่สุด:
- รองรับ Top/Side/Sub menu
- รองรับ side menu อยู่ได้หลาย top menu
- เพิ่มลดสิทธิ์ได้ยืดหยุ่น
- query ได้ตรงกับการใช้งานจริงของ React + .NET API
- ใช้กับ Supabase PostgreSQL และ Dapper ได้ตรงไปตรงมา

---

## 8) สถานะล่าสุดที่ทำแล้ว (Implementation Update)

เพื่อให้พร้อมใช้งานจริงในโปรเจกต์ ได้เพิ่ม SQL script แล้วดังนี้:

- `sql/00_menu_auth_schema.sql`
  - สร้าง enum `menu_type`
  - สร้างตาราง:
    - `app_role`
    - `app_menu`
    - `top_side_menu_map`
    - `role_menu_permission`
    - `user_role`
  - เพิ่ม indexes ที่จำเป็นสำหรับ query เมนู/สิทธิ์
  - เพิ่ม trigger validation ใน `top_side_menu_map`
    - `top_menu_id` ต้องเป็น `menu_type='top'`
    - `side_menu_id` ต้องเป็น `menu_type='side'`

- `sql/01_menu_auth_seed.sql`
  - seed role ตัวอย่าง: `super_admin`, `teacher`, `student`
  - seed menu ตัวอย่าง: top / side / sub
  - seed mapping `top -> side` (รวมกรณี side เดียวอยู่หลาย top)
  - seed role permissions ตาม role ตัวอย่าง

### ลำดับการรัน

1. รัน `sql/00_menu_auth_schema.sql`
2. รัน `sql/01_menu_auth_seed.sql`

### ผลลัพธ์ที่ควรได้หลัง seed

- มีโครงสร้างเมนูพร้อมทดสอบ UI navigation
- มี role และสิทธิ์พร้อมทดสอบ API
- ทดสอบกรณี side menu เดียวแสดงได้หลาย top menu ได้ทันที

### ข้อเสนอแนะขั้นถัดไป

- เพิ่ม `sql/02_menu_auth_queries.sql` (query เมนูตาม `@UserId` สำหรับ Dapper)
- เพิ่ม admin API สำหรับ grant/revoke (`role_menu_permission`)
- เพิ่ม audit log สำหรับการเปลี่ยนสิทธิ์ (ใครแก้, เมื่อไร, ก่อน/หลัง)

---

## 9) Revised Design (ตาม flow ล่าสุด)

จาก requirement ล่าสุด ให้ใช้ flow นี้เป็นหลัก:

1. User login สำเร็จ -> ได้ `user_id` (จาก JWT `sub`)
2. เอา `user_id` ไปหาใน `user_profile` -> ได้ `role_code`
3. เอา `role_code` ไปหา `role_menu` -> ได้เมนูที่ role เข้าถึงได้
4. โครง menu เป็นชั้นตายตัว:
   - `top menu` -> `side menu` -> `side sub menu`
5. เมนูที่หลาย role เห็นร่วมกัน ให้กำหนดสิทธิ์ที่ `top menu`

### Decision Update

- ยืนยันล่าสุด: **ยกเลิกเงื่อนไข "1 side menu อยู่ได้หลาย top menu"**
- โครงสร้างที่ใช้จริง: **strict tree** เท่านั้น -> `top -> side -> sub`
- การแชร์เมนูข้าม role ให้ทำผ่าน `role_menu` ที่ระดับ `top menu`

> แนวทางนี้เหมาะกับระบบที่ permission ต้องง่าย, config เร็ว, และคุมสิทธิ์เป็นกลุ่มใหญ่ตามหมวดเมนู

### 9.1 ตารางที่ใช้ (ฉบับย่อและตรง requirement)

ใช้ 3 ตารางหลักตามที่ขอ:
- `menu`
- `role`
- `role_menu`

และมี `user_profile` (existing) สำหรับหา role ของ user

#### `public.role`

```sql
create table if not exists public.role (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,      -- SUPER_ADMIN, TEACHER, STUDENT
    name text not null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);
```

#### `public.menu`

```sql
do $$
begin
    if not exists (select 1 from pg_type where typname = 'menu_level') then
        create type public.menu_level as enum ('top', 'side', 'sub');
    end if;
end$$;

create table if not exists public.menu (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,
    name text not null,
    menu_level public.menu_level not null,
    parent_id uuid null references public.menu(id) on delete cascade,
    route_path text null,
    icon text null,
    sort_order int not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint chk_menu_hierarchy check (
        (menu_level = 'top' and parent_id is null) or
        (menu_level in ('side','sub') and parent_id is not null)
    )
);

create index if not exists idx_menu_parent_sort on public.menu(parent_id, sort_order);
create index if not exists idx_menu_level_active on public.menu(menu_level, is_active);
```

> กติกา business ที่ควรบังคับใน service layer:
> - `side.parent_id` ต้องชี้ไป `top`
> - `sub.parent_id` ต้องชี้ไป `side`

#### `public.role_menu`

```sql
create table if not exists public.role_menu (
    role_id uuid not null references public.role(id) on delete cascade,
    menu_id uuid not null references public.menu(id) on delete cascade,
    can_view boolean not null default true,
    created_at timestamptz not null default now(),
    created_by uuid null,
    primary key (role_id, menu_id)
);

create index if not exists idx_role_menu_menu on public.role_menu(menu_id);
```

### 9.2 วิธีคิด permission (แนะนำ)

เพื่อให้ config ง่ายตาม requirement:

- `role_menu` ให้ grant ที่ `top menu` เป็นหลัก
- เมื่อ role ได้ top ใด ให้เห็น `side + sub` ทั้งหมดภายใต้ top นั้น
- ถ้าต้อง granular เพิ่มทีหลัง ค่อยเปิดให้ grant ระดับ side/sub เพิ่มได้

ผลลัพธ์:
- เพิ่ม role ใหม่เร็ว (เลือก top menus ไม่กี่รายการ)
- เมนูที่ใช้ร่วมกันข้ามหลาย role จัดการง่ายมาก (grant top เดียวกัน)

### 9.3 Query ดึงเมนูตาม user (Dapper)

```sql
with my_role as (
    select r.id as role_id, r.code as role_code
    from public.user_profile up
    join public.role r on r.code = up.role_code
    where up.user_id = @UserId
      and r.is_active = true
    limit 1
),
allowed_top as (
    select m.*
    from public.role_menu rm
    join my_role mr on mr.role_id = rm.role_id
    join public.menu m on m.id = rm.menu_id
    where rm.can_view = true
      and m.menu_level = 'top'
      and m.is_active = true
),
allowed_side as (
    select s.*
    from public.menu s
    join allowed_top t on s.parent_id = t.id
    where s.menu_level = 'side'
      and s.is_active = true
),
allowed_sub as (
    select sb.*
    from public.menu sb
    join allowed_side s on sb.parent_id = s.id
    where sb.menu_level = 'sub'
      and sb.is_active = true
)
select
    t.id as top_id, t.code as top_code, t.name as top_name, t.sort_order as top_sort,
    s.id as side_id, s.code as side_code, s.name as side_name, s.sort_order as side_sort,
    sb.id as sub_id, sb.code as sub_code, sb.name as sub_name, sb.sort_order as sub_sort
from allowed_top t
left join allowed_side s on s.parent_id = t.id
left join allowed_sub sb on sb.parent_id = s.id
order by t.sort_order, s.sort_order, sb.sort_order, t.name, s.name, sb.name;
```

### 9.4 วิธี config ใน Database (Step-by-step)

1. ลง master data role ใน `role`
2. ลง `menu` แบบ tree:
   - top ก่อน
   - side (parent = top)
   - sub (parent = side)
3. mapping role กับ top menu ใน `role_menu`
4. ใส่ `role_code` ใน `user_profile`
5. ตอน user login:
   - API อ่าน `sub` -> query ตาม SQL ด้านบน -> คืน menu tree ให้ frontend

### 9.5 ตัวอย่าง config สั้นๆ

```sql
-- role
insert into public.role(code, name) values ('TEACHER','Teacher')
on conflict (code) do nothing;

-- top
insert into public.menu(code, name, menu_level, sort_order)
values ('TOP_ACADEMIC','Academic','top',10)
on conflict (code) do nothing;

-- side
insert into public.menu(code, name, menu_level, parent_id, sort_order)
select 'SIDE_COURSE','Courses','side', id, 10
from public.menu where code='TOP_ACADEMIC'
on conflict (code) do nothing;

-- sub
insert into public.menu(code, name, menu_level, parent_id, sort_order)
select 'SUB_COURSE_LIST','Course List','sub', id, 10
from public.menu where code='SIDE_COURSE'
on conflict (code) do nothing;

-- grant top to role
insert into public.role_menu(role_id, menu_id, can_view)
select r.id, m.id, true
from public.role r
join public.menu m on m.code='TOP_ACADEMIC'
where r.code='TEACHER'
on conflict (role_id, menu_id) do update set can_view=excluded.can_view;
```

### 9.6 คำแนะนำเชิงระบบ

- ใช้ `code` เป็น stable key (อย่าเปลี่ยนบ่อย)
- ใช้ `is_active=false` แทนลบจริง
- แยก endpoint admin:
  - จัดการ role
  - จัดการ menu tree
  - grant/revoke top menus ต่อ role
- cache menu ตาม `user_id` 1-5 นาที และ clear cache ตอนแก้สิทธิ์

### 9.7 หมายเหตุการย้ายจากดีไซน์ก่อนหน้า

ถ้าจะใช้ revised design นี้เป็นหลัก:
- ตาราง `top_side_menu_map` ไม่จำเป็นแล้ว (เพราะกลับไป strict tree)
- ใช้ parent tree ตรงๆ ใน `menu` ตามลำดับ top->side->sub
- script เดิมสามารถเก็บไว้เป็น reference หรือ archive ได้



### 99. Database (ใช้เพื่อบอกรายละเอียดสำหรับ implement) ###
1. Master data
 1.1 CREATE TABLE titles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL,
    description TEXT,
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    modified_by UUID);
 1.2 CREATE TABLE roles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified_at TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    modified UUID);
 1.3 CREATE TABLE semesters (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    academic_year VARCHAR(4) NOT NULL, 
    term VARCHAR(2) NOT NULL, 
    start_date DATE NOT NULL,
    end_date DATE NOT NULL,
    is_current BOOLEAN DEFAULT false, 
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP,
    modified_by UUID);
 1.4 CREATE TABLE classes (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    name VARCHAR(50) NOT NULL, -- เช่น 'ป.4/1'
    level VARCHAR(20),         -- เช่น 'ป.4'
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP,
    modified_by UUID);
 1.5 CREATE TABLE periods (
    id INT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
    period_number INT NOT NULL, 
    start_time TIME NOT NULL, 
    end_time TIME NOT NULL,   
    period_type VARCHAR(20) DEFAULT 'CLASS', -- CLASS, BREAK, HOMEROOM
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP,
    modified_by UUID);
 1.6 CREATE TABLE subjects (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(20) UNIQUE NOT NULL, 
    name VARCHAR(100) NOT NULL,       
    credit DECIMAL(2,1) DEFAULT 1.0,  
    color_hex VARCHAR(10) DEFAULT '#E2E8F0', -- สีของการ์ดวิชาในหน้า UI
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP,
    modified_by UUID);
 1.7 CREATE TABLE attendance_status (
    id INT PRIMARY KEY,                  -- ลำดับ ID (1, 2, 3...)
    code VARCHAR(20) UNIQUE NOT NULL,    -- รหัสภาษาอังกฤษสำหรับ C# Enum (เช่น PRESENT, ABSENT)
    name_th VARCHAR(50) NOT NULL,        -- ชื่อภาษาไทยสำหรับแสดงผลบนหน้าจอ React (มา, ขาด, สาย...)
    color_hex VARCHAR(10) NOT NULL,      -- โค้ดสีสำหรับปุ่มหรือ Badge บน UI
    icon_name VARCHAR(50),               -- (Optional) ชื่อไอคอนสำหรับแสดงผล
    status_id INT DEFAULT 1 REFERENCES system_status(id) );      -- สถานะการใช้งาน
 1.8 CREATE TABLE system_status (
    id INT PRIMARY KEY GENERATED BY DEFAULT AS IDENTITY,
    name VARCHAR(50) UNIQUE NOT NULL, 
    description TEXT,
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    modified_by UUID);
 1.9 CREATE TABLE menus (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    code VARCHAR(50) UNIQUE NOT NULL,
    label VARCHAR(100) NOT NULL,
    parent_code VARCHAR(50) REFERENCES menus(code) ON DELETE CASCADE,
    path VARCHAR(255),
    type VARCHAR(50), -- TOPBAR, SIDEBAR
    description TEXT,
    status_id INT DEFAULT 1 REFERENCES system_status(id),
    created TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    created_by UUID,
    modified TIMESTAMP WITH TIME ZONE DEFAULT NOW(),
    modified_by UUID);





