import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GenericTable } from '../components/GenericTable';
import { EditModal } from '../components/modals/EditModal';
import { DeleteModal } from '../components/modals/DeleteModal';
import { BackButton } from '../components/BackButton';
import { getPoints, deletePoint, updatePoint, createPoint } from '../services/pointService';
import { getFloors } from '../services/floorService';
import { useAuth } from '../contexts/AuthContext';
import type { Point, CreatePointDto, Floor } from '../types/api';

const PointsPage = () => {
  const { floorId } = useParams();
  const parsedFloorId = Number(floorId);
  const navigate = useNavigate();

  const [points, setPoints] = useState<Point[]>([]);
  const [floors, setFloors] = useState<Floor[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [editItem, setEditItem] = useState<Point | null>(null);
  const [deleteItem, setDeleteItem] = useState<Point | null>(null);
  const [createMode, setCreateMode] = useState(false);

  const { permissionScopes, hasPermission, hasPermissionForId } = useAuth();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [pointsResponse, floorsResponse] = await Promise.all([
        getPoints(),
        getFloors()
      ]);
      const allPoints = pointsResponse.data;
      const filtered = isNaN(parsedFloorId)
        ? allPoints
        : allPoints.filter(p => p.floorId === parsedFloorId);
      setPoints(filtered);
      setFloors(floorsResponse.data);
    } catch (error) {
      console.error('Failed to load points:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = (point: Point) => {
    setEditItem(point);
  };

  const handleSaveEdit = async (updatedPoint: Point) => {
    const updateDto = {
      floorId: updatedPoint.floorId ?? undefined,
      latitude: updatedPoint.latitude ?? undefined,
      longitude: updatedPoint.longitude ?? undefined,
      apCount: updatedPoint.apCount ?? undefined,
    };
    await updatePoint(updatedPoint.id, updateDto);
    setPoints(points.map(p => p.id === updatedPoint.id ? updatedPoint : p));
    setEditItem(null);
    setCreateMode(false);
  };

  const handleDelete = (point: Point) => {
    setDeleteItem(point);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    await deletePoint(deleteItem.id);
    setPoints(points.filter(p => p.id !== deleteItem.id));
    setDeleteItem(null);
  };

  const handleCreate = () => {
    setCreateMode(true);
    setEditItem({ id: 0, floorId: parsedFloorId, latitude: 0, longitude: 0, apCount: 0 } as Point);
  };

  const handleSaveCreate = async (newPoint: Point) => {
    const createDto: CreatePointDto = {
      floorId: newPoint.floorId,
      latitude: newPoint.latitude,
      longitude: newPoint.longitude,
      apCount: newPoint.apCount,
    };
    const response = await createPoint(createDto);
    setPoints([...points, response.data]);
    setEditItem(null);
    setCreateMode(false);
  };

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'floorId', label: 'Floor ID' },
    { key: 'latitude', label: 'Latitude' },
    { key: 'longitude', label: 'Longitude' },
    { key: 'apCount', label: 'AP Count' },
  ];

  // Build floor -> building map for cascade checks
  const floorToBuildingMap = useMemo(() => {
    const map: Record<number, number> = {};
    floors.forEach(f => {
      map[f.id] = f.buildingId;
    });
    return map;
  }, [floors]);

  // Permission gating: direct per-point allows or cascade from Floor or Building
  const pointEditScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'edit' && s.resourceType === 'Point')
  ), [permissionScopes]);
  const pointDeleteScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'delete' && s.resourceType === 'Point')
  ), [permissionScopes]);
  const pointCreateScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'create' && s.resourceType === 'Point')
  ), [permissionScopes]);

  const cascadeFloorsForEdit = pointEditScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForDelete = pointDeleteScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForCreate = pointCreateScope?.cascadeFrom?.['Floor'] || [];

  const cascadeBuildingsForEdit = pointEditScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForDelete = pointDeleteScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForCreate = pointCreateScope?.cascadeFrom?.['Building'] || [];

  const canEdit = (point: Point) => {
    if (hasPermissionForId('edit', 'Point', point.id)) return true;
    if (cascadeFloorsForEdit.includes(point.floorId)) return true;
    const buildingId = floorToBuildingMap[point.floorId];
    return buildingId !== undefined && cascadeBuildingsForEdit.includes(buildingId);
  };

  const canDelete = (point: Point) => {
    if (hasPermissionForId('delete', 'Point', point.id)) return true;
    if (cascadeFloorsForDelete.includes(point.floorId)) return true;
    const buildingId = floorToBuildingMap[point.floorId];
    return buildingId !== undefined && cascadeBuildingsForDelete.includes(buildingId);
  };

  const canCreate = () => {
    if (hasPermission('create', 'Point')) return true;
    if (!isNaN(parsedFloorId) && cascadeFloorsForCreate.includes(parsedFloorId)) return true;
    const buildingId = floorToBuildingMap[parsedFloorId];
    return buildingId !== undefined && cascadeBuildingsForCreate.includes(buildingId);
  };

  const childRoutes = [
    {
      label: 'View Scans',
      path: (point: Point) => `/points/${point.id}/scans`,
    },
  ];

  return (
    <>
      <BackButton 
        label="Back to Floors" 
        onClick={() => {
          const floor = floors.find(f => f.id === parsedFloorId);
          if (floor) {
            navigate(`/buildings/${floor.buildingId}/floors`);
          } else {
            navigate('/');
          }
        }} 
      />
      <GenericTable
        title={isNaN(parsedFloorId) ? 'Points' : `Points of Floor #${parsedFloorId}`}
        data={points}
        columns={columns}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        childRoutes={childRoutes}
        isLoading={isLoading}
        getItemId={(point) => point.id}
        canEdit={canEdit}
        canDelete={canDelete}
        canCreate={canCreate()}
      />

      <EditModal
        item={editItem}
        onSave={createMode ? handleSaveCreate : handleSaveEdit}
        onClose={() => {
          setEditItem(null);
          setCreateMode(false);
        }}
        title={createMode ? 'Create Point' : 'Edit Point'}
      />

      <DeleteModal
        item={deleteItem}
        onConfirm={handleConfirmDelete}
        onClose={() => setDeleteItem(null)}
        itemName="point"
        getDisplayText={(point) => `Point #${point.id} (Floor ${point.floorId})`}
      />
    </>
  );
};

export default PointsPage;
