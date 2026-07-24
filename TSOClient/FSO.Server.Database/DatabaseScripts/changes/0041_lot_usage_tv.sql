-- TV-on hours join the usage meter (billed at discoso_bills 0:9)
ALTER TABLE `fso_lot_usage`
  ADD COLUMN `tv_hours` FLOAT NOT NULL DEFAULT 0,
  ADD COLUMN `billed_tv_hours` FLOAT NOT NULL DEFAULT 0;
