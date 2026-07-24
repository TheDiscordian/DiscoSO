-- per-lot event log for the house panel Activity Log's Events tab
CREATE TABLE IF NOT EXISTS `fso_lot_events` (
  `event_id` INT NOT NULL AUTO_INCREMENT,
  `lot_id` INT NOT NULL,
  `avatar_id` INT UNSIGNED NULL,
  `target_avatar_id` INT UNSIGNED NULL,
  `type` TINYINT NOT NULL,
  `value` INT NOT NULL DEFAULT 0,
  `data` VARCHAR(64) NULL,
  `time` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  PRIMARY KEY (`event_id`),
  INDEX `lot_events_by_lot` (`lot_id`, `time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
