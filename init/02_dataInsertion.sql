BEGIN;

-- Optional: ensure all deferrable constraints are deferred for this transaction
SET CONSTRAINTS ALL DEFERRED;

-- Buildings
INSERT INTO saitynai.building (id, address, "name") VALUES
  (1, '123 University Ave', 'Main Campus A'),
  (2, '500 Market St', 'Downtown Annex');

-- Floors
INSERT INTO saitynai.floor (id, building_id, floor_number, floor_plan_path) VALUES
  (1, 1, 0, '/plans/main_a_f0.png'),
  (2, 1, 1, '/plans/main_a_f1.png'),
  (3, 2, 0, '/plans/annex_f0.png'),
  (4, 2, 1, '/plans/annex_f1.png');

-- Points (latitude/longitude up to 6 decimal places)
INSERT INTO saitynai.point (id, floor_id, latitude, longitude) VALUES
  (1, 1, 54.687157, 25.279652),
  (2, 1, 54.687500, 25.279800),
  (3, 2, 54.687950, 25.279300),
  (4, 2, 54.688300, 25.279050),
  (5, 3, 54.690000, 25.280000),
  (6, 3, 54.690350, 25.280250),
  (7, 4, 54.690700, 25.280500),
  (8, 4, 54.691050, 25.280750);

-- Scans (let triggers maintain ap_count)
INSERT INTO saitynai.scan (id, point_id, scanned_at, filters) VALUES
  (1, 1, '2025-10-12T10:00:00Z', 'band=2.4GHz'),
  (2, 2, '2025-10-12T10:02:00Z', 'band=mixed'),
  (3, 3, '2025-10-12T10:04:00Z', NULL),
  (4, 4, '2025-10-12T10:06:00Z', 'band=5GHz'),
  (5, 5, '2025-10-12T10:08:00Z', NULL),
  (6, 6, '2025-10-12T10:10:00Z', 'band=mixed'),
  (7, 7, '2025-10-12T10:12:00Z', NULL),
  (8, 8, '2025-10-12T10:14:00Z', 'band=2.4GHz');

-- Access points (3 per scan; (scan_id, bssid) must be unique)
-- Scan 1
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (1,  1, 'CampusWiFi',     '00:11:22:33:44:01', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -45),
  (2,  1, 'CampusWiFi-5G',  '00:11:22:33:44:02', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5180, -60),
  (3,  1, 'Guest',          '00:11:22:33:44:03', '[WPA-PSK-CCMP][ESS]',  NULL, NULL, 2437, -70);

-- Scan 2
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (4,  2, 'CampusWiFi',     '00:11:22:33:44:01', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -50),
  (5,  2, 'CampusWiFi-5G',  '00:11:22:33:44:04', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5200, -62),
  (6,  2, 'LabNet',         '00:11:22:33:44:05', '[WPA2-Enterprise][ESS]', NULL, NULL, 2462, -68);

-- Scan 3
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (7,  3, 'CampusWiFi',     '00:11:22:33:44:06', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -48),
  (8,  3, 'CampusWiFi-5G',  '00:11:22:33:44:02', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5180, -58),
  (9,  3, 'Guest',          '00:11:22:33:44:07', '[WPA-PSK-CCMP][ESS]',  NULL, NULL, 2437, -72);

-- Scan 4
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (10, 4, 'CampusWiFi',     '00:11:22:33:44:06', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -52),
  (11, 4, 'CampusWiFi-5G',  '00:11:22:33:44:08', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5240, -59),
  (12, 4, 'Guest',          '00:11:22:33:44:03', '[WPA-PSK-CCMP][ESS]',  NULL, NULL, 2437, -69);

-- Scan 5
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (13, 5, 'CampusWiFi',     '00:11:22:33:44:09', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -47),
  (14, 5, 'CampusWiFi-5G',  '00:11:22:33:44:0A', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5300, -61),
  (15, 5, 'LabNet',         '00:11:22:33:44:05', '[WPA2-Enterprise][ESS]', NULL, NULL, 2462, -67);

-- Scan 6
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (16, 6, 'CampusWiFi',     '00:11:22:33:44:09', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -51),
  (17, 6, 'CampusWiFi-5G',  '00:11:22:33:44:0B', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5500, -63),
  (18, 6, 'Guest',          '00:11:22:33:44:07', '[WPA-PSK-CCMP][ESS]',  NULL, NULL, 2437, -73);

-- Scan 7
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (19, 7, 'CampusWiFi',     '00:11:22:33:44:01', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -49),
  (20, 7, 'CampusWiFi-5G',  '00:11:22:33:44:0C', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5180, -57),
  (21, 7, 'LabNet',         '00:11:22:33:44:05', '[WPA2-Enterprise][ESS]', NULL, NULL, 2462, -66);

-- Scan 8
INSERT INTO saitynai.access_point
  (id, scan_id, ssid, bssid, capabilities, centerfreq0, centerfreq1, frequency, "level")
VALUES
  (22, 8, 'CampusWiFi',     '00:11:22:33:44:06', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 2412, -53),
  (23, 8, 'CampusWiFi-5G',  '00:11:22:33:44:0D', '[WPA2-PSK-CCMP][ESS]', NULL, NULL, 5200, -60),
  (24, 8, 'Guest',          '00:11:22:33:44:03', '[WPA-PSK-CCMP][ESS]',  NULL, NULL, 2437, -71);

-- Optional: sync sequences to max(id) so future inserts don't collide
SELECT setval('saitynai.building_id_seq',     (SELECT COALESCE(MAX(id), 0) FROM saitynai.building),     true);
SELECT setval('saitynai.floor_id_seq',        (SELECT COALESCE(MAX(id), 0) FROM saitynai.floor),        true);
SELECT setval('saitynai.point_id_seq',        (SELECT COALESCE(MAX(id), 0) FROM saitynai.point),        true);
SELECT setval('saitynai.scan_id_seq',         (SELECT COALESCE(MAX(id), 0) FROM saitynai.scan),         true);
SELECT setval('saitynai.access_point_id_seq', (SELECT COALESCE(MAX(id), 0) FROM saitynai.access_point), true);

COMMIT;
