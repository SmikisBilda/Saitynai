CREATE SCHEMA IF NOT EXISTS saitynai AUTHORIZATION postgres;

CREATE TABLE IF NOT EXISTS saitynai.building (
  id serial4 PRIMARY KEY,
  address text NOT NULL,
  "name" text NOT NULL
);

CREATE TABLE IF NOT EXISTS saitynai.floor (
  id serial4 PRIMARY KEY,
  building_id int4 NOT NULL,
  floor_number int4 NOT NULL,
  floor_plan_path text NOT NULL,
  CONSTRAINT floor_building_id_fkey
    FOREIGN KEY (building_id)
    REFERENCES saitynai.building(id)
    ON DELETE CASCADE
    DEFERRABLE INITIALLY DEFERRED
);
CREATE INDEX IF NOT EXISTS idx_floor_building_id ON saitynai.floor(building_id);

CREATE TABLE IF NOT EXISTS saitynai.point (
  id serial4 PRIMARY KEY,
  floor_id int4 NOT NULL,
  latitude numeric(9, 6) NOT NULL,
  longitude numeric(9, 6) NOT NULL,
  ap_count int4 NOT NULL DEFAULT 0,
  CONSTRAINT point_floor_id_fkey
    FOREIGN KEY (floor_id)
    REFERENCES saitynai.floor(id)
    ON DELETE CASCADE
    DEFERRABLE INITIALLY DEFERRED
);
CREATE INDEX IF NOT EXISTS idx_point_floor_id ON saitynai.point(floor_id);

CREATE TABLE IF NOT EXISTS saitynai.scan (
  id serial4 PRIMARY KEY,
  point_id int4 NOT NULL,
  scanned_at timestamptz NOT NULL,
  filters text NULL,
  ap_count int4 NOT NULL DEFAULT 0,
  CONSTRAINT scan_point_id_fkey
    FOREIGN KEY (point_id)
    REFERENCES saitynai.point(id)
    ON DELETE CASCADE
    DEFERRABLE INITIALLY DEFERRED
);
CREATE INDEX IF NOT EXISTS idx_scan_point_id ON saitynai.scan(point_id);

CREATE TABLE IF NOT EXISTS saitynai.access_point (
  id serial4 PRIMARY KEY,
  scan_id int4 NOT NULL,
  ssid text NULL,
  bssid text NOT NULL,
  capabilities text NULL,
  centerfreq0 int4 NULL,
  centerfreq1 int4 NULL,
  frequency int4 NULL,
  "level" int2 NOT NULL,
  CONSTRAINT access_point_scan_id_fkey
    FOREIGN KEY (scan_id)
    REFERENCES saitynai.scan(id)
    ON DELETE CASCADE
    DEFERRABLE INITIALLY DEFERRED
);
CREATE INDEX IF NOT EXISTS idx_ap_scan_id ON saitynai.access_point(scan_id);
CREATE UNIQUE INDEX IF NOT EXISTS ux_ap_scan_bssid ON saitynai.access_point(scan_id, bssid);

CREATE OR REPLACE FUNCTION saitynai.recompute_scan_ap_count()
RETURNS trigger
LANGUAGE plpgsql
AS $$
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
$$;

CREATE OR REPLACE FUNCTION saitynai.recompute_point_ap_count()
RETURNS trigger
LANGUAGE plpgsql
AS $$
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
$$;

DROP TRIGGER IF EXISTS trg_access_point_recompute_scan ON saitynai.access_point;
CREATE TRIGGER trg_access_point_recompute_scan
AFTER INSERT OR UPDATE OR DELETE ON saitynai.access_point
FOR EACH ROW
EXECUTE FUNCTION saitynai.recompute_scan_ap_count();

DROP TRIGGER IF EXISTS trg_scan_recompute_point_all ON saitynai.scan;
CREATE TRIGGER trg_scan_recompute_point_all
AFTER INSERT OR UPDATE OR DELETE ON saitynai.scan
FOR EACH ROW
EXECUTE FUNCTION saitynai.recompute_point_ap_count();

WITH ap_counts AS (
  SELECT ap.scan_id, COUNT(*)::int AS cnt
  FROM saitynai.access_point ap
  GROUP BY ap.scan_id
)
UPDATE saitynai.scan s
SET ap_count = COALESCE(ac.cnt, 0)
FROM ap_counts ac
WHERE s.id = ac.scan_id;

UPDATE saitynai.scan s
SET ap_count = 0
WHERE NOT EXISTS (SELECT 1 FROM saitynai.access_point ap WHERE ap.scan_id = s.id);

WITH point_sums AS (
  SELECT s.point_id, COALESCE(SUM(s.ap_count), 0)::int AS sum_cnt
  FROM saitynai.scan s
  GROUP BY s.point_id
)
UPDATE saitynai.point p
SET ap_count = COALESCE(ps.sum_cnt, 0)
FROM point_sums ps
WHERE p.id = ps.point_id;

UPDATE saitynai.point p
SET ap_count = 0
WHERE NOT EXISTS (SELECT 1 FROM saitynai.scan s WHERE s.point_id = p.id);
