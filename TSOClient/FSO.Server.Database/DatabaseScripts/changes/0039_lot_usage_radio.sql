-- stereo-on hours join the usage meter (billed at discoso_bills 0:8)
ALTER TABLE `fso_lot_usage`
  ADD COLUMN `radio_hours` FLOAT NOT NULL DEFAULT 0,
  ADD COLUMN `billed_radio_hours` FLOAT NOT NULL DEFAULT 0;
