import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GenericTable } from '../components/GenericTable';
import { EditModal } from '../components/modals/EditModal';
import { DeleteModal } from '../components/modals/DeleteModal';
import { BackButton } from '../components/BackButton';
import { getFloors, deleteFloor, updateFloor, createFloor, uploadFloorPlan } from '../services/floorService';
import { useAuth } from '../contexts/AuthContext';
import type { Floor, CreateFloorDto } from '../types/api';

const FloorsPage = () => {
  const { buildingId } = useParams();
  const parsedBuildingId = Number(buildingId);
  const navigate = useNavigate();

  const [floors, setFloors] = useState<Floor[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [editItem, setEditItem] = useState<Floor | null>(null);
  const [deleteItem, setDeleteItem] = useState<Floor | null>(null);
  const [createMode, setCreateMode] = useState(false);

  const { permissionScopes, hasPermission, hasPermissionForId } = useAuth();

  useEffect(() => {
    loadFloors();
  }, []);

  const loadFloors = async () => {
    try {
      const response = await getFloors();
      const allFloors = response.data;
      const filtered = isNaN(parsedBuildingId)
        ? allFloors
        : allFloors.filter(f => f.buildingId === parsedBuildingId);
      setFloors(filtered);
    } catch (error) {
      console.error('Failed to load floors:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = (floor: Floor) => {
    setEditItem(floor);
  };

  const handleSaveEdit = async (updatedFloor: Floor) => {
    const updateDto = {
      buildingId: updatedFloor.buildingId ?? undefined,
      floorNumber: updatedFloor.floorNumber ?? undefined,
      floorPlanPath: updatedFloor.floorPlanPath ?? undefined,
    };
    await updateFloor(updatedFloor.id, updateDto);
    let newItem = updatedFloor;
    const file = (updatedFloor as any).floorPlanPathFile as File | undefined | null;
    if (file) {
      const resp = await uploadFloorPlan(updatedFloor.id, file);
      newItem = resp.data;
    }
    setFloors(floors.map(f => f.id === updatedFloor.id ? newItem : f));
    setEditItem(null);
    setCreateMode(false);
  };

  const handleDelete = (floor: Floor) => {
    setDeleteItem(floor);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    await deleteFloor(deleteItem.id);
    setFloors(floors.filter(f => f.id !== deleteItem.id));
    setDeleteItem(null);
  };

  const handleCreate = () => {
    setCreateMode(true);
    setEditItem({ id: 0, buildingId: parsedBuildingId, floorNumber: 0, floorPlanPath: '' } as Floor);
  };

  const handleSaveCreate = async (newFloor: Floor) => {
    const createDto: CreateFloorDto = {
      buildingId: newFloor.buildingId,
      floorNumber: newFloor.floorNumber,
      floorPlanPath: newFloor.floorPlanPath ?? undefined,
    };
    const response = await createFloor(createDto);
    let created = response.data as Floor;
    const file = (newFloor as any).floorPlanPathFile as File | undefined | null;
    if (file) {
      const resp = await uploadFloorPlan(created.id, file);
      created = resp.data;
    }
    setFloors([...floors, created]);
    setEditItem(null);
    setCreateMode(false);
  };


  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'buildingId', label: 'Building ID' },
    { key: 'floorNumber', label: 'Floor Number' },
    { key: 'floorPlanPath', label: 'Floor Plan' },
  ];

  const childRoutes = [
    {
      label: 'View Points',
      path: (floor: Floor) => `/floors/${floor.id}/points`,
    },
  ];

  // Permission gating: direct per-floor allows or cascade from Building
  const floorEditScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'edit' && s.resourceType === 'Floor')
  ), [permissionScopes]);
  const floorDeleteScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'delete' && s.resourceType === 'Floor')
  ), [permissionScopes]);
  const floorCreateScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'create' && s.resourceType === 'Floor')
  ), [permissionScopes]);

  const cascadeBuildingsForEdit = floorEditScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForDelete = floorDeleteScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForCreate = floorCreateScope?.cascadeFrom?.['Building'] || [];

  const canEdit = (floor: Floor) => (
    hasPermissionForId('edit', 'Floor', floor.id) || cascadeBuildingsForEdit.includes(floor.buildingId)
  );
  const canDelete = (floor: Floor) => (
    hasPermissionForId('delete', 'Floor', floor.id) || cascadeBuildingsForDelete.includes(floor.buildingId)
  );
  const canCreate = (
    hasPermission('create', 'Floor') || (!isNaN(parsedBuildingId) && cascadeBuildingsForCreate.includes(parsedBuildingId))
  );

  return (
    <>
      <BackButton label="Back to Buildings" onClick={() => navigate('/')} />
      <GenericTable
        title={isNaN(parsedBuildingId) ? 'Floors' : `Floors of Building #${parsedBuildingId}`}
        data={floors}
        columns={columns}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        childRoutes={childRoutes}
        isLoading={isLoading}
        getItemId={(floor) => floor.id}
        canEdit={canEdit}
        canDelete={canDelete}
        canCreate={canCreate}
      />

      <EditModal
        item={editItem}
        onSave={createMode ? handleSaveCreate : handleSaveEdit}
        onClose={() => {
          setEditItem(null);
          setCreateMode(false);
        }}
        title={createMode ? 'Create Floor' : 'Edit Floor'}
      />

      <DeleteModal
        item={deleteItem}
        onConfirm={handleConfirmDelete}
        onClose={() => setDeleteItem(null)}
        itemName="floor"
        getDisplayText={(floor) => `Floor #${floor.id} (Building ${floor.buildingId})`}
      />

      {/* Upload modal no longer used; uploads handled in edit/create */}
    </>
  );
};

export default FloorsPage;