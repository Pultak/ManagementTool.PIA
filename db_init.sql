-- DROP SCHEMA "manTool";

CREATE SCHEMA "manTool" AUTHORIZATION postgres;

-- DROP TYPE "manTool".roletype;

CREATE TYPE "manTool".roletype AS ENUM (
	'Superior',
	'DepartmentManager',
	'Secretariat',
	'ProjectManager');
-- "manTool"."Assignment" definition

-- Drop table

-- DROP TABLE "manTool"."Assignment";

CREATE TABLE "manTool"."Assignment" (
	id_assignment int8 NULL,
	id_project int8 NULL,
	id_user int8 NULL,
	allocation_scope int8 NULL,
	from_date date NULL,
	to_date date NULL,
	state text NULL
);


-- "manTool"."Project" definition

-- Drop table

-- DROP TABLE "manTool"."Project";

CREATE TABLE "manTool"."Project" (
	id_project int8 NULL,
	project_name text NULL,
	from_date date NULL,
	to_date date NULL,
	description text NULL
);


-- "manTool"."Role" definition

-- Drop table

-- DROP TABLE "manTool"."Role";

CREATE TABLE "manTool"."Role" (
	id_role int8 NULL,
	"name" text NULL,
	"type" public.roletype NULL,
	id_project int8 NULL
);


-- "manTool"."User" definition

-- Drop table

-- DROP TABLE "manTool"."User";

CREATE TABLE "manTool"."User" (
	id_user int8 NULL,
	pwd text NULL,
	full_name text NULL,
	username text NULL,
	primary_workplace text NULL,
	email_address text NULL
);


-- "manTool".rolesassign definition

-- Drop table

-- DROP TABLE "manTool".rolesassign;

CREATE TABLE "manTool".rolesassign (
	id_user int8 NULL,
	id_role int8 NULL,
	assigned_date date NULL
);
