DROP FUNCTION IF EXISTS 'update_tree_for_creation'

CREATE OR REPLACE FUNCTION update_tree_for_creation(table_name VARCHAR, self_id BIGINT, parent_id BIGINT) 
RETURNS BOOLEAN AS $$
DECLARE
    _left BIGINT;
    _right BIGINT;
    rhs_value BIGINT;
    cmd VARCHAR;
BEGIN

    EXECUTE 'LOCK TABLE ' || table_name::regclass;

    IF parent_id IS NOT NULL THEN
        EXECUTE 'SELECT _left, _right FROM ' || table_name::regclass || ' WHERE _id = ' || parent_id
            INTO _left, _right;
        IF _right - _left = 1 THEN
            rhs_value := _left;
        ELSE
            rhs_value := _right - 1; --添加到集合的末尾
        END IF;
    ELSE --没有就作为最后一个根节点
        EXECUTE 'SELECT COALESCE(MAX(_right), 0) FROM ' || table_name::regclass || ' WHERE _left >= 0' 
            INTO rhs_value;    
    END IF;    
    
    EXECUTE 'UPDATE ' 
        || table_name::regclass
        || ' SET _right = _right + 2 WHERE _right > ' || rhs_value;
        
    EXECUTE 'UPDATE ' || table_name::regclass
        || ' SET _left = _left + 2 WHERE _left > ' || rhs_value;

    EXECUTE 'UPDATE ' || table_name::regclass || ' SET _left=' || rhs_value + 1
        || ', _right=' || rhs_value + 2 || ' WHERE _id=' || self_id;
        
    RETURN rhs_value;
END;
$$ LANGUAGE plpgsql;

select update_tree_for_creation('core_organization', 1, null);