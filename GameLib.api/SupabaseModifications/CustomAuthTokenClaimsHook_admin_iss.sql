﻿create table profiles (
                          user_id uuid not null primary key references auth.users (id),
                          is_admin boolean not null default false
);

-- Assumes that there is an is_admin flag on the profiles table.
create or replace function public.custom_access_token_hook(event jsonb)
returns jsonb
language plpgsql
security definer
as $$
  declare
claims jsonb;
    is_user_admin boolean;
begin
    -- Check if the user is marked as admin in the profiles table
select is_admin into is_user_admin from public.profiles where user_id = (event->>'user_id')::uuid;

-- Proceed only if the user is an admin
if is_user_admin then
      claims := event->'claims';

      -- Check if 'user_metadata' exists in claims
      if jsonb_typeof(claims->'user_metadata') is null then
        -- If 'user_metadata' does not exist, create an empty object
        claims := jsonb_set(claims, '{user_metadata}', '{}');
end if;

      -- Set a claim of 'admin'
      claims := jsonb_set(claims, '{user_metadata, admin}', 'true');

      -- Update the 'claims' object in the original event
      event := jsonb_set(event, '{claims}', claims);
end if;

    if (jsonb_typeof(claims->'iss')) is null then
      claims := jsonb_set(claims, '{iss}', '""');
end if;

    claims := jsonb_set(claims, '{iss}', '"http://localhost:8000/auth/v1"');

    event := jsonb_set(event, '{claims}', claims);
    
    -- Return the modified or original event
return event;
end;
$$;

grant execute
    on function public.custom_access_token_hook
    to supabase_auth_admin;

revoke execute
    on function public.custom_access_token_hook
    from authenticated, anon, public;

grant usage on schema public to supabase_auth_admin;