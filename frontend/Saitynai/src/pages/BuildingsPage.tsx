import { useState, useEffect } from 'react';
import { GenericTable } from '../components/GenericTable';
import { EditModal } from '../components/modals/EditModal';
import { DeleteModal } from '../components/modals/DeleteModal';
import { getBuildings, deleteBuilding, updateBuilding, createBuilding } from '../services/buildingService';
import { useAuth } from '../contexts/AuthContext';
import type { Building, CreateBuildingDto } from '../types/api';

const BuildingsPage = () => {
  const [buildings, setBuildings] = useState<Building[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [editItem, setEditItem] = useState<Building | null>(null);
  const [deleteItem, setDeleteItem] = useState<Building | null>(null);
  const [createMode, setCreateMode] = useState(false);
  const { hasPermission, hasPermissionForId } = useAuth();

  useEffect(() => {
    loadBuildings();
  }, []);

  const loadBuildings = async () => {
    try {
      const response = await getBuildings();
      setBuildings(response.data);
    } catch (error) {
      console.error('Failed to load buildings:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = (building: Building) => {
    setEditItem(building);
  };

  const handleSaveEdit = async (updatedBuilding: Building) => {
    const updateDto = {
      name: updatedBuilding.name ?? undefined,
      address: updatedBuilding.address ?? undefined
    };
    await updateBuilding(updatedBuilding.id, updateDto);
    setBuildings(buildings.map(b => b.id === updatedBuilding.id ? updatedBuilding : b));
    setEditItem(null);
    setCreateMode(false);
  };

  const handleDelete = (building: Building) => {
    setDeleteItem(building);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    await deleteBuilding(deleteItem.id);
    setBuildings(buildings.filter(b => b.id !== deleteItem.id));
    setDeleteItem(null);
  };

  const handleCreate = () => {
    setCreateMode(true);
    setEditItem({ id: 0, name: '', address: '' } as Building);
  };

  const handleSaveCreate = async (newBuilding: Building) => {
    const createDto: CreateBuildingDto = {
      name: newBuilding.name ?? undefined,
      address: newBuilding.address ?? undefined
    };
    const response = await createBuilding(createDto);
    setBuildings([...buildings, response.data]);
    setEditItem(null);
    setCreateMode(false);
  };

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'name', label: 'Name' },
    { key: 'address', label: 'Address' },
  ];

  const childRoutes = [
    {
      label: 'View Floors',
      path: (building: Building) => `/buildings/${building.id}/floors`,
    },
  ];

  return (
    <>
      <GenericTable
        title="Buildings"
        data={buildings}
        columns={columns}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        childRoutes={childRoutes}
        isLoading={isLoading}
        getItemId={(building) => building.id}
        canEdit={(building) => hasPermissionForId('edit', 'Building', building.id)}
        canDelete={(building) => hasPermissionForId('delete', 'Building', building.id)}
        canCreate={hasPermission('create', 'Building')}
      />

      <EditModal
        item={editItem}
        onSave={createMode ? handleSaveCreate : handleSaveEdit}
        onClose={() => {
          setEditItem(null);
          setCreateMode(false);
        }}
        title={createMode ? 'Create Building' : 'Edit Building'}
      />

      <DeleteModal
        item={deleteItem}
        onConfirm={handleConfirmDelete}
        onClose={() => setDeleteItem(null)}
        itemName="building"
        getDisplayText={(building) => building.name || `Building #${building.id}`}
      />
    </>
  );
};

export default BuildingsPage;
