--select * from public.form_dealerships_insert_query(city_id, user_id);
--DROP FUNCTION form_dealerships_insert_query() ;

CREATE OR REPLACE FUNCTION form_dealerships_insert_query(city_id int, user_id int) 
RETURNS TABLE(
	insert_query text,
	dealership_id int, 
	dealership_name text,
	address text,
	phone_number text,
	provider_name text
)
AS $$
	select 
	'insert into "Dealerships" ("CityId", "UserId", "Name", "Address", "PhoneNumber", "OfficialDealer", "MoWorkTime", "TuWorkTime", "WeWorkTime", "ThWorkTime", "FrWorkTime", "SaWorkTime", "SuWorkTime") values' 
	insert_query,
	0 dealership_id, '' dealership_name,'' address,'' phone_number, '' provider_name
	union all
	select '('
	|| 569 ||', '
	|| 1 ||', '
	|| '''' || "Name" ||''', ' 
	|| '''' || "Address" ||''', ' 
	|| '''' || "PhoneNumber" ||''', '
	|| 'false' ||', '
	|| '''9:00-20:00'', ''9:00-20:00'', ''9:00-20:00'', ''9:00-20:00'', ''9:00-20:00'', ''9:00-20:00'', ''9:00-20:00'''
	|| '),' insert_query, *
	from "Dealerships"
	where "Id" in (
		select min("Id") "Id" from "Dealerships"
		group by "Name"
	);

$$ LANGUAGE SQL;