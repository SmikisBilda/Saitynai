-- DROP SCHEMA saitynai;

CREATE SCHEMA saitynai AUTHORIZATION postgres;

-- Drop table

-- DROP TABLE saitynai.access_point;
CREATE TABLE saitynai.building (
	id serial4 NOT NULL,
	address text NOT NULL,
	"name" text NOT NULL,
	CONSTRAINT building_pkey PRIMARY KEY (id)
);
CREATE TABLE floor (
	id serial4 NOT NULL,
	building_id int4 NOT NULL,
	floor_number int4 NOT NULL,
	floor_plan_path text NOT NULL,
	CONSTRAINT floor_pkey PRIMARY KEY (id),
	CONSTRAINT floor_building_id_fkey FOREIGN KEY (building_id) REFERENCES saitynai.building(id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED
);
CREATE TABLE point (
	id serial4 NOT NULL,
	floor_id int4 NOT NULL,
	latitude numeric(9, 6) NOT NULL,
	longitude numeric(9, 6) NOT NULL,
	ap_count int4 DEFAULT 0 NOT NULL,
	CONSTRAINT point_pkey PRIMARY KEY (id),
	CONSTRAINT point_floor_id_fkey FOREIGN KEY (floor_id) REFERENCES saitynai.floor(id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED
);
CREATE TABLE saitynai.scan (
	id serial4 NOT NULL,
	point_id int4 NOT NULL,
	scanned_at timestamptz NOT NULL,
	filters text NULL,
	ap_count int4 DEFAULT 0 NOT NULL,
	CONSTRAINT scan_pkey PRIMARY KEY (id),
	CONSTRAINT scan_point_id_fkey FOREIGN KEY (point_id) REFERENCES saitynai.point(id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED
);
CREATE TABLE saitynai.access_point (
	id serial4 NOT NULL,
	scan_id int4 NOT NULL,
	ssid text NULL,
	bssid macaddr NOT NULL,
	capabilities text NULL,
	centerfreq0 int4 NULL,
	centerfreq1 int4 NULL,
	frequency int4 NULL,
	"level" int2 NOT NULL,
	CONSTRAINT access_point_pkey PRIMARY KEY (id),
	CONSTRAINT access_point_scan_id_fkey FOREIGN KEY (scan_id) REFERENCES saitynai.scan(id) ON DELETE CASCADE DEFERRABLE INITIALLY DEFERRED
);

CREATE INDEX idx_ap_scan_id ON saitynai.access_point USING btree (scan_id);
CREATE UNIQUE INDEX ux_ap_scan_bssid ON saitynai.access_point USING btree (scan_id, bssid);

CREATE OR REPLACE FUNCTION saitynai.recompute_point_ap_count()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
  IF TG_OP = 'INSERT' THEN
    UPDATE saitynai.point p
      SET ap_count = COALESCE((SELECT SUM(s.ap_count) FROM saitynai.scan s WHERE s.point_id = NEW.point_id), 0)
      WHERE p.id = NEW.point_id;
  ELSIF TG_OP = 'DELETE' THEN
    UPDATE saitynai.point p
      SET ap_count = COALESCE((SELECT SUM(s.ap_count) FROM saitynai.scan s WHERE s.point_id = OLD.point_id), 0)
      WHERE p.id = OLD.point_id;
  ELSIF TG_OP = 'UPDATE' THEN
    IF NEW.point_id IS DISTINCT FROM OLD.point_id THEN
      UPDATE saitynai.point p
        SET ap_count = COALESCE((SELECT SUM(s.ap_count) FROM saitynai.scan s WHERE s.point_id = OLD.point_id), 0)
        WHERE p.id = OLD.point_id;
      UPDATE saitynai.point p
        SET ap_count = COALESCE((SELECT SUM(s.ap_count) FROM saitynai.scan s WHERE s.point_id = NEW.point_id), 0)
        WHERE p.id = NEW.point_id;
    ELSIF NEW.ap_count IS DISTINCT FROM OLD.ap_count THEN
      UPDATE saitynai.point p
        SET ap_count = COALESCE((SELECT SUM(s.ap_count) FROM saitynai.scan s WHERE s.point_id = NEW.point_id), 0)
        WHERE p.id = NEW.point_id;
    END IF;
  END IF;
  RETURN NULL;
END
$function$
;

-- DROP FUNCTION saitynai.recompute_scan_ap_count();

CREATE OR REPLACE FUNCTION saitynai.recompute_scan_ap_count()
 RETURNS trigger
 LANGUAGE plpgsql
AS $function$
BEGIN
  IF TG_OP = 'INSERT' THEN
    UPDATE saitynai.scan s
      SET ap_count = (SELECT COUNT(*) FROM saitynai.access_point ap WHERE ap.scan_id = NEW.scan_id)
      WHERE s.id = NEW.scan_id;
  ELSIF TG_OP = 'DELETE' THEN
    UPDATE saitynai.scan s
      SET ap_count = (SELECT COUNT(*) FROM saitynai.access_point ap WHERE ap.scan_id = OLD.scan_id)
      WHERE s.id = OLD.scan_id;
  ELSIF TG_OP = 'UPDATE' THEN
    IF NEW.scan_id IS DISTINCT FROM OLD.scan_id THEN
      UPDATE saitynai.scan s
        SET ap_count = (SELECT COUNT(*) FROM saitynai.access_point ap WHERE ap.scan_id = OLD.scan_id)
        WHERE s.id = OLD.scan_id;
      UPDATE saitynai.scan s
        SET ap_count = (SELECT COUNT(*) FROM saitynai.access_point ap WHERE ap.scan_id = NEW.scan_id)
        WHERE s.id = NEW.scan_id;
    END IF;
  END IF;
  RETURN NULL;
END
$function$
;

-- Table Triggers

create trigger trg_access_point_recompute_scan after
insert
    or
delete
    or
update
    on
    saitynai.access_point for each row execute function saitynai.recompute_scan_ap_count();



-- Drop table

-- DROP TABLE saitynai.building;



-- Drop table

-- DROP TABLE saitynai.floor;


CREATE INDEX idx_floor_building_id ON saitynai.floor USING btree (building_id);

-- Drop table

-- DROP TABLE saitynai.point;


CREATE INDEX idx_point_floor_id ON saitynai.point USING btree (floor_id);

-- Drop table

-- DROP TABLE saitynai.scan;


CREATE INDEX idx_scan_point_id ON saitynai.scan USING btree (point_id);

-- Table Triggers

create trigger trg_scan_recompute_point_all after
insert
    or
delete
    or
update
    on
    saitynai.scan for each row execute function saitynai.recompute_point_ap_count();

-- DROP FUNCTION saitynai.recompute_point_ap_count();

