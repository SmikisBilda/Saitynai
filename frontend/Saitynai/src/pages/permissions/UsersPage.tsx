import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { GenericTable } from '../../components/GenericTable';
import { DeleteModal } from '../../components/modals/DeleteModal';
import { BackButton } from '../../components/BackButton';
import { getUsers, deleteUser } from '../../services/permissionService';
import type { User } from '../../types/api';

const UsersPage = () => {
  const navigate = useNavigate();
  const [users, setUsers] = useState<User[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [deleteItem, setDeleteItem] = useState<User | null>(null);

  useEffect(() => {
    loadUsers();
  }, []);

  const loadUsers = async () => {
    try {
      const response = await getUsers();
      setUsers(response.data);
    } catch (error) {
      console.error('Failed to load users:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleDelete = (user: User) => {
    setDeleteItem(user);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    try {
      await deleteUser(deleteItem.id);
      setUsers(users.filter(u => u.id !== deleteItem.id));
      setDeleteItem(null);
    } catch (error) {
      console.error('Failed to delete user:', error);
    }
  };

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'username', label: 'Username' },
    { key: 'email', label: 'Email' },
  ];

  return (
    <>
      <div style={{ padding: '1rem' }}>
        <BackButton label="Back" onClick={() => navigate('/permissions')} />
      </div>
      <GenericTable
        title="Users"
        data={users}
        columns={columns}
        onEdit={() => {}}
        onDelete={handleDelete}
        onCreate={() => {}}
        isLoading={isLoading}
        getItemId={(user) => user.id}
        canEdit={() => false}
        canDelete={() => true}
        canCreate={false}
      />
      {deleteItem && (
        <DeleteModal
          item={deleteItem}
          onClose={() => setDeleteItem(null)}
          onConfirm={handleConfirmDelete}
          itemName="User"
          getDisplayText={(user) => user.username}
        />
      )}
    </>
  );
};

export default UsersPage;
