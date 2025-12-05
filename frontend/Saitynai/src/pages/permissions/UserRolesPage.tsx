import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { GenericTable } from '../../components/GenericTable';
import { DeleteModal } from '../../components/modals/DeleteModal';
import { EditModal } from '../../components/modals/EditModal';
import { BackButton } from '../../components/BackButton';
import { getUserRoles, assignRoleToUser, deleteUserRole, getUsers, getRoles } from '../../services/permissionService';
import type { UserRole, AssignRoleDto, User, Role } from '../../types/api';

const UserRolesPage = () => {
  const navigate = useNavigate();
  const [userRoles, setUserRoles] = useState<UserRole[]>([]);
  const [users, setUsers] = useState<User[]>([]);
  const [roles, setRoles] = useState<Role[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [deleteItem, setDeleteItem] = useState<UserRole | null>(null);
  const [createMode, setCreateMode] = useState(false);
  const [editItem, setEditItem] = useState<UserRole | null>(null);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [userRolesRes, usersRes, rolesRes] = await Promise.all([
        getUserRoles(),
        getUsers(),
        getRoles()
      ]);
      setUserRoles(userRolesRes.data);
      setUsers(usersRes.data);
      setRoles(rolesRes.data);
    } catch (error) {
      console.error('Failed to load data:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const loadUserRoles = async () => {
    try {
      const response = await getUserRoles();
      setUserRoles(response.data);
    } catch (error) {
      console.error('Failed to load user roles:', error);
    }
  };

  const handleDelete = (item: UserRole) => {
    setDeleteItem(item);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    try {
      await deleteUserRole(deleteItem.userId, deleteItem.roleId);
      setUserRoles(userRoles.filter(ur => !(ur.userId === deleteItem.userId && ur.roleId === deleteItem.roleId)));
      setDeleteItem(null);
    } catch (error) {
      console.error('Failed to delete user role:', error);
    }
  };

  const handleCreate = () => {
    setCreateMode(true);
    setEditItem({ userId: 0, roleId: 0 } as UserRole);
  };

  const handleSaveCreate = async (newItem: UserRole) => {
    const createDto: AssignRoleDto = {
      userId: Number(newItem.userId),
      roleId: Number(newItem.roleId)
    };
    await assignRoleToUser(createDto);
    loadUserRoles();
    setEditItem(null);
    setCreateMode(false);
  };

  const columns = [
    { key: 'username', label: 'User' },
    { key: 'roleName', label: 'Role' },
  ];

  return (
    <>
      <div style={{ padding: '1rem' }}>
        <BackButton label="Back" onClick={() => navigate('/permissions')} />
      </div>
      <GenericTable
        title="User Roles"
        data={userRoles}
        columns={columns}
        onEdit={() => {}}
        onDelete={handleDelete}
        onCreate={handleCreate}
        isLoading={isLoading}
        getItemId={(item) => `${item.userId}-${item.roleId}`}
        canEdit={() => false}
        canDelete={() => true}
        canCreate={true}
      />
      {deleteItem && (
        <DeleteModal
          item={deleteItem}
          onClose={() => setDeleteItem(null)}
          onConfirm={handleConfirmDelete}
          itemName="User Role"
          getDisplayText={(item) => `${item.username || item.userId} - ${item.roleName || item.roleId}`}
        />
      )}
      {(createMode) && editItem && (
        <EditModal
          item={editItem}
          onClose={() => { setEditItem(null); setCreateMode(false); }}
          onSave={handleSaveCreate}
          title="Assign Role to User"
          fields={[
            { 
              key: 'userId', 
              label: 'User', 
              type: 'select',
              options: users.map(u => ({ value: u.id, label: u.username }))
            },
            { 
              key: 'roleId', 
              label: 'Role', 
              type: 'select',
              options: roles.map(r => ({ value: r.id, label: r.name }))
            }
          ]}
        />
      )}
    </>
  );
};

export default UserRolesPage;
