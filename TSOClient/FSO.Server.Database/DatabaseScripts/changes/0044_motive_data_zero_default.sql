-- fso_avatars.motive_data is binary(32) read as 16 big-endian shorts, one per VMMotive.
-- Its default was the string '0\0\0...' - a literal '0' (0x30) followed by 31 nulls - so any
-- avatar inserted without an explicit value came out with motive 0 (HappyLife) = 0x3000.
-- The insert now passes 32 zero bytes; this fixes the default behind it and normalises the
-- rows written before that, so every avatar on the shard has the same shape.

ALTER TABLE `fso_avatars`
  MODIFY `motive_data` binary(32) NOT NULL
  DEFAULT 0x0000000000000000000000000000000000000000000000000000000000000000;

UPDATE `fso_avatars`
  SET `motive_data` = CONCAT(0x00, SUBSTRING(`motive_data`, 2, 31))
  WHERE LEFT(`motive_data`, 1) = 0x30;
