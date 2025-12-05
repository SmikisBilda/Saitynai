import { useEffect, useMemo, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { GenericTable } from '../components/GenericTable';
import { EditModal } from '../components/modals/EditModal';
import { DeleteModal } from '../components/modals/DeleteModal';
import { BackButton } from '../components/BackButton';
import { getScans, deleteScan, updateScan, createScan } from '../services/scanService';
import { getPoints } from '../services/pointService';
import { getFloors } from '../services/floorService';
import { useAuth } from '../contexts/AuthContext';
import type { Scan, CreateScanDto, Point, Floor } from '../types/api';

const ScansPage = () => {
  const { pointId } = useParams();
  const parsedPointId = Number(pointId);
  const navigate = useNavigate();

  const [scans, setScans] = useState<Scan[]>([]);
  const [points, setPoints] = useState<Point[]>([]);
  const [floors, setFloors] = useState<Floor[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [editItem, setEditItem] = useState<Scan | null>(null);
  const [deleteItem, setDeleteItem] = useState<Scan | null>(null);
  const [createMode, setCreateMode] = useState(false);

  const { permissionScopes, hasPermission, hasPermissionForId } = useAuth();

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    try {
      const [scansResponse, pointsResponse, floorsResponse] = await Promise.all([
        getScans(),
        getPoints(),
        getFloors()
      ]);
      const allScans = scansResponse.data;
      const filtered = isNaN(parsedPointId)
        ? allScans
        : allScans.filter(s => s.pointId === parsedPointId);
      setScans(filtered);
      setPoints(pointsResponse.data);
      setFloors(floorsResponse.data);
    } catch (error) {
      console.error('Failed to load scans:', error);
    } finally {
      setIsLoading(false);
    }
  };

  const handleEdit = (scan: Scan) => {
    setEditItem(scan);
  };

  const handleSaveEdit = async (updatedScan: Scan) => {
    const updateDto = {
      pointId: updatedScan.pointId ?? undefined,
      scannedAt: updatedScan.scannedAt ?? undefined,
      filters: updatedScan.filters ?? undefined,
      apCount: updatedScan.apCount ?? undefined,
    };
    await updateScan(updatedScan.id, updateDto);
    setScans(scans.map(s => s.id === updatedScan.id ? updatedScan : s));
    setEditItem(null);
    setCreateMode(false);
  };

  const handleDelete = (scan: Scan) => {
    setDeleteItem(scan);
  };

  const handleConfirmDelete = async () => {
    if (!deleteItem) return;
    await deleteScan(deleteItem.id);
    setScans(scans.filter(s => s.id !== deleteItem.id));
    setDeleteItem(null);
  };

  const handleCreate = () => {
    setCreateMode(true);
    const now = new Date().toISOString();
    setEditItem({ id: 0, pointId: parsedPointId, scannedAt: now, filters: '', apCount: 0 } as Scan);
  };

  const handleSaveCreate = async (newScan: Scan) => {
    const createDto: CreateScanDto = {
      pointId: newScan.pointId,
      scannedAt: newScan.scannedAt,
      filters: newScan.filters ?? undefined,
      apCount: newScan.apCount,
    };
    const response = await createScan(createDto);
    setScans([...scans, response.data]);
    setEditItem(null);
    setCreateMode(false);
  };

  const columns = [
    { key: 'id', label: 'ID' },
    { key: 'pointId', label: 'Point ID' },
    { key: 'scannedAt', label: 'Scanned At' },
    { key: 'filters', label: 'Filters' },
    { key: 'apCount', label: 'AP Count' },
  ];

  // Build point -> floor -> building maps for cascade checks
  const pointToFloorMap = useMemo(() => {
    const map: Record<number, number> = {};
    points.forEach(p => {
      map[p.id] = p.floorId;
    });
    return map;
  }, [points]);

  const floorToBuildingMap = useMemo(() => {
    const map: Record<number, number> = {};
    floors.forEach(f => {
      map[f.id] = f.buildingId;
    });
    return map;
  }, [floors]);

  // Permission gating: direct or cascade from Point, Floor, or Building
  const scanEditScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'edit' && s.resourceType === 'Scan')
  ), [permissionScopes]);
  const scanDeleteScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'delete' && s.resourceType === 'Scan')
  ), [permissionScopes]);
  const scanCreateScope = useMemo(() => (
    permissionScopes.find(s => s.permissionName === 'create' && s.resourceType === 'Scan')
  ), [permissionScopes]);

  const cascadePointsForEdit = scanEditScope?.cascadeFrom?.['Point'] || [];
  const cascadePointsForDelete = scanDeleteScope?.cascadeFrom?.['Point'] || [];
  const cascadePointsForCreate = scanCreateScope?.cascadeFrom?.['Point'] || [];

  const cascadeFloorsForEdit = scanEditScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForDelete = scanDeleteScope?.cascadeFrom?.['Floor'] || [];
  const cascadeFloorsForCreate = scanCreateScope?.cascadeFrom?.['Floor'] || [];

  const cascadeBuildingsForEdit = scanEditScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForDelete = scanDeleteScope?.cascadeFrom?.['Building'] || [];
  const cascadeBuildingsForCreate = scanCreateScope?.cascadeFrom?.['Building'] || [];

  const canEdit = (scan: Scan) => {
    if (hasPermissionForId('edit', 'Scan', scan.id)) return true;
    if (cascadePointsForEdit.includes(scan.pointId)) return true;
    const floorId = pointToFloorMap[scan.pointId];
    if (floorId !== undefined && cascadeFloorsForEdit.includes(floorId)) return true;
    const buildingId = floorToBuildingMap[floorId];
    return buildingId !== undefined && cascadeBuildingsForEdit.includes(buildingId);
  };

  const canDelete = (scan: Scan) => {
    if (hasPermissionForId('delete', 'Scan', scan.id)) return true;
    if (cascadePointsForDelete.includes(scan.pointId)) return true;
    const floorId = pointToFloorMap[scan.pointId];
    if (floorId !== undefined && cascadeFloorsForDelete.includes(floorId)) return true;
    const buildingId = floorToBuildingMap[floorId];
    return buildingId !== undefined && cascadeBuildingsForDelete.includes(buildingId);
  };

  const canCreate = () => {
    if (hasPermission('create', 'Scan')) return true;
    if (!isNaN(parsedPointId) && cascadePointsForCreate.includes(parsedPointId)) return true;
    const floorId = pointToFloorMap[parsedPointId];
    if (floorId !== undefined && cascadeFloorsForCreate.includes(floorId)) return true;
    const buildingId = floorToBuildingMap[floorId];
    return buildingId !== undefined && cascadeBuildingsForCreate.includes(buildingId);
  };

  const childRoutes = [
    {
      label: 'View Access Points',
      path: (scan: Scan) => `/scans/${scan.id}/access-points`,
    },
  ];

  return (
    <>
      <BackButton 
        label="Back to Points" 
        onClick={() => {
          const point = points.find(p => p.id === parsedPointId);
          if (point) {
            const floor = floors.find(f => f.id === point.floorId);
            if (floor) {
              navigate(`/floors/${floor.id}/points`);
              return;
            }
          }
          navigate('/');
        }} 
      />
      <GenericTable
        title={isNaN(parsedPointId) ? 'Scans' : `Scans of Point #${parsedPointId}`}
        data={scans}
        columns={columns}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onCreate={handleCreate}
        childRoutes={childRoutes}
        isLoading={isLoading}
        getItemId={(scan) => scan.id}
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
        title={createMode ? 'Create Scan' : 'Edit Scan'}
      />

      <DeleteModal
        item={deleteItem}
        onConfirm={handleConfirmDelete}
        onClose={() => setDeleteItem(null)}
        itemName="scan"
        getDisplayText={(scan) => `Scan #${scan.id} (Point ${scan.pointId})`}
      />
    </>
  );
};

export default ScansPage;
