SET NOCOUNT ON;

INSERT INTO Auth_ModulePermission_Table (Module, Operation, Aciklama)
SELECT v.Module, v.Operation, v.Aciklama
FROM (
    VALUES
        -- Admin = 0
        (0, 0, 'Admin / View'),
        (0, 1, 'Admin / Create'),
        (0, 2, 'Admin / Edit'),
        (0, 3, 'Admin / Delete'),
        (0, 4, 'Admin / Manage'),

        -- TBYS = 1
        (1, 0, 'TBYS / View'),
        (1, 1, 'TBYS / Create'),
        (1, 2, 'TBYS / Edit'),
        (1, 3, 'TBYS / Delete'),
        (1, 4, 'TBYS / Manage'),

        -- NBYS = 2
        (2, 0, 'NBYS / View'),
        (2, 1, 'NBYS / Create'),
        (2, 2, 'NBYS / Edit'),
        (2, 3, 'NBYS / Delete'),
        (2, 4, 'NBYS / Manage'),

        -- IKYS = 3
        (3, 0, 'IKYS / View'),
        (3, 1, 'IKYS / Create'),
        (3, 2, 'IKYS / Edit'),
        (3, 3, 'IKYS / Delete'),
        (3, 4, 'IKYS / Manage'),

        -- Inventory = 4
        (4, 0, 'Inventory / View'),
        (4, 1, 'Inventory / Create'),
        (4, 2, 'Inventory / Edit'),
        (4, 3, 'Inventory / Delete'),
        (4, 4, 'Inventory / Manage')
) AS v(Module, Operation, Aciklama)
WHERE NOT EXISTS (
    SELECT 1
    FROM Auth_ModulePermission_Table mp
    WHERE mp.Module = v.Module
      AND mp.Operation = v.Operation
)
ORDER BY
    CASE v.Module
        WHEN 0 THEN 0
        ELSE 1
    END,
    v.Module,
    v.Operation;