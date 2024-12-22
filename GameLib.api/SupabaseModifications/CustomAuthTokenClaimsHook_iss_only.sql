-- Assumes that there is an is_admin flag on the profiles table.
create or replace function public.custom_access_token_hook(event jsonb)
returns jsonb
language plpgsql
security definer
as $$
  declare
claims jsonb;
begin
    if (jsonb_typeof(claims->'iss')) is null then
        claims := jsonb_set(claims, '{iss}', '""');
    end if;

    claims := jsonb_set(claims, '{iss}', '"http://localhost:8000/auth/v1"');

    event := jsonb_set(event, '{claims}', claims);
    
    -- Return the modified or original event
    return event;
end;
$$;