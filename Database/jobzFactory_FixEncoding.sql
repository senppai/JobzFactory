/*
======================================================================================
 JobzFactory — comprehensive mojibake repair
======================================================================================
 Fixes text that was stored as UTF-8 decoded as Latin-1/CP1252 (e.g. "SociÃ©tÃ©"
 instead of "Société", "iÃ©tÃ©" instead of "ité"). Walks every NVARCHAR/NCHAR/NTEXT
 column in every dbo table and applies a full mojibake -> correct replacement map.

 Pure ASCII source: all accented characters are produced with NCHAR() so the script
 is independent of the file/codepage used to run it. Safe to re-run (no-op once data
 is correct). Does NOT touch sysdiagrams.

 Run once against the existing database:
   sqlcmd -S "(localdb)\MSSQLLocalDB" -E -i Database\jobzFactory_FixEncoding.sql
======================================================================================
*/
USE jobzFactory;
SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#pairs') IS NOT NULL DROP TABLE #pairs;
CREATE TABLE #pairs (seq INT IDENTITY(1,1) PRIMARY KEY, bad NVARCHAR(4) NOT NULL, good NVARCHAR(2) NOT NULL);

-- 2-byte mojibake: UTF-8 byte 0xC3 ('Ã') + second byte decoded as Latin-1/CP1252.
-- Second bytes 0xA0-0xBF are identical in Latin-1 and CP1252, so these are reliable.
INSERT #pairs (bad, good) VALUES
 (NCHAR(0xC3)+NCHAR(0xA0), NCHAR(0xE0)),  -- à
 (NCHAR(0xC3)+NCHAR(0xA2), NCHAR(0xE2)),  -- â
 (NCHAR(0xC3)+NCHAR(0xA4), NCHAR(0xE4)),  -- ä
 (NCHAR(0xC3)+NCHAR(0xA5), NCHAR(0xE5)),  -- å
 (NCHAR(0xC3)+NCHAR(0xA7), NCHAR(0xE7)),  -- ç
 (NCHAR(0xC3)+NCHAR(0xA8), NCHAR(0xE8)),  -- è
 (NCHAR(0xC3)+NCHAR(0xA9), NCHAR(0xE9)),  -- é
 (NCHAR(0xC3)+NCHAR(0xAA), NCHAR(0xEA)),  -- ê
 (NCHAR(0xC3)+NCHAR(0xAB), NCHAR(0xEB)),  -- ë
 (NCHAR(0xC3)+NCHAR(0xAE), NCHAR(0xEE)),  -- î
 (NCHAR(0xC3)+NCHAR(0xAF), NCHAR(0xEF)),  -- ï
 (NCHAR(0xC3)+NCHAR(0xB1), NCHAR(0xF1)),  -- ñ
 (NCHAR(0xC3)+NCHAR(0xB4), NCHAR(0xF4)),  -- ô
 (NCHAR(0xC3)+NCHAR(0xB6), NCHAR(0xF6)),  -- ö
 (NCHAR(0xC3)+NCHAR(0xB9), NCHAR(0xF9)),  -- ù
 (NCHAR(0xC3)+NCHAR(0xBB), NCHAR(0xFB)),  -- û
 (NCHAR(0xC3)+NCHAR(0xBC), NCHAR(0xFC));  -- ü

-- Uppercase accents (UTF-8 0xC3 + 0x80-0x9F). Best-effort CP1252 second-byte mapping.
INSERT #pairs (bad, good) VALUES
 (NCHAR(0xC3)+NCHAR(0x20AC), NCHAR(0xC0)),  -- À  (0x80 = €)
 (NCHAR(0xC3)+NCHAR(0x201A), NCHAR(0xC2)),  -- Â  (0x82 = ‚)
 (NCHAR(0xC3)+NCHAR(0x2021), NCHAR(0xC7)),  -- Ç  (0x87 = ‡)
 (NCHAR(0xC3)+NCHAR(0x02C6), NCHAR(0xC8)),  -- È  (0x88 = ˆ)
 (NCHAR(0xC3)+NCHAR(0x2030), NCHAR(0xC9)),  -- É  (0x89 = ‰)
 (NCHAR(0xC3)+NCHAR(0x0160), NCHAR(0xCA)),  -- Ê  (0x8A = Š)
 (NCHAR(0xC3)+NCHAR(0x2039), NCHAR(0xCB)),  -- Ë  (0x8B = ‹)
 (NCHAR(0xC3)+NCHAR(0x017D), NCHAR(0xCE)),  -- Î  (0x8E = Ž)
 (NCHAR(0xC3)+NCHAR(0x201D), NCHAR(0xD4)),  -- Ô  (0x94 = ”)
 (NCHAR(0xC3)+NCHAR(0x2122), NCHAR(0xD9)),  -- Ù  (0x99 = ™)
 (NCHAR(0xC3)+NCHAR(0x203B), NCHAR(0xDB)),  -- Û  (0x9B = ›)
 (NCHAR(0xC3)+NCHAR(0x0153), NCHAR(0xDC));  -- Ü  (0x9C = œ)

