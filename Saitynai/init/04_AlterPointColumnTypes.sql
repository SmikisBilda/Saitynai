-- This migration script updates the Point table column types to support larger coordinate values
-- Changes: latitude and longitude from numeric(9,6) to numeric(10,2)
-- This allows coordinates up to 99,999,999.99 instead of 999.999999
-- and supports pixel coordinates without precision loss

ALTER TABLE IF EXISTS saitynai.point
ALTER COLUMN latitude TYPE numeric(10, 2),
ALTER COLUMN longitude TYPE numeric(10, 2);

-- Verify the change
SELECT column_name, data_type, numeric_precision, numeric_scale
FROM information_schema.columns
WHERE table_schema = 'saitynai'
  AND table_name = 'point'
  AND column_name IN ('latitude', 'longitude');
