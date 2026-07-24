-- billed_* track metered hours already crystallized into a bill by the mail delivery
ALTER TABLE `fso_lot_usage`
  ADD COLUMN `billed_light_hours` FLOAT NOT NULL DEFAULT 0,
  ADD COLUMN `billed_stall_hours` FLOAT NOT NULL DEFAULT 0;
