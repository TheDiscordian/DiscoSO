-- Top-100 bonuses reach budgets via the fso_bonus_after_insert trigger, bypassing the
-- transaction path. Mirror each payout into fso_transactions so the Budget Window's
-- Income tab includes them. Ledger codes: 105 visitor, 106 property, 107 sim.
DROP TRIGGER IF EXISTS `fso_bonus_ledger`;
CREATE TRIGGER `fso_bonus_ledger` AFTER INSERT ON `fso_bonus` FOR EACH ROW BEGIN
	IF IFNULL(NEW.bonus_visitor, 0) > 0 THEN
		INSERT INTO fso_transactions (from_id, to_id, transaction_type, day, value, count)
		VALUES (4294967295, NEW.avatar_id, 105, DATEDIFF(NEW.period, '1970-01-01'), NEW.bonus_visitor, 1)
		ON DUPLICATE KEY UPDATE value = value + NEW.bonus_visitor, count = count + 1;
	END IF;
	IF IFNULL(NEW.bonus_property, 0) > 0 THEN
		INSERT INTO fso_transactions (from_id, to_id, transaction_type, day, value, count)
		VALUES (4294967295, NEW.avatar_id, 106, DATEDIFF(NEW.period, '1970-01-01'), NEW.bonus_property, 1)
		ON DUPLICATE KEY UPDATE value = value + NEW.bonus_property, count = count + 1;
	END IF;
	IF IFNULL(NEW.bonus_sim, 0) > 0 THEN
		INSERT INTO fso_transactions (from_id, to_id, transaction_type, day, value, count)
		VALUES (4294967295, NEW.avatar_id, 107, DATEDIFF(NEW.period, '1970-01-01'), NEW.bonus_sim, 1)
		ON DUPLICATE KEY UPDATE value = value + NEW.bonus_sim, count = count + 1;
	END IF;
END;

-- Backfill the retention window from existing bonus history (idempotent: existing
-- ledger rows for these keys are left untouched).
INSERT INTO fso_transactions (from_id, to_id, transaction_type, day, value, count)
	SELECT 4294967295, avatar_id, 105, DATEDIFF(period, '1970-01-01'), bonus_visitor, 1
	FROM fso_bonus WHERE IFNULL(bonus_visitor, 0) > 0 AND period >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
	ON DUPLICATE KEY UPDATE value = fso_transactions.value;
INSERT INTO fso_transactions (from_id, to_id, transaction_type, day, value, count)
	SELECT 4294967295, avatar_id, 106, DATEDIFF(period, '1970-01-01'), bonus_property, 1
	FROM fso_bonus WHERE IFNULL(bonus_property, 0) > 0 AND period >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
	ON DUPLICATE KEY UPDATE value = fso_transactions.value;
INSERT INTO fso_transactions (from_id, to_id, transaction_type, day, value, count)
	SELECT 4294967295, avatar_id, 107, DATEDIFF(period, '1970-01-01'), bonus_sim, 1
	FROM fso_bonus WHERE IFNULL(bonus_sim, 0) > 0 AND period >= DATE_SUB(CURDATE(), INTERVAL 30 DAY)
	ON DUPLICATE KEY UPDATE value = fso_transactions.value;
