-- Metered usage now settles on its own schedule (game 7am, every even UTC hour at :35)
-- rather than when a lot closes, so an offline lot is billed at the same moment the
-- carrier would have knocked. Needs its own task type to run on a separate cron.
ALTER TABLE `fso_tasks`
  MODIFY COLUMN `task_type` ENUM('prune_database','bonus','shutdown','job_balance','multi_check','prune_abandoned_lots','neighborhood_tick','birthday_gift','bills','bills_metered') NOT NULL;
