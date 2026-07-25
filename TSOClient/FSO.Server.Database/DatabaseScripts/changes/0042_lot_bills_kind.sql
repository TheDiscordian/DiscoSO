-- Bill rows carry their origin: 'daily' = the nightly per-active-day charge,
-- 'metered' = usage folded in on mail delivery / lot close. The daily task's
-- billing cursor reads only its own kind.
ALTER TABLE `fso_lot_bills`
  ADD COLUMN `kind` ENUM('daily','metered') NOT NULL DEFAULT 'daily' AFTER `amount`;
