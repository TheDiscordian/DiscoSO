-- outstanding property bills per lot (physical bills flow)
CREATE TABLE IF NOT EXISTS `fso_lot_bills` (
  `bill_id` INT NOT NULL AUTO_INCREMENT,
  `lot_id` INT NOT NULL,
  `amount` INT NOT NULL,
  `billed_day` INT NOT NULL,
  `paid_day` INT NULL DEFAULT NULL,
  `paid_by` INT UNSIGNED NULL DEFAULT NULL,
  PRIMARY KEY (`bill_id`),
  UNIQUE KEY `lot_bill_day` (`lot_id`, `billed_day`),
  INDEX `lot_outstanding` (`lot_id`, `paid_day`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;