-- 3-byte mojibake: smart quotes / dashes / ellipsis (UTF-8 0xE2 0x80 0x.. -> â€..).
INSERT #pairs (bad, good) VALUES
 (NCHAR(0xE2)+NCHAR(0x20AC)+NCHAR(0x2122), NCHAR(0x2019)),  -- '  (â€™)
 (NCHAR(0xE2)+NCHAR(0x20AC)+NCHAR(0x0153), NCHAR(0x201C)),  -- "  (â€œ)
 (NCHAR(0xE2)+NCHAR(0x20AC)+NCHAR(0x201C), NCHAR(0x2013)),  -- –  (â€")
 (NCHAR(0xE2)+NCHAR(0x20AC)+NCHAR(0x201D), NCHAR(0x2014)),  -- —  (â€")
 (NCHAR(0xE2)+NCHAR(0x20AC)+NCHAR(0x00A6), NCHAR(0x2026));  -- …  (â€¦)

DECLARE @tbl SYSNAME, @col SYSNAME, @typ SYSNAME, @sql NVARCHAR(MAX), @chain NVARCHAR(MAX), @bad NVARCHAR(4), @good NVARCHAR(2);

DECLARE col_cur CURSOR LOCAL FAST_FORWARD FOR
  SELECT t.name, c.name, ty.name
  FROM sys.columns c
  JOIN sys.tables t  ON c.object_id = t.object_id
  JOIN sys.types ty  ON c.user_type_id = ty.user_type_id
  WHERE t.schema_id = SCHEMA_ID('dbo')
    AND t.name <> 'sysdiagrams'
    AND ty.name IN ('nvarchar','nchar','ntext')
    AND c.is_computed = 0
  ORDER BY t.name, c.column_id;

OPEN col_cur;
FETCH NEXT FROM col_cur INTO @tbl, @col, @typ;

WHILE @@FETCH_STATUS = 0
BEGIN
    -- Build the nested REPLACE(...) chain, longest bad-sequences first.
    SET @chain = QUOTENAME(@col);
    DECLARE pair_cur CURSOR LOCAL FAST_FORWARD FOR
        SELECT bad, good FROM #pairs ORDER BY LEN(bad) DESC, seq;
    OPEN pair_cur;
    FETCH NEXT FROM pair_cur INTO @bad, @good;
    WHILE @@FETCH_STATUS = 0
    BEGIN
        SET @chain = 'REPLACE(' + @chain + ',N' + QUOTENAME(@bad,'''') + ',N' + QUOTENAME(@good,'''') + ')';
        FETCH NEXT FROM pair_cur INTO @bad, @good;
    END
    CLOSE pair_cur; DEALLOCATE pair_cur;

    SET @sql = N'UPDATE dbo.' + QUOTENAME(@tbl) + N' SET ' + QUOTENAME(@col) + N' = ' + @chain
             + N' WHERE ' + QUOTENAME(@col) + N' LIKE N''%'' + NCHAR(0xC3) + N''%'''
             + N' OR '   + QUOTENAME(@col) + N' LIKE N''%'' + NCHAR(0xE2) + N''%''';

    EXEC sp_executesql @sql;
    PRINT 'Repaired dbo.' + @tbl + '.' + @col + ' -> ' + CAST(@@ROWCOUNT AS VARCHAR(10)) + ' row(s)';

    FETCH NEXT FROM col_cur INTO @tbl, @col, @typ;
END
CLOSE col_cur; DEALLOCATE col_cur;

DROP TABLE #pairs;

-- Sanity check: list any remaining rows still containing the mojibake markers.
PRINT '--- Remaining rows with mojibake markers (should be empty) ---';
DECLARE @chk NVARCHAR(MAX) = N'';
SELECT @chk = @chk +
  'SELECT ''' + t.name + '.' + c.name + ''' AS col, COUNT(*) AS bad_rows FROM dbo.' + QUOTENAME(t.name) +
  ' WHERE ' + QUOTENAME(c.name) + ' LIKE N''%'' + NCHAR(0xC3) + N''%'' OR ' + QUOTENAME(c.name) + ' LIKE N''%'' + NCHAR(0xE2) + N''%'' UNION ALL '
FROM sys.columns c
JOIN sys.tables t  ON c.object_id = t.object_id
JOIN sys.types ty  ON c.user_type_id = ty.user_type_id
WHERE t.schema_id = SCHEMA_ID('dbo') AND t.name <> 'sysdiagrams'
  AND ty.name IN ('nvarchar','nchar','ntext') AND c.is_computed = 0;

IF LEN(@chk) > 0
BEGIN
    SET @chk = LEFT(@chk, LEN(@chk) - 10); -- strip trailing ' UNION ALL '
    EXEC sp_executesql @chk;
END
GO
