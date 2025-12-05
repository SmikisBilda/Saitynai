import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { GenericTable } from '../../components/GenericTable';
import { DeleteModal } from '../../components/modals/DeleteModal';
import { EditModal } from '../../components/modals/EditModal';
import { BackButton } from '../../components/BackButton';
import { getRolePermissions, assignPermissionToRole, deleteRolePermission, getRoles, getPermissions } from '../../services/permissionService';
import type { RolePermission, AssignPermissionDto, Role, Permission } from '../../types/api';

const RolePermissionsPage = () => {
  const navigate = useNavigate();
  const [rolePermissions, setRolePermissions] = useState<RolePermission[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [permissions, setPermissions] = useState<Permission[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [deleteItem, setDeleteItem] = useState<RolePermission | null>(null);
  const [createMode, setCreateMode] = useState(false);
  const [editItem, setEditItem] = useState<RolePermission | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [rpRes, rolesRes, permsRes] = await Promise.all([
        getRolePermissions(),
        getRoles(),
        getPermissions()
      ]);
      setRolePermissions(rpRes.data);
      setRoles(rolesRes.data);
      setPermissions(permsRes.data);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadRolePermissions = async () => {
    try {
      const response = await getRolePermissions();
      setRolePermissions(response.data);
    } catch (error) {
      console.error('Failed to load role permissions:', error);
    }
  };

  const handleDelete = (item: RolePermission) => {
    setDeleteItem(item);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    try {
      // Use resourceTypeName if available, otherwise default to Global
      const resourceType = deleteItem.resourceTypeName || 'Global';
      await deleteRolePermission(deleteItem.roleId, deleteItem.permissionId, resourceType, deleteItem.resourceId);
      setRolePermissions(rolePermissions.filter(rp => 
        !(rp.roleId === deleteItem.roleId && 
          rp.permissionId === deleteItem.permissionId && 
          rp.resourceTypeId === deleteItem.resourceTypeId && 
          rp.resourceId === deleteItem.resourceId)
      ));
      setDeleteItem(null);
    } catch (error) {
      console.error('Failed to delete role permission:', error);
    }
  };

  const handleCreate = () => {
    setCreateMode(true);
    setEditItem({ 
        roleId: 0, 
        permissionId: 0, 
        resourceTypeId: 0, 
        resourceId: 0, 
        allow: true, 
        cascade: false,
        resourceTypeName: 'Global' // Default for create
    } as RolePermission);
  };

  const handleSaveCreate = async (newItem: RolePermission) => {
    const createDto: AssignPermissionDto = {
      roleId: Number(newItem.roleId),
      permissionId: Number(newItem.permissionId),
      resourceType: newItem.resourceTypeName || 'Global',
      resourceId: Number(newItem.resourceId) || 0,
      allow: Boolean(newItem.allow),
      cascade: Boolean(newItem.cascade)
    };
    await assignPermissionToRole(createDto);
    loadRolePermissions();
    setEditItem(null);
    setCreateMode(false);
  };

  const columns = [
    { key: 'roleName', label: 'Role' },
    { key: 'permissionName', label: 'Permission' },
    { key: 'resourceTypeName', label: 'Resource Type' },
    { key: 'resourceId', label: 'Resource ID' },
    { key: 'allow', label: 'Allow', render: (item: RolePermission) => item.allow ? 'Yes' : 'No' },
  ];

  const resourceTypes = [
    { value: 'Global', label: 'Global' },
    { value: 'Building', label: 'Building' },
    { value: 'Floor', label: 'Floor' },
    { value: 'Point', label: 'Point' },
    { value: 'Scan', label: 'Scan' },
    { value: 'AccessPoint', label: 'AccessPoint' }
  ];

  return (
    <>
      <div style={{ padding: '1rem' }}>
        <BackButton label="Back" onClick={() => navigate('/permissions')} />
      </div>
      <GenericTable
        title="Role Permissions"
        data={rolePermissions}
        columns={columns}
        onEdit={() => {}}
        onDelete={handleDelete}
        onCreate={handleCreate}
        isLoading={isLoading}
        getItemId={(item) => `${item.roleId}-${item.permissionId}-${item.resourceTypeId}-${item.resourceId}`}
        canEdit={() => false}
        canDelete={() => true}
        canCreate={true}
      />
      {deleteItem && (
        <DeleteModal
          item={deleteItem}
          onClose={() => setDeleteItem(null)}
          onConfirm={handleConfirmDelete}
          itemName="Role Permission"
          getDisplayText={(item) => `${item.roleName} - ${item.permissionName} (${item.resourceTypeName})`}
        />
      )}
      {(createMode) && editItem && (
        <EditModal
          item={editItem}
          onClose={() => { setEditItem(null); setCreateMode(false); }}
          onSave={handleSaveCreate}
          title="Assign Permission to Role"
          fields={[
            { 
              key: 'roleId', 
              label: 'Role', 
              type: 'select',
              options: roles.map(r => ({ value: r.id, label: r.name }))
            },
            { 
              key: 'permissionId', 
              label: 'Permission', 
              type: 'select',
              options: permissions.map(p => ({ value: p.id, label: p.name }))
            },
            { 
              key: 'resourceTypeName', 
              label: 'Resource Type', 
              type: 'select',
              options: resourceTypes
            },
            { key: 'resourceId', label: 'Resource ID (0 for Global)', type: 'number' },
            { key: 'allow', label: 'Allow', type: 'checkbox' },
            { key: 'cascade', label: 'Cascade', type: 'checkbox' }
          ]}
        />
      )}
    </>
  );
};

export default RolePermissionsPage;
