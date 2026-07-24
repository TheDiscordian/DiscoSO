-- multiple bills per day: a lot close after a same-day payment opens a new bill
-- instead of deferring the charge to tomorrow
ALTER TABLE `fso_lot_bills`
  DROP INDEX `lot_bill_day`,
  ADD INDEX `lot_bill_day` (`lot_id`, `billed_day`);
