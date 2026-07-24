-- add the DiscoSO bills task type
ALTER TABLE `fso_tasks`
CHANGE COLUMN `task_type` `task_type` ENUM('prune_database', 'bonus', 'shutdown', 'job_balance', 'multi_check', 'prune_abandoned_lots', 'neighborhood_tick', 'birthday_gift', 'bills') NOT NULL ;
