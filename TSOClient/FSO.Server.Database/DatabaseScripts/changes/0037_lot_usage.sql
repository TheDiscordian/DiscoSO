-- per-day usage meters (in-game hours) driving the bills task
CREATE TABLE IF NOT EXISTS `fso_lot_usage` (
  `lot_id` INT NOT NULL,
  `day` INT NOT NULL,
  `light_hours` FLOAT NOT NULL DEFAULT 0,
  `stall_hours` FLOAT NOT NULL DEFAULT 0,
  PRIMARY KEY (`lot_id`, `day`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
