-- persist the lot's packed size (size | floors << 8) for billing; updated on every lot save
ALTER TABLE `fso_lots` ADD COLUMN `size` INT NOT NULL DEFAULT 0;
