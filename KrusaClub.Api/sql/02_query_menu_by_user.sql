-- 02_query_menu_by_user.sql
-- Query helpers for strict tree menu model: top -> side -> sub
-- Works with: app_role, app_menu, role_menu_permission, user_profile(role_code)

-- =========================================================
-- A) Get role of current user from user_profile
-- Input:  @UserId (uuid)
-- Output: 1 row role_id + role_code
-- =========================================================
with my_role as (
    select r.id as role_id, r.code as role_code
    from public.user_profile up
    join public.app_role r on r.code = up.role_code
    where up.user_id = @UserId
      and r.is_active = true
    limit 1
)
select *
from my_role;


-- =========================================================
-- B) Get menu tree by user (top-level permission strategy)
-- Strategy:
--   - Role gets permission mainly on TOP menu
--   - Side/Sub are inherited from allowed top
-- Input:  @UserId (uuid)
-- Output: flattened rows for backend to shape as tree
-- =========================================================
with my_role as (
    select r.id as role_id
    from public.user_profile up
    join public.app_role r on r.code = up.role_code
    where up.user_id = @UserId
      and r.is_active = true
    limit 1
),
allowed_top as (
    select m.id, m.code, m.name, m.route_path, m.icon, m.sort_order
    from public.role_menu_permission rp
    join my_role mr on mr.role_id = rp.role_id
    join public.app_menu m on m.id = rp.menu_id
    where rp.can_view = true
      and m.menu_type = 'top'
      and m.is_active = true
),
allowed_side as (
    select s.id, s.code, s.name, s.parent_id, s.route_path, s.icon, s.sort_order
    from public.app_menu s
    join allowed_top t on s.parent_id = t.id
    where s.menu_type = 'side'
      and s.is_active = true
),
allowed_sub as (
    select sb.id, sb.code, sb.name, sb.parent_id, sb.route_path, sb.icon, sb.sort_order
    from public.app_menu sb
    join allowed_side s on sb.parent_id = s.id
    where sb.menu_type = 'sub'
      and sb.is_active = true
)
select
    t.id as top_id,
    t.code as top_code,
    t.name as top_name,
    t.route_path as top_route_path,
    t.icon as top_icon,
    t.sort_order as top_sort_order,

    s.id as side_id,
    s.code as side_code,
    s.name as side_name,
    s.route_path as side_route_path,
    s.icon as side_icon,
    s.sort_order as side_sort_order,

    sb.id as sub_id,
    sb.code as sub_code,
    sb.name as sub_name,
    sb.route_path as sub_route_path,
    sb.icon as sub_icon,
    sb.sort_order as sub_sort_order
from allowed_top t
left join allowed_side s on s.parent_id = t.id
left join allowed_sub sb on sb.parent_id = s.id
order by
    t.sort_order,
    s.sort_order nulls last,
    sb.sort_order nulls last,
    t.name,
    s.name,
    sb.name;


-- =========================================================
-- C) Grant top menu to role
-- Input: @RoleCode (text), @TopMenuCode (text), @AdminUserId (uuid optional)
-- =========================================================
insert into public.role_menu_permission (role_id, menu_id, can_view, created_by)
select r.id, m.id, true, @AdminUserId
from public.app_role r
join public.app_menu m on m.code = @TopMenuCode
where r.code = @RoleCode
  and m.menu_type = 'top'
on conflict (role_id, menu_id) do update
set can_view = excluded.can_view;


-- =========================================================
-- D) Revoke top menu from role
-- Input: @RoleCode (text), @TopMenuCode (text)
-- =========================================================
delete from public.role_menu_permission rp
using public.app_role r, public.app_menu m
where rp.role_id = r.id
  and rp.menu_id = m.id
  and r.code = @RoleCode
  and m.code = @TopMenuCode
  and m.menu_type = 'top';


-- =========================================================
-- E) Optional admin check: list current top-menu grants per role
-- =========================================================
select
    r.code as role_code,
    r.name as role_name,
    m.code as top_menu_code,
    m.name as top_menu_name,
    rp.can_view
from public.role_menu_permission rp
join public.app_role r on r.id = rp.role_id
join public.app_menu m on m.id = rp.menu_id
where m.menu_type = 'top'
order by r.code, m.sort_order, m.name;

