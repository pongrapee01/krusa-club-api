-- 01_menu_auth_seed.sql
-- Seed data for role/menu (strict tree: top -> side -> sub)
-- Run after 00_menu_auth_schema.sql

-- 1) Roles
insert into public.app_role (code, name, description)
values
    ('super_admin', 'Super Admin', 'Full access to all menus'),
    ('teacher', 'Teacher', 'Teacher access'),
    ('student', 'Student', 'Student access')
on conflict (code) do update
set
    name = excluded.name,
    description = excluded.description,
    updated_at = now();

-- 2) Top menus
insert into public.app_menu (code, name, menu_type, route_path, icon, sort_order)
values
    ('TOP_ACADEMIC', 'Academic', 'top', '/academic', 'book-open', 10),
    ('TOP_ACTIVITY', 'Activity', 'top', '/activity', 'sparkles', 20),
    ('TOP_ADMIN', 'Admin', 'top', '/admin', 'shield-check', 30)
on conflict (code) do update
set
    name = excluded.name,
    route_path = excluded.route_path,
    icon = excluded.icon,
    sort_order = excluded.sort_order,
    is_active = true,
    updated_at = now();

-- 3) Side menus (parent = top)
with t as (
    select id, code
    from public.app_menu
    where code in ('TOP_ACADEMIC', 'TOP_ACTIVITY', 'TOP_ADMIN')
)
insert into public.app_menu (code, name, menu_type, parent_id, route_path, icon, sort_order)
values
    ('SIDE_DASHBOARD', 'Dashboard', 'side', (select id from t where code = 'TOP_ACADEMIC'), '/dashboard', 'layout-dashboard', 10),
    ('SIDE_COURSE', 'Courses', 'side', (select id from t where code = 'TOP_ACADEMIC'), '/courses', 'book', 20),
    ('SIDE_ASSIGNMENT', 'Assignments', 'side', (select id from t where code = 'TOP_ACADEMIC'), '/assignments', 'clipboard-list', 30),
    ('SIDE_REPORT', 'Reports', 'side', (select id from t where code = 'TOP_ACTIVITY'), '/reports', 'chart-line', 40),
    ('SIDE_USER_MGMT', 'Users', 'side', (select id from t where code = 'TOP_ADMIN'), '/admin/users', 'users', 50),
    ('SIDE_ROLE_MGMT', 'Roles & Permissions', 'side', (select id from t where code = 'TOP_ADMIN'), '/admin/roles', 'key-round', 60)
on conflict (code) do update
set
    name = excluded.name,
    parent_id = excluded.parent_id,
    route_path = excluded.route_path,
    icon = excluded.icon,
    sort_order = excluded.sort_order,
    is_active = true,
    updated_at = now();

-- 4) Sub menus (parent = side menu)
with s as (
    select id, code
    from public.app_menu
    where code in ('SIDE_COURSE', 'SIDE_ASSIGNMENT', 'SIDE_REPORT')
)
insert into public.app_menu (code, name, menu_type, parent_id, route_path, sort_order)
values
    ('SUB_COURSE_LIST', 'Course List', 'sub', (select id from s where code = 'SIDE_COURSE'), '/courses/list', 10),
    ('SUB_COURSE_CALENDAR', 'Course Calendar', 'sub', (select id from s where code = 'SIDE_COURSE'), '/courses/calendar', 20),
    ('SUB_ASSIGNMENT_LIST', 'Assignment List', 'sub', (select id from s where code = 'SIDE_ASSIGNMENT'), '/assignments/list', 10),
    ('SUB_ASSIGNMENT_SUBMIT', 'My Submissions', 'sub', (select id from s where code = 'SIDE_ASSIGNMENT'), '/assignments/submissions', 20),
    ('SUB_REPORT_CLASS', 'Class Report', 'sub', (select id from s where code = 'SIDE_REPORT'), '/reports/class', 10),
    ('SUB_REPORT_STUDENT', 'Student Report', 'sub', (select id from s where code = 'SIDE_REPORT'), '/reports/student', 20)
on conflict (code) do update
set
    name = excluded.name,
    parent_id = excluded.parent_id,
    route_path = excluded.route_path,
    sort_order = excluded.sort_order,
    is_active = true,
    updated_at = now();

-- 5) Role permissions
-- super_admin = all menus
insert into public.role_menu_permission (role_id, menu_id, can_view)
select r.id, m.id, true
from public.app_role r
cross join public.app_menu m
where r.code = 'super_admin'
on conflict (role_id, menu_id) do update
set can_view = excluded.can_view;

-- teacher permissions
with role_teacher as (
    select id from public.app_role where code = 'teacher'
),
menu_teacher as (
    select id
    from public.app_menu
    where code in (
        'TOP_ACADEMIC', 'TOP_ACTIVITY',
        'SIDE_DASHBOARD', 'SIDE_COURSE', 'SIDE_ASSIGNMENT', 'SIDE_REPORT',
        'SUB_COURSE_LIST', 'SUB_COURSE_CALENDAR',
        'SUB_ASSIGNMENT_LIST',
        'SUB_REPORT_CLASS', 'SUB_REPORT_STUDENT'
    )
)
insert into public.role_menu_permission (role_id, menu_id, can_view)
select rt.id, mt.id, true
from role_teacher rt
cross join menu_teacher mt
on conflict (role_id, menu_id) do update
set can_view = excluded.can_view;

-- student permissions
with role_student as (
    select id from public.app_role where code = 'student'
),
menu_student as (
    select id
    from public.app_menu
    where code in (
        'TOP_ACADEMIC',
        'SIDE_DASHBOARD', 'SIDE_COURSE', 'SIDE_ASSIGNMENT',
        'SUB_COURSE_LIST',
        'SUB_ASSIGNMENT_SUBMIT'
    )
)
insert into public.role_menu_permission (role_id, menu_id, can_view)
select rs.id, ms.id, true
from role_student rs
cross join menu_student ms
on conflict (role_id, menu_id) do update
set can_view = excluded.can_view;

