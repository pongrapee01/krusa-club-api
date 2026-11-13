-- 00_menu_auth_schema.sql
-- Core schema for menu + role permissions (strict tree)
-- hierarchy: top -> side -> sub
-- PostgreSQL / Supabase compatible

create extension if not exists pgcrypto;

do $$
begin
    if not exists (select 1 from pg_type where typname = 'menu_type') then
        create type public.menu_type as enum ('top', 'side', 'sub');
    end if;
end$$;

create table if not exists public.app_role (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,
    name text not null,
    description text null,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now()
);

create table if not exists public.app_menu (
    id uuid primary key default gen_random_uuid(),
    code text not null unique,
    name text not null,
    menu_type public.menu_type not null,
    parent_id uuid null references public.app_menu(id) on delete cascade,
    route_path text null,
    icon text null,
    sort_order int not null default 0,
    is_active boolean not null default true,
    created_at timestamptz not null default now(),
    updated_at timestamptz not null default now(),
    constraint chk_menu_parent_presence check (
        (menu_type = 'top' and parent_id is null) or
        (menu_type in ('side', 'sub') and parent_id is not null)
    )
);

create index if not exists idx_app_menu_parent_sort on public.app_menu(parent_id, sort_order);
create index if not exists idx_app_menu_type_active on public.app_menu(menu_type, is_active);

create table if not exists public.role_menu_permission (
    role_id uuid not null references public.app_role(id) on delete cascade,
    menu_id uuid not null references public.app_menu(id) on delete cascade,
    can_view boolean not null default true,
    created_at timestamptz not null default now(),
    created_by uuid null,
    primary key (role_id, menu_id)
);

create index if not exists idx_role_menu_permission_menu on public.role_menu_permission(menu_id);

create table if not exists public.user_role (
    user_id uuid not null, -- supabase auth.users.id
    role_id uuid not null references public.app_role(id) on delete cascade,
    assigned_at timestamptz not null default now(),
    assigned_by uuid null,
    primary key (user_id, role_id)
);

create index if not exists idx_user_role_role on public.user_role(role_id);

-- Ensure hierarchy consistency:
-- side.parent must be top
-- sub.parent must be side
create or replace function public.fn_validate_menu_hierarchy()
returns trigger
language plpgsql
as $$
declare
    v_parent_type public.menu_type;
begin
    if new.menu_type = 'top' then
        if new.parent_id is not null then
            raise exception 'top menu must not have parent_id';
        end if;
        return new;
    end if;

    select menu_type into v_parent_type
    from public.app_menu
    where id = new.parent_id;

    if v_parent_type is null then
        raise exception 'parent menu not found for menu %', new.code;
    end if;

    if new.menu_type = 'side' and v_parent_type <> 'top' then
        raise exception 'side menu parent must be top';
    end if;

    if new.menu_type = 'sub' and v_parent_type <> 'side' then
        raise exception 'sub menu parent must be side';
    end if;

    return new;
end;
$$;

drop trigger if exists trg_validate_menu_hierarchy on public.app_menu;
create trigger trg_validate_menu_hierarchy
before insert or update on public.app_menu
for each row execute function public.fn_validate_menu_hierarchy();

